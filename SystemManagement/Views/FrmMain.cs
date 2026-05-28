using CommonLibs.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SystemManagement.Models;
using SystemManagement.Views.Base;

namespace SystemManagement.Views
{
    public partial class FrmMain : Form
    {
        #region Public props

        public static FrmMain Instance { get; private set; }
        public static SystemConfig SysConfig { get; set; }
        public static int FrmWidth => Instance?.Width ?? 0;
        public static int FrmHeight => Instance?.Height ?? 0;

        #endregion
        #region Private props

        private System.Windows.Forms.Timer _clockTimer;
        private Stack<UserControl> navigationStack = new Stack<UserControl>();

        #endregion
        public FrmMain()
        {
            InitializeComponent();
            Instance = this;
        }

        #region Public functions
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

                UserControl previousPage = navigationStack.Pop();

                previousPage.Dock = DockStyle.Fill;

                pnl_Main.Controls.Add(previousPage);
            }
        }

        public static async Task RunWithLoadingAsync(Func<Task> action)
        {
            ShowLoading();
            try
            {
                await Task.Yield();
                await action();
            }
            finally
            {
                HideLoading();
            }
        }

        #region Global Messages

        public static void ThrowInfo(string message, string title = "Thông báo")
        {
            HideLoading();

            MessageBox.Show(
                Instance,
                message,
                title,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        public static void ThrowWarning(string message, string title = "Cảnh báo")
        {
            HideLoading();

            MessageBox.Show(
                Instance,
                message,
                title,
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        public static void ThrowError(string message, string title = "Lỗi")
        {
            HideLoading();

            MessageBox.Show(
                Instance,
                message,
                title,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        public static bool ThrowQuestion(string message, string title = "Xác nhận")
        {
            HideLoading();

            return MessageBox.Show(
                Instance,
                message,
                title,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.Yes;
        }

        #endregion

        #region Global Loading

        public static void ShowLoading()
        {
            if (Instance.InvokeRequired)
            {
                Instance.Invoke(new Action(ShowLoading));
                return;
            }

            Instance.progressBar1.Visible = true;
            Instance.progressBar1.Style = ProgressBarStyle.Marquee;
            Instance.progressBar1.MarqueeAnimationSpeed = 20;
            Instance.progressBar1.BringToFront();
        }

        public static void HideLoading()
        {
            if (Instance.InvokeRequired)
            {
                Instance.Invoke(new Action(HideLoading));
                return;
            }

            Instance.progressBar1.Visible = false;
            Instance.progressBar1.Style = ProgressBarStyle.Blocks;
            Instance.progressBar1.Value = 0;
            Instance.Refresh();
        }

        public static void SetProgress(int value)
        {
            if (Instance.InvokeRequired)
            {
                Instance.Invoke(new Action<int>(SetProgress), value);
                return;
            }

            if (value < 0) value = 0;

            if (value > 100) value = 100;

            Instance.progressBar1.Visible = true;
            Instance.progressBar1.Style = ProgressBarStyle.Blocks;
            Instance.progressBar1.Value = value;

            if (value >= 100)
            {
                HideLoading();
            }
        }

        #endregion
        #endregion

        #region Private functions
        private async Task ReadSystemConfig()
        {
            try
            {
                string configPath = "Config/system.config";

                SystemConfig? config = await FileHelper.ReadXML<SystemConfig>(configPath);
                SysConfig = config ?? new SystemConfig
                {
                    EnvKeys = new List<string>(),
                    Connections = new List<ConnectionItem>(),
                    Sites = new List<SiteItem>()
                };
            }
            catch (Exception ex)
            {
                ThrowError($"Failed to read environment keys: {ex.Message}");
            }
        }

        private async void FrmHome_Load(object sender, EventArgs e)
        {
            await RunWithLoadingAsync(async () =>
            {
                await ReadSystemConfig();
            });
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            tstrip_LblVersion.Text = $"Version: {version}";
            UpdateTimeLabel();

            _clockTimer = new System.Windows.Forms.Timer();
            _clockTimer.Interval = 1000;
            _clockTimer.Tick += ClockTimer_Tick;
            _clockTimer.Start();

            bool hasPage = pnl_Main.Controls
            .OfType<UserControl>()
            .Any();

            if (navigationStack.Count == 0 && !hasPage)
            {
                UC_Base basePage = new UC_Base();
                basePage.Dock = DockStyle.Fill;
                pnl_Main.Controls.Add(basePage);
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

        private void homeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Clear navigation history
            navigationStack.Clear();

            // Clear current controls
            pnl_Main.Controls.Clear();

            // Create home page
            UC_Base homePage = new UC_Base();

            // Dock fill
            homePage.Dock = DockStyle.Fill;

            // Add to panel
            pnl_Main.Controls.Add(homePage);

            // Save as root page
            navigationStack.Push(homePage);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
        #endregion

        #region Override functions
        const int WM_SYSCOMMAND = 0x0112;
        const int SC_MAXIMIZE = 0xF030;
        const int SC_RESTORE = 0xF120;
        protected override void WndProc(ref Message m) {
            if (m.Msg == WM_SYSCOMMAND)
            {
                if (m.WParam == (IntPtr)SC_MAXIMIZE ||
                    m.WParam == (IntPtr)SC_RESTORE)
                {
                    OnResizeEnd(EventArgs.Empty);
                }
            }

            base.WndProc(ref m);
        }
        #endregion
    }
}
