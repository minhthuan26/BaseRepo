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
                FrmMain.ThrowWarning($"Biến '{selected.Key}' là biến hệ thống cốt lõi, không được phép sửa.");
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

            try
            {
                await FrmMain.RunWithLoadingAsync(() =>
                {
                    CustomEnvService.Delete(selected.Key);
                    return Task.CompletedTask;
                });

                FrmMain.ThrowInfo($"Đã xóa biến '{selected.Key}' thành công.");
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

    internal class FrmCustomEnvEditor : Form
    {
        private readonly bool _isEditMode;
        private readonly TextBox _txtKey;
        private readonly TextBox _txtValue;
        private readonly ComboBox _cboGenType;
        private readonly Label _lblParam;
        private readonly ComboBox _cboAesBits;
        private readonly TextBox _txtGuidPrefix;
        private readonly NumericUpDown _nudRandomLength;
        private readonly Label _lblRsaNote;

        public FrmCustomEnvEditor(bool isEditMode, string key, string currentValue)
        {
            _isEditMode = isEditMode;

            Text = _isEditMode ? "Sửa giá trị biến môi trường" : "Thêm biến môi trường tùy chỉnh";
            ClientSize = new Size(540, 460);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 9F);

            // Key
            var lblKey = new Label
            {
                Text = "Tên biến (Key):",
                Location = new Point(20, 20),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            _txtKey = new TextBox
            {
                Location = new Point(130, 17),
                Size = new Size(380, 23),
                Text = key,
                ReadOnly = _isEditMode,
                BackColor = _isEditMode ? SystemColors.Control : Color.White
            };

            // Value
            var lblValue = new Label
            {
                Text = "Giá trị (Value):",
                Location = new Point(20, 55),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            _txtValue = new TextBox
            {
                Location = new Point(130, 52),
                Size = new Size(380, 75),
                Text = currentValue,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };

            // Generator GroupBox
            var grpGen = new GroupBox
            {
                Text = "Sinh giá trị tự động",
                Location = new Point(20, 140),
                Size = new Size(490, 240)
            };

            var lblGenType = new Label
            {
                Text = "Kiểu sinh:",
                Location = new Point(15, 30),
                AutoSize = true
            };
            _cboGenType = new ComboBox
            {
                Location = new Point(110, 27),
                Size = new Size(180, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cboGenType.Items.AddRange(new object[]
            {
                "AES base64",
                "GUID urn",
                "Chuỗi ngẫu nhiên",
                "Cặp RSA 2048"
            });
            _cboGenType.SelectedIndex = 0;

            _lblParam = new Label
            {
                Text = "Độ dài bit:",
                Location = new Point(15, 68),
                AutoSize = true
            };

            _cboAesBits = new ComboBox
            {
                Location = new Point(110, 65),
                Size = new Size(120, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cboAesBits.Items.AddRange(new object[] { "512", "256", "128" });
            _cboAesBits.SelectedIndex = 0;

            _txtGuidPrefix = new TextBox
            {
                Location = new Point(110, 65),
                Size = new Size(220, 23),
                Text = "urn:api:"
            };

            _nudRandomLength = new NumericUpDown
            {
                Location = new Point(110, 65),
                Size = new Size(120, 23),
                Minimum = 8,
                Maximum = 256,
                Value = 32
            };

            _lblRsaNote = new Label
            {
                Location = new Point(15, 65),
                Size = new Size(460, 60),
                Text = "Sinh cả cặp khóa RSA 2048: mở hộp thoại lưu file lần lượt cho Private Key rồi Public Key.\nĐường dẫn file Private Key sẽ được gán làm giá trị biến môi trường."
            };

            var btnGenerate = new Button
            {
                Text = "Sinh giá trị",
                Location = new Point(110, 145),
                Size = new Size(120, 32),
                BackColor = Color.Gold,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            btnGenerate.Click += BtnGenerate_Click;

            grpGen.Controls.Add(lblGenType);
            grpGen.Controls.Add(_cboGenType);
            grpGen.Controls.Add(_lblParam);
            grpGen.Controls.Add(_cboAesBits);
            grpGen.Controls.Add(_txtGuidPrefix);
            grpGen.Controls.Add(_nudRandomLength);
            grpGen.Controls.Add(_lblRsaNote);
            grpGen.Controls.Add(btnGenerate);

            _cboGenType.SelectedIndexChanged += CboGenType_SelectedIndexChanged;
            UpdateGenControlsVisibility();

            // Bottom Buttons
            var btnSave = new Button
            {
                Text = "Lưu",
                Location = new Point(320, 400),
                Size = new Size(90, 35),
                BackColor = Color.LimeGreen,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            btnSave.Click += BtnSave_Click;

            var btnCancel = new Button
            {
                Text = "Hủy",
                Location = new Point(420, 400),
                Size = new Size(90, 35),
                BackColor = Color.WhiteSmoke,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = true
            };
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            Controls.Add(lblKey);
            Controls.Add(_txtKey);
            Controls.Add(lblValue);
            Controls.Add(_txtValue);
            Controls.Add(grpGen);
            Controls.Add(btnSave);
            Controls.Add(btnCancel);

            CancelButton = btnCancel;
        }

        private void CboGenType_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdateGenControlsVisibility();
        }

        private void UpdateGenControlsVisibility()
        {
            int index = _cboGenType.SelectedIndex;
            _cboAesBits.Visible = (index == 0);
            _txtGuidPrefix.Visible = (index == 1);
            _nudRandomLength.Visible = (index == 2);
            _lblRsaNote.Visible = (index == 3);

            switch (index)
            {
                case 0:
                    _lblParam.Visible = true;
                    _lblParam.Text = "Độ dài bit:";
                    break;
                case 1:
                    _lblParam.Visible = true;
                    _lblParam.Text = "Tiền tố URN:";
                    break;
                case 2:
                    _lblParam.Visible = true;
                    _lblParam.Text = "Độ dài ký tự:";
                    break;
                case 3:
                    _lblParam.Visible = false;
                    break;
            }
        }

        private void BtnGenerate_Click(object? sender, EventArgs e)
        {
            try
            {
                int index = _cboGenType.SelectedIndex;
                switch (index)
                {
                    case 0: // AES
                        int bits = int.Parse(_cboAesBits.SelectedItem?.ToString() ?? "512");
                        _txtValue.Text = EnvValueGenerator.GenerateAesKey(bits);
                        break;
                    case 1: // GUID urn
                        _txtValue.Text = EnvValueGenerator.GenerateUrn(_txtGuidPrefix.Text);
                        break;
                    case 2: // Random string
                        int len = (int)_nudRandomLength.Value;
                        _txtValue.Text = EnvValueGenerator.GenerateRandomString(len);
                        break;
                    case 3: // RSA 2048
                        GenerateRsaKeyPair();
                        break;
                }
            }
            catch (Exception ex)
            {
                FrmMain.ThrowError($"Lỗi khi sinh giá trị: {ex.Message}");
            }
        }

        private void GenerateRsaKeyPair()
        {
            try
            {
                var keyPair = Cypher.RsaCypher.GenerateKeys(2048);
                string privateKeyBase64 = Cypher.RsaCypher.ExportPrivateKey(keyPair.PrivateKey);
                byte[] privateBytes = Cypher.CypherHelper.Utf8ToByteArray(privateKeyBase64);
                string publicKeyBase64 = Cypher.RsaCypher.ExportPublicKey(keyPair.PublicKey);
                byte[] publicBytes = Cypher.CypherHelper.Utf8ToByteArray(publicKeyBase64);

                using var sfdPrivate = new SaveFileDialog();
                sfdPrivate.Title = "Lưu Private Key";
                sfdPrivate.Filter = "Key Files (*.key)|*.key|PEM Files (*.pem)|*.pem|All Files (*.*)|*.*";
                sfdPrivate.FileName = "private_key.key";
                sfdPrivate.InitialDirectory = Directory.GetCurrentDirectory();
                if (sfdPrivate.ShowDialog(this) != DialogResult.OK)
                    return; // Hủy = giữ nguyên value, không báo lỗi

                string privatePath = Path.GetFullPath(sfdPrivate.FileName);

                using var sfdPublic = new SaveFileDialog();
                sfdPublic.Title = "Lưu Public Key";
                sfdPublic.Filter = "Key Files (*.key)|*.key|PEM Files (*.pem)|*.pem|All Files (*.*)|*.*";
                sfdPublic.FileName = "public_key.key";
                sfdPublic.InitialDirectory = Path.GetDirectoryName(privatePath) ?? Directory.GetCurrentDirectory();
                if (sfdPublic.ShowDialog(this) != DialogResult.OK)
                    return; // Hủy = giữ nguyên value, không báo lỗi

                string publicPath = Path.GetFullPath(sfdPublic.FileName);

                string? privDir = Path.GetDirectoryName(privatePath);
                if (!string.IsNullOrEmpty(privDir) && !Directory.Exists(privDir))
                    Directory.CreateDirectory(privDir);
                File.WriteAllBytes(privatePath, privateBytes);

                string? pubDir = Path.GetDirectoryName(publicPath);
                if (!string.IsNullOrEmpty(pubDir) && !Directory.Exists(pubDir))
                    Directory.CreateDirectory(pubDir);
                File.WriteAllBytes(publicPath, publicBytes);

                _txtValue.Text = privatePath;
                FrmMain.ThrowInfo($"Đã sinh và lưu cặp khóa RSA 2048 thành công!\n- Private Key (đã gán vào ô giá trị): {privatePath}\n- Public Key: {publicPath}");
            }
            catch (Exception ex)
            {
                FrmMain.ThrowError($"Lỗi khi sinh cặp khóa RSA: {ex.Message}");
            }
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            string key = _txtKey.Text;
            string val = _txtValue.Text;

            if (!_isEditMode)
            {
                var defaultKeys = FrmMain.SysConfig?.EnvKeys ?? Enumerable.Empty<string>();
                string? keyError = CustomEnvService.ValidateKey(key, defaultKeys);
                if (keyError != null)
                {
                    FrmMain.ThrowError(keyError);
                    return;
                }
            }

            string? valError = CustomEnvService.ValidateValue(val);
            if (valError != null)
            {
                FrmMain.ThrowError(valError);
                return;
            }

            try
            {
                if (_isEditMode)
                {
                    await FrmMain.RunWithLoadingAsync(() =>
                    {
                        CustomEnvService.UpdateValue(key, val);
                        return Task.CompletedTask;
                    });
                    FrmMain.ThrowInfo($"Cập nhật biến '{key}' thành công.");
                }
                else
                {
                    var defaultKeys = FrmMain.SysConfig?.EnvKeys ?? Enumerable.Empty<string>();
                    await FrmMain.RunWithLoadingAsync(() =>
                    {
                        CustomEnvService.Create(key, val, defaultKeys);
                        return Task.CompletedTask;
                    });
                    FrmMain.ThrowInfo($"Thêm biến '{key}' thành công.");
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex) when (ex is SecurityException or UnauthorizedAccessException)
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
