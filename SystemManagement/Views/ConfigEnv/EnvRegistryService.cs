using Microsoft.Win32;
using System;
using System.Runtime.InteropServices;

namespace SystemManagement.Views.ConfigEnv
{
    /// <summary>
    /// Lưu trữ giá trị thô và kiểu registry của biến môi trường hệ thống.
    /// </summary>
    public record EnvRegistryValue(string Value, RegistryValueKind Kind);

    /// <summary>
    /// Dịch vụ thao tác trực tiếp với Registry Machine cho biến môi trường hệ thống.
    /// Giải thích: Cần đọc/ghi trực tiếp qua Registry thay vì Environment.SetEnvironmentVariable
    /// để bảo toàn kiểu dữ liệu REG_EXPAND_SZ (chứa biến %...%) và đọc giá trị thô chưa bị mở rộng.
    /// </summary>
    public static class EnvRegistryService
    {
        private const string EnvironmentKeyPath = @"SYSTEM\CurrentControlSet\Control\Session Manager\Environment";

        // P/Invoke SendMessageTimeout để phát tín hiệu WM_SETTINGCHANGE cho toàn bộ hệ thống
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern nint SendMessageTimeout(
            nint hWnd,
            uint msg,
            nuint wParam,
            string lParam,
            uint fuFlags,
            uint uTimeout,
            out nuint lpdwResult);

        private static readonly nint HwndBroadcast = (nint)0xffff;
        private const uint WmSettingChange = 0x001A;
        private const uint SmtoAbortIfHung = 0x0002;

        /// <summary>
        /// Đọc giá trị thô (không mở rộng %...%) và kiểu dữ liệu của biến trong HKLM Environment.
        /// Trả về null nếu biến thực sự không tồn tại.
        /// </summary>
        public static EnvRegistryValue? ReadRaw(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return null;

            using var baseKey = Registry.LocalMachine.OpenSubKey(EnvironmentKeyPath, writable: false);
            if (baseKey == null)
                throw new InvalidOperationException("Không thể mở khóa registry Machine Environment để đọc.");

            // Dùng DoNotExpandEnvironmentNames để lấy chuỗi thô (ví dụ: %SystemRoot%\system32)
            object? rawObj = baseKey.GetValue(key, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
            if (rawObj == null)
                return null;

            RegistryValueKind kind = baseKey.GetValueKind(key);
            return new EnvRegistryValue(rawObj.ToString() ?? string.Empty, kind);
        }

        /// <summary>
        /// Ghi giá trị thô và kiểu dữ liệu vào HKLM Environment.
        /// </summary>
        public static void WriteRaw(string key, string value, RegistryValueKind kind)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Tên biến không được để trống.", nameof(key));

            using var baseKey = Registry.LocalMachine.OpenSubKey(EnvironmentKeyPath, writable: true);
            if (baseKey == null)
                throw new InvalidOperationException("Không thể mở khóa registry Machine Environment để ghi.");

            // Giữ nguyên kiểu RegistryValueKind (REG_EXPAND_SZ hoặc REG_SZ) để không làm mất tính năng mở rộng %...%
            baseKey.SetValue(key, value, kind);
        }

        /// <summary>
        /// Phát broadcast WM_SETTINGCHANGE ("Environment") để các tiến trình mới nhận được biến môi trường vừa đổi.
        /// </summary>
        public static void BroadcastSettingChange()
        {
            try
            {
                SendMessageTimeout(
                    HwndBroadcast,
                    WmSettingChange,
                    nuint.Zero,
                    "Environment",
                    SmtoAbortIfHung,
                    5000,
                    out _);
            }
            catch
            {
                // Phát thông báo thất bại không được phép làm dừng hay hủy luồng ghi registry chính
            }
        }
    }
}
