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
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public UC_Base BaseUC { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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

        private void label4_Click(object sender, EventArgs e)
        {

        }

        public void Responsive()
        {
            throw new NotImplementedException();
        }
    }
}
