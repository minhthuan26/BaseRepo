using System;
using System.Drawing;
using System.Windows.Forms;

namespace SystemManagement.Views.ConfigEnv
{
    /// <summary>
    /// Hộp thoại xác nhận thao tác nguy hiểm trên biến môi trường cốt lõi.
    /// Giải thích: Yêu cầu người dùng gõ lại chính xác tên biến (so sánh Ordinal)
    /// để tránh thao tác nhầm lẫn gây hỏng hóc hệ thống.
    /// </summary>
    public partial class FrmEnvTypeKeyConfirm : Form
    {
        private readonly string _expectedKey;

        public FrmEnvTypeKeyConfirm(string key, string title, string message)
        {
            _expectedKey = key ?? string.Empty;

            InitializeComponent();

            Text = title;
            lbl_Message.Text = message;
            lbl_Prompt.Text = $"Vui lòng nhập chính xác tên biến '{_expectedKey}' để xác nhận:";
        }

        private void txt_Input_TextChanged(object? sender, EventArgs e)
        {
            // Nút OK chỉ được bật khi người dùng nhập khớp chính xác tên key (so sánh Ordinal)
            btn_Ok.Enabled = string.Equals(txt_Input.Text, _expectedKey, StringComparison.Ordinal);
        }

        private void btn_Ok_Click(object? sender, EventArgs e)
        {
            if (!string.Equals(txt_Input.Text, _expectedKey, StringComparison.Ordinal))
            {
                btn_Ok.Enabled = false;
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
