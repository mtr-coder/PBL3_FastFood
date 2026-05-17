using System.Windows.Forms;
using System;
using System.Data;
using System.Globalization;
using PBL3.Business;
using System.IO;
using System.Windows.Forms.DataVisualization.Charting;

namespace PBL3
{
    public partial class ThongKe : Form
    {
        private readonly ThongKeService _thongKeService;
        private Chart? _chartRevenue;
        private Chart? _chartOrdersByHour;
        private Chart? _chartCategoryShare;

        public ThongKe()
        {
            _thongKeService = new ThongKeService();
            InitializeComponent();
            WireActionButtons();
            cboDateFilter.SelectedIndex = 2;
            // start with custom date pickers disabled
            dtpFrom.Enabled = false;
            dtpTo.Enabled = false;
            InitializeCharts();
            ApplyDashboardStyles();
            this.Load += ThongKe_Load;
        }

        private void WireActionButtons()
        {
            btnRefresh.Click += btnRefresh_Click;
            label1.Click += btnRefresh_Click;

            roundedPanel2.Click += btnExportCsv_Click;
            label2.Click += btnExportCsv_Click;
        }

        private void InitializeCharts()
        {
            _chartRevenue = new Chart
            {
                Location = dgvRevenue.Location,
                Size = dgvRevenue.Size,
                BackColor = Color.White,
                BorderlineWidth = 0
            };
            _chartRevenue.ChartAreas.Clear();
            _chartRevenue.Series.Clear();
            _chartRevenue.ChartAreas.Add(new ChartArea("RevenueArea"));
            _chartRevenue.Series.Add(new Series("DoanhThu") { ChartType = SeriesChartType.Line, BorderWidth = 3, XValueType = ChartValueType.Date });

            _chartOrdersByHour = new Chart
            {
                Location = dgvOrdersByHour.Location,
                Size = dgvOrdersByHour.Size,
                BackColor = Color.White,
                BorderlineWidth = 0
            };
            _chartOrdersByHour.ChartAreas.Clear();
            _chartOrdersByHour.Series.Clear();
            _chartOrdersByHour.ChartAreas.Add(new ChartArea("HourArea"));
            _chartOrdersByHour.Series.Add(new Series("SoDon") { ChartType = SeriesChartType.Column, BorderWidth = 2 });

            _chartCategoryShare = new Chart
            {
                Location = dgvCategoryShare.Location,
                Size = dgvCategoryShare.Size,
                BackColor = Color.White,
                BorderlineWidth = 0
            };
            _chartCategoryShare.ChartAreas.Clear();
            _chartCategoryShare.Series.Clear();
            _chartCategoryShare.ChartAreas.Add(new ChartArea("CategoryArea"));
            _chartCategoryShare.Series.Add(new Series("Nhom") { ChartType = SeriesChartType.Pie, BorderWidth = 1 });

            hcnt_Khung.Controls.Add(_chartRevenue);
            hcnt_Khung.Controls.Add(_chartOrdersByHour);
            hcnt_Khung.Controls.Add(_chartCategoryShare);
            _chartRevenue.BringToFront();
            _chartOrdersByHour.BringToFront();
            _chartCategoryShare.BringToFront();

            // Remove designer placeholders khỏi giao diện runtime, chỉ giữ để map vị trí trong code
            hcnt_Khung.Controls.Remove(dgvRevenue);
            hcnt_Khung.Controls.Remove(dgvOrdersByHour);
            hcnt_Khung.Controls.Remove(dgvCategoryShare);
        }

        private void ApplyDashboardStyles()
        {
            lblTotalRevenue.AutoSize = false;
            lblTotalProfit.AutoSize = false;
            lblTotalOrders.AutoSize = false;
            lblTotalRevenue.ForeColor = Color.Black;
            lblTotalProfit.ForeColor = Color.Black;
            lblTotalOrders.ForeColor = Color.Black;

            dgvTopItems.BackgroundColor = Color.White;
            dgvTopItems.BorderStyle = BorderStyle.None;
            dgvCategoryShare.BackgroundColor = Color.White;
            dgvCategoryShare.BorderStyle = BorderStyle.None;
            dgvLowStock.BackgroundColor = Color.White;
            dgvLowStock.BorderStyle = BorderStyle.None;
            dgvRevenue.RowHeadersVisible = false;
            dgvOrdersByHour.RowHeadersVisible = false;
            dgvCategoryShare.RowHeadersVisible = false;
            dgvTopItems.RowHeadersVisible = false;
            dgvLowStock.RowHeadersVisible = false;

            label1.Text = "Làm mới";
            label2.Text = "Xuất CSV";

            // always keep summary labels visible above other controls
            lblTotalRevenue.Visible = lblTotalProfit.Visible = lblTotalOrders.Visible = true;
            lblTotalRevenue.BringToFront();
            lblTotalProfit.BringToFront();
            lblTotalOrders.BringToFront();
        }

        private void ThongKe_Load(object? sender, EventArgs e)
        {
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
                return;
            LoadDashboard();
        }
        private void cboDateFilter_SelectedIndexChanged(object? sender, EventArgs e)
        {
            // enable/disable custom date pickers (robust to encoding/culture differences)
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                dtpFrom.Enabled = dtpTo.Enabled = false;
                return;
            }

            bool custom = false;
            try
            {
                if (cboDateFilter.SelectedIndex >= 0 && cboDateFilter.SelectedIndex == 4)
                {
                    custom = true;
                }
                else
                {
                    var s = (cboDateFilter.SelectedItem?.ToString() ?? string.Empty).ToLowerInvariant();
                    if (s.Contains("tùy") || s.Contains("tuy") || s.Contains("tùy chỉnh") || s.Contains("tuy chinh"))
                    {
                        custom = true;
                    }
                }
            }
            catch
            {
                custom = false;
            }

            dtpFrom.Enabled = dtpTo.Enabled = custom;
        }

        private void btnRefresh_Click(object? sender, EventArgs e)
        {
            if (dtpFrom.Value.Date > dtpTo.Value.Date)
            {
                MessageBox.Show("Ngày bắt đầu không được lớn hơn ngày kết thúc.", "Dữ liệu sai", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                dtpTo.Value = dtpFrom.Value.Date;
            }

            LoadDashboard();
        }

        private void btnExportCsv_Click(object? sender, EventArgs e)
        {
            try
            {
                string fn = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"ThongKe_{DateTime.Now:yyyyMMddHHmm}.csv");
                using StreamWriter sw = new StreamWriter(fn, false, new System.Text.UTF8Encoding(true));
                sw.WriteLine("Chỉ số,Giá trị");
                sw.WriteLine($"Từ ngày,{dtpFrom.Value:dd/MM/yyyy}");
                sw.WriteLine($"Đến ngày,{dtpTo.Value:dd/MM/yyyy}");
                sw.WriteLine($"Tổng doanh thu,{ExtractNumberFromLabel(lblTotalRevenue.Text)}");
                sw.WriteLine($"Tổng lợi nhuận,{ExtractNumberFromLabel(lblTotalProfit.Text)}");
                sw.WriteLine($"Tổng số đơn,{ExtractNumberFromLabel(lblTotalOrders.Text)}");
                MessageBox.Show($"Xuất CSV thành công: {fn}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"L?i xu?t CSV: {ex.Message}", "L?i", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static string ExtractNumberFromLabel(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return "0";
            }

            var match = System.Text.RegularExpressions.Regex.Match(text, @"-?[\d\.,]+");
            if (!match.Success)
            {
                return "0";
            }

            string raw = match.Value.Replace(".", string.Empty).Replace(",", string.Empty);
            return long.TryParse(raw, out long n) ? n.ToString(CultureInfo.InvariantCulture) : "0";
        }

        private void LoadDashboard()
        {
            // compute date range by SelectedIndex to avoid text/encoding mismatch
            DateTime from = DateTime.Today;
            DateTime to = DateTime.Today.AddDays(1).AddTicks(-1);

            switch (cboDateFilter.SelectedIndex)
            {
                case 1: // Hôm qua
                    from = DateTime.Today.AddDays(-1);
                    to = DateTime.Today.AddTicks(-1);
                    break;
                case 2: // 7 ngày qua (bao gồm hôm nay)
                    from = DateTime.Today.AddDays(-6);
                    to = DateTime.Today.AddDays(1).AddTicks(-1);
                    break;
                case 3: // Tháng này
                    from = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                    to = DateTime.Today.AddDays(1).AddTicks(-1);
                    break;
                case 4: // Tùy chỉnh
                    from = dtpFrom.Value.Date;
                    to = dtpTo.Value.Date.AddDays(1).AddTicks(-1);
                    break;
                default: // Hôm nay
                    from = DateTime.Today;
                    to = DateTime.Today.AddDays(1).AddTicks(-1);
                    break;
            }

            try
            {
                var dashboard = _thongKeService.GetDashboardData(from, to);

                try
                {
                    lblDateRange.Text = $"Khoảng: {from:dd/MM/yyyy} - {to:dd/MM/yyyy HH:mm:ss}";
                }
                catch { }

                lblTotalRevenue.Text = $"Tổng doanh thu: {dashboard.TotalRevenue:N0} đ";
                lblTotalOrders.Text = $"Tổng số đơn: {dashboard.TotalOrders}";
                lblTotalProfit.Text = $"Tổng lợi nhuận: {dashboard.TotalProfit:N0} đ";

                dgvRevenue.DataSource = dashboard.RevenueTable;
                if (dgvRevenue.Columns.Contains("DoanhThu"))
                {
                    dgvRevenue.Columns["DoanhThu"].DefaultCellStyle.Format = "N0";
                    dgvRevenue.Columns["DoanhThu"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
                BindRevenueChart(dashboard.RevenueTable, from, to);

                dgvTopItems.DataSource = dashboard.TopItemsTable;
                if (dgvTopItems.Columns.Contains("MaMon"))
                    dgvTopItems.Columns["MaMon"].Visible = false;
                if (dgvTopItems.Columns.Contains("SoLuong"))
                {
                    dgvTopItems.Columns["SoLuong"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }

                dgvOrdersByHour.DataSource = dashboard.OrdersByHourTable;
                if (dgvOrdersByHour.Columns.Contains("SoDon"))
                {
                    dgvOrdersByHour.Columns["SoDon"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
                BindOrdersByHourChart(dashboard.OrdersByHourTable);

                dgvCategoryShare.DataSource = dashboard.CategoryShareTable;
                BindCategoryChart(dashboard.CategoryShareTable);
                if (dgvCategoryShare.Columns.Contains("DoanhThu"))
                {
                    dgvCategoryShare.Columns["DoanhThu"].DefaultCellStyle.Format = "N0";
                    dgvCategoryShare.Columns["DoanhThu"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }

                dgvLowStock.DataSource = dashboard.LowStockTable;
                if (dgvLowStock.Columns.Contains("MaNL"))
                    dgvLowStock.Columns["MaNL"].Visible = false;
                if (dgvLowStock.Columns.Contains("SoLuongTon"))
                    dgvLowStock.Columns["SoLuongTon"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                if (dgvLowStock.Columns.Contains("NguongToiThieu"))
                    dgvLowStock.Columns["NguongToiThieu"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                ConfigureLowStockGrid();
                ConfigureTopItemsGrid();
                HighlightLowStockRows();
            }
            catch (Exception ex)
            {
                lblTotalRevenue.Text = "Tổng doanh thu: 0 đ";
                lblTotalProfit.Text = "Tổng lợi nhuận: 0 đ";
                MessageBox.Show($"L?i t?i d? li?u th?ng kê: {ex.Message}", "L?i", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_QLNV_Click(object? sender, EventArgs e)
        {
            AdminNavigationManager.Navigate<QuanLiNhanVien>(this);
        }

        private void btn_QLNCC_Click(object? sender, EventArgs e)
        {
            AdminNavigationManager.Navigate<QuanLiNhaCungCap>(this);
        }

        private void btn_QLKH_Click(object? sender, EventArgs e)
        {
            AdminNavigationManager.Navigate<QuanLiKhachHang>(this);
        }

        private void btn_QLMA_Click(object? sender, EventArgs e)
        {
            AdminNavigationManager.Navigate<QuanLiMonAn>(this);
        }

        private void btn_QLHDN_Click(object? sender, EventArgs e)
        {
            AdminNavigationManager.Navigate<QuanLiNguyenLieu>(this);
        }

        private void btn_QLHDB_Click(object? sender, EventArgs e)
        {
            AdminNavigationManager.Navigate<LichSuHoaDon>(this);
        }

        private void btn_DangXuat_MouseEnter(object? sender, EventArgs e)
        {
            btn_DangXuat.BackColor = Color.FromArgb(255, 69, 0);
        }

        private void btn_DangXuat_MouseLeave(object? sender, EventArgs e)
        {
            btn_DangXuat.BackColor = Color.LightSalmon;
        }

        private void btn_DangXuat_Click(object? sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Bạn có chắc chắn muốn đăng xuất?",
                "Xác nhận đăng xuất",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                AdminNavigationManager.Logout(this);
            }
        }

        private void btn_ThongKe_Click(object? sender, EventArgs e)
        {

        }

        private void HighlightLowStockRows()
        {
            foreach (DataGridViewRow row in dgvLowStock.Rows)
            {
                if (row.IsNewRow) continue;
                decimal ton = 0m;
                decimal nguong = 0m;
                if (row.Cells["SoLuongTon"]?.Value != null && row.Cells["SoLuongTon"].Value != DBNull.Value)
                    ton = Convert.ToDecimal(row.Cells["SoLuongTon"].Value, CultureInfo.InvariantCulture);
                if (row.Cells["NguongToiThieu"]?.Value != null && row.Cells["NguongToiThieu"].Value != DBNull.Value)
                    nguong = Convert.ToDecimal(row.Cells["NguongToiThieu"].Value, CultureInfo.InvariantCulture);
                if (ton <= nguong)
                {
                    row.DefaultCellStyle.BackColor = Color.LightYellow;
                    row.DefaultCellStyle.ForeColor = Color.DarkRed;
                }
            }
        }

        private void BindRevenueChart(DataTable dt, DateTime from, DateTime to)
        {
            if (_chartRevenue == null) return;
            var series = _chartRevenue.Series[0];
            series.Points.Clear();
            bool byHour = dt.Columns.Contains("Gio");
            series.XValueType = byHour ? ChartValueType.Int32 : ChartValueType.String;
            if (byHour)
            {
                foreach (DataRow row in dt.Rows)
                {
                    decimal revenue = row["DoanhThu"] == DBNull.Value ? 0m : Convert.ToDecimal(row["DoanhThu"], CultureInfo.InvariantCulture);
                    int hour = row["Gio"] == DBNull.Value ? 0 : Convert.ToInt32(row["Gio"]);
                    series.Points.AddXY(hour, revenue);
                }
            }
            else
            {
                Dictionary<DateTime, decimal> revenueByDate = new Dictionary<DateTime, decimal>();
                foreach (DataRow row in dt.Rows)
                {
                    DateTime day = Convert.ToDateTime(row["Ngay"]).Date;
                    decimal revenue = row["DoanhThu"] == DBNull.Value ? 0m : Convert.ToDecimal(row["DoanhThu"], CultureInfo.InvariantCulture);
                    revenueByDate[day] = revenue;
                }

                DateTime current = from.Date;
                DateTime end = to.Date;
                while (current <= end)
                {
                    revenueByDate.TryGetValue(current, out decimal revenue);
                    series.Points.AddXY(current.Day.ToString(CultureInfo.InvariantCulture), revenue);
                    current = current.AddDays(1);
                }
            }
            _chartRevenue.ChartAreas[0].AxisX.Interval = 1;
            _chartRevenue.ChartAreas[0].AxisX.LabelStyle.Format = byHour ? "0h" : string.Empty;
            _chartRevenue.ChartAreas[0].AxisY.LabelStyle.Format = "#,##0 đ";
            _chartRevenue.ChartAreas[0].AxisY.Minimum = 0;
        }

        private void BindOrdersByHourChart(DataTable dt)
        {
            if (_chartOrdersByHour == null) return;
            var series = _chartOrdersByHour.Series[0];
            series.Points.Clear();
            Dictionary<int, int> countByHour = new Dictionary<int, int>();
            foreach (DataRow row in dt.Rows)
            {
                int hour = row["Gio"] == DBNull.Value ? 0 : Convert.ToInt32(row["Gio"]);
                int count = row["SoDon"] == DBNull.Value ? 0 : Convert.ToInt32(row["SoDon"]);
                countByHour[hour] = count;
            }
            for (int hour = 0; hour <= 23; hour++)
            {
                countByHour.TryGetValue(hour, out int count);
                series.Points.AddXY(hour, count);
            }
            _chartOrdersByHour.ChartAreas[0].AxisX.Interval = 1;
            _chartOrdersByHour.ChartAreas[0].AxisX.Minimum = 0;
            _chartOrdersByHour.ChartAreas[0].AxisX.Maximum = 23;
            _chartOrdersByHour.ChartAreas[0].AxisY.Interval = 1;
            _chartOrdersByHour.ChartAreas[0].AxisY.Minimum = 0;
            _chartOrdersByHour.ChartAreas[0].AxisY.LabelStyle.Format = "N0";
        }

        private void BindCategoryChart(DataTable dt)
        {
            if (_chartCategoryShare == null) return;
            var series = _chartCategoryShare.Series[0];
            series.Points.Clear();
            decimal total = 0m;
            foreach (DataRow row in dt.Rows)
            {
                string nhom = Convert.ToString(row["Nhom"]) ?? "Khác";
                decimal doanhThu = row["DoanhThu"] == DBNull.Value ? 0m : Convert.ToDecimal(row["DoanhThu"], CultureInfo.InvariantCulture);
                if (doanhThu > 0)
                {
                    int idx = series.Points.AddXY(nhom, doanhThu);
                    series.Points[idx].LegendText = nhom;
                    series.Points[idx].Label = "#PERCENT{P0}";

                    // thêm nhãn giá trị trực tiếp lên cột cho biểu đồ cột
                    if (series.ChartType == SeriesChartType.Column)
                    {
                        series.Points[idx].Label = $"{doanhThu:N0} đ";
                    }
                    total += doanhThu;
                }
            }

            // tránh vùng trắng khi tất cả doanh thu nhóm = 0
            if (series.Points.Count == 0 || total <= 0)
            {
                int idx = series.Points.AddXY("Không có", 1);
                series.Points[idx].LegendText = "Không có";
            }

            _chartCategoryShare.Legends.Clear();
            _chartCategoryShare.Legends.Add(new Legend("Legend1"));
            _chartCategoryShare.Titles.Clear();
            _chartCategoryShare.Titles.Add("Tỉ trọng doanh thu theo nhóm");
        }

        private void lblHourTitle_Click(object sender, EventArgs e)
        {

        }

        private void ConfigureTopItemsGrid()
        {
            if (dgvTopItems.Columns.Contains("MaMon"))
                dgvTopItems.Columns["MaMon"].Visible = false;
            if (dgvTopItems.Columns.Contains("TenMon"))
                dgvTopItems.Columns["TenMon"].HeaderText = "Tên món";
            if (dgvTopItems.Columns.Contains("SoLuong"))
                dgvTopItems.Columns["SoLuong"].HeaderText = "Số lượng";
        }

        private void ConfigureLowStockGrid()
        {
            if (dgvLowStock.Columns.Contains("TenNL"))
                dgvLowStock.Columns["TenNL"].HeaderText = "Tên nguyên liệu";
            if (dgvLowStock.Columns.Contains("SoLuongTon"))
                dgvLowStock.Columns["SoLuongTon"].HeaderText = "Số lượng tồn";
            if (dgvLowStock.Columns.Contains("NguongToiThieu"))
                dgvLowStock.Columns["NguongToiThieu"].HeaderText = "Ngưỡng tối thiểu";
        }
    }
}
