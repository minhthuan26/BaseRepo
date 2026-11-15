using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SystemManagement.Views.ConfigEnv
{
    /// <summary>
    /// Cung cấp các hàm xử lý logic thuần cho biến môi trường dạng danh sách (Path, PATHEXT, PSModulePath).
    /// Giải thích: Tách biệt hoàn toàn logic tách/ghép chuỗi, chuẩn hóa, kiểm tra trùng lặp và xác thực
    /// khỏi giao diện người dùng và tầng registry để dễ kiểm thử và tái sử dụng.
    /// </summary>
    public static class EnvListService
    {
        /// <summary>
        /// Tách chuỗi thô thành danh sách các mục phân tách bởi dấu chấm phẩy ';'.
        /// Giữ nguyên thứ tự ban đầu và các mục rỗng.
        /// </summary>
        public static List<string> Split(string rawValue)
        {
            if (rawValue == null)
                return new List<string>();

            return rawValue.Split(';').ToList();
        }

        /// <summary>
        /// Chuẩn hóa danh sách các mục: cắt khoảng trắng đầu/cuối của từng mục và loại bỏ các mục rỗng.
        /// </summary>
        public static List<string> Normalize(IEnumerable<string> items)
        {
            if (items == null)
                return new List<string>();

            return items
                .Where(x => x != null)
                .Select(x => x.Trim())
                .Where(x => x.Length > 0)
                .ToList();
        }

        /// <summary>
        /// Ghép danh sách các mục thành chuỗi phân tách bởi dấu chấm phẩy ';', không có dấu ';' ở cuối.
        /// </summary>
        public static string Join(IEnumerable<string> items)
        {
            if (items == null)
                return string.Empty;

            return string.Join(";", items);
        }

        /// <summary>
        /// Tìm các mục bị trùng lặp trong danh sách (so sánh không phân biệt hoa thường theo OrdinalIgnoreCase).
        /// Mỗi mục bị trùng lặp chỉ được trả về một lần duy nhất.
        /// </summary>
        public static List<string> FindDuplicates(IEnumerable<string> items)
        {
            if (items == null)
                return new List<string>();

            var seen = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var duplicateKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var result = new List<string>();

            foreach (var rawItem in items)
            {
                if (rawItem == null)
                    continue;

                string item = rawItem.Trim();
                if (item.Length == 0)
                    continue;

                if (!seen.ContainsKey(item))
                {
                    seen[item] = item;
                }
                else
                {
                    if (duplicateKeys.Add(item))
                    {
                        result.Add(seen[item]);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Tìm các mục là đường dẫn thư mục không tồn tại trên máy.
        /// Bỏ qua các mục chứa ký tự '%' (biến mở rộng động như %SystemRoot%).
        /// Không dùng cho PATHEXT (vì PATHEXT là danh sách phần mở rộng tập tin).
        /// </summary>
        public static List<string> FindMissingPaths(IEnumerable<string> items)
        {
            if (items == null)
                return new List<string>();

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var missing = new List<string>();

            foreach (var rawItem in items)
            {
                if (rawItem == null)
                    continue;

                string item = rawItem.Trim();
                if (item.Length == 0 || item.Contains('%'))
                    continue;

                bool exists;
                try
                {
                    exists = Directory.Exists(item);
                }
                catch
                {
                    exists = false;
                }

                if (!exists && seen.Add(item))
                {
                    missing.Add(item);
                }
            }

            return missing;
        }

        /// <summary>
        /// Xác thực tính hợp lệ của danh sách các mục:
        /// - Trả về thông báo lỗi nếu danh sách rỗng hoặc chuỗi ghép rỗng.
        /// - Trả về thông báo lỗi nếu tổng độ dài chuỗi sau khi ghép vượt quá 2047 ký tự.
        /// - Trả về null nếu danh sách hợp lệ.
        /// </summary>
        public static string? Validate(IEnumerable<string> items)
        {
            if (items == null)
                return "Danh sách không được để trống.";

            var list = items.ToList();
            if (list.Count == 0)
                return "Danh sách không được để trống.";

            string joined = Join(list);
            if (string.IsNullOrWhiteSpace(joined))
                return "Danh sách không được để trống.";

            if (joined.Length > 2047)
                return $"Độ dài chuỗi giá trị ({joined.Length} ký tự) vượt quá giới hạn tối đa cho phép (2047 ký tự).";

            return null;
        }
    }
}
