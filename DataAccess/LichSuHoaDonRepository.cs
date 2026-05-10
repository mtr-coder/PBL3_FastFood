using PBL3.DataBase;
using PBL3.Models;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace PBL3.DataAccess
{
    internal sealed class LichSuHoaDonRepository
    {
        public LichSuHoaDonSchemaInfo DetectSchema()
        {
            using SqlConnection conn = DbHelper.GetConnection();
            conn.Open();

            EnsureTrangThaiColumns(conn);

            bool hasHdb = TableColumnExists(conn, "HOA_DON_BAN", "TrangThai");
            bool hasHdn = TableColumnExists(conn, "HOA_DON_NHAP", "TrangThai");

            return new LichSuHoaDonSchemaInfo
            {
                HasTrangThaiHdb = hasHdb,
                HasTrangThaiHdn = hasHdn,
                TrangThaiHdbType = hasHdb ? GetColumnDataType(conn, "HOA_DON_BAN", "TrangThai") : string.Empty,
                TrangThaiHdnType = hasHdn ? GetColumnDataType(conn, "HOA_DON_NHAP", "TrangThai") : string.Empty
            };
        }

        public (DateTime TuNgay, DateTime DenNgay) GetDefaultDateRange(bool isAdmin)
        {
            if (!isAdmin)
            {
                DateTime today = DateTime.Today;
                return (today, today);
            }

            DateTime tuNgay = DateTime.Today.AddDays(-30);
            DateTime denNgay = DateTime.Today;

            try
            {
                using SqlConnection conn = DbHelper.GetConnection();
                conn.Open();

                const string sqlMin = @"
SELECT MIN(NgayAny)
FROM (
    SELECT MIN(NgayBan) AS NgayAny FROM dbo.HOA_DON_BAN
    UNION ALL
    SELECT MIN(NgayNhap) AS NgayAny FROM dbo.HOA_DON_NHAP
) x
WHERE NgayAny IS NOT NULL";

                const string sqlMax = @"
SELECT MAX(NgayAny)
FROM (
    SELECT MAX(NgayBan) AS NgayAny FROM dbo.HOA_DON_BAN
    UNION ALL
    SELECT MAX(NgayNhap) AS NgayAny FROM dbo.HOA_DON_NHAP
) x
WHERE NgayAny IS NOT NULL";

                using (SqlCommand cmdMin = new SqlCommand(sqlMin, conn))
                {
                    object? minObj = cmdMin.ExecuteScalar();
                    if (minObj is not null && minObj != DBNull.Value)
                    {
                        tuNgay = Convert.ToDateTime(minObj, CultureInfo.InvariantCulture).Date;
                    }
                }

                using (SqlCommand cmdMax = new SqlCommand(sqlMax, conn))
                {
                    object? maxObj = cmdMax.ExecuteScalar();
                    if (maxObj is not null && maxObj != DBNull.Value)
                    {
                        DateTime maxDate = Convert.ToDateTime(maxObj, CultureInfo.InvariantCulture).Date;
                        denNgay = maxDate > denNgay ? maxDate : denNgay;
                    }
                }
            }
            catch
            {
            }

            return (tuNgay, denNgay);
        }

        public DataTable GetMasterData(string invoiceType, DateTime fromDate, DateTime toDate, bool isAdmin, string? maNvDangNhap, LichSuHoaDonSchemaInfo schema)
        {
            string statusExpr = invoiceType == "BAN"
                ? BuildTrangThaiExpression("h", schema.HasTrangThaiHdb, schema.TrangThaiHdbType)
                : BuildTrangThaiExpression("h", schema.HasTrangThaiHdn, schema.TrangThaiHdnType);

            string sql;
            if (invoiceType == "BAN")
            {
                sql = $@"
SELECT h.MaHDB AS MaHD,
       h.NgayBan AS ThoiGian,
       ISNULL(nv.HoTen, N'') AS NguoiThucHien,
       ISNULL(kh.SDT, N'Kh' + NCHAR(225) + N'ch l' + NCHAR(7867)) AS DoiTac,
       ISNULL(kh.SDT, N'') AS SDTKhach,
       ISNULL(h.TongTien, 0) AS TongTien,
       ISNULL(hv.TenHang, N'') AS TenHang,
       ISNULL(hv.PhanTramGiam, 0) AS PhanTramGiam,
       ISNULL(kh.DiemTichLuy, 0) AS DiemTichLuy,
       CAST(N'' AS NVARCHAR(20)) AS SDT,
       {statusExpr} AS TrangThai,
       CONVERT(VARCHAR(20), h.MaNV) AS MaNV,
       CONVERT(VARCHAR(20), h.MaKH) AS MaDoiTac
FROM dbo.HOA_DON_BAN h
LEFT JOIN dbo.NHAN_VIEN nv ON nv.MaNV = h.MaNV
LEFT JOIN dbo.KHACH_HANG kh ON kh.MaKH = h.MaKH
OUTER APPLY (
    SELECT TOP 1 hv2.TenHang, hv2.PhanTramGiam
    FROM dbo.HANG_THANH_VIEN hv2
    WHERE hv2.DiemToiThieu <= ISNULL(kh.DiemTichLuyTronDoi, 0)
    ORDER BY hv2.DiemToiThieu DESC, hv2.MaHang DESC
) hv
WHERE h.NgayBan >= @FromDate AND h.NgayBan < @ToDate";

                if (schema.HasTrangThaiHdb)
                {
                    sql += schema.TrangThaiHdbType == "bit"
                        ? " AND ISNULL(h.TrangThai, 1) = 1"
                        : " AND CAST(h.TrangThai AS NVARCHAR(50)) NOT LIKE N'%hủy%'";
                }

                if (!isAdmin && !string.IsNullOrWhiteSpace(maNvDangNhap))
                {
                    sql += " AND CONVERT(VARCHAR(20), h.MaNV) = @MaNV";
                }

                sql += " ORDER BY h.NgayBan DESC";
            }
            else
            {
                sql = $@"
SELECT h.MaHDN AS MaHD,
       h.NgayNhap AS ThoiGian,
       ISNULL(nv.HoTen, N'') AS NguoiThucHien,
       ISNULL(ncc.TenNCC, N'') AS DoiTac,
       CAST(N'' AS NVARCHAR(20)) AS SDTKhach,
       ISNULL(h.TongTien, 0) AS TongTien,
       CAST(N'' AS NVARCHAR(50)) AS TenHang,
       CAST(0 AS INT) AS PhanTramGiam,
       CAST(0 AS INT) AS DiemTichLuy,
       ISNULL(ncc.SDT, N'') AS SDT,
       {statusExpr} AS TrangThai,
       CONVERT(VARCHAR(20), h.MaNV) AS MaNV,
       CONVERT(VARCHAR(20), h.MaNCC) AS MaDoiTac
FROM dbo.HOA_DON_NHAP h
LEFT JOIN dbo.NHAN_VIEN nv ON nv.MaNV = h.MaNV
LEFT JOIN dbo.NHA_CUNG_CAP ncc ON ncc.MaNCC = h.MaNCC
WHERE h.NgayNhap >= @FromDate AND h.NgayNhap < @ToDate";

                if (schema.HasTrangThaiHdn)
                {
                    sql += schema.TrangThaiHdnType == "bit"
                        ? " AND ISNULL(h.TrangThai, 1) = 1"
                        : " AND CAST(h.TrangThai AS NVARCHAR(50)) NOT LIKE N'%hủy%'";
                }

                if (!isAdmin && !string.IsNullOrWhiteSpace(maNvDangNhap))
                {
                    sql += " AND CONVERT(VARCHAR(20), h.MaNV) = @MaNV";
                }

                sql += " ORDER BY h.NgayNhap DESC";
            }

            using SqlConnection conn = DbHelper.GetConnection();
            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@FromDate", SqlDbType.DateTime).Value = fromDate.Date;
            cmd.Parameters.Add("@ToDate", SqlDbType.DateTime).Value = toDate.Date.AddDays(1);
            if (!isAdmin && !string.IsNullOrWhiteSpace(maNvDangNhap))
            {
                cmd.Parameters.Add("@MaNV", SqlDbType.VarChar, 20).Value = maNvDangNhap;
            }

            using SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        public DataTable GetDetailData(string invoiceType, string maHd)
        {
            using SqlConnection conn = DbHelper.GetConnection();
            conn.Open();

            string sql;
            if (invoiceType == "BAN")
            {
                string? donGiaCol = DetectFirstColumn(conn, "CT_HOA_DON_BAN", "DonGia", "DonGiaBan", "Gia", "GiaBan", "DonGiaCT", "DonGiaBanLe");
                string? ghiChuCol = DetectFirstColumn(conn, "CT_HOA_DON_BAN", "GhiChu", "MoTa", "Note");
                bool hasCtMaDvpv = TableColumnExists(conn, "CT_HOA_DON_BAN", "MaDVPV");
                string? mdvGiaCol = DetectFirstColumn(conn, "MON_DON_VI_PHUC_VU", "DonGia", "GiaBan", "Gia", "DonGiaBan");
                string? mdvMaDvpvCol = DetectFirstColumn(conn, "MON_DON_VI_PHUC_VU", "MaDVPV", "MaDV", "MaDonViPhucVu");
                string? monGiaCol = DetectFirstColumn(conn, "MON_BAN", "DonGia", "GiaBan", "Gia", "DonGiaBan", "DonGiaBanLe");

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

                string ghiChuExpr = ghiChuCol is null ? "CAST(NULL AS NVARCHAR(300))" : $"ct.[{ghiChuCol}]";

                sql = $@"
SELECT ISNULL(mb.TenMon, CONVERT(NVARCHAR(100), ct.MaMon)) AS TenHang,
       ISNULL(ct.SoLuong, 0) AS SoLuong,
       {donGiaExpr} AS DonGia,
       ISNULL(ct.SoLuong, 0) * {donGiaExpr} AS ThanhTien,
       {ghiChuExpr} AS GhiChu
FROM dbo.CT_HOA_DON_BAN ct
LEFT JOIN dbo.MON_BAN mb ON mb.MaMon = ct.MaMon
{mdvJoin}
WHERE ct.MaHDB = @MaHD";
            }
            else
            {
                sql = @"
SELECT ISNULL(nl.TenNL, CONVERT(NVARCHAR(100), ct.MaNL)) AS TenHang,
       ISNULL(ct.SoLuong, 0) AS SoLuong,
       ISNULL(ct.DonGia, 0) AS DonGia,
       ISNULL(ct.SoLuong, 0) * ISNULL(ct.DonGia, 0) AS ThanhTien,
       CAST(NULL AS NVARCHAR(300)) AS GhiChu
FROM dbo.CT_HOA_DON_NHAP ct
LEFT JOIN dbo.NGUYEN_LIEU nl ON nl.MaNL = ct.MaNL
WHERE ct.MaHDN = @MaHD";
            }

            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@MaHD", SqlDbType.VarChar, 20).Value = maHd;
            using SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable detail = new DataTable();
            da.Fill(detail);
            return detail;
        }

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
            return Convert.ToDecimal(result ?? 0m);
        }

        public int GetCanceledCount(DateTime fromDate, DateTime toDate, LichSuHoaDonSchemaInfo schema)
        {
            int soDonHuy = 0;
            using SqlConnection conn = DbHelper.GetConnection();
            conn.Open();

            if (schema.HasTrangThaiHdb)
            {
                string where = schema.TrangThaiHdbType == "bit" ? "ISNULL(TrangThai,1)=0" : "CAST(TrangThai AS NVARCHAR(50)) LIKE N'%hủy%'";
                using SqlCommand cmd = new SqlCommand($"SELECT COUNT(1) FROM dbo.HOA_DON_BAN WHERE NgayBan >= @FromDate AND NgayBan < @ToDate AND {where}", conn);
                cmd.Parameters.Add("@FromDate", SqlDbType.DateTime).Value = fromDate.Date;
                cmd.Parameters.Add("@ToDate", SqlDbType.DateTime).Value = toDate.Date.AddDays(1);
                soDonHuy += Convert.ToInt32(cmd.ExecuteScalar() ?? 0, CultureInfo.InvariantCulture);
            }

            if (schema.HasTrangThaiHdn)
            {
                string where = schema.TrangThaiHdnType == "bit" ? "ISNULL(TrangThai,1)=0" : "CAST(TrangThai AS NVARCHAR(50)) LIKE N'%hủy%'";
                using SqlCommand cmd = new SqlCommand($"SELECT COUNT(1) FROM dbo.HOA_DON_NHAP WHERE NgayNhap >= @FromDate AND NgayNhap < @ToDate AND {where}", conn);
                cmd.Parameters.Add("@FromDate", SqlDbType.DateTime).Value = fromDate.Date;
                cmd.Parameters.Add("@ToDate", SqlDbType.DateTime).Value = toDate.Date.AddDays(1);
                soDonHuy += Convert.ToInt32(cmd.ExecuteScalar() ?? 0, CultureInfo.InvariantCulture);
            }

            return soDonHuy;
        }

        public void CancelInvoice(string invoiceType, string maHd, LichSuHoaDonSchemaInfo schema)
        {
            using SqlConnection conn = DbHelper.GetConnection();
            conn.Open();
            using SqlTransaction tran = conn.BeginTransaction();

            if (invoiceType == "BAN")
            {
                CancelBanInvoice(conn, tran, maHd, schema);
            }
            else
            {
                CancelNhapInvoice(conn, tran, maHd, schema);
            }

            tran.Commit();
        }

        private static void CancelBanInvoice(SqlConnection conn, SqlTransaction tran, string maHd, LichSuHoaDonSchemaInfo schema)
        {
            if (!schema.HasTrangThaiHdb)
            {
                throw new InvalidOperationException("CSDL chưa có cột TrangThai cho hóa đơn bán.");
            }

            EnsureNotCanceled(conn, tran, "HOA_DON_BAN", "MaHDB", maHd, schema.TrangThaiHdbType);
            UpdateInvoiceStatus(conn, tran, "HOA_DON_BAN", "MaHDB", maHd, schema.TrangThaiHdbType);

            const string rollbackStockSql = @"
UPDATE nl
SET nl.SoLuongTon = nl.SoLuongTon + (dm.SoLuongSuDung * ct.SoLuong)
FROM dbo.NGUYEN_LIEU nl
INNER JOIN dbo.DINH_MUC_MON dm ON dm.MaNL = nl.MaNL
INNER JOIN dbo.CT_HOA_DON_BAN ct ON ct.MaMon = dm.MaMon
WHERE ct.MaHDB = @MaHD
  AND (dm.MaDVPV = ct.MaDVPV
       OR NOT EXISTS (
            SELECT 1
            FROM dbo.DINH_MUC_MON d2
            WHERE d2.MaMon = ct.MaMon AND d2.MaDVPV = ct.MaDVPV
       ))";

            using SqlCommand cmdStock = new SqlCommand(rollbackStockSql, conn, tran);
            cmdStock.Parameters.Add("@MaHD", SqlDbType.VarChar, 20).Value = maHd;
            cmdStock.ExecuteNonQuery();
        }

        private static void CancelNhapInvoice(SqlConnection conn, SqlTransaction tran, string maHd, LichSuHoaDonSchemaInfo schema)
        {
            if (!schema.HasTrangThaiHdn)
            {
                throw new InvalidOperationException("CSDL chưa có cột TrangThai cho hóa đơn nhập.");
            }

            EnsureNotCanceled(conn, tran, "HOA_DON_NHAP", "MaHDN", maHd, schema.TrangThaiHdnType);
            UpdateInvoiceStatus(conn, tran, "HOA_DON_NHAP", "MaHDN", maHd, schema.TrangThaiHdnType);

            const string rollbackStockSql = @"
UPDATE nl
SET nl.SoLuongTon = nl.SoLuongTon - ct.SoLuong
FROM dbo.NGUYEN_LIEU nl
INNER JOIN dbo.CT_HOA_DON_NHAP ct ON ct.MaNL = nl.MaNL
WHERE ct.MaHDN = @MaHD";

            using SqlCommand cmdStock = new SqlCommand(rollbackStockSql, conn, tran);
            cmdStock.Parameters.Add("@MaHD", SqlDbType.VarChar, 20).Value = maHd;
            cmdStock.ExecuteNonQuery();
        }

        private static void EnsureNotCanceled(SqlConnection conn, SqlTransaction tran, string tableName, string idColumn, string maHd, string statusType)
        {
            string sql = statusType == "bit"
                ? $"SELECT CASE WHEN ISNULL(TrangThai,1)=0 THEN 1 ELSE 0 END FROM dbo.{tableName} WHERE {idColumn} = @MaHD"
                : $"SELECT CASE WHEN CAST(TrangThai AS NVARCHAR(50)) LIKE N'%hủy%' THEN 1 ELSE 0 END FROM dbo.{tableName} WHERE {idColumn} = @MaHD";

            using SqlCommand cmd = new SqlCommand(sql, conn, tran);
            cmd.Parameters.Add("@MaHD", SqlDbType.VarChar, 20).Value = maHd;
            bool isCanceled = Convert.ToInt32(cmd.ExecuteScalar() ?? 0, CultureInfo.InvariantCulture) == 1;
            if (isCanceled)
            {
                throw new InvalidOperationException("Hóa đơn đã ở trạng thái Đã hủy.");
            }
        }

        private static void UpdateInvoiceStatus(SqlConnection conn, SqlTransaction tran, string tableName, string idColumn, string maHd, string statusType)
        {
            string sql = $"UPDATE dbo.{tableName} SET TrangThai = @TrangThai WHERE {idColumn} = @MaHD";
            using SqlCommand cmd = new SqlCommand(sql, conn, tran);
            cmd.Parameters.Add("@MaHD", SqlDbType.VarChar, 20).Value = maHd;
            if (statusType == "bit")
            {
                cmd.Parameters.Add("@TrangThai", SqlDbType.Bit).Value = false;
            }
            else
            {
                cmd.Parameters.Add("@TrangThai", SqlDbType.NVarChar, 50).Value = "Đã hủy";
            }

            int rows = cmd.ExecuteNonQuery();
            if (rows == 0)
            {
                throw new InvalidOperationException("Không tìm thấy hóa đơn để cập nhật trạng thái.");
            }
        }

        private static void EnsureTrangThaiColumns(SqlConnection conn)
        {
            const string sql = @"
IF COL_LENGTH('dbo.HOA_DON_BAN', 'TrangThai') IS NULL
BEGIN
    ALTER TABLE dbo.HOA_DON_BAN
    ADD TrangThai bit NOT NULL CONSTRAINT DF_HOA_DON_BAN_TrangThai DEFAULT (1) WITH VALUES;
END

IF COL_LENGTH('dbo.HOA_DON_NHAP', 'TrangThai') IS NULL
BEGIN
    ALTER TABLE dbo.HOA_DON_NHAP
    ADD TrangThai bit NOT NULL CONSTRAINT DF_HOA_DON_NHAP_TrangThai DEFAULT (1) WITH VALUES;
END";

            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.ExecuteNonQuery();
        }

        private static bool TableColumnExists(SqlConnection conn, string tableName, string columnName)
        {
            using SqlCommand cmd = new SqlCommand(@"SELECT CASE WHEN EXISTS (
SELECT 1
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME=@TableName AND COLUMN_NAME=@ColumnName
) THEN 1 ELSE 0 END", conn);
            cmd.Parameters.Add("@TableName", SqlDbType.VarChar, 128).Value = tableName;
            cmd.Parameters.Add("@ColumnName", SqlDbType.VarChar, 128).Value = columnName;
            return Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture) == 1;
        }

        private static string GetColumnDataType(SqlConnection conn, string tableName, string columnName)
        {
            using SqlCommand cmd = new SqlCommand(@"SELECT DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME=@TableName AND COLUMN_NAME=@ColumnName", conn);
            cmd.Parameters.Add("@TableName", SqlDbType.VarChar, 128).Value = tableName;
            cmd.Parameters.Add("@ColumnName", SqlDbType.VarChar, 128).Value = columnName;
            return Convert.ToString(cmd.ExecuteScalar())?.Trim().ToLowerInvariant() ?? string.Empty;
        }

        private static string? DetectFirstColumn(SqlConnection conn, string tableName, params string[] candidates)
        {
            foreach (string candidate in candidates)
            {
                if (TableColumnExists(conn, tableName, candidate))
                {
                    return candidate;
                }
            }

            return null;
        }

        private static string BuildTrangThaiExpression(string alias, bool hasTrangThai, string dataType)
        {
            if (!hasTrangThai)
            {
                return "N'Đã thanh toán'";
            }

            if (dataType == "bit")
            {
                return $"CASE WHEN ISNULL({alias}.TrangThai, 1) = 1 THEN N'Đã thanh toán' ELSE N'Đã hủy' END";
            }

            return $"CASE WHEN CAST({alias}.TrangThai AS NVARCHAR(50)) LIKE N'%hủy%' THEN N'Đã hủy' ELSE ISNULL(CAST({alias}.TrangThai AS NVARCHAR(50)), N'Đã thanh toán') END";
        }
    }
}
