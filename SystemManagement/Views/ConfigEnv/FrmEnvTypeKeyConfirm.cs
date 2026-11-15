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
    public class FrmEnvTypeKeyConfirm : Form
    {
        private readonly string _expectedKey;
        private readonly TextBox _txtInput;
        private readonly Button _btnOk;
        private readonly Button _btnCancel;

        public FrmEnvTypeKeyConfirm(string key, string title, string message)
        {
            _expectedKey = key ?? string.Empty;

            Text = title;
            ClientSize = new Size(520, 270);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 9F);

            var lblMessage = new Label
            {
                Text = message,
                Location = new Point(20, 15),
                Size = new Size(480, 110),
                ForeColor = Color.DarkRed,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };

            var lblPrompt = new Label
            {
                Text = $"Vui lòng nhập chính xác tên biến '{_expectedKey}' để xác nhận:",
                Location = new Point(20, 135),
                Size = new Size(480, 20),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            _txtInput = new TextBox
            {
                Location = new Point(20, 160),
                Size = new Size(480, 23)
            };
            _txtInput.TextChanged += TxtInput_TextChanged;

            _btnOk = new Button
            {
                Text = "Xác nhận",
                Location = new Point(295, 210),
                Size = new Size(100, 34),
                BackColor = Color.Coral,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false,
                Enabled = false
            };
            _btnOk.Click += BtnOk_Click;

            _btnCancel = new Button
            {
                Text = "Hủy",
                Location = new Point(405, 210),
                Size = new Size(95, 34),
                BackColor = Color.WhiteSmoke,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = true,
                DialogResult = DialogResult.Cancel
            };

            Controls.Add(lblMessage);
            Controls.Add(lblPrompt);
            Controls.Add(_txtInput);
            Controls.Add(_btnOk);
            Controls.Add(_btnCancel);

            AcceptButton = _btnOk;
            CancelButton = _btnCancel;
        }

        private void TxtInput_TextChanged(object? sender, EventArgs e)
        {
            // Nút OK chỉ được bật khi người dùng nhập khớp chính xác tên key (so sánh Ordinal)
            _btnOk.Enabled = string.Equals(_txtInput.Text, _expectedKey, StringComparison.Ordinal);
        }

        private void BtnOk_Click(object? sender, EventArgs e)
        {
            if (!string.Equals(_txtInput.Text, _expectedKey, StringComparison.Ordinal))
            {
                _btnOk.Enabled = false;
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
