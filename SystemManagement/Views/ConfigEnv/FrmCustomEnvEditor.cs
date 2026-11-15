using CommonLibs.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SystemManagement.Views.ConfigEnv
{
    internal partial class FrmCustomEnvEditor : Form
    {
        private readonly bool _isEditMode;
        private readonly bool _isCoreEdit;

        public FrmCustomEnvEditor(bool isEditMode, string key, string currentValue, bool isCoreEdit = false)
        {
            _isEditMode = isEditMode;
            _isCoreEdit = isCoreEdit;

            InitializeComponent();

            Text = _isCoreEdit
                ? "Sửa giá trị biến hệ thống cốt lõi"
                : (_isEditMode ? "Sửa giá trị biến môi trường" : "Thêm biến môi trường tùy chỉnh");

            if (_isCoreEdit)
            {
                ClientSize = new Size(540, 200);
                grp_Gen.Visible = false;
                btn_Save.Location = new Point(320, 145);
                btn_Cancel.Location = new Point(420, 145);
            }

            txt_Key.Text = key;
            txt_Key.ReadOnly = _isEditMode;
            txt_Key.BackColor = _isEditMode ? SystemColors.Control : Color.White;

            txt_Value.Text = currentValue;

            cbo_GenType.SelectedIndex = 0;
            cbo_AesBits.SelectedIndex = 0;
            UpdateGenControlsVisibility();
        }

        private void cbo_GenType_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdateGenControlsVisibility();
        }

        private void UpdateGenControlsVisibility()
        {
            int index = cbo_GenType.SelectedIndex;
            cbo_AesBits.Visible = (index == 0);
            txt_GuidPrefix.Visible = (index == 1);
            nud_RandomLength.Visible = (index == 2);
            lbl_RsaNote.Visible = (index == 3);

            switch (index)
            {
                case 0:
                    lbl_Param.Visible = true;
                    lbl_Param.Text = "Độ dài bit:";
                    break;
                case 1:
                    lbl_Param.Visible = true;
                    lbl_Param.Text = "Tiền tố URN:";
                    break;
                case 2:
                    lbl_Param.Visible = true;
                    lbl_Param.Text = "Độ dài ký tự:";
                    break;
                case 3:
                    lbl_Param.Visible = false;
                    break;
            }
        }

        private void btn_Generate_Click(object? sender, EventArgs e)
        {
            try
            {
                int index = cbo_GenType.SelectedIndex;
                switch (index)
                {
                    case 0: // AES
                        int bits = int.Parse(cbo_AesBits.SelectedItem?.ToString() ?? "512");
                        txt_Value.Text = EnvValueGenerator.GenerateAesKey(bits);
                        break;
                    case 1: // GUID urn
                        txt_Value.Text = EnvValueGenerator.GenerateUrn(txt_GuidPrefix.Text);
                        break;
                    case 2: // Random string
                        int len = (int)nud_RandomLength.Value;
                        txt_Value.Text = EnvValueGenerator.GenerateRandomString(len);
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

                txt_Value.Text = privatePath;
                FrmMain.ThrowInfo($"Đã sinh và lưu cặp khóa RSA 2048 thành công!\n- Private Key (đã gán vào ô giá trị): {privatePath}\n- Public Key: {publicPath}");
            }
            catch (Exception ex)
            {
                FrmMain.ThrowError($"Lỗi khi sinh cặp khóa RSA: {ex.Message}");
            }
        }

        private async void btn_Save_Click(object? sender, EventArgs e)
        {
            string key = txt_Key.Text;
            string val = txt_Value.Text;

            if (_isCoreEdit)
            {
                string? coreValError = CustomEnvService.ValidateValue(val);
                if (coreValError != null)
                {
                    FrmMain.ThrowError(coreValError);
                    return;
                }

                if (val.Contains('\r') || val.Contains('\n'))
                {
                    FrmMain.ThrowError("Giá trị biến không được chứa ký tự xuống dòng.");
                    return;
                }

                try
                {
                    await FrmMain.RunWithLoadingAsync(() =>
                    {
                        CustomEnvService.UpdateCoreValue(key, val);
                        return Task.CompletedTask;
                    });

                    FrmMain.ThrowInfo($"Cập nhật biến '{key}' thành công.\nVui lòng mở lại terminal hoặc ứng dụng để nhận giá trị mới.");
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
                return;
            }

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

        private void btn_Cancel_Click(object? sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
