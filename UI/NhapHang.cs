using PBL3.Business;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.Net.Http;
using System.Text;

namespace PBL3
{
    public partial class NhapHang : Form
    {
        private readonly NhapKhoService _nhapKhoService;
        private readonly string _nguoiNhap;
        private readonly string _maNv;
        private readonly string? _preselectedMaNl;

        private readonly DataTable _gioNhapTable = new DataTable();
        private string _printContent = string.Empty;
        private string _printMaHdn = string.Empty;
        private string _printTenNcc = string.Empty;
        private DateTime _printNgayNhap;
        private decimal _printTongTien;
        private Image? _printQrImage;

        public NhapHang() : this(null, "Admin", "1")
        {
        }

        public NhapHang(string? maNl, string nguoiNhap, string maNv)
        {
            _nhapKhoService = new NhapKhoService();
            _preselectedMaNl = maNl;
            _nguoiNhap = string.IsNullOrWhiteSpace(nguoiNhap) ? "Admin" : nguoiNhap;
            _maNv = string.IsNullOrWhiteSpace(maNv) ? "1" : maNv;

            InitializeComponent();
            _lblNguoiNhapValue.Text = _nguoiNhap;

            Load += NhapHang_Load;
            _cboNguyenLieu.SelectionChangeCommitted += CboNguyenLieu_SelectionChangeCommitted;
            _btnThem.Click += BtnThem_Click;
            _btnXoaDong.Click += BtnXoaDong_Click;
            _btnXacNhan.Click += BtnXacNhan_Click;
            _btnHuy.Click += (_, __) => Close();
            _txtDonGia.KeyPress += NumericTextBox_KeyPress;
        }

        private void NhapHang_Load(object? sender, EventArgs e)
        {
            _lblNgayNhap.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
            _lblTongTien.Text = "Tổng tiền: 0 VNĐ";

            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                return;
            }

            InitGioNhapGrid();
            LoadNguyenLieu();
            UpdateTongTien();
        }

        private void InitGioNhapGrid()
        {
            _gioNhapTable.Columns.Add("MaNL", typeof(string));
            _gioNhapTable.Columns.Add("TenNL", typeof(string));
            _gioNhapTable.Columns.Add("SoLuong", typeof(decimal));
            _gioNhapTable.Columns.Add("DonGia", typeof(decimal));
            _gioNhapTable.Columns.Add("ThanhTien", typeof(decimal), "SoLuong * DonGia");

            _dgvGioHang.AutoGenerateColumns = false;
            _dgvGioHang.Columns.Clear();
            _dgvGioHang.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "TenNL", HeaderText = "Tên nguyên liệu", FillWeight = 40 });
            _dgvGioHang.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "SoLuong", HeaderText = "Số lượng", FillWeight = 18 });
            _dgvGioHang.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "DonGia",
                HeaderText = "Đơn giá",
                FillWeight = 21,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "#,##0", Alignment = DataGridViewContentAlignment.MiddleRight }
            });
            _dgvGioHang.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ThanhTien",
                HeaderText = "Thành tiền",
                FillWeight = 21,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "#,##0", Alignment = DataGridViewContentAlignment.MiddleRight }
            });

            _dgvGioHang.DataSource = _gioNhapTable;
        }

        private void LoadNhaCungCapByNguyenLieu(string maNl)
        {
            if (!int.TryParse(maNl, out int maNlInt))
            {
                _cboNhaCungCap.DataSource = null;
                return;
            }

            DataTable dt = _nhapKhoService.GetNhaCungCapByNguyenLieu(maNlInt);

            _cboNhaCungCap.DataSource = dt;
            _cboNhaCungCap.DisplayMember = "TenNCC";
            _cboNhaCungCap.ValueMember = "MaNCC";
            _cboNhaCungCap.SelectedIndex = -1;
        }

        private void LoadNguyenLieu()
        {
            DataTable dt = _nhapKhoService.GetNguyenLieu();

            _cboNguyenLieu.DataSource = dt;
            _cboNguyenLieu.DisplayMember = "TenNL";
            _cboNguyenLieu.ValueMember = "MaNL";

            if (!string.IsNullOrWhiteSpace(_preselectedMaNl))
            {
                int idx = -1;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (string.Equals(Convert.ToString(dt.Rows[i]["MaNL"]), _preselectedMaNl, StringComparison.OrdinalIgnoreCase))
                    {
                        idx = i;
                        break;
                    }
                }

                if (idx >= 0)
                {
                    _cboNguyenLieu.SelectedIndex = idx;
                }
            }

            if (_cboNguyenLieu.SelectedIndex < 0 && _cboNguyenLieu.Items.Count > 0)
            {
                _cboNguyenLieu.SelectedIndex = 0;
            }

            SyncDonGiaFromSelection();
            LoadNhaCungCapForCurrentNguyenLieu();
        }

        private void CboNguyenLieu_SelectionChangeCommitted(object? sender, EventArgs e)
        {
            SyncDonGiaFromSelection();
            LoadNhaCungCapForCurrentNguyenLieu();
        }

        private void LoadNhaCungCapForCurrentNguyenLieu()
        {
            if (_cboNguyenLieu.SelectedValue is null)
            {
                _cboNhaCungCap.DataSource = null;
                return;
            }

            string maNl = Convert.ToString(_cboNguyenLieu.SelectedValue) ?? string.Empty;
            if (string.IsNullOrWhiteSpace(maNl))
            {
                _cboNhaCungCap.DataSource = null;
                return;
            }

            LoadNhaCungCapByNguyenLieu(maNl);
        }

        private void SyncDonGiaFromSelection()
        {
            if (_cboNguyenLieu.SelectedItem is not DataRowView drv)
            {
                return;
            }

            decimal gia = drv["GiaNhap"] == DBNull.Value ? 0m : Convert.ToDecimal(drv["GiaNhap"], CultureInfo.InvariantCulture);
            _txtDonGia.Text = gia.ToString("0.##", CultureInfo.InvariantCulture);
        }

        private void BtnThem_Click(object? sender, EventArgs e)
        {
            if (_cboNguyenLieu.SelectedValue is null)
            {
                MessageBox.Show("Vui lòng chọn nguyên liệu.", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal soLuong = _nudSoLuong.Value;
            if (soLuong <= 0)
            {
                MessageBox.Show("Số lượng phải là số dương (> 0).", "Dữ liệu sai", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!TryParsePositiveDecimal(_txtDonGia.Text, out decimal donGia))
            {
                MessageBox.Show("Đơn giá phải là số dương (> 0).", "Dữ liệu sai", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string maNl = Convert.ToString(_cboNguyenLieu.SelectedValue) ?? string.Empty;
            string tenNl = (_cboNguyenLieu.Text ?? string.Empty).Trim();

            DataRow? existed = _gioNhapTable.AsEnumerable().FirstOrDefault(r => string.Equals(Convert.ToString(r["MaNL"]), maNl, StringComparison.OrdinalIgnoreCase));
            if (existed is null)
            {
                _gioNhapTable.Rows.Add(maNl, tenNl, soLuong, donGia);
            }
            else
            {
                existed["SoLuong"] = Convert.ToDecimal(existed["SoLuong"], CultureInfo.InvariantCulture) + soLuong;
                existed["DonGia"] = donGia;
            }

            UpdateTongTien();
        }

        private void BtnXoaDong_Click(object? sender, EventArgs e)
        {
            if (_dgvGioHang.CurrentRow is null || _dgvGioHang.CurrentRow.IsNewRow)
            {
                MessageBox.Show("Vui lòng chọn dòng cần xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            _dgvGioHang.Rows.Remove(_dgvGioHang.CurrentRow);
            UpdateTongTien();
        }

        private void BtnXacNhan_Click(object? sender, EventArgs e)
        {
            if (_cboNhaCungCap.SelectedValue is null)
            {
                MessageBox.Show("Vui lòng chọn nhà cung cấp.", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_gioNhapTable.Rows.Count == 0)
            {
                MessageBox.Show("Giỏ nhập đang trống.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string maNcc = Convert.ToString(_cboNhaCungCap.SelectedValue) ?? string.Empty;
            string tenNcc = Convert.ToString(_cboNhaCungCap.Text) ?? "-";
            decimal tongTien = _gioNhapTable.AsEnumerable().Sum(r => r.Field<decimal>("ThanhTien"));

            try
            {
                string maHdn = _nhapKhoService.SaveNhapKho(_maNv, maNcc, _gioNhapTable);

                BuildPrintContent(maHdn, tenNcc, tongTien);
                ShowPrintPreview();

                MessageBox.Show("Nhập kho thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Nhập kho thất bại.\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateTongTien()
        {
            decimal tongTien = _gioNhapTable.AsEnumerable().Sum(r => r.Field<decimal>("ThanhTien"));
            _lblTongTien.Text = $"Tổng tiền: {tongTien:#,##0} VNĐ";
        }

        private static bool TryParsePositiveDecimal(string input, out decimal value)
        {
            if (decimal.TryParse(input.Trim(), NumberStyles.Number, CultureInfo.CurrentCulture, out value) && value > 0)
            {
                return true;
            }

            if (decimal.TryParse(input.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out value) && value > 0)
            {
                return true;
            }

            value = 0m;
            return false;
        }

        private void NumericTextBox_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar))
            {
                return;
            }

            if (!char.IsDigit(e.KeyChar) && e.KeyChar != '.' && e.KeyChar != ',')
            {
                e.Handled = true;
                return;
            }

            if (sender is TextBox txt && (e.KeyChar == '.' || e.KeyChar == ',') && (txt.Text.Contains('.') || txt.Text.Contains(',')))
            {
                e.Handled = true;
            }
        }

        private void BuildPrintContent(string maHdn, string tenNcc, decimal tongTien)
        {
            _printMaHdn = maHdn;
            _printTenNcc = tenNcc;
            _printNgayNhap = DateTime.Now;
            _printTongTien = tongTien;

            string qrPayload = BuildNhapKhoQrId(_printMaHdn, _printNgayNhap);
            _printQrImage?.Dispose();
            _printQrImage = TryCreateQrImage(qrPayload, 170);
            _printContent = string.Empty;
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

        private void ShowPrintPreview()
        {
            PrintDocument doc = new PrintDocument();
            doc.DefaultPageSettings.PaperSize = new PaperSize("Receipt", 315, 1200);
            doc.DefaultPageSettings.Margins = new Margins(10, 10, 10, 10);
            doc.PrintPage += (_, e) =>
            {
                using Font brandFont = new Font("Segoe UI", 11, FontStyle.Bold);
                using Font titleFont = new Font("Segoe UI", 10, FontStyle.Bold);
                using Font normalFont = new Font("Segoe UI", 8.5f);
                using Font monoFont = new Font("Consolas", 8.5f);
                using Font totalFont = new Font("Segoe UI", 9.5f, FontStyle.Bold);

                float left = e.MarginBounds.Left;
                float right = e.MarginBounds.Right;
                float width = e.MarginBounds.Width;
                float y = e.MarginBounds.Top;

                void DrawCentered(string text, Font font)
                {
                    SizeF s = e.Graphics.MeasureString(text, font);
                    float x = left + (width - s.Width) / 2f;
                    e.Graphics.DrawString(text, font, Brushes.Black, x, y);
                    y += s.Height + 1f;
                }

                DrawCentered("TỨ ĐẠI THIÊN LONG", brandFont);
                DrawCentered("169 Nguyễn Lương Bằng", normalFont);
                DrawCentered("SĐT: 0374895922", normalFont);

                y += 2f;
                e.Graphics.DrawLine(Pens.Gray, left, y, right, y);
                y += 4f;

                DrawCentered("PHIẾU NHẬP KHO", titleFont);
                e.Graphics.DrawString($"Mã hóa đơn: {_printMaHdn}", normalFont, Brushes.Black, left, y); y += normalFont.GetHeight(e.Graphics) + 1f;
                e.Graphics.DrawString($"Ngày: {_printNgayNhap:dd/MM/yyyy HH:mm}", normalFont, Brushes.Black, left, y); y += normalFont.GetHeight(e.Graphics) + 1f;
                e.Graphics.DrawString($"Nhà cung cấp: {_printTenNcc}", normalFont, Brushes.Black, left, y); y += normalFont.GetHeight(e.Graphics) + 1f;
                e.Graphics.DrawString($"Người nhập: {_nguoiNhap}", normalFont, Brushes.Black, left, y); y += normalFont.GetHeight(e.Graphics) + 3f;

                e.Graphics.DrawLine(Pens.Gray, left, y, right, y);
                y += 4f;

                float qtyW = 24f;
                float priceW = 52f;
                float totalW = 62f;
                const float colGap = 4f;
                float nameW = width - qtyW - priceW - totalW - (colGap * 2f);

                RectangleF nameRect = new RectangleF(left, y, nameW, 16f);
                RectangleF qtyRect = new RectangleF(nameRect.Right + colGap, y, qtyW, 16f);
                RectangleF priceRect = new RectangleF(qtyRect.Right + colGap, y, priceW, 16f);
                RectangleF totalRect = new RectangleF(priceRect.Right, y, totalW, 16f);
                using StringFormat rightFormat = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Near };

                e.Graphics.DrawString("Tên hàng", monoFont, Brushes.Black, nameRect);
                e.Graphics.DrawString("SL", monoFont, Brushes.Black, qtyRect, rightFormat);
                e.Graphics.DrawString("Đơn giá", monoFont, Brushes.Black, priceRect, rightFormat);
                e.Graphics.DrawString("T.Tiền", monoFont, Brushes.Black, totalRect, rightFormat);
                y += 15f;

                e.Graphics.DrawLine(Pens.LightGray, left, y, right, y);
                y += 3f;

                foreach (DataRow row in _gioNhapTable.Rows)
                {
                    string ten = Convert.ToString(row["TenNL"]) ?? string.Empty;
                    string sl = Convert.ToDecimal(row["SoLuong"], CultureInfo.InvariantCulture).ToString("0.##", CultureInfo.InvariantCulture);
                    decimal dg = Convert.ToDecimal(row["DonGia"], CultureInfo.InvariantCulture);
                    decimal tt = Convert.ToDecimal(row["ThanhTien"], CultureInfo.InvariantCulture);

                    nameRect = new RectangleF(left, y, nameW, 14f);
                    qtyRect = new RectangleF(nameRect.Right + colGap, y, qtyW, 14f);
                    priceRect = new RectangleF(qtyRect.Right + colGap, y, priceW, 14f);
                    totalRect = new RectangleF(priceRect.Right, y, totalW, 14f);

                    e.Graphics.DrawString(ten, monoFont, Brushes.Black, nameRect);
                    e.Graphics.DrawString(sl, monoFont, Brushes.Black, qtyRect, rightFormat);
                    e.Graphics.DrawString($"{dg:N0}", monoFont, Brushes.Black, priceRect, rightFormat);
                    e.Graphics.DrawString($"{tt:N0}", monoFont, Brushes.Black, totalRect, rightFormat);
                    y += 14f;
                }

                y += 2f;
                e.Graphics.DrawLine(Pens.Gray, left, y, right, y);
                y += 4f;

                e.Graphics.DrawString($"TỔNG TIỀN: {_printTongTien:N0} VNĐ", totalFont, Brushes.Black, left, y);
                y += totalFont.GetHeight(e.Graphics) + 4f;

                if (_printQrImage is not null)
                {
                    y += 3f;
                    DrawCentered("Mã xác thực nhập kho", normalFont);
                    const float qrSize = 74f;
                    float qrX = left + (width - qrSize) / 2f;
                    e.Graphics.DrawImage(_printQrImage, qrX, y, qrSize, qrSize);
                    y += qrSize + 6f;
                }

                y += 4f;
                float signY = y;
                float half = width / 2f;
                e.Graphics.DrawString("Người nhập kho", normalFont, Brushes.Black, left, signY);
                e.Graphics.DrawString("Người nhận hàng", normalFont, Brushes.Black, left + half, signY);
                signY += normalFont.GetHeight(e.Graphics) + 16f;
                e.Graphics.DrawLine(Pens.Black, left, signY, left + half - 12f, signY);
                e.Graphics.DrawLine(Pens.Black, left + half, signY, right, signY);
            };

            using PrintPreviewDialog preview = new PrintPreviewDialog
            {
                Document = doc,
                Width = 900,
                Height = 700
            };

            preview.ShowDialog(this);
        }
    }
}
