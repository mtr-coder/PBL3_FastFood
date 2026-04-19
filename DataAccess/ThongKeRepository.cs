using PBL3.DataBase;
using PBL3.Models;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace PBL3.DataAccess
{
    internal sealed class ThongKeRepository
    {
        public ThongKeDashboardData GetDashboardData(DateTime from, DateTime to)
        {
            ThongKeDashboardData data = new ThongKeDashboardData();
            bool isTodayRange = from.Date == to.Date;
            string activeHoaDonFilter = BuildActiveHoaDonBanCondition(null);
            string activeHoaDonFilterWithAlias = BuildActiveHoaDonBanCondition("h");

            using SqlConnection conn = DbHelper.GetConnection();
            conn.Open();
            using SqlCommand cmd = conn.CreateCommand();

            cmd.CommandText = $@"SELECT SUM(ISNULL(TongTien,0)) FROM dbo.HOA_DON_BAN WHERE NgayBan BETWEEN @from AND @to AND {activeHoaDonFilter}";
            cmd.Parameters.AddWithValue("@from", from);
            cmd.Parameters.AddWithValue("@to", to);
            object? tot = cmd.ExecuteScalar();
            decimal totalRevenue = tot == DBNull.Value || tot is null ? 0m : Convert.ToDecimal(tot, CultureInfo.InvariantCulture);

            cmd.Parameters.Clear();
            cmd.CommandText = $@"SELECT COUNT(1) FROM dbo.HOA_DON_BAN WHERE NgayBan BETWEEN @from AND @to AND {activeHoaDonFilter}";
            cmd.Parameters.AddWithValue("@from", from);
            cmd.Parameters.AddWithValue("@to", to);
            object? cnt = cmd.ExecuteScalar();
            data.TotalOrders = cnt == DBNull.Value || cnt is null ? 0 : Convert.ToInt32(cnt, CultureInfo.InvariantCulture);

            cmd.Parameters.Clear();
            string? dateCol = null;
            string[] candidates = new[] { "NgayTao", "NgayDK", "NgayDangKy", "NgayDangKyKH", "NgayDangKyTaiKhoan" };
            foreach (string c in candidates)
            {
                using SqlCommand chk = conn.CreateCommand();
                chk.CommandText = "SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'KHACH_HANG' AND COLUMN_NAME = @col";
                chk.Parameters.AddWithValue("@col", c);
                if (chk.ExecuteScalar() is not null)
                {
                    dateCol = c;
                    break;
                }
            }

            if (!string.IsNullOrEmpty(dateCol))
            {
                cmd.CommandText = $"SELECT COUNT(1) FROM dbo.KHACH_HANG WHERE [{dateCol}] BETWEEN @from AND @to";
            }
            else
            {
                cmd.CommandText = $@"SELECT COUNT(1)
FROM (
    SELECT h.MaKH, MIN(h.NgayBan) AS FirstBuyDate
    FROM dbo.HOA_DON_BAN h
    WHERE h.MaKH IS NOT NULL AND {activeHoaDonFilterWithAlias}
    GROUP BY h.MaKH
) x
WHERE x.FirstBuyDate BETWEEN @from AND @to";
            }
            cmd.Parameters.AddWithValue("@from", from);
            cmd.Parameters.AddWithValue("@to", to);
            object? nc = cmd.ExecuteScalar();
            data.NewCustomers = nc == DBNull.Value || nc is null ? 0 : Convert.ToInt32(nc, CultureInfo.InvariantCulture);

            cmd.Parameters.Clear();
            cmd.CommandText = isTodayRange
                ? $@"SELECT DATEPART(hour, NgayBan) AS Gio, SUM(ISNULL(TongTien,0)) AS DoanhThu FROM dbo.HOA_DON_BAN WHERE NgayBan BETWEEN @from AND @to AND {activeHoaDonFilter} GROUP BY DATEPART(hour, NgayBan) ORDER BY Gio"
                : $@"SELECT CONVERT(date, NgayBan) AS Ngay, SUM(ISNULL(TongTien,0)) AS DoanhThu FROM dbo.HOA_DON_BAN WHERE NgayBan BETWEEN @from AND @to AND {activeHoaDonFilter} GROUP BY CONVERT(date, NgayBan) ORDER BY Ngay";
            cmd.Parameters.AddWithValue("@from", from);
            cmd.Parameters.AddWithValue("@to", to);
            using (SqlDataAdapter daRev = new SqlDataAdapter(cmd))
            {
                daRev.Fill(data.RevenueTable);
            }

            cmd.Parameters.Clear();
            cmd.CommandText = $@"SELECT TOP 5 ma.MaMon, ma.TenMon, SUM(ct.SoLuong) AS SoLuong FROM dbo.CT_HOA_DON_BAN ct JOIN dbo.MON_BAN ma ON ct.MaMon = ma.MaMon JOIN dbo.HOA_DON_BAN h ON ct.MaHDB = h.MaHDB WHERE h.NgayBan BETWEEN @from AND @to AND {activeHoaDonFilterWithAlias} GROUP BY ma.MaMon, ma.TenMon ORDER BY SoLuong DESC";
            cmd.Parameters.AddWithValue("@from", from);
            cmd.Parameters.AddWithValue("@to", to);
            using (SqlDataAdapter daTop = new SqlDataAdapter(cmd))
            {
                daTop.Fill(data.TopItemsTable);
            }

            cmd.Parameters.Clear();
            cmd.CommandText = $@"SELECT DATEPART(hour, NgayBan) AS Gio, COUNT(1) AS SoDon FROM dbo.HOA_DON_BAN WHERE NgayBan BETWEEN @from AND @to AND {activeHoaDonFilter} GROUP BY DATEPART(hour, NgayBan) ORDER BY Gio";
            cmd.Parameters.AddWithValue("@from", from);
            cmd.Parameters.AddWithValue("@to", to);
            using (SqlDataAdapter daHour = new SqlDataAdapter(cmd))
            {
                daHour.Fill(data.OrdersByHourTable);
            }

            cmd.Parameters.Clear();
            string[] priceCandidates = new[] { "DonGia", "DonGiaBan", "Gia", "GiaBan", "DonGiaCT", "DonGiaBanLe" };
            string? priceCol = null;
            foreach (string c in priceCandidates)
            {
                using SqlCommand chk = conn.CreateCommand();
                chk.CommandText = "SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CT_HOA_DON_BAN' AND COLUMN_NAME = @col";
                chk.Parameters.AddWithValue("@col", c);
                if (chk.ExecuteScalar() is not null)
                {
                    priceCol = c;
                    break;
                }
            }

            cmd.CommandText = !string.IsNullOrEmpty(priceCol)
                ? $@"SELECT lm.TenLoai AS Nhom, SUM(ct.SoLuong * ISNULL(ct.[{priceCol}],0)) AS DoanhThu FROM dbo.CT_HOA_DON_BAN ct JOIN dbo.MON_BAN ma ON ct.MaMon = ma.MaMon JOIN dbo.LOAI_MON lm ON ma.MaLoai = lm.MaLoai JOIN dbo.HOA_DON_BAN h ON ct.MaHDB = h.MaHDB WHERE h.NgayBan BETWEEN @from AND @to AND {activeHoaDonFilterWithAlias} GROUP BY lm.TenLoai ORDER BY DoanhThu DESC"
                : $@"SELECT lm.TenLoai AS Nhom,
       SUM( (h.TongTien * CAST(ct.SoLuong AS decimal(18,4)) / NULLIF(o.TotalQty,1)) ) AS DoanhThu
FROM dbo.CT_HOA_DON_BAN ct
JOIN dbo.MON_BAN ma ON ct.MaMon = ma.MaMon
JOIN dbo.LOAI_MON lm ON ma.MaLoai = lm.MaLoai
JOIN dbo.HOA_DON_BAN h ON ct.MaHDB = h.MaHDB
JOIN (
    SELECT MaHDB, SUM(SoLuong) AS TotalQty
    FROM dbo.CT_HOA_DON_BAN
    GROUP BY MaHDB
) o ON ct.MaHDB = o.MaHDB
WHERE h.NgayBan BETWEEN @from AND @to
  AND {activeHoaDonFilterWithAlias}
GROUP BY lm.TenLoai
ORDER BY DoanhThu DESC";
            cmd.Parameters.AddWithValue("@from", from);
            cmd.Parameters.AddWithValue("@to", to);
            using (SqlDataAdapter daCat = new SqlDataAdapter(cmd))
            {
                daCat.Fill(data.CategoryShareTable);
            }

            cmd.Parameters.Clear();
            cmd.CommandText = "SELECT MaNL, TenNL, SoLuongTon, NguongToiThieu FROM dbo.NGUYEN_LIEU WHERE SoLuongTon <= NguongToiThieu";
            using (SqlDataAdapter daLow = new SqlDataAdapter(cmd))
            {
                daLow.Fill(data.LowStockTable);
            }

            cmd.Parameters.Clear();
            string revenueFromSoldExpr = ResolveRevenueFromSoldExpression(conn);
            cmd.CommandText = $@"
WITH CostPerMon AS (
    SELECT dm.MaMon, SUM(dm.SoLuongSuDung * ISNULL(nl.GiaNhap,0)) AS CostPerMon
    FROM dbo.DINH_MUC_MON dm
    JOIN dbo.NGUYEN_LIEU nl ON dm.MaNL = nl.MaNL
    GROUP BY dm.MaMon
)
SELECT
    SUM({revenueFromSoldExpr}) AS RevFromSold,
    SUM(ct.SoLuong * ISNULL(c.CostPerMon,0)) AS CostFromSold
FROM dbo.CT_HOA_DON_BAN ct
JOIN dbo.HOA_DON_BAN h ON ct.MaHDB = h.MaHDB
JOIN dbo.MON_BAN ma ON ct.MaMon = ma.MaMon
LEFT JOIN CostPerMon c ON ct.MaMon = c.MaMon
WHERE h.NgayBan BETWEEN @from AND @to
  AND {activeHoaDonFilterWithAlias}";
            cmd.Parameters.AddWithValue("@from", from);
            cmd.Parameters.AddWithValue("@to", to);

            decimal totalCost = 0m;
            decimal revenueForProfit = totalRevenue;
            using (SqlDataReader pr = cmd.ExecuteReader())
            {
                if (pr.Read())
                {
                    revenueForProfit = pr.IsDBNull(0) ? 0m : Convert.ToDecimal(pr.GetValue(0), CultureInfo.InvariantCulture);
                    totalCost = pr.IsDBNull(1) ? 0m : Convert.ToDecimal(pr.GetValue(1), CultureInfo.InvariantCulture);
                }
            }

            if (revenueForProfit <= 0m && totalRevenue > 0m)
            {
                revenueForProfit = totalRevenue;
            }

            if (totalRevenue == 0m && revenueForProfit > 0m)
            {
                totalRevenue = revenueForProfit;
            }

            data.TotalRevenue = totalRevenue;
            data.TotalProfit = revenueForProfit - totalCost;
            return data;
        }

        private static string ResolveRevenueFromSoldExpression(SqlConnection conn)
        {
            string[] ctPriceCandidates = { "DonGia", "DonGiaBan", "Gia", "GiaBan", "DonGiaCT", "DonGiaBanLe", "ThanhTien" };
            foreach (string c in ctPriceCandidates)
            {
                if (ColumnExists(conn, "CT_HOA_DON_BAN", c))
                {
                    if (string.Equals(c, "ThanhTien", StringComparison.OrdinalIgnoreCase))
                    {
                        return "ISNULL(ct.[ThanhTien],0)";
                    }

                    return $"(ISNULL(ct.SoLuong,0) * ISNULL(ct.[{c}],0))";
                }
            }

            string[] monPriceCandidates = { "DonGia", "DonGiaBan", "Gia", "GiaBan", "GiaTien", "Price" };
            foreach (string c in monPriceCandidates)
            {
                if (ColumnExists(conn, "MON_BAN", c))
                {
                    return $"(ISNULL(ct.SoLuong,0) * ISNULL(ma.[{c}],0))";
                }
            }

            return "0";
        }

        private static bool ColumnExists(SqlConnection conn, string tableName, string columnName)
        {
            using SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @table AND COLUMN_NAME = @col";
            cmd.Parameters.AddWithValue("@table", tableName);
            cmd.Parameters.AddWithValue("@col", columnName);
            return cmd.ExecuteScalar() is not null;
        }

        private static string GetColumnDataType(SqlConnection conn, string tableName, string columnName)
        {
            using SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @table AND COLUMN_NAME = @col";
            cmd.Parameters.AddWithValue("@table", tableName);
            cmd.Parameters.AddWithValue("@col", columnName);
            return Convert.ToString(cmd.ExecuteScalar())?.Trim().ToLowerInvariant() ?? string.Empty;
        }

        private static string BuildActiveHoaDonBanCondition(string? alias)
        {
            using SqlConnection conn = DbHelper.GetConnection();
            conn.Open();

            if (!ColumnExists(conn, "HOA_DON_BAN", "TrangThai"))
            {
                return "1 = 1";
            }

            string dataType = GetColumnDataType(conn, "HOA_DON_BAN", "TrangThai");
            string prefix = string.IsNullOrWhiteSpace(alias) ? string.Empty : $"{alias}.";

            if (dataType == "bit")
            {
                return $"ISNULL({prefix}TrangThai, 1) = 1";
            }

            return $"CAST({prefix}TrangThai AS NVARCHAR(50)) NOT LIKE N'%hủy%'";
        }
    }
}
