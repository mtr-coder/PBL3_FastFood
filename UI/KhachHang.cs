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

        public KhachHang() : this("1")
        {
        }

        public KhachHang(string maNv)
        {
            _khachHangService = new KhachHangService();
            _banHangService = new BanHangService();
            _maNv = maNv;
            InitializeComponent();
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

        private void LoadKhachHang()
        {
            DataTable dt = _khachHangService.GetForKhachHangPage();

            dgvKhachHang.DataSource = dt;
            ConfigureKhachHangGrid();
            if (dgvKhachHang.Columns.Contains("GiamGiaToiDa"))
            {
                dgvKhachHang.Columns["GiamGiaToiDa"].HeaderText = "Giảm tối đa (đ)";
                dgvKhachHang.Columns["GiamGiaToiDa"].DefaultCellStyle.Format = "N0";
            }

            lblCongThuc.Text = $"Công thức: {DiemMoiMocGiam} điểm = {TienGiamMoiMoc:N0}đ giảm giá | Cộng {DiemCongMoiNguong} điểm mỗi {NguongCongDiem:N0}đ thanh toán";
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
                ClientSize = new Size(600, 360)
            };

            DataGridView grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                DataSource = data
            };

            if (grid.Columns.Contains("SoDiem"))
            {
                grid.Columns["SoDiem"].HeaderText = "Số điểm";
            }
            if (grid.Columns.Contains("LoaiGD"))
            {
                grid.Columns["LoaiGD"].HeaderText = "Loại GD";
            }
            if (grid.Columns.Contains("NoiDung"))
            {
                grid.Columns["NoiDung"].HeaderText = "Nội dung";
            }
            if (grid.Columns.Contains("NgayGD"))
            {
                grid.Columns["NgayGD"].HeaderText = "Ngày";
                grid.Columns["NgayGD"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
            }

            dialog.Controls.Add(grid);
            dialog.ShowDialog(this);
        }

        private void btnThem_Click(object? sender, EventArgs e)
        {
            string sdt = txtSdt.Text.Trim();
            string ten = txtTen.Text.Trim();

            if (string.IsNullOrWhiteSpace(sdt) || !long.TryParse(sdt, out _))
            {
                MessageBox.Show("SĐT không hợp lệ.", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (IsPhoneExists(sdt))
            {
                MessageBox.Show("SĐT đã tồn tại.", "Trùng dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                if (string.IsNullOrWhiteSpace(ten))
                {
                    ten = _khachHangService.GenerateNextDisplayCode();
                }

                _khachHangService.AddWithName(sdt, ten);

                txtSdt.Clear();
                txtTen.Clear();
                LoadKhachHang();
                MessageBox.Show("Thêm khách hàng thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể thêm khách hàng.\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool IsPhoneExists(string sdt)
        {
            return _khachHangService.IsPhoneExists(true, sdt, null);
        }
    }
}
