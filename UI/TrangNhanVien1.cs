using PBL3.Business;
using PBL3.UI;
using System.Data;

namespace PBL3
{
    public partial class TrangNhanVien1 : Form
    {
        private readonly TrangNhanVienService _trangNhanVienService;
        private DataTable? _nhanVienTable;
        private bool _isEditingExisting;
        private string? _selectedMaNvDbValue;
        private BanHang? _banHangEmbedded;

        private string _loggedInMaNV;
        private TextBox? _txtEmail;

        public TrangNhanVien1()
        {
            _trangNhanVienService = new TrangNhanVienService();
            _loggedInMaNV = "1";
            InitializeComponent();
            _cboTimTheo.SelectionChangeCommitted += SearchControl_Changed;
        }

        public TrangNhanVien1(string maNV) : this()
        {
            _loggedInMaNV = maNV;
        }

        private void TrangNhanVien1_Load(object? sender, EventArgs e)
        {
            try
            {
                EnsureProfileExtraControls();
                pnlDanhSachNhanVien.Visible = false;
                lblTrangThai.Visible = false;
                _cboTrangThai.Visible = false;

                _cboChucVu.Enabled = false;
                btn_QLNV.BackColor = Color.Salmon;
                label4.ForeColor = Color.White;

                LoadChucVu();
                LoadNhanVien();
                _isEditingExisting = true;
                UpdatePasswordUiState();
                EnsureLeaveRequestButtons();
                EnsureLeaveQuotaLabel();
                ConfigureLichSuYeuCauGrid();
                LoadLichSuYeuCau();
                UpdateLeaveQuotaUi();
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
        }

        private void LoadNhanVien()
        {
            DataTable dt = _trangNhanVienService.GetNhanVienByMaNv(_loggedInMaNV);
            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                _selectedMaNvDbValue = Convert.ToString(row["MaNV"]);
                _txtMaNV.Text = FormatMaNvForDisplay(_selectedMaNvDbValue);
                _txtMaNV.ReadOnly = true;
                _txtHoTen.Text = Convert.ToString(row["HoTen"]);
                _txtSdt.Text = Convert.ToString(row["SDT"]);
                if (_txtEmail is not null)
                {
                    _txtEmail.Text = Convert.ToString(row["Email"]);
                }
                _txtDiaChi.Text = Convert.ToString(row["DiaChi"]);

                if (DateTime.TryParse(Convert.ToString(row["NgaySinh"]), out DateTime ngaySinh))
                {
                    _dtpNgaySinh.Value = ngaySinh;
                }

                string maCv = Convert.ToString(row["MaCV"]) ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(maCv))
                {
                    _cboChucVu.SelectedValue = maCv;
                }
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
            if (string.IsNullOrWhiteSpace(keyword))
            {
                _nhanVienTable.DefaultView.RowFilter = string.Empty;
                return;
            }

            string selected = (Convert.ToString(_cboTimTheo.SelectedItem) ?? "MãNV").Trim();
            string filter = selected switch
            {
                "HọTên" => $"HoTen LIKE '%{keyword}%'",
                "NgàySinh" => $"Convert(NgaySinh, 'System.String') LIKE '%{keyword}%'",
                "SĐT" => $"SDT LIKE '%{keyword}%'",
                "ĐịaChỉ" => $"DiaChi LIKE '%{keyword}%'",
                "ChứcVụ" => $"TenCV LIKE '%{keyword}%'",
                "TrạngThái" => $"TrangThai LIKE '%{keyword}%'",
                _ => $"Convert(MaNV, 'System.String') LIKE '%{keyword}%'"
            };

            _nhanVienTable.DefaultView.RowFilter = filter;
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
            _txtDiaChi.Text = Convert.ToString(row.Cells["DiaChi"].Value) ?? string.Empty;
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

            if (!long.TryParse(_txtSdt.Text.Trim(), out _))
            {
                MessageBox.Show("Số điện thoại không hợp lệ.", "Dữ liệu sai", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            string email = _txtEmail?.Text.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(email) || !IsValidEmail(email))
            {
                MessageBox.Show("Email không hợp lệ.", "Dữ liệu sai", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                return;
            }
            try
            {
                _trangNhanVienService.AddNhanVien(
                    _txtHoTen.Text,
                    _dtpNgaySinh.Value.Date,
                    _txtSdt.Text,
                    _txtEmail?.Text ?? string.Empty,
                    _txtDiaChi.Text,
                    string.Empty,
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
                return;
            }

            try
            {
                int rows = _trangNhanVienService.UpdateNhanVien(
                    _selectedMaNvDbValue ?? _txtMaNV.Text.Trim(),
                    _txtHoTen.Text,
                    _dtpNgaySinh.Value.Date,
                    _txtSdt.Text,
                    _txtEmail?.Text ?? string.Empty,
                    _txtDiaChi.Text,
                    null);

                if (rows == 0)
                {
                    MessageBox.Show("Không tìm thấy nhân viên để cập nhật.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                MessageBox.Show("Cập nhật nhân viên thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadNhanVien();
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
            LoadLichSuYeuCau();
            _isEditingExisting = true;
            UpdatePasswordUiState();
        }

        private void ClearForm()
        {
            _selectedMaNvDbValue = null;
            _txtMaNV.Text = GenerateNextMaNhanVien();
            _txtHoTen.Clear();
            _txtSdt.Clear();
            _txtDiaChi.Clear();
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
            OpenAndClose(new TrangHoaDon(_selectedMaNvDbValue ?? _loggedInMaNV));
        }

        private void btn_QLKH_Click(object? sender, EventArgs e)
        {
            OpenAndClose(new BanHang(_selectedMaNvDbValue ?? _loggedInMaNV));
        }

        private void btn_QLMA_Click(object? sender, EventArgs e)
        {
            OpenAndClose(new MuaHang(_selectedMaNvDbValue ?? _loggedInMaNV));
        }

        private void btn_QLHDN_Click(object? sender, EventArgs e)
        {
            OpenAndClose(new KhachHang(_selectedMaNvDbValue ?? _loggedInMaNV));
        }

        private void OpenAndClose(Form target)
        {
            Form currentForm = FindForm() ?? this;
            AdminNavigationManager.Navigate(currentForm, target);
        }

        private void ShowBanHangInRightPanel()
        {
            pnlFormNhanVien.Visible = false;
            pnlDanhSachNhanVien.Visible = false;
            lb_QLNhanVienTitle.Text = "Bán hàng";

            btn_QLNV.BackColor = Color.Bisque;
            label4.ForeColor = SystemColors.ControlText;
            btn_QLKH.BackColor = Color.Salmon;
            label6.ForeColor = Color.White;

            if (_banHangEmbedded is null || _banHangEmbedded.IsDisposed)
            {
                _banHangEmbedded = new BanHang(_selectedMaNvDbValue ?? _loggedInMaNV)
                {
                    TopLevel = false,
                    FormBorderStyle = FormBorderStyle.None,
                    Dock = DockStyle.Fill
                };
                hcnt_Khung.Controls.Add(_banHangEmbedded);
            }

            _banHangEmbedded.BringToFront();
            _banHangEmbedded.Show();
        }

        private void ShowThongTinCaNhanPanel()
        {
            if (_banHangEmbedded is not null && !_banHangEmbedded.IsDisposed)
            {
                _banHangEmbedded.Hide();
            }

            lb_QLNhanVienTitle.Text = "Thông tin cá nhân";
            pnlFormNhanVien.Visible = true;
            pnlDanhSachNhanVien.Visible = false;

            btn_QLNV.BackColor = Color.Salmon;
            label4.ForeColor = Color.White;
            btn_QLKH.BackColor = Color.Bisque;
            label6.ForeColor = SystemColors.ControlText;
        }

        private void UpdatePasswordUiState()
        {
            bool isCreateMode = !_isEditingExisting;
            _btnDatLaiMatKhau.Visible = !isCreateMode;
            _btnXemLichTruc.Visible = !isCreateMode;
        }

        private void ConfigureProfileLayout()
        {
            lb_QLNhanVienTitle.Location = new Point(306, 8);
            pnlFormNhanVien.Size = new Size(834, 640);

            lblMaNV.Location = new Point(40, 25);
            pnlMaNVInput.Location = new Point(40, 54);

            lblHoTen.Location = new Point(300, 25);
            pnlHoTenInput.Location = new Point(300, 54);
            pnlHoTenInput.Size = new Size(220, 33);
            _txtHoTen.Size = new Size(196, 20);

            lblNgaySinh.Location = new Point(570, 25);
            _dtpNgaySinh.Location = new Point(570, 56);
            _dtpNgaySinh.Size = new Size(210, 27);

            lblSdt.Location = new Point(40, 116);
            pnlSdtInput.Location = new Point(40, 145);
            pnlSdtInput.Size = new Size(220, 33);
            _txtSdt.Size = new Size(196, 20);

            lblDiaChi.Location = new Point(300, 116);
            pnlDiaChiInput.Location = new Point(300, 145);
            pnlDiaChiInput.Size = new Size(480, 33);
            _txtDiaChi.Size = new Size(456, 20);

            lblChucVu.Location = new Point(430, 156);
            _cboChucVu.Location = new Point(430, 178);
            _cboChucVu.Size = new Size(350, 28);

            _btnSua.Location = new Point(40, 300);
            _btnSua.Size = new Size(150, 38);
            lblBtnSua.Location = new Point(53, 7);

            _btnDatLaiMatKhau.Location = new Point(215, 300);
            _btnDatLaiMatKhau.Size = new Size(180, 38);
            _lblBtnDatLaiMatKhau.Location = new Point(40, 7);

            _btnXemLichTruc.Location = new Point(420, 300);
            _btnXemLichTruc.Size = new Size(160, 38);
            _lblBtnXemLichTruc.Location = new Point(22, 7);
        }

        private void EnsureLeaveRequestButtons()
        {
            _btnYeuCauNghiPhep.Visible = true;
            _btnYeuCauNghiHan.Visible = true;
            _btnYeuCauNghiPhep.BringToFront();
            _btnYeuCauNghiHan.BringToFront();
        }

        private void BtnYeuCauNghiPhep_Click(object? sender, EventArgs e)
        {
            GuiYeuCauNghi("Nghỉ phép", true);
        }

        private void BtnYeuCauNghiHan_Click(object? sender, EventArgs e)
        {
            GuiYeuCauNghi("Nghỉ hẳn", false);
        }

        private void GuiYeuCauNghi(string loaiYeuCau, bool hasDateRange)
        {
            if (string.IsNullOrWhiteSpace(_selectedMaNvDbValue))
            {
                MessageBox.Show("Không xác định được nhân viên đăng nhập.", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int? maNvInt = ParseMaNvToInt(_selectedMaNvDbValue ?? _loggedInMaNV);
            if (maNvInt is null)
            {
                MessageBox.Show("Mã nhân viên không hợp lệ để gửi yêu cầu.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Check leave quota: only approved (TrangThai = 1) leaves count toward quota
            int approvedLeaveCount = _trangNhanVienService.GetApprovedLeaveCountForMonth(maNvInt.Value, DateTime.Today);

            if (loaiYeuCau == "Nghỉ phép" && approvedLeaveCount >= 3)
            {
                MessageBox.Show("Bạn đã hết lượt nghỉ phép trong tháng này (tối đa 3 ngày/tháng).", "Hết lượt nghỉ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!TryPromptLeaveReason(loaiYeuCau, hasDateRange, out string lyDo, out DateTime? tuNgay, out DateTime? denNgay, out int tongNgayNghi))
            {
                return;
            }

            DialogResult confirm = MessageBox.Show(
                $"Bạn chắc chắn muốn gửi '{loaiYeuCau}'?\n\nLý do: {lyDo}",
                "Xác nhận gửi yêu cầu",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes)
            {
                return;
            }

            try
            {
                _trangNhanVienService.InsertYeuCau(maNvInt.Value, loaiYeuCau, lyDo, tuNgay, denNgay);

                MessageBox.Show("Đã gửi yêu cầu thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadLichSuYeuCau();
                UpdateLeaveQuotaUi();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể gửi yêu cầu.\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigureLichSuYeuCauGrid()
        {
            dgvLichSuYeuCau.AutoGenerateColumns = false;
            dgvLichSuYeuCau.ReadOnly = true;
            dgvLichSuYeuCau.AllowUserToAddRows = false;
            dgvLichSuYeuCau.AllowUserToDeleteRows = false;
            dgvLichSuYeuCau.AllowUserToResizeRows = false;
            dgvLichSuYeuCau.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvLichSuYeuCau.MultiSelect = false;
            dgvLichSuYeuCau.RowHeadersVisible = false;
            dgvLichSuYeuCau.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

            if (dgvLichSuYeuCau.Columns.Count > 0)
            {
                return;
            }

            dgvLichSuYeuCau.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "STT",
                DataPropertyName = "STT",
                HeaderText = "STT",
                Width = 50,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            dgvLichSuYeuCau.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "NgayGui",
                DataPropertyName = "NgayGui",
                HeaderText = "Ngày gửi",
                Width = 95,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            dgvLichSuYeuCau.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Loai",
                DataPropertyName = "Loai",
                HeaderText = "Loại",
                Width = 100
            });

            dgvLichSuYeuCau.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "TongNgay",
                DataPropertyName = "TongNgay",
                HeaderText = "Tổng ngày",
                Width = 80,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            dgvLichSuYeuCau.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "TrangThai",
                DataPropertyName = "TrangThai",
                HeaderText = "Trạng thái",
                Width = 100
            });

            dgvLichSuYeuCau.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "PhanHoiAdmin",
                DataPropertyName = "PhanHoiAdmin",
                HeaderText = "Phản hồi từ Admin",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
        }

        private void LoadLichSuYeuCau()
        {
            int? maNvInt = ParseMaNvToInt(_selectedMaNvDbValue ?? _loggedInMaNV);
            if (maNvInt is null)
            {
                dgvLichSuYeuCau.DataSource = null;
                return;
            }

            try
            {
                DataTable dt = _trangNhanVienService.GetYeuCauHistory(maNvInt.Value);

                DataTable display = new DataTable();
                display.Columns.Add("STT", typeof(int));
                display.Columns.Add("NgayGui", typeof(string));
                display.Columns.Add("Loai", typeof(string));
                display.Columns.Add("TongNgay", typeof(string));
                display.Columns.Add("TrangThai", typeof(string));
                display.Columns.Add("PhanHoiAdmin", typeof(string));

                int stt = 1;
                foreach (DataRow row in dt.Rows)
                {
                    DateTime? ngayGui = row["NgayGui"] == DBNull.Value ? null : Convert.ToDateTime(row["NgayGui"]);
                    string loai = Convert.ToString(row["LoaiYeuCau"]) ?? string.Empty;
                    DateTime? tuNgay = row["TuNgay"] == DBNull.Value ? null : Convert.ToDateTime(row["TuNgay"]);
                    DateTime? denNgay = row["DenNgay"] == DBNull.Value ? null : Convert.ToDateTime(row["DenNgay"]);
                    int trangThai = row["TrangThai"] == DBNull.Value ? 0 : Convert.ToInt32(row["TrangThai"]);
                    string phanHoi = Convert.ToString(row["PhanHoiAdmin"]) ?? string.Empty;
                    string tongNgay = (tuNgay.HasValue && denNgay.HasValue)
                        ? ((denNgay.Value.Date - tuNgay.Value.Date).Days + 1).ToString()
                        : "-";

                    display.Rows.Add(
                        stt++,
                        ngayGui?.ToString("dd/MM/yyyy") ?? string.Empty,
                        NormalizeLoaiYeuCau(loai),
                        tongNgay,
                        RequestStateText(trangThai),
                        phanHoi);
                }

                dgvLichSuYeuCau.DataSource = display;
            }
            catch
            {
                dgvLichSuYeuCau.DataSource = null;
            }
        }

        private static string NormalizeLoaiYeuCau(string loai)
        {
            if (string.IsNullOrWhiteSpace(loai)) return string.Empty;
            string value = loai.Trim();
            if (value.Contains("nghỉ phép", StringComparison.OrdinalIgnoreCase)) return "Nghỉ phép";
            if (value.Contains("nghỉ hẳn", StringComparison.OrdinalIgnoreCase) || value.Contains("nghi han", StringComparison.OrdinalIgnoreCase)) return "Nghỉ hẳn";
            return value;
        }

        private static string RequestStateText(int state)
        {
            return state switch
            {
                1 => "Đã duyệt",
                2 => "Đã từ chối",
                _ => "Chờ duyệt"
            };
        }

        private static bool TryPromptLeaveReason(string loaiYeuCau, bool hasDateRange, out string lyDo, out DateTime? tuNgay, out DateTime? denNgay, out int tongNgayNghi)
        {
            lyDo = string.Empty;
            tuNgay = null;
            denNgay = null;
            tongNgayNghi = 0;

            using Form dialog = new Form
            {
                Text = loaiYeuCau,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MinimizeBox = false,
                MaximizeBox = false,
                ClientSize = hasDateRange ? new Size(430, 320) : new Size(430, 220)
            };

            Label lblReason = new Label { Text = "Nhập lý do:", Left = 12, Top = 15, AutoSize = true };
            TextBox txtReason = new TextBox
            {
                Left = 12,
                Top = 40,
                Width = 404,
                Height = hasDateRange ? 90 : 120,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                MaxLength = 500
            };

            Label? lblTu = null;
            DateTimePicker? dtpTu = null;
            Label? lblDen = null;
            DateTimePicker? dtpDen = null;
            Label? lblTong = null;

            if (hasDateRange)
            {
                lblTu = new Label { Text = "Từ ngày", Left = 12, Top = 138, AutoSize = true };
                dtpTu = new DateTimePicker
                {
                    Left = 12,
                    Top = 160,
                    Width = 180,
                    Format = DateTimePickerFormat.Custom,
                    CustomFormat = "dd/MM/yyyy",
                    MinDate = DateTime.Today,
                    Value = DateTime.Today
                };

                lblDen = new Label { Text = "Đến ngày", Left = 210, Top = 138, AutoSize = true };
                dtpDen = new DateTimePicker
                {
                    Left = 210,
                    Top = 160,
                    Width = 180,
                    Format = DateTimePickerFormat.Custom,
                    CustomFormat = "dd/MM/yyyy",
                    MinDate = DateTime.Today,
                    Value = DateTime.Today
                };

                lblTong = new Label
                {
                    Left = 12,
                    Top = 195,
                    AutoSize = true,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    ForeColor = Color.DarkSlateBlue,
                    Text = "Tổng số ngày nghỉ: 1"
                };

                void recalc()
                {
                    if (dtpDen!.Value.Date < dtpTu!.Value.Date)
                        dtpDen.Value = dtpTu.Value.Date;
                    int days = (dtpDen.Value.Date - dtpTu.Value.Date).Days + 1;
                    if (days < 1) days = 1;
                    lblTong!.Text = $"Tổng số ngày nghỉ: {days}";
                }

                dtpTu.ValueChanged += (_, __) => recalc();
                dtpDen.ValueChanged += (_, __) => recalc();
            }

            int buttonTop = hasDateRange ? 250 : 176;
            Button btnOk = new Button { Text = "Gửi", Left = 260, Top = buttonTop, Width = 75, DialogResult = DialogResult.OK };
            Button btnCancel = new Button { Text = "Hủy", Left = 341, Top = buttonTop, Width = 75, DialogResult = DialogResult.Cancel };

            dialog.Controls.Add(lblReason);
            dialog.Controls.Add(txtReason);
            if (hasDateRange)
            {
                dialog.Controls.Add(lblTu!);
                dialog.Controls.Add(dtpTu!);
                dialog.Controls.Add(lblDen!);
                dialog.Controls.Add(dtpDen!);
                dialog.Controls.Add(lblTong!);
            }
            dialog.Controls.Add(btnOk);
            dialog.Controls.Add(btnCancel);
            dialog.AcceptButton = btnOk;
            dialog.CancelButton = btnCancel;

            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return false;
            }

            string reason = txtReason.Text.Trim();
            if (string.IsNullOrWhiteSpace(reason))
            {
                MessageBox.Show("Vui lòng nhập lý do.", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (hasDateRange)
            {
                DateTime start = dtpTu!.Value.Date;
                DateTime end = dtpDen!.Value.Date;

                if (start < DateTime.Today)
                {
                    MessageBox.Show("Ngày bắt đầu không được nhỏ hơn ngày hiện tại.", "Dữ liệu sai", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (end < start)
                {
                    MessageBox.Show("Đến ngày phải lớn hơn hoặc bằng Từ ngày.", "Dữ liệu sai", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                tuNgay = start;
                denNgay = end;
                tongNgayNghi = (end - start).Days + 1;
            }

            lyDo = reason;
            return true;
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
                string maNvValue = _selectedMaNvDbValue ?? _txtMaNV.Text.Trim();
                string oldPassword = _trangNhanVienService.GetCurrentPassword(maNvValue);
                if (string.Equals(oldPassword, newPassword, StringComparison.Ordinal))
                {
                    MessageBox.Show("Mật khẩu mới không được trùng với mật khẩu cũ.", "Dữ liệu sai", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int rows = _trangNhanVienService.ResetPassword(maNvValue, newPassword);
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
            TextBox txtNew = new TextBox { Left = 120, Top = 12, Width = 140, UseSystemPasswordChar = true };
            Label lblConfirm = new Label { Text = "Xác nhận", Left = 12, Top = 50, AutoSize = true };
            TextBox txtConfirm = new TextBox { Left = 120, Top = 47, Width = 140, UseSystemPasswordChar = true };
            Button btnEye = new Button { Text = "👁", Left = 265, Top = 12, Width = 35, Height = 27 };
            Button btnOk = new Button { Text = "OK", Left = 144, Top = 92, Width = 75, DialogResult = DialogResult.OK };
            Button btnCancel = new Button { Text = "Hủy", Left = 225, Top = 92, Width = 75, DialogResult = DialogResult.Cancel };

            btnEye.Click += (_, __) =>
            {
                bool show = txtNew.UseSystemPasswordChar;
                txtNew.UseSystemPasswordChar = !show;
                txtConfirm.UseSystemPasswordChar = !show;
            };

            dialog.Controls.Add(lblNew);
            dialog.Controls.Add(txtNew);
            dialog.Controls.Add(lblConfirm);
            dialog.Controls.Add(txtConfirm);
            dialog.Controls.Add(btnEye);
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

        private void EnsureProfileExtraControls()
        {
            _txtEmail ??= textBox1;
        }

        private void EnsureLeaveQuotaLabel()
        {
            if (lblLeaveQuota == null)
            {
                return;
            }

            lblLeaveQuota.Text = "Số ngày đã nghỉ tháng này: 0/3";
            lblLeaveQuota.Visible = true;
        }

        private void UpdateLeaveQuotaUi()
        {
            int? maNvInt = ParseMaNvToInt(_selectedMaNvDbValue ?? _loggedInMaNV);
            if (lblLeaveQuota == null || maNvInt is null)
            {
                return;
            }

            int count = 0;
            try
            {
                count = _trangNhanVienService.GetApprovedLeaveCountForMonth(maNvInt.Value, DateTime.Today);
            }
            catch
            {
                count = 0;
            }

            if (count < 0) count = 0;
            if (count > 3) count = 3;

            lblLeaveQuota.Text = $"Số ngày đã nghỉ tháng này: {count}/3";
            bool canRequest = count < 3;
            _btnYeuCauNghiPhep.Enabled = canRequest;

            if (!canRequest)
            {
                _btnYeuCauNghiPhep.BackColor = Color.Gray;
                _btnYeuCauNghiPhep.Text = "Hết lượt nghỉ";
            }
            else
            {
                _btnYeuCauNghiPhep.BackColor = Color.SandyBrown;
                _btnYeuCauNghiPhep.Text = "Yêu cầu nghỉ phép";
            }
        }

        private static int? ParseMaNvToInt(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            string s = value.Trim();
            if (s.StartsWith("NV", StringComparison.OrdinalIgnoreCase))
            {
                s = s.Substring(2);
            }
            return int.TryParse(s, out int v) ? v : null;
        }

        private static bool IsValidEmail(string? email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            try
            {
                var regex = new System.Text.RegularExpressions.Regex(@"^[^\s@]+@[^\s@]+\.[^\s@]+$", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                return regex.IsMatch(email.Trim());
            }
            catch
            {
                return false;
            }
        }

        private void BtnXemLichTruc_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_txtMaNV.Text))
            {
                MessageBox.Show("Vui lòng chọn nhân viên để xem lịch trực.", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            const decimal donGiaCaTruc = 176000m;

            try
            {
                DataTable dt = _trangNhanVienService.GetLichTruc(_selectedMaNvDbValue ?? _txtMaNV.Text.Trim());

                if (dt.Rows.Count == 0)
                {
                    MessageBox.Show("Nhân viên này chưa có lịch trực.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                decimal tongHeSo = dt.AsEnumerable().Sum(r => Convert.ToDecimal(r["HeSoLuong"]));
                int tongSoCa = dt.Rows.Count;
                decimal luongDuKien = tongHeSo * donGiaCaTruc;

                using Form scheduleForm = new Form
                {
                    Text = $"Lịch trực - NV {_txtMaNV.Text}",
                    StartPosition = FormStartPosition.CenterParent,
                    Size = new Size(720, 420)
                };

                DataGridView dgv = new DataGridView
                {
                    Dock = DockStyle.Fill,
                    ReadOnly = true,
                    AllowUserToAddRows = false,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                    SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                    DataSource = dt
                };
                dgv.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
                dgv.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;

                if (dgv.Columns["NgayLam"] is not null)
                {
                    dgv.Columns["NgayLam"].HeaderText = "Ngày làm";
                    dgv.Columns["NgayLam"].DefaultCellStyle.Format = "dd/MM/yyyy";
                }

                if (dgv.Columns["GioBatDau"] is not null)
                {
                    dgv.Columns["GioBatDau"].HeaderText = "Giờ bắt đầu";
                    dgv.Columns["GioBatDau"].DefaultCellStyle.Format = "HH:mm";
                }

                if (dgv.Columns["GioKetThuc"] is not null)
                {
                    dgv.Columns["GioKetThuc"].HeaderText = "Giờ kết thúc";
                    dgv.Columns["GioKetThuc"].DefaultCellStyle.Format = "HH:mm";
                }

                if (dgv.Columns["HeSoLuong"] is not null)
                {
                    dgv.Columns["HeSoLuong"].HeaderText = "Hệ số lương";
                    dgv.Columns["HeSoLuong"].DefaultCellStyle.Format = "N2";
                }

                if (dgv.Columns["MaCa"] is not null)
                {
                    dgv.Columns["MaCa"].HeaderText = "Mã ca";
                }

                if (dgv.Columns["TenCa"] is not null)
                {
                    dgv.Columns["TenCa"].HeaderText = "Tên ca";
                }

                Panel pnlSummary = new Panel
                {
                    Dock = DockStyle.Bottom,
                    Height = 52,
                    BackColor = Color.FromArgb(248, 242, 235)
                };

                Label lblTongSoCa = new Label
                {
                    AutoSize = true,
                    Location = new Point(12, 17),
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    Text = $"Tổng số ca làm: {tongSoCa}"
                };

                Label lblTongHeSo = new Label
                {
                    AutoSize = true,
                    Location = new Point(210, 17),
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    Text = $"Tổng hệ số: {tongHeSo:N2}"
                };

                Label lblLuongDuKien = new Label
                {
                    AutoSize = true,
                    Location = new Point(380, 17),
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    Text = $"Tiền lương theo ca: {luongDuKien:N0} đ"
                };

                pnlSummary.Controls.Add(lblTongSoCa);
                pnlSummary.Controls.Add(lblTongHeSo);
                pnlSummary.Controls.Add(lblLuongDuKien);

                scheduleForm.Controls.Add(pnlSummary);
                scheduleForm.Controls.Add(dgv);
                scheduleForm.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể tải lịch trực.\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private decimal GetLuongCoBanNhanVien(string maNv)
        {
            return _trangNhanVienService.GetLuongCoBanNhanVien(maNv);
        }

        private void _txtMatKhau_TextChanged(object sender, EventArgs e)
        {

        }

        private void _dgvNhanVien_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {
            ShowThongTinCaNhanPanel();
        }

        private void btn_QLNCC_Paint(object sender, PaintEventArgs e)
        {

        }

        private void btn_QLKH_Paint(object sender, PaintEventArgs e)
        {

        }

        private void pnlFormNhanVien_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
