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
    public class FrmEnvListEditor : Form
    {
        private readonly string _key;
        private readonly ListBox _lstItems;
        private readonly TextBox _txtInput;
        private readonly Button _btnAdd;
        private readonly Button _btnUpdate;
        private readonly Button _btnDelete;
        private readonly Button _btnUp;
        private readonly Button _btnDown;
        private readonly Panel _pnlWarning;
        private readonly Label _lblWarning;
        private readonly Button _btnSave;
        private readonly Button _btnCancel;

        public FrmEnvListEditor(string key, string rawValue)
        {
            _key = key ?? string.Empty;

            Text = $"Sửa danh sách biến '{_key}'";
            ClientSize = new Size(720, 500);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 9F);

            // Tiêu đề & hướng dẫn
            var lblTitle = new Label
            {
                Text = $"Chỉnh sửa danh sách các mục của biến: {_key}",
                Location = new Point(20, 15),
                Size = new Size(680, 24),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };

            var lblDesc = new Label
            {
                Text = "Mỗi mục phân tách bởi dấu ';'. Ký tự thô dạng %...% được giữ nguyên khi lưu.",
                Location = new Point(20, 40),
                Size = new Size(680, 20),
                ForeColor = Color.DimGray
            };

            // Danh sách các mục
            _lstItems = new ListBox
            {
                Location = new Point(20, 68),
                Size = new Size(570, 240),
                IntegralHeight = false,
                HorizontalScrollbar = true,
                Font = new Font("Segoe UI", 9F)
            };
            _lstItems.SelectedIndexChanged += LstItems_SelectedIndexChanged;

            // Nút di chuyển và xóa
            _btnUp = new Button
            {
                Text = "▲ Lên",
                Location = new Point(600, 68),
                Size = new Size(100, 32),
                BackColor = Color.WhiteSmoke,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = true
            };
            _btnUp.Click += BtnUp_Click;

            _btnDown = new Button
            {
                Text = "▼ Xuống",
                Location = new Point(600, 108),
                Size = new Size(100, 32),
                BackColor = Color.WhiteSmoke,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = true
            };
            _btnDown.Click += BtnDown_Click;

            _btnDelete = new Button
            {
                Text = "Xóa",
                Location = new Point(600, 148),
                Size = new Size(100, 32),
                BackColor = Color.WhiteSmoke,
                ForeColor = Color.DarkRed,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = true
            };
            _btnDelete.Click += BtnDelete_Click;

            // Ô nhập và nút Thêm/Cập nhật
            var lblInput = new Label
            {
                Text = "Mục nhập:",
                Location = new Point(20, 322),
                Size = new Size(75, 23),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            _txtInput = new TextBox
            {
                Location = new Point(100, 320),
                Size = new Size(400, 23)
            };
            _txtInput.TextChanged += TxtInput_TextChanged;

            _btnAdd = new Button
            {
                Text = "Thêm",
                Location = new Point(510, 318),
                Size = new Size(90, 27),
                BackColor = Color.LightSkyBlue,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            _btnAdd.Click += BtnAdd_Click;

            _btnUpdate = new Button
            {
                Text = "Cập nhật",
                Location = new Point(610, 318),
                Size = new Size(90, 27),
                BackColor = Color.LightYellow,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            _btnUpdate.Click += BtnUpdate_Click;

            // Vùng cảnh báo
            _pnlWarning = new Panel
            {
                Location = new Point(20, 355),
                Size = new Size(680, 80),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(240, 253, 244)
            };

            _lblWarning = new Label
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Color.DarkGreen,
                Padding = new Padding(6),
                TextAlign = ContentAlignment.TopLeft
            };
            _pnlWarning.Controls.Add(_lblWarning);

            // Nút Lưu và Hủy
            _btnSave = new Button
            {
                Text = "Lưu",
                Location = new Point(500, 450),
                Size = new Size(95, 34),
                BackColor = Color.LimeGreen,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            _btnSave.Click += BtnSave_Click;

            _btnCancel = new Button
            {
                Text = "Hủy",
                Location = new Point(605, 450),
                Size = new Size(95, 34),
                BackColor = Color.WhiteSmoke,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = true,
                DialogResult = DialogResult.Cancel
            };
            _btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            CancelButton = _btnCancel;

            // Đưa các control lên Form
            Controls.Add(lblTitle);
            Controls.Add(lblDesc);
            Controls.Add(_lstItems);
            Controls.Add(_btnUp);
            Controls.Add(_btnDown);
            Controls.Add(_btnDelete);
            Controls.Add(lblInput);
            Controls.Add(_txtInput);
            Controls.Add(_btnAdd);
            Controls.Add(_btnUpdate);
            Controls.Add(_pnlWarning);
            Controls.Add(_btnSave);
            Controls.Add(_btnCancel);

            // Nạp dữ liệu ban đầu
            LoadItems(rawValue);
        }

        private void LoadItems(string rawValue)
        {
            var items = EnvListService.Split(rawValue);
            _lstItems.Items.Clear();
            foreach (var item in items)
            {
                _lstItems.Items.Add(item);
            }

            if (_lstItems.Items.Count > 0)
            {
                _lstItems.SelectedIndex = 0;
            }

            UpdateButtonStates();
            UpdateWarningDisplay();
        }

        private void LstItems_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_lstItems.SelectedIndex >= 0)
            {
                _txtInput.Text = _lstItems.SelectedItem?.ToString() ?? string.Empty;
            }
            UpdateButtonStates();
        }

        private void TxtInput_TextChanged(object? sender, EventArgs e)
        {
            UpdateButtonStates();
        }

        private void UpdateButtonStates()
        {
            int sel = _lstItems.SelectedIndex;
            bool hasText = !string.IsNullOrWhiteSpace(_txtInput.Text);

            _btnUp.Enabled = sel > 0;
            _btnDown.Enabled = sel >= 0 && sel < _lstItems.Items.Count - 1;
            _btnDelete.Enabled = sel >= 0;
            _btnUpdate.Enabled = sel >= 0 && hasText;
            _btnAdd.Enabled = hasText;
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            string text = _txtInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(text))
            {
                FrmMain.ThrowWarning("Vui lòng nhập nội dung mục cần thêm.");
                return;
            }

            _lstItems.Items.Add(text);
            _lstItems.SelectedIndex = _lstItems.Items.Count - 1;
            _txtInput.Clear();

            UpdateButtonStates();
            UpdateWarningDisplay();
        }

        private void BtnUpdate_Click(object? sender, EventArgs e)
        {
            int sel = _lstItems.SelectedIndex;
            if (sel < 0)
            {
                FrmMain.ThrowWarning("Vui lòng chọn một mục trong danh sách để cập nhật.");
                return;
            }

            string text = _txtInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(text))
            {
                FrmMain.ThrowWarning("Vui lòng nhập nội dung mục cần cập nhật.");
                return;
            }

            _lstItems.Items[sel] = text;
            _lstItems.SelectedIndex = sel;

            UpdateButtonStates();
            UpdateWarningDisplay();
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            int sel = _lstItems.SelectedIndex;
            if (sel < 0)
                return;

            _lstItems.Items.RemoveAt(sel);
            if (_lstItems.Items.Count > 0)
            {
                _lstItems.SelectedIndex = Math.Min(sel, _lstItems.Items.Count - 1);
            }
            else
            {
                _txtInput.Clear();
            }

            UpdateButtonStates();
            UpdateWarningDisplay();
        }

        private void BtnUp_Click(object? sender, EventArgs e)
        {
            int sel = _lstItems.SelectedIndex;
            if (sel <= 0)
                return;

            var item = _lstItems.Items[sel];
            _lstItems.Items.RemoveAt(sel);
            _lstItems.Items.Insert(sel - 1, item);
            _lstItems.SelectedIndex = sel - 1;

            UpdateButtonStates();
            UpdateWarningDisplay();
        }

        private void BtnDown_Click(object? sender, EventArgs e)
        {
            int sel = _lstItems.SelectedIndex;
            if (sel < 0 || sel >= _lstItems.Items.Count - 1)
                return;

            var item = _lstItems.Items[sel];
            _lstItems.Items.RemoveAt(sel);
            _lstItems.Items.Insert(sel + 1, item);
            _lstItems.SelectedIndex = sel + 1;

            UpdateButtonStates();
            UpdateWarningDisplay();
        }

        private void UpdateWarningDisplay()
        {
            var currentItems = _lstItems.Items.Cast<object>().Select(x => x?.ToString() ?? string.Empty).ToList();
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

            _lblWarning.Text = string.Join("\n", notes);
            if (totalLength > 2047)
            {
                _pnlWarning.BackColor = Color.FromArgb(254, 242, 242);
                _lblWarning.ForeColor = Color.DarkRed;
            }
            else if (duplicates.Count > 0 || missingPaths.Count > 0)
            {
                _pnlWarning.BackColor = Color.FromArgb(255, 251, 235);
                _lblWarning.ForeColor = Color.DarkGoldenrod;
            }
            else
            {
                _pnlWarning.BackColor = Color.FromArgb(240, 253, 244);
                _lblWarning.ForeColor = Color.DarkGreen;
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            var currentItems = _lstItems.Items.Cast<object>().Select(x => x?.ToString() ?? string.Empty).ToList();
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
    }
}
