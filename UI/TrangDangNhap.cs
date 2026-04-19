using PBL3.Business;

namespace PBL3
{
    public partial class TrangDangNhap : Form
    {
        private bool _isLoggingIn;
        private readonly AuthService _authService;

        public TrangDangNhap()
        {
            InitializeComponent();
            _authService = new AuthService();
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {
            if (sender is Label lbl)
            {
                // Exit the application when click the exit label
                if (lbl == lb_ChuaCoTK || lbl.Text.Equals("Exit", StringComparison.OrdinalIgnoreCase))
                {
                    Application.Exit();
                    return;
                }

                // Open forgot-password when clicking the "Quên mật khẩu" label
                if (lbl == lb_QuenMK || lbl.Text.Contains("Quên"))
                {
                    this.Hide();
                    TrangQuenMatKhau f = new TrangQuenMatKhau();
                    f.Show();
                    return;
                }

                // Fallback: originally used for "Đăng ký ngay"
                if (lbl.Text == "Đăng ký ngay")
                {
                    // keep previous behavior if registration label exists
                    this.Hide();
                    TrangQuenMatKhau f = new TrangQuenMatKhau();
                    f.Show();
                }
            }
        }

        private void roundedPanel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

        }
        private void btn_DangNhap_MouseEnter(object sender, EventArgs e)
        {
            btn_DangNhap.BackColor = Color.FromArgb(255, 69, 0);
        }

        private void btn_DangNhap_MouseLeave(object sender, EventArgs e)
        {
            btn_DangNhap.BackColor = Color.LightSalmon;
        }

        private void lb_QuenMK_MouseEnter(object sender, EventArgs e)
        {
            lb_QuenMK.ForeColor = Color.FromArgb(255, 69, 0);
        }

        private void lb_QuenMK_MouseLeave(object sender, EventArgs e)
        {
            lb_QuenMK.ForeColor = Color.LightSalmon;
        }

        private void lb_DangKi_MouseEnter(object sender, EventArgs e)
        {
            lb_ChuaCoTK.ForeColor = Color.FromArgb(255, 69, 0);
        }

        private void lb_DangKi_MouseLeave(object sender, EventArgs e)
        {
            lb_ChuaCoTK.ForeColor = Color.LightSalmon;
        }
        private void hcnt_KhungDangNhap_Paint(object sender, PaintEventArgs e)
        {

        }

        private void TrangDangNhap_Load(object sender, EventArgs e)
        {
            try
            {
                _authService.CheckDatabaseConnection();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Kết nối database thất bại.\n{ex.Message}\n\nKiểm tra lại SQL Server đang chạy và chuỗi kết nối `QL_FASTFOOD` trong `App.config`.",
                    "Lỗi kết nối",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void btn_DangNhap_Click(object sender, EventArgs e)
        {
            if (_isLoggingIn)
            {
                return;
            }

            string soDienThoai = txt_TaiKhoan.Text.Trim();
            string matKhau = txt_MatKhau.Text.Trim();

            _isLoggingIn = true;
            btn_DangNhap.Enabled = false;
            lb_DangNhap.Enabled = false;

            try
            {
                var result = _authService.Authenticate(soDienThoai, matKhau);
                if (!result.IsSuccess)
                {
                    MessageBox.Show(result.ErrorMessage,
                                    "Thông báo",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);
                    return;
                }

                Form target = result.IsAdmin
                    ? new QuanLiNhanVien()
                    : new TrangNhanVien1(result.MaNV);

                this.Hide();
                target.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
            finally
            {
                if (!IsDisposed)
                {
                    _isLoggingIn = false;
                    btn_DangNhap.Enabled = true;
                    lb_DangNhap.Enabled = true;
                }
            }
        }
    }
}


