using PBL3.Business;
using PBL3.Models;
using System.Data;

namespace PBL3
{
    public partial class HopThuYeuCauForm : Form
    {
        private readonly HopThuYeuCauService _service;
        private readonly List<HopThuYeuCauItem> _items = new();
        private HopThuYeuCauItem? _selected;

        public HopThuYeuCauForm()
        {
            _service = new HopThuYeuCauService();
            InitializeComponent();
            _cboLoai.SelectedIndexChanged += (_, __) => LoadRequests();
            _cboTrangThai.SelectedIndexChanged += (_, __) => LoadRequests();
            _btnDuyet.Click += (_, __) => ApproveSelected();
            _btnTuChoi.Click += (_, __) => RejectSelected();
            _btnXoa.Click += (_, __) => DeleteSelected();
            _btnThoat.Click += (_, __) => Close();
            lblBtnThem.Click += (_, __) => ApproveSelected();
            label1.Click += (_, __) => RejectSelected();
            label2.Click += (_, __) => DeleteSelected();
            lblBtnThoat.Click += (_, __) => Close();

            Load += (_, __) => LoadRequests();
        }

        private void LoadRequests()
        {
            try
            {
                _items.Clear();
                _flpCards.Controls.Clear();

                string loai = Convert.ToString(_cboLoai.SelectedItem) ?? "Tất cả";
                string tt = Convert.ToString(_cboTrangThai.SelectedItem) ?? "Tất cả";

                int trangThai = tt switch
                {
                    "Chờ duyệt" => 0,
                    "Đã duyệt" => 1,
                    "Đã từ chối" => 2,
                    _ => -1
                };

                foreach (var item in _service.GetRequests(loai, trangThai))
                {
                    _items.Add(item);
                    _flpCards.Controls.Add(BuildCard(item));
                }

                int pending = _items.Count(x => x.TrangThai == 0);
                Text = pending > 0 ? $"Hộp thư yêu cầu ({pending})" : "Hộp thư yêu cầu";

                if (_items.Count > 0)
                {
                    SelectItem(_items[0]);
                }
                else
                {
                    ClearDetail();
                }
            }
            catch (Exception ex)
            {
                ClearDetail();
                MessageBox.Show($"Không tải được danh sách yêu cầu.\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Control BuildCard(HopThuYeuCauItem item)
        {
            var card = new Panel
            {
                Width = 210,
                Height = 80,
                Margin = new Padding(1),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Cursor = Cursors.Hand,
                Tag = item
            };

            var left = new Panel { Dock = DockStyle.Left, Width = 6, BackColor = GetTypeColor(item.LoaiYeuCau) };
            card.Controls.Add(left);

            var lblName = new Label { Left = 12, Top = 8, AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Text = item.HoTen };
            var lblLoai = new Label { Left = 12, Top = 30, AutoSize = true, ForeColor = GetTypeColor(item.LoaiYeuCau), Text = NormalizeLoaiYeuCau(item.LoaiYeuCau) };
            var lblTime = new Label { Left = 12, Top = 50, AutoSize = true, ForeColor = Color.DimGray, Text = FormatTime(item.NgayGui) };

            card.Controls.Add(lblName);
            card.Controls.Add(lblLoai);
            card.Controls.Add(lblTime);

            void click(object? s, EventArgs e) => SelectItem(item);
            card.Click += click;
            lblName.Click += click;
            lblLoai.Click += click;
            lblTime.Click += click;
            left.Click += click;

            return card;
        }

        private void SelectItem(HopThuYeuCauItem item)
        {
            _selected = item;
            _lblNhanVien.Text = $"Nhân viên: {item.HoTen}";
            _lblChucVu.Text = $"Chức vụ: {item.TenCV}";
            _lblLoai.Text = $"Loại yêu cầu: {NormalizeLoaiYeuCau(item.LoaiYeuCau)}";
            _lblThoiGian.Text = $"Thời gian gửi: {FormatTime(item.NgayGui)}";
            _lblKhoangNgay.Text = item.TuNgay.HasValue && item.DenNgay.HasValue
                ? $"Thời gian nghỉ: {item.TuNgay:dd/MM/yyyy} - {item.DenNgay:dd/MM/yyyy} (Tổng {(item.DenNgay.Value - item.TuNgay.Value).Days + 1} ngày)"
                : "Thời gian nghỉ: -";
            _lblTrangThai.Text = $"Trạng thái: {StateText(item.TrangThai)}";
            _txtNoiDung.Text = item.NoiDung;
            _txtPhanHoi.Text = item.PhanHoiAdmin;

            _lblCaGanNhat.Text = "Ca gần nhất: -";
            _lblLeaveQuota.Text = "Số ngày đã nghỉ: -";
            _lblStaffingImpact.Text = "Ảnh hưởng nhân sự: -";
            LoadLatestShift(item.MaNV);
            LoadHistory(item.MaNV);
            LoadLeaveQuota(item.MaNV);
            LoadStaffingImpact(item);
        }

        private void LoadLeaveQuota(int maNv)
        {
            try
            {
                int count = _service.GetApprovedLeaveCountForMonth(maNv, DateTime.Today);
                _lblLeaveQuota.Text = $"Số ngày đã nghỉ: {count}/3";
            }
            catch
            {
                _lblLeaveQuota.Text = "Số ngày đã nghỉ: -";
            }
        }

        private void LoadStaffingImpact(HopThuYeuCauItem item)
        {
            if (!item.TuNgay.HasValue || !item.DenNgay.HasValue)
            {
                _lblStaffingImpact.Text = "Ảnh hưởng nhân sự: -";
                return;
            }

            try
            {
                DateTime ngay = item.TuNgay.Value.Date;
                DataTable counts = _service.GetStaffingCounts(ngay, ngay);
                int sang = 0;
                int chieu = 0;
                int toi = 0;
                int full = 0;
                if (counts.Rows.Count > 0)
                {
                    DataRow row = counts.Rows[0];
                    sang = row["SoSang"] == DBNull.Value ? 0 : Convert.ToInt32(row["SoSang"]);
                    chieu = row["SoChieu"] == DBNull.Value ? 0 : Convert.ToInt32(row["SoChieu"]);
                    toi = row["SoToi"] == DBNull.Value ? 0 : Convert.ToInt32(row["SoToi"]);
                    full = row["SoFull"] == DBNull.Value ? 0 : Convert.ToInt32(row["SoFull"]);
                }

                DataTable caTruc = _service.GetCaTruc();
                int toiThieuSang = GetSoNguoiToiThieu(caTruc, "1");
                int toiThieuChieu = GetSoNguoiToiThieu(caTruc, "2");

                int thucTeSang = sang + full;
                int thucTeChieu = chieu + full;

                if (item.LoaiYeuCau.Contains("nghỉ phép", StringComparison.OrdinalIgnoreCase))
                {
                    if (thucTeSang > 0) thucTeSang--;
                    if (thucTeChieu > 0) thucTeChieu--;
                }

                string impact = $"Nếu duyệt, ca Sáng {ngay:dd/MM/yyyy} còn {thucTeSang}/{toiThieuSang}, ca Chiều còn {thucTeChieu}/{toiThieuChieu}";
                if (thucTeSang < toiThieuSang || thucTeChieu < toiThieuChieu)
                {
                    impact += " (Thiếu người)";
                    _lblStaffingImpact.ForeColor = Color.IndianRed;
                }
                else
                {
                    _lblStaffingImpact.ForeColor = Color.DarkGreen;
                }

                _lblStaffingImpact.Text = impact;
            }
            catch
            {
                _lblStaffingImpact.Text = "Ảnh hưởng nhân sự: -";
            }
        }

        private static int GetSoNguoiToiThieu(DataTable dt, string maCa)
        {
            DataRow? row = dt.AsEnumerable().FirstOrDefault(r => Convert.ToString(r["MaCa"]) == maCa);
            if (row is null || row["SoNguoiToiThieu"] == DBNull.Value) return 0;
            return Convert.ToInt32(row["SoNguoiToiThieu"]);
        }

        private void LoadLatestShift(int maNv)
        {
            try
            {
                string? latest = _service.GetLatestShift(maNv);
                if (!string.IsNullOrWhiteSpace(latest))
                {
                    _lblCaGanNhat.Text = $"Ca gần nhất: {latest}";
                }
            }
            catch { }
        }

        private void LoadHistory(int maNv)
        {
            _lstHistory.Items.Clear();
            try
            {
                foreach (string line in _service.GetHistory(maNv))
                {
                    string[] parts = line.Split('|');
                    string ngay = parts.Length > 0 ? parts[0] : string.Empty;
                    string loai = parts.Length > 1 ? parts[1] : string.Empty;
                    int st = parts.Length > 2 && int.TryParse(parts[2], out int parsed) ? parsed : 0;
                    _lstHistory.Items.Add($"{ngay} - {NormalizeLoaiYeuCau(loai)} - {StateText(st)}");
                }
            }
            catch { }
        }

        private static string NormalizeLoaiYeuCau(string loai)
        {
            if (string.IsNullOrWhiteSpace(loai)) return string.Empty;

            string value = loai.Trim();
            if (value.Contains("nghỉ phép", StringComparison.OrdinalIgnoreCase)) return "Nghỉ phép";
            if (value.Contains("nghỉ hẳn", StringComparison.OrdinalIgnoreCase) || value.Contains("nghi han", StringComparison.OrdinalIgnoreCase)) return "Nghỉ hẳn";
            return value;
        }

        private void ApproveSelected()
        {
            if (_selected is null) return;

            try
            {
                _service.ApproveRequest(_selected, _txtPhanHoi.Text);
                MessageBox.Show("Đã duyệt yêu cầu.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadRequests();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Duyệt yêu cầu thất bại.\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RejectSelected()
        {
            if (_selected is null) return;
            if (string.IsNullOrWhiteSpace(_txtPhanHoi.Text))
            {
                MessageBox.Show("Bạn phải nhập phản hồi khi từ chối.", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                _service.RejectRequest(_selected.MaYeuCau, _txtPhanHoi.Text);
                MessageBox.Show("Đã từ chối yêu cầu.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadRequests();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Từ chối yêu cầu thất bại.\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeleteSelected()
        {
            if (_selected is null) return;
            if (_selected.TrangThai == 0)
            {
                MessageBox.Show("Không nên xóa yêu cầu đang chờ duyệt.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Xóa yêu cầu này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                _service.DeleteRequest(_selected.MaYeuCau);
                LoadRequests();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Xóa yêu cầu thất bại.\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearDetail()
        {
            _selected = null;
            _lblNhanVien.Text = "Nhân viên: -";
            _lblChucVu.Text = "Chức vụ: -";
            _lblCaGanNhat.Text = "Ca gần nhất: -";
            _lblLeaveQuota.Text = "Số ngày đã nghỉ: -";
            _lblStaffingImpact.Text = "Ảnh hưởng nhân sự: -";
            _lblLoai.Text = "Loại yêu cầu: -";
            _lblThoiGian.Text = "Thời gian gửi: -";
            _lblKhoangNgay.Text = "Thời gian nghỉ: -";
            _lblTrangThai.Text = "Trạng thái: -";
            _txtNoiDung.Clear();
            _txtPhanHoi.Clear();
            _lstHistory.Items.Clear();
        }

        private static Color GetTypeColor(string loai)
        {
            if (loai.Contains("hẳn", StringComparison.OrdinalIgnoreCase) || loai.Contains("han", StringComparison.OrdinalIgnoreCase))
                return Color.IndianRed;
            return Color.SeaGreen;
        }

        private static string StateText(int s) => s switch
        {
            1 => "✅ Đã duyệt",
            2 => "❌ Đã từ chối",
            _ => "⏳ Chờ duyệt"
        };

        private static string FormatTime(DateTime dt)
        {
            if (dt.Date == DateTime.Today)
                return $"Hôm nay, {dt:HH:mm}";
            if (dt.Date == DateTime.Today.AddDays(-1))
                return $"Hôm qua, {dt:HH:mm}";
            return dt.ToString("dd/MM/yyyy HH:mm");
        }

        private void lblLoaiFilter_Click(object sender, EventArgs e)
        {

        }
    }
}
