using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace SystemManagement.Views.ConfigEnv
{
    /// <summary>
    /// Đại diện cho một bản sao lưu biến môi trường theo phạm vi.
    /// </summary>
    public record EnvBackupEntry(
        string FilePath,
        string Key,
        string Value,
        RegistryValueKind Kind,
        DateTime CreatedAt,
        string Reason,
        EnvScope Scope = EnvScope.Machine);

    /// <summary>
    /// Dịch vụ sao lưu và quản lý các bản sao lưu biến môi trường cho cả hai phạm vi Machine và User.
    /// Giải thích: Trước khi ghi đè, xóa hoặc khôi phục biến môi trường, bắt buộc phải lưu giá trị cũ
    /// vào thư mục Backups/Env/ để người dùng có thể khôi phục khi có sự cố.
    /// </summary>
    public static class EnvBackupService
    {
        /// <summary>
        /// Đường dẫn thư mục sao lưu biến môi trường: Backups/Env/ cạnh file thực thi.
        /// </summary>
        public static string BackupDirectory => Path.Combine(AppContext.BaseDirectory, "Backups", "Env");

        /// <summary>
        /// Tạo một bản sao lưu mới cho biến môi trường theo phạm vi chỉ định.
        /// Tên file có dạng: {Scope}_{safeKey}_{yyyyMMdd_HHmmss}[_n].txt.
        /// </summary>
        public static string CreateBackup(EnvScope scope, string key, string rawValue, RegistryValueKind kind, string reason)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Tên biến không được để trống.", nameof(key));

            // Tự động tạo thư mục sao lưu nếu chưa tồn tại
            if (!Directory.Exists(BackupDirectory))
            {
                Directory.CreateDirectory(BackupDirectory);
            }

            string safeKey = SanitizeFileName(key);
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string baseFileName = $"{scope}_{safeKey}_{timestamp}";
            string filePath = Path.Combine(BackupDirectory, $"{baseFileName}.txt");

            // Xử lý chống ghi đè khi gọi nhiều lần trong cùng 1 giây
            int counter = 1;
            while (File.Exists(filePath))
            {
                filePath = Path.Combine(BackupDirectory, $"{baseFileName}_{counter}.txt");
                counter++;
            }

            var sb = new StringBuilder();
            sb.AppendLine($"Key={key}");
            sb.AppendLine($"Scope={scope}");
            sb.AppendLine($"Kind={kind}");
            sb.AppendLine($"CreatedAt={DateTime.Now:yyyy-MM-ddTHH:mm:ss}");
            sb.AppendLine($"Reason={reason}");
            sb.Append($"Value={rawValue}");

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
            return filePath;
        }

        /// <summary>
        /// Tạo một bản sao lưu mới cho biến môi trường Machine - giữ nguyên chữ ký và hành vi N1/N2.
        /// </summary>
        public static string CreateBackup(string key, string rawValue, RegistryValueKind kind, string reason)
        {
            return CreateBackup(EnvScope.Machine, key, rawValue, kind, reason);
        }

        /// <summary>
        /// Đọc và phân tích cú pháp một file sao lưu.
        /// Trả về null nếu file không tồn tại, thiếu trường bắt buộc, hoặc sai định dạng.
        /// File cũ không có dòng Scope= được coi là Scope=Machine.
        /// </summary>
        public static EnvBackupEntry? TryRead(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                return null;

            try
            {
                string content = File.ReadAllText(path, Encoding.UTF8);

                int valuePrefixIndex = -1;
                if (content.StartsWith("Value=", StringComparison.Ordinal))
                {
                    valuePrefixIndex = 0;
                }
                else
                {
                    int idx = content.IndexOf("\nValue=", StringComparison.Ordinal);
                    if (idx >= 0)
                    {
                        valuePrefixIndex = idx + 1;
                    }
                }

                if (valuePrefixIndex < 0)
                    return null;

                int valueContentStart = valuePrefixIndex + "Value=".Length;
                string headerPart = content.Substring(0, valuePrefixIndex);
                string value = content.Substring(valueContentStart);

                string? key = null;
                string? scopeStr = null;
                string? kindStr = null;
                string? createdAtStr = null;
                string? reason = null;

                using (var reader = new StringReader(headerPart))
                {
                    string? line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (key == null && line.StartsWith("Key=", StringComparison.Ordinal))
                        {
                            key = line["Key=".Length..];
                        }
                        else if (scopeStr == null && line.StartsWith("Scope=", StringComparison.Ordinal))
                        {
                            scopeStr = line["Scope=".Length..];
                        }
                        else if (kindStr == null && line.StartsWith("Kind=", StringComparison.Ordinal))
                        {
                            kindStr = line["Kind=".Length..];
                        }
                        else if (createdAtStr == null && line.StartsWith("CreatedAt=", StringComparison.Ordinal))
                        {
                            createdAtStr = line["CreatedAt=".Length..];
                        }
                        else if (reason == null && line.StartsWith("Reason=", StringComparison.Ordinal))
                        {
                            reason = line["Reason=".Length..];
                        }
                    }
                }

                // Thiếu bất kỳ trường nào trong các trường bắt buộc đều coi là file không hợp lệ
                if (string.IsNullOrEmpty(key) || kindStr == null || createdAtStr == null || reason == null || value == null)
                    return null;

                // Xử lý trường Scope (tùy chọn): nếu thiếu thì mặc định coi là Machine; nếu có nhưng giá trị lạ thì bỏ qua (trả null)
                EnvScope scope = EnvScope.Machine;
                if (scopeStr != null)
                {
                    if (!Enum.TryParse<EnvScope>(scopeStr, ignoreCase: false, out var parsedScope) || !Enum.IsDefined(typeof(EnvScope), parsedScope))
                    {
                        return null;
                    }
                    scope = parsedScope;
                }

                // Chỉ chấp nhận kiểu ExpandString hoặc String
                if (!Enum.TryParse<RegistryValueKind>(kindStr, out var kind) ||
                    (kind != RegistryValueKind.ExpandString && kind != RegistryValueKind.String))
                {
                    return null;
                }

                if (!DateTime.TryParseExact(createdAtStr, "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var createdAt))
                {
                    return null;
                }

                return new EnvBackupEntry(path, key, value, kind, createdAt, reason, scope);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Liệt kê mọi bản sao lưu hợp lệ của một phạm vi (Machine hoặc User), sắp xếp từ mới nhất đến cũ nhất.
        /// Bỏ qua các file hỏng, thiếu trường bắt buộc hoặc scope không khớp.
        /// </summary>
        public static List<EnvBackupEntry> ListBackups(EnvScope scope)
        {
            if (!Directory.Exists(BackupDirectory))
                return new List<EnvBackupEntry>();

            try
            {
                var dirInfo = new DirectoryInfo(BackupDirectory);
                var files = dirInfo.GetFiles("*.txt");
                var result = new List<EnvBackupEntry>();

                foreach (var file in files)
                {
                    var entry = TryRead(file.FullName);
                    // Bỏ qua file hỏng hoặc scope không khớp
                    if (entry != null && entry.Scope == scope)
                    {
                        result.Add(entry);
                    }
                }

                return result
                    .OrderByDescending(x => x.CreatedAt)
                    .ThenByDescending(x => x.FilePath)
                    .ToList();
            }
            catch
            {
                return new List<EnvBackupEntry>();
            }
        }

        /// <summary>
        /// Liệt kê các bản sao lưu hợp lệ của một key cụ thể thuộc phạm vi Machine (giữ nguyên hành vi N1/N2 cho các biến cốt lõi),
        /// sắp xếp từ mới nhất đến cũ nhất. Bỏ qua các file hỏng, thiếu trường, sai key hoặc không thuộc scope Machine.
        /// </summary>
        public static List<EnvBackupEntry> ListBackups(string key)
        {
            if (string.IsNullOrWhiteSpace(key) || !Directory.Exists(BackupDirectory))
                return new List<EnvBackupEntry>();

            try
            {
                var dirInfo = new DirectoryInfo(BackupDirectory);
                var files = dirInfo.GetFiles("*.txt");
                var result = new List<EnvBackupEntry>();

                foreach (var file in files)
                {
                    var entry = TryRead(file.FullName);
                    // Chỉ lấy bản sao lưu thuộc phạm vi Machine để giữ nguyên hành vi N1/N2 (tránh lấy nhầm bản sao lưu User cho biến cốt lõi)
                    // và bỏ qua file hỏng hoặc key không khớp (so sánh không phân biệt hoa thường theo chuẩn env Windows)
                    if (entry != null && entry.Scope == EnvScope.Machine && string.Equals(entry.Key, key, StringComparison.OrdinalIgnoreCase))
                    {
                        result.Add(entry);
                    }
                }

                return result
                    .OrderByDescending(x => x.CreatedAt)
                    .ThenByDescending(x => x.FilePath)
                    .ToList();
            }
            catch
            {
                return new List<EnvBackupEntry>();
            }
        }

        /// <summary>
        /// Thay thế các ký tự không hợp lệ trong tên file Windows bằng dấu gạch dưới '_'.
        /// </summary>
        private static string SanitizeFileName(string key)
        {
            char[] invalidChars = Path.GetInvalidFileNameChars();
            var sb = new StringBuilder(key.Length);
            foreach (char c in key)
            {
                sb.Append(invalidChars.Contains(c) ? '_' : c);
            }
            return sb.ToString();
        }
    }
}
