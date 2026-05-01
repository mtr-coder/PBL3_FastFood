using PBL3.Business;
using System.Data;

namespace PBL3
{
    public partial class QuanLiKhachHang : Form
    {
        private readonly KhachHangService _khachHangService;
        private readonly BanHangService _banHangService;
        private DataTable? _khachHangTable;
        private string? _selectedMaKhDbValue;

        private string DiemTichLuyText
        {
            get => _txtDiemTichLuy.Text;
            set => _txtDiemTichLuy.Text = value;
        }

        private string DiemTronDoiText
        {
            get => _txtDiemTronDoi.Text;
            set => _txtDiemTronDoi.Text = value;
        }

        private string HangText
        {
            get => _txtHang.Text;
            set => _txtHang.Text = value;
        }

        private static bool IsValidPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return false;
            phone = phone.Trim();
            var m = System.Text.RegularExpressions.Regex.Match(phone, "^\\d{10}$");
            return m.Success;
        }

        public QuanLiKhachHang()
        {
            _khachHangService = new KhachHangService();
            _banHangService = new BanHangService();
            InitializeComponent();
            _cboTimTheo.SelectionChangeCommitted += SearchControl_Changed;
        }

        private void QuanLiKhachHang_Load(object? sender, EventArgs e)
        {
            try
            {
                LoadKhachHang();
                if (_cboTimTheo.Items.Count > 0 && _cboTimTheo.SelectedIndex < 0)
                {
                    _cboTimTheo.SelectedIndex = 0;
                }

                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể tải dữ liệu khách hàng.\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadKhachHang()
        {
            _khachHangTable = _khachHangService.GetAll();
            _dgvNhanVien.DataSource = _khachHangTable;
            _dgvNhanVien.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            _dgvNhanVien.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
            _dgvNhanVien.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;

            SetHeaderText("MaKH", "MãKH");
            SetHeaderText("SDT", "SĐT");
            SetHeaderText("DiemTichLuy", "ĐiểmTíchLũy");
            SetHeaderText("DiemTichLuyTronDoi", "Điểm trọn đời");
            SetHeaderText("TenHang", "Hạng");
            
            // Ẩn cột MaHang
            if (_dgvNhanVien.Columns.Contains("MaHang"))
            {
                _dgvNhanVien.Columns["MaHang"].Visible = false;
            }

            SetColumnWidth("MaKH", 90);
            SetColumnWidth("SDT", 220);
            SetColumnWidth("DiemTichLuy", 130);
            SetColumnWidth("DiemTichLuyTronDoi", 130);
            SetColumnWidth("TenHang", 120);

            ApplySearchFilter();
        }

        private void SearchControl_Changed(object? sender, EventArgs e)
        {
            ApplySearchFilter();
        }

        private void ApplySearchFilter()
        {
            if (_khachHangTable is null)
            {
                return;
            }

            string keyword = _txtTimKiem.Text.Trim().Replace("'", "''");
            if (string.IsNullOrWhiteSpace(keyword))
            {
                _khachHangTable.DefaultView.RowFilter = string.Empty;
                return;
            }

            string selected = (Convert.ToString(_cboTimTheo.SelectedItem) ?? "MãKH").Trim();
            string filter;

            if (selected == "SĐT" || selected == "SDT")
            {
                filter = $"SDT LIKE '%{keyword}%'";
            }
            else if (selected == "ĐiểmTíchLũy" || selected == "Điểm tích lũy" || selected == "DiemTichLuy")
            {
                filter = $"Convert(DiemTichLuy, 'System.String') LIKE '%{keyword}%'";
            }
            else if (selected == "ĐiểmTrọnĐời" || selected == "DiemTronDoi" || selected == "Điểm trọn đời")
            {
                filter = $"Convert(DiemTichLuyTronDoi, 'System.String') LIKE '%{keyword}%'";
            }
            else if (selected == "Hạng" || selected == "Hang" || selected == "TenHang")
            {
                filter = $"TenHang LIKE '%{keyword}%'";
            }
            else
            {
                filter = $"Convert(MaKH, 'System.String') LIKE '%{keyword}%'";
            }

            _khachHangTable.DefaultView.RowFilter = filter;
        }

        private void SetColumnWidth(string columnName, int width)
        {
            DataGridViewColumn? column = _dgvNhanVien.Columns[columnName];
            if (column is not null)
            {
                column.Width = width;
            }
        }

        private void SetHeaderText(string columnName, string headerText)
        {
            DataGridViewColumn? column = _dgvNhanVien.Columns[columnName];
            if (column is not null)
            {
                column.HeaderText = headerText;
            }
        }

        private void DgvNhanVien_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            DataGridViewRow row = _dgvNhanVien.Rows[e.RowIndex];
            _selectedMaKhDbValue = Convert.ToString(row.Cells["MaKH"].Value) ?? string.Empty;
            _txtMaNV.Text = FormatMaKhForDisplay(_selectedMaKhDbValue);
            _txtSdt.Text = Convert.ToString(row.Cells["SDT"].Value) ?? string.Empty;
            DiemTichLuyText = Convert.ToString(row.Cells["DiemTichLuy"].Value) ?? "0";
            DiemTronDoiText = Convert.ToString(row.Cells["DiemTichLuyTronDoi"].Value) ?? DiemTichLuyText;
            HangText = Convert.ToString(row.Cells["TenHang"].Value) ?? string.Empty;
        }

        private bool ValidateInput(bool isInsert)
        {
            if (!isInsert && string.IsNullOrWhiteSpace(_txtMaNV.Text))
            {
                MessageBox.Show("Vui lòng chọn khách hàng.", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            string phone = _txtSdt.Text.Trim();
            if (!IsValidPhone(phone))
            {
                MessageBox.Show("Số điện thoại không hợp lệ. Vui lòng nhập đúng 10 chữ số.", "Dữ liệu sai", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtSdt.Focus();
                return false;
            }

            if (IsPhoneExists(isInsert, phone))
            {
                MessageBox.Show("Số điện thoại khách hàng đã tồn tại.", "Dữ liệu trùng lặp", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!int.TryParse(DiemTichLuyText.Trim(), out int diem) || diem < 0)
            {
                MessageBox.Show("Điểm tích lũy không hợp lệ.", "Dữ liệu sai", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!int.TryParse(DiemTronDoiText.Trim(), out int diemTronDoi) || diemTronDoi < 0)
            {
                MessageBox.Show("Điểm trọn đời không hợp lệ.", "Dữ liệu sai", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private bool IsPhoneExists(bool isInsert, string phone)
        {
            try
            {
                return _khachHangService.IsPhoneExists(isInsert, phone, _selectedMaKhDbValue ?? _txtMaNV.Text.Trim());
            }
            catch
            {
                return false;
            }
        }

        private void BtnThem_Click(object? sender, EventArgs e)
        {
            if (!ValidateInput(true))
            {
                return;
            }
            try
            {
                _khachHangService.Add(_txtSdt.Text.Trim(), int.Parse(DiemTichLuyText.Trim()));

                MessageBox.Show("Thêm khách hàng thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadKhachHang();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Thêm khách hàng thất bại.\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSua_Click(object? sender, EventArgs e)
        {
            if (!ValidateInput(false))
            {
                return;
            }

            try
            {
                int diemTichLuyMoi = int.Parse(DiemTichLuyText.Trim());
                int diemTronDoiHienTai = int.Parse(DiemTronDoiText.Trim());
                
                // Cập nhật thông tin khách hàng
                int rows = _khachHangService.Update(_selectedMaKhDbValue ?? _txtMaNV.Text.Trim(), _txtSdt.Text.Trim(), diemTichLuyMoi);

                if (rows == 0)
                {
                    MessageBox.Show("Không tìm thấy khách hàng để cập nhật.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (rows > 0)
                {
                    // Tính toán hạng mới dựa trên điểm trọn đời hiện tại
                    int maHangMoi = _banHangService.GetHangByDiemTronDoi(diemTronDoiHienTai);
                    _khachHangService.UpdateHang(_selectedMaKhDbValue ?? _txtMaNV.Text.Trim(), maHangMoi);
                }

                MessageBox.Show("Cập nhật khách hàng thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadKhachHang();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Cập nhật khách hàng thất bại.\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnXoa_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_txtMaNV.Text))
            {
                MessageBox.Show("Vui lòng chọn khách hàng cần xóa.", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult result = MessageBox.Show(
                "Bạn có chắc chắn muốn xóa khách hàng này?",
                "Xác nhận xóa",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
            {
                return;
            }

            try
            {
                int rows = _khachHangService.Delete(_selectedMaKhDbValue ?? _txtMaNV.Text.Trim());

                if (rows == 0)
                {
                    MessageBox.Show("Không tìm thấy khách hàng để xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                MessageBox.Show("Xóa khách hàng thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadKhachHang();
                ClearForm();
            }
            catch (Exception ex) when (DataErrorHelper.IsForeignKeyViolation(ex))
            {
                MessageBox.Show("Không thể xóa khách hàng vì đang được sử dụng ở dữ liệu liên quan.", "Không thể xóa", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Xóa khách hàng thất bại.\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnLamMoi_Click(object? sender, EventArgs e)
        {
            ClearForm();
            LoadKhachHang();
        }

        private void ClearForm()
        {
            _selectedMaKhDbValue = null;
            _txtMaNV.Text = GenerateNextMaKH();
            _txtSdt.Clear();
            DiemTichLuyText = "0";
            DiemTronDoiText = "0";
            HangText = string.Empty;
            _txtSdt.Focus();
        }

        private void UpdateHangForCustomer(string maKh, int diemTronDoi)
        {
            int maHangMoi = _banHangService.GetHangByDiemTronDoi(diemTronDoi);
            _khachHangService.UpdateHang(maKh, maHangMoi);
        }

        private string GenerateNextMaKH()
        {
            try
            {
                return _khachHangService.GenerateNextDisplayCode();
            }
            catch
            {
                return "KH1";
            }
        }

        private static string FormatMaKhForDisplay(string? maKhValue)
        {
            if (string.IsNullOrWhiteSpace(maKhValue))
            {
                return string.Empty;
            }

            string value = maKhValue.Trim();
            return value.StartsWith("KH", StringComparison.OrdinalIgnoreCase) ? value.ToUpperInvariant() : $"KH{value}";
        }

        private void btn_QLKH_Click(object? sender, EventArgs e) { }
        private void btn_QLNCC_Click(object? sender, EventArgs e) { AdminNavigationManager.Navigate<QuanLiNhaCungCap>(this); }
        private void btn_QLNV_Click(object? sender, EventArgs e) { AdminNavigationManager.Navigate<QuanLiNhanVien>(this); }
        private void btn_QLMA_Click(object? sender, EventArgs e) { AdminNavigationManager.Navigate<QuanLiMonAn>(this); }
        private void btn_QLHDN_Click(object? sender, EventArgs e) { AdminNavigationManager.Navigate<QuanLiNguyenLieu>(this); }
        private void btn_QLHDB_Click(object? sender, EventArgs e) { AdminNavigationManager.Navigate<LichSuHoaDon>(this); }
        private void btn_ThongKe_Click(object? sender, EventArgs e) { AdminNavigationManager.Navigate<ThongKe>(this); }

        private void btn_DangXuat_Click(object sender, EventArgs e)
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

        private void btn_DangXuat_MouseEnter(object sender, EventArgs e) { btn_DangXuat.BackColor = Color.FromArgb(255, 69, 0); }
        private void btn_DangXuat_MouseLeave(object sender, EventArgs e) { btn_DangXuat.BackColor = Color.LightSalmon; }

        private void roundedPanel1_Paint(object sender, PaintEventArgs e) { }
        private void btn_ThongKe_Paint(object sender, PaintEventArgs e) { }
        private void lblHoTen_Click(object sender, EventArgs e) { }
        private void lblTrangThai_Click(object sender, EventArgs e) { }
        private void lblDiemTichLuy_Click(object sender, EventArgs e) { }
        private void _cboTrangThai_SelectedIndexChanged(object sender, EventArgs e) { }
        private void pnlSdtInput_Paint(object sender, PaintEventArgs e) { }
        private void lblNgaySinh_Click(object sender, EventArgs e) { }
        private void label1_Click(object sender, EventArgs e) { }
        private void roundedPanel4_Paint(object sender, PaintEventArgs e) { }
        private void pictureBox1_Click(object sender, EventArgs e) { }
        private void pnlFormNhanVien_Paint(object sender, PaintEventArgs e) { }
        private void pnlHoTenInput_Paint(object sender, PaintEventArgs e) { }
    }
}