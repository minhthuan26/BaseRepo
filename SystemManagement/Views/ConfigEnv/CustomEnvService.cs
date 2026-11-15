using CommonLibs.Utils;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;

namespace SystemManagement.Views.ConfigEnv
{
    public record CustomEnvItem(
        string Key,
        string Value,
        bool IsProtected,
        bool NeedsExtraWarning,
        EnvScope Scope = EnvScope.Machine,
        RegistryValueKind Kind = RegistryValueKind.String);

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
        /// Kiểm tra phạm vi biến môi trường có yêu cầu quyền Administrator hay không.
        /// Giải thích: Chỉ phạm vi Machine (HKLM) yêu cầu quyền Administrator; phạm vi User (HKCU) không yêu cầu.
        /// </summary>
        public static bool RequiresElevation(EnvScope scope)
        {
            return scope == EnvScope.Machine;
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

            // Biến cốt lõi luôn thuộc Machine (HKLM), chặn khôi phục nếu bản sao lưu thuộc phạm vi User
            if (entry.Scope != EnvScope.Machine)
                throw new InvalidOperationException("Bản sao lưu thuộc phạm vi User, không dùng để khôi phục biến cốt lõi Machine.");

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

        /// <summary>
        /// Liệt kê mọi biến môi trường trong phạm vi chỉ định (Machine hoặc User), bao gồm cả biến mặc định.
        /// Sắp xếp theo tên biến không phân biệt hoa thường. Giá trị Value là chuỗi thô (chưa mở rộng %...%).
        /// </summary>
        public static List<CustomEnvItem> GetVariables(EnvScope scope)
        {
            var rawList = EnvRegistryService.ListAll(scope);
            var result = new List<CustomEnvItem>(rawList.Count);

            foreach (var (key, regVal) in rawList)
            {
                if (string.IsNullOrEmpty(key))
                    continue;

                bool isProtected = IsProtected(key);
                bool needsWarning = NeedsExtraWarning(key);
                result.Add(new CustomEnvItem(key, regVal.Value, isProtected, needsWarning, scope, regVal.Kind));
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

        /// <summary>
        /// Tạo một biến môi trường mới trong phạm vi chỉ định (Machine hoặc User).
        /// Giải thích: Kiểm tra quyền Administrator (chỉ với Machine), kiểm tra trùng lặp trong cùng scope,
        /// ghi String (hoặc ExpandString nếu chứa %), không tạo backup trước khi tạo mới.
        /// </summary>
        public static void CreateVariable(EnvScope scope, string key, string value, IEnumerable<string>? defaultKeys = null)
        {
            if (RequiresElevation(scope) && !IsElevated())
                throw new UnauthorizedAccessException("Cần chạy ứng dụng với quyền Administrator để tạo biến môi trường Machine.");

            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Tên biến không được để trống.", nameof(key));

            if (char.IsWhiteSpace(key[0]) || char.IsWhiteSpace(key[^1]))
                throw new ArgumentException("Tên biến không được chứa khoảng trắng ở đầu hoặc cuối.", nameof(key));

            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Tên biến không được chỉ chứa khoảng trắng.", nameof(key));

            if (key.Length > 255)
                throw new ArgumentException("Tên biến không được dài quá 255 ký tự.", nameof(key));

            if (key.Contains('='))
                throw new ArgumentException("Tên biến không được chứa ký tự '='.", nameof(key));

            if (key.Any(char.IsControl))
                throw new ArgumentException("Tên biến không được chứa ký tự điều khiển.", nameof(key));

            // Chỉ ở Machine mới chặn trùng default keys
            if (scope == EnvScope.Machine && defaultKeys != null)
            {
                var defaultKeySet = new HashSet<string>(defaultKeys, StringComparer.OrdinalIgnoreCase);
                if (defaultKeySet.Contains(key))
                    throw new ArgumentException("Tên biến trùng với biến mặc định của hệ thống.", nameof(key));
            }

            // Kiểm tra trùng lặp trong cùng phạm vi thông qua ReadRaw
            if (EnvRegistryService.ReadRaw(scope, key) != null)
                throw new ArgumentException($"Tên biến '{key}' đã tồn tại trong phạm vi {scope}.", nameof(key));

            string? valError = ValidateValue(value);
            if (valError != null)
                throw new ArgumentException(valError, nameof(value));

            if (value.Contains('\r') || value.Contains('\n'))
                throw new ArgumentException("Giá trị biến không được chứa ký tự xuống dòng.", nameof(value));

            // Tự động chọn REG_EXPAND_SZ nếu giá trị có chứa ký tự %, ngược lại là REG_SZ
            RegistryValueKind kind = value.Contains('%') ? RegistryValueKind.ExpandString : RegistryValueKind.String;

            EnvRegistryService.WriteRaw(scope, key, value, kind);
            EnvRegistryService.BroadcastSettingChange();
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

        /// <summary>
        /// Cập nhật giá trị biến môi trường trong phạm vi chỉ định.
        /// Áp dụng cho mọi biến (cốt lõi hay thường).
        /// Giải thích: Tự động sao lưu giá trị cũ trước khi ghi đè; giữ nguyên kiểu RegistryValueKind cũ.
        /// </summary>
        public static void UpdateVariable(EnvScope scope, string key, string newValue)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Tên biến không được để trống.", nameof(key));

            if (RequiresElevation(scope) && !IsElevated())
                throw new UnauthorizedAccessException("Cần chạy ứng dụng với quyền Administrator để sửa biến môi trường Machine.");

            string? valError = ValidateValue(newValue);
            if (valError != null)
                throw new ArgumentException(valError, nameof(newValue));

            if (newValue.Contains('\r') || newValue.Contains('\n'))
                throw new ArgumentException("Giá trị biến không được chứa ký tự xuống dòng.", nameof(newValue));

            var current = EnvRegistryService.ReadRaw(scope, key);
            if (current == null)
                throw new KeyNotFoundException($"Biến '{key}' không còn tồn tại trong phạm vi {scope}.");

            if (current.Kind != RegistryValueKind.String && current.Kind != RegistryValueKind.ExpandString)
                throw new InvalidOperationException($"Biến '{key}' có kiểu dữ liệu '{current.Kind}' không được hỗ trợ (chỉ hỗ trợ String hoặc ExpandString).");

            // Sao lưu giá trị hiện tại trước khi ghi; nếu sao lưu lỗi thì dừng ngay không ghi registry
            EnvBackupService.CreateBackup(scope, key, current.Value, current.Kind, reason: "Edit");

            // Ghi giá trị mới và bảo toàn kiểu registry cũ
            EnvRegistryService.WriteRaw(scope, key, newValue, current.Kind);
            EnvRegistryService.BroadcastSettingChange();
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

        /// <summary>
        /// Xóa biến môi trường trong phạm vi chỉ định.
        /// Giải thích: Cấm xóa biến cốt lõi (kiểm tra trước khi chạm registry);
        /// bắt buộc sao lưu giá trị hiện tại trước khi xóa để có thể khôi phục lại.
        /// </summary>
        public static void DeleteVariable(EnvScope scope, string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Tên biến không được để trống.", nameof(key));

            // Chặn xóa biến cốt lõi ngay trước khi chạm registry
            if (IsProtected(key))
                throw new InvalidOperationException($"Biến '{key}' là biến hệ thống cốt lõi, không được phép xóa.");

            if (RequiresElevation(scope) && !IsElevated())
                throw new UnauthorizedAccessException("Cần chạy ứng dụng với quyền Administrator để xóa biến môi trường Machine.");

            var current = EnvRegistryService.ReadRaw(scope, key);
            if (current == null)
                throw new KeyNotFoundException($"Biến '{key}' không còn tồn tại trong phạm vi {scope}.");

            if (current.Kind != RegistryValueKind.String && current.Kind != RegistryValueKind.ExpandString)
                throw new InvalidOperationException($"Biến '{key}' có kiểu dữ liệu '{current.Kind}' không được hỗ trợ (chỉ hỗ trợ String hoặc ExpandString).");

            // Sao lưu trước khi xóa; nếu sao lưu lỗi thì dừng không xóa
            EnvBackupService.CreateBackup(scope, key, current.Value, current.Kind, reason: "Delete");

            EnvRegistryService.DeleteRaw(scope, key);
            EnvRegistryService.BroadcastSettingChange();
        }

        /// <summary>
        /// Khôi phục biến môi trường từ một bản sao lưu (hỗ trợ mọi biến, cả cốt lõi lẫn tùy chỉnh, ở cả Machine và User).
        /// Giải thích: Nếu biến hiện vẫn còn tồn tại thì sao lưu lại giá trị hiện tại (reason: BeforeRestore) trước khi ghi đè;
        /// nếu biến đã bị xóa thì bỏ qua bước sao lưu BeforeRestore và tạo lại. Không yêu cầu biến phải là cốt lõi.
        /// </summary>
        public static void RestoreVariable(EnvBackupEntry entry)
        {
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));

            if (string.IsNullOrWhiteSpace(entry.Key))
                throw new ArgumentException("Tên biến trong bản sao lưu không hợp lệ.", nameof(entry));

            if (entry.Key != entry.Key.Trim())
                throw new ArgumentException($"Tên biến '{entry.Key}' trong bản sao lưu không được chứa khoảng trắng đầu hoặc cuối.", nameof(entry));

            string? valError = ValidateValue(entry.Value);
            if (valError != null)
                throw new ArgumentException($"Giá trị của biến '{entry.Key}' trong bản sao lưu không hợp lệ ({valError}).", nameof(entry));

            if (entry.Value.Contains('\r') || entry.Value.Contains('\n'))
                throw new ArgumentException($"Giá trị của biến '{entry.Key}' trong bản sao lưu không được chứa ký tự xuống dòng.", nameof(entry));

            if (entry.Kind != RegistryValueKind.String && entry.Kind != RegistryValueKind.ExpandString)
                throw new InvalidOperationException($"Bản sao lưu có kiểu dữ liệu '{entry.Kind}' không được hỗ trợ (chỉ hỗ trợ String hoặc ExpandString).");

            if (RequiresElevation(entry.Scope) && !IsElevated())
                throw new UnauthorizedAccessException("Cần chạy ứng dụng với quyền Administrator để khôi phục biến môi trường Machine.");

            // Kiểm tra biến hiện tại trong scope của bản sao lưu
            var current = EnvRegistryService.ReadRaw(entry.Scope, entry.Key);
            if (current != null)
            {
                if (current.Kind != RegistryValueKind.String && current.Kind != RegistryValueKind.ExpandString)
                    throw new InvalidOperationException($"Biến '{entry.Key}' có kiểu dữ liệu '{current.Kind}' không được hỗ trợ (chỉ hỗ trợ String hoặc ExpandString).");

                // Sao lưu giá trị hiện tại trước khi khôi phục
                EnvBackupService.CreateBackup(entry.Scope, entry.Key, current.Value, current.Kind, reason: "BeforeRestore");
            }

            EnvRegistryService.WriteRaw(entry.Scope, entry.Key, entry.Value, entry.Kind);
            EnvRegistryService.BroadcastSettingChange();
        }
    }
}
