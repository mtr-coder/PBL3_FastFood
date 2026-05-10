using PBL3.Business;
using PBL3.UI;
using System.Data;
using System.Linq;

namespace PBL3
{
    public partial class MuaHang : Form
    {
        private readonly MuaHangService _muaHangService;
        private readonly string _maNv;
        private readonly bool _isAdminPopup;
        private readonly string? _preselectedMaNcc;
        private readonly string? _preselectedMaNl;
        private readonly DataTable _phieuNhapTable = new DataTable();
        private CheckBox? _chkNguyenLieuMoi;
        private TextBox? _txtNguyenLieuMoi;
        private Label? _lblNguyenLieuMoi;
        public MuaHang() : this("1")
        {
        }

        public MuaHang(string maNv, bool isAdminPopup = false, string? preselectedMaNcc = null, string? preselectedMaNl = null)
        {
            _muaHangService = new MuaHangService();
            _maNv = maNv;
            _isAdminPopup = isAdminPopup;
            _preselectedMaNcc = preselectedMaNcc;
            _preselectedMaNl = preselectedMaNl;
            InitializeComponent();
        }

        private void MuaHang_Load(object? sender, EventArgs e)
        {
            try
            {
                InitPhieuNhapGrid();
                LoadNguyenLieu();
                LoadNhaCungCap();
                LoadDonViTinh();
                EnsureNguyenLieuMoiControls();
                ApplyAdminPopupLayout();
                ApplyInitialSelections();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể tải trang mua hàng.\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MuaHang_Resize(object? sender, EventArgs e)
        {
            ApplyResponsiveLayout();
        }

        private void EnsureNguyenLieuMoiControls()
        {
            if (_chkNguyenLieuMoi is not null)
            {
                return;
            }

            _chkNguyenLieuMoi = new CheckBox
            {
                Text = "Nguyên liệu mới",
                AutoSize = true
            };
            _chkNguyenLieuMoi.CheckedChanged += (_, _) => UpdateNguyenLieuMoiState();

            _lblNguyenLieuMoi = new Label
            {
                Text = "Tên nguyên liệu",
                Font = lblDonGia.Font,
                AutoSize = true,
                Visible = false
            };

            _txtNguyenLieuMoi = new TextBox
            {
                Visible = false
            };

            hcnt_Khung.Controls.Add(_chkNguyenLieuMoi);
            hcnt_Khung.Controls.Add(_lblNguyenLieuMoi);
            hcnt_Khung.Controls.Add(_txtNguyenLieuMoi);

            _chkNguyenLieuMoi.Location = new Point(493, 120);
            _lblNguyenLieuMoi.Location = new Point(493, 145);
            _txtNguyenLieuMoi.Location = new Point(493, 168);
            _txtNguyenLieuMoi.Size = new Size(323, 27);
        }

        private void UpdateNguyenLieuMoiState()
        {
            bool isNew = _chkNguyenLieuMoi?.Checked == true;
            dgvNguyenLieu.Enabled = !isNew;

            if (isNew)
            {
                LoadNhaCungCap();
            }

            if (_lblNguyenLieuMoi is not null)
            {
                _lblNguyenLieuMoi.Visible = isNew;
            }

            if (_txtNguyenLieuMoi is not null)
            {
                _txtNguyenLieuMoi.Visible = isNew;
                if (!isNew)
                {
                    _txtNguyenLieuMoi.Clear();
                }
            }
        }

        private void ApplyResponsiveLayout()
        {
            return;
        }

        private void ApplyAdminPopupLayout()
        {
            if (!_isAdminPopup)
            {
                return;
            }

            hcnt_KhungMenuAD.Visible = false;
            pb_Admin.Visible = false;
            lb_Admin.Visible = false;
            btn_DangXuat.Visible = false;

            hcnt_Khung.Location = new Point(12, 12);
            roundedPanel1.Size = new Size(900, 760);
            this.ClientSize = new Size(928, 790);
            this.StartPosition = FormStartPosition.CenterParent;
        }

        private void ApplyInitialSelections()
        {
            if (!string.IsNullOrWhiteSpace(_preselectedMaNl))
            {
                SelectNguyenLieuByMa(_preselectedMaNl);
            }

            if (!string.IsNullOrWhiteSpace(_preselectedMaNcc) && cboNhaCungCap.DataSource is not null)
            {
                try
                {
                    cboNhaCungCap.SelectedValue = _preselectedMaNcc;
                }
                catch
                {
                }
            }
        }

        private void SelectNguyenLieuByMa(string maNl)
        {
            string key = maNl.Trim();
            foreach (DataGridViewRow row in dgvNguyenLieu.Rows)
            {
                if (row.IsNewRow)
                {
                    continue;
                }

                string current = Convert.ToString(row.Cells["MaNL"].Value) ?? string.Empty;
                if (!string.Equals(current, key, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                row.Selected = true;
                if (row.Cells.Count > 0)
                {
                    dgvNguyenLieu.CurrentCell = row.Cells[0];
                }

                SyncInputFromSelectedNguyenLieu();
                break;
            }
        }

        private void btn_QLNV_Click(object? sender, EventArgs e) => OpenAndClose(new TrangNhanVien1(_maNv));
        private void btn_QLNCC_Click(object? sender, EventArgs e) => OpenAndClose(new TrangHoaDon(_maNv));
        private void btn_QLKH_Click(object? sender, EventArgs e) => OpenAndClose(new BanHang(_maNv));
        private void btn_QLHDN_Click(object? sender, EventArgs e) => OpenAndClose(new KhachHang(_maNv));

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
            Form currentForm = FindForm() ?? this;
            AdminNavigationManager.Navigate(currentForm, target);
        }

        private void InitPhieuNhapGrid()
        {
            _phieuNhapTable.Columns.Add("MaNL", typeof(string));
            _phieuNhapTable.Columns.Add("Tên nguyên liệu", typeof(string));
            _phieuNhapTable.Columns.Add("Đơn vị tính", typeof(string));
            _phieuNhapTable.Columns.Add("Số lượng", typeof(decimal));
            _phieuNhapTable.Columns.Add("Đơn giá", typeof(decimal));
            _phieuNhapTable.Columns.Add("Thành tiền", typeof(decimal), "[Số lượng] * [Đơn giá]");

            dgvPhieuNhap.DataSource = _phieuNhapTable;
            dgvPhieuNhap.Columns["MaNL"].Visible = false;
            dgvPhieuNhap.Columns["Đơn giá"].DefaultCellStyle.Format = "N0";
            dgvPhieuNhap.Columns["Thành tiền"].DefaultCellStyle.Format = "N0";
        }

        private void LoadNguyenLieu()
        {
            DataTable dt = _muaHangService.GetNguyenLieu();
            dgvNguyenLieu.DataSource = dt;

            if (dgvNguyenLieu.Columns.Contains("GiaNhap"))
            {
                dgvNguyenLieu.Columns["GiaNhap"].DefaultCellStyle.Format = "N0";
            }

            if (dgvNguyenLieu.Rows.Count > 0)
            {
                dgvNguyenLieu.Rows[0].Selected = true;
                SyncInputFromSelectedNguyenLieu();
            }
        }

        private void LoadNhaCungCap(string? maNl = null)
        {
            DataTable dt = _muaHangService.GetNhaCungCap(maNl);

            cboNhaCungCap.DataSource = dt;
            cboNhaCungCap.DisplayMember = "TenNCC";
            cboNhaCungCap.ValueMember = "MaNCC";
        }

        private void LoadDonViTinh()
        {
            DataTable dt = _muaHangService.GetDonViTinh();

            cboDonViTinh.DataSource = dt;
            cboDonViTinh.DisplayMember = "DonViTinh";
            cboDonViTinh.ValueMember = "DonViTinh";
        }

        private void dgvNguyenLieu_SelectionChanged(object? sender, EventArgs e)
        {
            SyncInputFromSelectedNguyenLieu();
        }

        private void SyncInputFromSelectedNguyenLieu()
        {
            if (dgvNguyenLieu.SelectedRows.Count == 0)
            {
                return;
            }

            DataGridViewRow row = dgvNguyenLieu.SelectedRows[0];
            string maNl = Convert.ToString(row.Cells["MaNL"].Value) ?? string.Empty;
            string donViTinh = Convert.ToString(row.Cells["DonViTinh"].Value) ?? string.Empty;
            decimal giaNhap = row.Cells["GiaNhap"].Value is null ? 0m : Convert.ToDecimal(row.Cells["GiaNhap"].Value);

            LoadNhaCungCap(maNl);

            if (!string.IsNullOrWhiteSpace(donViTinh) && cboDonViTinh.Items.Count > 0)
            {
                cboDonViTinh.SelectedValue = donViTinh;
            }

            txtDonGia.Text = giaNhap.ToString("0.##");
        }

        private void btnThem_Click(object? sender, EventArgs e)
        {
            bool isNewIngredient = _chkNguyenLieuMoi?.Checked == true;
            if (!isNewIngredient && dgvNguyenLieu.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn nguyên liệu.", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cboNhaCungCap.SelectedValue is null)
            {
                MessageBox.Show("Vui lòng chọn nhà cung cấp.", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cboDonViTinh.SelectedValue is null)
            {
                MessageBox.Show("Vui lòng chọn đơn vị tính.", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(txtDonGia.Text.Trim(), out decimal donGia) || donGia < 0)
            {
                MessageBox.Show("Đơn giá không hợp lệ.", "Dữ liệu sai", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string maNl;
            string tenNl;
            if (isNewIngredient)
            {
                tenNl = (_txtNguyenLieuMoi?.Text ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(tenNl))
                {
                    MessageBox.Show("Vui lòng nhập tên nguyên liệu mới.", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                maNl = string.Empty;
            }
            else
            {
                DataGridViewRow row = dgvNguyenLieu.SelectedRows[0];
                maNl = Convert.ToString(row.Cells["MaNL"].Value) ?? string.Empty;
                tenNl = Convert.ToString(row.Cells["TenNL"].Value) ?? string.Empty;
            }

            string dvt = Convert.ToString(cboDonViTinh.SelectedValue) ?? string.Empty;
            decimal soLuong = nudSoLuong.Value;

            DataRow? existed = _phieuNhapTable.AsEnumerable()
                .FirstOrDefault(r => string.Equals(Convert.ToString(r["MaNL"]), maNl, StringComparison.OrdinalIgnoreCase)
                                  && string.Equals(Convert.ToString(r["DonViTinh"]), dvt, StringComparison.OrdinalIgnoreCase));

            if (existed is null)
            {
                _phieuNhapTable.Rows.Add(maNl, tenNl, dvt, soLuong, donGia);
            }
            else
            {
                existed["SoLuong"] = Convert.ToDecimal(existed["SoLuong"]) + soLuong;
                existed["DonGia"] = donGia;
            }

            UpdateTongTien();
        }

        private void btnXoaDong_Click(object? sender, EventArgs e)
        {
            if (dgvPhieuNhap.SelectedRows.Count == 0)
            {
                return;
            }

            foreach (DataGridViewRow row in dgvPhieuNhap.SelectedRows)
            {
                if (!row.IsNewRow)
                {
                    dgvPhieuNhap.Rows.Remove(row);
                }
            }

            UpdateTongTien();
        }

        private void UpdateTongTien()
        {
            decimal total = _phieuNhapTable.AsEnumerable().Sum(r => r.Field<decimal>("ThanhTien"));
            lblTongTien.Text = $"Tổng tiền: {total:N0} đ";
        }

        private void btnLuuNhapHang_Click(object? sender, EventArgs e)
        {
            if (_phieuNhapTable.Rows.Count == 0)
            {
                MessageBox.Show("Phiếu nhập đang trống.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (cboNhaCungCap.SelectedValue is null)
            {
                MessageBox.Show("Nhà cung cấp là bắt buộc.", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal tongTien = _phieuNhapTable.AsEnumerable().Sum(r => r.Field<decimal>("ThanhTien"));
            object maNcc = int.TryParse(Convert.ToString(cboNhaCungCap.SelectedValue), out int maNccInt) ? maNccInt : (Convert.ToString(cboNhaCungCap.SelectedValue) ?? string.Empty);

            try
            {
                _muaHangService.SavePhieuNhap(_maNv, maNcc, tongTien, _phieuNhapTable);
                _phieuNhapTable.Clear();
                if (_chkNguyenLieuMoi != null)
                {
                    _chkNguyenLieuMoi.Checked = false;
                }
                nudSoLuong.Value = 1;
                txtDonGia.Clear();
                UpdateTongTien();
                LoadNguyenLieu();
                MessageBox.Show("Lưu phiếu nhập thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lưu phiếu nhập thất bại.\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_QLKH_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
