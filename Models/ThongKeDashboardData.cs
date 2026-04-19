using System.Data;

namespace PBL3.Models
{
    internal sealed class ThongKeDashboardData
    {
        public decimal TotalRevenue { get; set; }
        public decimal TotalProfit { get; set; }
        public int TotalOrders { get; set; }
        public int NewCustomers { get; set; }
        public DataTable RevenueTable { get; set; } = new DataTable();
        public DataTable TopItemsTable { get; set; } = new DataTable();
        public DataTable OrdersByHourTable { get; set; } = new DataTable();
        public DataTable CategoryShareTable { get; set; } = new DataTable();
        public DataTable LowStockTable { get; set; } = new DataTable();
    }
}
