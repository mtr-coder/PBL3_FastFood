using PBL3.UI;

namespace PBL3
{
    partial class KhachHang
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            roundedPanel1 = new RoundedPanel();
            pb_Admin = new PictureBox();
            btn_DangXuat = new RoundedPanel();
            lb_DangXuat = new Label();
            lb_Admin = new Label();
            hcnt_Khung = new RoundedPanel();
            pnlTimKiem = new RoundedPanel();
            _txtTimKiem = new TextBox();
            lblCongThuc = new Label();
            btnLamMoi = new Button();
            btnLichSuDiem = new Button();
            lblTitle = new Label();
            dgvKhachHang = new DataGridView();
            hcnt_KhungMenuAD = new RoundedPanel();
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
            hcnt_Khung.SuspendLayout();
            pnlTimKiem.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvKhachHang).BeginInit();
            hcnt_KhungMenuAD.SuspendLayout();
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
            roundedPanel1.Margin = new Padding(3, 2, 3, 2);
            roundedPanel1.Name = "roundedPanel1";
            roundedPanel1.Size = new Size(1078, 608);
            roundedPanel1.TabIndex = 0;
            // 
            // pb_Admin
            // 
            pb_Admin.BackColor = SystemColors.Control;
            pb_Admin.Image = Properties.Resources.nhanvien;
            pb_Admin.Location = new Point(17, 8);
            pb_Admin.Margin = new Padding(3, 2, 3, 2);
            pb_Admin.Name = "pb_Admin";
            pb_Admin.Size = new Size(45, 38);
            pb_Admin.SizeMode = PictureBoxSizeMode.StretchImage;
            pb_Admin.TabIndex = 2;
            pb_Admin.TabStop = false;
            // 
            // btn_DangXuat
            // 
            btn_DangXuat.BackColor = Color.SandyBrown;
            btn_DangXuat.Controls.Add(lb_DangXuat);
            btn_DangXuat.Location = new Point(170, 12);
            btn_DangXuat.Margin = new Padding(3, 2, 3, 2);
            btn_DangXuat.Name = "btn_DangXuat";
            btn_DangXuat.Size = new Size(110, 25);
            btn_DangXuat.TabIndex = 3;
            btn_DangXuat.Click += btn_DangXuat_Click;
            // 
            // lb_DangXuat
            // 
            lb_DangXuat.AutoSize = true;
            lb_DangXuat.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lb_DangXuat.ForeColor = Color.White;
            lb_DangXuat.Location = new Point(16, 3);
            lb_DangXuat.Name = "lb_DangXuat";
            lb_DangXuat.Size = new Size(77, 19);
            lb_DangXuat.TabIndex = 0;
            lb_DangXuat.Text = "Đăng xuất";
            lb_DangXuat.Click += btn_DangXuat_Click;
            // 
            // lb_Admin
            // 
            lb_Admin.AutoSize = true;
            lb_Admin.Font = new Font("Segoe UI", 12F);
            lb_Admin.Location = new Point(58, 12);
            lb_Admin.Name = "lb_Admin";
            lb_Admin.Size = new Size(81, 21);
            lb_Admin.TabIndex = 2;
            lb_Admin.Text = "Nhân viên";
            // 
            // hcnt_Khung
            // 
            hcnt_Khung.BackColor = Color.Linen;
            hcnt_Khung.Controls.Add(pnlTimKiem);
            hcnt_Khung.Controls.Add(lblCongThuc);
            hcnt_Khung.Controls.Add(btnLamMoi);
            hcnt_Khung.Controls.Add(btnLichSuDiem);
            hcnt_Khung.Controls.Add(lblTitle);
            hcnt_Khung.Controls.Add(dgvKhachHang);
            hcnt_Khung.Location = new Point(299, 49);
            hcnt_Khung.Margin = new Padding(3, 2, 3, 2);
            hcnt_Khung.Name = "hcnt_Khung";
            hcnt_Khung.Size = new Size(761, 538);
            hcnt_Khung.TabIndex = 1;
            // 
            // pnlTimKiem
            // 
            pnlTimKiem.BackColor = Color.Bisque;
            pnlTimKiem.Controls.Add(_txtTimKiem);
            pnlTimKiem.CornerRadius = 12;
            pnlTimKiem.Location = new Point(21, 46);
            pnlTimKiem.Margin = new Padding(3, 2, 3, 2);
            pnlTimKiem.Name = "pnlTimKiem";
            pnlTimKiem.Size = new Size(247, 25);
            pnlTimKiem.TabIndex = 17;
            // 
            // _txtTimKiem
            // 
            _txtTimKiem.BackColor = Color.Bisque;
            _txtTimKiem.BorderStyle = BorderStyle.None;
            _txtTimKiem.Location = new Point(10, 4);
            _txtTimKiem.Margin = new Padding(3, 2, 3, 2);
            _txtTimKiem.Name = "_txtTimKiem";
            _txtTimKiem.PlaceholderText = "Tìm kiếm";
            _txtTimKiem.Size = new Size(225, 16);
            _txtTimKiem.TabIndex = 0;
            // 
            // lblCongThuc
            // 
            lblCongThuc.AutoSize = true;
            lblCongThuc.Font = new Font("Segoe UI", 9F);
            lblCongThuc.ForeColor = Color.Firebrick;
            lblCongThuc.Location = new Point(21, 82);
            lblCongThuc.Name = "lblCongThuc";
            lblCongThuc.Size = new Size(622, 15);
            lblCongThuc.TabIndex = 7;
            lblCongThuc.Text = "Node: 1 điểm = 1.000đ giảm giá | +10đ mỗi 100.000đ thanh toán | Hạng giảm giá: Bạc: 0%, Vàng 5%, Kim cương: 10%";
            // 
            // btnLamMoi
            // 
            btnLamMoi.BackColor = Color.Peru;
            btnLamMoi.FlatStyle = FlatStyle.Flat;
            btnLamMoi.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnLamMoi.ForeColor = Color.White;
            btnLamMoi.Location = new Point(601, 46);
            btnLamMoi.Margin = new Padding(3, 2, 3, 2);
            btnLamMoi.Name = "btnLamMoi";
            btnLamMoi.Size = new Size(139, 23);
            btnLamMoi.TabIndex = 8;
            btnLamMoi.Text = "Làm mới";
            btnLamMoi.UseVisualStyleBackColor = false;
            btnLamMoi.Click += btnLamMoi_Click;
            // 
            // btnLichSuDiem
            // 
            btnLichSuDiem.BackColor = Color.SandyBrown;
            btnLichSuDiem.FlatStyle = FlatStyle.Flat;
            btnLichSuDiem.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnLichSuDiem.ForeColor = Color.White;
            btnLichSuDiem.Location = new Point(446, 46);
            btnLichSuDiem.Margin = new Padding(3, 2, 3, 2);
            btnLichSuDiem.Name = "btnLichSuDiem";
            btnLichSuDiem.Size = new Size(139, 23);
            btnLichSuDiem.TabIndex = 9;
            btnLichSuDiem.Text = "Lịch sử điểm";
            btnLichSuDiem.UseVisualStyleBackColor = false;
            btnLichSuDiem.Click += btnLichSuDiem_Click;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblTitle.ForeColor = Color.SaddleBrown;
            lblTitle.Location = new Point(347, 9);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(100, 21);
            lblTitle.TabIndex = 6;
            lblTitle.Text = "Khách hàng";
            // 
            // dgvKhachHang
            // 
            dgvKhachHang.AllowUserToAddRows = false;
            dgvKhachHang.AllowUserToDeleteRows = false;
            dgvKhachHang.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvKhachHang.BackgroundColor = Color.WhiteSmoke;
            dgvKhachHang.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvKhachHang.Location = new Point(21, 99);
            dgvKhachHang.Margin = new Padding(3, 2, 3, 2);
            dgvKhachHang.MultiSelect = false;
            dgvKhachHang.Name = "dgvKhachHang";
            dgvKhachHang.ReadOnly = true;
            dgvKhachHang.RowHeadersVisible = false;
            dgvKhachHang.RowHeadersWidth = 51;
            dgvKhachHang.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvKhachHang.Size = new Size(719, 415);
            dgvKhachHang.TabIndex = 0;
            // 
            // hcnt_KhungMenuAD
            // 
            hcnt_KhungMenuAD.BackColor = Color.Linen;
            hcnt_KhungMenuAD.Controls.Add(btn_QLHDN);
            hcnt_KhungMenuAD.Controls.Add(btn_QLMA);
            hcnt_KhungMenuAD.Controls.Add(btn_QLKH);
            hcnt_KhungMenuAD.Controls.Add(btn_QLNCC);
            hcnt_KhungMenuAD.Controls.Add(btn_QLNV);
            hcnt_KhungMenuAD.Controls.Add(lb_DMQL);
            hcnt_KhungMenuAD.Location = new Point(17, 49);
            hcnt_KhungMenuAD.Margin = new Padding(3, 2, 3, 2);
            hcnt_KhungMenuAD.Name = "hcnt_KhungMenuAD";
            hcnt_KhungMenuAD.Size = new Size(263, 538);
            hcnt_KhungMenuAD.TabIndex = 0;
            // 
            // btn_QLHDN
            // 
            btn_QLHDN.BackColor = Color.Salmon;
            btn_QLHDN.Controls.Add(pb_QLHDN);
            btn_QLHDN.Controls.Add(label8);
            btn_QLHDN.Location = new Point(18, 278);
            btn_QLHDN.Margin = new Padding(3, 2, 3, 2);
            btn_QLHDN.Name = "btn_QLHDN";
            btn_QLHDN.Size = new Size(224, 40);
            btn_QLHDN.TabIndex = 1;
            // 
            // pb_QLHDN
            // 
            pb_QLHDN.Image = Properties.Resources.khachhang;
            pb_QLHDN.Location = new Point(9, 0);
            pb_QLHDN.Margin = new Padding(3, 2, 3, 2);
            pb_QLHDN.Name = "pb_QLHDN";
            pb_QLHDN.Size = new Size(45, 38);
            pb_QLHDN.SizeMode = PictureBoxSizeMode.StretchImage;
            pb_QLHDN.TabIndex = 2;
            pb_QLHDN.TabStop = false;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Font = new Font("Segoe UI", 12F);
            label8.ForeColor = Color.White;
            label8.Location = new Point(51, 8);
            label8.Name = "label8";
            label8.Size = new Size(91, 21);
            label8.TabIndex = 0;
            label8.Text = "Khách hàng";
            // 
            // btn_QLMA
            // 
            btn_QLMA.BackColor = Color.Bisque;
            btn_QLMA.Controls.Add(pb_QLMA);
            btn_QLMA.Controls.Add(label7);
            btn_QLMA.Location = new Point(18, 223);
            btn_QLMA.Margin = new Padding(3, 2, 3, 2);
            btn_QLMA.Name = "btn_QLMA";
            btn_QLMA.Size = new Size(224, 40);
            btn_QLMA.TabIndex = 1;
            btn_QLMA.Click += btn_QLMA_Click;
            // 
            // pb_QLMA
            // 
            pb_QLMA.Image = Properties.Resources.monan;
            pb_QLMA.Location = new Point(9, 0);
            pb_QLMA.Margin = new Padding(3, 2, 3, 2);
            pb_QLMA.Name = "pb_QLMA";
            pb_QLMA.Size = new Size(45, 38);
            pb_QLMA.SizeMode = PictureBoxSizeMode.StretchImage;
            pb_QLMA.TabIndex = 2;
            pb_QLMA.TabStop = false;
            pb_QLMA.Click += btn_QLMA_Click;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Font = new Font("Segoe UI", 12F);
            label7.Location = new Point(51, 8);
            label7.Name = "label7";
            label7.Size = new Size(80, 21);
            label7.TabIndex = 0;
            label7.Text = "Mua hàng";
            label7.Click += btn_QLMA_Click;
            // 
            // btn_QLKH
            // 
            btn_QLKH.BackColor = Color.Bisque;
            btn_QLKH.Controls.Add(pb_QLKH);
            btn_QLKH.Controls.Add(label6);
            btn_QLKH.Location = new Point(18, 167);
            btn_QLKH.Margin = new Padding(3, 2, 3, 2);
            btn_QLKH.Name = "btn_QLKH";
            btn_QLKH.Size = new Size(224, 40);
            btn_QLKH.TabIndex = 1;
            btn_QLKH.Click += btn_QLKH_Click;
            // 
            // pb_QLKH
            // 
            pb_QLKH.Image = Properties.Resources.khachhang;
            pb_QLKH.Location = new Point(9, 0);
            pb_QLKH.Margin = new Padding(3, 2, 3, 2);
            pb_QLKH.Name = "pb_QLKH";
            pb_QLKH.Size = new Size(45, 38);
            pb_QLKH.SizeMode = PictureBoxSizeMode.StretchImage;
            pb_QLKH.TabIndex = 2;
            pb_QLKH.TabStop = false;
            pb_QLKH.Click += btn_QLKH_Click;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("Segoe UI", 12F);
            label6.Location = new Point(51, 8);
            label6.Name = "label6";
            label6.Size = new Size(75, 21);
            label6.TabIndex = 0;
            label6.Text = "Bán hàng";
            label6.Click += btn_QLKH_Click;
            // 
            // btn_QLNCC
            // 
            btn_QLNCC.BackColor = Color.Bisque;
            btn_QLNCC.Controls.Add(pb_QLNCC);
            btn_QLNCC.Controls.Add(label5);
            btn_QLNCC.Location = new Point(18, 112);
            btn_QLNCC.Margin = new Padding(3, 2, 3, 2);
            btn_QLNCC.Name = "btn_QLNCC";
            btn_QLNCC.Size = new Size(224, 40);
            btn_QLNCC.TabIndex = 1;
            btn_QLNCC.Click += btn_QLNCC_Click;
            // 
            // pb_QLNCC
            // 
            pb_QLNCC.Image = Properties.Resources.ncc;
            pb_QLNCC.Location = new Point(9, 0);
            pb_QLNCC.Margin = new Padding(3, 2, 3, 2);
            pb_QLNCC.Name = "pb_QLNCC";
            pb_QLNCC.Size = new Size(45, 38);
            pb_QLNCC.SizeMode = PictureBoxSizeMode.StretchImage;
            pb_QLNCC.TabIndex = 2;
            pb_QLNCC.TabStop = false;
            pb_QLNCC.Click += btn_QLNCC_Click;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Segoe UI", 12F);
            label5.Location = new Point(51, 8);
            label5.Name = "label5";
            label5.Size = new Size(70, 21);
            label5.TabIndex = 0;
            label5.Text = "Hóa đơn";
            label5.Click += btn_QLNCC_Click;
            // 
            // btn_QLNV
            // 
            btn_QLNV.BackColor = Color.Bisque;
            btn_QLNV.Controls.Add(pb_QLNV);
            btn_QLNV.Controls.Add(label4);
            btn_QLNV.Location = new Point(18, 57);
            btn_QLNV.Margin = new Padding(3, 2, 3, 2);
            btn_QLNV.Name = "btn_QLNV";
            btn_QLNV.Size = new Size(224, 40);
            btn_QLNV.TabIndex = 1;
            btn_QLNV.Click += btn_QLNV_Click;
            // 
            // pb_QLNV
            // 
            pb_QLNV.Image = Properties.Resources.nhanvien;
            pb_QLNV.Location = new Point(9, 0);
            pb_QLNV.Margin = new Padding(3, 2, 3, 2);
            pb_QLNV.Name = "pb_QLNV";
            pb_QLNV.Size = new Size(45, 38);
            pb_QLNV.SizeMode = PictureBoxSizeMode.StretchImage;
            pb_QLNV.TabIndex = 2;
            pb_QLNV.TabStop = false;
            pb_QLNV.Click += btn_QLNV_Click;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI", 12F);
            label4.Location = new Point(51, 8);
            label4.Name = "label4";
            label4.Size = new Size(134, 21);
            label4.TabIndex = 0;
            label4.Text = "Thông tin cá nhân";
            label4.Click += btn_QLNV_Click;
            // 
            // lb_DMQL
            // 
            lb_DMQL.AutoSize = true;
            lb_DMQL.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            lb_DMQL.ForeColor = Color.Salmon;
            lb_DMQL.Location = new Point(35, 9);
            lb_DMQL.Name = "lb_DMQL";
            lb_DMQL.Size = new Size(212, 28);
            lb_DMQL.TabIndex = 0;
            lb_DMQL.Text = "Danh mục chức năng";
            // 
            // KhachHang
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = Properties.Resources.mt;
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(1101, 628);
            Controls.Add(roundedPanel1);
            DoubleBuffered = true;
            Margin = new Padding(3, 2, 3, 2);
            Name = "KhachHang";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Khách hàng";
            Load += KhachHang_Load;
            roundedPanel1.ResumeLayout(false);
            roundedPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pb_Admin).EndInit();
            btn_DangXuat.ResumeLayout(false);
            btn_DangXuat.PerformLayout();
            hcnt_Khung.ResumeLayout(false);
            hcnt_Khung.PerformLayout();
            pnlTimKiem.ResumeLayout(false);
            pnlTimKiem.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvKhachHang).EndInit();
            hcnt_KhungMenuAD.ResumeLayout(false);
            hcnt_KhungMenuAD.PerformLayout();
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
        private PictureBox pb_Admin;
        private RoundedPanel btn_DangXuat;
        private Label lb_DangXuat;
        private Label lb_Admin;
        private RoundedPanel hcnt_Khung;
        private RoundedPanel hcnt_KhungMenuAD;
        private RoundedPanel btn_QLHDN;
        private PictureBox pb_QLHDN;
        private Label label8;
        private RoundedPanel btn_QLMA;
        private PictureBox pb_QLMA;
        private Label label7;
        private RoundedPanel btn_QLKH;
        private PictureBox pb_QLKH;
        private Label label6;
        private RoundedPanel btn_QLNCC;
        private PictureBox pb_QLNCC;
        private Label label5;
        private RoundedPanel btn_QLNV;
        private PictureBox pb_QLNV;
        private Label label4;
        private Label lb_DMQL;
        private Button btnLichSuDiem;
        private DataGridView dgvKhachHang;
        private Label lblTitle;
        private Label lblCongThuc;
        private Button btnLamMoi;
        private RoundedPanel pnlTimKiem;
        private TextBox _txtTimKiem;
    }
}


