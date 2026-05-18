using PBL3.DataBase;
using System.Data;
using System.Data.SqlClient;

namespace PBL3.DataAccess
{
    internal sealed class TrangHoaDonRepository
    {
        public DataTable GetHoaDonMaster(string loaiHoaDon, string maNv)
        {
            using SqlConnection conn = DbHelper.GetConnection();
            conn.Open();

            bool hasLyDoHuy = false;
            if (loaiHoaDon == "ban")
            {
                EnsureLyDoHuyColumn(conn);
                hasLyDoHuy = TableColumnExists(conn, "HOA_DON_BAN", "LyDoHuy");
            }

            string sql = loaiHoaDon == "ban"
                ? @"SELECT hdb.MaHDB AS MaHD, hdb.NgayBan AS ThoiGian, COALESCE(NULLIF(CAST(kh.SDT AS NVARCHAR(50)), N''), N'Khách lẻ') AS DoiTac,
                          ISNULL(kh.SDT, N'') AS SDT, ISNULL(hdb.TongTien,0) AS TongTien, ISNULL(nv.HoTen, N'-') AS NhanVien,
                          ISNULL(hv.TenHang, N'') AS TenHang, ISNULL(hv.PhanTramGiam, 0) AS PhanTramGiam,
                          ISNULL(kh.DiemTichLuy, 0) AS DiemTichLuy
                   FROM HOA_DON_BAN hdb
                   LEFT JOIN KHACH_HANG kh ON kh.MaKH = hdb.MaKH
                   LEFT JOIN NHAN_VIEN nv ON nv.MaNV = hdb.MaNV
                   OUTER APPLY (
                       SELECT TOP 1 hv2.TenHang, hv2.PhanTramGiam
                       FROM dbo.HANG_THANH_VIEN hv2
                       WHERE hv2.DiemToiThieu <= ISNULL(kh.DiemTichLuyTronDoi, 0)
                       ORDER BY hv2.DiemToiThieu DESC, hv2.MaHang DESC
                   ) hv
                   WHERE hdb.MaNV = @MaNV" + (hasLyDoHuy ? " AND hdb.LyDoHuy IS NULL" : string.Empty) + @"
                   ORDER BY hdb.MaHDB DESC"
                : @"SELECT hdn.MaHDN AS MaHD, hdn.NgayNhap AS ThoiGian, ISNULL(ncc.TenNCC, N'-') AS DoiTac,
                          ISNULL(ncc.SDT, N'') AS SDT, ISNULL(hdn.TongTien,0) AS TongTien, ISNULL(nv.HoTen, N'-') AS NhanVien,
                          CAST(N'' AS NVARCHAR(50)) AS TenHang, CAST(0 AS INT) AS PhanTramGiam,
                          CAST(0 AS INT) AS DiemTichLuy
                   FROM HOA_DON_NHAP hdn
                   LEFT JOIN NHA_CUNG_CAP ncc ON ncc.MaNCC = hdn.MaNCC
                   LEFT JOIN NHAN_VIEN nv ON nv.MaNV = hdn.MaNV
                   WHERE hdn.MaNV = @MaNV
                   ORDER BY hdn.MaHDN DESC";

            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@MaNV", maNv);
            using SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        /// <summary>Tính tổng tiền hàng gốc (trước giảm giá) từ chi tiết hóa đơn bán.</summary>
        public decimal GetTienHangGoc(string maHd)
        {
            using SqlConnection conn = DbHelper.GetConnection();
            conn.Open();

            string? thanhTienCol = DetectFirstColumn(conn, "CT_HOA_DON_BAN", "ThanhTien", "TongTien", "Tien", "GiaTien");
            string? donGiaCol = DetectFirstColumn(conn, "CT_HOA_DON_BAN", "DonGia", "DonGiaBan", "Gia", "GiaBan", "DonGiaCT", "DonGiaBanLe", "GiaTien");
            string? mdvGiaCol = DetectFirstColumn(conn, "MON_DON_VI_PHUC_VU", "DonGia", "GiaBan", "Gia", "DonGiaBan", "GiaTien");
            string? mdvMaDvpvCol = DetectFirstColumn(conn, "MON_DON_VI_PHUC_VU", "MaDVPV", "MaDV", "MaDonViPhucVu");
            bool hasCtMaDvpv = TableColumnExists(conn, "CT_HOA_DON_BAN", "MaDVPV");

            string donGiaExpr;
            string mdvJoin = string.Empty;
            if (donGiaCol is not null)
            {
                donGiaExpr = $"ISNULL(ct.[{donGiaCol}], 0)";
            }
            else if (mdvGiaCol is not null && hasCtMaDvpv && !string.IsNullOrWhiteSpace(mdvMaDvpvCol))
            {
                mdvJoin = $"\nLEFT JOIN dbo.MON_DON_VI_PHUC_VU mdv ON mdv.MaMon = ct.MaMon AND mdv.[{mdvMaDvpvCol}] = ct.MaDVPV";
                string? monGiaCol = DetectFirstColumn(conn, "MON_BAN", "DonGia", "GiaBan", "Gia", "DonGiaBan", "DonGiaBanLe", "GiaTien");
                string monGiaExpr = monGiaCol is null ? "CAST(0 AS DECIMAL(18,2))" : $"ISNULL(mb.[{monGiaCol}], 0)";
                donGiaExpr = $"ISNULL(mdv.[{mdvGiaCol}], {monGiaExpr})";
            }
            else
            {
                string? monGiaCol = DetectFirstColumn(conn, "MON_BAN", "DonGia", "GiaBan", "Gia", "DonGiaBan", "DonGiaBanLe", "GiaTien");
                donGiaExpr = monGiaCol is null ? "CAST(0 AS DECIMAL(18,2))" : $"ISNULL(mb.[{monGiaCol}], 0)";
            }

            string thanhTienExpr = thanhTienCol is not null
                ? $"ISNULL(ct.[{thanhTienCol}], ISNULL(ct.SoLuong,0) * {donGiaExpr})"
                : $"ISNULL(ct.SoLuong,0) * {donGiaExpr}";

            string sql = $@"SELECT ISNULL(SUM({thanhTienExpr}), 0)
FROM CT_HOA_DON_BAN ct
LEFT JOIN MON_BAN mb ON mb.MaMon = ct.MaMon
{mdvJoin}
WHERE ct.MaHDB = @MaHD";

            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@MaHD", SqlDbType.VarChar, 20).Value = maHd;
            object? result = cmd.ExecuteScalar();
            return result is null || result == DBNull.Value ? 0m : Convert.ToDecimal(result, System.Globalization.CultureInfo.InvariantCulture);
        }

        public bool RequestCancelBanInvoice(string maHd, string lyDoHuy)
        {
            using SqlConnection conn = DbHelper.GetConnection();
            conn.Open();
            EnsureLyDoHuyColumn(conn);
            EnsureTrangThaiColumn(conn);

            using SqlCommand cmd = new SqlCommand(@"
UPDATE dbo.HOA_DON_BAN
SET LyDoHuy = @LyDo,
    TrangThai = 0
WHERE MaHDB = @MaHD AND LyDoHuy IS NULL", conn);
            cmd.Parameters.Add("@LyDo", SqlDbType.NVarChar, 500).Value = lyDoHuy;
            cmd.Parameters.Add("@MaHD", SqlDbType.VarChar, 20).Value = maHd;
            return cmd.ExecuteNonQuery() > 0;
        }

        public DataTable GetHoaDonDetail(string loaiHoaDon, string maHd)
        {
            using SqlConnection conn = DbHelper.GetConnection();
            conn.Open();

            string sql;
            if (loaiHoaDon == "ban")
            {
                string? donGiaCol = DetectFirstColumn(conn, "CT_HOA_DON_BAN", "DonGia", "DonGiaBan", "Gia", "GiaBan", "DonGiaCT", "DonGiaBanLe", "GiaTien");
                string? monGiaCol = DetectFirstColumn(conn, "MON_BAN", "DonGia", "GiaBan", "Gia", "DonGiaBan", "DonGiaBanLe", "GiaTien");
                string? thanhTienCol = DetectFirstColumn(conn, "CT_HOA_DON_BAN", "ThanhTien", "TongTien", "Tien", "GiaTien");
                bool hasCtMaDvpv = TableColumnExists(conn, "CT_HOA_DON_BAN", "MaDVPV");
                string? mdvGiaCol = DetectFirstColumn(conn, "MON_DON_VI_PHUC_VU", "DonGia", "GiaBan", "Gia", "DonGiaBan", "GiaTien");
                string? mdvMaDvpvCol = DetectFirstColumn(conn, "MON_DON_VI_PHUC_VU", "MaDVPV", "MaDV", "MaDonViPhucVu");
                string monGiaExpr = monGiaCol is null ? "CAST(0 AS DECIMAL(18,2))" : $"ISNULL(mb.[{monGiaCol}], 0)";
                string donGiaExpr;
                string mdvJoin = string.Empty;

                if (donGiaCol is not null)
                {
                    donGiaExpr = $"ISNULL(ct.[{donGiaCol}], 0)";
                }
                else if (mdvGiaCol is not null && hasCtMaDvpv && !string.IsNullOrWhiteSpace(mdvMaDvpvCol))
                {
                    mdvJoin = $"\nLEFT JOIN dbo.MON_DON_VI_PHUC_VU mdv ON mdv.MaMon = ct.MaMon AND mdv.[{mdvMaDvpvCol}] = ct.MaDVPV";
                    donGiaExpr = $"ISNULL(mdv.[{mdvGiaCol}], {monGiaExpr})";
                }
                else
                {
                    donGiaExpr = monGiaExpr;
                }

                string thanhTienExpr = thanhTienCol is not null ? $"ISNULL(ct.[{thanhTienCol}], ISNULL(ct.SoLuong,0) * {donGiaExpr})" : $"ISNULL(ct.SoLuong,0) * {donGiaExpr}";

                sql = $@"
SELECT ISNULL(mb.TenMon, CONVERT(NVARCHAR(100), ct.MaMon)) AS TenHang,
       ISNULL(ct.SoLuong, 0) AS SoLuong,
       CASE WHEN {donGiaExpr} = 0 AND ISNULL(ct.SoLuong,0) > 0 THEN ({thanhTienExpr} / NULLIF(ISNULL(ct.SoLuong,0),0)) ELSE {donGiaExpr} END AS DonGia,
       {thanhTienExpr} AS ThanhTien
FROM CT_HOA_DON_BAN ct
LEFT JOIN MON_BAN mb ON mb.MaMon = ct.MaMon
{mdvJoin}
WHERE ct.MaHDB = @MaHD";
            }
            else
            {
                string? donGiaCol = DetectFirstColumn(conn, "CT_HOA_DON_NHAP", "DonGia", "DonGiaNhap", "Gia", "GiaNhap", "GiaTien");
                string? thanhTienCol = DetectFirstColumn(conn, "CT_HOA_DON_NHAP", "ThanhTien", "TongTien", "Tien", "GiaTien");
                string donGiaExpr = donGiaCol is not null ? $"ISNULL(ct.[{donGiaCol}], 0)" : "CAST(0 AS DECIMAL(18,2))";
                string thanhTienExpr = thanhTienCol is not null ? $"ISNULL(ct.[{thanhTienCol}], ISNULL(ct.SoLuong,0) * {donGiaExpr})" : $"ISNULL(ct.SoLuong,0) * {donGiaExpr}";

                sql = $@"
SELECT ISNULL(nl.TenNL, CONVERT(NVARCHAR(100), ct.MaNL)) AS TenHang,
       ISNULL(nl.DonViTinh, N'') AS DonViTinh,
       ISNULL(ct.SoLuong, 0) AS SoLuong,
       CASE WHEN {donGiaExpr} = 0 AND ISNULL(ct.SoLuong,0) > 0 THEN ({thanhTienExpr} / NULLIF(ISNULL(ct.SoLuong,0),0)) ELSE {donGiaExpr} END AS DonGia,
       {thanhTienExpr} AS ThanhTien
FROM CT_HOA_DON_NHAP ct
LEFT JOIN NGUYEN_LIEU nl ON nl.MaNL = ct.MaNL
WHERE ct.MaHDN = @MaHD";
            }

            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@MaHD", SqlDbType.VarChar, 20).Value = maHd;
            using SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        public void DeleteHoaDon(string loaiHoaDon, string maHd)
        {
            string detailTable = loaiHoaDon == "ban" ? "CT_HOA_DON_BAN" : "CT_HOA_DON_NHAP";
            string headerTable = loaiHoaDon == "ban" ? "HOA_DON_BAN" : "HOA_DON_NHAP";
            string key = loaiHoaDon == "ban" ? "MaHDB" : "MaHDN";

            using SqlConnection conn = DbHelper.GetConnection();
            conn.Open();
            using SqlTransaction tran = conn.BeginTransaction();
            try
            {
                using SqlCommand cmdDetail = new SqlCommand($"DELETE FROM {detailTable} WHERE {key} = @MaHD", conn, tran);
                cmdDetail.Parameters.AddWithValue("@MaHD", maHd);
                cmdDetail.ExecuteNonQuery();

                using SqlCommand cmdHeader = new SqlCommand($"DELETE FROM {headerTable} WHERE {key} = @MaHD", conn, tran);
                cmdHeader.Parameters.AddWithValue("@MaHD", maHd);
                cmdHeader.ExecuteNonQuery();

                tran.Commit();
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }

        public bool ValidateManagerPassword(string password)
        {
            using SqlConnection conn = DbHelper.GetConnection();
            using SqlCommand cmd = new SqlCommand(@"
SELECT COUNT(1)
FROM NHAN_VIEN nv
LEFT JOIN CHUC_VU cv ON nv.MaCV = cv.MaCV
WHERE nv.MatKhau = @MatKhau
  AND (
        ISNULL(cv.TenCV, N'') LIKE N'%Quản%'
        OR ISNULL(cv.TenCV, N'') LIKE N'%Admin%'
      )", conn);
            cmd.Parameters.AddWithValue("@MatKhau", password);

            conn.Open();
            object? result = cmd.ExecuteScalar();
            return result is not null && Convert.ToInt32(result) > 0;
        }

        private static string? DetectFirstColumn(SqlConnection conn, string tableName, params string[] candidates)
        {
            foreach (string candidate in candidates)
            {
                using SqlCommand cmd = new SqlCommand(@"SELECT CASE WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME=@TableName AND COLUMN_NAME=@Col) THEN 1 ELSE 0 END", conn);
                cmd.Parameters.AddWithValue("@TableName", tableName);
                cmd.Parameters.AddWithValue("@Col", candidate);
                object? result = cmd.ExecuteScalar();
                if (result is not null && Convert.ToInt32(result) == 1)
                {
                    return candidate;
                }
            }

            return null;
        }

        private static bool TableColumnExists(SqlConnection conn, string tableName, string columnName)
        {
            using SqlCommand cmd = new SqlCommand(@"
SELECT CASE WHEN EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME=@TableName AND COLUMN_NAME=@Col
) THEN 1 ELSE 0 END", conn);
            cmd.Parameters.AddWithValue("@TableName", tableName);
            cmd.Parameters.AddWithValue("@Col", columnName);
            object? res = cmd.ExecuteScalar();
            return res is not null && Convert.ToInt32(res) == 1;
        }

        private static void EnsureLyDoHuyColumn(SqlConnection conn)
        {
            const string sql = @"
IF COL_LENGTH('dbo.HOA_DON_BAN', 'LyDoHuy') IS NULL
BEGIN
    ALTER TABLE dbo.HOA_DON_BAN
    ADD LyDoHuy NVARCHAR(500) NULL;
END";
            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.ExecuteNonQuery();
        }

        private static void EnsureTrangThaiColumn(SqlConnection conn)
        {
            const string sql = @"
IF COL_LENGTH('dbo.HOA_DON_BAN', 'TrangThai') IS NULL
BEGIN
    ALTER TABLE dbo.HOA_DON_BAN
    ADD TrangThai bit NOT NULL CONSTRAINT DF_HOA_DON_BAN_TrangThai DEFAULT (1) WITH VALUES;
END";
            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.ExecuteNonQuery();
        }
    }
}
