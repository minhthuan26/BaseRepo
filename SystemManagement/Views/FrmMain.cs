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
    /// <summary>
    /// Cửa sổ chính của SystemManagement.
    /// Giải thích: chứa một trang (UserControl) trong <c>pnl_Main</c> và quản lý điều hướng giữa các trang
    /// (ngăn xếp lịch sử), đồng hồ ở thanh trạng thái, nạp <c>system.config</c> khi mở, và cung cấp các hàm
    /// dùng chung cho toàn ứng dụng: loader (<see cref="RunWithLoadingAsync"/>, <see cref="ShowLoading"/>...)
    /// và hộp thông báo (<see cref="ThrowInfo"/>, <see cref="ThrowWarning"/>, <see cref="ThrowError"/>,
    /// <see cref="ThrowQuestion"/>). Kế thừa <see cref="Frm_Base"/> như mọi form khác của ứng dụng.
    /// </summary>
    public partial class FrmMain : Frm_Base
    {
        #region Public props

        /// <summary>
        /// Thể hiện duy nhất của cửa sổ chính, đặt trong constructor; các hàm static (loader, hộp thông báo) dựa vào nó.
        /// </summary>
        public static FrmMain Instance { get; private set; }

        /// <summary>
        /// Cấu hình đọc từ <c>Config/system.config</c> (danh sách khóa env, kết nối, site) khi form mở (<see cref="FrmHome_Load"/>).
        /// </summary>
        public static SystemConfig SysConfig { get; set; }

        /// <summary>
        /// Chiều rộng hiện tại của cửa sổ chính (0 nếu chưa tạo).
        /// </summary>
        public static int FrmWidth => Instance?.Width ?? 0;

        /// <summary>
        /// Chiều cao hiện tại của cửa sổ chính (0 nếu chưa tạo).
        /// </summary>
        public static int FrmHeight => Instance?.Height ?? 0;

        #endregion
        #region Private props

        /// <summary>
        /// Bộ đếm 1 giây để cập nhật đồng hồ ở thanh trạng thái; dừng và hủy khi đóng form.
        /// </summary>
        private System.Windows.Forms.Timer _clockTimer;

        /// <summary>
        /// Ngăn xếp các trang đã mở, dùng cho nút quay lại (<see cref="GoBack"/>).
        /// </summary>
        private Stack<UserControl> navigationStack = new Stack<UserControl>();

        #endregion

        /// <summary>
        /// Dựng giao diện và ghi nhận thể hiện duy nhất <see cref="Instance"/>.
        /// </summary>
        public FrmMain()
        {
            InitializeComponent();
            Instance = this;
        }

        #region Public functions

        /// <summary>
        /// Xóa toàn bộ lịch sử điều hướng (không đổi trang đang hiển thị).
        /// </summary>
        public void ClearNavigation()
        {
            navigationStack.Clear();
        }

        /// <summary>
        /// Hiển thị một trang thay cho trang hiện tại mà không lưu trang cũ vào lịch sử (không quay lại được).
        /// </summary>
        public void LoadPageWithoutHistory(UserControl page)
        {
            pnl_Main.Controls.Clear();
            page.Dock = DockStyle.Fill;
            pnl_Main.Controls.Add(page);
        }

        /// <summary>
        /// Trả về ngăn xếp lịch sử điều hướng hiện tại (trả tham chiếu gốc, không phải bản sao).
        /// </summary>
        public Stack<UserControl> GetNavigationStack()
        {
            return navigationStack;
        }

        /// <summary>
        /// Hiển thị một trang mới và đẩy trang đang hiển thị vào lịch sử để <see cref="GoBack"/> quay lại được.
        /// </summary>
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

        /// <summary>
        /// Quay lại trang gần nhất trong lịch sử; không làm gì nếu lịch sử rỗng.
        /// </summary>
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

        /// <summary>
        /// Hiện loader, chạy <paramref name="action"/>, rồi luôn ẩn loader (kể cả khi action ném lỗi).
        /// Giải thích: <c>Task.Yield()</c> nhường luồng một lần để loader kịp vẽ, nhưng <paramref name="action"/>
        /// vẫn chạy <b>trên luồng UI</b>. Việc nặng (ghi registry, IO) phải được nơi gọi tự bọc <c>Task.Run</c>,
        /// còn action chạm control UI thì để nguyên; vì vậy hàm này không tự đẩy action sang luồng nền.
        /// </summary>
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

        /// <summary>
        /// Ẩn loader rồi hiện hộp thông báo thông tin (OK), thuộc cửa sổ chính.
        /// </summary>
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

        /// <summary>
        /// Ẩn loader rồi hiện hộp cảnh báo (OK), thuộc cửa sổ chính.
        /// </summary>
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

        /// <summary>
        /// Ẩn loader rồi hiện hộp báo lỗi (OK), thuộc cửa sổ chính.
        /// </summary>
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

        /// <summary>
        /// Ẩn loader rồi hỏi Có/Không; trả về true khi người dùng chọn Có.
        /// </summary>
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

        /// <summary>
        /// Hiện thanh tiến trình dạng chạy liên tục (marquee) của cửa sổ chính; an toàn khi gọi từ luồng khác.
        /// </summary>
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

        /// <summary>
        /// Ẩn thanh tiến trình, đưa về trạng thái ban đầu và vẽ lại cửa sổ; an toàn khi gọi từ luồng khác.
        /// </summary>
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

        /// <summary>
        /// Hiện thanh tiến trình dạng phần trăm (giới hạn 0-100); tự ẩn khi đạt 100. An toàn khi gọi từ luồng khác.
        /// </summary>
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

        /// <summary>
        /// Đọc <c>Config/system.config</c> vào <see cref="SysConfig"/>. Đọc không được thì dùng cấu hình rỗng
        /// (các danh sách rỗng) và báo lỗi, để các màn hình khác không gặp <c>null</c>.
        /// </summary>
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

        /// <summary>
        /// Khi form mở: nạp cấu hình dưới loader, hiện số phiên bản và giờ, bật đồng hồ, và hiển thị trang chủ
        /// (<see cref="UC_Base"/>) nếu chưa có trang nào.
        /// </summary>
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

        /// <summary>
        /// Mỗi giây cập nhật đồng hồ ở thanh trạng thái.
        /// </summary>
        private void ClockTimer_Tick(object sender, EventArgs e)
        {
            // This runs automatically every 1 second
            UpdateTimeLabel();
        }

        /// <summary>
        /// Ghi giờ hiện tại (định dạng HH:mm:ss dd/MM/yyyy) lên thanh trạng thái.
        /// </summary>
        private void UpdateTimeLabel()
        {
            tstrip_LblTime.Text = DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy");
        }

        /// <summary>
        /// Khi đóng form: dừng và hủy bộ đếm đồng hồ.
        /// </summary>
        private void FrmHome_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_clockTimer != null)
            {
                _clockTimer.Stop();
                _clockTimer.Dispose();
            }
        }

        /// <summary>
        /// Menu Home: xóa lịch sử và trang hiện tại, hiển thị lại trang chủ (<see cref="UC_Base"/>) làm trang gốc.
        /// </summary>
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

        /// <summary>
        /// Menu About: chưa triển khai (handler rỗng).
        /// </summary>
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
        #endregion

        #region Override functions

        /// <summary>Mã thông điệp hệ thống của cửa sổ (lệnh phóng to/thu nhỏ/khôi phục...).</summary>
        const int WM_SYSCOMMAND = 0x0112;

        /// <summary>Lệnh phóng to cửa sổ (trong <c>WM_SYSCOMMAND</c>).</summary>
        const int SC_MAXIMIZE = 0xF030;

        /// <summary>Lệnh khôi phục kích thước cửa sổ (trong <c>WM_SYSCOMMAND</c>).</summary>
        const int SC_RESTORE = 0xF120;

        /// <summary>
        /// Khi người dùng phóng to hoặc khôi phục cửa sổ thì phát <c>OnResizeEnd</c>.
        /// Giải thích: <c>ResizeEnd</c> mặc định chỉ phát sau khi kéo đổi kích thước, không phát khi phóng to/khôi phục,
        /// nên phải tự phát để các thành phần lắng nghe sự kiện này co giãn lại.
        /// </summary>
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
