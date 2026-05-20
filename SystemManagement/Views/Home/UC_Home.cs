using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SystemManagement.Interfaces;
using SystemManagement.Views.Base;
using SystemManagement.Views.ConfigEnv;

namespace SystemManagement.Views.Home
{
    public partial class UC_Home : UserControl, IBasePage
    {
        public UC_Base BaseUC { get; set; }
        public string Title { get; set; } = "Trang chủ";
        public UC_Home()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {
            BaseUC.InternalNavigate(new UC_ConfigEnv());
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            BaseUC.InternalNavigate(new UC_ConfigEnv());
        }
    }
}
