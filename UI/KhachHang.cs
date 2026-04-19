using PBL3.Business;
using PBL3.UI;
using System.Data;
using System.Linq;

namespace PBL3
{
    public partial class KhachHang : Form
    {
        private const int DiemMoiMocGiam = 10;
        private const int TienGiamMoiMoc = 10000;
        private const int NguongCongDiem = 100000;
        private const int DiemCongMoiNguong = 10;

        private readonly string _maNv;
        private readonly KhachHangService _khachHangService;
        private bool _isNavigating;

        public KhachHang() : this("1")
        {
        }

        public KhachHang(string maNv)
        {
            _khachHangService = new KhachHangService();
            _maNv = maNv;
            InitializeComponent();
        }

        private void btn_QLNV_Click(object? sender, EventArgs e) => OpenAndClose(new TrangNhanVien1(_maNv));
        private void btn_QLNCC_Click(object? sender, EventArgs e) => OpenAndClose(new TrangHoaDon(_maNv));
        private void btn_QLKH_Click(object? sender, EventArgs e) => OpenAndClose(new BanHang(_maNv));
        private void btn_QLMA_Click(object? sender, EventArgs e) => OpenAndClose(new MuaHang(_maNv));

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

        private void btnLamMoi_Click(object? sender, EventArgs e)
        {
            LoadKhachHang();
        }

        private void KhachHang_Load(object? sender, EventArgs e)
        {
            LoadKhachHang();
        }

        private void LoadKhachHang()
        {
            DataTable dt = _khachHangService.GetForKhachHangPage();

            dgvKhachHang.DataSource = dt;
            if (dgvKhachHang.Columns.Contains("GiamGiaToiDa"))
            {
                dgvKhachHang.Columns["GiamGiaToiDa"].HeaderText = "Giảm tối đa (đ)";
                dgvKhachHang.Columns["GiamGiaToiDa"].DefaultCellStyle.Format = "N0";
            }

            lblCongThuc.Text = $"Công thức: {DiemMoiMocGiam} điểm = {TienGiamMoiMoc:N0}đ giảm giá | Cộng {DiemCongMoiNguong} điểm mỗi {NguongCongDiem:N0}đ thanh toán";
        }

        private void btnThem_Click(object? sender, EventArgs e)
        {
            string sdt = txtSdt.Text.Trim();
            string ten = txtTen.Text.Trim();

            if (string.IsNullOrWhiteSpace(sdt) || !long.TryParse(sdt, out _))
            {
                MessageBox.Show("SĐT không hợp lệ.", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (IsPhoneExists(sdt))
            {
                MessageBox.Show("SĐT đã tồn tại.", "Trùng dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                if (string.IsNullOrWhiteSpace(ten))
                {
                    ten = _khachHangService.GenerateNextDisplayCode();
                }

                _khachHangService.AddWithName(sdt, ten);

                txtSdt.Clear();
                txtTen.Clear();
                LoadKhachHang();
                MessageBox.Show("Thêm khách hàng thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể thêm khách hàng.\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool IsPhoneExists(string sdt)
        {
            return _khachHangService.IsPhoneExists(true, sdt, null);
        }
    }
}
