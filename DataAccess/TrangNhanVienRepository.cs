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
                cmd.Parameters.Add("@MatKhau", SqlDbType.NVarChar, 100).Value = matKhau;
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
            cmd.Parameters.Add("@MatKhau", SqlDbType.NVarChar, 100).Value = matKhau ?? string.Empty;
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
            using SqlConnection conn = DbHelper.GetConnection();
            using SqlCommand cmd = new SqlCommand("UPDATE dbo.NHAN_VIEN SET MatKhau = @MatKhau WHERE MaNV = @MaNV", conn);
            cmd.Parameters.Add("@MatKhau", SqlDbType.NVarChar, 100).Value = newPassword;
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
