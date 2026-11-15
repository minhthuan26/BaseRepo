using CommonLibs.Utils;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;

namespace SystemManagement.Views.ConfigEnv
{
    public record CustomEnvItem(string Key, string Value, bool IsProtected, bool NeedsExtraWarning);

    public static class CustomEnvService
    {
        private static readonly HashSet<string> CoreProtectedKeys = new(StringComparer.OrdinalIgnoreCase)
        {
            "Path",
            "PATHEXT",
            "ComSpec",
            "SystemRoot",
            "windir",
            "SystemDrive",
            "OS",
            "TEMP",
            "TMP",
            "PSModulePath",
            "PROCESSOR_ARCHITECTURE",
            "NUMBER_OF_PROCESSORS"
        };

        private static readonly HashSet<string> ListKeys = new(StringComparer.OrdinalIgnoreCase)
        {
            "Path",
            "PATHEXT",
            "PSModulePath"
        };

        public static bool IsProtected(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return false;

            return CoreProtectedKeys.Contains(key.Trim());
        }

        public static bool IsListKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return false;

            return ListKeys.Contains(key.Trim());
        }

        /// <summary>
        /// Kiểm tra tiến trình có đang chạy dưới quyền Administrator (elevated) hay không.
        /// Giải thích: Sửa registry HKLM bắt buộc tiến trình phải có quyền Administrator.
        /// </summary>
        public static bool IsElevated()
        {
            if (!OperatingSystem.IsWindows())
                return false;

            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        /// <summary>
        /// Đọc giá trị thô (chưa mở rộng %...%) của một biến hệ thống cốt lõi.
        /// Ném KeyNotFoundException nếu biến không còn tồn tại trên máy.
        /// </summary>
        public static string GetCoreRawValue(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Tên biến không được để trống.", nameof(key));

            var raw = EnvRegistryService.ReadRaw(key);
            if (raw == null)
                throw new KeyNotFoundException($"Biến '{key}' không còn tồn tại trên máy.");

            return raw.Value;
        }

        /// <summary>
        /// Cập nhật giá trị biến hệ thống cốt lõi (chấp nhận cả biến danh sách vì N2-2 dùng lại; UI mới là nơi chặn list key).
        /// Bắt buộc sao lưu giá trị cũ trước khi ghi.
        /// </summary>
        public static void UpdateCoreValue(string key, string newValue)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Tên biến không được để trống.", nameof(key));

            if (!IsProtected(key))
                throw new InvalidOperationException($"Biến '{key}' không phải là biến hệ thống cốt lõi.");

            if (!IsElevated())
                throw new UnauthorizedAccessException("Cần chạy ứng dụng với quyền Administrator để sửa biến hệ thống cốt lõi.");

            string? valError = ValidateValue(newValue);
            if (valError != null)
                throw new ArgumentException(valError);

            if (newValue.Contains('\r') || newValue.Contains('\n'))
                throw new ArgumentException("Giá trị biến không được chứa ký tự xuống dòng.");

            var current = EnvRegistryService.ReadRaw(key);
            if (current == null)
                throw new KeyNotFoundException($"Biến '{key}' không còn tồn tại trên máy.");

            // Kiểm tra kiểu dữ liệu của biến hiện có: chỉ hỗ trợ String hoặc ExpandString
            if (current.Kind != RegistryValueKind.String && current.Kind != RegistryValueKind.ExpandString)
                throw new InvalidOperationException($"Biến '{key}' có kiểu dữ liệu '{current.Kind}' không được hỗ trợ (chỉ hỗ trợ String hoặc ExpandString).");

            // Sao lưu giá trị hiện tại trước khi ghi; nếu CreateBackup lỗi thì dừng ngay không ghi registry
            EnvBackupService.CreateBackup(key, current.Value, current.Kind, reason: "Edit");

            // Ghi giá trị mới và giữ nguyên kiểu RegistryValueKind cũ
            EnvRegistryService.WriteRaw(key, newValue, current.Kind);

            // Broadcast thông báo toàn hệ thống về thay đổi biến môi trường
            EnvRegistryService.BroadcastSettingChange();
        }

        /// <summary>
        /// Khôi phục biến hệ thống cốt lõi từ một bản sao lưu.
        /// Sao lưu giá trị hiện tại (reason: BeforeRestore) trước khi ghi đè.
        /// Thứ tự thực hiện: kiểm tra -> ReadRaw -> CreateBackup (lỗi thì dừng) -> WriteRaw -> Broadcast.
        /// </summary>
        public static void RestoreCoreValue(EnvBackupEntry entry)
        {
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));

            if (string.IsNullOrWhiteSpace(entry.Key))
                throw new ArgumentException("Tên biến trong bản sao lưu không hợp lệ.");

            if (entry.Key != entry.Key.Trim())
                throw new ArgumentException($"Tên biến '{entry.Key}' trong bản sao lưu không được chứa khoảng trắng đầu hoặc cuối.");

            if (!IsProtected(entry.Key))
                throw new InvalidOperationException($"Biến '{entry.Key}' không phải là biến hệ thống cốt lõi.");

            if (!IsElevated())
                throw new UnauthorizedAccessException("Cần chạy ứng dụng với quyền Administrator để khôi phục biến hệ thống cốt lõi.");

            string? valError = ValidateValue(entry.Value);
            if (valError != null)
                throw new ArgumentException($"Giá trị của biến '{entry.Key}' trong bản sao lưu không hợp lệ ({valError}).");

            if (entry.Value.Contains('\r') || entry.Value.Contains('\n'))
                throw new ArgumentException($"Giá trị của biến '{entry.Key}' trong bản sao lưu không được chứa ký tự xuống dòng.");

            // Đọc giá trị hiện tại; nếu biến thực sự không tồn tại (ReadRaw trả null) thì bỏ qua backup BeforeRestore;
            // mọi lỗi đọc khác (ReadRaw ném lỗi) sẽ làm dừng ngay lập tức, không ghi.
            var current = EnvRegistryService.ReadRaw(entry.Key);
            if (current != null)
            {
                // Kiểm tra kiểu dữ liệu của biến hiện có trước khi tạo backup/ghi đè
                if (current.Kind != RegistryValueKind.String && current.Kind != RegistryValueKind.ExpandString)
                    throw new InvalidOperationException($"Biến '{entry.Key}' có kiểu dữ liệu '{current.Kind}' không được hỗ trợ (chỉ hỗ trợ String hoặc ExpandString).");

                // Sao lưu giá trị hiện tại trước khi ghi đè; lỗi thì dừng không ghi
                EnvBackupService.CreateBackup(entry.Key, current.Value, current.Kind, reason: "BeforeRestore");
            }

            // Ghi đè giá trị và kiểu từ bản sao lưu
            EnvRegistryService.WriteRaw(entry.Key, entry.Value, entry.Kind);

            // Broadcast thông báo toàn hệ thống
            EnvRegistryService.BroadcastSettingChange();
        }

        public static bool NeedsExtraWarning(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return false;

            return !key.Trim().StartsWith("BaseRepo__", StringComparison.OrdinalIgnoreCase);
        }

        public static List<CustomEnvItem> GetCustomEnvs(IEnumerable<string> defaultKeys)
        {
            var defaultKeySet = new HashSet<string>(defaultKeys ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            var machineEnvs = Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Machine);

            var result = new List<CustomEnvItem>();
            foreach (DictionaryEntry de in machineEnvs)
            {
                string? key = de.Key?.ToString();
                if (string.IsNullOrEmpty(key))
                    continue;

                if (defaultKeySet.Contains(key))
                    continue;

                string value = de.Value?.ToString() ?? string.Empty;
                bool isProtected = IsProtected(key);
                bool needsWarning = NeedsExtraWarning(key);

                result.Add(new CustomEnvItem(key, value, isProtected, needsWarning));
            }

            return result.OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase).ToList();
        }

        public static string? ValidateKey(string key, IEnumerable<string> defaultKeys)
        {
            if (string.IsNullOrEmpty(key))
                return "Tên biến không được để trống.";

            if (char.IsWhiteSpace(key[0]) || char.IsWhiteSpace(key[^1]))
                return "Tên biến không được chứa khoảng trắng ở đầu hoặc cuối.";

            if (string.IsNullOrWhiteSpace(key))
                return "Tên biến không được chỉ chứa khoảng trắng.";

            if (key.Length > 255)
                return "Tên biến không được dài quá 255 ký tự.";

            if (key.Contains('='))
                return "Tên biến không được chứa ký tự '='.";

            if (key.Any(char.IsControl))
                return "Tên biến không được chứa ký tự điều khiển.";

            var defaultKeySet = new HashSet<string>(defaultKeys ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            if (defaultKeySet.Contains(key))
                return "Tên biến trùng với biến mặc định của hệ thống.";

            var machineEnvs = Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Machine);
            foreach (var k in machineEnvs.Keys)
            {
                if (string.Equals(k?.ToString(), key, StringComparison.OrdinalIgnoreCase))
                    return "Tên biến đã tồn tại trong biến môi trường Machine.";
            }

            return null;
        }

        public static string? ValidateValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "Giá trị biến không được để trống hoặc chỉ chứa khoảng trắng.";

            return null;
        }

        public static void Create(string key, string value, IEnumerable<string> defaultKeys)
        {
            string? keyError = ValidateKey(key, defaultKeys);
            if (keyError != null)
                throw new ArgumentException(keyError);

            string? valError = ValidateValue(value);
            if (valError != null)
                throw new ArgumentException(valError);

            Environment.SetEnvironmentVariable(key, value, EnvironmentVariableTarget.Machine);
        }

        public static void UpdateValue(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Tên biến không được để trống.", nameof(key));

            if (IsProtected(key))
                throw new InvalidOperationException($"Biến '{key}' là biến hệ thống cốt lõi, không được phép sửa.");

            string? valError = ValidateValue(value);
            if (valError != null)
                throw new ArgumentException(valError);

            string? current = Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.Machine);
            if (current == null)
                throw new KeyNotFoundException($"Biến '{key}' không còn tồn tại trên máy.");

            Environment.SetEnvironmentVariable(key, value, EnvironmentVariableTarget.Machine);
        }

        public static void Delete(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Tên biến không được để trống.", nameof(key));

            if (IsProtected(key))
                throw new InvalidOperationException($"Biến '{key}' là biến hệ thống cốt lõi, không được phép xóa.");

            string? current = Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.Machine);
            if (current == null)
                throw new KeyNotFoundException($"Biến '{key}' không còn tồn tại trên máy.");

            EnvironmentHelper.Delete(key, EnvironmentVariableTarget.Machine);
        }
    }
}
