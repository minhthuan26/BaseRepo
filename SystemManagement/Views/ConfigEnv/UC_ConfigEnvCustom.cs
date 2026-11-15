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
        private List<CustomEnvItem> _systemItems = new();
        private List<CustomEnvItem> _userItems = new();
        private DataGridView _activeGrid = null!;
        private bool _isMasked = true;
        private bool _isBusy = false;

        public UC_ConfigEnvCustom()
        {
            InitializeComponent();
            _activeGrid = dgv_System;
            UpdateGridTitles();
        }

        private async void UC_ConfigEnvCustom_Load(object sender, EventArgs e)
        {
            await LoadDataAsync();
        }

        /// <summary>
        /// Nạp toàn bộ biến môi trường ở cả hai phạm vi Machine và User từ máy tính.
        /// Chạy tác vụ I/O registry ở luồng nền thông qua RunWithLoadingAsync để không làm đơ giao diện.
        /// </summary>
        public async Task LoadDataAsync()
        {
            try
            {
                await FrmMain.RunWithLoadingAsync(() => Task.Run(() =>
                {
                    _systemItems = CustomEnvService.GetVariables(EnvScope.Machine);
                    _userItems = CustomEnvService.GetVariables(EnvScope.User);
                }));

                ApplyFilter();
            }
            catch (Exception ex)
            {
                FrmMain.ThrowError($"Lỗi khi tải danh sách biến môi trường: {ex.Message}");
            }
        }

        /// <summary>
        /// Lọc danh sách biến môi trường theo từ khóa tìm kiếm và áp dụng mặt nạ che giá trị cho cả hai lưới.
        /// </summary>
        private void ApplyFilter()
        {
            string filter = txt_Search.Text.Trim();

            var filteredSys = string.IsNullOrEmpty(filter)
                ? _systemItems
                : _systemItems.Where(x => x.Key.Contains(filter, StringComparison.OrdinalIgnoreCase)).ToList();

            var filteredUser = string.IsNullOrEmpty(filter)
                ? _userItems
                : _userItems.Where(x => x.Key.Contains(filter, StringComparison.OrdinalIgnoreCase)).ToList();

            dgv_System.DataSource = filteredSys.Select(x => new EnvRowViewModel
            {
                Key = x.Key,
                DisplayValue = _isMasked ? (string.IsNullOrEmpty(x.Value) ? string.Empty : "••••••••") : x.Value,
                TypeDescription = GetTypeDescription(x),
                Item = x
            }).ToList();

            dgv_User.DataSource = filteredUser.Select(x => new EnvRowViewModel
            {
                Key = x.Key,
                DisplayValue = _isMasked ? (string.IsNullOrEmpty(x.Value) ? string.Empty : "••••••••") : x.Value,
                TypeDescription = GetTypeDescription(x),
                Item = x
            }).ToList();

            // Gán DataSource tự chọn dòng đầu ở cả hai lưới; chỉ giữ lại lưới đang hoạt động
            ClearInactiveSelection();
        }

        /// <summary>
        /// Xác định mô tả loại biến theo đặc tả: Cốt lõi, BaseRepo hoặc Khác.
        /// </summary>
        private static string GetTypeDescription(CustomEnvItem item)
        {
            if (item.IsProtected)
                return "Cốt lõi";
            if (item.Key.StartsWith("BaseRepo__", StringComparison.OrdinalIgnoreCase))
                return "BaseRepo";
            return "Khác";
        }

        /// <summary>
        /// Cập nhật nhãn tiêu đề của hai lưới để hiển thị trực quan lưới nào đang nhận focus (thao tác).
        /// </summary>
        private void UpdateGridTitles()
        {
            string sysPrefix = (_activeGrid == dgv_System) ? "▶ " : "   ";
            string userPrefix = (_activeGrid == dgv_User) ? "▶ " : "   ";
            lbl_SystemTitle.Text = $"{sysPrefix}Biến môi trường Hệ thống (System / Machine)";
            lbl_UserTitle.Text = $"{userPrefix}Biến môi trường Người dùng ({Environment.UserDomainName}\\{Environment.UserName})";
            lbl_SystemTitle.Font = new Font(lbl_SystemTitle.Font, _activeGrid == dgv_System ? FontStyle.Bold : FontStyle.Regular);
            lbl_UserTitle.Font = new Font(lbl_UserTitle.Font, _activeGrid == dgv_User ? FontStyle.Bold : FontStyle.Regular);
        }

        /// <summary>
        /// Bỏ chọn dòng ở lưới không hoạt động để chỉ còn đúng một dòng được chọn trên toàn tab.
        /// Giải thích: DataGridView vẫn tô xanh dòng chọn khi mất focus, và gán DataSource tự chọn dòng đầu,
        /// nên hai lưới cùng có dòng xanh khiến người dùng không biết Sửa/Xóa/Khôi phục sẽ tác động dòng nào.
        /// Các nút trên thanh công cụ không đổi _activeGrid nên lưới hoạt động giữ nguyên dòng chọn khi bấm nút.
        /// </summary>
        private void ClearInactiveSelection()
        {
            var inactive = (_activeGrid == dgv_System) ? dgv_User : dgv_System;
            inactive.CurrentCell = null;
            inactive.ClearSelection();
        }

        private void dgv_System_Enter(object? sender, EventArgs e)
        {
            _activeGrid = dgv_System;
            UpdateGridTitles();
            ClearInactiveSelection();
        }

        private void dgv_User_Enter(object? sender, EventArgs e)
        {
            _activeGrid = dgv_User;
            UpdateGridTitles();
            ClearInactiveSelection();
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
            if (_isBusy) return;
            _isBusy = true;
            try
            {
                await LoadDataAsync();
            }
            finally
            {
                _isBusy = false;
            }
        }

        private async void btn_Add_Click(object sender, EventArgs e)
        {
            if (_isBusy) return;
            _isBusy = true;
            try
            {
                // Thêm biến vào phạm vi của lưới đang được chọn
                EnvScope scope = (_activeGrid == dgv_System) ? EnvScope.Machine : EnvScope.User;
                using var editor = new FrmCustomEnvEditor(isEditMode: false, key: string.Empty, currentValue: string.Empty, scope: scope);
                if (editor.ShowDialog(this) == DialogResult.OK)
                {
                    await LoadDataAsync();
                }
            }
            finally
            {
                _isBusy = false;
            }
        }

        private async void btn_Edit_Click(object? sender, EventArgs e)
        {
            if (_isBusy) return;
            _isBusy = true;
            try
            {
                var currentGrid = _activeGrid ?? dgv_System;
                var selected = (currentGrid.CurrentRow?.DataBoundItem as EnvRowViewModel)?.Item;
                if (selected == null)
                {
                    FrmMain.ThrowWarning("Vui lòng chọn một biến môi trường để sửa.");
                    return;
                }

                EnvScope scope = (currentGrid == dgv_System) ? EnvScope.Machine : EnvScope.User;

                // Xử lý sửa biến hệ thống cốt lõi
                if (selected.IsProtected)
                {
                    if (CustomEnvService.RequiresElevation(scope) && !CustomEnvService.IsElevated())
                    {
                        FrmMain.ThrowWarning("Cần chạy ứng dụng với quyền Administrator để sửa biến hệ thống cốt lõi.");
                        return;
                    }

                    string confirmMsg = $"CẢNH BÁO NGUY HIỂM: Biến '{selected.Key}' là biến hệ thống cốt lõi của Windows ({scope}).\n" +
                                        $"Thay đổi giá trị biến này có thể ảnh hưởng nghiêm trọng đến toàn bộ hệ điều hành và các ứng dụng đang chạy.\n\n" +
                                        $"Để tiếp tục, vui lòng gõ lại chính xác tên biến để xác nhận:";

                    using var confirmForm = new FrmEnvTypeKeyConfirm(selected.Key, "Xác nhận sửa biến hệ thống cốt lõi", confirmMsg);
                    if (confirmForm.ShowDialog(this) != DialogResult.OK)
                        return;

                    string rawValue;
                    try
                    {
                        var raw = EnvRegistryService.ReadRaw(scope, selected.Key);
                        if (raw == null)
                            throw new KeyNotFoundException($"Biến '{selected.Key}' không còn tồn tại trên máy.");
                        rawValue = raw.Value;
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
                        using var listEditor = new FrmEnvListEditor(selected.Key, rawValue, scope);
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

                    using var coreEditor = new FrmCustomEnvEditor(isEditMode: true, key: selected.Key, currentValue: rawValue, scope: scope, isCoreEdit: true);
                    var coreRes = coreEditor.ShowDialog(this);
                    if (coreRes == DialogResult.OK || coreRes == DialogResult.Abort)
                    {
                        await LoadDataAsync();
                    }
                    return;
                }

                // Cảnh báo thêm nếu là biến thường không thuộc tiền tố BaseRepo__
                if (selected.NeedsExtraWarning)
                {
                    bool confirmed = FrmMain.ThrowQuestion(
                        $"CẢNH BÁO: Biến '{selected.Key}' không có tiền tố 'BaseRepo__'.\nViệc thay đổi biến hệ thống có thể ảnh hưởng đến Windows hoặc các ứng dụng khác.\n\nBạn có chắc chắn muốn sửa biến '{selected.Key}' không?",
                        "Cảnh báo thay đổi biến hệ thống");
                    if (!confirmed)
                        return;
                }

                using var editor = new FrmCustomEnvEditor(isEditMode: true, key: selected.Key, currentValue: selected.Value, scope: scope);
                var res = editor.ShowDialog(this);
                if (res == DialogResult.OK || res == DialogResult.Abort)
                {
                    await LoadDataAsync();
                }
            }
            finally
            {
                _isBusy = false;
            }
        }

        private async void btn_Delete_Click(object? sender, EventArgs e)
        {
            if (_isBusy) return;
            _isBusy = true;
            try
            {
                var currentGrid = _activeGrid ?? dgv_System;
                var selected = (currentGrid.CurrentRow?.DataBoundItem as EnvRowViewModel)?.Item;
                if (selected == null)
                {
                    FrmMain.ThrowWarning("Vui lòng chọn một biến môi trường để xóa.");
                    return;
                }

                EnvScope scope = (currentGrid == dgv_System) ? EnvScope.Machine : EnvScope.User;

                if (selected.IsProtected)
                {
                    FrmMain.ThrowWarning($"Biến '{selected.Key}' là biến hệ thống cốt lõi, không được phép xóa.");
                    return;
                }

                if (CustomEnvService.RequiresElevation(scope) && !CustomEnvService.IsElevated())
                {
                    FrmMain.ThrowWarning("Cần chạy ứng dụng với quyền Administrator để xóa biến môi trường Machine.");
                    return;
                }

                string message = selected.NeedsExtraWarning
                    ? $"CẢNH BÁO: Biến '{selected.Key}' không có tiền tố 'BaseRepo__'.\nViệc xóa biến hệ thống có thể gây lỗi hệ điều hành Windows hoặc các ứng dụng khác.\n(Hệ thống sẽ tự động sao lưu trước khi xóa).\n\nBạn có chắc chắn muốn xóa biến '{selected.Key}' không?"
                    : $"Bạn có chắc chắn muốn xóa biến môi trường '{selected.Key}' không?\n(Hệ thống sẽ tự động sao lưu trước khi xóa).";

                bool confirmed = FrmMain.ThrowQuestion(message, "Xác nhận xóa biến môi trường");
                if (!confirmed)
                    return;

                string deleteKey = selected.Key;
                try
                {
                    // Chạy xóa ở luồng nền dưới loader để tránh đơ UI
                    await FrmMain.RunWithLoadingAsync(() => Task.Run(() =>
                    {
                        CustomEnvService.DeleteVariable(scope, deleteKey);
                    }));

                    FrmMain.ThrowInfo($"Đã xóa biến '{deleteKey}' thành công.\nVui lòng mở lại terminal hoặc ứng dụng để nhận giá trị mới.");
                    await LoadDataAsync();
                }
                catch (Exception ex) when (ex is SecurityException or UnauthorizedAccessException)
                {
                    if (scope == EnvScope.Machine)
                        FrmMain.ThrowError("Thiếu quyền ghi biến môi trường Machine (cần chạy với quyền Administrator)");
                    else
                        FrmMain.ThrowError("Thiếu quyền ghi biến môi trường User");
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
            finally
            {
                _isBusy = false;
            }
        }

        private async void btn_Restore_Click(object? sender, EventArgs e)
        {
            if (_isBusy) return;
            _isBusy = true;
            try
            {
                EnvScope scope = (_activeGrid == dgv_System) ? EnvScope.Machine : EnvScope.User;

                if (CustomEnvService.RequiresElevation(scope) && !CustomEnvService.IsElevated())
                {
                    FrmMain.ThrowWarning("Cần chạy ứng dụng với quyền Administrator để khôi phục biến môi trường Machine.");
                    return;
                }

                var backups = EnvBackupService.ListBackups(scope);
                if (backups.Count == 0)
                {
                    FrmMain.ThrowInfo($"Phạm vi {scope} chưa có bản sao lưu nào để khôi phục.");
                    return;
                }

                using var picker = new FrmEnvRestorePicker(scope);
                if (picker.ShowDialog(this) != DialogResult.OK || picker.SelectedEntry == null)
                {
                    return;
                }

                var selectedEntry = picker.SelectedEntry;

                string confirmMsg = $"CẢNH BÁO NGUY HIỂM: Khôi phục biến '{selectedEntry.Key}' ({selectedEntry.Scope}) về bản sao lưu thời điểm {selectedEntry.CreatedAt:yyyy-MM-dd HH:mm:ss}.\n" +
                                    $"Thao tác này sẽ ghi đè giá trị biến môi trường của hệ thống Windows.\n\n" +
                                    $"Để tiếp tục, vui lòng gõ lại chính xác tên biến để xác nhận:";

                using var confirmForm = new FrmEnvTypeKeyConfirm(selectedEntry.Key, "Xác nhận khôi phục biến môi trường", confirmMsg);
                if (confirmForm.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    // Chạy khôi phục ở luồng nền dưới loader để tránh đơ UI
                    await FrmMain.RunWithLoadingAsync(() => Task.Run(() =>
                    {
                        CustomEnvService.RestoreVariable(selectedEntry);
                    }));

                    FrmMain.ThrowInfo($"Đã khôi phục biến '{selectedEntry.Key}' thành công.\nVui lòng mở lại terminal hoặc ứng dụng để nhận giá trị mới.");
                    await LoadDataAsync();
                }
                catch (Exception ex) when (ex is SecurityException or UnauthorizedAccessException)
                {
                    if (scope == EnvScope.Machine)
                        FrmMain.ThrowError("Thiếu quyền ghi biến môi trường Machine (cần chạy với quyền Administrator)");
                    else
                        FrmMain.ThrowError("Thiếu quyền ghi biến môi trường User");
                }
                catch (Exception ex)
                {
                    FrmMain.ThrowError(ex.Message);
                }
            }
            finally
            {
                _isBusy = false;
            }
        }

        private void dgv_System_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                btn_Edit_Click(sender, EventArgs.Empty);
            }
        }

        private void dgv_User_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                btn_Edit_Click(sender, EventArgs.Empty);
            }
        }

        private void dgv_System_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            FormatGridRow(dgv_System, col_SysType.Index, e);
        }

        private void dgv_User_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            FormatGridRow(dgv_User, col_UserType.Index, e);
        }

        private static void FormatGridRow(DataGridView grid, int typeColIndex, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < grid.Rows.Count)
            {
                if (grid.Rows[e.RowIndex].DataBoundItem is EnvRowViewModel row && row.Item.IsProtected)
                {
                    e.CellStyle.ForeColor = Color.DimGray;
                    if (e.ColumnIndex == typeColIndex)
                    {
                        e.CellStyle.Font = new Font(grid.Font, FontStyle.Italic);
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
