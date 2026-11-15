using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SystemManagement.Views.Base;

namespace SystemManagement.Views.ConfigEnv
{
    /// <summary>
    /// Hộp thoại chọn một bản sao lưu để khôi phục cho biến môi trường theo phạm vi.
    /// Giải thích: Hiển thị các bản sao lưu của toàn bộ key thuộc phạm vi được chọn theo thứ tự thời gian mới nhất trước.
    /// </summary>
    public partial class FrmEnvRestorePicker : Frm_Base
    {
        private readonly EnvScope _scope;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public EnvBackupEntry? SelectedEntry { get; private set; }

        public FrmEnvRestorePicker(EnvScope scope)
        {
            _scope = scope;

            InitializeComponent();

            Text = $"Chọn bản sao lưu để khôi phục - {_scope}";
            lbl_Header.Text = $"Danh sách các bản sao lưu thuộc phạm vi {_scope} (mới nhất xếp trước):";

            LoadBackups();
        }

        private void LoadBackups()
        {
            var backups = EnvBackupService.ListBackups(_scope);
            var viewModels = backups.Select(x => new BackupRowViewModel
            {
                CreatedAtDisplay = x.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                Key = x.Key,
                Reason = x.Reason,
                Kind = x.Kind.ToString(),
                ValuePreview = x.Value.Length > 80 ? x.Value[..80] + "..." : x.Value,
                Entry = x
            }).ToList();

            dgv_Backups.DataSource = viewModels;
            UpdateButtonState();
        }

        private void UpdateButtonState()
        {
            btn_Select.Enabled = dgv_Backups.CurrentRow?.DataBoundItem is BackupRowViewModel;
        }

        private void dgv_Backups_SelectionChanged(object? sender, EventArgs e)
        {
            UpdateButtonState();
        }

        private void dgv_Backups_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                ConfirmSelection();
            }
        }

        private void btn_Select_Click(object? sender, EventArgs e)
        {
            ConfirmSelection();
        }

        private void ConfirmSelection()
        {
            if (dgv_Backups.CurrentRow?.DataBoundItem is BackupRowViewModel row)
            {
                SelectedEntry = row.Entry;
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private class BackupRowViewModel
        {
            public string CreatedAtDisplay { get; set; } = string.Empty;
            public string Key { get; set; } = string.Empty;
            public string Reason { get; set; } = string.Empty;
            public string Kind { get; set; } = string.Empty;
            public string ValuePreview { get; set; } = string.Empty;
            public EnvBackupEntry Entry { get; set; } = null!;
        }
    }
}
