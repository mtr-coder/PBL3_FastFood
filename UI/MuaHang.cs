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
        private bool _isNavigating;

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
            AdminNavigationManager.Navigate(this, target);
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
            decimal total = _phieuNhapTable.AsEnumerable().Sum(r => r.Field<decimal>("Thành tiền"));
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

            decimal tongTien = _phieuNhapTable.AsEnumerable().Sum(r => r.Field<decimal>("Thành tiền"));
            object maNcc = int.TryParse(Convert.ToString(cboNhaCungCap.SelectedValue), out int maNccInt) ? maNccInt : (Convert.ToString(cboNhaCungCap.SelectedValue) ?? string.Empty);
            string tenNcc = cboNhaCungCap.Text ?? "-";

            try
            {
                DataTable printTable = _phieuNhapTable.Copy();
                object maHdn = _muaHangService.SavePhieuNhap(_maNv, maNcc, tongTien, _phieuNhapTable);

                ShowPhieuNhapPreview(maHdn, printTable, tongTien, tenNcc);

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

        private void ShowPhieuNhapPreview(object maHdn, DataTable detailTable, decimal tongTien, string tenNcc)
        {
            DateTime thoiGian = DateTime.Now;
            string qrPayload = $"PNK-{thoiGian:yyyy-MM-dd}-{maHdn}";

            Image? qrImage = null;
            try
            {
                string url = $"https://api.qrserver.com/v1/create-qr-code/?size=170x170&data={Uri.EscapeDataString(qrPayload)}";
                using System.Net.Http.HttpClient client = new();
                byte[] bytes = client.GetByteArrayAsync(url).GetAwaiter().GetResult();
                using System.IO.MemoryStream ms = new(bytes);
                using Image tmp = Image.FromStream(ms);
                qrImage = new Bitmap(tmp);
            }
            catch { }

            System.Drawing.Printing.PrintDocument doc = new();
            doc.DefaultPageSettings.PaperSize = new System.Drawing.Printing.PaperSize("Receipt", 315, 1400);
            doc.DefaultPageSettings.Margins = new System.Drawing.Printing.Margins(10, 10, 10, 10);
            doc.PrintPage += (s, e) =>
            {
                using Font brandFont = new("Segoe UI", 11, FontStyle.Bold);
                using Font titleFont = new("Segoe UI", 10, FontStyle.Bold);
                using Font normalFont = new("Segoe UI", 8.5f);
                using Font monoFont = new("Consolas", 8.5f);
                using Font totalFont = new("Segoe UI", 10f, FontStyle.Bold);

                float left = e.MarginBounds.Left;
                float right = e.MarginBounds.Right;
                float width = e.MarginBounds.Width;
                float y = e.MarginBounds.Top;

                using System.Drawing.StringFormat rightFmt = new() { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Near };

                void DrawCentered(string text, Font font)
                {
                    SizeF sz = e.Graphics.MeasureString(text, font);
                    e.Graphics.DrawString(text, font, Brushes.Black, left + (width - sz.Width) / 2f, y);
                    y += sz.Height + 1f;
                }

                void DrawLineRA(string label, string amount, Font font, Brush brush)
                {
                    e.Graphics.DrawString(label, font, brush, left, y);
                    RectangleF ar = new(left, y, width, font.GetHeight(e.Graphics));
                    e.Graphics.DrawString(amount, font, brush, ar, rightFmt);
                    y += font.GetHeight(e.Graphics) + 1f;
                }

                // Header
                DrawCentered("TỨ ĐẠI THIÊN LONG", brandFont);
                DrawCentered("169 Nguyễn Lương Bằng", normalFont);
                DrawCentered("SĐT: 0374895922", normalFont);
                y += 2f;
                e.Graphics.DrawLine(Pens.Gray, left, y, right, y);
                y += 4f;

                // Thông tin chung
                DrawCentered("PHIẾU NHẬP KHO", titleFont);
                e.Graphics.DrawString($"Mã phiếu: {maHdn}", normalFont, Brushes.Black, left, y); y += normalFont.GetHeight(e.Graphics) + 1f;
                e.Graphics.DrawString($"Ngày: {thoiGian:dd/MM/yyyy HH:mm}", normalFont, Brushes.Black, left, y); y += normalFont.GetHeight(e.Graphics) + 1f;
                e.Graphics.DrawString($"Nhà cung cấp: {tenNcc}", normalFont, Brushes.Black, left, y); y += normalFont.GetHeight(e.Graphics) + 1f;
                y += 2f;
                e.Graphics.DrawLine(Pens.Gray, left, y, right, y);
                y += 4f;

                // Bảng: Tên hàng | ĐVT | SL | Đơn giá | T.Tiền
                float dvtW = 40f; float qtyW = 24f; float priceW = 52f; float totalW = 62f;
                const float gap = 4f;
                float nameW = width - dvtW - qtyW - priceW - totalW - (gap * 3f);

                RectangleF nR = new(left, y, nameW, 16f);
                RectangleF dR = new(nR.Right + gap, y, dvtW, 16f);
                RectangleF qR = new(dR.Right + gap, y, qtyW, 16f);
                RectangleF pR = new(qR.Right + gap, y, priceW, 16f);
                RectangleF tR = new(pR.Right, y, totalW, 16f);

                e.Graphics.DrawString("Tên hàng", monoFont, Brushes.Black, nR);
                e.Graphics.DrawString("ĐVT", monoFont, Brushes.Black, dR);
                e.Graphics.DrawString("SL", monoFont, Brushes.Black, qR, rightFmt);
                e.Graphics.DrawString("Đơn giá", monoFont, Brushes.Black, pR, rightFmt);
                e.Graphics.DrawString("T.Tiền", monoFont, Brushes.Black, tR, rightFmt);
                y += 15f;
                e.Graphics.DrawLine(Pens.LightGray, left, y, right, y);
                y += 3f;

                foreach (DataRow row in detailTable.Rows)
                {
                    string ten = Convert.ToString(row["Tên nguyên liệu"]) ?? "";
                    string dvt = Convert.ToString(row["Đơn vị tính"]) ?? "";
                    decimal sl = Convert.ToDecimal(row["Số lượng"]);
                    decimal dg = Convert.ToDecimal(row["Đơn giá"]);
                    decimal tt = Convert.ToDecimal(row["Thành tiền"]);

                    nR = new(left, y, nameW, 14f);
                    dR = new(nR.Right + gap, y, dvtW, 14f);
                    qR = new(dR.Right + gap, y, qtyW, 14f);
                    pR = new(qR.Right + gap, y, priceW, 14f);
                    tR = new(pR.Right, y, totalW, 14f);

                    e.Graphics.DrawString(ten, monoFont, Brushes.Black, nR);
                    e.Graphics.DrawString(dvt, monoFont, Brushes.Black, dR);
                    e.Graphics.DrawString($"{sl:N0}", monoFont, Brushes.Black, qR, rightFmt);
                    e.Graphics.DrawString($"{dg:N0}", monoFont, Brushes.Black, pR, rightFmt);
                    e.Graphics.DrawString($"{tt:N0}", monoFont, Brushes.Black, tR, rightFmt);
                    y += 14f;
                }

                y += 2f;
                e.Graphics.DrawLine(Pens.Gray, left, y, right, y);
                y += 4f;

                // Tổng tiền
                DrawLineRA("TỔNG TIỀN:", $"{tongTien:N0} VNĐ", totalFont, Brushes.Black);

                // Bằng chữ
                string bangChu = DocSoTienBangChu(tongTien);
                e.Graphics.DrawString($"Bằng chữ: {bangChu}", normalFont, Brushes.Black, left, y);
                y += normalFont.GetHeight(e.Graphics) + 4f;

                // QR
                if (qrImage is not null)
                {
                    y += 3f;
                    DrawCentered("Mã xác thực nhập kho", normalFont);
                    const float qrSz = 74f;
                    e.Graphics.DrawImage(qrImage, left + (width - qrSz) / 2f, y, qrSz, qrSz);
                    y += qrSz + 6f;
                }

                // Chữ ký
                y += 4f;
                float signY = y;
                float half = width / 2f;
                e.Graphics.DrawString("Người giao hàng", normalFont, Brushes.Black, left, signY);
                e.Graphics.DrawString("Người nhận", normalFont, Brushes.Black, left + half, signY);
                signY += normalFont.GetHeight(e.Graphics) + 16f;
                e.Graphics.DrawLine(Pens.Black, left, signY, left + half - 12f, signY);
                e.Graphics.DrawLine(Pens.Black, left + half, signY, right, signY);
            };

            using PrintPreviewDialog preview = new()
            {
                Document = doc,
                Width = 900,
                Height = 700
            };
            preview.ShowDialog(this);

            qrImage?.Dispose();
        }

        /// <summary>Đọc số tiền ra chữ tiếng Việt.</summary>
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
                if (tram > 0) result = chu[tram] + " trăm";
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

        private void btn_QLKH_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
