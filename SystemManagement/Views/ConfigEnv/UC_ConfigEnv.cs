using CommonLibs.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SystemManagement.Helpers;
using SystemManagement.Interfaces;
using SystemManagement.Views.Base;

namespace SystemManagement.Views.ConfigEnv
{
    public partial class UC_ConfigEnv : UserControl, IBasePage
    {
        public UC_Base BaseUC { get; set; }
        public string Title { get; set; } = "Cấu hình biến môi trường";
        private string AccessKey { get; set; }
        private string Audience { get; set; }
        private string Issuer { get; set; }
        private string RefreshKey { get; set; }
        private string PrivateKeyPath { get; set; }
        private string PublicKeyPath { get; set; }
        private readonly string[] EnvKeys = new string[]
        {
            "BloodBank__JWT__AccessKey",
            "BloodBank__JWT__RefreshKey",
            "BloodBank__JWT__Audience",
            "BloodBank__JWT__Issuer",
            "BloodBank__PrivateKey",
            "BloodBank__PublicKey"
        };
        public UC_ConfigEnv()
        {
            InitializeComponent();
        }

        private void UC_ConfigEnv_Load(object sender, EventArgs e)
        {
            LoadSavedConfig();
            txt_AccessKey.Text = AccessKey;
            txt_RefreshKey.Text = RefreshKey;
            txt_Audience.Text = Audience;
            txt_Issuer.Text = Issuer;
            txt_PrivateKeyPath.Text = PrivateKeyPath;
            txt_PublicKeyPath.Text = PublicKeyPath;
            CheckRequiredField(txt_AccessKey, this.AccessKey);
            CheckRequiredField(txt_RefreshKey, this.RefreshKey);
            CheckRequiredField(txt_Audience, this.Audience);
            CheckRequiredField(txt_Issuer, this.Issuer);
            CheckRequiredField(txt_PrivateKeyPath, this.PrivateKeyPath);
            CheckRequiredField(txt_PublicKeyPath, this.PublicKeyPath);
        }

        private void LoadSavedConfig()
        {
            this.AccessKey = EnvironmentHelper.Get(EnvKeys[0]);
            this.RefreshKey = EnvironmentHelper.Get(EnvKeys[1]);
            this.Audience = EnvironmentHelper.Get(EnvKeys[2]);
            this.Issuer = EnvironmentHelper.Get(EnvKeys[3]);
            this.PrivateKeyPath = EnvironmentHelper.Get(EnvKeys[4]);
            this.PublicKeyPath = EnvironmentHelper.Get(EnvKeys[5]);
        }

        private bool CheckRequiredField(TextBox textBox, string savedValue)
        {
            if (string.IsNullOrEmpty(textBox.Text) || textBox.Text != savedValue)
            {
                textBox.BackColor = Color.LightPink;
                return false;
            }
            else
            {
                textBox.BackColor = Color.LightGreen;
                return true;
            }
        }

        private void txt_AccessKey_TextChanged(object sender, EventArgs e)
        {
            CheckRequiredField(txt_AccessKey, this.AccessKey);
        }

        private void txt_RefreshKey_TextChanged(object sender, EventArgs e)
        {
            CheckRequiredField(txt_RefreshKey, this.RefreshKey);
        }

        private void txt_Audience_TextChanged(object sender, EventArgs e)
        {
            CheckRequiredField(txt_Audience, this.Audience);
        }

        private void txt_Issuer_TextChanged(object sender, EventArgs e)
        {
            CheckRequiredField(txt_Issuer, this.Issuer);
        }

        private void txt_PrivateKeyPath_TextChanged(object sender, EventArgs e)
        {
            CheckRequiredField(txt_PrivateKeyPath, this.PrivateKeyPath);
        }

        private void txt_PublicKeyPath_TextChanged(object sender, EventArgs e)
        {
            CheckRequiredField(txt_PublicKeyPath, this.PublicKeyPath);
        }

        private void btn_RandomAccessKey_Click(object sender, EventArgs e)
        {
            byte[] keyBytes = Cypher.AesCypher.GenerateAesKey(256);
            txt_AccessKey.Text = Cypher.CypherHelper.ByteArrayToBase64(keyBytes);
        }

        private void btn_RandomRefreshKey_Click(object sender, EventArgs e)
        {
            byte[] keyBytes = Cypher.AesCypher.GenerateAesKey(256);
            txt_RefreshKey.Text = Cypher.CypherHelper.ByteArrayToBase64(keyBytes);
        }

        private void btn_RandomAudience_Click(object sender, EventArgs e)
        {
            txt_Audience.Text = $"urn:api:{Guid.NewGuid().ToString("N")}";
        }

        private void btn_RandomIssuer_Click(object sender, EventArgs e)
        {
            txt_Issuer.Text = $"urn:auth:{Guid.NewGuid().ToString("N")}";
        }

        private async void btn_LoadPrivateKeyPath_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Key Files (*.key;*.pem;*.txt)|*.key;*.pem;*.txt|All Files (*.*)|*.*";
                ofd.InitialDirectory = Directory.GetCurrentDirectory();

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string absolutePath = Path.GetFullPath(ofd.FileName);
                        txt_PrivateKeyPath.Text = absolutePath;

                        byte[] fileBytes = await File.ReadAllBytesAsync(absolutePath);
                        string base64Key = Cypher.CypherHelper.ByteArrayToUtf8(fileBytes).Trim();

                        var testRsa = Cypher.RsaCypher.ImportPrivateKey(base64Key);
                    }
                    catch (Exception ex)
                    {
                        FrmMain.ThrowError($"Failed to load private key: {ex.Message}");
                    }
                }
            }
        }

        private async void btn_RandomPrivateKeyPath_Click(object sender, EventArgs e)
        {
            var keyPair = Cypher.RsaCypher.GenerateKeys(2048);
            string privateKeyBase64 = Cypher.RsaCypher.ExportPrivateKey(keyPair.PrivateKey);
            byte[] keyBytes = Cypher.CypherHelper.Utf8ToByteArray(privateKeyBase64);

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Key Files (*.key)|*.key|PEM Files (*.pem)|*.pem";
                sfd.FileName = "private_key.key";
                sfd.InitialDirectory = Directory.GetCurrentDirectory();

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string absolutePath = Path.GetFullPath(sfd.FileName);

                        string dir = Path.GetDirectoryName(absolutePath);
                        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                        {
                            Directory.CreateDirectory(dir);
                        }

                        await File.WriteAllBytesAsync(absolutePath, keyBytes);
                        txt_PrivateKeyPath.Text = absolutePath;
                    }
                    catch (Exception ex)
                    {
                        FrmMain.ThrowError($"Failed to save private key: {ex.Message}");
                    }
                }
            }
        }

        private async void btn_LoadPublicKeyPath_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Key Files (*.key;*.pem;*.txt)|*.key;*.pem;*.txt|All Files (*.*)|*.*";
                ofd.InitialDirectory = Directory.GetCurrentDirectory();

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string absolutePath = Path.GetFullPath(ofd.FileName);
                        txt_PublicKeyPath.Text = absolutePath;

                        byte[] fileBytes = await File.ReadAllBytesAsync(absolutePath);
                        string base64Key = Cypher.CypherHelper.ByteArrayToUtf8(fileBytes).Trim();

                        var testRsa = Cypher.RsaCypher.ImportPublicKey(base64Key);
                    }
                    catch (Exception ex)
                    {
                        FrmMain.ThrowError($"Failed to load public key: {ex.Message}");
                    }
                }
            }
        }

        private async void btn_RandomPublicKeyPath_Click(object sender, EventArgs e)
        {
            var keyPair = Cypher.RsaCypher.GenerateKeys(2048);
            string publicKeyBase64 = Cypher.RsaCypher.ExportPublicKey(keyPair.PublicKey);
            byte[] keyBytes = Cypher.CypherHelper.Utf8ToByteArray(publicKeyBase64);

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Key Files (*.key)|*.key|PEM Files (*.pem)|*.pem";
                sfd.FileName = "public_key.key";
                sfd.InitialDirectory = Directory.GetCurrentDirectory();

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string absolutePath = Path.GetFullPath(sfd.FileName);

                        string dir = Path.GetDirectoryName(absolutePath);
                        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                        {
                            Directory.CreateDirectory(dir);
                        }

                        await File.WriteAllBytesAsync(absolutePath, keyBytes);
                        txt_PublicKeyPath.Text = absolutePath;

                        FrmMain.ThrowInfo("A brand new matching asymmetric key pair was created! Remember to export the corresponding Private Key file along with it.");
                    }
                    catch (Exception ex)
                    {
                        FrmMain.ThrowError($"Failed to save public key: {ex.Message}");
                    }
                }
            }
        }

        private void SaveAllKeys()
        {
            SaveKey(0, txt_AccessKey);
            SaveKey(1, txt_RefreshKey);
            SaveKey(2, txt_Audience);
            SaveKey(3, txt_Issuer);
            SaveKey(4, txt_PrivateKeyPath);
            SaveKey(5, txt_PublicKeyPath);
        }

        private void SaveKey(int index, TextBox txt)
        {
            string key = EnvKeys[index];
            string value = txt.Text.Trim();

            bool success = EnvironmentHelper.Exists(key)
                ? EnvironmentHelper.Update(key, value)
                : EnvironmentHelper.Create(key, value);

            if (!success)
                throw new Exception($"Lưu thất bại: {key}");

            switch (index)
            {
                case 0: AccessKey = value; break;
                case 1: RefreshKey = value; break;
                case 2: Audience = value; break;
                case 3: Issuer = value; break;
                case 4: PrivateKeyPath = value; break;
                case 5: PublicKeyPath = value; break;
            }

            this.Invoke(new Action(() =>
            {
                CheckRequiredField(txt, value);
            }));
        }

        private async void btn_Save_Click(object sender, EventArgs e)
        {
            try
            {
                await FrmMain.RunWithLoadingAsync(async () =>
                {
                    await Task.Run(() => SaveAllKeys());
                });

                FrmMain.ThrowInfo("Cập nhật thành công");
            }
            catch (Exception ex)
            {
                FrmMain.ThrowError($"{ex.Message}");
            }
        }

        private void btn_AutoAll_Click(object sender, EventArgs e)
        {
            btn_RandomAccessKey_Click(sender, e);
            btn_RandomRefreshKey_Click(sender, e);
            btn_RandomAudience_Click(sender, e);
            btn_RandomIssuer_Click(sender, e);
            btn_RandomPrivateKeyPath_Click(sender, e);
            btn_RandomPublicKeyPath_Click(sender, e);
        }
    }
}
