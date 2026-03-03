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
    }
}
