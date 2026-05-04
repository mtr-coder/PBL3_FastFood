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

            string keyword = _txtTimKiem.Text.Trim().Replace("'", "''");
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
                filterConditions.Add($"TenCV = '{selectedChucVu.Replace("'", "''")}'");
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
                    Size = new Size(760, 460),
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

                Button btnThoat = new Button
                {
                    Text = "Thoát",
                    Size = new Size(70, 24),
                    FlatStyle = FlatStyle.System,
                    Location = new Point(12, 14)
                };

                Button btnThemCa = new Button
                {
                    Text = "Thêm ca làm",
                    Size = new Size(92, 24),
                    FlatStyle = FlatStyle.System,
                    Location = new Point(92, 14)
                };

                pnlActionButtons.Controls.Add(btnThoat);
                pnlActionButtons.Controls.Add(btnThemCa);

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

                pnlSummary.Controls.Add(lblTongSoCa);
                pnlSummary.Controls.Add(lblTongHeSo);
                pnlSummary.Controls.Add(lblLuongDuKien);
                pnlSummary.Controls.Add(pnlActionButtons);

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
                };

                void ReloadSchedule()
                {
                    DataTable dt = _trangNhanVienService.GetLichTruc(maNv);
                    decimal tongHeSo = dt.Rows.Count > 0 ? dt.AsEnumerable().Sum(r => Convert.ToDecimal(r["HeSoLuong"])) : 0m;
                    int tongSoCa = dt.Rows.Count;
                    _lastLuongDuKien = tongHeSo * donGiaCaTruc;

                    dgv.DataSource = dt;

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

                    lblTongSoCa.Text = $"Tổng số ca làm: {tongSoCa}";
                    lblTongHeSo.Text = $"Tổng hệ số: {tongHeSo:N2}";
                    lblLuongDuKien.Text = $"Tiền lương theo ca: {_lastLuongDuKien:N0} đ";
                }

                void OpenAddShiftPopup()
                {
                    using Form addForm = new Form
                    {
                        Text = "Thêm ca làm",
                        StartPosition = FormStartPosition.CenterParent,
                        FormBorderStyle = FormBorderStyle.FixedDialog,
                        MinimizeBox = false,
                        MaximizeBox = false,
                        ClientSize = new Size(540, 255)
                    };

                    Panel addBottomBar = new Panel
                    {
                        Dock = DockStyle.Bottom,
                        Height = 38,
                        BackColor = Color.FromArgb(248, 242, 235)
                    };

                    Panel addButtonArea = new Panel
                    {
                        Dock = DockStyle.Right,
                        Width = 190,
                        BackColor = Color.FromArgb(248, 242, 235)
                    };

                    Button btnThoatAdd = new Button
                    {
                        Text = "Thoát",
                        Size = new Size(70, 24),
                        Location = new Point(12, 7)
                    };
                    Button btnLuu = new Button
                    {
                        Text = "Lưu",
                        Size = new Size(75, 24),
                        Location = new Point(95, 7),
                        DialogResult = DialogResult.OK
                    };
                    btnThoatAdd.Click += (_, _) => addForm.Close();
                    addButtonArea.Controls.Add(btnThoatAdd);
                    addButtonArea.Controls.Add(btnLuu);
                    addBottomBar.Controls.Add(addButtonArea);

                    Label lblNhanVien = new Label { Left = 18, Top = 20, AutoSize = true, Text = "Nhân viên" };
                    TextBox txtNhanVien = new TextBox
                    {
                        Left = 110,
                        Top = 16,
                        Width = 250,
                        ReadOnly = true,
                        Text = !string.IsNullOrWhiteSpace(tenNv) ? tenNv : _txtMaNV.Text
                    };

                    Label lblCa = new Label { Left = 18, Top = 56, AutoSize = true, Text = "Ca làm" };
                    ComboBox cboCa = new ComboBox
                    {
                        Left = 110,
                        Top = 52,
                        Width = 250,
                        DropDownStyle = ComboBoxStyle.DropDownList,
                        DisplayMember = "TenCa",
                        ValueMember = "MaCa",
                        DataSource = _trangNhanVienService.GetCaTruc()
                    };

                    Label lblThu = new Label { Left = 18, Top = 92, AutoSize = true, Text = "Chọn thứ" };
                    CheckBox cb2 = new CheckBox { Left = 110, Top = 90, AutoSize = true, Text = "Thứ 2" };
                    CheckBox cb3 = new CheckBox { Left = 180, Top = 90, AutoSize = true, Text = "Thứ 3" };
                    CheckBox cb4 = new CheckBox { Left = 250, Top = 90, AutoSize = true, Text = "Thứ 4" };
                    CheckBox cb5 = new CheckBox { Left = 320, Top = 90, AutoSize = true, Text = "Thứ 5" };
                    CheckBox cb6 = new CheckBox { Left = 390, Top = 90, AutoSize = true, Text = "Thứ 6" };
                    CheckBox cb7 = new CheckBox { Left = 110, Top = 120, AutoSize = true, Text = "Thứ 7" };
                    CheckBox cbCN = new CheckBox { Left = 180, Top = 120, AutoSize = true, Text = "Chủ nhật" };

                    Label lblInfo = new Label
                    {
                        Left = 18,
                        Top = 155,
                        AutoSize = true,
                        ForeColor = Color.DimGray,
                        Text = "Lịch sẽ được tự động thêm từ hôm nay đến hết tháng theo các thứ đã chọn."
                    };

                    addForm.Controls.Add(lblNhanVien);
                    addForm.Controls.Add(txtNhanVien);
                    addForm.Controls.Add(lblCa);
                    addForm.Controls.Add(cboCa);
                    addForm.Controls.Add(lblThu);
                    addForm.Controls.Add(cb2);
                    addForm.Controls.Add(cb3);
                    addForm.Controls.Add(cb4);
                    addForm.Controls.Add(cb5);
                    addForm.Controls.Add(cb6);
                    addForm.Controls.Add(cb7);
                    addForm.Controls.Add(cbCN);
                    addForm.Controls.Add(lblInfo);
                    addForm.Controls.Add(addBottomBar);
                    addForm.AcceptButton = btnLuu;
                    addForm.CancelButton = btnThoatAdd;

                    if (addForm.ShowDialog(scheduleForm) != DialogResult.OK)
                    {
                        return;
                    }

                    if (cboCa.SelectedValue is null)
                    {
                        MessageBox.Show("Vui lòng chọn ca làm.", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    List<DayOfWeek> selectedDays = new List<DayOfWeek>();
                    if (cb2.Checked) selectedDays.Add(DayOfWeek.Monday);
                    if (cb3.Checked) selectedDays.Add(DayOfWeek.Tuesday);
                    if (cb4.Checked) selectedDays.Add(DayOfWeek.Wednesday);
                    if (cb5.Checked) selectedDays.Add(DayOfWeek.Thursday);
                    if (cb6.Checked) selectedDays.Add(DayOfWeek.Friday);
                    if (cb7.Checked) selectedDays.Add(DayOfWeek.Saturday);
                    if (cbCN.Checked) selectedDays.Add(DayOfWeek.Sunday);

                    if (selectedDays.Count == 0)
                    {
                        MessageBox.Show("Vui lòng chọn ít nhất một thứ trong tuần.", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    try
                    {
                        DateTime from = DateTime.Today;
                        DateTime to = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month));
                        string maCa = Convert.ToString(cboCa.SelectedValue) ?? string.Empty;
                        int inserted = 0;

                        for (DateTime day = from; day <= to; day = day.AddDays(1))
                        {
                            if (!selectedDays.Contains(day.DayOfWeek))
                            {
                                continue;
                            }

                            inserted += _trangNhanVienService.AddPhanCongCa(maNv, maCa, day);
                        }

                        if (inserted > 0)
                        {
                            MessageBox.Show($"Đã thêm {inserted} lịch trong tháng này.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            ReloadSchedule();
                        }
                        else
                        {
                            MessageBox.Show("Đã có lịch rồi.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Không thể thêm ca làm.\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }

                btnThoat.Click += (_, _) => scheduleForm.Close();
                btnThemCa.Click += (_, _) => OpenAddShiftPopup();

                scheduleForm.Controls.Add(dgv);
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
                    : "Hộp thư yêu cầu";
            }
            catch
            {
                label2.Text = "Hộp thư yêu cầu";
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
