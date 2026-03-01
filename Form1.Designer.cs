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
            roundedPanel1 = new RoundedPanel();
            label7 = new Label();
            label4 = new Label();
            lb_DangKi = new Label();
            label8 = new Label();
            lb_QuenMK = new Label();
            label3 = new Label();
            label2 = new Label();
            roundedPanel3 = new RoundedPanel();
            textBox2 = new TextBox();
            btn_DangNhap = new RoundedPanel();
            label6 = new Label();
            roundedPanel2 = new RoundedPanel();
            textBox1 = new TextBox();
            label1 = new Label();
            roundedPanel1.SuspendLayout();
            roundedPanel3.SuspendLayout();
            btn_DangNhap.SuspendLayout();
            roundedPanel2.SuspendLayout();
            SuspendLayout();
            // 
            // roundedPanel1
            // 
            roundedPanel1.BackColor = SystemColors.ButtonHighlight;
            roundedPanel1.Controls.Add(label7);
            roundedPanel1.Controls.Add(label4);
            roundedPanel1.Controls.Add(lb_DangKi);
            roundedPanel1.Controls.Add(label8);
            roundedPanel1.Controls.Add(lb_QuenMK);
            roundedPanel1.Controls.Add(label3);
            roundedPanel1.Controls.Add(label2);
            roundedPanel1.Controls.Add(roundedPanel3);
            roundedPanel1.Controls.Add(btn_DangNhap);
            roundedPanel1.Controls.Add(roundedPanel2);
            roundedPanel1.Controls.Add(label1);
            roundedPanel1.Location = new Point(315, 114);
            roundedPanel1.Name = "roundedPanel1";
            roundedPanel1.Size = new Size(481, 379);
            roundedPanel1.TabIndex = 0;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.ForeColor = SystemColors.ControlDarkDark;
            label7.Location = new Point(127, 314);
            label7.Name = "label7";
            label7.Size = new Size(223, 15);
            label7.TabIndex = 4;
            label7.Text = "-------------------hoặc-------------------";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            label4.ForeColor = SystemColors.ControlDarkDark;
            label4.Location = new Point(36, 171);
            label4.Name = "label4";
            label4.Size = new Size(75, 19);
            label4.TabIndex = 3;
            label4.Text = "Mật khẩu:";
            label4.Click += label4_Click;
            // 
            // lb_DangKi
            // 
            lb_DangKi.AutoSize = true;
            lb_DangKi.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lb_DangKi.ForeColor = Color.Salmon;
            lb_DangKi.Location = new Point(249, 338);
            lb_DangKi.Name = "lb_DangKi";
            lb_DangKi.Size = new Size(101, 19);
            lb_DangKi.TabIndex = 1;
            lb_DangKi.Text = "Đăng ký ngay";
            lb_DangKi.Click += label2_Click;
            lb_DangKi.MouseEnter += lb_DangKi_MouseEnter;
            lb_DangKi.MouseLeave += lb_DangKi_MouseLeave;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Font = new Font("Segoe UI", 10F);
            label8.ForeColor = SystemColors.ControlDarkDark;
            label8.Location = new Point(127, 338);
            label8.Name = "label8";
            label8.Size = new Size(126, 19);
            label8.TabIndex = 1;
            label8.Text = "Chưa có tài khoản?";
            label8.Click += label2_Click;
            // 
            // lb_QuenMK
            // 
            lb_QuenMK.AutoSize = true;
            lb_QuenMK.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lb_QuenMK.ForeColor = Color.Salmon;
            lb_QuenMK.Location = new Point(314, 238);
            lb_QuenMK.Name = "lb_QuenMK";
            lb_QuenMK.Size = new Size(116, 19);
            lb_QuenMK.TabIndex = 1;
            lb_QuenMK.Text = "Quên mật khẩu?";
            lb_QuenMK.Click += label2_Click;
            lb_QuenMK.MouseEnter += lb_QuenMK_MouseEnter;
            lb_QuenMK.MouseLeave += lb_QuenMK_MouseLeave;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            label3.ForeColor = SystemColors.ControlDarkDark;
            label3.Location = new Point(36, 108);
            label3.Name = "label3";
            label3.Size = new Size(77, 19);
            label3.TabIndex = 1;
            label3.Text = "Tài khoản:";
            label3.Click += label2_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            label2.ForeColor = SystemColors.ControlDarkDark;
            label2.Location = new Point(22, 61);
            label2.Name = "label2";
            label2.Size = new Size(282, 19);
            label2.TabIndex = 1;
            label2.Text = "Hệ thống quản lí cửa hàng thức ăn nhanh";
            label2.Click += label2_Click;
            // 
            // roundedPanel3
            // 
            roundedPanel3.BackColor = SystemColors.ButtonFace;
            roundedPanel3.Controls.Add(textBox2);
            roundedPanel3.Location = new Point(36, 193);
            roundedPanel3.Name = "roundedPanel3";
            roundedPanel3.Size = new Size(408, 42);
            roundedPanel3.TabIndex = 2;
            roundedPanel3.Paint += roundedPanel2_Paint;
            // 
            // textBox2
            // 
            textBox2.BackColor = SystemColors.ButtonFace;
            textBox2.BorderStyle = BorderStyle.None;
            textBox2.Font = new Font("Segoe UI", 12F);
            textBox2.ForeColor = SystemColors.ActiveCaptionText;
            textBox2.Location = new Point(20, 9);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(369, 22);
            textBox2.TabIndex = 0;
            // 
            // btn_DangNhap
            // 
            btn_DangNhap.BackColor = Color.LightSalmon;
            btn_DangNhap.Controls.Add(label6);
            btn_DangNhap.Location = new Point(127, 269);
            btn_DangNhap.Name = "btn_DangNhap";
            btn_DangNhap.Size = new Size(223, 42);
            btn_DangNhap.TabIndex = 2;
            btn_DangNhap.Paint += roundedPanel2_Paint;
            btn_DangNhap.MouseEnter += btn_DangNhap_MouseEnter;
            btn_DangNhap.MouseLeave += btn_DangNhap_MouseLeave;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            label6.ForeColor = Color.White;
            label6.Location = new Point(70, 11);
            label6.Name = "label6";
            label6.Size = new Size(81, 19);
            label6.TabIndex = 1;
            label6.Text = "Đăng nhập";
            label6.Click += label2_Click;
            label6.MouseEnter += btn_DangNhap_MouseEnter;
            label6.MouseLeave += btn_DangNhap_MouseLeave;
            // 
            // roundedPanel2
            // 
            roundedPanel2.BackColor = SystemColors.ButtonFace;
            roundedPanel2.Controls.Add(textBox1);
            roundedPanel2.Location = new Point(36, 130);
            roundedPanel2.Name = "roundedPanel2";
            roundedPanel2.Size = new Size(408, 42);
            roundedPanel2.TabIndex = 2;
            roundedPanel2.Paint += roundedPanel2_Paint;
            // 
            // textBox1
            // 
            textBox1.BackColor = SystemColors.ButtonFace;
            textBox1.BorderStyle = BorderStyle.None;
            textBox1.Font = new Font("Segoe UI", 12F);
            textBox1.ForeColor = SystemColors.ActiveCaptionText;
            textBox1.Location = new Point(20, 9);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(369, 22);
            textBox1.TabIndex = 0;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            label1.Location = new Point(22, 15);
            label1.Name = "label1";
            label1.Size = new Size(161, 37);
            label1.TabIndex = 0;
            label1.Text = "Đăng Nhập";
            // 
            // TrangDangNhap
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = Properties.Resources.mt;
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(1101, 628);
            Controls.Add(roundedPanel1);
            DoubleBuffered = true;
            Name = "TrangDangNhap";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Form1";
            roundedPanel1.ResumeLayout(false);
            roundedPanel1.PerformLayout();
            roundedPanel3.ResumeLayout(false);
            roundedPanel3.PerformLayout();
            btn_DangNhap.ResumeLayout(false);
            btn_DangNhap.PerformLayout();
            roundedPanel2.ResumeLayout(false);
            roundedPanel2.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private RoundedPanel roundedPanel1;
        private Label label1;
        private Label label2;
        private RoundedPanel roundedPanel2;
        private TextBox textBox1;
        private RoundedPanel roundedPanel3;
        private TextBox textBox2;
        private Label label4;
        private Label label3;
        private Label label5;
        private RoundedPanel btnDangNhap;
        private Label label7;
        private Label label9;
        private Label label8;
        private Label label6;
        private Label lb_DangKi;
        private Label lb_QuenMK;
        private RoundedPanel btn_DangNhap;
    }
}
