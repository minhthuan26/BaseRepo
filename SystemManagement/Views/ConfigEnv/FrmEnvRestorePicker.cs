using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SystemManagement.Views.ConfigEnv
{
    /// <summary>
    /// Hộp thoại chọn một bản sao lưu để khôi phục cho biến môi trường cốt lõi.
    /// Giải thích: Hiển thị các bản sao lưu của đúng key được chọn theo thứ tự thời gian mới nhất trước.
    /// </summary>
    public class FrmEnvRestorePicker : Form
    {
        private readonly string _targetKey;
        private readonly DataGridView _dgvBackups;
        private readonly Button _btnSelect;
        private readonly Button _btnCancel;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public EnvBackupEntry? SelectedEntry { get; private set; }

        public FrmEnvRestorePicker(string key)
        {
            _targetKey = key ?? string.Empty;

            Text = $"Chọn bản sao lưu để khôi phục - {_targetKey}";
            ClientSize = new Size(760, 430);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 9F);

            var lblHeader = new Label
            {
                Text = $"Danh sách các bản sao lưu của biến '{_targetKey}' (mới nhất xếp trước):",
                Location = new Point(15, 15),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            _dgvBackups = new DataGridView
            {
                Location = new Point(15, 42),
                Size = new Size(730, 320),
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                BackgroundColor = Color.White,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                MultiSelect = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoGenerateColumns = false
            };

            var colTime = new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(BackupRowViewModel.CreatedAtDisplay),
                HeaderText = "Thời gian",
                Width = 140,
                ReadOnly = true
            };

            var colReason = new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(BackupRowViewModel.Reason),
                HeaderText = "Lý do sao lưu",
                Width = 120,
                ReadOnly = true
            };

            var colKind = new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(BackupRowViewModel.Kind),
                HeaderText = "Kiểu dữ liệu",
                Width = 110,
                ReadOnly = true
            };

            var colValue = new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(BackupRowViewModel.ValuePreview),
                HeaderText = "Giá trị (80 ký tự đầu)",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                ReadOnly = true
            };

            _dgvBackups.Columns.AddRange(colTime, colReason, colKind, colValue);
            _dgvBackups.SelectionChanged += DgvBackups_SelectionChanged;
            _dgvBackups.CellDoubleClick += DgvBackups_CellDoubleClick;

            _btnSelect = new Button
            {
                Text = "Chọn",
                Location = new Point(535, 380),
                Size = new Size(100, 34),
                BackColor = Color.LimeGreen,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false,
                Enabled = false
            };
            _btnSelect.Click += BtnSelect_Click;

            _btnCancel = new Button
            {
                Text = "Hủy",
                Location = new Point(645, 380),
                Size = new Size(100, 34),
                BackColor = Color.WhiteSmoke,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = true,
                DialogResult = DialogResult.Cancel
            };

            Controls.Add(lblHeader);
            Controls.Add(_dgvBackups);
            Controls.Add(_btnSelect);
            Controls.Add(_btnCancel);

            AcceptButton = _btnSelect;
            CancelButton = _btnCancel;

            LoadBackups();
        }

        private void LoadBackups()
        {
            var backups = EnvBackupService.ListBackups(_targetKey);
            var viewModels = backups.Select(x => new BackupRowViewModel
            {
                CreatedAtDisplay = x.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                Reason = x.Reason,
                Kind = x.Kind.ToString(),
                ValuePreview = x.Value.Length > 80 ? x.Value[..80] + "..." : x.Value,
                Entry = x
            }).ToList();

            _dgvBackups.DataSource = viewModels;
            UpdateButtonState();
        }

        private void UpdateButtonState()
        {
            _btnSelect.Enabled = _dgvBackups.CurrentRow?.DataBoundItem is BackupRowViewModel;
        }

        private void DgvBackups_SelectionChanged(object? sender, EventArgs e)
        {
            UpdateButtonState();
        }

        private void DgvBackups_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                ConfirmSelection();
            }
        }

        private void BtnSelect_Click(object? sender, EventArgs e)
        {
            ConfirmSelection();
        }

        private void ConfirmSelection()
        {
            if (_dgvBackups.CurrentRow?.DataBoundItem is BackupRowViewModel row)
            {
                SelectedEntry = row.Entry;
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private class BackupRowViewModel
        {
            public string CreatedAtDisplay { get; set; } = string.Empty;
            public string Reason { get; set; } = string.Empty;
            public string Kind { get; set; } = string.Empty;
            public string ValuePreview { get; set; } = string.Empty;
            public EnvBackupEntry Entry { get; set; } = null!;
        }
    }
}
