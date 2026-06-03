using PBL3.Business;
using PBL3.UI;
using System.Data;
using System.Linq;

namespace PBL3
{
    public partial class KhachHang : Form
    {
        private const int DiemMoiMocGiam = 10;
        private const int TienGiamMoiMoc = 10000;
        private const int NguongCongDiem = 100000;
        private const int DiemCongMoiNguong = 10;

        private readonly string _maNv;
        private readonly KhachHangService _khachHangService;
        private readonly BanHangService _banHangService;
        private bool _isNavigating;
        private DataTable? _khachHangDataSource;

        public KhachHang() : this("1")
        {
        }

        public KhachHang(string maNv)
        {
            _khachHangService = new KhachHangService();
            _banHangService = new BanHangService();
            _maNv = maNv;
            InitializeComponent();
            _txtTimKiem.TextChanged += TxtTimKiem_TextChanged;
        }

        private void btn_QLNV_Click(object? sender, EventArgs e) => OpenAndClose(new TrangNhanVien1(_maNv));
        private void btn_QLNCC_Click(object? sender, EventArgs e) => OpenAndClose(new TrangHoaDon(_maNv));
        private void btn_QLKH_Click(object? sender, EventArgs e) => OpenAndClose(new BanHang(_maNv));
        private void btn_QLMA_Click(object? sender, EventArgs e) => OpenAndClose(new MuaHang(_maNv));

        private void btn_DangXuat_Click(object? sender, EventArgs e)
        {
            if (MessageBox.Show("Bạn có chắc muốn đăng xuất?",
                    "Đăng xuất", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                AdminNavigationManager.Logout(this);
            }
        }

        private void OpenAndClose(Form target)
        {
            AdminNavigationManager.Navigate(this, target);
        }

        private void btnLamMoi_Click(object? sender, EventArgs e)
        {
            LoadKhachHang();
        }

        private void KhachHang_Load(object? sender, EventArgs e)
        {
            LoadKhachHang();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            LoadKhachHang();
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (Visible)
            {
                LoadKhachHang();
            }
        }

        private void LoadKhachHang()
        {
            _khachHangDataSource = _khachHangService.GetForKhachHangPage();
            ApplyFilter();

            lblCongThuc.Text = $"Công thức: {DiemMoiMocGiam} điểm = {TienGiamMoiMoc:N0}đ giảm giá | Cộng {DiemCongMoiNguong} điểm mỗi {NguongCongDiem:N0}đ thanh toán";
        }

        private void ApplyFilter()
        {
            if (_khachHangDataSource == null)
            {
                return;
            }

            string filterText = _txtTimKiem.Text.Trim();
            if (string.IsNullOrWhiteSpace(filterText))
            {
                dgvKhachHang.DataSource = _khachHangDataSource;
            }
            else
            {
                var matches = _khachHangDataSource.AsEnumerable()
                    .Where(r => r["SDT"].ToString()?.Contains(filterText, StringComparison.OrdinalIgnoreCase) ?? false)
                    .ToList();

                dgvKhachHang.DataSource = matches.Count > 0
                    ? matches.CopyToDataTable()
                    : _khachHangDataSource.Clone();
            }

            ConfigureKhachHangGrid();
            if (dgvKhachHang.Columns.Contains("GiamGiaToiDa"))
            {
                dgvKhachHang.Columns["GiamGiaToiDa"].HeaderText = "Giảm tối đa (đ)";
                dgvKhachHang.Columns["GiamGiaToiDa"].DefaultCellStyle.Format = "N0";
            }
        }

        private void TxtTimKiem_TextChanged(object? sender, EventArgs e)
        {
            ApplyFilter();
        }

        private void ConfigureKhachHangGrid()
        {
            dgvKhachHang.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvKhachHang.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
            dgvKhachHang.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;

            SetHeaderText("MaKH", "MãKH");
            SetHeaderText("SDT", "SĐT");
            SetHeaderText("DiemTichLuy", "Điểm tích lũy");
            SetHeaderText("DiemTichLuyTronDoi", "Điểm trọn đời");
            SetHeaderText("TenHang", "Hạng");

            SetColumnWidth("MaKH", 80);
            SetColumnWidth("SDT", 130);
            SetColumnWidth("TenHang", 110);
            SetColumnWidth("DiemTichLuy", 110);
            SetColumnWidth("DiemTichLuyTronDoi", 120);
            SetColumnWidth("GiamGiaToiDa", 120);

            DataGridViewColumn? hangColumn = dgvKhachHang.Columns["TenHang"];
            if (hangColumn is not null)
            {
                hangColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                hangColumn.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }

            DataGridViewColumn? diemColumn = dgvKhachHang.Columns["DiemTichLuyTronDoi"];
            if (diemColumn is not null)
            {
                diemColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }

            dgvKhachHang.CellFormatting -= DgvKhachHang_CellFormatting;
            dgvKhachHang.CellFormatting += DgvKhachHang_CellFormatting;
        }

        private void DgvKhachHang_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvKhachHang.Columns[e.ColumnIndex].Name != "TenHang" || e.Value is null)
            {
                return;
            }

            string hang = Convert.ToString(e.Value) ?? string.Empty;
            if (hang.Equals("Kim cương", StringComparison.OrdinalIgnoreCase))
            {
                e.CellStyle.BackColor = Color.LightSkyBlue;
                e.CellStyle.ForeColor = Color.Navy;
            }
            else if (hang.Equals("Vàng", StringComparison.OrdinalIgnoreCase))
            {
                e.CellStyle.BackColor = Color.Gold;
                e.CellStyle.ForeColor = Color.SaddleBrown;
            }
        }

        private void SetHeaderText(string columnName, string headerText)
        {
            DataGridViewColumn? column = dgvKhachHang.Columns[columnName];
            if (column is not null)
            {
                column.HeaderText = headerText;
            }
        }

        private void SetColumnWidth(string columnName, int width)
        {
            DataGridViewColumn? column = dgvKhachHang.Columns[columnName];
            if (column is not null)
            {
                column.Width = width;
            }
        }

        private void btnLichSuDiem_Click(object? sender, EventArgs e)
        {
            if (dgvKhachHang.CurrentRow is null)
            {
                MessageBox.Show("Vui lòng chọn khách hàng.", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int? maKh = null;
            object? value = dgvKhachHang.CurrentRow.Cells["MaKH"].Value;
            if (value is not null && value != DBNull.Value)
            {
                int.TryParse(Convert.ToString(value), out int parsed);
                maKh = parsed;
            }

            if (!maKh.HasValue)
            {
                MessageBox.Show("Không xác định khách hàng.", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DataTable dt = _banHangService.GetLichSuDiem(maKh.Value);
            ShowLichSuDiemPopup(dt, maKh.Value);
        }

        private void ShowLichSuDiemPopup(DataTable data, int maKh)
        {
            using Form dialog = new Form
            {
                Text = $"Lịch sử điểm KH{maKh}",
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MinimizeBox = false,
                MaximizeBox = false,
                ClientSize = new Size(720, 390)
            };

            Panel topBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 34,
                BackColor = SystemColors.Control
            };

            Button btnDong = new Button
            {
                Text = "Đóng",
                Dock = DockStyle.Right,
                Width = 90,
                FlatStyle = FlatStyle.System
            };
            btnDong.Click += (_, _) => dialog.Close();
            topBar.Controls.Add(btnDong);

            DataGridView grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            dialog.Controls.Add(grid);
            dialog.Controls.Add(topBar);

            grid.DataSource = data;

            if (grid.Columns.Contains("MaLS"))
            {
                grid.Columns["MaLS"].Visible = false;
            }
            if (grid.Columns.Contains("NgayGD"))
            {
                grid.Columns["NgayGD"].HeaderText = "Ngày GD";
                grid.Columns["NgayGD"].DisplayIndex = 0;
                grid.Columns["NgayGD"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                grid.Columns["NgayGD"].DefaultCellStyle.Format = "HH:mm dd/MM/yyyy";
            }
            if (grid.Columns.Contains("SoDiem"))
            {
                grid.Columns["SoDiem"].HeaderText = "Số điểm";
                grid.Columns["SoDiem"].DisplayIndex = 1;
                grid.Columns["SoDiem"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }
            if (grid.Columns.Contains("LoaiGD"))
            {
                grid.Columns["LoaiGD"].HeaderText = "Loại GD";
                grid.Columns["LoaiGD"].DisplayIndex = 2;
                grid.Columns["LoaiGD"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }
            if (grid.Columns.Contains("NoiDung"))
            {
                grid.Columns["NoiDung"].HeaderText = "Nội dung";
                grid.Columns["NoiDung"].DisplayIndex = 3;
                grid.Columns["NoiDung"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }

            dialog.ShowDialog(this);
        }

        private void pnlTimKiem_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
