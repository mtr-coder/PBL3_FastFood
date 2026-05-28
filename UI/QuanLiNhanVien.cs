using PBL3.Business;
using PBL3.UI;
using System.Data;

namespace PBL3
{
    public partial class QuanLiNhanVien : Form
    {
        private readonly TrangNhanVienService _trangNhanVienService;
        private DataTable? _nhanVienTable;
        private bool _isEditingExisting;
        private string? _selectedMaNvDbValue;
        private decimal _lastLuongDuKien;

        public QuanLiNhanVien()
        {
            _trangNhanVienService = new TrangNhanVienService();
            InitializeComponent();
            _cboTimTheo.SelectionChangeCommitted += SearchControl_Changed;
            _txtMatKhau.UseSystemPasswordChar = true;
            _lblBtnXemLichTruc.Text = "Xem lịch trực";
        }

        private static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            // Chỉ chấp nhận email có đuôi @gmail.com
            try
            {
                var regex = new System.Text.RegularExpressions.Regex(@"^[A-Za-z0-9._%+-]+@gmail\.com$", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                return regex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }

        private static bool IsValidPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return false;
            phone = phone.Trim();
            // require exactly 10 digits
            var m = System.Text.RegularExpressions.Regex.Match(phone, "^\\d{10}$");
            return m.Success;
        }

        private void QuanLiNhanVien_Load(object? sender, EventArgs e)
        {
            try
            {
                LoadChucVu();
                LoadNhanVien();
                EnsureCreateModeUi();
                UpdateInboxButtonBadge();
                ClearForm();
                ApplySearchFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể tải dữ liệu nhân viên.\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadChucVu()
        {
            DataTable dt = _trangNhanVienService.GetChucVu();
            _cboChucVu.DataSource = dt;
            _cboChucVu.DisplayMember = "TenCV";
            _cboChucVu.ValueMember = "MaCV";
            DataTable dtFilter = dt.Copy();

            DataRow rowAll = dtFilter.NewRow();
            rowAll["MaCV"] = "-1";
            rowAll["TenCV"] = "Tất cả";
            dtFilter.Rows.InsertAt(rowAll, 0);
            _cboTimTheo.SelectionChangeCommitted -= SearchControl_Changed;

            _cboTimTheo.DataSource = dtFilter;
            _cboTimTheo.DisplayMember = "TenCV";
            _cboTimTheo.ValueMember = "TenCV";

            _cboTimTheo.SelectionChangeCommitted += SearchControl_Changed;
        }

        private void LoadNhanVien()
        {
            _nhanVienTable = _trangNhanVienService.GetAllNhanVien();
            _dgvNhanVien.DataSource = _nhanVienTable;
            _dgvNhanVien.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

            DataGridViewColumn? maCvColumn = _dgvNhanVien.Columns["MaCV"];
            if (maCvColumn is not null)
            {
                maCvColumn.Visible = false;
            }

            DataGridViewColumn? tenCvColumn = _dgvNhanVien.Columns["TenCV"];
            if (tenCvColumn is not null)
            {
                tenCvColumn.HeaderText = "Chức vụ";
                tenCvColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            }

            SetHeaderText("MaNV", "Mã NV");
            SetHeaderText("HoTen", "Họ Tên");
            SetHeaderText("NgaySinh", "Ngày sinh");
            SetHeaderText("SDT", "SĐT");
            SetHeaderText("Email", "Email");
            SetHeaderText("DiaChi", "Địa chỉ");
            SetHeaderText("MatKhau", "Mật khẩu");
            SetHeaderText("TrangThai", "Trạng thái");

            DataGridViewColumn? matKhauColumn = _dgvNhanVien.Columns["MatKhau"];
            if (matKhauColumn is not null)
            {
                matKhauColumn.Visible = false;
            }

            SetColumnWidth("MaNV", 85);
            SetColumnWidth("HoTen", 135);
            SetColumnWidth("NgaySinh", 105);
            SetColumnWidth("SDT", 95);
            SetColumnWidth("Email", 150);
            SetColumnWidth("DiaChi", 145);
            SetColumnWidth("TrangThai", 105);
            SetColumnWidth("TenCV", 115);

            ConfigureGridAppearance();

            ApplySearchFilter();

            if (_dgvNhanVien.Rows.Count > 0)
            {
                _dgvNhanVien.ClearSelection();
                _dgvNhanVien.CurrentCell = null;
            }
        }

        private void EnsureCreateModeUi()
        {
            _isEditingExisting = false;
            UpdatePasswordUiState();
        }

        private void ConfigureGridAppearance()
        {
            _dgvNhanVien.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
            _dgvNhanVien.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;

            DataGridViewColumn? ngaySinhColumn = _dgvNhanVien.Columns["NgaySinh"];
            if (ngaySinhColumn is not null)
            {
                ngaySinhColumn.DefaultCellStyle.Format = "dd/MM/yyyy";
            }

            CenterColumn("MaNV");
            CenterColumn("SDT");
            CenterColumn("TrangThai");
            CenterColumn("TenCV");
            CenterColumn("NgaySinh");

            LeftColumn("HoTen");
            LeftColumn("Email");
            LeftColumn("DiaChi");
        }

        private void CenterColumn(string columnName)
        {
            DataGridViewColumn? column = _dgvNhanVien.Columns[columnName];
            if (column is not null)
            {
                column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
        }

        private void LeftColumn(string columnName)
        {
            DataGridViewColumn? column = _dgvNhanVien.Columns[columnName];
            if (column is not null)
            {
                column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            }
        }

        private void SearchControl_Changed(object? sender, EventArgs e)
        {
            ApplySearchFilter();
        }

        private void ApplySearchFilter()
        {
            if (_nhanVienTable is null)
            {
                return;
            }

            string keyword = EscapeRowFilterValue(_txtTimKiem.Text.Trim());
            string selectedChucVu = _cboTimTheo.Text.Trim();
            if (string.IsNullOrEmpty(selectedChucVu))
            {
                selectedChucVu = "Tất cả";
            }

            List<string> filterConditions = new List<string>();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                filterConditions.Add($"(Convert(MaNV, 'System.String') LIKE '%{keyword}%' OR HoTen LIKE '%{keyword}%' OR SDT LIKE '%{keyword}%' OR Email LIKE '%{keyword}%')");
            }
            if (selectedChucVu != "Tất cả")
            {
                filterConditions.Add($"TenCV = '{EscapeRowFilterValue(selectedChucVu)}'");
            }
            if (filterConditions.Count > 0)
            {
                _nhanVienTable.DefaultView.RowFilter = string.Join(" AND ", filterConditions);
            }
            else
            {
                _nhanVienTable.DefaultView.RowFilter = string.Empty;
            }
        }

        private static string EscapeRowFilterValue(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            string escaped = value.Replace("'", "''").Replace("[", "[[]").Replace("%", "[%]").Replace("*", "[*]");
            return escaped.Replace("]", "[]]");
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
            _selectedMaNvDbValue = Convert.ToString(row.Cells["MaNV"].Value) ?? string.Empty;
            _txtMaNV.Text = FormatMaNvForDisplay(_selectedMaNvDbValue);
            _txtHoTen.Text = Convert.ToString(row.Cells["HoTen"].Value) ?? string.Empty;
            _txtSdt.Text = Convert.ToString(row.Cells["SDT"].Value) ?? string.Empty;
            textBox1.Text = Convert.ToString(row.Cells["Email"].Value) ?? string.Empty;
            _txtDiaChi.Text = Convert.ToString(row.Cells["DiaChi"].Value) ?? string.Empty;
            _txtMatKhau.Clear();
            _cboTrangThai.Text = Convert.ToString(row.Cells["TrangThai"].Value) ?? "Đang làm";

            if (DateTime.TryParse(Convert.ToString(row.Cells["NgaySinh"].Value), out DateTime ngaySinh))
            {
                _dtpNgaySinh.Value = ngaySinh;
            }

            string maCv = Convert.ToString(row.Cells["MaCV"].Value) ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(maCv))
            {
                _cboChucVu.SelectedValue = maCv;
            }

            _isEditingExisting = true;
            UpdatePasswordUiState();
        }

        private bool ValidateInput(bool isInsert)
        {
            if (!isInsert && string.IsNullOrWhiteSpace(_txtMaNV.Text))
            {
                MessageBox.Show("Không tạo được mã nhân viên tự động.", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(_txtHoTen.Text))
            {
                MessageBox.Show("Vui lòng nhập họ tên.", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(_txtSdt.Text))
            {
                MessageBox.Show("Vui lòng nhập số điện thoại.", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // Phone validation: digits only, exactly 10 digits
            string phone = _txtSdt.Text.Trim();
            if (!IsValidPhone(phone))
            {
                MessageBox.Show("Số điện thoại không hợp lệ. Vui lòng nhập đúng 10 chữ số.", "Dữ liệu sai", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (IsPhoneExists(isInsert, _txtSdt.Text.Trim()))
            {
                MessageBox.Show("Số điện thoại đã tồn tại trong hệ thống.", "Dữ liệu trùng lặp", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (_cboChucVu.SelectedValue is null)
            {
                MessageBox.Show("Vui lòng chọn chức vụ.", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // Email format validation (optional field but if provided must be valid)
            string emailValue = textBox1.Text.Trim();
            if (!string.IsNullOrWhiteSpace(emailValue) && !IsValidEmail(emailValue))
            {
                MessageBox.Show("Email phải có dạng ten@gmail.com.", "Dữ liệu sai", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (isInsert && string.IsNullOrWhiteSpace(_txtMatKhau.Text))
            {
                MessageBox.Show("Vui lòng nhập mật khẩu cho nhân viên mới.", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private bool IsPhoneExists(bool isInsert, string phone)
        {
            try
            {
                return _trangNhanVienService.IsPhoneExists(isInsert, phone, _selectedMaNvDbValue ?? _txtMaNV.Text.Trim());
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
                // ValidateInput shows messages; ensure we stop the add operation
                return;
            }

            // Double-check and show explicit messages if any format rules still fail
            string phoneToAdd = _txtSdt.Text.Trim();
            if (!IsValidPhone(phoneToAdd))
            {
                MessageBox.Show("Số điện thoại không hợp lệ. Vui lòng nhập đúng 10 chữ số.", "Dữ liệu sai", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtSdt.Focus();
                return;
            }
            string emailToAdd = textBox1.Text.Trim();
            if (!string.IsNullOrWhiteSpace(emailToAdd) && !IsValidEmail(emailToAdd))
            {
                MessageBox.Show("Email phải có dạng ten@gmail.com.", "Dữ liệu sai", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBox1.Focus();
                return;
            }
            try
            {
                _trangNhanVienService.AddNhanVien(
                    _txtHoTen.Text,
                    _dtpNgaySinh.Value.Date,
                    _txtSdt.Text,
                    textBox1.Text,
                    _txtDiaChi.Text,
                    _txtMatKhau.Text,
                    _cboTrangThai.Text,
                    _cboChucVu.SelectedValue?.ToString() ?? string.Empty);

                MessageBox.Show("Thêm nhân viên thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadNhanVien();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Thêm nhân viên thất bại.\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSua_Click(object? sender, EventArgs e)
        {
            if (!ValidateInput(false))
            {
                // ValidateInput shows messages; ensure we stop the edit operation
                return;
            }

            // Additional explicit format checks for edit operation with clear MessageBox feedback
            string phoneToEdit = _txtSdt.Text.Trim();
            if (!IsValidPhone(phoneToEdit))
            {
                MessageBox.Show("Số điện thoại không hợp lệ. Vui lòng nhập đúng 10 chữ số.", "Dữ liệu sai", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtSdt.Focus();
                return;
            }
            string emailToEdit = textBox1.Text.Trim();
            if (!string.IsNullOrWhiteSpace(emailToEdit) && !IsValidEmail(emailToEdit))
            {
                MessageBox.Show("Email phải có dạng ten@gmail.com.", "Dữ liệu sai", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBox1.Focus();
                return;
            }

            try
            {
                int rows = _trangNhanVienService.UpdateNhanVienAdmin(
                    _selectedMaNvDbValue ?? _txtMaNV.Text.Trim(),
                    _txtHoTen.Text,
                    _dtpNgaySinh.Value.Date,
                    _txtSdt.Text,
                    textBox1.Text,
                    _txtDiaChi.Text,
                    _cboTrangThai.Text,
                    _cboChucVu.SelectedValue?.ToString() ?? string.Empty,
                    _txtMatKhau.Text);

                if (rows == 0)
                {
                    MessageBox.Show("Không tìm thấy nhân viên để cập nhật.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                MessageBox.Show("Cập nhật nhân viên thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadNhanVien();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Cập nhật nhân viên thất bại.\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnXoa_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_txtMaNV.Text))
            {
                MessageBox.Show("Vui lòng chọn nhân viên cần xóa.", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult result = MessageBox.Show(
                "Bạn có chắc chắn muốn xóa nhân viên này?",
                "Xác nhận xóa",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
            {
                return;
            }

            try
            {
                int rows = _trangNhanVienService.DeleteNhanVien(_selectedMaNvDbValue ?? _txtMaNV.Text.Trim());

                if (rows == 0)
                {
                    MessageBox.Show("Không tìm thấy nhân viên để xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                MessageBox.Show("Xóa nhân viên thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadNhanVien();
                ClearForm();
            }
            catch (Exception ex) when (DataErrorHelper.IsForeignKeyViolation(ex))
            {
                MessageBox.Show("Không thể xóa nhân viên vì đang được sử dụng ở dữ liệu liên quan.", "Không thể xóa", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Xóa nhân viên thất bại.\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnLamMoi_Click(object? sender, EventArgs e)
        {
            ClearForm();
            LoadNhanVien();
        }

        private void ClearForm()
        {
            _selectedMaNvDbValue = null;
            _txtMaNV.Text = GenerateNextMaNhanVien();
            _txtHoTen.Clear();
            _txtSdt.Clear();
            textBox1.Clear();
            _txtDiaChi.Clear();
            _txtMatKhau.Clear();
            _cboTrangThai.SelectedIndex = 0;
            _isEditingExisting = false;
            UpdatePasswordUiState();

            if (_cboChucVu.Items.Count > 0)
            {
                _cboChucVu.SelectedIndex = 0;
            }

            _dtpNgaySinh.Value = DateTime.Today;
            _txtHoTen.Focus();
        }

        private string GenerateNextMaNhanVien()
        {
            try
            {
                return _trangNhanVienService.GenerateNextDisplayCode();
            }
            catch
            {
                return "NV1";
            }
        }

        private static string FormatMaNvForDisplay(string? maNvValue)
        {
            if (string.IsNullOrWhiteSpace(maNvValue))
            {
                return string.Empty;
            }

            string value = maNvValue.Trim();
            return value.StartsWith("NV", StringComparison.OrdinalIgnoreCase) ? value.ToUpperInvariant() : $"NV{value}";
        }

        private void label2_Click(object sender, EventArgs e)
        {
            try
            {
                using var frm = new HopThuYeuCauForm();
                frm.ShowDialog(this);
                UpdateInboxButtonBadge();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể mở hộp thư yêu cầu.\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void roundedPanel4_Paint(object sender, PaintEventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

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

        private void btn_DangXuat_MouseEnter(object sender, EventArgs e)
        {
            btn_DangXuat.BackColor = Color.FromArgb(255, 69, 0);
        }

        private void btn_DangXuat_MouseLeave(object sender, EventArgs e)
        {
            btn_DangXuat.BackColor = Color.LightSalmon;
        }

        private void roundedPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void btn_ThongKe_Paint(object sender, PaintEventArgs e)
        {

        }

        private void lblHoTen_Click(object sender, EventArgs e)
        {

        }

        private void lblTrangThai_Click(object sender, EventArgs e)
        {

        }

        private void lblDiaChi_Click(object sender, EventArgs e)
        {

        }

        private void _cboTrangThai_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void pnlSdtInput_Paint(object sender, PaintEventArgs e)
        {

        }

        private void lblNgaySinh_Click(object sender, EventArgs e)
        {

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

        private void btn_ThongKe_Click(object? sender, EventArgs e)
        {
            AdminNavigationManager.Navigate<ThongKe>(this);
        }

        private void btn_QLHDN_Click(object? sender, EventArgs e)
        {
            AdminNavigationManager.Navigate<QuanLiNguyenLieu>(this);
        }

        private void btn_QLHDB_Click(object? sender, EventArgs e)
        {
            AdminNavigationManager.Navigate<LichSuHoaDon>(this);
        }

        private void UpdatePasswordUiState()
        {
            bool isCreateMode = !_isEditingExisting;
            _btnDatLaiMatKhau.Visible = !isCreateMode;
            _btnXemLichTruc.Visible = !isCreateMode;

            _txtMatKhau.ReadOnly = !isCreateMode;
            _txtMatKhau.PlaceholderText = isCreateMode ? string.Empty : "Nhấn 'Đặt lại mật khẩu'";
        }

        private void BtnDatLaiMatKhau_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_txtMaNV.Text))
            {
                MessageBox.Show("Vui lòng chọn nhân viên cần đặt lại mật khẩu.", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!TryPromptNewPassword(out string newPassword))
            {
                return;
            }

            try
            {
                int rows = _trangNhanVienService.ResetPassword(_selectedMaNvDbValue ?? _txtMaNV.Text.Trim(), newPassword);
                if (rows == 0)
                {
                    MessageBox.Show("Không tìm thấy nhân viên để đặt lại mật khẩu.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                MessageBox.Show("Đặt lại mật khẩu thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Đặt lại mật khẩu thất bại.\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static bool TryPromptNewPassword(out string password)
        {
            password = string.Empty;

            using Form dialog = new Form
            {
                Text = "Đặt lại mật khẩu",
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MinimizeBox = false,
                MaximizeBox = false,
                ClientSize = new Size(320, 140)
            };

            Label lblNew = new Label { Text = "Mật khẩu mới", Left = 12, Top = 15, AutoSize = true };
            TextBox txtNew = new TextBox { Left = 120, Top = 12, Width = 180, UseSystemPasswordChar = true };
            Label lblConfirm = new Label { Text = "Xác nhận", Left = 12, Top = 50, AutoSize = true };
            TextBox txtConfirm = new TextBox { Left = 120, Top = 47, Width = 180, UseSystemPasswordChar = true };
            Button btnOk = new Button { Text = "OK", Left = 144, Top = 92, Width = 75, DialogResult = DialogResult.OK };
            Button btnCancel = new Button { Text = "Hủy", Left = 225, Top = 92, Width = 75, DialogResult = DialogResult.Cancel };

            dialog.Controls.Add(lblNew);
            dialog.Controls.Add(txtNew);
            dialog.Controls.Add(lblConfirm);
            dialog.Controls.Add(txtConfirm);
            dialog.Controls.Add(btnOk);
            dialog.Controls.Add(btnCancel);
            dialog.AcceptButton = btnOk;
            dialog.CancelButton = btnCancel;

            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return false;
            }

            string newPassword = txtNew.Text.Trim();
            string confirmPassword = txtConfirm.Text.Trim();

            if (string.IsNullOrWhiteSpace(newPassword))
            {
                MessageBox.Show("Mật khẩu mới không được để trống.", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (newPassword != confirmPassword)
            {
                MessageBox.Show("Mật khẩu xác nhận không khớp.", "Dữ liệu sai", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            password = newPassword;
            return true;
        }

        private void BtnXemLichTruc_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_txtMaNV.Text))
            {
                MessageBox.Show("Vui lòng chọn nhân viên để xem lịch trực.", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            const decimal donGiaCaTruc = 176000m;
            string maNv = _selectedMaNvDbValue ?? _txtMaNV.Text.Trim();
            string tenNv = _txtHoTen.Text.Trim();

            try
            {
                using Form scheduleForm = new Form
                {
                    Text = $"Lịch trực - NV {_txtMaNV.Text}",
                    StartPosition = FormStartPosition.CenterParent,
                    Size = new Size(950, 550),
                    MinimizeBox = false,
                    MaximizeBox = false
                };

                Panel pnlSummary = new Panel
                {
                    Dock = DockStyle.Bottom,
                    Height = 52,
                    BackColor = Color.FromArgb(248, 242, 235)
                };

                Panel pnlActionButtons = new Panel
                {
                    Dock = DockStyle.Right,
                    Width = 190,
                    BackColor = Color.FromArgb(248, 242, 235)
                };

                Label lblTongSoCa = new Label
                {
                    AutoSize = true,
                    Location = new Point(12, 17),
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold)
                };

                Label lblTongHeSo = new Label
                {
                    AutoSize = true,
                    Location = new Point(210, 17),
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold)
                };

                Label lblLuongDuKien = new Label
                {
                    AutoSize = true,
                    Location = new Point(380, 17),
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold)
                };

                DataGridView dgv = new DataGridView
                {
                    Dock = DockStyle.Fill,
                    ReadOnly = true,
                    AllowUserToAddRows = false,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                    SelectionMode = DataGridViewSelectionMode.FullRowSelect
                };
                dgv.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
                dgv.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;

                dgv.CellFormatting += (_, e) =>
                {
                    if (dgv.Columns[e.ColumnIndex].Name is "GioBatDau" or "GioKetThuc")
                    {
                        if (e.Value is TimeSpan ts)
                        {
                            e.Value = ts.ToString(@"hh\:mm");
                            e.FormattingApplied = true;
                        }
                        else if (e.Value is DateTime dt)
                        {
                            e.Value = dt.ToString("HH:mm");
                            e.FormattingApplied = true;
                        }
                    }
                    else if (dgv.Columns[e.ColumnIndex].Name == "TinhTrangCa")
                    {
                        if (dgv.Rows[e.RowIndex].DataBoundItem is DataRowView rowView)
                        {
                            int thucTe = rowView.Row.Field<int>("SoNguoiThucTe");
                            int toiThieu = rowView.Row.Field<int>("SoNguoiToiThieu");
                            bool du = thucTe >= toiThieu;
                            e.Value = du ? "Đủ người" : "Thiếu người";
                            dgv.Rows[e.RowIndex].DefaultCellStyle.BackColor = du ? Color.Honeydew : Color.MistyRose;
                            dgv.Rows[e.RowIndex].DefaultCellStyle.ForeColor = du ? Color.DarkGreen : Color.DarkRed;
                            e.FormattingApplied = true;
                        }
                    }
                };

                // Now define local functions that use the controls
                DataTable BuildScheduleTable(DataTable source)
                {
                    DataTable dt = source.Copy();
                    DataTable caTrucTable = _trangNhanVienService.GetCaTruc();
                    var minByCa = caTrucTable.AsEnumerable().ToDictionary(r => Convert.ToString(r["MaCa"]) ?? string.Empty, r => Convert.ToInt32(r["SoNguoiToiThieu"]));

                    DataTable staffingCounts = _trangNhanVienService.GetStaffingCounts(DateTime.Today.AddMonths(-2), DateTime.Today.AddMonths(2));
                    var staffingByDate = new Dictionary<DateTime, (int Sang, int Chieu, int Toi, int Full)>();
                    foreach (DataRow row in staffingCounts.Rows)
                    {
                        DateTime ngay = Convert.ToDateTime(row["NgayLam"]).Date;
                        int sang = row["SoSang"] == DBNull.Value ? 0 : Convert.ToInt32(row["SoSang"]);
                        int chieu = row["SoChieu"] == DBNull.Value ? 0 : Convert.ToInt32(row["SoChieu"]);
                        int toi = row["SoToi"] == DBNull.Value ? 0 : Convert.ToInt32(row["SoToi"]);
                        int full = row["SoFull"] == DBNull.Value ? 0 : Convert.ToInt32(row["SoFull"]);
                        staffingByDate[ngay] = (sang, chieu, toi, full);
                    }

                    if (!dt.Columns.Contains("SoNguoiToiThieu"))
                    {
                        dt.Columns.Add("SoNguoiToiThieu", typeof(int));
                    }
                    if (!dt.Columns.Contains("SoNguoiThucTe"))
                    {
                        dt.Columns.Add("SoNguoiThucTe", typeof(int));
                    }

                    foreach (DataRow row in dt.Rows)
                    {
                        string maCaRow = Convert.ToString(row["MaCa"]) ?? string.Empty;
                        DateTime ngay = Convert.ToDateTime(row["NgayLam"]).Date;
                        int min = minByCa.TryGetValue(maCaRow, out int val) ? val : 0;
                        int thucTe = 0;
                        if (staffingByDate.TryGetValue(ngay, out var counts))
                        {
                            thucTe = maCaRow switch
                            {
                                "1" => counts.Sang + counts.Full,
                                "2" => counts.Chieu + counts.Full,
                                "3" => counts.Toi,
                                "4" => counts.Full,
                                _ => 0
                            };
                        }
                        row["SoNguoiToiThieu"] = min;
                        row["SoNguoiThucTe"] = thucTe;
                    }

                    return dt;
                }

                void ReloadSchedule()
                {
                    DataTable dt = _trangNhanVienService.GetLichTruc(maNv);
                    decimal tongHeSo = dt.Rows.Count > 0 ? dt.AsEnumerable().Sum(r => Convert.ToDecimal(r["HeSoLuong"])) : 0m;
                    int tongSoCa = dt.Rows.Count;
                    _lastLuongDuKien = tongHeSo * donGiaCaTruc;

                    DataTable displayDt = BuildScheduleTable(dt);
                    dgv.DataSource = displayDt;

                    if (dgv.Columns["NgayLam"] is not null)
                    {
                        dgv.Columns["NgayLam"].HeaderText = "Ngày làm";
                        dgv.Columns["NgayLam"].DefaultCellStyle.Format = "dd/MM/yyyy";
                    }

                    if (dgv.Columns["GioBatDau"] is not null)
                    {
                        dgv.Columns["GioBatDau"].HeaderText = "Giờ bắt đầu";
                    }

                    if (dgv.Columns["GioKetThuc"] is not null)
                    {
                        dgv.Columns["GioKetThuc"].HeaderText = "Giờ kết thúc";
                    }

                    if (dgv.Columns["HeSoLuong"] is not null)
                    {
                        dgv.Columns["HeSoLuong"].HeaderText = "Hệ số lương";
                        dgv.Columns["HeSoLuong"].DefaultCellStyle.Format = "N2";
                    }

                    if (dgv.Columns["MaCa"] is not null)
                    {
                        dgv.Columns["MaCa"].Visible = false;
                    }

                    if (dgv.Columns["TenCa"] is not null)
                    {
                        dgv.Columns["TenCa"].HeaderText = "Tên ca";
                    }

                    if (dgv.Columns["SoNguoiToiThieu"] is not null)
                    {
                        dgv.Columns["SoNguoiToiThieu"].Visible = false;
                    }

                    if (dgv.Columns["SoNguoiThucTe"] is not null)
                    {
                        dgv.Columns["SoNguoiThucTe"].Visible = false;
                    }

                    if (dgv.Columns["TinhTrangCa"] is null)
                    {
                        dgv.Columns.Add(new DataGridViewTextBoxColumn
                        {
                            Name = "TinhTrangCa",
                            HeaderText = "Tình trạng ca",
                            AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
                        });
                    }

                    lblTongSoCa.Text = $"Tổng số ca làm: {tongSoCa}";
                    lblTongHeSo.Text = $"Tổng hệ số: {tongHeSo:N2}";
                    lblLuongDuKien.Text = $"Tiền lương theo ca: {_lastLuongDuKien:N0} đ";
                }

                void FilterScheduleByPeriod(string filterType)
                {
                    DataTable dt = _trangNhanVienService.GetLichTruc(maNv);
                    DateTime today = DateTime.Today;
                    int todayDayOfWeek = (int)today.DayOfWeek;
                    DateTime startOfWeek = today.AddDays(-todayDayOfWeek);
                    DateTime startOfMonth = new DateTime(today.Year, today.Month, 1);

                    DataView dv = dt.DefaultView;

                    switch (filterType)
                    {
                        case "Tháng này":
                            dv.RowFilter = $"NgayLam >= '{startOfMonth:yyyy-MM-dd}' AND NgayLam <= '{new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month)):yyyy-MM-dd}'";
                            break;
                        case "Tháng trước":
                            DateTime lastMonth = today.AddMonths(-1);
                            DateTime firstDayLastMonth = new DateTime(lastMonth.Year, lastMonth.Month, 1);
                            DateTime lastDayLastMonth = new DateTime(lastMonth.Year, lastMonth.Month, DateTime.DaysInMonth(lastMonth.Year, lastMonth.Month));
                            dv.RowFilter = $"NgayLam >= '{firstDayLastMonth:yyyy-MM-dd}' AND NgayLam <= '{lastDayLastMonth:yyyy-MM-dd}'";
                            break;
                        case "Tháng sau":
                            DateTime nextMonth = today.AddMonths(1);
                            DateTime firstDayNextMonth = new DateTime(nextMonth.Year, nextMonth.Month, 1);
                            DateTime lastDayNextMonth = new DateTime(nextMonth.Year, nextMonth.Month, DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month));
                            dv.RowFilter = $"NgayLam >= '{firstDayNextMonth:yyyy-MM-dd}' AND NgayLam <= '{lastDayNextMonth:yyyy-MM-dd}'";
                            break;
                        case "Tuần này":
                            DateTime endOfWeek = startOfWeek.AddDays(6);
                            dv.RowFilter = $"NgayLam >= '{startOfWeek:yyyy-MM-dd}' AND NgayLam <= '{endOfWeek:yyyy-MM-dd}'";
                            break;
                        case "Tuần trước":
                            DateTime startOfLastWeek = startOfWeek.AddDays(-7);
                            DateTime endOfLastWeek = startOfLastWeek.AddDays(6);
                            dv.RowFilter = $"NgayLam >= '{startOfLastWeek:yyyy-MM-dd}' AND NgayLam <= '{endOfLastWeek:yyyy-MM-dd}'";
                            break;
                        case "Tuần sau":
                            DateTime startOfNextWeek = startOfWeek.AddDays(7);
                            DateTime endOfNextWeek = startOfNextWeek.AddDays(6);
                            dv.RowFilter = $"NgayLam >= '{startOfNextWeek:yyyy-MM-dd}' AND NgayLam <= '{endOfNextWeek:yyyy-MM-dd}'";
                            break;
                        default:
                            dv.RowFilter = "";
                            break;
                    }

                    DataTable filteredDt = dv.ToTable();
                    decimal tongHeSo = filteredDt.Rows.Count > 0 ? filteredDt.AsEnumerable().Sum(r => Convert.ToDecimal(r["HeSoLuong"])) : 0m;
                    int tongSoCa = filteredDt.Rows.Count;
                    decimal luongDuKien = tongHeSo * donGiaCaTruc;

                    DataTable displayDt = BuildScheduleTable(filteredDt);
                    dgv.DataSource = displayDt;

                    lblTongSoCa.Text = $"Tổng số ca làm: {tongSoCa}";
                    lblTongHeSo.Text = $"Tổng hệ số: {tongHeSo:N2}";
                    lblLuongDuKien.Text = $"Tiền lương theo ca: {luongDuKien:N0} đ";
                }

                void OpenAddShiftPopup()
                {
                    using Form addShiftForm = new Form
                    {
                        Text = $"Thêm ca làm - {tenNv}",
                        FormBorderStyle = FormBorderStyle.FixedDialog,
                        StartPosition = FormStartPosition.CenterParent,
                        MinimizeBox = false,
                        MaximizeBox = false,
                        ClientSize = new Size(440, 400)
                    };

                    Label lblEmployee = new Label
                    {
                        Text = $"Nhân viên: {tenNv}",
                        Left = 12,
                        Top = 15,
                        AutoSize = true,
                        Font = new Font("Segoe UI", 9F, FontStyle.Bold)
                    };

                    // --- ComboBox chọn ca ---
                    Label lblShift = new Label { Text = "Chọn ca:", Left = 12, Top = 45, AutoSize = true };
                    ComboBox cboShift = new ComboBox
                    {
                        Left = 90,
                        Top = 42,
                        Width = 330,
                        DropDownStyle = ComboBoxStyle.DropDownList
                    };

                    DataTable caTrucTable = _trangNhanVienService.GetCaTruc();

                    // --- ComboBox chọn tuần (số tuần reset theo tháng) ---
                    Label lblTuan = new Label { Text = "Chọn tuần:", Left = 12, Top = 75, AutoSize = true };
                    ComboBox cboTuan = new ComboBox
                    {
                        Left = 90,
                        Top = 72,
                        Width = 330,
                        DropDownStyle = ComboBoxStyle.DropDownList
                    };

                    // Tạo danh sách tuần: tháng hiện tại + tháng sau, số tuần reset theo tháng
                    DateTime today = DateTime.Today;
                    var weekItems = new List<(string display, DateTime monday, DateTime sunday)>();

                    for (int monthOffset = 0; monthOffset <= 1; monthOffset++)
                    {
                        DateTime targetMonth = today.AddMonths(monthOffset);
                        int year = targetMonth.Year;
                        int month = targetMonth.Month;
                        DateTime firstDay = new DateTime(year, month, 1);
                        DateTime lastDay = new DateTime(year, month, DateTime.DaysInMonth(year, month));

                        // Tìm Thứ 2 đầu tiên >= ngày 1 của tháng
                        DateTime monday = firstDay;
                        while (monday.DayOfWeek != DayOfWeek.Monday)
                        {
                            monday = monday.AddDays(1);
                        }

                        // Nếu tháng bắt đầu không phải Thứ 2, thêm tuần đầu tiên (bắt đầu từ ngày 1)
                        if (firstDay.DayOfWeek != DayOfWeek.Monday)
                        {
                            DateTime firstSunday = monday.AddDays(-1);
                            if (firstSunday > lastDay) firstSunday = lastDay;
                            weekItems.Add(($"Tuần 1 (Từ {firstDay:dd/MM} đến {firstSunday:dd/MM})", firstDay, firstSunday));
                        }

                        int weekNum = firstDay.DayOfWeek == DayOfWeek.Monday ? 1 : 2;
                        while (monday <= lastDay)
                        {
                            DateTime sunday = monday.AddDays(6);
                            if (sunday > lastDay) sunday = lastDay;
                            weekItems.Add(($"Tuần {weekNum} (Từ {monday:dd/MM} đến {sunday:dd/MM})", monday, sunday));
                            weekNum++;
                            monday = monday.AddDays(7);
                        }
                    }

                    foreach (var item in weekItems)
                    {
                        cboTuan.Items.Add(item.display);
                    }

                    // Mặc định chọn tuần chứa ngày hôm nay
                    int defaultIndex = 0;
                    for (int i = 0; i < weekItems.Count; i++)
                    {
                        if (today >= weekItems[i].monday && today <= weekItems[i].sunday)
                        {
                            defaultIndex = i;
                            break;
                        }
                    }
                    if (cboTuan.Items.Count > 0)
                    {
                        cboTuan.SelectedIndex = defaultIndex;
                    }

                    // --- Hướng dẫn ---
                    Label lblNote = new Label
                    {
                        Text = "(*) Click vào từng thứ để xem số lượng người:",
                        Left = 12,
                        Top = 105,
                        AutoSize = true,
                        Font = new Font("Segoe UI", 8F, FontStyle.Italic)
                    };

                    // --- Panel chứa CheckBox các thứ ---
                    Panel pnlDays = new Panel
                    {
                        Left = 12,
                        Top = 128,
                        Width = 408,
                        Height = 180,
                        BorderStyle = BorderStyle.FixedSingle,
                        AutoScroll = false
                    };

                    string[] dayNames = { "Thứ 2", "Thứ 3", "Thứ 4", "Thứ 5", "Thứ 6", "Thứ 7", "Chủ nhật" };
                    CheckBox[] chkDays = new CheckBox[7];

                    for (int i = 0; i < 7; i++)
                    {
                        chkDays[i] = new CheckBox
                        {
                            Text = dayNames[i],
                            Left = 10,
                            Top = 4 + i * 24,
                            Width = 380,
                            AutoSize = false,
                            Height = 22
                        };
                        pnlDays.Controls.Add(chkDays[i]);
                    }

                    // --- Label sức chứa real-time ---
                    Label lblStaffing = new Label
                    {
                        Left = 12,
                        Top = 315,
                        Width = 408,
                        AutoSize = false,
                        Height = 20,
                        ForeColor = Color.DarkSlateBlue,
                        Font = new Font("Segoe UI", 8.5F, FontStyle.Italic),
                        Text = ""
                    };

                    // --- Nút Thêm / Thoát ---
                    Button btnOk = new Button
                    {
                        Text = "Thêm",
                        Left = 260,
                        Top = 345,
                        Width = 75,
                        Height = 28,
                        DialogResult = DialogResult.OK
                    };

                    Button btnCancel = new Button
                    {
                        Text = "Thoát",
                        Left = 345,
                        Top = 345,
                        Width = 75,
                        Height = 28,
                        DialogResult = DialogResult.Cancel
                    };

                    addShiftForm.Controls.Add(lblEmployee);
                    addShiftForm.Controls.Add(lblShift);
                    addShiftForm.Controls.Add(cboShift);
                    addShiftForm.Controls.Add(lblTuan);
                    addShiftForm.Controls.Add(cboTuan);
                    addShiftForm.Controls.Add(lblNote);
                    addShiftForm.Controls.Add(pnlDays);
                    addShiftForm.Controls.Add(lblStaffing);
                    addShiftForm.Controls.Add(btnOk);
                    addShiftForm.Controls.Add(btnCancel);
                    addShiftForm.AcceptButton = btnOk;
                    addShiftForm.CancelButton = btnCancel;

                    // Bind dữ liệu ca trực SAU KHI controls đã được thêm vào form
                    cboShift.DataSource = caTrucTable;
                    cboShift.DisplayMember = "TenCa";
                    cboShift.ValueMember = "MaCa";
                    if (caTrucTable.Rows.Count > 0)
                    {
                        cboShift.SelectedIndex = 0;
                    }

                    // === Hàm cập nhật text checkbox theo tuần đang chọn ===
                    void UpdateCheckBoxDates()
                    {
                        int idx = cboTuan.SelectedIndex;
                        if (idx < 0 || idx >= weekItems.Count) return;

                        DateTime weekStart = weekItems[idx].monday;
                        DateTime weekEnd = weekItems[idx].sunday;

                        for (int i = 0; i < 7; i++)
                        {
                            // Tính ngày cụ thể cho thứ i (Thứ 2 = 0, ..., Chủ nhật = 6)
                            DateTime dayDate;
                            if (weekStart.DayOfWeek == DayOfWeek.Monday)
                            {
                                dayDate = weekStart.AddDays(i);
                            }
                            else
                            {
                                // Tuần lẻ đầu tháng (bắt đầu không phải Thứ 2)
                                // Tính từ ngày Thứ 2 trước weekStart
                                int offset = ((int)weekStart.DayOfWeek - 1 + 7) % 7;
                                DateTime virtualMonday = weekStart.AddDays(-offset);
                                dayDate = virtualMonday.AddDays(i);
                            }

                            bool isInRange = dayDate >= weekStart && dayDate <= weekEnd;
                            chkDays[i].Text = isInRange
                                ? $"{dayNames[i]} ({dayDate:dd/MM})"
                                : $"{dayNames[i]}";
                            chkDays[i].Enabled = isInRange && dayDate >= today;
                            chkDays[i].Tag = isInRange ? dayDate : (object?)null;

                            if (!isInRange || dayDate < today)
                            {
                                chkDays[i].Checked = false;
                            }
                        }

                        lblStaffing.Text = "";
                    }

                    // === Hàm kiểm tra sức chứa real-time cho 1 ngày cụ thể ===
                    void KiemTraDinhBienChoNgay(DateTime ngay)
                    {
                        if (cboShift.SelectedValue is null) return;
                        string maCaCheck = cboShift.SelectedValue.ToString() ?? string.Empty;

                        try
                        {
                            var (thucTe, toiThieu) = _trangNhanVienService.GetStaffingCountForDateAndShift(ngay, maCaCheck);
                            string warning = "";
                            if (toiThieu > 0 && thucTe >= toiThieu)
                            {
                                warning = " (Đã đầy!)";
                                lblStaffing.ForeColor = Color.OrangeRed;
                            }
                            else if (toiThieu > 0 && thucTe >= toiThieu - 1)
                            {
                                warning = " (Sắp đầy!)";
                                lblStaffing.ForeColor = Color.DarkOrange;
                            }
                            else
                            {
                                lblStaffing.ForeColor = Color.DarkSlateBlue;
                            }

                            lblStaffing.Text = $"Ngày {ngay:dd/MM/yyyy} → Ca này hiện có {thucTe}/{toiThieu} người{warning}";
                        }
                        catch
                        {
                            lblStaffing.Text = "";
                        }
                    }

                    // === Đăng ký sự kiện ===
                    cboTuan.SelectedIndexChanged += (_, __) => UpdateCheckBoxDates();

                    cboShift.SelectedIndexChanged += (_, __) =>
                    {
                        UpdateCheckBoxDates();
                    };

                    for (int i = 0; i < 7; i++)
                    {
                        int capturedIndex = i;
                        chkDays[i].MouseEnter += (_, __) =>
                        {
                            if (chkDays[capturedIndex].Tag is DateTime d)
                            {
                                KiemTraDinhBienChoNgay(d);
                            }
                        };
                        chkDays[i].CheckedChanged += (_, __) =>
                        {
                            if (chkDays[capturedIndex].Tag is DateTime d)
                            {
                                KiemTraDinhBienChoNgay(d);
                            }
                        };
                    }

                    // Khởi tạo ban đầu
                    UpdateCheckBoxDates();

                    // === Xử lý nút Thêm ===
                    if (addShiftForm.ShowDialog(null) == DialogResult.OK)
                    {
                        string? maCa = cboShift.SelectedValue?.ToString();
                        if (string.IsNullOrWhiteSpace(maCa))
                        {
                            MessageBox.Show("Vui lòng chọn ca làm.", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        bool anyDaySelected = false;
                        for (int i = 0; i < 7; i++)
                        {
                            if (chkDays[i].Checked)
                            {
                                anyDaySelected = true;
                                break;
                            }
                        }

                        if (!anyDaySelected)
                        {
                            MessageBox.Show("Vui lòng chọn ít nhất một ngày.", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        try
                        {
                            int addedCount = 0;
                            int duplicateCount = 0;
                            int overlapCount = 0;

                            for (int i = 0; i < 7; i++)
                            {
                                if (chkDays[i].Checked && chkDays[i].Tag is DateTime ngayCuThe)
                                {
                                    int result = _trangNhanVienService.AddPhanCongCa(maNv, maCa, ngayCuThe);
                                    if (result > 0)
                                    {
                                        addedCount++;
                                    }
                                    else if (result == -1)
                                    {
                                        overlapCount++;
                                    }
                                    else
                                    {
                                        duplicateCount++;
                                    }
                                }
                            }

                            if (overlapCount > 0)
                            {
                                MessageBox.Show($"Đã thêm {addedCount} lịch. ({overlapCount} lịch bị trùng giờ)", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else if (duplicateCount > 0)
                            {
                                MessageBox.Show($"Đã thêm {addedCount} lịch. ({duplicateCount} lịch bị trùng lặp)", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else if (addedCount > 0)
                            {
                                MessageBox.Show($"Đã thêm {addedCount} lịch thành công.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                MessageBox.Show("Không có lịch nào được thêm (tất cả bị trùng lặp).", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }

                            ReloadSchedule();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Không thể thêm lịch làm.\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }

                Button btnThoat = new Button
                {
                    Text = "Thoát",
                    Size = new Size(70, 24),
                    FlatStyle = FlatStyle.System,
                    Location = new Point(12, 14)
                };
                btnThoat.Click += (_, _) => scheduleForm.Close();

                Button btnThemCa = new Button
                {
                    Text = "Thêm ca làm",
                    Size = new Size(92, 24),
                    FlatStyle = FlatStyle.System,
                    Location = new Point(92, 14)
                };
                btnThemCa.Click += (_, _) => OpenAddShiftPopup();

                pnlActionButtons.Controls.Add(btnThoat);
                pnlActionButtons.Controls.Add(btnThemCa);

                pnlSummary.Controls.Add(lblTongSoCa);
                pnlSummary.Controls.Add(lblTongHeSo);
                pnlSummary.Controls.Add(lblLuongDuKien);
                pnlSummary.Controls.Add(pnlActionButtons);

                Panel pnlFilter = new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 35,
                    BackColor = Color.FromArgb(248, 242, 235),
                    Padding = new Padding(12, 5, 12, 5)
                };

                Label lblFilter = new Label
                {
                    Text = "Lọc:",
                    AutoSize = true,
                    Location = new Point(12, 8),
                    Font = new Font("Segoe UI", 9F)
                };

                ComboBox cboFilter = new ComboBox
                {
                    Left = 50,
                    Top = 5,
                    Width = 180,
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Items = { "Tất cả", "Tháng này", "Tháng trước", "Tháng sau", "Tuần này", "Tuần trước", "Tuần sau" }
                };
                cboFilter.SelectedIndex = 0;
                cboFilter.SelectedIndexChanged += (_, _) =>
                {
                    if (cboFilter.SelectedItem is string selected && selected != "Tất cả")
                    {
                        FilterScheduleByPeriod(selected);
                    }
                    else
                    {
                        ReloadSchedule();
                    }
                };

                pnlFilter.Controls.Add(lblFilter);
                pnlFilter.Controls.Add(cboFilter);

                scheduleForm.Controls.Add(dgv);
                scheduleForm.Controls.Add(pnlFilter);
                scheduleForm.Controls.Add(pnlSummary);
                ReloadSchedule();
                scheduleForm.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể tải lịch trực.\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateInboxButtonBadge()
        {
            try
            {
                int pending = _trangNhanVienService.GetPendingYeuCauCount();
                label2.Text = pending > 0
                    ? $"Hộp thư yêu cầu ({pending})"
                    : "Hộp thư yêu cầu (0)";
            }
            catch
            {
                label2.Text = "Hộp thư yêu cầu (0)";
            }
        }

        private void _txtMatKhau_TextChanged(object sender, EventArgs e)
        {
        }

        private void _dgvNhanVien_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }
    }
}
