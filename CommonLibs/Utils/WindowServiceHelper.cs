using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibs.Utils
{
    /// <summary>
    /// Định nghĩa các trạng thái của Service một cách tường minh
    /// </summary>
    public enum WindowsServiceStatus
    {
        NotInstalled, // Chưa cài đặt trên máy
        Stopped,      // Đang dừng
        Running,      // Đang chạy
        Pending,      // Đang trong quá trình chuyển đổi (Đang bật/Đang tắt)
        Unknown       // Không xác định (Lỗi hệ thống hoặc hệ điều hành không hỗ trợ)
    }

    public static class WindowsServiceHelper
    {
        /// <summary>
        /// Kiểm tra nền tảng hệ điều hành hiện tại có phải Windows hay không
        /// </summary>
        public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        /// <summary>
        /// Lấy trạng thái hiện tại của một Windows Service
        /// </summary>
        public static WindowsServiceStatus GetStatus(string serviceName)
        {
            if (!IsWindows) return WindowsServiceStatus.Unknown;

            try
            {
                using (var sc = new ServiceController(serviceName))
                {
                    sc.Refresh(); // Cập nhật trạng thái mới nhất từ OS

                    return sc.Status switch
                    {
                        ServiceControllerStatus.Running => WindowsServiceStatus.Running,
                        ServiceControllerStatus.Stopped => WindowsServiceStatus.Stopped,
                        ServiceControllerStatus.StartPending or
                        ServiceControllerStatus.StopPending or
                        ServiceControllerStatus.ContinuePending or
                        ServiceControllerStatus.PausePending => WindowsServiceStatus.Pending,
                        _ => WindowsServiceStatus.Unknown,
                    };
                }
            }
            catch (InvalidOperationException)
            {
                // Ngoại lệ này ném ra khi không tìm thấy Service Name tương ứng trong hệ thống
                return WindowsServiceStatus.NotInstalled;
            }
            catch
            {
                return WindowsServiceStatus.Unknown;
            }
        }

        /// <summary>
        /// Khởi chạy (Start) một Windows Service bất đồng bộ kèm theo Timeout
        /// </summary>
        /// <returns>Trả về true nếu start thành công hoặc service đã chạy sẵn</returns>
        public static async Task<bool> StartServiceAsync(string serviceName, int timeoutSeconds = 30, CancellationToken cancellationToken = default)
        {
            if (!IsWindows) return false;

            try
            {
                using (var sc = new ServiceController(serviceName))
                {
                    sc.Refresh();
                    if (sc.Status == ServiceControllerStatus.Running) return true;

                    sc.Start();

                    var timeout = TimeSpan.FromSeconds(timeoutSeconds);
                    // Sử dụng Task.Run để giải phóng Thread Pool khi đợi Service chuyển trạng thái
                    await Task.Run(() => sc.WaitForStatus(ServiceControllerStatus.Running, timeout), cancellationToken);

                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Dừng (Stop) một Windows Service bất đồng bộ kèm theo Timeout
        /// </summary>
        /// <returns>Trả về true nếu dừng thành công hoặc service đã dừng sẵn</returns>
        public static async Task<bool> StopServiceAsync(string serviceName, int timeoutSeconds = 30, CancellationToken cancellationToken = default)
        {
            if (!IsWindows) return false;

            try
            {
                using (var sc = new ServiceController(serviceName))
                {
                    sc.Refresh();
                    if (sc.Status == ServiceControllerStatus.Stopped) return true;

                    if (!sc.CanStop) return false; // Service hệ thống hoặc cấu hình không cho phép dừng

                    sc.Stop();

                    var timeout = TimeSpan.FromSeconds(timeoutSeconds);
                    await Task.Run(() => sc.WaitForStatus(ServiceControllerStatus.Stopped, timeout), cancellationToken);

                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Khởi động lại (Restart) một Windows Service
        /// </summary>
        public static async Task<bool> RestartServiceAsync(string serviceName, int timeoutSeconds = 30, CancellationToken cancellationToken = default)
        {
            // Thực hiện Stop trước, sau đó Start lại
            bool isStopped = await StopServiceAsync(serviceName, timeoutSeconds, cancellationToken);
            if (!isStopped) return false;

            return await StartServiceAsync(serviceName, timeoutSeconds, cancellationToken);
        }
    }
}
