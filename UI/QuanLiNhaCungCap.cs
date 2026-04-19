using PBL3.Business;
using PBL3.UI;
using System.Data;
using System.Globalization;
using System.Linq;

namespace PBL3
{
    public partial class QuanLiNhaCungCap : Form
    {
        private readonly NhaCungCapService _nhaCungCapService;
        private DataTable? _nhaCungCapTable;
        private string? _selectedMaNccDbValue;
        private bool _hasTrangThaiColumn;
        private bool _hasEmailColumn;
        private bool _hasGhiChuColumn;

        public QuanLiNhaCungCap()
        {
            _nhaCungCapService = new NhaCungCapService();
            InitializeComponent();
            _cboTimTheo.SelectionChangeCommitted += SearchControl_Changed;
            _dgvNhanVien.DataBindingComplete += DgvNhanVien_DataBindingComplete;
        }

        private static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
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
            var m = System.Text.RegularExpressions.Regex.Match(phone, "^\\d{10}$");
            return m.Success;
        }

        private void QuanLiNhaCungCap_Load(object? sender, EventArgs e)
        {
            try
            {
                DetectNhaCungCapSchema();
                LoadNhaCungCap();
                if (_cboTimTheo.Items.Count > 0 && _cboTimTheo.SelectedIndex < 0)
                {
                    _cboTimTheo.SelectedIndex = 0;
                }

                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể tải dữ liệu nhà cung cấp.\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DetectNhaCungCapSchema()
        {
            var schema = _nhaCungCapService.GetSchemaInfo();
            _hasTrangThaiColumn = schema.HasTrangThaiColumn;
            _hasEmailColumn = schema.HasEmailColumn;
            _hasGhiChuColumn = schema.HasGhiChuColumn;

            _txtEmail.Enabled = _hasEmailColumn;
            _txtEmail.PlaceholderText = _hasEmailColumn ? "Email" : "DB chưa có cột Email";

            if (_txtMatHang is not null)
            {
                _txtMatHang.ReadOnly = !_hasGhiChuColumn;
                _txtMatHang.PlaceholderText = _hasGhiChuColumn ? "Ghi chú NCC" : "DB chưa có cột GhiChu";
            }
        }

        private void LoadNhaCungCap()
        {
            _nhaCungCapTable = _nhaCungCapService.GetAll(_hasEmailColumn, _hasGhiChuColumn);
            _dgvNhanVien.DataSource = _nhaCungCapTable;
            _dgvNhanVien.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            _dgvNhanVien.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
            _dgvNhanVien.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;

            // Desired column order: MaNCC, TenNCC, SDT, Email, DiaChi, MatHangChuYeu, GhiChu
            SetHeaderText("MaNCC", "MãNCC");
            SetHeaderText("TenNCC", "Tên");
            SetHeaderText("SDT", "SĐT");
            SetHeaderText("Email", "Email");
            SetHeaderText("DiaChi", "ĐịaChỉ");
            SetHeaderText("MatHangChuYeu", "Mặt hàng chủ yếu");
            SetHeaderText("GhiChu", "Ghi chú");

            SetColumnWidth("MaNCC", 95);
            SetColumnWidth("TenNCC", 150);
            SetColumnWidth("SDT", 110);
            SetColumnWidth("Email", 150);
            SetColumnWidth("DiaChi", 120);
            SetColumnWidth("MatHangChuYeu", 160);
            SetColumnWidth("GhiChu", 140);

            // TrangThai column is not shown in the grid

            ApplySearchFilter();
            UpdateTopMetrics();
        }

        private void SearchControl_Changed(object? sender, EventArgs e)
        {
            ApplySearchFilter();
        }

        private void ApplySearchFilter()
        {
            if (_nhaCungCapTable is null)
            {
                return;
            }

            string keyword = _txtTimKiem.Text.Trim().Replace("'", "''");
            // status filtering removed (no TrangThai column shown)

            if (string.IsNullOrWhiteSpace(keyword))
            {
                _nhaCungCapTable.DefaultView.RowFilter = string.Empty;
                ApplyTrangThaiRowStyle();
                UpdateTopMetrics();
                return;
            }

            string selected = (Convert.ToString(_cboTimTheo.SelectedItem) ?? "MãNCC").Trim();
            string filter;

            if (selected == "TênNCC" || selected == "TenNCC" || selected == "Tên")
            {
                filter = $"TenNCC LIKE '%{keyword}%'";
            }
            else if (selected == "SĐT" || selected == "SDT")
            {
                filter = $"SDT LIKE '%{keyword}%'";
            }
            else if (selected == "ĐịaChỉ" || selected == "Địa chỉ" || selected == "DiaChi")
            {
                filter = $"DiaChi LIKE '%{keyword}%'";
            }
            else if (selected == "MặtHàngChủYếu" || selected == "MatHangChuYeu")
            {
                filter = $"MatHangChuYeu LIKE '%{keyword}%'";
            }
            else
            {
                filter = $"Convert(MaNCC, 'System.String') LIKE '%{keyword}%'";
            }

            // statusFilter removed, only apply keyword filter
            _nhaCungCapTable.DefaultView.RowFilter = filter;

            ApplyTrangThaiRowStyle();
            UpdateTopMetrics();
        }

        private void DgvNhanVien_DataBindingComplete(object? sender, DataGridViewBindingCompleteEventArgs e)
        {
            ApplyTrangThaiRowStyle();
            UpdateTopMetrics();
        }

        private void ApplyTrangThaiRowStyle()
        {
            if (!_hasTrangThaiColumn)
            {
                return;
            }

            // If the grid/data does not contain the TrangThai column (we don't select it into the DataTable), skip styling
            if (!_dgvNhanVien.Columns.Contains("TrangThai"))
            {
                return;
            }

            foreach (DataGridViewRow row in _dgvNhanVien.Rows)
            {
                if (row.IsNewRow)
                {
                    continue;
                }

                bool active = true;
                object value = row.Cells["TrangThai"].Value;
                if (value is not null && value != DBNull.Value)
                {
                    active = Convert.ToBoolean(value);
                }

                if (active)
                {
                    row.DefaultCellStyle.ForeColor = Color.Black;
                    row.DefaultCellStyle.BackColor = Color.White;
                }
                else
                {
                    row.DefaultCellStyle.ForeColor = Color.DimGray;
                    row.DefaultCellStyle.BackColor = Color.Gainsboro;
                }
            }
        }

        private void UpdateTopMetrics()
        {
            if (_nhaCungCapTable is null)
            {
                return;
            }
            DataRowView[] rows = _nhaCungCapTable.DefaultView.Cast<DataRowView>().ToArray();
            int totalNcc = rows.Length;

            decimal tongTien = 0m;
            if (_nhaCungCapTable.Columns.Contains("TongTienDaNhap"))
            {
                tongTien = rows.Sum(r => r.Row["TongTienDaNhap"] == DBNull.Value ? 0m : Convert.ToDecimal(r.Row["TongTienDaNhap"], CultureInfo.InvariantCulture));
            }

            DateTime? ngayGanNhat = null;
            if (_nhaCungCapTable.Columns.Contains("NgayGiaoGanNhat"))
            {
                ngayGanNhat = rows
                    .Select(r => r.Row["NgayGiaoGanNhat"] == DBNull.Value
                        ? (DateTime?)null
                        : Convert.ToDateTime(r.Row["NgayGiaoGanNhat"], CultureInfo.InvariantCulture))
                    .Max();
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
            _selectedMaNccDbValue = Convert.ToString(row.Cells["MaNCC"].Value) ?? string.Empty;
            _txtMaNCC.Text = FormatMaNccForDisplay(_selectedMaNccDbValue);
            _txtHoTen.Text = Convert.ToString(row.Cells["TenNCC"].Value) ?? string.Empty;
            _txtSdt.Text = Convert.ToString(row.Cells["SDT"].Value) ?? string.Empty;
            _txtDiaChi.Text = Convert.ToString(row.Cells["DiaChi"].Value) ?? string.Empty;
            _txtMatHang.Text = Convert.ToString(row.Cells["GhiChu"].Value) ?? string.Empty;
            _txtEmail.Text = Convert.ToString(row.Cells["Email"].Value) ?? string.Empty;

        }

        private bool ValidateInput(bool isInsert)
        {
            if (!isInsert && string.IsNullOrWhiteSpace(_txtMaNCC.Text))
            {
                MessageBox.Show("Vui lòng chọn nhà cung cấp.", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(_txtHoTen.Text))
            {
                MessageBox.Show("Vui lòng nhập tên nhà cung cấp.", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // Phone validation: digits only, exactly 10 digits
            string phone = _txtSdt.Text.Trim();
            if (!IsValidPhone(phone))
            {
                MessageBox.Show("Số điện thoại không hợp lệ. Vui lòng nhập đúng 10 chữ số.", "Dữ liệu sai", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtSdt.Focus();
                return false;
            }

            if (_hasEmailColumn && !string.IsNullOrWhiteSpace(_txtEmail.Text))
            {
                string email = _txtEmail.Text.Trim();
                if (!IsValidEmail(email))
                {
                    MessageBox.Show("Email phải có dạng ten@gmail.com.", "Dữ liệu sai", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _txtEmail.Focus();
                    return false;
                }
            }

            if (IsPhoneExists(isInsert, _txtSdt.Text.Trim()))
            {
                MessageBox.Show("Số điện thoại nhà cung cấp đã tồn tại.", "Dữ liệu trùng lặp", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private bool IsPhoneExists(bool isInsert, string phone)
        {
            try
            {
                return _nhaCungCapService.IsPhoneExists(isInsert, phone, _selectedMaNccDbValue ?? _txtMaNCC.Text.Trim());
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
                _nhaCungCapService.Add(
                    _txtHoTen.Text,
                    _txtSdt.Text,
                    _txtDiaChi.Text,
                    _txtEmail.Text,
                    _txtMatHang.Text,
                    _hasEmailColumn,
                    _hasGhiChuColumn,
                    _hasTrangThaiColumn);

                MessageBox.Show("Thêm nhà cung cấp thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadNhaCungCap();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Thêm nhà cung cấp thất bại.\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                int rows = _nhaCungCapService.Update(
                    _selectedMaNccDbValue ?? _txtMaNCC.Text.Trim(),
                    _txtHoTen.Text,
                    _txtSdt.Text,
                    _txtDiaChi.Text,
                    _txtEmail.Text,
                    _txtMatHang.Text,
                    _hasEmailColumn,
                    _hasGhiChuColumn);

                if (rows == 0)
                {
                    MessageBox.Show("Không tìm thấy nhà cung cấp để cập nhật.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                MessageBox.Show("Cập nhật nhà cung cấp thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadNhaCungCap();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Cập nhật nhà cung cấp thất bại.\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnXoa_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_txtMaNCC.Text))
            {
                MessageBox.Show("Vui lòng chọn nhà cung cấp cần xóa.", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string maNcc = _selectedMaNccDbValue ?? _txtMaNCC.Text.Trim();
                int historyCount = _nhaCungCapService.GetNhapHistoryCount(maNcc);

                if (historyCount > 0)
                {
                    if (!_hasTrangThaiColumn)
                    {
                        MessageBox.Show("NCC này đã có giao dịch và DB chưa có cột TrangThai để xóa mềm.", "Không thể xóa", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    DialogResult softDeleteResult = MessageBox.Show(
                        "NCC này đã có giao dịch, bạn muốn chuyển sang trạng thái Ngừng hợp tác không?",
                        "Xác nhận xóa mềm",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (softDeleteResult != DialogResult.Yes)
                    {
                        return;
                    }

                    _nhaCungCapService.SoftDeactivate(maNcc);
                    MessageBox.Show("Đã chuyển NCC sang trạng thái Ngừng hợp tác.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadNhaCungCap();
                    ClearForm();
                    return;
                }

                DialogResult result = MessageBox.Show(
                    "Bạn có chắc chắn muốn xóa nhà cung cấp này?",
                    "Xác nhận xóa",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result != DialogResult.Yes)
                {
                    return;
                }

                int rows = _nhaCungCapService.Delete(maNcc);

                if (rows == 0)
                {
                    MessageBox.Show("Không tìm thấy nhà cung cấp để xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                MessageBox.Show("Xóa nhà cung cấp thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadNhaCungCap();
                ClearForm();
            }
            catch (Exception ex) when (DataErrorHelper.IsForeignKeyViolation(ex))
            {
                MessageBox.Show("Không thể xóa nhà cung cấp vì đang được sử dụng ở dữ liệu liên quan.", "Không thể xóa", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Xóa nhà cung cấp thất bại.\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnLamMoi_Click(object? sender, EventArgs e)
        {
            ClearForm();
            LoadNhaCungCap();
        }

        private void ClearForm()
        {
            _selectedMaNccDbValue = null;
            _txtMaNCC.Text = GenerateNextMaNCC();
            _txtHoTen.Clear();
            _txtSdt.Clear();
            _txtDiaChi.Clear();
            _txtMatHang.Clear();
            _txtEmail.Clear();

            _txtHoTen.Focus();
        }

        private void BtnXemLichSu_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_selectedMaNccDbValue))
            {
                MessageBox.Show("Vui lòng chọn nhà cung cấp để xem lịch sử.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DataTable dt = _nhaCungCapService.GetNhapHistory(_selectedMaNccDbValue);

            Form popup = new Form
            {
                Text = $"Lịch sử nhập - {_txtHoTen.Text}",
                StartPosition = FormStartPosition.CenterParent,
                Size = new Size(760, 460)
            };

            Panel pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 40,
                Padding = new Padding(10, 8, 10, 8)
            };

            Button btnThoat = new Button
            {
                Text = "Thoát",
                Dock = DockStyle.Right,
                Width = 90
            };
            btnThoat.Click += (_, __) => popup.Close();

            pnlBottom.Controls.Add(btnThoat);

            Label lblTongKet = new Label
            {
                Dock = DockStyle.Fill,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Text = $"Tổng số đơn nhập: {dt.Rows.Count} - Tổng giá trị: {dt.AsEnumerable().Sum(r => r["TongTien"] == DBNull.Value ? 0m : Convert.ToDecimal(r["TongTien"], CultureInfo.InvariantCulture)):N0} VNĐ"
            };
            pnlBottom.Controls.Add(lblTongKet);

            DataGridView dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoGenerateColumns = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                DataSource = dt,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false
            };

            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "MaHDN",
                Name = "MaHDN",
                HeaderText = "Mã hóa đơn",
                FillWeight = 30
            });
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "NgayNhap",
                Name = "NgayNhap",
                HeaderText = "Thời gian nhập",
                FillWeight = 35,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy HH:mm" }
            });
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "TongTien",
                Name = "TongTien",
                HeaderText = "Tổng tiền",
                FillWeight = 35,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "#,##0",
                    Alignment = DataGridViewContentAlignment.MiddleRight
                }
            });

            dgv.CellDoubleClick += (_, args) =>
            {
                if (args.RowIndex < 0) return;
                string maHdn = Convert.ToString(dgv.Rows[args.RowIndex].Cells["MaHDN"].Value) ?? string.Empty;
                if (string.IsNullOrWhiteSpace(maHdn)) return;
                ShowHoaDonNhapDetailPopup(maHdn);
            };

            if (dt.Rows.Count == 0)
            {
                dgv.Visible = false;
                lblTongKet.Text = "Tổng số đơn nhập: 0 - Tổng giá trị: 0 VNĐ";
                Label lblEmpty = new Label
                {
                    Dock = DockStyle.Fill,
                    Text = "Chưa có lịch sử nhập",
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 11F, FontStyle.Italic),
                    ForeColor = Color.DimGray
                };
                popup.Controls.Add(lblEmpty);
            }

            popup.Controls.Add(dgv);
            popup.Controls.Add(pnlBottom);
            popup.ShowDialog(this);
        }

        private void ShowHoaDonNhapDetailPopup(string maHdn)
        {
            DataTable detail = _nhaCungCapService.GetNhapDetail(maHdn);

            Form detailPopup = new Form
            {
                Text = $"Chi tiết hóa đơn nhập {maHdn}",
                StartPosition = FormStartPosition.CenterParent,
                Size = new Size(760, 420)
            };

            Panel pnlDetailBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 44,
                Padding = new Padding(10, 6, 10, 6)
            };

            Button btnThoatDetail = new Button
            {
                Text = "Thoát",
                Dock = DockStyle.Right,
                Width = 90
            };
            btnThoatDetail.Click += (_, __) => detailPopup.Close();
            pnlDetailBottom.Controls.Add(btnThoatDetail);

            DataGridView dgvDetail = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoGenerateColumns = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                DataSource = detail,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false
            };

            dgvDetail.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "TenNguyenLieu",
                Name = "TenNguyenLieu",
                HeaderText = "Nguyên liệu",
                FillWeight = 40
            });
            dgvDetail.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "SoLuong",
                Name = "SoLuong",
                HeaderText = "Số lượng",
                FillWeight = 18
            });
            dgvDetail.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "DonGia",
                Name = "DonGia",
                HeaderText = "Đơn giá",
                FillWeight = 21,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "#,##0",
                    Alignment = DataGridViewContentAlignment.MiddleRight
                }
            });
            dgvDetail.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ThanhTien",
                Name = "ThanhTien",
                HeaderText = "Thành tiền",
                FillWeight = 21,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "#,##0",
                    Alignment = DataGridViewContentAlignment.MiddleRight
                }
            });

            detailPopup.Controls.Add(dgvDetail);
            detailPopup.Controls.Add(pnlDetailBottom);
            detailPopup.ShowDialog(this);
        }

        private void BtnTaoDonNhap_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_selectedMaNccDbValue))
            {
                MessageBox.Show("Vui lòng chọn nhà cung cấp trước khi tạo đơn nhập.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string tenNcc = string.IsNullOrWhiteSpace(_txtHoTen.Text) ? "Nhà cung cấp" : _txtHoTen.Text.Trim();
            using ChiTietNhapHang form = new ChiTietNhapHang(_selectedMaNccDbValue, tenNcc, "Admin", "1");
            form.ShowDialog(this);
        }

        private string GenerateNextMaNCC()
        {
            try
            {
                return _nhaCungCapService.GenerateNextDisplayCode();
            }
            catch
            {
                return "NCC1";
            }
        }

        private static string FormatMaNccForDisplay(string? maNccValue)
        {
            if (string.IsNullOrWhiteSpace(maNccValue))
            {
                return string.Empty;
            }

            string value = maNccValue.Trim();
            return value.StartsWith("NCC", StringComparison.OrdinalIgnoreCase) ? value.ToUpperInvariant() : $"NCC{value}";
        }

        private void btn_QLNCC_Click(object? sender, EventArgs e)
        {
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

        private void btn_QLHDB_Click(object? sender, EventArgs e)
        {
            AdminNavigationManager.Navigate<LichSuHoaDon>(this);
        }

        private void btn_ThongKe_Click(object? sender, EventArgs e)
        {
            AdminNavigationManager.Navigate<ThongKe>(this);
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

        private void label1_Click(object sender, EventArgs e)
        {
        }

        private void roundedPanel4_Paint(object sender, PaintEventArgs e)
        {
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
        }
    }
}
