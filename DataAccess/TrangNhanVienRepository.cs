using PBL3.DataBase;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace PBL3.DataAccess
{
    internal sealed class TrangNhanVienRepository
    {
        public DataTable GetAllNhanVien()
        {
            const string sql = @"
SELECT nv.MaNV, nv.HoTen, nv.NgaySinh, nv.SDT, nv.Email, nv.DiaChi, nv.MatKhau, nv.TrangThai, nv.MaCV, cv.TenCV
FROM dbo.NHAN_VIEN nv
LEFT JOIN dbo.CHUC_VU cv ON cv.MaCV = nv.MaCV
ORDER BY nv.MaNV";

            using SqlConnection conn = DbHelper.GetConnection();
            using SqlCommand cmd = new SqlCommand(sql, conn);
            using SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        public DataTable GetChucVu()
        {
            using SqlConnection conn = DbHelper.GetConnection();
            using SqlCommand cmd = new SqlCommand("SELECT MaCV, TenCV FROM dbo.CHUC_VU ORDER BY MaCV", conn);
            using SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        public DataTable GetNhanVienByMaNv(string maNv)
        {
            const string sql = @"
SELECT nv.MaNV, nv.HoTen, nv.NgaySinh, nv.SDT, nv.Email, nv.DiaChi, nv.MatKhau, nv.TrangThai, nv.MaCV
FROM dbo.NHAN_VIEN nv
WHERE nv.MaNV = @MaNV";

            using SqlConnection conn = DbHelper.GetConnection();
            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@MaNV", SqlDbType.VarChar, 20).Value = maNv;
            using SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        public bool IsPhoneExists(bool isInsert, string phone, string? excludeMaNv)
        {
            string query = isInsert
                ? "SELECT COUNT(1) FROM dbo.NHAN_VIEN WHERE SDT = @SDT"
                : "SELECT COUNT(1) FROM dbo.NHAN_VIEN WHERE SDT = @SDT AND MaNV != @ID";

            using SqlConnection conn = DbHelper.GetConnection();
            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.Add("@SDT", SqlDbType.VarChar, 20).Value = phone;
            if (!isInsert)
            {
                cmd.Parameters.Add("@ID", SqlDbType.VarChar, 20).Value = excludeMaNv ?? string.Empty;
            }

            conn.Open();
            return Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture) > 0;
        }

        public void AddNhanVien(string hoTen, DateTime ngaySinh, string sdt, string email, string diaChi, string matKhau, string trangThai, string maCv)
        {
            // Hash password using BCrypt
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(matKhau);

            using SqlConnection conn = DbHelper.GetConnection();
            conn.Open();

            int nextId = 1;
            using (SqlCommand cmdGet = new SqlCommand("SELECT MaNV FROM dbo.NHAN_VIEN ORDER BY TRY_CAST(CASE WHEN CONVERT(VARCHAR(20), MaNV) LIKE 'NV%' THEN SUBSTRING(CONVERT(VARCHAR(20), MaNV), 3, LEN(CONVERT(VARCHAR(20), MaNV)) - 2) ELSE CONVERT(VARCHAR(20), MaNV) END AS INT)", conn))
            using (SqlDataReader reader = cmdGet.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (reader.IsDBNull(0))
                        continue;

                    string raw = Convert.ToString(reader.GetValue(0)) ?? string.Empty;
                    string numeric = raw.StartsWith("NV", StringComparison.OrdinalIgnoreCase) ? raw[2..] : raw;
                    if (!int.TryParse(numeric, out int v))
                        continue;

                    if (v == nextId)
                    {
                        nextId++;
                    }
                    else if (v > nextId)
                    {
                        break;
                    }
                }
            }

            bool hasIdentity;
            using (SqlCommand cmdCheck = new SqlCommand("SELECT CASE WHEN COLUMNPROPERTY(OBJECT_ID('dbo.NHAN_VIEN'),'MaNV','IsIdentity') = 1 THEN 1 ELSE 0 END", conn))
            {
                hasIdentity = Convert.ToInt32(cmdCheck.ExecuteScalar() ?? 0, CultureInfo.InvariantCulture) == 1;
            }

            using SqlTransaction tran = conn.BeginTransaction();
            try
            {
                string insertSql = hasIdentity
                    ? "SET IDENTITY_INSERT dbo.NHAN_VIEN ON; INSERT INTO dbo.NHAN_VIEN (MaNV, HoTen, NgaySinh, SDT, Email, DiaChi, MatKhau, TrangThai, MaCV) VALUES (@MaNV, @HoTen, @NgaySinh, @SDT, @Email, @DiaChi, @MatKhau, @TrangThai, @MaCV); SET IDENTITY_INSERT dbo.NHAN_VIEN OFF;"
                    : "INSERT INTO dbo.NHAN_VIEN (MaNV, HoTen, NgaySinh, SDT, Email, DiaChi, MatKhau, TrangThai, MaCV) VALUES (@MaNV, @HoTen, @NgaySinh, @SDT, @Email, @DiaChi, @MatKhau, @TrangThai, @MaCV)";

                using SqlCommand cmd = new SqlCommand(insertSql, conn, tran);
                cmd.Parameters.Add("@MaNV", SqlDbType.Int).Value = nextId;
                cmd.Parameters.Add("@HoTen", SqlDbType.NVarChar, 100).Value = hoTen;
                cmd.Parameters.Add("@NgaySinh", SqlDbType.Date).Value = ngaySinh.Date;
                cmd.Parameters.Add("@SDT", SqlDbType.VarChar, 20).Value = sdt;
                cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 100).Value = email;
                cmd.Parameters.Add("@DiaChi", SqlDbType.NVarChar, 200).Value = diaChi;
                cmd.Parameters.Add("@MatKhau", SqlDbType.NVarChar, 100).Value = hashedPassword;
                cmd.Parameters.Add("@TrangThai", SqlDbType.NVarChar, 30).Value = trangThai;
                cmd.Parameters.Add("@MaCV", SqlDbType.VarChar, 20).Value = maCv;
                cmd.ExecuteNonQuery();

                tran.Commit();
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }

        public int UpdateNhanVien(string maNv, string hoTen, DateTime ngaySinh, string sdt, string email, string diaChi, string? matKhau)
        {
            const string sql = @"
UPDATE dbo.NHAN_VIEN
SET HoTen = @HoTen,
    NgaySinh = @NgaySinh,
    SDT = @SDT,
    Email = @Email,
    DiaChi = @DiaChi,
    MatKhau = CASE WHEN @MatKhau = '' THEN MatKhau ELSE @MatKhau END
WHERE MaNV = @MaNV";

            using SqlConnection conn = DbHelper.GetConnection();
            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@MaNV", SqlDbType.VarChar, 20).Value = maNv;
            cmd.Parameters.Add("@HoTen", SqlDbType.NVarChar, 100).Value = hoTen;
            cmd.Parameters.Add("@NgaySinh", SqlDbType.Date).Value = ngaySinh.Date;
            cmd.Parameters.Add("@SDT", SqlDbType.VarChar, 20).Value = sdt;
            cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 100).Value = email;
            cmd.Parameters.Add("@DiaChi", SqlDbType.NVarChar, 200).Value = diaChi;
            
            // Hash password using BCrypt if provided
            string hashedPassword = string.IsNullOrWhiteSpace(matKhau) ? string.Empty : BCrypt.Net.BCrypt.HashPassword(matKhau);
            cmd.Parameters.Add("@MatKhau", SqlDbType.NVarChar, 100).Value = hashedPassword;
            
            conn.Open();
            return cmd.ExecuteNonQuery();
        }

        public int UpdateNhanVienAdmin(string maNv, string hoTen, DateTime ngaySinh, string sdt, string email, string diaChi, string trangThai, string maCv, string? matKhau)
        {
            const string sql = @"
UPDATE dbo.NHAN_VIEN
SET HoTen = @HoTen,
    NgaySinh = @NgaySinh,
    SDT = @SDT,
    Email = @Email,
    DiaChi = @DiaChi,
    TrangThai = @TrangThai,
    MaCV = @MaCV,
    MatKhau = CASE WHEN @MatKhau = '' THEN MatKhau ELSE @MatKhau END
WHERE MaNV = @MaNV";

            using SqlConnection conn = DbHelper.GetConnection();
            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@MaNV", SqlDbType.VarChar, 20).Value = maNv;
            cmd.Parameters.Add("@HoTen", SqlDbType.NVarChar, 100).Value = hoTen;
            cmd.Parameters.Add("@NgaySinh", SqlDbType.Date).Value = ngaySinh.Date;
            cmd.Parameters.Add("@SDT", SqlDbType.VarChar, 20).Value = sdt;
            cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 100).Value = email;
            cmd.Parameters.Add("@DiaChi", SqlDbType.NVarChar, 200).Value = diaChi;
            cmd.Parameters.Add("@TrangThai", SqlDbType.NVarChar, 30).Value = trangThai;
            cmd.Parameters.Add("@MaCV", SqlDbType.VarChar, 20).Value = maCv;
            cmd.Parameters.Add("@MatKhau", SqlDbType.NVarChar, 100).Value = matKhau ?? string.Empty;
            conn.Open();
            return cmd.ExecuteNonQuery();
        }

        public int DeleteNhanVien(string maNv)
        {
            using SqlConnection conn = DbHelper.GetConnection();
            using SqlCommand cmd = new SqlCommand("DELETE FROM dbo.NHAN_VIEN WHERE MaNV = @MaNV", conn);
            cmd.Parameters.Add("@MaNV", SqlDbType.VarChar, 20).Value = maNv;
            conn.Open();
            return cmd.ExecuteNonQuery();
        }

        public string GenerateNextDisplayCode()
        {
            const string sql = @"SELECT MaNV FROM dbo.NHAN_VIEN
ORDER BY TRY_CAST(
    CASE
        WHEN CONVERT(VARCHAR(20), MaNV) LIKE 'NV%' THEN SUBSTRING(CONVERT(VARCHAR(20), MaNV), 3, LEN(CONVERT(VARCHAR(20), MaNV)) - 2)
        ELSE CONVERT(VARCHAR(20), MaNV)
    END AS INT)";

            using SqlConnection conn = DbHelper.GetConnection();
            conn.Open();
            using SqlCommand cmd = new SqlCommand(sql, conn);
            using SqlDataReader reader = cmd.ExecuteReader();
            int nextId = 1;
            while (reader.Read())
            {
                if (reader.IsDBNull(0))
                    continue;

                string raw = Convert.ToString(reader.GetValue(0)) ?? string.Empty;
                string numeric = raw.StartsWith("NV", StringComparison.OrdinalIgnoreCase) ? raw[2..] : raw;
                if (!int.TryParse(numeric, out int v))
                    continue;

                if (v == nextId)
                {
                    nextId++;
                }
                else if (v > nextId)
                {
                    break;
                }
            }

            return $"NV{nextId}";
        }

        public void InsertYeuCau(int maNv, string loaiYeuCau, string noiDung, DateTime? tuNgay, DateTime? denNgay)
        {
            const string sql = @"
INSERT INTO dbo.YEU_CAU (MaNV, LoaiYeuCau, NoiDung, TuNgay, DenNgay, TrangThai, NgayGui, PhanHoiAdmin)
VALUES (@MaNV, @LoaiYeuCau, @NoiDung, @TuNgay, @DenNgay, 0, GETDATE(), N'')";

            using SqlConnection conn = DbHelper.GetConnection();
            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@MaNV", SqlDbType.Int).Value = maNv;
            cmd.Parameters.Add("@LoaiYeuCau", SqlDbType.NVarChar, 50).Value = loaiYeuCau;
            cmd.Parameters.Add("@NoiDung", SqlDbType.NVarChar).Value = noiDung;
            cmd.Parameters.Add("@TuNgay", SqlDbType.Date).Value = (object?)tuNgay?.Date ?? DBNull.Value;
            cmd.Parameters.Add("@DenNgay", SqlDbType.Date).Value = (object?)denNgay?.Date ?? DBNull.Value;
            conn.Open();
            cmd.ExecuteNonQuery();
        }

        public DataTable GetYeuCauHistory(int maNv)
        {
            const string sql = @"
SELECT NgayGui, LoaiYeuCau, TuNgay, DenNgay, TrangThai, ISNULL(PhanHoiAdmin, N'') AS PhanHoiAdmin
FROM dbo.YEU_CAU
WHERE MaNV = @MaNV
ORDER BY NgayGui DESC";

            using SqlConnection conn = DbHelper.GetConnection();
            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@MaNV", SqlDbType.Int).Value = maNv;
            using SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        public DataTable GetLichTruc(string maNv)
        {
            const string sql = @"
SELECT pc.MaCa, ct.TenCa, ct.GioBatDau, ct.GioKetThuc, ct.HeSoLuong, pc.NgayLam
FROM dbo.PHAN_CONG_CA pc
INNER JOIN dbo.CA_TRUC ct ON ct.MaCa = pc.MaCa
WHERE pc.MaNV = @MaNV
ORDER BY pc.NgayLam DESC, pc.MaCa";

            using SqlConnection conn = DbHelper.GetConnection();
            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@MaNV", SqlDbType.VarChar, 20).Value = maNv;
            using SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        public DataTable GetCaTruc()
        {
            const string sql = @"
SELECT MaCa, TenCa, GioBatDau, GioKetThuc, HeSoLuong, SoNguoiToiThieu
FROM dbo.CA_TRUC
ORDER BY MaCa";

            using SqlConnection conn = DbHelper.GetConnection();
            using SqlCommand cmd = new SqlCommand(sql, conn);
            using SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        public int AddPhanCongCa(string maNv, string maCa, DateTime ngayLam)
        {
            using SqlConnection conn = DbHelper.GetConnection();
            conn.Open();

            TimeSpan? newStart = null;
            TimeSpan? newEnd = null;
            using (SqlCommand cmdShift = new SqlCommand("SELECT GioBatDau, GioKetThuc FROM dbo.CA_TRUC WHERE MaCa = @MaCa", conn))
            {
                cmdShift.Parameters.Add("@MaCa", SqlDbType.VarChar, 20).Value = maCa;
                using SqlDataReader rd = cmdShift.ExecuteReader();
                if (rd.Read())
                {
                    if (rd[0] != DBNull.Value)
                    {
                        newStart = (TimeSpan)rd[0];
                    }
                    if (rd[1] != DBNull.Value)
                    {
                        newEnd = (TimeSpan)rd[1];
                    }
                }
            }

            List<(string MaCa, TimeSpan? Start, TimeSpan? End)> existing = new();
            using (SqlCommand cmdExisting = new SqlCommand(@"
 SELECT ct.MaCa, ct.GioBatDau, ct.GioKetThuc
FROM dbo.PHAN_CONG_CA pc
INNER JOIN dbo.CA_TRUC ct ON ct.MaCa = pc.MaCa
WHERE pc.MaNV = @MaNV AND pc.NgayLam = @NgayLam", conn))
            {
                cmdExisting.Parameters.Add("@MaNV", SqlDbType.VarChar, 20).Value = maNv;
                cmdExisting.Parameters.Add("@NgayLam", SqlDbType.Date).Value = ngayLam.Date;
                using SqlDataReader rd = cmdExisting.ExecuteReader();
                while (rd.Read())
                {
                    TimeSpan? start = rd[1] == DBNull.Value ? null : (TimeSpan)rd[1];
                    TimeSpan? end = rd[2] == DBNull.Value ? null : (TimeSpan)rd[2];
                    existing.Add((Convert.ToString(rd[0]) ?? string.Empty, start, end));
                }
            }

            foreach (var (existingMaCa, existingStart, existingEnd) in existing)
            {
                if (string.Equals(existingMaCa, maCa, StringComparison.OrdinalIgnoreCase))
                {
                    return 0;
                }

                if (newStart.HasValue && newEnd.HasValue && existingStart.HasValue && existingEnd.HasValue)
                {
                    bool overlaps = newStart.Value < existingEnd.Value && existingStart.Value < newEnd.Value;
                    if (overlaps)
                    {
                        return -1;
                    }
                }
            }

            using SqlCommand cmdInsert = new SqlCommand(@"
INSERT INTO dbo.PHAN_CONG_CA (MaNV, MaCa, NgayLam)
VALUES (@MaNV, @MaCa, @NgayLam);
SELECT 1;", conn);
            cmdInsert.Parameters.Add("@MaNV", SqlDbType.VarChar, 20).Value = maNv;
            cmdInsert.Parameters.Add("@MaCa", SqlDbType.VarChar, 20).Value = maCa;
            cmdInsert.Parameters.Add("@NgayLam", SqlDbType.Date).Value = ngayLam.Date;
            object? result = cmdInsert.ExecuteScalar();
            return result is null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
        }

        public int GetApprovedLeaveCountForMonth(int maNv, DateTime month)
        {
            const string sql = @"
SELECT COALESCE(SUM(DATEDIFF(DAY, COALESCE(TuNgay, NgayGui), COALESCE(DenNgay, NgayGui)) + 1), 0)
FROM dbo.YEU_CAU
WHERE MaNV = @MaNV
  AND TrangThai = 1
  AND YEAR(COALESCE(TuNgay, NgayGui)) = @Year
  AND MONTH(COALESCE(TuNgay, NgayGui)) = @Month
  AND LoaiYeuCau = N'Nghỉ phép'";

            using SqlConnection conn = DbHelper.GetConnection();
            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@MaNV", SqlDbType.Int).Value = maNv;
            cmd.Parameters.Add("@Year", SqlDbType.Int).Value = month.Year;
            cmd.Parameters.Add("@Month", SqlDbType.Int).Value = month.Month;
            conn.Open();
            return Convert.ToInt32(cmd.ExecuteScalar() ?? 0, CultureInfo.InvariantCulture);
        }

        public DataTable GetStaffingCounts(DateTime tuNgay, DateTime denNgay)
        {
            const string sql = @"
SELECT NgayLam,
       SUM(CASE WHEN MaCa = 1 THEN 1 ELSE 0 END) AS SoSang,
       SUM(CASE WHEN MaCa = 2 THEN 1 ELSE 0 END) AS SoChieu,
       SUM(CASE WHEN MaCa = 3 THEN 1 ELSE 0 END) AS SoToi,
       SUM(CASE WHEN MaCa = 4 THEN 1 ELSE 0 END) AS SoFull
FROM dbo.PHAN_CONG_CA
WHERE NgayLam >= @TuNgay AND NgayLam <= @DenNgay
GROUP BY NgayLam";

            using SqlConnection conn = DbHelper.GetConnection();
            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@TuNgay", SqlDbType.Date).Value = tuNgay.Date;
            cmd.Parameters.Add("@DenNgay", SqlDbType.Date).Value = denNgay.Date;
            using SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        public DataTable GetPhanCongCaRange(string maNv, DateTime tuNgay, DateTime denNgay)
        {
            const string sql = @"
SELECT pc.MaCa, ct.TenCa, pc.NgayLam
FROM dbo.PHAN_CONG_CA pc
INNER JOIN dbo.CA_TRUC ct ON ct.MaCa = pc.MaCa
WHERE pc.MaNV = @MaNV
  AND pc.NgayLam >= @TuNgay
  AND pc.NgayLam <= @DenNgay
ORDER BY pc.NgayLam";

            using SqlConnection conn = DbHelper.GetConnection();
            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@MaNV", SqlDbType.VarChar, 20).Value = maNv;
            cmd.Parameters.Add("@TuNgay", SqlDbType.Date).Value = tuNgay.Date;
            cmd.Parameters.Add("@DenNgay", SqlDbType.Date).Value = denNgay.Date;
            using SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        public int DeletePhanCongCaByLeaveRange(string maNv, DateTime tuNgay, DateTime denNgay)
        {
            const string sql = @"
DELETE FROM dbo.PHAN_CONG_CA
WHERE MaNV = @MaNV
  AND NgayLam >= @TuNgay
  AND NgayLam <= @DenNgay";

            using SqlConnection conn = DbHelper.GetConnection();
            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@MaNV", SqlDbType.VarChar, 20).Value = maNv;
            cmd.Parameters.Add("@TuNgay", SqlDbType.Date).Value = tuNgay.Date;
            cmd.Parameters.Add("@DenNgay", SqlDbType.Date).Value = denNgay.Date;
            conn.Open();
            return cmd.ExecuteNonQuery();
        }

        public string GetCurrentPassword(string maNv)
        {
            using SqlConnection conn = DbHelper.GetConnection();
            using SqlCommand cmd = new SqlCommand("SELECT MatKhau FROM dbo.NHAN_VIEN WHERE MaNV = @MaNV", conn);
            cmd.Parameters.Add("@MaNV", SqlDbType.VarChar, 20).Value = maNv;
            conn.Open();
            return Convert.ToString(cmd.ExecuteScalar()) ?? string.Empty;
        }

        public int ResetPassword(string maNv, string newPassword)
        {
            // Hash password using BCrypt
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);

            using SqlConnection conn = DbHelper.GetConnection();
            using SqlCommand cmd = new SqlCommand("UPDATE dbo.NHAN_VIEN SET MatKhau = @MatKhau WHERE MaNV = @MaNV", conn);
            cmd.Parameters.Add("@MatKhau", SqlDbType.NVarChar, 255).Value = hashedPassword;
            cmd.Parameters.Add("@MaNV", SqlDbType.VarChar, 20).Value = maNv;
            conn.Open();
            return cmd.ExecuteNonQuery();
        }

        public decimal GetLuongCoBanNhanVien(string maNv)
        {
            const string sql = @"
SELECT TOP 1 cv.LuongCoBan
FROM dbo.NHAN_VIEN nv
INNER JOIN dbo.CHUC_VU cv ON cv.MaCV = nv.MaCV
WHERE nv.MaNV = @MaNV";

            using SqlConnection conn = DbHelper.GetConnection();
            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@MaNV", SqlDbType.VarChar, 20).Value = maNv;
            conn.Open();
            object? result = cmd.ExecuteScalar();
            return result is null || result == DBNull.Value ? 0m : Convert.ToDecimal(result, CultureInfo.InvariantCulture);
        }

        public int GetPendingYeuCauCount()
        {
            using SqlConnection conn = DbHelper.GetConnection();
            using SqlCommand cmd = new SqlCommand("SELECT COUNT(1) FROM dbo.YEU_CAU WHERE TrangThai = 0", conn);
            conn.Open();
            return Convert.ToInt32(cmd.ExecuteScalar() ?? 0, CultureInfo.InvariantCulture);
        }
    }
}
