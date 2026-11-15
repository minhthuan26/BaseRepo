using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SystemManagement.Views.ConfigEnv
{
    /// <summary>
    /// Form chỉnh sửa từng mục cho các biến môi trường dạng danh sách (Path, PATHEXT, PSModulePath).
    /// Giải thích: Cho phép thêm, cập nhật, xóa, thay đổi thứ tự các mục phân tách bởi dấu ';',
    /// đồng thời kiểm tra trùng lặp và đường dẫn không tồn tại trước khi lưu.
    /// </summary>
    public partial class FrmEnvListEditor : Form
    {
        private readonly string _key;

        public FrmEnvListEditor(string key, string rawValue)
        {
            _key = key ?? string.Empty;

            InitializeComponent();

            Text = $"Sửa danh sách biến '{_key}'";
            lbl_Title.Text = $"Chỉnh sửa danh sách các mục của biến: {_key}";

            // Nạp dữ liệu ban đầu
            LoadItems(rawValue);
        }

        private void LoadItems(string rawValue)
        {
            var items = EnvListService.Split(rawValue);
            lst_Items.Items.Clear();
            foreach (var item in items)
            {
                lst_Items.Items.Add(item);
            }

            if (lst_Items.Items.Count > 0)
            {
                lst_Items.SelectedIndex = 0;
            }

            UpdateButtonStates();
            UpdateWarningDisplay();
        }

        private void lst_Items_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (lst_Items.SelectedIndex >= 0)
            {
                txt_Input.Text = lst_Items.SelectedItem?.ToString() ?? string.Empty;
            }
            UpdateButtonStates();
        }

        private void txt_Input_TextChanged(object? sender, EventArgs e)
        {
            UpdateButtonStates();
        }

        private void UpdateButtonStates()
        {
            int sel = lst_Items.SelectedIndex;
            bool hasText = !string.IsNullOrWhiteSpace(txt_Input.Text);

            btn_Up.Enabled = sel > 0;
            btn_Down.Enabled = sel >= 0 && sel < lst_Items.Items.Count - 1;
            btn_Delete.Enabled = sel >= 0;
            btn_Update.Enabled = sel >= 0 && hasText;
            btn_Add.Enabled = hasText;
        }

        private void btn_Add_Click(object? sender, EventArgs e)
        {
            string text = txt_Input.Text.Trim();
            if (string.IsNullOrWhiteSpace(text))
            {
                FrmMain.ThrowWarning("Vui lòng nhập nội dung mục cần thêm.");
                return;
            }

            lst_Items.Items.Add(text);
            lst_Items.SelectedIndex = lst_Items.Items.Count - 1;
            txt_Input.Clear();

            UpdateButtonStates();
            UpdateWarningDisplay();
        }

        private void btn_Update_Click(object? sender, EventArgs e)
        {
            int sel = lst_Items.SelectedIndex;
            if (sel < 0)
            {
                FrmMain.ThrowWarning("Vui lòng chọn một mục trong danh sách để cập nhật.");
                return;
            }

            string text = txt_Input.Text.Trim();
            if (string.IsNullOrWhiteSpace(text))
            {
                FrmMain.ThrowWarning("Vui lòng nhập nội dung mục cần cập nhật.");
                return;
            }

            lst_Items.Items[sel] = text;
            lst_Items.SelectedIndex = sel;

            UpdateButtonStates();
            UpdateWarningDisplay();
        }

        private void btn_Delete_Click(object? sender, EventArgs e)
        {
            int sel = lst_Items.SelectedIndex;
            if (sel < 0)
                return;

            lst_Items.Items.RemoveAt(sel);
            if (lst_Items.Items.Count > 0)
            {
                lst_Items.SelectedIndex = Math.Min(sel, lst_Items.Items.Count - 1);
            }
            else
            {
                txt_Input.Clear();
            }

            UpdateButtonStates();
            UpdateWarningDisplay();
        }

        private void btn_Up_Click(object? sender, EventArgs e)
        {
            int sel = lst_Items.SelectedIndex;
            if (sel <= 0)
                return;

            var item = lst_Items.Items[sel];
            lst_Items.Items.RemoveAt(sel);
            lst_Items.Items.Insert(sel - 1, item);
            lst_Items.SelectedIndex = sel - 1;

            UpdateButtonStates();
            UpdateWarningDisplay();
        }

        private void btn_Down_Click(object? sender, EventArgs e)
        {
            int sel = lst_Items.SelectedIndex;
            if (sel < 0 || sel >= lst_Items.Items.Count - 1)
                return;

            var item = lst_Items.Items[sel];
            lst_Items.Items.RemoveAt(sel);
            lst_Items.Items.Insert(sel + 1, item);
            lst_Items.SelectedIndex = sel + 1;

            UpdateButtonStates();
            UpdateWarningDisplay();
        }

        private void UpdateWarningDisplay()
        {
            var currentItems = lst_Items.Items.Cast<object>().Select(x => x?.ToString() ?? string.Empty).ToList();
            var normalized = EnvListService.Normalize(currentItems);
            int totalLength = EnvListService.Join(normalized).Length;

            var duplicates = EnvListService.FindDuplicates(normalized);
            bool isPathExt = string.Equals(_key, "PATHEXT", StringComparison.OrdinalIgnoreCase);
            var missingPaths = !isPathExt ? EnvListService.FindMissingPaths(normalized) : new List<string>();

            var notes = new List<string>();
            notes.Add($"Tổng số mục: {normalized.Count} | Độ dài chuỗi ghép: {totalLength}/2047 ký tự");

            if (totalLength > 2047)
            {
                notes.Add($"• VƯỢT GIỚI HẠN: Độ dài ({totalLength}) vượt quá 2047 ký tự (không thể lưu).");
            }

            if (duplicates.Count > 0)
            {
                string dupPreview = string.Join(", ", duplicates.Take(3)) + (duplicates.Count > 3 ? "..." : "");
                notes.Add($"• Cảnh báo trùng lặp ({duplicates.Count} mục): {dupPreview}");
            }

            if (missingPaths.Count > 0)
            {
                string missPreview = string.Join(", ", missingPaths.Take(2)) + (missingPaths.Count > 2 ? "..." : "");
                notes.Add($"• Cảnh báo đường dẫn không tồn tại ({missingPaths.Count} mục): {missPreview}");
            }

            lbl_Warning.Text = string.Join("\n", notes);
            if (totalLength > 2047)
            {
                pnl_Warning.BackColor = Color.FromArgb(254, 242, 242);
                lbl_Warning.ForeColor = Color.DarkRed;
            }
            else if (duplicates.Count > 0 || missingPaths.Count > 0)
            {
                pnl_Warning.BackColor = Color.FromArgb(255, 251, 235);
                lbl_Warning.ForeColor = Color.DarkGoldenrod;
            }
            else
            {
                pnl_Warning.BackColor = Color.FromArgb(240, 253, 244);
                lbl_Warning.ForeColor = Color.DarkGreen;
            }
        }

        private void btn_Save_Click(object? sender, EventArgs e)
        {
            var currentItems = lst_Items.Items.Cast<object>().Select(x => x?.ToString() ?? string.Empty).ToList();
            var normalized = EnvListService.Normalize(currentItems);

            string? valError = EnvListService.Validate(normalized);
            if (valError != null)
            {
                FrmMain.ThrowError(valError);
                return;
            }

            var duplicates = EnvListService.FindDuplicates(normalized);
            bool isPathExt = string.Equals(_key, "PATHEXT", StringComparison.OrdinalIgnoreCase);
            var missingPaths = !isPathExt ? EnvListService.FindMissingPaths(normalized) : new List<string>();

            if (duplicates.Count > 0 || missingPaths.Count > 0)
            {
                var sb = new StringBuilder();
                sb.AppendLine("Phát hiện các cảnh báo sau trong danh sách:");
                sb.AppendLine();

                if (duplicates.Count > 0)
                {
                    sb.AppendLine($"• Mục trùng lặp ({duplicates.Count}):");
                    foreach (var d in duplicates)
                    {
                        sb.AppendLine($"   - {d}");
                    }
                    sb.AppendLine();
                }

                if (missingPaths.Count > 0)
                {
                    sb.AppendLine($"• Đường dẫn không tồn tại ({missingPaths.Count}):");
                    foreach (var m in missingPaths)
                    {
                        sb.AppendLine($"   - {m}");
                    }
                    sb.AppendLine();
                }

                sb.Append("Bạn có chắc chắn muốn tiếp tục lưu giá trị này không?");

                bool confirmed = FrmMain.ThrowQuestion(sb.ToString(), "Cảnh báo lưu biến môi trường");
                if (!confirmed)
                    return;
            }

            string joinedValue = EnvListService.Join(normalized);

            try
            {
                CustomEnvService.UpdateCoreValue(_key, joinedValue);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex) when (ex is System.Security.SecurityException or UnauthorizedAccessException)
            {
                FrmMain.ThrowError("Thiếu quyền ghi biến môi trường Machine (cần chạy với quyền Administrator)");
            }
            catch (KeyNotFoundException ex)
            {
                FrmMain.ThrowError(ex.Message);
                DialogResult = DialogResult.Abort;
                Close();
            }
            catch (Exception ex)
            {
                FrmMain.ThrowError(ex.Message);
            }
        }

        private void btn_Cancel_Click(object? sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
