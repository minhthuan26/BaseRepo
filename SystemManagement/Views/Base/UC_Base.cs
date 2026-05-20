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
using SystemManagement.Views.ConfigEnv;
using SystemManagement.Views.Home;

namespace SystemManagement.Views.Base
{
    public partial class UC_Base : UserControl
    {
        private readonly Stack<UserControl> _internalNavigationStack = new();
        private UserControl _currentInternalPage;
        public UC_Base()
        {
            InitializeComponent();
        }

        public Panel PageContainer
        {
            get { return pnl_PageMain; }
        }

        public FrmMain MainForm
        {
            get
            {
                return (FrmMain)this.FindForm();
            }
        }

        public void InternalNavigate(UserControl page)
        {
            if (_currentInternalPage != null)
            {
                _internalNavigationStack.Push(_currentInternalPage);
            }

            if (page is IBasePage basePage)
            {
                basePage.BaseUC = this;
                lbl_Title.Text = basePage.Title;
            }

            pnl_PageMain.Controls.Clear();

            page.Dock = DockStyle.Fill;

            pnl_PageMain.Controls.Add(page);

            _currentInternalPage = page;

            UpdateInternalBackButton();
        }

        private void UpdateInternalBackButton()
        {
            picBox_Back.Visible =
                _internalNavigationStack.Count > 0
                || (MainForm?.GetNavigationStack().Count > 1);
        }

        public void InternalBack()
        {
            if (_internalNavigationStack.Count <= 0)
                return;

            pnl_PageMain.Controls.Clear();

            var previousPage = _internalNavigationStack.Pop();

            previousPage.Dock = DockStyle.Fill;

            pnl_PageMain.Controls.Add(previousPage);

            _currentInternalPage = previousPage;

            if (previousPage is IBasePage basePage)
            {
                lbl_Title.Text = basePage.Title;
            }
            UpdateInternalBackButton();
        }

        public void FormNavigate(UserControl page)
        {
            MainForm?.LoadPage(page);
        }

        public void FormBack()
        {
            MainForm?.GoBack();
        }

        public void SetTitle(string title)
        {
            lbl_Title.Text = title;
        }

        public void ShowMessage(string message, string caption = "Thông báo")
        {
            MessageBox.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }


        private void picBox_Back_Click(object sender, EventArgs e)
        {
            if (_internalNavigationStack.Count > 0)
            {
                InternalBack();
            }
            else
            {
                FormBack();
            }
        }

        private void UC_Base_Load(object sender, EventArgs e)
        {
            if (MainForm != null)
            {
                picBox_Back.Visible = MainForm.GetNavigationStack().Count > 1;
            }

            if (pnl_PageMain.Controls.Count == 0)
            {
                InternalNavigate(new Home.UC_Home());
            }
        }
    }
}
