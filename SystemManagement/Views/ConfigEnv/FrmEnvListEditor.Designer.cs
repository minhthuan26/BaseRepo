namespace SystemManagement.Views.ConfigEnv
{
    partial class FrmEnvListEditor
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
            lbl_Title = new Label();
            lbl_Desc = new Label();
            lst_Items = new ListBox();
            btn_Up = new Button();
            btn_Down = new Button();
            btn_Delete = new Button();
            lbl_Input = new Label();
            txt_Input = new TextBox();
            btn_Add = new Button();
            btn_Update = new Button();
            pnl_Warning = new Panel();
            lbl_Warning = new Label();
            btn_Save = new Button();
            btn_Cancel = new Button();
            pnl_Warning.SuspendLayout();
            SuspendLayout();
            // 
            // lbl_Title
            // 
            lbl_Title.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lbl_Title.Location = new Point(20, 15);
            lbl_Title.Name = "lbl_Title";
            lbl_Title.Size = new Size(680, 24);
            lbl_Title.TabIndex = 0;
            // 
            // lbl_Desc
            // 
            lbl_Desc.ForeColor = Color.DimGray;
            lbl_Desc.Location = new Point(20, 40);
            lbl_Desc.Name = "lbl_Desc";
            lbl_Desc.Size = new Size(680, 20);
            lbl_Desc.TabIndex = 1;
            lbl_Desc.Text = "Mỗi mục phân tách bởi dấu ';'. Ký tự thô dạng %...% được giữ nguyên khi lưu.";
            // 
            // lst_Items
            // 
            lst_Items.Font = new Font("Segoe UI", 9F);
            lst_Items.HorizontalScrollbar = true;
            lst_Items.IntegralHeight = false;
            lst_Items.ItemHeight = 15;
            lst_Items.Location = new Point(20, 68);
            lst_Items.Name = "lst_Items";
            lst_Items.Size = new Size(570, 240);
            lst_Items.TabIndex = 2;
            lst_Items.SelectedIndexChanged += lst_Items_SelectedIndexChanged;
            // 
            // btn_Up
            // 
            btn_Up.BackColor = Color.WhiteSmoke;
            btn_Up.Cursor = Cursors.Hand;
            btn_Up.Location = new Point(600, 68);
            btn_Up.Name = "btn_Up";
            btn_Up.Size = new Size(100, 32);
            btn_Up.TabIndex = 3;
            btn_Up.Text = "▲ Lên";
            btn_Up.UseVisualStyleBackColor = true;
            btn_Up.Click += btn_Up_Click;
            // 
            // btn_Down
            // 
            btn_Down.BackColor = Color.WhiteSmoke;
            btn_Down.Cursor = Cursors.Hand;
            btn_Down.Location = new Point(600, 108);
            btn_Down.Name = "btn_Down";
            btn_Down.Size = new Size(100, 32);
            btn_Down.TabIndex = 4;
            btn_Down.Text = "▼ Xuống";
            btn_Down.UseVisualStyleBackColor = true;
            btn_Down.Click += btn_Down_Click;
            // 
            // btn_Delete
            // 
            btn_Delete.BackColor = Color.WhiteSmoke;
            btn_Delete.Cursor = Cursors.Hand;
            btn_Delete.ForeColor = Color.DarkRed;
            btn_Delete.Location = new Point(600, 148);
            btn_Delete.Name = "btn_Delete";
            btn_Delete.Size = new Size(100, 32);
            btn_Delete.TabIndex = 5;
            btn_Delete.Text = "Xóa";
            btn_Delete.UseVisualStyleBackColor = true;
            btn_Delete.Click += btn_Delete_Click;
            // 
            // lbl_Input
            // 
            lbl_Input.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lbl_Input.Location = new Point(20, 322);
            lbl_Input.Name = "lbl_Input";
            lbl_Input.Size = new Size(75, 23);
            lbl_Input.TabIndex = 6;
            lbl_Input.Text = "Mục nhập:";
            // 
            // txt_Input
            // 
            txt_Input.Location = new Point(100, 320);
            txt_Input.Name = "txt_Input";
            txt_Input.Size = new Size(400, 23);
            txt_Input.TabIndex = 7;
            txt_Input.TextChanged += txt_Input_TextChanged;
            // 
            // btn_Add
            // 
            btn_Add.BackColor = Color.LightSkyBlue;
            btn_Add.Cursor = Cursors.Hand;
            btn_Add.Location = new Point(510, 318);
            btn_Add.Name = "btn_Add";
            btn_Add.Size = new Size(90, 27);
            btn_Add.TabIndex = 8;
            btn_Add.Text = "Thêm";
            btn_Add.UseVisualStyleBackColor = false;
            btn_Add.Click += btn_Add_Click;
            // 
            // btn_Update
            // 
            btn_Update.BackColor = Color.LightYellow;
            btn_Update.Cursor = Cursors.Hand;
            btn_Update.Location = new Point(610, 318);
            btn_Update.Name = "btn_Update";
            btn_Update.Size = new Size(90, 27);
            btn_Update.TabIndex = 9;
            btn_Update.Text = "Cập nhật";
            btn_Update.UseVisualStyleBackColor = false;
            btn_Update.Click += btn_Update_Click;
            // 
            // pnl_Warning
            // 
            pnl_Warning.BackColor = Color.FromArgb(240, 253, 244);
            pnl_Warning.BorderStyle = BorderStyle.FixedSingle;
            pnl_Warning.Controls.Add(lbl_Warning);
            pnl_Warning.Location = new Point(20, 355);
            pnl_Warning.Name = "pnl_Warning";
            pnl_Warning.Size = new Size(680, 80);
            pnl_Warning.TabIndex = 10;
            // 
            // lbl_Warning
            // 
            lbl_Warning.Dock = DockStyle.Fill;
            lbl_Warning.Font = new Font("Segoe UI", 8.5F);
            lbl_Warning.ForeColor = Color.DarkGreen;
            lbl_Warning.Location = new Point(0, 0);
            lbl_Warning.Name = "lbl_Warning";
            lbl_Warning.Padding = new Padding(6);
            lbl_Warning.Size = new Size(678, 78);
            lbl_Warning.TabIndex = 0;
            // 
            // btn_Save
            // 
            btn_Save.BackColor = Color.LimeGreen;
            btn_Save.Cursor = Cursors.Hand;
            btn_Save.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btn_Save.Location = new Point(500, 450);
            btn_Save.Name = "btn_Save";
            btn_Save.Size = new Size(95, 34);
            btn_Save.TabIndex = 11;
            btn_Save.Text = "Lưu";
            btn_Save.UseVisualStyleBackColor = false;
            btn_Save.Click += btn_Save_Click;
            // 
            // btn_Cancel
            // 
            btn_Cancel.BackColor = Color.WhiteSmoke;
            btn_Cancel.Cursor = Cursors.Hand;
            btn_Cancel.DialogResult = DialogResult.Cancel;
            btn_Cancel.Location = new Point(605, 450);
            btn_Cancel.Name = "btn_Cancel";
            btn_Cancel.Size = new Size(95, 34);
            btn_Cancel.TabIndex = 12;
            btn_Cancel.Text = "Hủy";
            btn_Cancel.UseVisualStyleBackColor = true;
            btn_Cancel.Click += btn_Cancel_Click;
            // 
            // FrmEnvListEditor
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            CancelButton = btn_Cancel;
            ClientSize = new Size(720, 500);
            Controls.Add(lbl_Title);
            Controls.Add(lbl_Desc);
            Controls.Add(lst_Items);
            Controls.Add(btn_Up);
            Controls.Add(btn_Down);
            Controls.Add(btn_Delete);
            Controls.Add(lbl_Input);
            Controls.Add(txt_Input);
            Controls.Add(btn_Add);
            Controls.Add(btn_Update);
            Controls.Add(pnl_Warning);
            Controls.Add(btn_Save);
            Controls.Add(btn_Cancel);
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FrmEnvListEditor";
            StartPosition = FormStartPosition.CenterParent;
            pnl_Warning.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lbl_Title;
        private Label lbl_Desc;
        private ListBox lst_Items;
        private Button btn_Up;
        private Button btn_Down;
        private Button btn_Delete;
        private Label lbl_Input;
        private TextBox txt_Input;
        private Button btn_Add;
        private Button btn_Update;
        private Panel pnl_Warning;
        private Label lbl_Warning;
        private Button btn_Save;
        private Button btn_Cancel;
    }
}
