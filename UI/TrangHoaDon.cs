using PBL3.Business;
using System;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace PBL3.UI
{
    public partial class TrangHoaDon : Form
    {
        private readonly TrangHoaDonService _trangHoaDonService;
        private readonly string _loggedInMaNV;
        private string _loaiHoaDon = "ban";
        private string? _selectedMaHD;
        private DataTable? _masterTable;
        private string _printContent = string.Empty;
        private Image? _printQrImage;
        private string _printMaHd = string.Empty;
        private string _printNhanVien = string.Empty;
        private string _printDoiTac = string.Empty;
        private DateTime _printThoiGian;
        private decimal _printTongTien;
        private string _printTenHang = string.Empty;
        private int _printPhanTramGiam;
        private int _printDiemTichLuy;
        private decimal _printTienHangGoc;
        private string _printSdtNcc = string.Empty;

        public TrangHoaDon() : this("1") { }

        public TrangHoaDon(string maNV)
        {
            _trangHoaDonService = new TrangHoaDonService();
            _loggedInMaNV = maNV;
            InitializeComponent();
        }

        private void TrangHoaDon_Load(object sender, EventArgs e)
        {
            btn_QLNCC.BackColor = Color.Salmon;
            label5.ForeColor = Color.White;

            _dtpNgay.Value = DateTime.Now;
            _dtpNgay.Enabled = false;
            lblBtnHuyHoaDon.Text = "Yêu cầu hủy hóa đơn";

            _btnHoaDonBan.Click += (s, ev) => ChuyenLoaiHoaDon("ban");
            _btnHoaDonNhap.Click += (s, ev) => ChuyenLoaiHoaDon("nhap");
            _txtTimMaHD.TextChanged += (s, ev) => ApplyFilter();
            _dgvHoaDonMaster.SelectionChanged += (s, ev) => LoadSelectedHoaDon();
            _btnHuyHoaDon.Click += _btnHuyHoaDon_Click;
            lblBtnHuyHoaDon.Click += _btnHuyHoaDon_Click;
            _btnLamMoi.Click += _btnLamMoi_Click;
            lblBtnLamMoi.Click += _btnLamMoi_Click;
            _btnInLai.Click += _btnInLai_Click;
            lblBtnInLai.Click += _btnInLai_Click;

            // `_pnlKhachHangView`, `_lblKhachHangView` and `_lblTongCongTatCa` are created in the Designer so they
            // are available in design view. At runtime we only update their Text values.

            ChuyenLoaiHoaDon("ban");
        }

        private void ChuyenLoaiHoaDon(string loai)
        {
            _loaiHoaDon = loai;
            bool isBan = _loaiHoaDon == "ban";

            _btnHoaDonBan.BackColor = isBan ? Color.Coral : Color.BurlyWood;
            _btnHoaDonBan.ForeColor = isBan ? Color.White : Color.Black;
            _btnHoaDonNhap.BackColor = isBan ? Color.BurlyWood : Color.Coral;
            _btnHoaDonNhap.ForeColor = isBan ? Color.Black : Color.White;

            lblTitle.Text = isBan ? "Hóa đơn bán" : "Hóa đơn nhập";
            lblHoTen.Text = isBan ? "Khách hàng" : "Nhà cung cấp";

            LoadHoaDonMaster();
        }

        private void LoadHoaDonMaster()
        {
            _masterTable = _trangHoaDonService.GetHoaDonMaster(_loaiHoaDon, _loggedInMaNV);
            _dgvHoaDonMaster.DataSource = _masterTable;

            if (_dgvHoaDonMaster.Columns.Contains("MaHD")) _dgvHoaDonMaster.Columns["MaHD"].HeaderText = "Mã HD";

            if (_dgvHoaDonMaster.Columns.Contains("ThoiGian"))
            {
                _dgvHoaDonMaster.Columns["ThoiGian"].HeaderText = "Thời gian";
                _dgvHoaDonMaster.Columns["ThoiGian"].DefaultCellStyle.Format = "HH:mm - dd/MM";
            }

            if (_dgvHoaDonMaster.Columns.Contains("TongTien"))
            {
                _dgvHoaDonMaster.Columns["TongTien"].HeaderText = "Tổng tiền";
                _dgvHoaDonMaster.Columns["TongTien"].DefaultCellStyle.Format = "N0";
                _dgvHoaDonMaster.Columns["TongTien"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }

            if (_dgvHoaDonMaster.Columns.Contains("DoiTac"))
            {
                _dgvHoaDonMaster.Columns["DoiTac"].HeaderText = _loaiHoaDon == "ban" ? "Khách hàng" : "Nhà cung cấp";
            }

            if (_dgvHoaDonMaster.Columns.Contains("NhanVien")) _dgvHoaDonMaster.Columns["NhanVien"].Visible = false;
            if (_dgvHoaDonMaster.Columns.Contains("SDT")) _dgvHoaDonMaster.Columns["SDT"].Visible = false;
            if (_dgvHoaDonMaster.Columns.Contains("TenHang")) _dgvHoaDonMaster.Columns["TenHang"].Visible = false;
            if (_dgvHoaDonMaster.Columns.Contains("PhanTramGiam")) _dgvHoaDonMaster.Columns["PhanTramGiam"].Visible = false;
            if (_dgvHoaDonMaster.Columns.Contains("DiemTichLuy")) _dgvHoaDonMaster.Columns["DiemTichLuy"].Visible = false;
            if (_dgvHoaDonMaster.Columns.Contains("DoiTac")) _dgvHoaDonMaster.Columns["DoiTac"].Visible = false;

            decimal tongTatCa = 0;
            foreach (DataRow row in _masterTable.Rows)
            {
                tongTatCa += Convert.ToDecimal(row["TongTien"] ?? 0);
            }
            if (_lblTongCongTatCa != null)
            {
                _lblTongCongTatCa.Text = $"Tổng cộng: {tongTatCa:N0} đ";
            }

            if (_dgvHoaDonMaster.Rows.Count > 0)
            {
                _dgvHoaDonMaster.Rows[0].Selected = true;
                _dgvHoaDonMaster.CurrentCell = _dgvHoaDonMaster.Rows[0].Cells[0];
                LoadSelectedHoaDon();
            }
            else
            {
                ClearReceipt();
            }
        }

        private void ApplyFilter()
        {
            if (_masterTable is null) return;

            string key = _txtTimMaHD.Text.Trim().Replace("'", "''");
            string filter = "1=1";
            if (!string.IsNullOrWhiteSpace(key))
                filter += $" AND (CONVERT(MaHD, 'System.String') LIKE '%{key}%' OR CONVERT(SDT, 'System.String') LIKE '%{key}%')";

            _masterTable.DefaultView.RowFilter = filter;
        }

        private void LoadSelectedHoaDon()
        {
            if (_dgvHoaDonMaster.CurrentRow is null)
            {
                ClearReceipt();
                return;
            }

            _selectedMaHD = _dgvHoaDonMaster.CurrentRow.Cells["MaHD"].Value?.ToString();
            if (string.IsNullOrWhiteSpace(_selectedMaHD))
            {
                ClearReceipt();
                return;
            }

            txtMaHDB.Text = _selectedMaHD;

            if (_dgvHoaDonMaster.CurrentRow.Cells["ThoiGian"].Value is DateTime dt)
                _dtpNgay.Value = dt;

            lblReceiptNhanVien.Text = $"👤 Nhân viên: {_dgvHoaDonMaster.CurrentRow.Cells["NhanVien"].Value}";
            lblReceiptDoiTac.Text = $"🤝 {(_loaiHoaDon == "ban" ? "Khách hàng" : "Nhà cung cấp")}: {_dgvHoaDonMaster.CurrentRow.Cells["DoiTac"].Value}";
            if (_lblKhachHangView != null) _lblKhachHangView.Text = Convert.ToString(_dgvHoaDonMaster.CurrentRow.Cells["DoiTac"].Value) ?? "-";

            LoadHoaDonDetail(_selectedMaHD);
        }

        private void LoadHoaDonDetail(string maHd)
        {
            DataTable dt = _trangHoaDonService.GetHoaDonDetail(_loaiHoaDon, maHd);
            _dgvHoaDonDetail.DataSource = dt;

            if (_dgvHoaDonDetail.Columns.Contains("TenHang"))
            {
                _dgvHoaDonDetail.Columns["TenHang"].HeaderText = "Tên món";
                _dgvHoaDonDetail.Columns["TenHang"].Width = 180;
            }
            if (_dgvHoaDonDetail.Columns.Contains("SoLuong"))
            {
                _dgvHoaDonDetail.Columns["SoLuong"].HeaderText = "SL";
                _dgvHoaDonDetail.Columns["SoLuong"].Width = 40;
                _dgvHoaDonDetail.Columns["SoLuong"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
            if (_dgvHoaDonDetail.Columns.Contains("DonGia"))
            {
                _dgvHoaDonDetail.Columns["DonGia"].HeaderText = "Đơn giá";
                _dgvHoaDonDetail.Columns["DonGia"].Width = 100;
                _dgvHoaDonDetail.Columns["DonGia"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
            if (_dgvHoaDonDetail.Columns.Contains("ThanhTien"))
            {
                _dgvHoaDonDetail.Columns["ThanhTien"].HeaderText = "Thành Tiền";
                _dgvHoaDonDetail.Columns["ThanhTien"].Width = 110;
                _dgvHoaDonDetail.Columns["ThanhTien"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }

            if (_dgvHoaDonDetail.Columns.Contains("DonGia")) _dgvHoaDonDetail.Columns["DonGia"].DefaultCellStyle.Format = "N0";
            if (_dgvHoaDonDetail.Columns.Contains("ThanhTien")) _dgvHoaDonDetail.Columns["ThanhTien"].DefaultCellStyle.Format = "N0";
        }

        private void _btnHuyHoaDon_Click(object? sender, EventArgs e)
        {
            if (_loaiHoaDon != "ban")
            {
                MessageBox.Show("Chỉ hỗ trợ yêu cầu hủy hóa đơn bán.");
                return;
            }

            if (string.IsNullOrWhiteSpace(_selectedMaHD))
            {
                MessageBox.Show("Vui lòng chọn hóa đơn cần hủy.");
                return;
            }

            if (!TryGetSelectedInvoiceTime(out DateTime createdAt))
            {
                MessageBox.Show("Không đọc được thời gian hóa đơn.");
                return;
            }

            if (DateTime.Now.Subtract(createdAt).TotalMinutes > 10)
            {
                MessageBox.Show("Hóa đơn đã quá 10 phút nên không thể yêu cầu hủy.");
                return;
            }

            string? lyDo = PromptForCancelReason();
            if (string.IsNullOrWhiteSpace(lyDo))
            {
                return;
            }

            try
            {
                bool updated = _trangHoaDonService.RequestCancelBanInvoice(_selectedMaHD, lyDo);
                if (!updated)
                {
                    MessageBox.Show("Không thể gửi yêu cầu hủy. Vui lòng kiểm tra lại.");
                    return;
                }

                LoadHoaDonMaster();
                ClearReceipt();
                MessageBox.Show("Đã gửi yêu cầu hủy hóa đơn.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể gửi yêu cầu hủy hóa đơn.\n{ex.Message}");
            }
        }

        private bool TryGetSelectedInvoiceTime(out DateTime invoiceTime)
        {
            invoiceTime = DateTime.MinValue;
            object? value = _dgvHoaDonMaster.CurrentRow?.Cells["ThoiGian"]?.Value;
            if (value is DateTime dt)
            {
                invoiceTime = dt;
                return true;
            }

            return DateTime.TryParse(Convert.ToString(value), out invoiceTime);
        }

        private static string? PromptForPassword(string title)
        {
            using Form prompt = new Form();
            prompt.Width = 360;
            prompt.Height = 170;
            prompt.Text = "Xác nhận quyền";
            prompt.FormBorderStyle = FormBorderStyle.FixedDialog;
            prompt.StartPosition = FormStartPosition.CenterParent;

            Label lbl = new Label { Left = 16, Top = 16, Width = 320, Text = title };
            TextBox txt = new TextBox { Left = 16, Top = 46, Width = 320, PasswordChar = '*' };
            Button ok = new Button { Text = "Xác nhận", Left = 176, Width = 76, Top = 80, DialogResult = DialogResult.OK };
            Button cancel = new Button { Text = "Hủy", Left = 260, Width = 76, Top = 80, DialogResult = DialogResult.Cancel };

            prompt.Controls.Add(lbl);
            prompt.Controls.Add(txt);
            prompt.Controls.Add(ok);
            prompt.Controls.Add(cancel);
            prompt.AcceptButton = ok;
            prompt.CancelButton = cancel;

            return prompt.ShowDialog() == DialogResult.OK ? txt.Text : null;
        }

        private static string? PromptForCancelReason()
        {
            using Form prompt = new Form();
            prompt.Width = 420;
            prompt.Height = 260;
            prompt.Text = "Lý do hủy";
            prompt.FormBorderStyle = FormBorderStyle.FixedDialog;
            prompt.StartPosition = FormStartPosition.CenterParent;

            Label lbl = new Label { Left = 16, Top = 16, Width = 380, Text = "Nhập lý do hủy:" };
            RichTextBox rtb = new RichTextBox { Left = 16, Top = 46, Width = 380, Height = 110 };
            Button ok = new Button { Text = "Xác nhận", Left = 216, Width = 80, Top = 170, DialogResult = DialogResult.OK };
            Button cancel = new Button { Text = "Hủy", Left = 306, Width = 80, Top = 170, DialogResult = DialogResult.Cancel };

            prompt.Controls.Add(lbl);
            prompt.Controls.Add(rtb);
            prompt.Controls.Add(ok);
            prompt.Controls.Add(cancel);
            prompt.AcceptButton = ok;
            prompt.CancelButton = cancel;

            return prompt.ShowDialog() == DialogResult.OK ? rtb.Text.Trim() : null;
        }

        private bool ValidateManagerPassword(string password)
        {
            return _trangHoaDonService.ValidateManagerPassword(password);
        }

        private void ClearReceipt()
        {
            _selectedMaHD = null;
            txtMaHDB.Clear();
            _dtpNgay.Value = DateTime.Now;
            lblReceiptNhanVien.Text = "👤 Nhân viên: -";
            lblReceiptDoiTac.Text = _loaiHoaDon == "ban" ? "🤝 Khách hàng: -" : "🤝 Nhà cung cấp: -";
            _dgvHoaDonDetail.DataSource = null;
            if (_lblKhachHangView != null) _lblKhachHangView.Text = "-";
        }

        private void _btnLamMoi_Click(object? sender, EventArgs e)
        {
            _txtTimMaHD.Clear();
            LoadHoaDonMaster();
            ClearReceipt();
        }

        private void _btnInLai_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_selectedMaHD))
            {
                MessageBox.Show("Vui lòng chọn hóa đơn để in lại.");
                return;
            }

            if (_dgvHoaDonMaster.CurrentRow is null)
            {
                MessageBox.Show("Vui lòng chọn hóa đơn để in lại.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DataGridViewRow row = _dgvHoaDonMaster.CurrentRow;
            decimal tongTien = Convert.ToDecimal(row.Cells["TongTien"].Value ?? 0m, CultureInfo.InvariantCulture);
            DateTime thoiGian = Convert.ToDateTime(row.Cells["ThoiGian"].Value, CultureInfo.InvariantCulture);
            _printMaHd = Convert.ToString(row.Cells["MaHD"].Value) ?? string.Empty;
            _printNhanVien = Convert.ToString(row.Cells["NhanVien"].Value) ?? "-";
            _printDoiTac = Convert.ToString(row.Cells["DoiTac"].Value) ?? "-";
            _printThoiGian = thoiGian;
            _printTongTien = tongTien;

            // Lấy thông tin hạng, điểm, tiền gốc cho hóa đơn bán
            if (_loaiHoaDon == "ban")
            {
                _printTenHang = Convert.ToString(row.Cells["TenHang"].Value) ?? string.Empty;
                _printPhanTramGiam = Convert.ToInt32(row.Cells["PhanTramGiam"].Value ?? 0);
                _printDiemTichLuy = Convert.ToInt32(row.Cells["DiemTichLuy"].Value ?? 0);
                _printTienHangGoc = _trangHoaDonService.GetTienHangGoc(_printMaHd);
                _printSdtNcc = string.Empty;
            }
            else
            {
                _printTenHang = string.Empty;
                _printPhanTramGiam = 0;
                _printDiemTichLuy = 0;
                _printTienHangGoc = 0;
                _printSdtNcc = Convert.ToString(row.Cells["SDT"].Value) ?? string.Empty;
            }

            string qrPayload;
            if (_loaiHoaDon == "nhap")
            {
                qrPayload = BuildNhapKhoQrId(_printMaHd, _printThoiGian);
            }
            else
            {
                string vnPayCode = BuildVnPayCode(Convert.ToString(row.Cells["MaHD"].Value), tongTien, thoiGian);
                qrPayload = BuildVnPayQrPayload(vnPayCode, tongTien, thoiGian);
            }

            _printQrImage?.Dispose();
            _printQrImage = TryCreateQrImage(qrPayload, 170);

            _printContent = string.Empty;

            PrintDocument doc = new PrintDocument();
            // Configure a narrow receipt-sized page (width in hundredths of an inch)
            // ~80mm receipt width ≈ 3.15in -> 315 hundredths
            doc.DefaultPageSettings.PaperSize = new PaperSize("Receipt", 315, 1200);
            doc.DefaultPageSettings.Margins = new Margins(10, 10, 10, 10);
            doc.PrintPage += Doc_PrintPage;

            using PrintPreviewDialog preview = new PrintPreviewDialog
            {
                Document = doc,
                Width = 900,
                Height = 700
            };

            preview.ShowDialog(this);
        }

        private static string BuildVnPayCode(string? maHd, decimal tongTien, DateTime thoiGian)
        {
            string id = string.IsNullOrWhiteSpace(maHd) ? "UNK" : maHd.Trim();
            long amount = decimal.ToInt64(decimal.Round(tongTien, 0, MidpointRounding.AwayFromZero));
            string seed = $"{id}|{amount}|{thoiGian:yyyyMMddHHmmss}";

            using SHA1 sha1 = SHA1.Create();
            byte[] hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(seed));
            string token = BitConverter.ToString(hash, 0, 3).Replace("-", string.Empty);

            return $"VNP-{amount}-{token}";
        }

        private static string BuildVnPayQrPayload(string vnPayCode, decimal tongTien, DateTime thoiGian)
        {
            long amount = decimal.ToInt64(decimal.Round(tongTien, 0, MidpointRounding.AwayFromZero));
            return $"VNPAY|CODE={vnPayCode}|AMOUNT={amount}|TIME={thoiGian:yyyyMMddHHmmss}";
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

        private void Doc_PrintPage(object? sender, PrintPageEventArgs e)
        {
            using Font brandFont = new Font("Segoe UI", 11, FontStyle.Bold);
            using Font titleFont = new Font("Segoe UI", 10, FontStyle.Bold);
            using Font normalFont = new Font("Segoe UI", 8.5f);
            using Font monoFont = new Font("Consolas", 8.5f);
            using Font totalFont = new Font("Segoe UI", 10f, FontStyle.Bold);
            using Font discountFont = new Font("Segoe UI", 8.5f, FontStyle.Italic);

            float left = e.MarginBounds.Left;
            float right = e.MarginBounds.Right;
            float width = e.MarginBounds.Width;
            float y = e.MarginBounds.Top;

            using StringFormat rightFormat = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Near };

            void DrawCentered(string text, Font font)
            {
                SizeF s = e.Graphics.MeasureString(text, font);
                float x = left + (width - s.Width) / 2f;
                e.Graphics.DrawString(text, font, Brushes.Black, x, y);
                y += s.Height + 1f;
            }

            // Vẽ dòng label bên trái, số tiền căn phải
            void DrawLineRightAligned(string label, string amount, Font font, Brush brush)
            {
                e.Graphics.DrawString(label, font, brush, left, y);
                RectangleF amountRect = new RectangleF(left, y, width, font.GetHeight(e.Graphics));
                e.Graphics.DrawString(amount, font, brush, amountRect, rightFormat);
                y += font.GetHeight(e.Graphics) + 1f;
            }

            bool isNhap = _loaiHoaDon == "nhap";

            // === HEADER ===
            DrawCentered("TỨ ĐẠI THIÊN LONG", brandFont);
            DrawCentered("169 Nguyễn Lương Bằng", normalFont);
            DrawCentered("SĐT: 0374895922", normalFont);

            y += 2f;
            e.Graphics.DrawLine(Pens.Gray, left, y, right, y);
            y += 4f;

            // === THÔNG TIN CHUNG ===
            DrawCentered(isNhap ? "PHIẾU NHẬP KHO" : "HÓA ĐƠN BÁN HÀNG", titleFont);
            e.Graphics.DrawString($"Mã hóa đơn: {_printMaHd}", normalFont, Brushes.Black, left, y); y += normalFont.GetHeight(e.Graphics) + 1f;
            e.Graphics.DrawString($"Ngày: {_printThoiGian:dd/MM/yyyy HH:mm}", normalFont, Brushes.Black, left, y); y += normalFont.GetHeight(e.Graphics) + 1f;
            // Nhân viên lên trước Khách hàng
            e.Graphics.DrawString($"{(isNhap ? "Người nhập" : "Nhân viên")}: {_printNhanVien}", normalFont, Brushes.Black, left, y); y += normalFont.GetHeight(e.Graphics) + 1f;
            e.Graphics.DrawString($"{(isNhap ? "Nhà cung cấp" : "Khách hàng")}: {_printDoiTac}", normalFont, Brushes.Black, left, y); y += normalFont.GetHeight(e.Graphics) + 1f;

            // Hiện SĐT NCC nếu là phiếu nhập
            if (isNhap && !string.IsNullOrWhiteSpace(_printSdtNcc))
            {
                e.Graphics.DrawString($"SĐT NCC: {_printSdtNcc}", normalFont, Brushes.Black, left, y); y += normalFont.GetHeight(e.Graphics) + 1f;
            }

            // Hiện hạng thành viên + điểm nếu là hóa đơn bán và có thông tin
            if (!isNhap && !string.IsNullOrWhiteSpace(_printTenHang))
            {
                e.Graphics.DrawString($"Hạng thành viên: {_printTenHang}", normalFont, Brushes.Black, left, y); y += normalFont.GetHeight(e.Graphics) + 1f;
                e.Graphics.DrawString($"Điểm tích lũy: {_printDiemTichLuy:N0}", normalFont, Brushes.Black, left, y); y += normalFont.GetHeight(e.Graphics) + 1f;
            }
            y += 2f;

            e.Graphics.DrawLine(Pens.Gray, left, y, right, y);
            y += 4f;

            // === BẢNG DANH SÁCH ===
            float dvtW = isNhap ? 40f : 0f;
            float qtyW = 24f;
            float priceW = 52f;
            float totalW = 62f;
            const float colGap = 4f;
            float nameW = width - dvtW - qtyW - priceW - totalW - (colGap * (isNhap ? 3f : 2f));

            RectangleF nameRect = new RectangleF(left, y, nameW, 16f);
            RectangleF dvtRect = new RectangleF(nameRect.Right + colGap, y, dvtW, 16f);
            RectangleF qtyRect = new RectangleF((isNhap ? dvtRect.Right : nameRect.Right) + colGap, y, qtyW, 16f);
            RectangleF priceRect = new RectangleF(qtyRect.Right + colGap, y, priceW, 16f);
            RectangleF totalRect = new RectangleF(priceRect.Right, y, totalW, 16f);

            e.Graphics.DrawString(isNhap ? "Tên hàng" : "Tên món", monoFont, Brushes.Black, nameRect);
            if (isNhap) e.Graphics.DrawString("ĐVT", monoFont, Brushes.Black, dvtRect);
            e.Graphics.DrawString("SL", monoFont, Brushes.Black, qtyRect, rightFormat);
            e.Graphics.DrawString("Đơn giá", monoFont, Brushes.Black, priceRect, rightFormat);
            e.Graphics.DrawString("T.Tiền", monoFont, Brushes.Black, totalRect, rightFormat);
            y += 15f;

            e.Graphics.DrawLine(Pens.LightGray, left, y, right, y);
            y += 3f;

            foreach (DataGridViewRow r in _dgvHoaDonDetail.Rows)
            {
                if (r.IsNewRow) continue;

                string ten = Convert.ToString(r.Cells["TenHang"].Value) ?? string.Empty;
                string dvt = isNhap && _dgvHoaDonDetail.Columns.Contains("DonViTinh") ? (Convert.ToString(r.Cells["DonViTinh"].Value) ?? string.Empty) : string.Empty;
                string sl = Convert.ToString(r.Cells["SoLuong"].Value) ?? "0";
                decimal dg = 0m;
                decimal tt = 0m;
                try { dg = Convert.ToDecimal(r.Cells["DonGia"].Value ?? 0m, CultureInfo.InvariantCulture); } catch { }
                try { tt = Convert.ToDecimal(r.Cells["ThanhTien"].Value ?? 0m, CultureInfo.InvariantCulture); } catch { }

                nameRect = new RectangleF(left, y, nameW, 14f);
                dvtRect = new RectangleF(nameRect.Right + colGap, y, dvtW, 14f);
                qtyRect = new RectangleF((isNhap ? dvtRect.Right : nameRect.Right) + colGap, y, qtyW, 14f);
                priceRect = new RectangleF(qtyRect.Right + colGap, y, priceW, 14f);
                totalRect = new RectangleF(priceRect.Right, y, totalW, 14f);

                e.Graphics.DrawString(ten, monoFont, Brushes.Black, nameRect);
                if (isNhap) e.Graphics.DrawString(dvt, monoFont, Brushes.Black, dvtRect);
                e.Graphics.DrawString(sl, monoFont, Brushes.Black, qtyRect, rightFormat);
                e.Graphics.DrawString($"{dg:N0}", monoFont, Brushes.Black, priceRect, rightFormat);
                e.Graphics.DrawString($"{tt:N0}", monoFont, Brushes.Black, totalRect, rightFormat);

                y += 14f;
            }

            y += 2f;
            e.Graphics.DrawLine(Pens.Gray, left, y, right, y);
            y += 4f;

            // === PHẦN THANH TOÁN ===
            if (isNhap)
            {
                DrawLineRightAligned("TỔNG TIỀN:", $"{_printTongTien:N0} VNĐ", totalFont, Brushes.Black);
                // Số tiền bằng chữ
                string bangChu = DocSoTienBangChu(_printTongTien);
                e.Graphics.DrawString($"Bằng chữ: {bangChu}", normalFont, Brushes.Black, left, y); y += normalFont.GetHeight(e.Graphics) + 1f;
                y += 4f;
            }
            else
            {
                // Tính toán giảm giá
                decimal tienHang = _printTienHangGoc > 0 ? _printTienHangGoc : _printTongTien;
                decimal giamHang = _printPhanTramGiam > 0 ? tienHang * _printPhanTramGiam / 100m : 0m;
                decimal giamDiem = tienHang > _printTongTien + giamHang ? (tienHang - _printTongTien - giamHang) : 0m;
                // Ước tính điểm đã dùng: mỗi điểm = 1000đ (theo logic BanHang)
                int diemDaDung = giamDiem > 0 ? (int)(giamDiem / 1000m) : 0;
                decimal tongGiam = giamHang + giamDiem;
                bool coGiamGia = tongGiam > 0;

                if (coGiamGia)
                {
                    // Tiền hàng gốc
                    DrawLineRightAligned("Tiền hàng:", $"{tienHang:N0} đ", normalFont, Brushes.Black);

                    // Giảm giá hạng (chỉ hiện nếu hạng Vàng/Kim cương - tức PhanTramGiam > 0)
                    if (_printPhanTramGiam > 0)
                    {
                        DrawLineRightAligned($"Giảm giá hạng ({_printTenHang} - {_printPhanTramGiam}%):", $"- {giamHang:N0} đ", discountFont, Brushes.Black);
                    }

                    // Tiêu điểm (hiện nếu khách có dùng điểm)
                    if (diemDaDung > 0)
                    {
                        DrawLineRightAligned($"Tiêu điểm ({diemDaDung} điểm):", $"- {giamDiem:N0} đ", discountFont, Brushes.Black);
                    }

                    // Tổng giảm giá
                    DrawLineRightAligned("Tổng giảm giá:", $"- {tongGiam:N0} đ", normalFont, Brushes.Black);
                    y += 2f;
                }

                // TỔNG CỘNG - in đậm, cỡ to
                DrawLineRightAligned("TỔNG CỘNG:", $"{_printTongTien:N0} đ", totalFont, Brushes.Black);
                y += 2f;
            }

            if (_printQrImage is not null)
            {
                y += 3f;
                if (isNhap)
                {
                    DrawCentered("Mã xác thực nhập kho", normalFont);
                }
                else
                {
                    DrawCentered("Quét mã để thanh toán", normalFont);
                }

                const float qrSize = 74f;
                float qrX = left + (width - qrSize) / 2f;
                e.Graphics.DrawImage(_printQrImage, qrX, y, qrSize, qrSize);
                y += qrSize + 6f;
            }

            if (isNhap)
            {
                y += 4f;
                float signY = y;
                float half = width / 2f;
                e.Graphics.DrawString("Người giao hàng", normalFont, Brushes.Black, left, signY);
                e.Graphics.DrawString("Người nhận", normalFont, Brushes.Black, left + half, signY);
                signY += normalFont.GetHeight(e.Graphics) + 16f;
                e.Graphics.DrawLine(Pens.Black, left, signY, left + half - 12f, signY);
                e.Graphics.DrawLine(Pens.Black, left + half, signY, right, signY);
            }
            else
            {
                DrawCentered("Cảm ơn Quý khách. Hẹn gặp lại!", normalFont);
            }
        }

        private void ThongTinCaNhan_Click(object sender, EventArgs e) => OpenAndClose(new TrangNhanVien1(_loggedInMaNV));
        private void btn_QLKH_Click(object sender, EventArgs e) => OpenAndClose(new BanHang(_loggedInMaNV));
        private void btn_QLMA_Click(object sender, EventArgs e) => OpenAndClose(new MuaHang(_loggedInMaNV));
        private void btn_QLHDN_Click(object sender, EventArgs e) => OpenAndClose(new KhachHang(_loggedInMaNV));
        private void btn_QLNCC_Click(object sender, EventArgs e) => _btnLamMoi_Click(sender, e);

        private void btn_DangXuat_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Bạn có chắc muốn đăng xuất?", "Đăng xuất", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                PBL3.AdminNavigationManager.Logout(this);
            }
        }

        private void OpenAndClose(Form target) => PBL3.AdminNavigationManager.Navigate(this, target);

        private void lblHoTen_Click(object sender, EventArgs e) { }
        private void lblMatKhau_Click(object sender, EventArgs e) { }
        private void _txtMaNV_TextChanged(object sender, EventArgs e) { }

        private void _lblTongCongTatCa_Click(object sender, EventArgs e)
        {

        }

        private void txtKhachHang_TextChanged(object sender, EventArgs e)
        {

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

                if (tram > 0)
                    result = chu[tram] + " trăm";

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
            // Viết hoa chữ đầu
            if (ketQua.Length > 0)
                ketQua = char.ToUpper(ketQua[0]) + ketQua[1..];

            return ketQua + " đồng chẵn";
        }
    }
}
