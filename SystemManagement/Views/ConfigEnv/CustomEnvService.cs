using CommonLibs.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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

        public static bool IsProtected(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return false;

            return CoreProtectedKeys.Contains(key.Trim());
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
