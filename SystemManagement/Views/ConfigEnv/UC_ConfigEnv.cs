using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SystemManagement.Helpers;
using SystemManagement.Interfaces;
using SystemManagement.Views.Base;

namespace SystemManagement.Views.ConfigEnv
{
    public partial class UC_ConfigEnv : UserControl, IBasePage
    {
        public UC_Base BaseUC { get; set; }
        public string Title { get; set; } = "Cấu hình biến môi trường";
        public UC_ConfigEnv()
        {
            InitializeComponent();
        }

        private void UC_ConfigEnv_Load(object sender, EventArgs e)
        {
            string accessKey = EnvironmentHelper.Get("USERNAME");
        }
    }
}
