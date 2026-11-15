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
    /// Đại diện cho một bản sao lưu biến môi trường.
    /// </summary>
    public record EnvBackupEntry(string FilePath, string Key, string Value, RegistryValueKind Kind, DateTime CreatedAt, string Reason);

    /// <summary>
    /// Dịch vụ sao lưu và quản lý các bản sao lưu biến môi trường cốt lõi.
    /// Giải thích: Trước khi ghi đè hoặc khôi phục biến cốt lõi, bắt buộc phải lưu giá trị cũ
    /// vào thư mục Backups/Env/ để người dùng có thể khôi phục khi có sự cố.
    /// </summary>
    public static class EnvBackupService
    {
        /// <summary>
        /// Đường dẫn thư mục sao lưu biến môi trường: Backups/Env/ cạnh file thực thi.
        /// </summary>
        public static string BackupDirectory => Path.Combine(AppContext.BaseDirectory, "Backups", "Env");

        /// <summary>
        /// Tạo một bản sao lưu mới cho biến môi trường.
        /// Tên file có dạng: {key}_{yyyyMMdd_HHmmss}.txt (trùng giây thêm hậu tố _1, _2...).
        /// </summary>
        public static string CreateBackup(string key, string rawValue, RegistryValueKind kind, string reason)
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
            string baseFileName = $"{safeKey}_{timestamp}";
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
            sb.AppendLine($"Kind={kind}");
            sb.AppendLine($"CreatedAt={DateTime.Now:yyyy-MM-ddTHH:mm:ss}");
            sb.AppendLine($"Reason={reason}");
            sb.Append($"Value={rawValue}");

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
            return filePath;
        }

        /// <summary>
        /// Đọc và phân tích cú pháp một file sao lưu.
        /// Trả về null nếu file không tồn tại, thiếu trường, hoặc sai định dạng.
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

                // Thiếu bất kỳ trường nào trong 5 trường đều coi là file không hợp lệ
                if (string.IsNullOrEmpty(key) || kindStr == null || createdAtStr == null || reason == null || value == null)
                    return null;

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

                return new EnvBackupEntry(path, key, value, kind, createdAt, reason);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Liệt kê các bản sao lưu hợp lệ của một key cụ thể, sắp xếp từ mới nhất đến cũ nhất.
        /// Bỏ qua các file hỏng, thiếu trường hoặc sai key.
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
                    // Bỏ qua file hỏng hoặc key không khớp (so sánh không phân biệt hoa thường theo chuẩn env Windows)
                    if (entry != null && string.Equals(entry.Key, key, StringComparison.OrdinalIgnoreCase))
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
