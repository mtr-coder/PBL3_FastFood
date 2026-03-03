namespace PBL3
{
    partial class TrangDangNhap
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            hcnt_KhungDangNhap = new RoundedPanel();
            lb_hoac = new Label();
            lb_MatKhau = new Label();
            lb_DangKi = new Label();
            lb_ChuaCoTK = new Label();
            lb_QuenMK = new Label();
            lb_TaiKhoan = new Label();
            lb_TieuDeHeThong = new Label();
            hcnt_MatKhau = new RoundedPanel();
            txt_MatKhau = new TextBox();
            hcnt_DangNhap = new RoundedPanel();
            lb_DangNhap = new Label();
            hcnt_TaiKhoan = new RoundedPanel();
            txt_TaiKhoan = new TextBox();
            lb_TieuDeDangNhap = new Label();
            hcnt_KhungDangNhap.SuspendLayout();
            hcnt_MatKhau.SuspendLayout();
            hcnt_DangNhap.SuspendLayout();
            hcnt_TaiKhoan.SuspendLayout();
            SuspendLayout();
            // 
            // hcnt_KhungDangNhap
            // 
            hcnt_KhungDangNhap.BackColor = SystemColors.ButtonHighlight;
            hcnt_KhungDangNhap.Controls.Add(lb_hoac);
            hcnt_KhungDangNhap.Controls.Add(lb_MatKhau);
            hcnt_KhungDangNhap.Controls.Add(lb_DangKi);
            hcnt_KhungDangNhap.Controls.Add(lb_ChuaCoTK);
            hcnt_KhungDangNhap.Controls.Add(lb_QuenMK);
            hcnt_KhungDangNhap.Controls.Add(lb_TaiKhoan);
            hcnt_KhungDangNhap.Controls.Add(lb_TieuDeHeThong);
            hcnt_KhungDangNhap.Controls.Add(hcnt_MatKhau);
            hcnt_KhungDangNhap.Controls.Add(hcnt_DangNhap);
            hcnt_KhungDangNhap.Controls.Add(hcnt_TaiKhoan);
            hcnt_KhungDangNhap.Controls.Add(lb_TieuDeDangNhap);
            hcnt_KhungDangNhap.Location = new Point(360, 152);
            hcnt_KhungDangNhap.Margin = new Padding(3, 4, 3, 4);
            hcnt_KhungDangNhap.Name = "hcnt_KhungDangNhap";
            hcnt_KhungDangNhap.Size = new Size(550, 505);
            hcnt_KhungDangNhap.TabIndex = 0;
            hcnt_KhungDangNhap.Paint += hcnt_KhungDangNhap_Paint;
            // 
            // lb_hoac
            // 
            lb_hoac.AutoSize = true;
            lb_hoac.ForeColor = SystemColors.ControlDarkDark;
            lb_hoac.Location = new Point(145, 419);
            lb_hoac.Name = "lb_hoac";
            lb_hoac.Size = new Size(269, 20);
            lb_hoac.TabIndex = 4;
            lb_hoac.Text = "-------------------hoặc-------------------";
            // 
            // lb_MatKhau
            // 
            lb_MatKhau.AutoSize = true;
            lb_MatKhau.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lb_MatKhau.ForeColor = SystemColors.ControlDarkDark;
            lb_MatKhau.Location = new Point(41, 228);
            lb_MatKhau.Name = "lb_MatKhau";
            lb_MatKhau.Size = new Size(91, 23);
            lb_MatKhau.TabIndex = 3;
            lb_MatKhau.Text = "Mật khẩu:";
            lb_MatKhau.Click += label4_Click;
            // 
            // lbl_DangKi
            // 
            lb_DangKi.AutoSize = true;
            lb_DangKi.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lb_DangKi.ForeColor = Color.Salmon;
            lb_DangKi.Location = new Point(285, 451);
            lb_DangKi.Name = "lb_DangKi";
            lb_DangKi.Size = new Size(121, 23);
            lb_DangKi.TabIndex = 1;
            lb_DangKi.Text = "Đăng ký ngay";
            lb_DangKi.Click += label2_Click;
            lb_DangKi.MouseEnter += lb_DangKi_MouseEnter;
            lb_DangKi.MouseLeave += lb_DangKi_MouseLeave;
            // 
            // lb_ChuaCoTK
            // 
            lb_ChuaCoTK.AutoSize = true;
            lb_ChuaCoTK.Font = new Font("Segoe UI", 10F);
            lb_ChuaCoTK.ForeColor = SystemColors.ControlDarkDark;
            lb_ChuaCoTK.Location = new Point(145, 451);
            lb_ChuaCoTK.Location = new Point(141, 451);
            lb_ChuaCoTK.Name = "lb_ChuaCoTK";
            lb_ChuaCoTK.Size = new Size(157, 23);
            lb_ChuaCoTK.TabIndex = 1;
            lb_ChuaCoTK.Text = "Chưa có tài khoản?";
            lb_ChuaCoTK.Click += label2_Click;
            // 
            // lb_QuenMK
            // 
            lb_QuenMK.AutoSize = true;
            lb_QuenMK.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lb_QuenMK.ForeColor = Color.Salmon;
            lb_QuenMK.Location = new Point(359, 317);
            lb_QuenMK.Name = "lb_QuenMK";
            lb_QuenMK.Size = new Size(140, 23);
            lb_QuenMK.TabIndex = 1;
            lb_QuenMK.Text = "Quên mật khẩu?";
            lb_QuenMK.Click += label2_Click;
            lb_QuenMK.MouseEnter += lb_QuenMK_MouseEnter;
            lb_QuenMK.MouseLeave += lb_QuenMK_MouseLeave;
            // 
            // lb_TaiKhoan
            // 
            lb_TaiKhoan.AutoSize = true;
            lb_TaiKhoan.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lb_TaiKhoan.ForeColor = SystemColors.ControlDarkDark;
            lb_TaiKhoan.Location = new Point(41, 144);
            lb_TaiKhoan.Name = "lb_TaiKhoan";
            lb_TaiKhoan.Size = new Size(92, 23);
            lb_TaiKhoan.TabIndex = 1;
            lb_TaiKhoan.Text = "Tài khoản:";
            lb_TaiKhoan.Click += label2_Click;
            // 
            // lb_TieuDeHeThong
            // 
            lb_TieuDeHeThong.AutoSize = true;
            lb_TieuDeHeThong.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lb_TieuDeHeThong.ForeColor = SystemColors.ControlDarkDark;
            lb_TieuDeHeThong.Location = new Point(25, 81);
            lb_TieuDeHeThong.Location = new Point(104, 102);
            lb_TieuDeHeThong.Name = "lb_TieuDeHeThong";
            lb_TieuDeHeThong.Size = new Size(342, 23);
            lb_TieuDeHeThong.TabIndex = 1;
            lb_TieuDeHeThong.Text = "Hệ thống quản lí cửa hàng thức ăn nhanh";
            lb_TieuDeHeThong.Click += label2_Click;
            // 
            // hcnt_MatKhau
            // 
            hcnt_MatKhau.BackColor = SystemColors.ButtonFace;
            hcnt_MatKhau.Controls.Add(txt_MatKhau);
            hcnt_MatKhau.Location = new Point(41, 257);
            hcnt_MatKhau.Margin = new Padding(3, 4, 3, 4);
            hcnt_MatKhau.Name = "hcnt_MatKhau";
            hcnt_MatKhau.Size = new Size(466, 56);
            hcnt_MatKhau.TabIndex = 2;
            hcnt_MatKhau.Paint += roundedPanel2_Paint;
            // 
            // txt_MatKhau
            // 
            txt_MatKhau.BackColor = SystemColors.ButtonFace;
            txt_MatKhau.BorderStyle = BorderStyle.None;
            txt_MatKhau.Font = new Font("Segoe UI", 12F);
            txt_MatKhau.ForeColor = SystemColors.ActiveCaptionText;
            txt_MatKhau.Location = new Point(23, 12);
            txt_MatKhau.Margin = new Padding(3, 4, 3, 4);
            txt_MatKhau.Name = "txt_MatKhau";
            txt_MatKhau.Size = new Size(422, 27);
            txt_MatKhau.TabIndex = 0;
            // 
            // hcnt_DangNhap
            // 
            hcnt_DangNhap.BackColor = Color.LightSalmon;
            hcnt_DangNhap.Controls.Add(lb_DangNhap);
            hcnt_DangNhap.Location = new Point(145, 359);
            hcnt_DangNhap.Margin = new Padding(3, 4, 3, 4);
            hcnt_DangNhap.Name = "hcnt_DangNhap";
            hcnt_DangNhap.Size = new Size(255, 56);
            hcnt_DangNhap.TabIndex = 2;
            hcnt_DangNhap.Click += hcnt_DangNhap_Click;
            hcnt_DangNhap.Paint += roundedPanel2_Paint;
            hcnt_DangNhap.MouseEnter += hcnt_DangNhap_MouseEnter;
            hcnt_DangNhap.MouseLeave += hcnt_DangNhap_MouseLeave;
            // 
            // lb_DangNhap
            // 
            lb_DangNhap.AutoSize = true;
            lb_DangNhap.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lb_DangNhap.ForeColor = Color.White;
            lb_DangNhap.Location = new Point(80, 15);
            lb_DangNhap.Name = "lb_DangNhap";
            lb_DangNhap.Size = new Size(98, 23);
            lb_DangNhap.TabIndex = 1;
            lb_DangNhap.Text = "Đăng nhập";
            lb_DangNhap.Click += btn_DanhNhap_Click;
            lb_DangNhap.MouseEnter += hcnt_DangNhap_MouseEnter;
            lb_DangNhap.MouseLeave += hcnt_DangNhap_MouseLeave;
            lb_DangNhap.Click += hcnt_DangNhap_Click;
            lb_DangNhap.MouseEnter += hcnt_DangNhap_MouseEnter;
            lb_DangNhap.MouseLeave += hcnt_DangNhap_MouseLeave;
            // 
            // hcnt_TaiKhoan
            // 
            hcnt_TaiKhoan.BackColor = SystemColors.ButtonFace;
            hcnt_TaiKhoan.Controls.Add(txt_TaiKhoan);
            hcnt_TaiKhoan.Location = new Point(41, 173);
            hcnt_TaiKhoan.Margin = new Padding(3, 4, 3, 4);
            hcnt_TaiKhoan.Name = "hcnt_TaiKhoan";
            hcnt_TaiKhoan.Size = new Size(466, 56);
            hcnt_TaiKhoan.TabIndex = 2;
            hcnt_TaiKhoan.Paint += roundedPanel2_Paint;
            // 
            // txt_TaiKhoan
            // 
            txt_TaiKhoan.BackColor = SystemColors.ButtonFace;
            txt_TaiKhoan.BorderStyle = BorderStyle.None;
            txt_TaiKhoan.Font = new Font("Segoe UI", 12F);
            txt_TaiKhoan.ForeColor = SystemColors.ActiveCaptionText;
            txt_TaiKhoan.Location = new Point(23, 12);
            txt_TaiKhoan.Margin = new Padding(3, 4, 3, 4);
            txt_TaiKhoan.Name = "txt_TaiKhoan";
            txt_TaiKhoan.Size = new Size(422, 27);
            txt_TaiKhoan.TabIndex = 0;
            // 
            // lb_TieuDeDangNhap
            // 
            lb_TieuDeDangNhap.AutoSize = true;
            lb_TieuDeDangNhap.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            lb_TieuDeDangNhap.Location = new Point(25, 20);
            lb_TieuDeDangNhap.Location = new Point(175, 41);
            lb_TieuDeDangNhap.Name = "lb_TieuDeDangNhap";
            lb_TieuDeDangNhap.Size = new Size(200, 46);
            lb_TieuDeDangNhap.TabIndex = 0;
            lb_TieuDeDangNhap.Text = "Đăng Nhập";
            // 
            // TrangDangNhap
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = Properties.Resources.mt;
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(1258, 837);
            Controls.Add(hcnt_KhungDangNhap);
            DoubleBuffered = true;
            Margin = new Padding(3, 4, 3, 4);
            Name = "TrangDangNhap";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "TrangDangNhap";
            Load += TrangDangNhap_Load;
            hcnt_KhungDangNhap.ResumeLayout(false);
            hcnt_KhungDangNhap.PerformLayout();
            hcnt_MatKhau.ResumeLayout(false);
            hcnt_MatKhau.PerformLayout();
            hcnt_DangNhap.ResumeLayout(false);
            hcnt_DangNhap.PerformLayout();
            hcnt_TaiKhoan.ResumeLayout(false);
            hcnt_TaiKhoan.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private RoundedPanel hcnt_KhungDangNhap;
        private Label lb_TieuDeDangNhap;
        private Label lb_TieuDeHeThong;
        private RoundedPanel hcnt_TaiKhoan;
        private TextBox txt_TaiKhoan;
        private RoundedPanel hcnt_MatKhau;
        private TextBox txt_MatKhau;
        private Label lb_MatKhau;
        private Label lb_TaiKhoan;
        private RoundedPanel btnDangNhap;
        private Label lb_hoac;
        private Label lb_ChuaCoTK;
        private Label lb_DangNhap;
        private Label lb_DangKi;
        private Label lb_QuenMK;
        private RoundedPanel hcnt_DangNhap;
    }
}
