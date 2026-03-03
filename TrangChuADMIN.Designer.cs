namespace PBL3
{
    partial class TrangChuADMIN
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            roundedPanel1 = new RoundedPanel();
            pb_Admin = new PictureBox();
            btn_DangXuat = new RoundedPanel();
            lb_DangXuat = new Label();
            lb_Admin = new Label();
            hcnt_Khung = new RoundedPanel();
            hcnt_KhungMenuAD = new RoundedPanel();
            btn_ThongKe = new RoundedPanel();
            pb_ThongKe = new PictureBox();
            label10 = new Label();
            btn_QLHDB = new RoundedPanel();
            pb_QLHDB = new PictureBox();
            hcnt_QLHDB = new Label();
            btn_QLHDN = new RoundedPanel();
            pb_QLHDN = new PictureBox();
            label8 = new Label();
            btn_QLMA = new RoundedPanel();
            pb_QLMA = new PictureBox();
            label7 = new Label();
            btn_QLKH = new RoundedPanel();
            pb_QLKH = new PictureBox();
            label6 = new Label();
            btn_QLNCC = new RoundedPanel();
            pb_QLNCC = new PictureBox();
            label5 = new Label();
            btn_QLNV = new RoundedPanel();
            pb_QLNV = new PictureBox();
            label4 = new Label();
            lb_DMQL = new Label();
            roundedPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pb_Admin).BeginInit();
            btn_DangXuat.SuspendLayout();
            hcnt_KhungMenuAD.SuspendLayout();
            btn_ThongKe.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pb_ThongKe).BeginInit();
            btn_QLHDB.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pb_QLHDB).BeginInit();
            btn_QLHDN.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pb_QLHDN).BeginInit();
            btn_QLMA.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pb_QLMA).BeginInit();
            btn_QLKH.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pb_QLKH).BeginInit();
            btn_QLNCC.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pb_QLNCC).BeginInit();
            btn_QLNV.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pb_QLNV).BeginInit();
            SuspendLayout();
            // 
            // roundedPanel1
            // 
            roundedPanel1.Controls.Add(pb_Admin);
            roundedPanel1.Controls.Add(btn_DangXuat);
            roundedPanel1.Controls.Add(lb_Admin);
            roundedPanel1.Controls.Add(hcnt_Khung);
            roundedPanel1.Controls.Add(hcnt_KhungMenuAD);
            roundedPanel1.Location = new Point(12, 12);
            roundedPanel1.Name = "roundedPanel1";
            roundedPanel1.Size = new Size(1078, 608);
            roundedPanel1.TabIndex = 0;
            // 
            // pb_Admin
            // 
            pb_Admin.BackColor = SystemColors.Control;
            pb_Admin.Image = Properties.Resources.admin;
            pb_Admin.Location = new Point(17, 8);
            pb_Admin.Name = "pb_Admin";
            pb_Admin.Size = new Size(38, 38);
            pb_Admin.SizeMode = PictureBoxSizeMode.StretchImage;
            pb_Admin.TabIndex = 2;
            pb_Admin.TabStop = false;
            pb_Admin.Click += pictureBox1_Click;
            // 
            // btn_DangXuat
            // 
            btn_DangXuat.BackColor = Color.SandyBrown;
            btn_DangXuat.Controls.Add(lb_DangXuat);
            btn_DangXuat.Font = new Font("Segoe UI", 9F);
            btn_DangXuat.Location = new Point(170, 12);
            btn_DangXuat.Name = "btn_DangXuat";
            btn_DangXuat.Size = new Size(110, 25);
            btn_DangXuat.TabIndex = 3;
            btn_DangXuat.Click += btn_DangXuat_Click;
            btn_DangXuat.Paint += roundedPanel4_Paint;
            btn_DangXuat.MouseEnter += btn_DangXuat_MouseEnter;
            btn_DangXuat.MouseLeave += btn_DangXuat_MouseLeave;
            // 
            // lb_DangXuat
            // 
            lb_DangXuat.AutoSize = true;
            lb_DangXuat.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lb_DangXuat.ForeColor = SystemColors.ButtonFace;
            lb_DangXuat.Location = new Point(13, 2);
            lb_DangXuat.Name = "lb_DangXuat";
            lb_DangXuat.Size = new Size(81, 20);
            lb_DangXuat.TabIndex = 2;
            lb_DangXuat.Text = "Đăng xuất";
            lb_DangXuat.Click += btn_DangXuat_Click;
            // 
            // lb_Admin
            // 
            lb_Admin.AutoSize = true;
            lb_Admin.Font = new Font("Segoe UI", 12F);
            lb_Admin.Location = new Point(58, 12);
            lb_Admin.Name = "lb_Admin";
            lb_Admin.Size = new Size(56, 21);
            lb_Admin.TabIndex = 2;
            lb_Admin.Text = "Admin";
            lb_Admin.Click += label1_Click;
            // 
            // hcnt_Khung
            // 
            hcnt_Khung.BackColor = Color.Linen;
            hcnt_Khung.Location = new Point(299, 49);
            hcnt_Khung.Name = "hcnt_Khung";
            hcnt_Khung.Size = new Size(761, 538);
            hcnt_Khung.TabIndex = 1;
            // 
            // hcnt_KhungMenuAD
            // 
            hcnt_KhungMenuAD.BackColor = Color.Linen;
            hcnt_KhungMenuAD.Controls.Add(btn_ThongKe);
            hcnt_KhungMenuAD.Controls.Add(btn_QLHDB);
            hcnt_KhungMenuAD.Controls.Add(btn_QLHDN);
            hcnt_KhungMenuAD.Controls.Add(btn_QLMA);
            hcnt_KhungMenuAD.Controls.Add(btn_QLKH);
            hcnt_KhungMenuAD.Controls.Add(btn_QLNCC);
            hcnt_KhungMenuAD.Controls.Add(btn_QLNV);
            hcnt_KhungMenuAD.Controls.Add(lb_DMQL);
            hcnt_KhungMenuAD.Location = new Point(17, 49);
            hcnt_KhungMenuAD.Name = "hcnt_KhungMenuAD";
            hcnt_KhungMenuAD.Size = new Size(263, 538);
            hcnt_KhungMenuAD.TabIndex = 0;
            // 
            // btn_ThongKe
            // 
            btn_ThongKe.BackColor = Color.Bisque;
            btn_ThongKe.Controls.Add(pb_ThongKe);
            btn_ThongKe.Controls.Add(label10);
            btn_ThongKe.Location = new Point(18, 388);
            btn_ThongKe.Name = "btn_ThongKe";
            btn_ThongKe.Size = new Size(224, 40);
            btn_ThongKe.TabIndex = 1;
            // 
            // pb_ThongKe
            // 
            pb_ThongKe.Image = Properties.Resources.thongke;
            pb_ThongKe.Location = new Point(10, 0);
            pb_ThongKe.Name = "pb_ThongKe";
            pb_ThongKe.Size = new Size(38, 38);
            pb_ThongKe.SizeMode = PictureBoxSizeMode.StretchImage;
            pb_ThongKe.TabIndex = 2;
            pb_ThongKe.TabStop = false;
            pb_ThongKe.Click += pictureBox1_Click;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Font = new Font("Segoe UI", 12F);
            label10.ForeColor = SystemColors.ControlText;
            label10.Location = new Point(51, 8);
            label10.Name = "label10";
            label10.Size = new Size(74, 21);
            label10.TabIndex = 0;
            label10.Text = "Thống kê";
            // 
            // btn_QLHDB
            // 
            btn_QLHDB.BackColor = Color.Bisque;
            btn_QLHDB.Controls.Add(pb_QLHDB);
            btn_QLHDB.Controls.Add(hcnt_QLHDB);
            btn_QLHDB.Location = new Point(18, 332);
            btn_QLHDB.Name = "btn_QLHDB";
            btn_QLHDB.Size = new Size(224, 40);
            btn_QLHDB.TabIndex = 1;
            // 
            // pb_QLHDB
            // 
            pb_QLHDB.Image = Properties.Resources.hoadonban1;
            pb_QLHDB.Location = new Point(10, 1);
            pb_QLHDB.Name = "pb_QLHDB";
            pb_QLHDB.Size = new Size(38, 38);
            pb_QLHDB.SizeMode = PictureBoxSizeMode.StretchImage;
            pb_QLHDB.TabIndex = 2;
            pb_QLHDB.TabStop = false;
            pb_QLHDB.Click += pictureBox1_Click;
            // 
            // hcnt_QLHDB
            // 
            hcnt_QLHDB.AutoSize = true;
            hcnt_QLHDB.Font = new Font("Segoe UI", 12F);
            hcnt_QLHDB.ForeColor = SystemColors.ControlText;
            hcnt_QLHDB.Location = new Point(51, 8);
            hcnt_QLHDB.Name = "hcnt_QLHDB";
            hcnt_QLHDB.Size = new Size(152, 21);
            hcnt_QLHDB.TabIndex = 0;
            hcnt_QLHDB.Text = "Quản lí hóa đơn bán";
            // 
            // btn_QLHDN
            // 
            btn_QLHDN.BackColor = Color.Bisque;
            btn_QLHDN.Controls.Add(pb_QLHDN);
            btn_QLHDN.Controls.Add(label8);
            btn_QLHDN.Location = new Point(18, 277);
            btn_QLHDN.Name = "btn_QLHDN";
            btn_QLHDN.Size = new Size(224, 40);
            btn_QLHDN.TabIndex = 1;
            // 
            // pb_QLHDN
            // 
            pb_QLHDN.Image = Properties.Resources.hoadonnhap;
            pb_QLHDN.Location = new Point(10, 0);
            pb_QLHDN.Name = "pb_QLHDN";
            pb_QLHDN.Size = new Size(38, 38);
            pb_QLHDN.SizeMode = PictureBoxSizeMode.StretchImage;
            pb_QLHDN.TabIndex = 2;
            pb_QLHDN.TabStop = false;
            pb_QLHDN.Click += pictureBox1_Click;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Font = new Font("Segoe UI", 12F);
            label8.ForeColor = SystemColors.ControlText;
            label8.Location = new Point(51, 8);
            label8.Name = "label8";
            label8.Size = new Size(161, 21);
            label8.TabIndex = 0;
            label8.Text = "Quản lí hóa đơn nhập";
            // 
            // btn_QLMA
            // 
            btn_QLMA.BackColor = Color.Bisque;
            btn_QLMA.Controls.Add(pb_QLMA);
            btn_QLMA.Controls.Add(label7);
            btn_QLMA.Location = new Point(18, 222);
            btn_QLMA.Name = "btn_QLMA";
            btn_QLMA.Size = new Size(224, 40);
            btn_QLMA.TabIndex = 1;
            // 
            // pb_QLMA
            // 
            pb_QLMA.Image = Properties.Resources.monan;
            pb_QLMA.Location = new Point(10, 0);
            pb_QLMA.Name = "pb_QLMA";
            pb_QLMA.Size = new Size(38, 38);
            pb_QLMA.SizeMode = PictureBoxSizeMode.StretchImage;
            pb_QLMA.TabIndex = 2;
            pb_QLMA.TabStop = false;
            pb_QLMA.Click += pictureBox1_Click;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Font = new Font("Segoe UI", 12F);
            label7.ForeColor = SystemColors.ControlText;
            label7.Location = new Point(51, 8);
            label7.Name = "label7";
            label7.Size = new Size(117, 21);
            label7.TabIndex = 0;
            label7.Text = "Quản lí món ăn";
            // 
            // btn_QLKH
            // 
            btn_QLKH.BackColor = Color.Bisque;
            btn_QLKH.Controls.Add(pb_QLKH);
            btn_QLKH.Controls.Add(label6);
            btn_QLKH.Location = new Point(18, 167);
            btn_QLKH.Name = "btn_QLKH";
            btn_QLKH.Size = new Size(224, 40);
            btn_QLKH.TabIndex = 1;
            // 
            // pb_QLKH
            // 
            pb_QLKH.Image = Properties.Resources.khachhang;
            pb_QLKH.Location = new Point(10, 1);
            pb_QLKH.Name = "pb_QLKH";
            pb_QLKH.Size = new Size(38, 38);
            pb_QLKH.SizeMode = PictureBoxSizeMode.StretchImage;
            pb_QLKH.TabIndex = 2;
            pb_QLKH.TabStop = false;
            pb_QLKH.Click += pictureBox1_Click;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("Segoe UI", 12F);
            label6.ForeColor = SystemColors.ControlText;
            label6.Location = new Point(51, 8);
            label6.Name = "label6";
            label6.Size = new Size(144, 21);
            label6.TabIndex = 0;
            label6.Text = "Quản lí khách hàng";
            // 
            // btn_QLNCC
            // 
            btn_QLNCC.BackColor = Color.Bisque;
            btn_QLNCC.Controls.Add(pb_QLNCC);
            btn_QLNCC.Controls.Add(label5);
            btn_QLNCC.Location = new Point(18, 112);
            btn_QLNCC.Name = "btn_QLNCC";
            btn_QLNCC.Size = new Size(224, 40);
            btn_QLNCC.TabIndex = 1;
            // 
            // pb_QLNCC
            // 
            pb_QLNCC.Image = Properties.Resources.ncc;
            pb_QLNCC.Location = new Point(10, 1);
            pb_QLNCC.Name = "pb_QLNCC";
            pb_QLNCC.Size = new Size(38, 38);
            pb_QLNCC.SizeMode = PictureBoxSizeMode.StretchImage;
            pb_QLNCC.TabIndex = 2;
            pb_QLNCC.TabStop = false;
            pb_QLNCC.Click += pictureBox1_Click;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Segoe UI", 12F);
            label5.ForeColor = SystemColors.ControlText;
            label5.Location = new Point(51, 8);
            label5.Name = "label5";
            label5.Size = new Size(156, 21);
            label5.TabIndex = 0;
            label5.Text = "Quản lí nhà cung cấp";
            // 
            // btn_QLNV
            // 
            btn_QLNV.BackColor = Color.Bisque;
            btn_QLNV.Controls.Add(pb_QLNV);
            btn_QLNV.Controls.Add(label4);
            btn_QLNV.Location = new Point(18, 57);
            btn_QLNV.Name = "btn_QLNV";
            btn_QLNV.Size = new Size(224, 40);
            btn_QLNV.TabIndex = 1;
            // 
            // pb_QLNV
            // 
            pb_QLNV.Image = Properties.Resources.nhanvien;
            pb_QLNV.Location = new Point(10, 1);
            pb_QLNV.Name = "pb_QLNV";
            pb_QLNV.Size = new Size(38, 38);
            pb_QLNV.SizeMode = PictureBoxSizeMode.StretchImage;
            pb_QLNV.TabIndex = 2;
            pb_QLNV.TabStop = false;
            pb_QLNV.Click += pictureBox1_Click;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI", 12F);
            label4.ForeColor = SystemColors.ControlText;
            label4.Location = new Point(51, 8);
            label4.Name = "label4";
            label4.Size = new Size(132, 21);
            label4.TabIndex = 0;
            label4.Text = "Quản lí nhân viên";
            // 
            // lb_DMQL
            // 
            lb_DMQL.AutoSize = true;
            lb_DMQL.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            lb_DMQL.ForeColor = Color.Salmon;
            lb_DMQL.Location = new Point(35, 9);
            lb_DMQL.Name = "lb_DMQL";
            lb_DMQL.Size = new Size(182, 28);
            lb_DMQL.TabIndex = 0;
            lb_DMQL.Text = "Danh mục Quản lí";
            // 
            // TrangChuADMIN
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = Properties.Resources.mt;
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(1101, 628);
            Controls.Add(roundedPanel1);
            DoubleBuffered = true;
            Name = "TrangChuADMIN";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "TrangChuADMIN";
            roundedPanel1.ResumeLayout(false);
            roundedPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pb_Admin).EndInit();
            btn_DangXuat.ResumeLayout(false);
            btn_DangXuat.PerformLayout();
            hcnt_KhungMenuAD.ResumeLayout(false);
            hcnt_KhungMenuAD.PerformLayout();
            btn_ThongKe.ResumeLayout(false);
            btn_ThongKe.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pb_ThongKe).EndInit();
            btn_QLHDB.ResumeLayout(false);
            btn_QLHDB.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pb_QLHDB).EndInit();
            btn_QLHDN.ResumeLayout(false);
            btn_QLHDN.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pb_QLHDN).EndInit();
            btn_QLMA.ResumeLayout(false);
            btn_QLMA.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pb_QLMA).EndInit();
            btn_QLKH.ResumeLayout(false);
            btn_QLKH.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pb_QLKH).EndInit();
            btn_QLNCC.ResumeLayout(false);
            btn_QLNCC.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pb_QLNCC).EndInit();
            btn_QLNV.ResumeLayout(false);
            btn_QLNV.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pb_QLNV).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private RoundedPanel roundedPanel1;
        private RoundedPanel hcnt_KhungMenuAD;
        private Label lb_Admin;
        private RoundedPanel hcnt_Khung;
        private Label lb_DMQL;
        private RoundedPanel btn_QLNV;
        private RoundedPanel btn_QLNCC;
        private Label label5;
        private Label label4;
        private RoundedPanel btn_ThongKe;
        private Label label10;
        private RoundedPanel btn_QLHDB;
        private Label hcnt_QLHDB;
        private RoundedPanel btn_QLHDN;
        private Label label8;
        private RoundedPanel btn_QLMA;
        private Label label7;
        private RoundedPanel btn_QLKH;
        private Label label6;
        private PictureBox pb_Admin;
        private PictureBox pb_QLNV;
        private PictureBox pb_QLKH;
        private PictureBox pb_QLNCC;
        private PictureBox pb_QLMA;
        private PictureBox pb_QLHDN;
        private PictureBox pb_QLHDB;
        private PictureBox pb_ThongKe;
        private RoundedPanel btn_DangXuat;
        private Label lb_DangXuat;
    }
}