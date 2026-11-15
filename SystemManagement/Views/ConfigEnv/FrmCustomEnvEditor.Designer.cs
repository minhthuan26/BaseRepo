namespace SystemManagement.Views.ConfigEnv
{
    partial class FrmCustomEnvEditor
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
            lbl_Key = new Label();
            txt_Key = new TextBox();
            lbl_Value = new Label();
            txt_Value = new TextBox();
            grp_Gen = new GroupBox();
            btn_Generate = new Button();
            lbl_RsaNote = new Label();
            nud_RandomLength = new NumericUpDown();
            txt_GuidPrefix = new TextBox();
            cbo_AesBits = new ComboBox();
            lbl_Param = new Label();
            cbo_GenType = new ComboBox();
            lbl_GenType = new Label();
            btn_Save = new Button();
            btn_Cancel = new Button();
            grp_Gen.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nud_RandomLength).BeginInit();
            SuspendLayout();
            // 
            // lbl_Key
            // 
            lbl_Key.AutoSize = true;
            lbl_Key.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lbl_Key.Location = new Point(20, 20);
            lbl_Key.Name = "lbl_Key";
            lbl_Key.Size = new Size(89, 15);
            lbl_Key.TabIndex = 0;
            lbl_Key.Text = "Tên biến (Key):";
            // 
            // txt_Key
            // 
            txt_Key.Location = new Point(130, 17);
            txt_Key.Name = "txt_Key";
            txt_Key.Size = new Size(380, 23);
            txt_Key.TabIndex = 1;
            // 
            // lbl_Value
            // 
            lbl_Value.AutoSize = true;
            lbl_Value.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lbl_Value.Location = new Point(20, 55);
            lbl_Value.Name = "lbl_Value";
            lbl_Value.Size = new Size(88, 15);
            lbl_Value.TabIndex = 2;
            lbl_Value.Text = "Giá trị (Value):";
            // 
            // txt_Value
            // 
            txt_Value.Location = new Point(130, 52);
            txt_Value.Multiline = true;
            txt_Value.Name = "txt_Value";
            txt_Value.ScrollBars = ScrollBars.Vertical;
            txt_Value.Size = new Size(380, 75);
            txt_Value.TabIndex = 3;
            // 
            // grp_Gen
            // 
            grp_Gen.Controls.Add(btn_Generate);
            grp_Gen.Controls.Add(lbl_RsaNote);
            grp_Gen.Controls.Add(nud_RandomLength);
            grp_Gen.Controls.Add(txt_GuidPrefix);
            grp_Gen.Controls.Add(cbo_AesBits);
            grp_Gen.Controls.Add(lbl_Param);
            grp_Gen.Controls.Add(cbo_GenType);
            grp_Gen.Controls.Add(lbl_GenType);
            grp_Gen.Location = new Point(20, 140);
            grp_Gen.Name = "grp_Gen";
            grp_Gen.Size = new Size(490, 240);
            grp_Gen.TabIndex = 4;
            grp_Gen.TabStop = false;
            grp_Gen.Text = "Sinh giá trị tự động";
            // 
            // btn_Generate
            // 
            btn_Generate.BackColor = Color.Gold;
            btn_Generate.Cursor = Cursors.Hand;
            btn_Generate.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btn_Generate.Location = new Point(110, 145);
            btn_Generate.Name = "btn_Generate";
            btn_Generate.Size = new Size(120, 32);
            btn_Generate.TabIndex = 7;
            btn_Generate.Text = "Sinh giá trị";
            btn_Generate.UseVisualStyleBackColor = false;
            btn_Generate.Click += btn_Generate_Click;
            // 
            // lbl_RsaNote
            // 
            lbl_RsaNote.Location = new Point(15, 65);
            lbl_RsaNote.Name = "lbl_RsaNote";
            lbl_RsaNote.Size = new Size(460, 60);
            lbl_RsaNote.TabIndex = 6;
            lbl_RsaNote.Text = "Sinh cả cặp khóa RSA 2048: mở hộp thoại lưu file lần lượt cho Private Key rồi Public Key.\r\nĐường dẫn file Private Key sẽ được gán làm giá trị biến môi trường.";
            // 
            // nud_RandomLength
            // 
            nud_RandomLength.Location = new Point(110, 65);
            nud_RandomLength.Maximum = new decimal(new int[] { 256, 0, 0, 0 });
            nud_RandomLength.Minimum = new decimal(new int[] { 8, 0, 0, 0 });
            nud_RandomLength.Name = "nud_RandomLength";
            nud_RandomLength.Size = new Size(120, 23);
            nud_RandomLength.TabIndex = 5;
            nud_RandomLength.Value = new decimal(new int[] { 32, 0, 0, 0 });
            // 
            // txt_GuidPrefix
            // 
            txt_GuidPrefix.Location = new Point(110, 65);
            txt_GuidPrefix.Name = "txt_GuidPrefix";
            txt_GuidPrefix.Size = new Size(220, 23);
            txt_GuidPrefix.TabIndex = 4;
            txt_GuidPrefix.Text = "urn:api:";
            // 
            // cbo_AesBits
            // 
            cbo_AesBits.DropDownStyle = ComboBoxStyle.DropDownList;
            cbo_AesBits.FormattingEnabled = true;
            cbo_AesBits.Items.AddRange(new object[] { "512", "256", "128" });
            cbo_AesBits.Location = new Point(110, 65);
            cbo_AesBits.Name = "cbo_AesBits";
            cbo_AesBits.Size = new Size(120, 23);
            cbo_AesBits.TabIndex = 3;
            // 
            // lbl_Param
            // 
            lbl_Param.AutoSize = true;
            lbl_Param.Location = new Point(15, 68);
            lbl_Param.Name = "lbl_Param";
            lbl_Param.Size = new Size(65, 15);
            lbl_Param.TabIndex = 2;
            lbl_Param.Text = "Độ dài bit:";
            // 
            // cbo_GenType
            // 
            cbo_GenType.DropDownStyle = ComboBoxStyle.DropDownList;
            cbo_GenType.FormattingEnabled = true;
            cbo_GenType.Items.AddRange(new object[] { "AES base64", "GUID urn", "Chuỗi ngẫu nhiên", "Cặp RSA 2048" });
            cbo_GenType.Location = new Point(110, 27);
            cbo_GenType.Name = "cbo_GenType";
            cbo_GenType.Size = new Size(180, 23);
            cbo_GenType.TabIndex = 1;
            cbo_GenType.SelectedIndexChanged += cbo_GenType_SelectedIndexChanged;
            // 
            // lbl_GenType
            // 
            lbl_GenType.AutoSize = true;
            lbl_GenType.Location = new Point(15, 30);
            lbl_GenType.Name = "lbl_GenType";
            lbl_GenType.Size = new Size(59, 15);
            lbl_GenType.TabIndex = 0;
            lbl_GenType.Text = "Kiểu sinh:";
            // 
            // btn_Save
            // 
            btn_Save.BackColor = Color.LimeGreen;
            btn_Save.Cursor = Cursors.Hand;
            btn_Save.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btn_Save.Location = new Point(320, 400);
            btn_Save.Name = "btn_Save";
            btn_Save.Size = new Size(90, 35);
            btn_Save.TabIndex = 5;
            btn_Save.Text = "Lưu";
            btn_Save.UseVisualStyleBackColor = false;
            btn_Save.Click += btn_Save_Click;
            // 
            // btn_Cancel
            // 
            btn_Cancel.BackColor = Color.WhiteSmoke;
            btn_Cancel.Cursor = Cursors.Hand;
            btn_Cancel.DialogResult = DialogResult.Cancel;
            btn_Cancel.Location = new Point(420, 400);
            btn_Cancel.Name = "btn_Cancel";
            btn_Cancel.Size = new Size(90, 35);
            btn_Cancel.TabIndex = 6;
            btn_Cancel.Text = "Hủy";
            btn_Cancel.UseVisualStyleBackColor = true;
            btn_Cancel.Click += btn_Cancel_Click;
            // 
            // FrmCustomEnvEditor
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            CancelButton = btn_Cancel;
            ClientSize = new Size(540, 460);
            Controls.Add(btn_Cancel);
            Controls.Add(btn_Save);
            Controls.Add(grp_Gen);
            Controls.Add(txt_Value);
            Controls.Add(lbl_Value);
            Controls.Add(txt_Key);
            Controls.Add(lbl_Key);
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FrmCustomEnvEditor";
            StartPosition = FormStartPosition.CenterParent;
            grp_Gen.ResumeLayout(false);
            grp_Gen.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nud_RandomLength).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lbl_Key;
        private TextBox txt_Key;
        private Label lbl_Value;
        private TextBox txt_Value;
        private GroupBox grp_Gen;
        private Label lbl_GenType;
        private ComboBox cbo_GenType;
        private Label lbl_Param;
        private ComboBox cbo_AesBits;
        private TextBox txt_GuidPrefix;
        private NumericUpDown nud_RandomLength;
        private Label lbl_RsaNote;
        private Button btn_Generate;
        private Button btn_Save;
        private Button btn_Cancel;
    }
}
