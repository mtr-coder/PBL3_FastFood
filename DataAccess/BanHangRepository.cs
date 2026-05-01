using PBL3.DataBase;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace PBL3.DataAccess
{
    internal sealed class BanHangRepository
    {
        public DataTable GetDanhMucMonAnData()
        {
            const string sql = @"
SELECT mb.MaMon, mb.TenMon, mb.MaLoai, lm.TenLoai, mb.MaDVT, dvt.TenDVT
FROM dbo.MON_BAN mb
LEFT JOIN dbo.LOAI_MON lm ON lm.MaLoai = mb.MaLoai
LEFT JOIN dbo.DON_VI_TINH dvt ON dvt.MaDVT = mb.MaDVT
WHERE ISNULL(mb.TrangThai, N'Đang bán') <> N'Ngừng bán'
ORDER BY mb.MaMon";

            using SqlConnection conn = DbHelper.GetConnection();
            conn.Open();
            using SqlDataAdapter da = new SqlDataAdapter(sql, conn);
            DataTable dt = new DataTable();
            da.Fill(dt);

            if (!dt.Columns.Contains("DonGia"))
            {
                dt.Columns.Add("DonGia", typeof(decimal));
            }

            Dictionary<string, decimal> giaMap = LoadGiaMapByMon(conn);
            foreach (DataRow row in dt.Rows)
            {
                string key = NormalizeMonKey(Convert.ToString(row["MaMon"]));
                row["DonGia"] = giaMap.TryGetValue(key, out decimal gia) ? gia : 0m;
            }

            return dt;
        }

        public DataTable GetSizeOptionsForMon(string maMonRaw, string maMonNumeric, int fallbackMaDvt, string fallbackTenDvt, decimal fallbackGiaMon)
        {
            using SqlConnection conn = DbHelper.GetConnection();
            conn.Open();

            string giaColumn = ResolveMonDvpvGiaColumn(conn);
            string sql = $@"SELECT mdv.MaDVPV, dv.TenDVPV, ISNULL(mdv.{giaColumn},0) AS DonGia,
CAST(dv.TenDVPV + N' - ' + FORMAT(ISNULL(mdv.{giaColumn},0), 'N0') + N' đ' AS NVARCHAR(120)) AS HienThi
FROM dbo.MON_DON_VI_PHUC_VU mdv
INNER JOIN dbo.DON_VI_PHUC_VU dv ON dv.MaDVPV = mdv.MaDVPV
WHERE CONVERT(VARCHAR(20), mdv.MaMon) = @MaMonRaw
   OR (CASE
           WHEN CONVERT(VARCHAR(20), mdv.MaMon) LIKE 'MA%'
               THEN SUBSTRING(CONVERT(VARCHAR(20), mdv.MaMon), 3, LEN(CONVERT(VARCHAR(20), mdv.MaMon)) - 2)
           ELSE CONVERT(VARCHAR(20), mdv.MaMon)
       END) = @MaMonNumeric";

            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@MaMonRaw", SqlDbType.VarChar, 20).Value = maMonRaw;
            cmd.Parameters.Add("@MaMonNumeric", SqlDbType.VarChar, 20).Value = maMonNumeric;
            using SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);

            if (dt.Rows.Count == 0)
            {
                string giaFallbackExpression = ResolveMonBanGiaExpression(conn, "mb");
                string sqlFallback = $@"SELECT ISNULL(mb.MaDVT, 0) AS MaDVPV,
ISNULL(dvt.TenDVT, N'Mặc định') AS TenDVPV,
{giaFallbackExpression} AS DonGia,
CAST(ISNULL(dvt.TenDVT, N'Mặc định') + N' - ' + FORMAT({giaFallbackExpression}, 'N0') + N' đ' AS NVARCHAR(120)) AS HienThi
FROM dbo.MON_BAN mb
LEFT JOIN dbo.DON_VI_TINH dvt ON dvt.MaDVT = mb.MaDVT
WHERE CONVERT(VARCHAR(20), mb.MaMon) = @MaMonRaw
   OR (CASE
           WHEN CONVERT(VARCHAR(20), mb.MaMon) LIKE 'MA%'
               THEN SUBSTRING(CONVERT(VARCHAR(20), mb.MaMon), 3, LEN(CONVERT(VARCHAR(20), mb.MaMon)) - 2)
           ELSE CONVERT(VARCHAR(20), mb.MaMon)
       END) = @MaMonNumeric";

                using SqlCommand fallbackCmd = new SqlCommand(sqlFallback, conn);
                fallbackCmd.Parameters.Add("@MaMonRaw", SqlDbType.VarChar, 20).Value = maMonRaw;
                fallbackCmd.Parameters.Add("@MaMonNumeric", SqlDbType.VarChar, 20).Value = maMonNumeric;
                using SqlDataAdapter fallbackDa = new SqlDataAdapter(fallbackCmd);
                fallbackDa.Fill(dt);

                if (dt.Rows.Count == 0)
                {
                    DataRow r = dt.NewRow();
                    r["MaDVPV"] = fallbackMaDvt;
                    r["TenDVPV"] = fallbackTenDvt;
                    r["DonGia"] = fallbackGiaMon;
                    r["HienThi"] = $"{fallbackTenDvt} - {fallbackGiaMon:N0} đ";
                    dt.Rows.Add(r);
                }
            }

            return dt;
        }

        public DataTable GetKhachHangOptions()
        {
            using SqlConnection conn = DbHelper.GetConnection();
            const string sql = @"
SELECT kh.MaKH, kh.TenKH, kh.SDT,
       ISNULL(kh.DiemTichLuy,0) AS DiemTichLuy,
       ISNULL(kh.DiemTichLuyTronDoi,0) AS DiemTichLuyTronDoi,
       ISNULL(hv.MaHang, 1) AS MaHang,
       ISNULL(hv.TenHang, N'Bạc') AS TenHang,
       ISNULL(hv.PhanTramGiam, 0) AS PhanTramGiam
FROM dbo.KHACH_HANG kh
OUTER APPLY (
    SELECT TOP 1 hv2.MaHang, hv2.TenHang, hv2.PhanTramGiam
    FROM dbo.HANG_THANH_VIEN hv2
    WHERE hv2.DiemToiThieu <= ISNULL(kh.DiemTichLuyTronDoi,0)
    ORDER BY hv2.DiemToiThieu DESC, hv2.MaHang DESC
) hv
ORDER BY kh.MaKH";

            using SqlDataAdapter da = new SqlDataAdapter(sql, conn);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        public int CreateCustomerByPhone(string sdt)
        {
            using SqlConnection conn = DbHelper.GetConnection();
            conn.Open();

            using (SqlCommand chk = new SqlCommand("SELECT MaKH FROM dbo.KHACH_HANG WHERE SDT = @SDT", conn))
            {
                chk.Parameters.Add("@SDT", SqlDbType.VarChar, 20).Value = sdt;
                object? exists = chk.ExecuteScalar();
                if (exists is not null && exists != DBNull.Value)
                {
                    return Convert.ToInt32(exists, CultureInfo.InvariantCulture);
                }
            }

            bool hasIdentity;
            using (SqlCommand cmdCheck = new SqlCommand("SELECT CASE WHEN COLUMNPROPERTY(OBJECT_ID('dbo.KHACH_HANG'),'MaKH','IsIdentity') = 1 THEN 1 ELSE 0 END", conn))
            {
                hasIdentity = Convert.ToInt32(cmdCheck.ExecuteScalar() ?? 0, CultureInfo.InvariantCulture) == 1;
            }

            if (hasIdentity)
            {
                using SqlCommand cmd = new SqlCommand("INSERT INTO dbo.KHACH_HANG (TenKH, SDT, DiemTichLuy) OUTPUT INSERTED.MaKH VALUES (@TenKH, @SDT, 0)", conn);
                cmd.Parameters.Add("@TenKH", SqlDbType.NVarChar, 100).Value = "Khách hàng";
                cmd.Parameters.Add("@SDT", SqlDbType.VarChar, 20).Value = sdt;
                object? id = cmd.ExecuteScalar();
                return Convert.ToInt32(id, CultureInfo.InvariantCulture);
            }

            int next;
            using (SqlCommand cmdNext = new SqlCommand(@"SELECT ISNULL(MAX(TRY_CAST(CASE WHEN CONVERT(VARCHAR(20), MaKH) LIKE 'KH%' THEN SUBSTRING(CONVERT(VARCHAR(20), MaKH), 3, LEN(CONVERT(VARCHAR(20), MaKH)) - 2) ELSE CONVERT(VARCHAR(20), MaKH) END AS INT)), 0) + 1 FROM dbo.KHACH_HANG", conn))
            {
                next = Convert.ToInt32(cmdNext.ExecuteScalar(), CultureInfo.InvariantCulture);
            }

            using SqlCommand cmd2 = new SqlCommand("INSERT INTO dbo.KHACH_HANG (MaKH, TenKH, SDT, DiemTichLuy) OUTPUT INSERTED.MaKH VALUES (@MaKH, @TenKH, @SDT, 0)", conn);
            cmd2.Parameters.Add("@MaKH", SqlDbType.Int).Value = next;
            cmd2.Parameters.Add("@TenKH", SqlDbType.NVarChar, 100).Value = "Khách hàng";
            cmd2.Parameters.Add("@SDT", SqlDbType.VarChar, 20).Value = sdt;
            object? id2 = cmd2.ExecuteScalar();
            return Convert.ToInt32(id2, CultureInfo.InvariantCulture);
        }

        public int SaveHoaDonBan(string maNv, int? maKh, decimal tongSauGiam, int diemCong, int diemDung, DataTable hoaDonTable)
        {
            using SqlConnection conn = DbHelper.GetConnection();
            conn.Open();
            using SqlTransaction tran = conn.BeginTransaction();

            try
            {
                int maHdb = InsertHoaDonBan(conn, tran, maNv, tongSauGiam, maKh);

                foreach (DataRow row in hoaDonTable.Rows)
                {
                    string maMon = Convert.ToString(row["MaMon"]) ?? string.Empty;
                    string maDvpv = Convert.ToString(row["MaDVPV"]) ?? string.Empty;
                    int soLuong = Convert.ToInt32(row["SoLuong"], CultureInfo.InvariantCulture);
                    if (!int.TryParse(NormalizeMonKey(maMon), out int maMonInt))
                    {
                        throw new InvalidOperationException($"Mã món không hợp lệ: {maMon}");
                    }

                    if (!int.TryParse(maDvpv, out int maDvpvInt))
                    {
                        throw new InvalidOperationException($"Mã đơn vị phục vụ không hợp lệ: {maDvpv}");
                    }

                    InsertChiTietHoaDon(conn, tran, maHdb, maMonInt, maDvpvInt, soLuong);
                    TruKhoNguyenLieu(conn, tran, maMonInt, maDvpvInt, soLuong);
                }

                if (maKh.HasValue)
                {
                    using SqlCommand cmd = new SqlCommand(@"
UPDATE dbo.KHACH_HANG
SET DiemTichLuy = ISNULL(DiemTichLuy,0) + @Cong - @Tru,
    DiemTichLuyTronDoi = ISNULL(DiemTichLuyTronDoi,0) + @Cong
WHERE MaKH=@MaKH", conn, tran);
                    cmd.Parameters.Add("@Cong", SqlDbType.Int).Value = diemCong;
                    cmd.Parameters.Add("@Tru", SqlDbType.Int).Value = diemDung;
                    cmd.Parameters.Add("@MaKH", SqlDbType.Int).Value = maKh.Value;
                    cmd.ExecuteNonQuery();
                }

                tran.Commit();
                return maHdb;
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }

        public int SaveHoaDonBan(string maNv, int? maKh, decimal tongSauGiam, int diemCong, int diemDung, int diemTronDoiCong, int maHangMoi, DataTable hoaDonTable)
        {
            using SqlConnection conn = DbHelper.GetConnection();
            conn.Open();
            using SqlTransaction tran = conn.BeginTransaction();

            try
            {
                int maHdb = InsertHoaDonBan(conn, tran, maNv, tongSauGiam, maKh);

                foreach (DataRow row in hoaDonTable.Rows)
                {
                    string maMon = Convert.ToString(row["MaMon"]) ?? string.Empty;
                    string maDvpv = Convert.ToString(row["MaDVPV"]) ?? string.Empty;
                    int soLuong = Convert.ToInt32(row["SoLuong"], CultureInfo.InvariantCulture);
                    if (!int.TryParse(NormalizeMonKey(maMon), out int maMonInt))
                    {
                        throw new InvalidOperationException($"Mã món không hợp lệ: {maMon}");
                    }

                    if (!int.TryParse(maDvpv, out int maDvpvInt))
                    {
                        throw new InvalidOperationException($"Mã đơn vị phục vụ không hợp lệ: {maDvpv}");
                    }

                    InsertChiTietHoaDon(conn, tran, maHdb, maMonInt, maDvpvInt, soLuong);
                    TruKhoNguyenLieu(conn, tran, maMonInt, maDvpvInt, soLuong);
                }

                if (maKh.HasValue)
                {
                    using SqlCommand cmd = new SqlCommand(@"
UPDATE dbo.KHACH_HANG
SET DiemTichLuy = ISNULL(DiemTichLuy,0) + @Cong - @Tru,
    DiemTichLuyTronDoi = ISNULL(DiemTichLuyTronDoi,0) + @Cong
WHERE MaKH=@MaKH", conn, tran);
                    cmd.Parameters.Add("@Cong", SqlDbType.Int).Value = diemCong;
                    cmd.Parameters.Add("@Tru", SqlDbType.Int).Value = diemDung;
                    cmd.Parameters.Add("@MaKH", SqlDbType.Int).Value = maKh.Value;
                    cmd.ExecuteNonQuery();

                    if (TableExists(conn, tran, "LICH_SU_DIEM"))
                    {
                        InsertDiemHistory(conn, tran, maKh.Value, diemCong, "Tích điểm", $"Thanh toán hóa đơn #{maHdb}");
                        if (diemDung > 0)
                        {
                            InsertDiemHistory(conn, tran, maKh.Value, -diemDung, "Dùng điểm", $"Giảm giá {diemDung * 1000:N0}đ cho hóa đơn #{maHdb}");
                        }
                    }
                }

                if (maKh.HasValue && TableColumnExists(conn, tran, "KHACH_HANG", "MaHang") && maHangMoi > 0)
                {
                    using SqlCommand cmdHang = new SqlCommand("UPDATE dbo.KHACH_HANG SET MaHang = @MaHang WHERE MaKH = @MaKH", conn, tran);
                    cmdHang.Parameters.Add("@MaHang", SqlDbType.Int).Value = maHangMoi;
                    cmdHang.Parameters.Add("@MaKH", SqlDbType.Int).Value = maKh.Value;
                    cmdHang.ExecuteNonQuery();
                }

                tran.Commit();
                return maHdb;
            }
            catch
            {
                tran.Rollback();
                throw;
            }
            finally
            {
                if (conn.State == System.Data.ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }

        public int GetHangByDiemTronDoi(int diemTronDoi)
        {
            using SqlConnection conn = DbHelper.GetConnection();
            conn.Open();
            using SqlCommand cmd = new SqlCommand(@"
SELECT TOP 1 MaHang
FROM dbo.HANG_THANH_VIEN
WHERE DiemToiThieu <= @Diem
ORDER BY DiemToiThieu DESC, MaHang DESC", conn);
            cmd.Parameters.Add("@Diem", SqlDbType.Int).Value = diemTronDoi;
            object? value = cmd.ExecuteScalar();
            return value is null || value == DBNull.Value ? 1 : Convert.ToInt32(value, CultureInfo.InvariantCulture);
        }

        public DataTable GetLichSuDiem(int maKh)
        {
            using SqlConnection conn = DbHelper.GetConnection();
            const string sql = @"
SELECT MaLS, SoDiem, LoaiGD, NoiDung, NgayGD
FROM dbo.LICH_SU_DIEM
WHERE MaKH = @MaKH
ORDER BY NgayGD DESC, MaLS DESC";

            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@MaKH", SqlDbType.Int).Value = maKh;
            using SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        private static int InsertHoaDonBan(SqlConnection conn, SqlTransaction tran, string maNv, decimal tongTien, int? maKh)
        {
            bool hasIdentity;
            using (SqlCommand cmdIdentity = new SqlCommand("SELECT CASE WHEN COLUMNPROPERTY(OBJECT_ID('dbo.HOA_DON_BAN'),'MaHDB','IsIdentity') = 1 THEN 1 ELSE 0 END", conn, tran))
            {
                hasIdentity = Convert.ToInt32(cmdIdentity.ExecuteScalar() ?? 0, CultureInfo.InvariantCulture) == 1;
            }

            if (hasIdentity)
            {
                using SqlCommand cmd = new SqlCommand("INSERT INTO dbo.HOA_DON_BAN (NgayBan, MaNV, MaKH, TongTien) OUTPUT INSERTED.MaHDB VALUES (@NgayBan, @MaNV, @MaKH, @TongTien);", conn, tran);
                cmd.Parameters.Add("@NgayBan", SqlDbType.DateTime).Value = DateTime.Now;
                cmd.Parameters.Add("@MaNV", SqlDbType.VarChar, 20).Value = maNv;
                cmd.Parameters.Add("@MaKH", SqlDbType.Int).Value = maKh.HasValue ? maKh.Value : DBNull.Value;
                cmd.Parameters.Add("@TongTien", SqlDbType.Decimal).Value = tongTien;
                cmd.Parameters["@TongTien"].Precision = 18;
                cmd.Parameters["@TongTien"].Scale = 2;
                object? inserted = cmd.ExecuteScalar();
                return Convert.ToInt32(inserted, CultureInfo.InvariantCulture);
            }

            int maHdb;
            using (SqlCommand cmd = new SqlCommand("SELECT ISNULL(MAX(TRY_CAST(MaHDB AS INT)), 0) + 1 FROM dbo.HOA_DON_BAN", conn, tran))
            {
                maHdb = Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture);
            }

            using (SqlCommand cmd = new SqlCommand("INSERT INTO dbo.HOA_DON_BAN (MaHDB, NgayBan, MaNV, MaKH, TongTien) VALUES (@MaHDB, @NgayBan, @MaNV, @MaKH, @TongTien)", conn, tran))
            {
                cmd.Parameters.Add("@MaHDB", SqlDbType.Int).Value = maHdb;
                cmd.Parameters.Add("@NgayBan", SqlDbType.DateTime).Value = DateTime.Now;
                cmd.Parameters.Add("@MaNV", SqlDbType.VarChar, 20).Value = maNv;
                cmd.Parameters.Add("@MaKH", SqlDbType.Int).Value = maKh.HasValue ? maKh.Value : DBNull.Value;
                cmd.Parameters.Add("@TongTien", SqlDbType.Decimal).Value = tongTien;
                cmd.Parameters["@TongTien"].Precision = 18;
                cmd.Parameters["@TongTien"].Scale = 2;
                cmd.ExecuteNonQuery();
            }

            return maHdb;
        }

        private static void InsertDiemHistory(SqlConnection conn, SqlTransaction tran, int maKh, int soDiem, string loaiGd, string noiDung)
        {
            using SqlCommand cmd = new SqlCommand(@"
INSERT INTO dbo.LICH_SU_DIEM (MaKH, SoDiem, LoaiGD, NoiDung, NgayGD)
VALUES (@MaKH, @SoDiem, @LoaiGD, @NoiDung, GETDATE())", conn, tran);
            cmd.Parameters.Add("@MaKH", SqlDbType.Int).Value = maKh;
            cmd.Parameters.Add("@SoDiem", SqlDbType.Int).Value = soDiem;
            cmd.Parameters.Add("@LoaiGD", SqlDbType.NVarChar, 50).Value = loaiGd;
            cmd.Parameters.Add("@NoiDung", SqlDbType.NVarChar, 255).Value = noiDung;
            cmd.ExecuteNonQuery();
        }

        private static bool TableColumnExists(SqlConnection conn, SqlTransaction? tran, string tableName, string columnName)
        {
            using SqlCommand cmd = new SqlCommand("SELECT CASE WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME=@TableName AND COLUMN_NAME=@ColumnName) THEN 1 ELSE 0 END", conn, tran);
            cmd.Parameters.Add("@TableName", SqlDbType.VarChar, 128).Value = tableName;
            cmd.Parameters.Add("@ColumnName", SqlDbType.VarChar, 128).Value = columnName;
            return Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture) == 1;
        }

        private static void InsertChiTietHoaDon(SqlConnection conn, SqlTransaction tran, int maHdb, int maMon, int maDvpv, int soLuong)
        {
            using SqlCommand cmd = new SqlCommand("INSERT INTO dbo.CT_HOA_DON_BAN (MaHDB, MaMon, MaDVPV, SoLuong) VALUES (@MaHDB, @MaMon, @MaDVPV, @SoLuong)", conn, tran);
            cmd.Parameters.Add("@MaHDB", SqlDbType.Int).Value = maHdb;
            cmd.Parameters.Add("@MaMon", SqlDbType.Int).Value = maMon;
            cmd.Parameters.Add("@MaDVPV", SqlDbType.Int).Value = maDvpv;
            cmd.Parameters.Add("@SoLuong", SqlDbType.Int).Value = soLuong;
            cmd.ExecuteNonQuery();
        }

        private static void TruKhoNguyenLieu(SqlConnection conn, SqlTransaction tran, int maMon, int maDvpv, int soLuong)
        {
            const string checkSql = @"SELECT nl.MaNL, nl.SoLuongTon, ISNULL(dm.SoLuongSuDung,0) AS SoLuongSuDung
FROM dbo.DINH_MUC_MON dm
INNER JOIN dbo.NGUYEN_LIEU nl ON nl.MaNL = dm.MaNL
WHERE dm.MaMon = @MaMon AND (dm.MaDVPV = @MaDVPV OR NOT EXISTS (SELECT 1 FROM dbo.DINH_MUC_MON WHERE MaMon = @MaMon AND MaDVPV = @MaDVPV))";

            using (SqlCommand checkCmd = new SqlCommand(checkSql, conn, tran))
            {
                checkCmd.Parameters.Add("@MaMon", SqlDbType.Int).Value = maMon;
                checkCmd.Parameters.Add("@MaDVPV", SqlDbType.Int).Value = maDvpv;

                using SqlDataReader reader = checkCmd.ExecuteReader();
                while (reader.Read())
                {
                    decimal soLuongTon = reader["SoLuongTon"] is DBNull ? 0m : Convert.ToDecimal(reader["SoLuongTon"], CultureInfo.InvariantCulture);
                    decimal soLuongSuDung = reader["SoLuongSuDung"] is DBNull ? 0m : Convert.ToDecimal(reader["SoLuongSuDung"], CultureInfo.InvariantCulture);
                    decimal required = soLuongSuDung * soLuong;
                    if (soLuongTon < required)
                    {
                        reader.Close();
                        throw new InvalidOperationException("Nguyên liệu không đủ");
                    }
                }
            }

            using SqlCommand cmd = new SqlCommand(@"UPDATE nl SET nl.SoLuongTon = nl.SoLuongTon - dm.SoLuongSuDung * @SoLuong FROM dbo.NGUYEN_LIEU nl INNER JOIN dbo.DINH_MUC_MON dm ON dm.MaNL = nl.MaNL WHERE dm.MaMon = @MaMon AND (dm.MaDVPV = @MaDVPV OR NOT EXISTS (SELECT 1 FROM dbo.DINH_MUC_MON WHERE MaMon = @MaMon AND MaDVPV = @MaDVPV))", conn, tran);
            cmd.Parameters.Add("@SoLuong", SqlDbType.Int).Value = soLuong;
            cmd.Parameters.Add("@MaMon", SqlDbType.Int).Value = maMon;
            cmd.Parameters.Add("@MaDVPV", SqlDbType.Int).Value = maDvpv;
            cmd.ExecuteNonQuery();
        }

        private static string NormalizeMonKey(string? maMon)
        {
            if (string.IsNullOrWhiteSpace(maMon))
            {
                return string.Empty;
            }

            string value = maMon.Trim();
            if (value.StartsWith("MA", StringComparison.OrdinalIgnoreCase))
            {
                return value.Length > 2 ? value[2..] : string.Empty;
            }

            return value;
        }

        private static Dictionary<string, decimal> LoadGiaMapByMon(SqlConnection conn)
        {
            Dictionary<string, decimal> map = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            if (!TableExists(conn, "MON_DON_VI_PHUC_VU"))
            {
                return map;
            }

            string giaCol = ResolveMonDvpvGiaColumn(conn);
            bool hasTrangThai = TableColumnExists(conn, null, "MON_DON_VI_PHUC_VU", "TrangThai");
            string trangThaiFilter = hasTrangThai ? "WHERE ISNULL(TrangThai, N'Đang bán') <> N'Ngừng bán'" : string.Empty;

            string sql = $@"SELECT MaMon, MIN(ISNULL({giaCol}, 0)) AS DonGia
FROM dbo.MON_DON_VI_PHUC_VU
{trangThaiFilter}
GROUP BY MaMon";

            using SqlCommand cmd = new SqlCommand(sql, conn);
            using SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string key = NormalizeMonKey(Convert.ToString(reader["MaMon"]));
                decimal gia = ToDecimalValue(reader["DonGia"]);
                if (!string.IsNullOrWhiteSpace(key) && !map.ContainsKey(key))
                {
                    map[key] = gia;
                }
            }

            return map;
        }

        private static string ResolveMonDvpvGiaColumn(SqlConnection conn)
        {
            const string sql = @"SELECT TOP 1 COLUMN_NAME
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'dbo'
  AND TABLE_NAME = 'MON_DON_VI_PHUC_VU'
  AND COLUMN_NAME IN ('DonGia', 'GiaBan', 'Gia')
ORDER BY CASE COLUMN_NAME WHEN 'DonGia' THEN 1 WHEN 'GiaBan' THEN 2 WHEN 'Gia' THEN 3 ELSE 99 END";

            using SqlCommand cmd = new SqlCommand(sql, conn);
            string? columnName = Convert.ToString(cmd.ExecuteScalar());
            return string.IsNullOrWhiteSpace(columnName) ? "DonGia" : columnName;
        }

        private static string ResolveMonBanGiaExpression(SqlConnection conn, string tableAlias)
        {
            const string sql = @"SELECT TOP 1 COLUMN_NAME
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'dbo'
  AND TABLE_NAME = 'MON_BAN'
  AND COLUMN_NAME IN ('DonGia', 'GiaBan', 'Gia')
ORDER BY CASE COLUMN_NAME WHEN 'DonGia' THEN 1 WHEN 'GiaBan' THEN 2 WHEN 'Gia' THEN 3 ELSE 99 END";

            using SqlCommand cmd = new SqlCommand(sql, conn);
            string? columnName = Convert.ToString(cmd.ExecuteScalar());
            return string.IsNullOrWhiteSpace(columnName)
                ? "CAST(0 AS DECIMAL(18,2))"
                : $"ISNULL({tableAlias}.{columnName}, 0)";
        }

        private static bool TableExists(SqlConnection conn, string tableName)
        {
            using SqlCommand cmd = new SqlCommand("SELECT CASE WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME=@TableName) THEN 1 ELSE 0 END", conn);
            cmd.Parameters.Add("@TableName", SqlDbType.VarChar, 128).Value = tableName;
            return Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture) == 1;
        }

        private static bool TableExists(SqlConnection conn, SqlTransaction? tran, string tableName)
        {
            using SqlCommand cmd = new SqlCommand("SELECT CASE WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME=@TableName) THEN 1 ELSE 0 END", conn, tran);
            cmd.Parameters.Add("@TableName", SqlDbType.VarChar, 128).Value = tableName;
            return Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture) == 1;
        }

        private static decimal ToDecimalValue(object? value)
        {
            if (value is null || value == DBNull.Value)
            {
                return 0m;
            }

            if (value is decimal d)
            {
                return d;
            }

            if (decimal.TryParse(Convert.ToString(value), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal parsedInvariant))
            {
                return parsedInvariant;
            }

            return decimal.TryParse(Convert.ToString(value), NumberStyles.Any, CultureInfo.CurrentCulture, out decimal parsedCurrent)
                ? parsedCurrent
                : 0m;
        }
    }
}
