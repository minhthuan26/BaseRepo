using CommonLibs.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using System.Windows.Forms;
using SystemManagement.Views;

namespace SystemManagement.Views.ConfigEnv
{
    public partial class UC_ConfigEnvCustom : UserControl
    {
        private List<CustomEnvItem> _allItems = new();
        private bool _isMasked = true;

        public UC_ConfigEnvCustom()
        {
            InitializeComponent();
        }

        private async void UC_ConfigEnvCustom_Load(object sender, EventArgs e)
        {
            await LoadDataAsync();
        }

        public async Task LoadDataAsync()
        {
            try
            {
                var defaultKeys = FrmMain.SysConfig?.EnvKeys ?? Enumerable.Empty<string>();
                await FrmMain.RunWithLoadingAsync(() =>
                {
                    _allItems = CustomEnvService.GetCustomEnvs(defaultKeys);
                    return Task.CompletedTask;
                });

                ApplyFilter();
            }
            catch (Exception ex)
            {
                FrmMain.ThrowError($"Lỗi khi tải danh sách biến môi trường: {ex.Message}");
            }
        }

        private void ApplyFilter()
        {
            string filter = txt_Search.Text.Trim();
            var filtered = string.IsNullOrEmpty(filter)
                ? _allItems
                : _allItems.Where(x => x.Key.Contains(filter, StringComparison.OrdinalIgnoreCase)).ToList();

            var displayList = filtered.Select(x => new EnvRowViewModel
            {
                Key = x.Key,
                DisplayValue = _isMasked ? (string.IsNullOrEmpty(x.Value) ? string.Empty : "••••••••") : x.Value,
                TypeDescription = x.IsProtected ? "Hệ thống (chỉ xem)" : (x.NeedsExtraWarning ? "Hệ thống" : "BaseRepo"),
                Item = x
            }).ToList();

            dgv_Envs.DataSource = displayList;
        }

        private void txt_Search_TextChanged(object sender, EventArgs e)
        {
            ApplyFilter();
        }

        private void btn_ToggleMask_Click(object sender, EventArgs e)
        {
            _isMasked = !_isMasked;
            btn_ToggleMask.Text = _isMasked ? "Hiện giá trị" : "Ẩn giá trị";
            ApplyFilter();
        }

        private async void btn_Refresh_Click(object sender, EventArgs e)
        {
            await LoadDataAsync();
        }

        private async void btn_Add_Click(object sender, EventArgs e)
        {
            using var editor = new FrmCustomEnvEditor(isEditMode: false, key: string.Empty, currentValue: string.Empty);
            if (editor.ShowDialog(this) == DialogResult.OK)
            {
                await LoadDataAsync();
            }
        }

        private async void btn_Restore_Click(object? sender, EventArgs e)
        {
            var selected = (dgv_Envs.CurrentRow?.DataBoundItem as EnvRowViewModel)?.Item;
            if (selected == null)
            {
                FrmMain.ThrowWarning("Vui lòng chọn một biến môi trường để khôi phục.");
                return;
            }

            if (!selected.IsProtected)
            {
                FrmMain.ThrowWarning($"Biến '{selected.Key}' không phải là biến hệ thống cốt lõi. Chức năng khôi phục chỉ áp dụng cho biến cốt lõi.");
                return;
            }

            if (!CustomEnvService.IsElevated())
            {
                FrmMain.ThrowWarning("Cần chạy ứng dụng với quyền Administrator để khôi phục biến hệ thống cốt lõi.");
                return;
            }

            var backups = EnvBackupService.ListBackups(selected.Key);
            if (backups.Count == 0)
            {
                FrmMain.ThrowInfo($"Biến '{selected.Key}' chưa có bản sao lưu nào để khôi phục.");
                return;
            }

            using var picker = new FrmEnvRestorePicker(selected.Key);
            if (picker.ShowDialog(this) != DialogResult.OK || picker.SelectedEntry == null)
            {
                return;
            }

            var selectedEntry = picker.SelectedEntry;

            string confirmMsg = $"CẢNH BÁO NGUY HIỂM: Khôi phục biến hệ thống cốt lõi '{selected.Key}' về bản sao lưu thời điểm {selectedEntry.CreatedAt:yyyy-MM-dd HH:mm:ss}.\n" +
                                $"Thao tác này sẽ ghi đè giá trị biến môi trường của hệ thống Windows.\n\n" +
                                $"Để tiếp tục, vui lòng gõ lại chính xác tên biến để xác nhận:";

            using var confirmForm = new FrmEnvTypeKeyConfirm(selected.Key, "Xác nhận khôi phục biến hệ thống cốt lõi", confirmMsg);
            if (confirmForm.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            try
            {
                // Chạy khôi phục ở luồng nền dưới loader để tránh đơ UI
                await FrmMain.RunWithLoadingAsync(() => Task.Run(() =>
                {
                    CustomEnvService.RestoreCoreValue(selectedEntry);
                }));

                FrmMain.ThrowInfo($"Đã khôi phục biến '{selected.Key}' thành công.\nVui lòng mở lại terminal hoặc ứng dụng để nhận giá trị mới.");
                await LoadDataAsync();
            }
            catch (Exception ex) when (ex is SecurityException or UnauthorizedAccessException)
            {
                FrmMain.ThrowError("Thiếu quyền ghi biến môi trường Machine (cần chạy với quyền Administrator)");
            }
            catch (Exception ex)
            {
                FrmMain.ThrowError(ex.Message);
            }
        }

        private async void btn_Edit_Click(object sender, EventArgs e)
        {
            var selected = (dgv_Envs.CurrentRow?.DataBoundItem as EnvRowViewModel)?.Item;
            if (selected == null)
            {
                FrmMain.ThrowWarning("Vui lòng chọn một biến môi trường để sửa.");
                return;
            }

            if (selected.IsProtected)
            {
                if (!CustomEnvService.IsElevated())
                {
                    FrmMain.ThrowWarning("Cần chạy ứng dụng với quyền Administrator để sửa biến hệ thống cốt lõi.");
                    return;
                }

                string confirmMsg = $"CẢNH BÁO NGUY HIỂM: Biến '{selected.Key}' là biến hệ thống cốt lõi của Windows.\n" +
                                    $"Thay đổi giá trị biến này có thể ảnh hưởng nghiêm trọng đến toàn bộ hệ điều hành và các ứng dụng đang chạy.\n\n" +
                                    $"Để tiếp tục, vui lòng gõ lại chính xác tên biến để xác nhận:";

                using var confirmForm = new FrmEnvTypeKeyConfirm(selected.Key, "Xác nhận sửa biến hệ thống cốt lõi", confirmMsg);
                if (confirmForm.ShowDialog(this) != DialogResult.OK)
                    return;

                string rawValue;
                try
                {
                    rawValue = CustomEnvService.GetCoreRawValue(selected.Key);
                }
                catch (KeyNotFoundException ex)
                {
                    FrmMain.ThrowError(ex.Message);
                    await LoadDataAsync();
                    return;
                }
                catch (Exception ex)
                {
                    FrmMain.ThrowError(ex.Message);
                    return;
                }

                if (CustomEnvService.IsListKey(selected.Key))
                {
                    using var listEditor = new FrmEnvListEditor(selected.Key, rawValue);
                    var listRes = listEditor.ShowDialog(this);
                    if (listRes == DialogResult.OK)
                    {
                        FrmMain.ThrowInfo($"Cập nhật biến '{selected.Key}' thành công.\nVui lòng mở lại terminal hoặc ứng dụng để nhận giá trị mới.");
                        await LoadDataAsync();
                    }
                    else if (listRes == DialogResult.Abort)
                    {
                        await LoadDataAsync();
                    }
                    return;
                }

                using var coreEditor = new FrmCustomEnvEditor(isEditMode: true, key: selected.Key, currentValue: rawValue, isCoreEdit: true);
                var coreRes = coreEditor.ShowDialog(this);
                if (coreRes == DialogResult.OK || coreRes == DialogResult.Abort)
                {
                    await LoadDataAsync();
                }
                return;
            }

            if (selected.NeedsExtraWarning)
            {
                bool confirmed = FrmMain.ThrowQuestion(
                    $"CẢNH BÁO: Biến '{selected.Key}' không có tiền tố 'BaseRepo__'.\nViệc thay đổi biến hệ thống có thể ảnh hưởng đến Windows hoặc các ứng dụng khác.\n\nBạn có chắc chắn muốn sửa biến '{selected.Key}' không?",
                    "Cảnh báo thay đổi biến hệ thống");
                if (!confirmed)
                    return;
            }

            using var editor = new FrmCustomEnvEditor(isEditMode: true, key: selected.Key, currentValue: selected.Value);
            var res = editor.ShowDialog(this);
            if (res == DialogResult.OK || res == DialogResult.Abort)
            {
                await LoadDataAsync();
            }
        }

        private async void btn_Delete_Click(object sender, EventArgs e)
        {
            var selected = (dgv_Envs.CurrentRow?.DataBoundItem as EnvRowViewModel)?.Item;
            if (selected == null)
            {
                FrmMain.ThrowWarning("Vui lòng chọn một biến môi trường để xóa.");
                return;
            }

            if (selected.IsProtected)
            {
                FrmMain.ThrowWarning($"Biến '{selected.Key}' là biến hệ thống cốt lõi, không được phép xóa.");
                return;
            }

            string message = selected.NeedsExtraWarning
                ? $"CẢNH BÁO: Biến '{selected.Key}' không có tiền tố 'BaseRepo__'.\nViệc xóa biến hệ thống có thể gây lỗi hệ điều hành Windows hoặc các ứng dụng khác.\n\nBạn có chắc chắn muốn xóa biến '{selected.Key}' không?"
                : $"Bạn có chắc chắn muốn xóa biến môi trường '{selected.Key}' không?";

            bool confirmed = FrmMain.ThrowQuestion(message, "Xác nhận xóa biến môi trường");
            if (!confirmed)
                return;

            string deleteKey = selected.Key;
            try
            {
                // Chạy xóa ở luồng nền dưới loader để tránh đơ UI
                await FrmMain.RunWithLoadingAsync(() => Task.Run(() =>
                {
                    CustomEnvService.Delete(deleteKey);
                }));

                FrmMain.ThrowInfo($"Đã xóa biến '{deleteKey}' thành công.");
                await LoadDataAsync();
            }
            catch (Exception ex) when (ex is SecurityException or UnauthorizedAccessException)
            {
                FrmMain.ThrowError("Thiếu quyền ghi biến môi trường Machine (cần chạy với quyền Administrator)");
            }
            catch (KeyNotFoundException ex)
            {
                FrmMain.ThrowError(ex.Message);
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                FrmMain.ThrowError(ex.Message);
            }
        }

        private void dgv_Envs_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                btn_Edit_Click(sender, EventArgs.Empty);
            }
        }

        private void dgv_Envs_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dgv_Envs.Rows.Count)
            {
                if (dgv_Envs.Rows[e.RowIndex].DataBoundItem is EnvRowViewModel row && row.Item.IsProtected)
                {
                    e.CellStyle.ForeColor = Color.DimGray;
                    if (e.ColumnIndex == col_Type.Index)
                    {
                        e.CellStyle.Font = new Font(dgv_Envs.Font, FontStyle.Italic);
                    }
                }
            }
        }

        private class EnvRowViewModel
        {
            public string Key { get; set; } = string.Empty;
            public string DisplayValue { get; set; } = string.Empty;
            public string TypeDescription { get; set; } = string.Empty;
            public CustomEnvItem Item { get; set; } = null!;
        }
    }
}
