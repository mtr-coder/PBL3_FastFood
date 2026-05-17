using PBL3.Business;
using PBL3.Models;
using System;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

namespace PBL3
{
    public partial class LichSuHoaDon : Form
    {
        private readonly LichSuHoaDonService _lichSuHoaDonService;
        private LichSuHoaDonSchemaInfo _schemaInfo = new LichSuHoaDonSchemaInfo();
        private sealed class FilterOption
        {
            public string Display { get; init; } = string.Empty;
            public string FieldName { get; init; } = string.Empty;
            public string Value { get; init; } = string.Empty;
            public override string ToString() => Display;
        }

        private readonly bool _isAdmin;
        private readonly string? _maNvDangNhap;
        private readonly List<FilterOption> _filterOptions = new List<FilterOption>();
        private DataTable? _masterTable;
        private string _currentInvoiceType = "BAN";
        private string? _selectedMaHoaDon;

        private bool _hasTrangThaiHdb;
        private bool _hasTrangThaiHdn;
        private string _trangThaiHdbType = string.Empty;
        private string _trangThaiHdnType = string.Empty;

        private string _printContent = string.Empty;
        private Image? _printQrImage;
        private string _printMaHd = string.Empty;
        private string _printNhanVien = string.Empty;
        private string _printDoiTac = string.Empty;
        private DateTime _printThoiGian;
        private decimal _printTongTien;
        private string _printTenHang = string.Empty;
        private int _printPhanTramGiam;
        private int _printDiemTichLuy;
        private decimal _printTienHangGoc;
        private string _printSdtNcc = string.Empty;


        public LichSuHoaDon() : this(true, null)
        {
        }

        public LichSuHoaDon(bool isAdmin, string? maNvDangNhap)
        {
            _lichSuHoaDonService = new LichSuHoaDonService();
            _isAdmin = isAdmin;
            _maNvDangNhap = maNvDangNhap;
            InitializeComponent();

            _btnHoaDonBan.Click += (_, __) => SwitchInvoiceType("BAN");
            _btnHoaDonNhap.Click += (_, __) => SwitchInvoiceType("NHAP");

            _txtTimMaHD.TextChanged += (_, __) => ApplySearchFilter();
            _dtpTuNgay.ValueChanged += FilterTime_Changed;
            _dtpDenNgay.ValueChanged += FilterTime_Changed;

            _dgvHoaDonMaster.CellClick += DgvHoaDonMaster_CellClick;

            _btnLamMoi.Click += BtnLamMoi_Click;
            _btnHuyHoaDon.Click += BtnHuyHoaDon_Click;
            _btnXuatBaoCao.Click += BtnXuatBaoCao_Click;
            _btnInLai.Click += BtnInLai_Click;

            btn_QLNCC.Click += btn_QLNCC_Click;
            btn_QLKH.Click += btn_QLKH_Click;
            btn_QLNV.Click += btn_QLNV_Click;
            btn_QLMA.Click += btn_QLMA_Click;
            btn_QLHDN.Click += btn_QLHDN_Click;
            btn_ThongKe.Click += btn_ThongKe_Click;
            btn_DangXuat.Click += btn_DangXuat_Click;
            btn_LSHD.Click += (_, __) => { };

            // Layout is now managed in Designer to match runtime with Design view.
        }

        private void LichSuHoaDon_Load(object? sender, EventArgs e)
        {
            try
            {
                DetectSchema();
                InitializeDefaultFilters();
                _btnHuyHoaDon.Visible = _isAdmin;
                SwitchInvoiceType("BAN");
                LoadTopMetrics();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể tải trang lịch sử hóa đơn.\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DetectSchema()
        {
            _schemaInfo = _lichSuHoaDonService.DetectSchema();
            _hasTrangThaiHdb = _schemaInfo.HasTrangThaiHdb;
            _hasTrangThaiHdn = _schemaInfo.HasTrangThaiHdn;
            _trangThaiHdbType = _schemaInfo.TrangThaiHdbType;
            _trangThaiHdnType = _schemaInfo.TrangThaiHdnType;
        }

        private void InitializeDefaultFilters()
        {
            var range = _lichSuHoaDonService.GetDefaultDateRange(_isAdmin);
            _dtpTuNgay.Value = range.TuNgay;
            _dtpDenNgay.Value = range.DenNgay;
        }

        private static string EscapeForRowFilter(string input)
        {
            return input.Replace("'", "''").Replace("[", "[[]").Replace("%", "[%]").Replace("*", "[*]");
        }

        private static decimal ToDecimalOrZero(object? value)
        {
            return value is null || value == DBNull.Value
                ? 0m
                : Convert.ToDecimal(value, CultureInfo.InvariantCulture);
        }

        private static int ToIntOrZero(object? value)
        {
            return value is null || value == DBNull.Value
                ? 0
                : Convert.ToInt32(value, CultureInfo.InvariantCulture);
        }

        private void SwitchInvoiceType(string type)
        {
            _currentInvoiceType = type;
            _selectedMaHoaDon = null;

            _btnHoaDonBan.BackColor = type == "BAN" ? Color.Salmon : Color.Bisque;
            _btnHoaDonNhap.BackColor = type == "NHAP" ? Color.Salmon : Color.Bisque;

            LoadFilterOptions();
            LoadMasterData();
            LoadTopMetrics();
            UpdateMetricCardsByType();
        }

        private void ApplyCompactLayout()
        {
            // Designer-managed layout
        }

        private void UpdateMetricCardsByType()
        {
            pnlCard3.Visible = true;
        }

        private void FilterTime_Changed(object? sender, EventArgs e)
        {
            if (_dtpTuNgay.Value.Date > _dtpDenNgay.Value.Date)
            {
                _dtpDenNgay.Value = _dtpTuNgay.Value.Date;
            }

            LoadMasterData();
            LoadTopMetrics();
        }

        private void LoadFilterOptions()
        {
            _filterOptions.Clear();
            _filterOptions.Add(new FilterOption { Display = "Tất cả", FieldName = string.Empty, Value = string.Empty });

            _filterOptions.Add(new FilterOption { Display = "Mã HĐ", FieldName = "MaHD", Value = string.Empty });
            _filterOptions.Add(new FilterOption { Display = "Thời gian", FieldName = "ThoiGian", Value = string.Empty });
            _filterOptions.Add(new FilterOption { Display = "Tổng tiền", FieldName = "TongTien", Value = string.Empty });
        }

        private string BuildTrangThaiExpression(string alias, bool hasTrangThai, string dataType)
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

        private void LoadMasterData()
        {
            _masterTable = _lichSuHoaDonService.GetMasterData(
                _currentInvoiceType,
                _dtpTuNgay.Value.Date,
                _dtpDenNgay.Value.Date,
                _isAdmin,
                _maNvDangNhap,
                _schemaInfo);
            _dgvHoaDonMaster.DataSource = _masterTable;
            ConfigureMasterGrid();
            ApplySearchFilter();
        }

        private void ConfigureMasterGrid()
        {
            _dgvHoaDonMaster.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            _dgvHoaDonMaster.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
            _dgvHoaDonMaster.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;

            SetHeaderText(_dgvHoaDonMaster, "MaHD", "Mã HĐ");
            SetHeaderText(_dgvHoaDonMaster, "ThoiGian", "Thời gian");
            SetHeaderText(_dgvHoaDonMaster, "NguoiThucHien", "Người thực hiện");
            SetHeaderText(_dgvHoaDonMaster, "DoiTac", _currentInvoiceType == "BAN" ? "Khách hàng" : "Nhà cung cấp");
            SetHeaderText(_dgvHoaDonMaster, "TongTien", "Tổng tiền");
            SetHeaderText(_dgvHoaDonMaster, "TrangThai", "Trạng thái");

            SetColumnWidth(_dgvHoaDonMaster, "MaHD", 85);
            SetColumnWidth(_dgvHoaDonMaster, "ThoiGian", 100);
            SetColumnWidth(_dgvHoaDonMaster, "NguoiThucHien", 150);
            SetColumnWidth(_dgvHoaDonMaster, "DoiTac", 170);
            SetColumnWidth(_dgvHoaDonMaster, "TongTien", 120);
            SetColumnWidth(_dgvHoaDonMaster, "TrangThai", 110);

            if (_dgvHoaDonMaster.Columns.Contains("ThoiGian"))
            {
                _dgvHoaDonMaster.Columns["ThoiGian"].DefaultCellStyle.Format = "HH:mm - dd/MM";
            }

            if (_dgvHoaDonMaster.Columns.Contains("TongTien"))
            {
                _dgvHoaDonMaster.Columns["TongTien"].DefaultCellStyle.Format = "N0";
                _dgvHoaDonMaster.Columns["TongTien"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                _dgvHoaDonMaster.Columns["TongTien"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            }

            HideColumn(_dgvHoaDonMaster, "SDTKhach");
            HideColumn(_dgvHoaDonMaster, "MaNV");
            HideColumn(_dgvHoaDonMaster, "MaDoiTac");
            HideColumn(_dgvHoaDonMaster, "NguoiThucHien");
            HideColumn(_dgvHoaDonMaster, "DoiTac");
            HideColumn(_dgvHoaDonMaster, "TrangThai");
            HideColumn(_dgvHoaDonMaster, "TenHang");
            HideColumn(_dgvHoaDonMaster, "PhanTramGiam");
            HideColumn(_dgvHoaDonMaster, "DiemTichLuy");
            HideColumn(_dgvHoaDonMaster, "SDT");
        }

        private void ConfigureDetailGrid()
        {
            _dgvHoaDonDetail.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            _dgvHoaDonDetail.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
            _dgvHoaDonDetail.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;

            SetHeaderText(_dgvHoaDonDetail, "TenHang", _currentInvoiceType == "BAN" ? "Tên món" : "Tên nguyên liệu");
            SetHeaderText(_dgvHoaDonDetail, "SoLuong", "SL");
            SetHeaderText(_dgvHoaDonDetail, "DonGia", "Đơn giá");
            SetHeaderText(_dgvHoaDonDetail, "ThanhTien", "Thành tiền");
            SetHeaderText(_dgvHoaDonDetail, "GhiChu", "Ghi chú");

            SetColumnWidth(_dgvHoaDonDetail, "TenHang", 180);
            SetColumnWidth(_dgvHoaDonDetail, "SoLuong", 40);
            SetColumnWidth(_dgvHoaDonDetail, "DonGia", 100);
            SetColumnWidth(_dgvHoaDonDetail, "ThanhTien", 110);
            SetColumnWidth(_dgvHoaDonDetail, "GhiChu", 140);

            HideColumn(_dgvHoaDonDetail, "GhiChu");

            if (_dgvHoaDonDetail.Columns.Contains("SoLuong"))
            {
                _dgvHoaDonDetail.Columns["SoLuong"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }

            if (_dgvHoaDonDetail.Columns.Contains("DonGia"))
            {
                _dgvHoaDonDetail.Columns["DonGia"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                _dgvHoaDonDetail.Columns["DonGia"].DefaultCellStyle.Format = "N0";
            }

            if (_dgvHoaDonDetail.Columns.Contains("ThanhTien"))
            {
                _dgvHoaDonDetail.Columns["ThanhTien"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                _dgvHoaDonDetail.Columns["ThanhTien"].DefaultCellStyle.Format = "N0";
            }
        }

        private static void HideColumn(DataGridView dgv, string name)
        {
            if (dgv.Columns.Contains(name))
            {
                dgv.Columns[name].Visible = false;
            }
        }

        private static void SetHeaderText(DataGridView dgv, string columnName, string header)
        {
            if (dgv.Columns.Contains(columnName))
            {
                dgv.Columns[columnName].HeaderText = header;
            }
        }

        private static void SetColumnWidth(DataGridView dgv, string columnName, int width)
        {
            if (dgv.Columns.Contains(columnName))
            {
                dgv.Columns[columnName].Width = width;
            }
        }

        private void ApplySearchFilter()
        {
            if (_masterTable is null)
            {
                return;
            }

            string keyword = EscapeForRowFilter(_txtTimMaHD.Text.Trim());

            if (string.IsNullOrWhiteSpace(keyword))
            {
                _masterTable.DefaultView.RowFilter = string.Empty;
                UpdateDetailSelectionAfterFilter();
                return;
            }
            _masterTable.DefaultView.RowFilter = $"Convert(MaHD, 'System.String') LIKE '%{keyword}%'";
            UpdateDetailSelectionAfterFilter();
        }

        private void UpdateDetailSelectionAfterFilter()
        {
            if (_dgvHoaDonMaster.Rows.Count == 0)
            {
                _selectedMaHoaDon = null;
                _dgvHoaDonDetail.DataSource = null;
                UpdateReceiptHeader(null, null);
                return;
            }

            if (_dgvHoaDonMaster.CurrentRow is null || _dgvHoaDonMaster.CurrentRow.IsNewRow)
            {
                _dgvHoaDonMaster.Rows[0].Selected = true;
                _dgvHoaDonMaster.CurrentCell = _dgvHoaDonMaster.Rows[0].Cells[0];
            }

            if (_dgvHoaDonMaster.CurrentRow is not null)
            {
                _selectedMaHoaDon = Convert.ToString(_dgvHoaDonMaster.CurrentRow.Cells["MaHD"].Value);
                LoadDetailData(_selectedMaHoaDon);
                UpdateReceiptHeader(_dgvHoaDonMaster.CurrentRow, _currentInvoiceType);
            }
        }

        private void LoadDetailData(string? maHd)
        {
            if (string.IsNullOrWhiteSpace(maHd))
            {
                _dgvHoaDonDetail.DataSource = null;
                return;
            }
            DataTable detail = _lichSuHoaDonService.GetDetailData(_currentInvoiceType, maHd);
            _dgvHoaDonDetail.DataSource = detail;
            ConfigureDetailGrid();
        }

        private void DgvHoaDonMaster_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            DataGridViewRow row = _dgvHoaDonMaster.Rows[e.RowIndex];
            _selectedMaHoaDon = Convert.ToString(row.Cells["MaHD"].Value) ?? string.Empty;
            LoadDetailData(_selectedMaHoaDon);
            UpdateReceiptHeader(row, _currentInvoiceType);
        }

        private void UpdateReceiptHeader(DataGridViewRow? row, string? invoiceType)
        {
            string nhanVien = row is null ? "-" : (Convert.ToString(row.Cells["NguoiThucHien"].Value) ?? "-");
            string doiTac = row is null ? "-" : (Convert.ToString(row.Cells["DoiTac"].Value) ?? "-");

            lblReceiptNhanVien.Text = $"👤 Nhân viên: {nhanVien}";
            lblReceiptDoiTac.Text = invoiceType == "BAN"
                ? $"🤝 Khách hàng: {doiTac}"
                : $"🤝 Nhà cung cấp: {doiTac}";
        }

        private void LoadTopMetrics()
        {
            int soDonHuy = _lichSuHoaDonService.GetCanceledCount(_dtpTuNgay.Value.Date, _dtpDenNgay.Value.Date, _schemaInfo);
            _lblSoDonHuyValue.Text = soDonHuy.ToString(CultureInfo.InvariantCulture);

            if (!_isAdmin)
            {
                pnlChoDuyetHuy.Visible = false;
                return;
            }

            int pendingCount = _lichSuHoaDonService.GetPendingCancelCount(_dtpTuNgay.Value.Date, _dtpDenNgay.Value.Date, _schemaInfo);

            pnlChoDuyetHuy.Visible = true;
            lblChoDuyetHuyTitle.Text = $"Đơn chờ duyệt ({pendingCount})";
        }

        private void pnlChoDuyetHuy_Click(object? sender, EventArgs e)
        {
            if (!_isAdmin)
            {
                return;
            }

            ShowPendingCancelPopup();
        }

        private void ShowPendingCancelPopup()
        {
            DataTable pending = _lichSuHoaDonService.GetPendingCancelList(_dtpTuNgay.Value.Date, _dtpDenNgay.Value.Date, _schemaInfo);

            using Form popup = new Form
            {
                Text = "Duyệt hóa đơn hủy",
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                Width = 1080, // Tăng chiều rộng Form từ 940 lên 1080 để xem hết các cột
                Height = 540
            };

            // Chỉnh lại Panel trái nhỏ hơn 1 chút để nhường không gian cho Panel chi tiết bên phải
            Panel leftPanel = new Panel { Dock = DockStyle.Left, Width = 560, Padding = new Padding(10) };
            Panel rightPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            Label lblLeft = new Label { Text = $"Danh sách đơn chờ ({pending.Rows.Count})", Dock = DockStyle.Top, Height = 26, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            DataGridView dgvPending = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                BackgroundColor = Color.WhiteSmoke,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells
            };

            leftPanel.Controls.Add(dgvPending);
            leftPanel.Controls.Add(lblLeft);

            Label lblRight = new Label { Text = "Chi tiết hóa đơn", Dock = DockStyle.Top, Height = 26, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            DataGridView dgvDetail = new DataGridView
            {
                Dock = DockStyle.Top,
                Height = 260,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                BackgroundColor = Color.WhiteSmoke,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells
            };

            Label lblReason = new Label { Text = "Lý do hủy", Dock = DockStyle.Top, Height = 22 };
            TextBox txtReason = new TextBox
            {
                Dock = DockStyle.Top,
                Height = 70,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical
            };

            FlowLayoutPanel buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                FlowDirection = FlowDirection.RightToLeft // Vẫn giữ RightToLeft để neo các nút sát mép phải
            };

            Button btnApprove = new Button { Text = "Đồng ý hủy", Width = 110, Height = 32, BackColor = Color.Orange, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            // Đổi màu nút Từ chối sang màu đỏ (IndianRed)
            Button btnReject = new Button { Text = "Từ chối", Width = 90, Height = 32, BackColor = Color.IndianRed, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            Button btnClose = new Button { Text = "Thoát", Width = 80, Height = 32, BackColor = Color.Silver, ForeColor = Color.Black, FlatStyle = FlatStyle.Flat };

            // Add ngược thứ tự do FlowDirection là RightToLeft
            // Nút nào Add trước sẽ nằm sát bên phải. Thứ tự xuất hiện sẽ là: Đồng ý (trái) -> Từ chối (giữa) -> Thoát (phải)
            buttonPanel.Controls.Add(btnClose);
            buttonPanel.Controls.Add(btnReject);
            buttonPanel.Controls.Add(btnApprove);

            rightPanel.Controls.Add(buttonPanel);
            rightPanel.Controls.Add(txtReason);
            rightPanel.Controls.Add(lblReason);
            rightPanel.Controls.Add(dgvDetail);
            rightPanel.Controls.Add(lblRight);

            popup.Controls.Add(rightPanel);
            popup.Controls.Add(leftPanel);

            dgvPending.DataSource = pending;
            ConfigurePendingGrid(dgvPending);

            void LoadSelectedPending()
            {
                if (dgvPending.CurrentRow is null)
                {
                    dgvDetail.DataSource = null;
                    txtReason.Text = string.Empty;
                    return;
                }

                string maHd = Convert.ToString(dgvPending.CurrentRow.Cells["MaHD"].Value) ?? string.Empty;
                txtReason.Text = Convert.ToString(dgvPending.CurrentRow.Cells["LyDoHuy"].Value) ?? string.Empty;
                // Chi tiết hóa đơn
                DataTable detail = _lichSuHoaDonService.GetDetailData("BAN", maHd);
                dgvDetail.DataSource = detail;
                ConfigurePopupDetailGrid(dgvDetail);
            }

            dgvPending.SelectionChanged += (_, __) => LoadSelectedPending();

            btnReject.Click += (_, __) =>
            {
                if (dgvPending.CurrentRow is null) return;

                string maHd = Convert.ToString(dgvPending.CurrentRow.Cells["MaHD"].Value) ?? string.Empty;
                if (string.IsNullOrWhiteSpace(maHd)) return;

                try
                {
                    _lichSuHoaDonService.RejectCancelRequest(maHd);
                    pending.Rows.RemoveAt(dgvPending.CurrentRow.Index);
                    LoadTopMetrics();
                    LoadMasterData();
                    LoadSelectedPending();
                    MessageBox.Show("Đã từ chối hủy hóa đơn.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Từ chối yêu cầu thất bại.\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            btnClose.Click += (_, __) => popup.Close();

            btnApprove.Click += (_, __) =>
            {
                if (dgvPending.CurrentRow is null) return;

                string maHd = Convert.ToString(dgvPending.CurrentRow.Cells["MaHD"].Value) ?? string.Empty;
                if (string.IsNullOrWhiteSpace(maHd)) return;

                try
                {
                    _lichSuHoaDonService.CancelInvoice("BAN", maHd, _schemaInfo);
                    pending.Rows.RemoveAt(dgvPending.CurrentRow.Index);
                    LoadTopMetrics();
                    LoadMasterData();
                    LoadSelectedPending();
                    MessageBox.Show("Đã hủy hóa đơn thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Duyệt hủy hóa đơn thất bại.\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            if (dgvPending.Rows.Count > 0)
            {
                dgvPending.Rows[0].Selected = true;
                dgvPending.CurrentCell = dgvPending.Rows[0].Cells[0];
                LoadSelectedPending();
            }

            popup.ShowDialog(this);
        }

        private static void ConfigurePendingGrid(DataGridView dgv)
        {
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
            dgv.DefaultCellStyle.WrapMode = DataGridViewTriState.False;

            if (dgv.Columns.Contains("MaHD")) dgv.Columns["MaHD"].HeaderText = "Mã HĐ";
            if (dgv.Columns.Contains("ThoiGian"))
            {
                dgv.Columns["ThoiGian"].HeaderText = "Thời gian";
                dgv.Columns["ThoiGian"].DefaultCellStyle.Format = "HH:mm - dd/MM";
                dgv.Columns["ThoiGian"].Width = 110;
            }

            if (dgv.Columns.Contains("NhanVien"))
            {
                dgv.Columns["NhanVien"].HeaderText = "Nhân viên";
                dgv.Columns["NhanVien"].Width = 130;
            }

            if (dgv.Columns.Contains("DoiTac"))
            {
                dgv.Columns["DoiTac"].HeaderText = "Khách hàng";
                dgv.Columns["DoiTac"].Width = 120;
            }

            if (dgv.Columns.Contains("TongTien"))
            {
                dgv.Columns["TongTien"].HeaderText = "Tổng tiền";
                dgv.Columns["TongTien"].DefaultCellStyle.Format = "N0";
                dgv.Columns["TongTien"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                dgv.Columns["TongTien"].Width = 100;
            }

            if (dgv.Columns.Contains("LyDoHuy"))
            {
                dgv.Columns["LyDoHuy"].Visible = false;
            }
        }

        private static void ConfigurePopupDetailGrid(DataGridView dgv)
        {
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
            dgv.DefaultCellStyle.WrapMode = DataGridViewTriState.False;

            if (dgv.Columns.Contains("TenHang"))
            {
                dgv.Columns["TenHang"].HeaderText = "Tên món";
                dgv.Columns["TenHang"].Width = 180;
            }

            if (dgv.Columns.Contains("SoLuong"))
            {
                dgv.Columns["SoLuong"].HeaderText = "SL";
                dgv.Columns["SoLuong"].Width = 45;
                dgv.Columns["SoLuong"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }

            if (dgv.Columns.Contains("DonGia"))
            {
                dgv.Columns["DonGia"].HeaderText = "Đơn giá";
                dgv.Columns["DonGia"].Width = 90;
                dgv.Columns["DonGia"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                dgv.Columns["DonGia"].DefaultCellStyle.Format = "N0";
            }

            if (dgv.Columns.Contains("ThanhTien"))
            {
                dgv.Columns["ThanhTien"].HeaderText = "Thành tiền";
                dgv.Columns["ThanhTien"].Width = 100;
                dgv.Columns["ThanhTien"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                dgv.Columns["ThanhTien"].DefaultCellStyle.Format = "N0";
            }

            if (dgv.Columns.Contains("GhiChu"))
            {
                dgv.Columns["GhiChu"].Visible = false;
            }
        }

        private void BtnLamMoi_Click(object? sender, EventArgs e)
        {
            _txtTimMaHD.Clear();
            LoadMasterData();
            LoadTopMetrics();
        }

        private void BtnHuyHoaDon_Click(object? sender, EventArgs e)
        {
            if (!_isAdmin)
            {
                MessageBox.Show("Bạn không có quyền hủy hóa đơn.", "Phân quyền", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(_selectedMaHoaDon))
            {
                MessageBox.Show("Vui lòng chọn hóa đơn cần hủy.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult result = MessageBox.Show("Bạn có chắc chắn muốn hủy hóa đơn đã chọn?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes)
            {
                return;
            }

            try
            {
                _lichSuHoaDonService.CancelInvoice(_currentInvoiceType, _selectedMaHoaDon, _schemaInfo);
                MessageBox.Show("Hủy hóa đơn thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadMasterData();
                LoadTopMetrics();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hủy hóa đơn thất bại.\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnXuatBaoCao_Click(object? sender, EventArgs e)
        {
            if (_masterTable is null)
            {
                return;
            }

            using SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "CSV file (*.csv)|*.csv",
                FileName = $"LichSuHoaDon_{_currentInvoiceType}_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };

            if (sfd.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("MaHD,ThoiGian,NguoiThucHien,DoiTac,TongTien,TrangThai");

            foreach (DataRowView row in _masterTable.DefaultView)
            {
                string line = string.Join(",",
                    Csv(row["MaHD"]),
                    Csv(row["ThoiGian"]),
                    Csv(row["NguoiThucHien"]),
                    Csv(row["DoiTac"]),
                    Csv(row["TongTien"]),
                    Csv(row["TrangThai"]));
                sb.AppendLine(line);
            }

            File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
            MessageBox.Show("Xuất báo cáo thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private static string Csv(object? value)
        {
            string text = Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
            text = text.Replace("\"", "\"\"");
            return $"\"{text}\"";
        }

        private void BtnInLai_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_selectedMaHoaDon) || _dgvHoaDonMaster.CurrentRow is null)
            {
                MessageBox.Show("Vui lòng chọn hóa đơn cần in lại.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DataGridViewRow row = _dgvHoaDonMaster.CurrentRow;
            decimal tongTien = Convert.ToDecimal(row.Cells["TongTien"].Value ?? 0m, CultureInfo.InvariantCulture);
            DateTime thoiGian = Convert.ToDateTime(row.Cells["ThoiGian"].Value, CultureInfo.InvariantCulture);
            _printMaHd = Convert.ToString(row.Cells["MaHD"].Value) ?? string.Empty;
            _printNhanVien = Convert.ToString(row.Cells["NguoiThucHien"].Value) ?? "-";
            _printDoiTac = Convert.ToString(row.Cells["DoiTac"].Value) ?? "-";
            _printThoiGian = thoiGian;
            _printTongTien = tongTien;

            if (_currentInvoiceType == "BAN")
            {
                _printTenHang = Convert.ToString(row.Cells["TenHang"].Value) ?? string.Empty;
                _printPhanTramGiam = Convert.ToInt32(row.Cells["PhanTramGiam"].Value ?? 0);
                _printDiemTichLuy = Convert.ToInt32(row.Cells["DiemTichLuy"].Value ?? 0);
                _printTienHangGoc = _lichSuHoaDonService.GetTienHangGoc(_printMaHd);
                _printSdtNcc = string.Empty;
            }
            else
            {
                _printTenHang = string.Empty;
                _printPhanTramGiam = 0;
                _printDiemTichLuy = 0;
                _printTienHangGoc = 0;
                _printSdtNcc = Convert.ToString(row.Cells["SDT"].Value) ?? string.Empty;
            }

            string qrPayload;
            if (_currentInvoiceType == "NHAP")
            {
                qrPayload = BuildNhapKhoQrId(_printMaHd, _printThoiGian);
            }
            else
            {
                string vnPayCode = BuildVnPayCode(_printMaHd, tongTien, thoiGian);
                qrPayload = BuildVnPayQrPayload(vnPayCode, tongTien, thoiGian);
            }

            _printQrImage?.Dispose();
            _printQrImage = TryCreateQrImage(qrPayload, 170);

            _printContent = string.Empty;

            PrintDocument doc = new PrintDocument();
            doc.DefaultPageSettings.PaperSize = new PaperSize("Receipt", 315, 1200);
            doc.DefaultPageSettings.Margins = new Margins(10, 10, 10, 10);
            doc.PrintPage += Doc_PrintPage;

            using PrintPreviewDialog preview = new PrintPreviewDialog
            {
                Document = doc,
                Width = 900,
                Height = 700
            };

            preview.ShowDialog(this);
        }

        private static string BuildVnPayCode(string? maHd, decimal tongTien, DateTime thoiGian)
        {
            string id = string.IsNullOrWhiteSpace(maHd) ? "UNK" : maHd.Trim();
            long amount = decimal.ToInt64(decimal.Round(tongTien, 0, MidpointRounding.AwayFromZero));
            string seed = $"{id}|{amount}|{thoiGian:yyyyMMddHHmmss}";

            using SHA1 sha1 = SHA1.Create();
            byte[] hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(seed));
            string token = BitConverter.ToString(hash, 0, 3).Replace("-", string.Empty);

            return $"VNP-{amount}-{token}";
        }

        private static string BuildVnPayQrPayload(string vnPayCode, decimal tongTien, DateTime thoiGian)
        {
            long amount = decimal.ToInt64(decimal.Round(tongTien, 0, MidpointRounding.AwayFromZero));
            return $"VNPAY|CODE={vnPayCode}|AMOUNT={amount}|TIME={thoiGian:yyyyMMddHHmmss}";
        }

        private static string BuildNhapKhoQrId(string? maHd, DateTime thoiGian)
        {
            string idPart = string.IsNullOrWhiteSpace(maHd) ? "000" : maHd.Trim();
            return $"PNK-{thoiGian:yyyy-MM-dd}-{idPart}";
        }

        private static Image? TryCreateQrImage(string payload, int size)
        {
            try
            {
                string url = $"https://api.qrserver.com/v1/create-qr-code/?size={size}x{size}&data={Uri.EscapeDataString(payload)}";
                using HttpClient client = new HttpClient();
                byte[] bytes = client.GetByteArrayAsync(url).GetAwaiter().GetResult();
                using MemoryStream ms = new MemoryStream(bytes);
                using Image img = Image.FromStream(ms);
                return new Bitmap(img);
            }
            catch
            {
                return null;
            }
        }

        private void Doc_PrintPage(object? sender, PrintPageEventArgs e)
        {
            using Font brandFont = new Font("Segoe UI", 11, FontStyle.Bold);
            using Font titleFont = new Font("Segoe UI", 10, FontStyle.Bold);
            using Font normalFont = new Font("Segoe UI", 8.5f);
            using Font monoFont = new Font("Consolas", 8.5f);
            using Font totalFont = new Font("Segoe UI", 10f, FontStyle.Bold);
            using Font discountFont = new Font("Segoe UI", 8.5f, FontStyle.Italic);

            float left = e.MarginBounds.Left;
            float right = e.MarginBounds.Right;
            float width = e.MarginBounds.Width;
            float y = e.MarginBounds.Top;

            using StringFormat rightFormat = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Near };

            void DrawCentered(string text, Font font)
            {
                SizeF s = e.Graphics.MeasureString(text, font);
                float x = left + (width - s.Width) / 2f;
                e.Graphics.DrawString(text, font, Brushes.Black, x, y);
                y += s.Height + 1f;
            }

            void DrawLineRightAligned(string label, string amount, Font font, Brush brush)
            {
                e.Graphics.DrawString(label, font, brush, left, y);
                RectangleF amountRect = new RectangleF(left, y, width, font.GetHeight(e.Graphics));
                e.Graphics.DrawString(amount, font, brush, amountRect, rightFormat);
                y += font.GetHeight(e.Graphics) + 1f;
            }

            bool isNhap = _currentInvoiceType == "NHAP";

            // === HEADER ===
            DrawCentered("TỨ ĐẠI THIÊN LONG", brandFont);
            DrawCentered("169 Nguyễn Lương Bằng", normalFont);
            DrawCentered("SĐT: 0374895922", normalFont);

            y += 2f;
            e.Graphics.DrawLine(Pens.Gray, left, y, right, y);
            y += 4f;

            // === THÔNG TIN CHUNG ===
            DrawCentered(isNhap ? "PHIẾU NHẬP KHO" : "HÓA ĐƠN BÁN HÀNG", titleFont);
            e.Graphics.DrawString($"Mã hóa đơn: {_printMaHd}", normalFont, Brushes.Black, left, y); y += normalFont.GetHeight(e.Graphics) + 1f;
            e.Graphics.DrawString($"Ngày: {_printThoiGian:dd/MM/yyyy HH:mm}", normalFont, Brushes.Black, left, y); y += normalFont.GetHeight(e.Graphics) + 1f;
            e.Graphics.DrawString($"{(isNhap ? "Người nhập" : "Nhân viên")}: {_printNhanVien}", normalFont, Brushes.Black, left, y); y += normalFont.GetHeight(e.Graphics) + 1f;
            e.Graphics.DrawString($"{(isNhap ? "Nhà cung cấp" : "Khách hàng")}: {_printDoiTac}", normalFont, Brushes.Black, left, y); y += normalFont.GetHeight(e.Graphics) + 1f;
            
            if (isNhap && !string.IsNullOrWhiteSpace(_printSdtNcc))
            {
                e.Graphics.DrawString($"SĐT NCC: {_printSdtNcc}", normalFont, Brushes.Black, left, y); y += normalFont.GetHeight(e.Graphics) + 1f;
            }

            if (!isNhap && !string.IsNullOrWhiteSpace(_printTenHang))
            {
                e.Graphics.DrawString($"Hạng thành viên: {_printTenHang}", normalFont, Brushes.Black, left, y); y += normalFont.GetHeight(e.Graphics) + 1f;
                e.Graphics.DrawString($"Điểm tích lũy: {_printDiemTichLuy:N0}", normalFont, Brushes.Black, left, y); y += normalFont.GetHeight(e.Graphics) + 1f;
            }
            y += 2f;

            e.Graphics.DrawLine(Pens.Gray, left, y, right, y);
            y += 4f;

            // === BẢNG DANH SÁCH ===
            float dvtW = isNhap ? 40f : 0f;
            float qtyW = 24f;
            float priceW = 52f;
            float totalW = 62f;
            const float colGap = 4f;
            float nameW = width - dvtW - qtyW - priceW - totalW - (colGap * (isNhap ? 3f : 2f));

            RectangleF nameRect = new RectangleF(left, y, nameW, 16f);
            RectangleF dvtRect = new RectangleF(nameRect.Right + colGap, y, dvtW, 16f);
            RectangleF qtyRect = new RectangleF((isNhap ? dvtRect.Right : nameRect.Right) + colGap, y, qtyW, 16f);
            RectangleF priceRect = new RectangleF(qtyRect.Right + colGap, y, priceW, 16f);
            RectangleF totalRect = new RectangleF(priceRect.Right, y, totalW, 16f);

            e.Graphics.DrawString(isNhap ? "Tên hàng" : "Tên món", monoFont, Brushes.Black, nameRect);
            if (isNhap) e.Graphics.DrawString("ĐVT", monoFont, Brushes.Black, dvtRect);
            e.Graphics.DrawString("SL", monoFont, Brushes.Black, qtyRect, rightFormat);
            e.Graphics.DrawString("Đơn giá", monoFont, Brushes.Black, priceRect, rightFormat);
            e.Graphics.DrawString("T.Tiền", monoFont, Brushes.Black, totalRect, rightFormat);
            y += 15f;

            e.Graphics.DrawLine(Pens.LightGray, left, y, right, y);
            y += 3f;

            foreach (DataGridViewRow r in _dgvHoaDonDetail.Rows)
            {
                if (r.IsNewRow) continue;

                string ten = Convert.ToString(r.Cells["TenHang"].Value) ?? string.Empty;
                string dvt = isNhap && _dgvHoaDonDetail.Columns.Contains("DonViTinh") ? (Convert.ToString(r.Cells["DonViTinh"].Value) ?? string.Empty) : string.Empty;
                string sl = Convert.ToString(r.Cells["SoLuong"].Value) ?? "0";
                decimal dg = 0m;
                decimal tt = 0m;
                try { dg = Convert.ToDecimal(r.Cells["DonGia"].Value ?? 0m, CultureInfo.InvariantCulture); } catch { }
                try { tt = Convert.ToDecimal(r.Cells["ThanhTien"].Value ?? 0m, CultureInfo.InvariantCulture); } catch { }

                nameRect = new RectangleF(left, y, nameW, 14f);
                dvtRect = new RectangleF(nameRect.Right + colGap, y, dvtW, 14f);
                qtyRect = new RectangleF((isNhap ? dvtRect.Right : nameRect.Right) + colGap, y, qtyW, 14f);
                priceRect = new RectangleF(qtyRect.Right + colGap, y, priceW, 14f);
                totalRect = new RectangleF(priceRect.Right, y, totalW, 14f);

                e.Graphics.DrawString(ten, monoFont, Brushes.Black, nameRect);
                if (isNhap) e.Graphics.DrawString(dvt, monoFont, Brushes.Black, dvtRect);
                e.Graphics.DrawString(sl, monoFont, Brushes.Black, qtyRect, rightFormat);
                e.Graphics.DrawString($"{dg:N0}", monoFont, Brushes.Black, priceRect, rightFormat);
                e.Graphics.DrawString($"{tt:N0}", monoFont, Brushes.Black, totalRect, rightFormat);

                y += 14f;
            }

            y += 2f;
            e.Graphics.DrawLine(Pens.Gray, left, y, right, y);
            y += 4f;

            // === PHẦN THANH TOÁN ===
            if (isNhap)
            {
                DrawLineRightAligned("TỔNG TIỀN:", $"{_printTongTien:N0} VNĐ", totalFont, Brushes.Black);
                string bangChu = DocSoTienBangChu(_printTongTien);
                e.Graphics.DrawString($"Bằng chữ: {bangChu}", normalFont, Brushes.Black, left, y); y += normalFont.GetHeight(e.Graphics) + 1f;
                y += 4f;
            }
            else
            {
                decimal tienHang = _printTienHangGoc > 0 ? _printTienHangGoc : _printTongTien;
                decimal giamHang = _printPhanTramGiam > 0 ? tienHang * _printPhanTramGiam / 100m : 0m;
                decimal giamDiem = tienHang > _printTongTien + giamHang ? (tienHang - _printTongTien - giamHang) : 0m;
                int diemDaDung = giamDiem > 0 ? (int)(giamDiem / 1000m) : 0;
                decimal tongGiam = giamHang + giamDiem;
                bool coGiamGia = tongGiam > 0;

                if (coGiamGia)
                {
                    DrawLineRightAligned("Tiền hàng:", $"{tienHang:N0} đ", normalFont, Brushes.Black);

                    if (_printPhanTramGiam > 0)
                    {
                        DrawLineRightAligned($"Giảm giá hạng ({_printTenHang} - {_printPhanTramGiam}%):", $"- {giamHang:N0} đ", discountFont, Brushes.Black);
                    }

                    if (diemDaDung > 0)
                    {
                        DrawLineRightAligned($"Tiêu điểm ({diemDaDung} điểm):", $"- {giamDiem:N0} đ", discountFont, Brushes.Black);
                    }

                    DrawLineRightAligned("Tổng giảm giá:", $"- {tongGiam:N0} đ", normalFont, Brushes.Black);
                    y += 2f;
                }

                DrawLineRightAligned("TỔNG CỘNG:", $"{_printTongTien:N0} đ", totalFont, Brushes.Black);
                y += 2f;
            }

            if (_printQrImage is not null)
            {
                y += 3f;
                DrawCentered(isNhap ? "Mã xác thực nhập kho" : "Quét mã để thanh toán", normalFont);

                const float qrSize = 74f;
                float qrX = left + (width - qrSize) / 2f;
                e.Graphics.DrawImage(_printQrImage, qrX, y, qrSize, qrSize);
                y += qrSize + 6f;
            }

            if (isNhap)
            {
                y += 4f;
                float signY = y;
                float half = width / 2f;
                e.Graphics.DrawString("Người giao hàng", normalFont, Brushes.Black, left, signY);
                e.Graphics.DrawString("Người nhận", normalFont, Brushes.Black, left + half, signY);
                signY += normalFont.GetHeight(e.Graphics) + 16f;
                e.Graphics.DrawLine(Pens.Black, left, signY, left + half - 12f, signY);
                e.Graphics.DrawLine(Pens.Black, left + half, signY, right, signY);
            }
            else
            {
                DrawCentered("Cảm ơn Quý khách. Hẹn gặp lại!", normalFont);
            }
        }

        private void btn_QLNCC_Click(object? sender, EventArgs e)
        {
            AdminNavigationManager.Navigate<QuanLiNhaCungCap>(this);
        }

        private void btn_QLKH_Click(object? sender, EventArgs e)
        {
            AdminNavigationManager.Navigate<QuanLiKhachHang>(this);
        }

        private void btn_QLNV_Click(object? sender, EventArgs e)
        {
            AdminNavigationManager.Navigate<QuanLiNhanVien>(this);
        }

        private void btn_QLMA_Click(object? sender, EventArgs e)
        {
            AdminNavigationManager.Navigate<QuanLiMonAn>(this);
        }

        private void btn_QLHDN_Click(object? sender, EventArgs e)
        {
            AdminNavigationManager.Navigate<QuanLiNguyenLieu>(this);
        }

        private void btn_ThongKe_Click(object? sender, EventArgs e)
        {
            AdminNavigationManager.Navigate<ThongKe>(this);
        }

        private void btn_DangXuat_Click(object? sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Bạn có chắc chắn muốn đăng xuất?", "Xác nhận đăng xuất", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes)
            {
                return;
            }
            AdminNavigationManager.Logout(this);
        }

        private void _txtTimMaHD_TextChanged(object sender, EventArgs e)
        {

        }

        private static string DocSoTienBangChu(decimal soTien)
        {
            long so = (long)Math.Round(soTien, 0, MidpointRounding.AwayFromZero);
            if (so == 0) return "Không đồng";

            string[] donVi = { "", "nghìn", "triệu", "tỷ" };
            string[] chu = { "không", "một", "hai", "ba", "bốn", "năm", "sáu", "bảy", "tám", "chín" };

            string DocBaChuSo(int n)
            {
                int tram = n / 100;
                int chuc = (n % 100) / 10;
                int dv = n % 10;
                string result = "";

                if (tram > 0)
                    result = chu[tram] + " trăm";

                if (chuc > 1)
                {
                    result += " " + chu[chuc] + " mươi";
                    if (dv == 1) result += " mốt";
                    else if (dv == 4) result += " tư";
                    else if (dv == 5) result += " lăm";
                    else if (dv > 0) result += " " + chu[dv];
                }
                else if (chuc == 1)
                {
                    result += " mười";
                    if (dv == 5) result += " lăm";
                    else if (dv > 0) result += " " + chu[dv];
                }
                else if (dv > 0)
                {
                    if (tram > 0) result += " lẻ";
                    result += " " + chu[dv];
                }

                return result.Trim();
            }

            if (so < 0) return "Âm " + DocSoTienBangChu(-soTien);

            var parts = new System.Collections.Generic.List<string>();
            int groupIndex = 0;
            long temp = so;
            while (temp > 0)
            {
                int group = (int)(temp % 1000);
                temp /= 1000;
                if (group > 0)
                {
                    string s = DocBaChuSo(group);
                    if (groupIndex < donVi.Length && !string.IsNullOrWhiteSpace(donVi[groupIndex]))
                        s += " " + donVi[groupIndex];
                    parts.Insert(0, s);
                }
                groupIndex++;
            }

            string ketQua = string.Join(" ", parts).Trim();
            if (ketQua.Length > 0)
                ketQua = char.ToUpper(ketQua[0]) + ketQua[1..];

            return ketQua + " đồng chẵn";
        }
    }
}
