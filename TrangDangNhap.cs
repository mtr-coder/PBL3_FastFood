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
            // Only open registration when the clicked label is the "Đăng ký ngay" label
            if (sender is Label lbl && lbl.Text == "Đăng ký ngay")
            {
                this.Hide();
                TrangDangKy f = new TrangDangKy();
                f.Show();
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

            using (SqlConnection conn = DbHelper.GetConnection())
            conn.Open();
        }

        private void btn_DangNhap_Click(object sender, EventArgs e)
        {
            string soDienThoai = txt_TaiKhoan.Text.Trim();
            string matKhau = txt_MatKhau.Text.Trim();

            if (string.IsNullOrEmpty(soDienThoai) || string.IsNullOrEmpty(matKhau))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ số điện thoại và mật khẩu!",
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

                    string query = "SELECT MaCV FROM dbo.NHAN_VIEN WHERE SDT = @sdt AND MatKhau = @mk";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@sdt", soDienThoai);
                        cmd.Parameters.AddWithValue("@mk", matKhau);

                        object result = cmd.ExecuteScalar();

                        if (result != null)
                        {
                            string maChucVu = result.ToString().Trim();

                            MessageBox.Show("Đăng nhập thành công!",
                                            "Thông báo",
                                            MessageBoxButtons.OK,
                                            MessageBoxIcon.Information);

                            this.Hide();

                            if (maChucVu == "6")
                            {
                                TrangChuADMIN fAdmin = new TrangChuADMIN();
                                fAdmin.Show();
                            }
                            else
                            {
                                //TrangNhanVien fNV = new TrangNhanVien();
                                //fNV.Show();
                            }
                        }
                        else
                        {
                            MessageBox.Show("Sai số điện thoại hoặc mật khẩu!",
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
