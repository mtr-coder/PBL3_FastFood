using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PBL3
{
    public partial class TrangDangKy : Form
    {
        public TrangDangKy()
        {
            InitializeComponent();
        }

        private void dtp_NgaySinh_ValueChanged(object sender, EventArgs e)
        {
            tbx_NgaySinh.Text = dtp_NgaySinh.Value.ToString("dd/MM/yyyy");
        }

        private void btn_DangKy_Click(object sender, EventArgs e)
        {
            // Validate required fields
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(tbx_MaNhanVien.Text))
                errors.Add("Mã nhân viên không được để trống.");

            if (string.IsNullOrWhiteSpace(tbx_HoTen.Text))
                errors.Add("Họ tên không được để trống.");

            if (string.IsNullOrWhiteSpace(tbx_SoDienThoai.Text))
                errors.Add("Số điện thoại không được để trống.");
            else
            {
                string phone = tbx_SoDienThoai.Text.Trim();
                if (!long.TryParse(phone, out _) || phone.Length < 9)
                    errors.Add("Số điện thoại không hợp lệ.");
            }

            if (string.IsNullOrWhiteSpace(tbx_NgaySinh.Text))
                errors.Add("Ngày sinh không được để trống.");

            if (string.IsNullOrWhiteSpace(tbx_DiaChi.Text))
                errors.Add("Địa chỉ không được để trống.");

            if (!rdb_Nam.Checked && !rdb_Nu.Checked)
                errors.Add("Vui lòng chọn giới tính.");

            if (errors.Count > 0)
            {
                MessageBox.Show(string.Join("\n", errors), "Lỗi đăng ký", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // If validation passed, show success message and return to login
            MessageBox.Show("Đăng ký thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Hide();
            TrangDangNhap f = new TrangDangNhap();
            f.Show();
        }
    }
}
