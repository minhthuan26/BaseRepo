using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SystemManagement.Views.Base;

namespace SystemManagement.Views
{
    public partial class FrmMain : Form
    {
        private System.Windows.Forms.Timer _clockTimer;

        private Stack<UserControl> navigationStack = new Stack<UserControl>();
        public FrmMain()
        {
            InitializeComponent();
        }

        public void ClearNavigation()
        {
            navigationStack.Clear();
        }

        public void LoadPageWithoutHistory(UserControl page)
        {
            pnl_Main.Controls.Clear();
            page.Dock = DockStyle.Fill;
            pnl_Main.Controls.Add(page);
        }

        public Stack<UserControl> GetNavigationStack()
        {
            return navigationStack;
        }

        public void LoadPage(UserControl page)
        {
            // Save current page
            if (pnl_Main.Controls.Count > 0)
            {
                UserControl currentPage = (UserControl)pnl_Main.Controls[0];

                navigationStack.Push(currentPage);
            }

            pnl_Main.Controls.Clear();

            page.Dock = DockStyle.Fill;

            pnl_Main.Controls.Add(page);
        }

        public void GoBack()
        {
            if (navigationStack.Count > 0)
            {
                pnl_Main.Controls.Clear();

                UserControl previousPage =
                    navigationStack.Pop();

                previousPage.Dock = DockStyle.Fill;

                pnl_Main.Controls.Add(previousPage);
            }
        }

        private void FrmHome_Load(object sender, EventArgs e)
        {
            tstrip_LblVersion.Text = $"Version: {Application.ProductVersion}";
            UpdateTimeLabel();

            // 3. Initialize and configure the timer
            _clockTimer = new System.Windows.Forms.Timer();
            _clockTimer.Interval = 1000; // 1000 milliseconds = 1 second
            _clockTimer.Tick += ClockTimer_Tick; // Link it to the event below
            _clockTimer.Start(); // Turn it on!

            // Load default page if empty
            if (navigationStack.Count == 0 && pnl_Main.Controls.Count == 0)
            {
                UC_Base basePage = new UC_Base();

                basePage.Dock = DockStyle.Fill;

                pnl_Main.Controls.Add(basePage);

                // Save as root/default page
                navigationStack.Push(basePage);
            }
        }

        private void ClockTimer_Tick(object sender, EventArgs e)
        {
            // This runs automatically every 1 second
            UpdateTimeLabel();
        }

        private void UpdateTimeLabel()
        {
            tstrip_LblTime.Text = DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy");
        }

        private void FrmHome_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_clockTimer != null)
            {
                _clockTimer.Stop();
                _clockTimer.Dispose();
            }
        }
    }
}
