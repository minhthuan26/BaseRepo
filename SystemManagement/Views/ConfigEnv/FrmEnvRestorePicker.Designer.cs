namespace SystemManagement.Views.ConfigEnv
{
    partial class FrmEnvRestorePicker
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
            lbl_Header = new Label();
            dgv_Backups = new DataGridView();
            col_Time = new DataGridViewTextBoxColumn();
            col_Reason = new DataGridViewTextBoxColumn();
            col_Kind = new DataGridViewTextBoxColumn();
            col_Value = new DataGridViewTextBoxColumn();
            btn_Select = new Button();
            btn_Cancel = new Button();
            ((System.ComponentModel.ISupportInitialize)dgv_Backups).BeginInit();
            SuspendLayout();
            // 
            // lbl_Header
            // 
            lbl_Header.AutoSize = true;
            lbl_Header.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lbl_Header.Location = new Point(15, 15);
            lbl_Header.Name = "lbl_Header";
            lbl_Header.Size = new Size(0, 15);
            lbl_Header.TabIndex = 0;
            // 
            // dgv_Backups
            // 
            dgv_Backups.AllowUserToAddRows = false;
            dgv_Backups.AllowUserToDeleteRows = false;
            dgv_Backups.AllowUserToResizeRows = false;
            dgv_Backups.AutoGenerateColumns = false;
            dgv_Backups.BackgroundColor = Color.White;
            dgv_Backups.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgv_Backups.Columns.AddRange(new DataGridViewColumn[] { col_Time, col_Reason, col_Kind, col_Value });
            dgv_Backups.Location = new Point(15, 42);
            dgv_Backups.MultiSelect = false;
            dgv_Backups.Name = "dgv_Backups";
            dgv_Backups.ReadOnly = true;
            dgv_Backups.RowHeadersVisible = false;
            dgv_Backups.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv_Backups.Size = new Size(730, 320);
            dgv_Backups.TabIndex = 1;
            dgv_Backups.CellDoubleClick += dgv_Backups_CellDoubleClick;
            dgv_Backups.SelectionChanged += dgv_Backups_SelectionChanged;
            // 
            // col_Time
            // 
            col_Time.DataPropertyName = "CreatedAtDisplay";
            col_Time.HeaderText = "Thời gian";
            col_Time.Name = "col_Time";
            col_Time.ReadOnly = true;
            col_Time.Width = 140;
            // 
            // col_Reason
            // 
            col_Reason.DataPropertyName = "Reason";
            col_Reason.HeaderText = "Lý do sao lưu";
            col_Reason.Name = "col_Reason";
            col_Reason.ReadOnly = true;
            col_Reason.Width = 120;
            // 
            // col_Kind
            // 
            col_Kind.DataPropertyName = "Kind";
            col_Kind.HeaderText = "Kiểu dữ liệu";
            col_Kind.Name = "col_Kind";
            col_Kind.ReadOnly = true;
            col_Kind.Width = 110;
            // 
            // col_Value
            // 
            col_Value.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            col_Value.DataPropertyName = "ValuePreview";
            col_Value.HeaderText = "Giá trị (80 ký tự đầu)";
            col_Value.Name = "col_Value";
            col_Value.ReadOnly = true;
            // 
            // btn_Select
            // 
            btn_Select.BackColor = Color.LimeGreen;
            btn_Select.Cursor = Cursors.Hand;
            btn_Select.Enabled = false;
            btn_Select.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btn_Select.Location = new Point(535, 380);
            btn_Select.Name = "btn_Select";
            btn_Select.Size = new Size(100, 34);
            btn_Select.TabIndex = 2;
            btn_Select.Text = "Chọn";
            btn_Select.UseVisualStyleBackColor = false;
            btn_Select.Click += btn_Select_Click;
            // 
            // btn_Cancel
            // 
            btn_Cancel.BackColor = Color.WhiteSmoke;
            btn_Cancel.Cursor = Cursors.Hand;
            btn_Cancel.DialogResult = DialogResult.Cancel;
            btn_Cancel.Location = new Point(645, 380);
            btn_Cancel.Name = "btn_Cancel";
            btn_Cancel.Size = new Size(100, 34);
            btn_Cancel.TabIndex = 3;
            btn_Cancel.Text = "Hủy";
            btn_Cancel.UseVisualStyleBackColor = true;
            // 
            // FrmEnvRestorePicker
            // 
            AcceptButton = btn_Select;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            CancelButton = btn_Cancel;
            ClientSize = new Size(760, 430);
            Controls.Add(lbl_Header);
            Controls.Add(dgv_Backups);
            Controls.Add(btn_Select);
            Controls.Add(btn_Cancel);
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FrmEnvRestorePicker";
            StartPosition = FormStartPosition.CenterParent;
            ((System.ComponentModel.ISupportInitialize)dgv_Backups).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lbl_Header;
        private DataGridView dgv_Backups;
        private DataGridViewTextBoxColumn col_Time;
        private DataGridViewTextBoxColumn col_Reason;
        private DataGridViewTextBoxColumn col_Kind;
        private DataGridViewTextBoxColumn col_Value;
        private Button btn_Select;
        private Button btn_Cancel;
    }
}
