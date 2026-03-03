using PBL3.DataBase;
using System.Data.SqlClient;

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
            lb_DangKi.ForeColor = Color.FromArgb(255, 69, 0);
        }

        private void lb_DangKi_MouseLeave(object sender, EventArgs e)
        {
            lb_DangKi.ForeColor = Color.LightSalmon;
        }
        private void hcnt_KhungDangNhap_Paint(object sender, PaintEventArgs e)
        {

        }

        private void TrangDangNhap_Load(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection conn = DbHelper.GetConnection())
                {
                    conn.Open();
                    MessageBox.Show("Kết nối database thành công!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối: " + ex.Message);
            }
        }

        private void btn_DangNhap_Click(object sender, EventArgs e)
        {
            string taiKhoan = txt_TaiKhoan.Text.Trim();
            string matKhau = txt_MatKhau.Text.Trim();

            if (string.IsNullOrEmpty(taiKhoan) || string.IsNullOrEmpty(matKhau))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ tài khoản và mật khẩu!",
                                "Thông báo",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (SqlConnection conn = DbHelper.GetConnection())
                {
                    conn.Open();

                    string query = "SELECT VaiTro FROM TaiKhoan WHERE TenDangNhap = @tk AND MatKhau = @mk";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@tk", taiKhoan);
                        cmd.Parameters.AddWithValue("@mk", matKhau);

                        object result = cmd.ExecuteScalar();

                        if (result != null)
                        {
                            string vaiTro = result.ToString();

                            MessageBox.Show("Đăng nhập thành công!",
                                            "Thông báo",
                                            MessageBoxButtons.OK,
                                            MessageBoxIcon.Information);

                            this.Hide();

                            if (vaiTro == "Admin")
                            {
                                TrangChuADMIN fAdmin = new TrangChuADMIN();
                                fAdmin.Show();
                            }
                            else if (vaiTro == "NhanVien")
                            {
                                //TrangChuNhanVien fNV = new TrangChuNhanVien();
                                //fNV.Show();
                            }
                            else
                            {
                                MessageBox.Show("Vai trò không hợp lệ!");
                                this.Show();
                            }
                        }
                        else
                        {
                            MessageBox.Show("Sai tài khoản hoặc mật khẩu!",
                                            "Lỗi",
                                            MessageBoxButtons.OK,
                                            MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }
    }
}
