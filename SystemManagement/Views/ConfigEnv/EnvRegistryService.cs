using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SystemManagement.Views.ConfigEnv
{
    /// <summary>
    /// Phạm vi của biến môi trường: Machine (toàn máy, HKLM) hoặc User (người dùng hiện tại, HKCU).
    /// </summary>
    public enum EnvScope
    {
        Machine,
        User
    }

    /// <summary>
    /// Lưu trữ giá trị thô và kiểu registry của biến môi trường hệ thống.
    /// </summary>
    public record EnvRegistryValue(string Value, RegistryValueKind Kind);

    /// <summary>
    /// Dịch vụ thao tác trực tiếp với Registry cho biến môi trường theo phạm vi Machine và User.
    /// Giải thích: Cần đọc/ghi trực tiếp qua Registry thay vì Environment.SetEnvironmentVariable
    /// để bảo toàn kiểu dữ liệu REG_EXPAND_SZ (chứa biến %...%) và đọc giá trị thô chưa bị mở rộng.
    /// </summary>
    public static class EnvRegistryService
    {
        public const string MachineEnvironmentKeyPath = @"SYSTEM\CurrentControlSet\Control\Session Manager\Environment";
        public const string UserEnvironmentKeyPath = @"Environment";

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
        /// Mở registry subkey tương ứng với phạm vi Machine (HKLM) hoặc User (HKCU).
        /// </summary>
        private static RegistryKey? OpenEnvironmentKey(EnvScope scope, bool writable)
        {
            return scope switch
            {
                EnvScope.Machine => Registry.LocalMachine.OpenSubKey(MachineEnvironmentKeyPath, writable),
                EnvScope.User => Registry.CurrentUser.OpenSubKey(UserEnvironmentKeyPath, writable),
                _ => throw new ArgumentOutOfRangeException(nameof(scope), scope, "Phạm vi biến môi trường không hợp lệ.")
            };
        }

        /// <summary>
        /// Đọc giá trị thô (không mở rộng %...%) và kiểu dữ liệu của biến theo phạm vi Machine hoặc User.
        /// Trả về null nếu biến thực sự không tồn tại.
        /// </summary>
        public static EnvRegistryValue? ReadRaw(EnvScope scope, string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return null;

            using var baseKey = OpenEnvironmentKey(scope, writable: false);
            if (baseKey == null)
                throw new InvalidOperationException($"Không thể mở khóa registry {scope} Environment để đọc.");

            // Dùng DoNotExpandEnvironmentNames để lấy chuỗi thô (ví dụ: %SystemRoot%\system32)
            object? rawObj = baseKey.GetValue(key, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
            if (rawObj == null)
                return null;

            RegistryValueKind kind = baseKey.GetValueKind(key);
            return new EnvRegistryValue(rawObj.ToString() ?? string.Empty, kind);
        }

        /// <summary>
        /// Đọc giá trị thô từ phạm vi Machine (HKLM Environment) - giữ nguyên chữ ký và hành vi N1/N2.
        /// </summary>
        public static EnvRegistryValue? ReadRaw(string key)
        {
            return ReadRaw(EnvScope.Machine, key);
        }

        /// <summary>
        /// Ghi giá trị thô và kiểu dữ liệu vào registry theo phạm vi Machine hoặc User.
        /// </summary>
        public static void WriteRaw(EnvScope scope, string key, string value, RegistryValueKind kind)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Tên biến không được để trống.", nameof(key));

            using var baseKey = OpenEnvironmentKey(scope, writable: true);
            if (baseKey == null)
                throw new InvalidOperationException($"Không thể mở khóa registry {scope} Environment để ghi.");

            // Giữ nguyên kiểu RegistryValueKind (REG_EXPAND_SZ hoặc REG_SZ) để không làm mất tính năng mở rộng %...%
            baseKey.SetValue(key, value, kind);
        }

        /// <summary>
        /// Ghi giá trị thô và kiểu dữ liệu vào HKLM Environment - giữ nguyên chữ ký và hành vi N1/N2.
        /// </summary>
        public static void WriteRaw(string key, string value, RegistryValueKind kind)
        {
            WriteRaw(EnvScope.Machine, key, value, kind);
        }

        /// <summary>
        /// Xóa biến môi trường trong registry theo phạm vi.
        /// Giải thích: Nếu giá trị không tồn tại thì không ném lỗi (throwOnMissingValue: false).
        /// </summary>
        public static void DeleteRaw(EnvScope scope, string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Tên biến không được để trống.", nameof(key));

            using var baseKey = OpenEnvironmentKey(scope, writable: true);
            if (baseKey == null)
                throw new InvalidOperationException($"Không thể mở khóa registry {scope} Environment để ghi.");

            baseKey.DeleteValue(key, throwOnMissingValue: false);
        }

        /// <summary>
        /// Xóa biến môi trường trong HKLM Environment.
        /// </summary>
        public static void DeleteRaw(string key)
        {
            DeleteRaw(EnvScope.Machine, key);
        }

        /// <summary>
        /// Liệt kê mọi biến môi trường thô và kiểu registry trong phạm vi chỉ định.
        /// </summary>
        public static List<(string Key, EnvRegistryValue Value)> ListAll(EnvScope scope)
        {
            using var baseKey = OpenEnvironmentKey(scope, writable: false);
            if (baseKey == null)
                throw new InvalidOperationException($"Không thể mở khóa registry {scope} Environment để đọc.");

            var result = new List<(string Key, EnvRegistryValue Value)>();
            string[] valueNames = baseKey.GetValueNames();
            foreach (string name in valueNames)
            {
                if (string.IsNullOrEmpty(name))
                    continue;

                object? rawObj = baseKey.GetValue(name, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
                if (rawObj != null)
                {
                    RegistryValueKind kind = baseKey.GetValueKind(name);
                    result.Add((name, new EnvRegistryValue(rawObj.ToString() ?? string.Empty, kind)));
                }
            }

            return result;
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
