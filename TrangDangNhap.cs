namespace PBL3
{
    public partial class TrangDangNhap : Form
    {
        public TrangDangNhap()
        {
            InitializeComponent();
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

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
        private void hcnt_DangNhap_MouseEnter(object sender, EventArgs e)
        {
            hcnt_DangNhap.BackColor = Color.FromArgb(255, 69, 0);
        }

        private void hcnt_DangNhap_MouseLeave(object sender, EventArgs e)
        {
            hcnt_DangNhap.BackColor = Color.LightSalmon;
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
            lb_DangKi.ForeColor = Color.FromArgb(255, 69, 0);
        }

        private void lb_DangKi_MouseLeave(object sender, EventArgs e)
        {
            lb_DangKi.ForeColor = Color.LightSalmon;
        }

        private void hcnt_DangNhap_Click(object sender, EventArgs e)
        {
            string taiKhoan = txt_TaiKhoan.Text.Trim();
            string matKhau = txt_MatKhau.Text.Trim();

            if (taiKhoan == "admin1" && matKhau == "12345")
            {
                TrangChuADMIN trangChuAdmin = new TrangChuADMIN();
                this.Hide();
                trangChuAdmin.ShowDialog();
                this.Close();
            }
            else
            {
                MessageBox.Show("Sai tài khoản hoặc mật khẩu!", "Lỗi");
                txt_TaiKhoan.Clear();
                txt_MatKhau.Clear();
            }
        }

        private void hcnt_KhungDangNhap_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
