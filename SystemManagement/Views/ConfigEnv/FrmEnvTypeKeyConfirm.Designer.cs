namespace SystemManagement.Views.ConfigEnv
{
    partial class FrmEnvTypeKeyConfirm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            lbl_Message = new Label();
            lbl_Prompt = new Label();
            txt_Input = new TextBox();
            btn_Ok = new Button();
            btn_Cancel = new Button();
            SuspendLayout();
            // 
            // lbl_Message
            // 
            lbl_Message.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            lbl_Message.ForeColor = Color.DarkRed;
            lbl_Message.Location = new Point(20, 15);
            lbl_Message.Name = "lbl_Message";
            lbl_Message.Size = new Size(480, 110);
            lbl_Message.TabIndex = 0;
            // 
            // lbl_Prompt
            // 
            lbl_Prompt.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lbl_Prompt.Location = new Point(20, 135);
            lbl_Prompt.Name = "lbl_Prompt";
            lbl_Prompt.Size = new Size(480, 20);
            lbl_Prompt.TabIndex = 1;
            // 
            // txt_Input
            // 
            txt_Input.Location = new Point(20, 160);
            txt_Input.Name = "txt_Input";
            txt_Input.Size = new Size(480, 23);
            txt_Input.TabIndex = 2;
            txt_Input.TextChanged += txt_Input_TextChanged;
            // 
            // btn_Ok
            // 
            btn_Ok.BackColor = Color.Coral;
            btn_Ok.Cursor = Cursors.Hand;
            btn_Ok.Enabled = false;
            btn_Ok.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btn_Ok.ForeColor = Color.White;
            btn_Ok.Location = new Point(295, 210);
            btn_Ok.Name = "btn_Ok";
            btn_Ok.Size = new Size(100, 34);
            btn_Ok.TabIndex = 3;
            btn_Ok.Text = "Xác nhận";
            btn_Ok.UseVisualStyleBackColor = false;
            btn_Ok.Click += btn_Ok_Click;
            // 
            // btn_Cancel
            // 
            btn_Cancel.BackColor = Color.WhiteSmoke;
            btn_Cancel.Cursor = Cursors.Hand;
            btn_Cancel.DialogResult = DialogResult.Cancel;
            btn_Cancel.Location = new Point(405, 210);
            btn_Cancel.Name = "btn_Cancel";
            btn_Cancel.Size = new Size(95, 34);
            btn_Cancel.TabIndex = 4;
            btn_Cancel.Text = "Hủy";
            btn_Cancel.UseVisualStyleBackColor = true;
            // 
            // FrmEnvTypeKeyConfirm
            // 
            AcceptButton = btn_Ok;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            CancelButton = btn_Cancel;
            ClientSize = new Size(520, 270);
            Controls.Add(lbl_Message);
            Controls.Add(lbl_Prompt);
            Controls.Add(txt_Input);
            Controls.Add(btn_Ok);
            Controls.Add(btn_Cancel);
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FrmEnvTypeKeyConfirm";
            StartPosition = FormStartPosition.CenterParent;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lbl_Message;
        private Label lbl_Prompt;
        private TextBox txt_Input;
        private Button btn_Ok;
        private Button btn_Cancel;
    }
}
