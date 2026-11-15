namespace SystemManagement.Views.ConfigEnv
{
    partial class UC_ConfigEnvCustom
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            pnl_Toolbar = new Panel();
            btn_Delete = new Button();
            btn_Edit = new Button();
            btn_Add = new Button();
            btn_Refresh = new Button();
            btn_ToggleMask = new Button();
            txt_Search = new TextBox();
            lbl_Search = new Label();
            dgv_Envs = new DataGridView();
            col_Key = new DataGridViewTextBoxColumn();
            col_Value = new DataGridViewTextBoxColumn();
            col_Type = new DataGridViewTextBoxColumn();
            toolTip1 = new ToolTip(components);
            pnl_Toolbar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgv_Envs).BeginInit();
            SuspendLayout();
            // 
            // pnl_Toolbar
            // 
            pnl_Toolbar.BackColor = Color.FromArgb(245, 246, 248);
            pnl_Toolbar.Controls.Add(btn_Delete);
            pnl_Toolbar.Controls.Add(btn_Edit);
            pnl_Toolbar.Controls.Add(btn_Add);
            pnl_Toolbar.Controls.Add(btn_Refresh);
            pnl_Toolbar.Controls.Add(btn_ToggleMask);
            pnl_Toolbar.Controls.Add(txt_Search);
            pnl_Toolbar.Controls.Add(lbl_Search);
            pnl_Toolbar.Dock = DockStyle.Top;
            pnl_Toolbar.Location = new Point(0, 0);
            pnl_Toolbar.Name = "pnl_Toolbar";
            pnl_Toolbar.Size = new Size(988, 48);
            pnl_Toolbar.TabIndex = 0;
            // 
            // btn_Delete
            // 
            btn_Delete.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btn_Delete.BackColor = Color.Coral;
            btn_Delete.Cursor = Cursors.Hand;
            btn_Delete.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btn_Delete.Location = new Point(895, 8);
            btn_Delete.Name = "btn_Delete";
            btn_Delete.Size = new Size(80, 32);
            btn_Delete.TabIndex = 6;
            btn_Delete.Text = "Xóa";
            toolTip1.SetToolTip(btn_Delete, "Xóa biến môi trường Machine");
            btn_Delete.UseVisualStyleBackColor = false;
            btn_Delete.Click += btn_Delete_Click;
            // 
            // btn_Edit
            // 
            btn_Edit.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btn_Edit.BackColor = Color.Gold;
            btn_Edit.Cursor = Cursors.Hand;
            btn_Edit.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btn_Edit.Location = new Point(810, 8);
            btn_Edit.Name = "btn_Edit";
            btn_Edit.Size = new Size(80, 32);
            btn_Edit.TabIndex = 5;
            btn_Edit.Text = "Sửa value";
            toolTip1.SetToolTip(btn_Edit, "Sửa giá trị biến đang chọn");
            btn_Edit.UseVisualStyleBackColor = false;
            btn_Edit.Click += btn_Edit_Click;
            // 
            // btn_Add
            // 
            btn_Add.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btn_Add.BackColor = Color.LimeGreen;
            btn_Add.Cursor = Cursors.Hand;
            btn_Add.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btn_Add.Location = new Point(725, 8);
            btn_Add.Name = "btn_Add";
            btn_Add.Size = new Size(80, 32);
            btn_Add.TabIndex = 4;
            btn_Add.Text = "Thêm";
            toolTip1.SetToolTip(btn_Add, "Thêm biến môi trường Machine mới");
            btn_Add.UseVisualStyleBackColor = false;
            btn_Add.Click += btn_Add_Click;
            // 
            // btn_Refresh
            // 
            btn_Refresh.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btn_Refresh.BackColor = Color.LightSkyBlue;
            btn_Refresh.Cursor = Cursors.Hand;
            btn_Refresh.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btn_Refresh.Location = new Point(640, 8);
            btn_Refresh.Name = "btn_Refresh";
            btn_Refresh.Size = new Size(80, 32);
            btn_Refresh.TabIndex = 3;
            btn_Refresh.Text = "Làm mới";
            toolTip1.SetToolTip(btn_Refresh, "Tải lại danh sách từ máy tính");
            btn_Refresh.UseVisualStyleBackColor = false;
            btn_Refresh.Click += btn_Refresh_Click;
            // 
            // btn_ToggleMask
            // 
            btn_ToggleMask.BackColor = Color.White;
            btn_ToggleMask.Cursor = Cursors.Hand;
            btn_ToggleMask.Font = new Font("Segoe UI", 9F);
            btn_ToggleMask.Location = new Point(290, 8);
            btn_ToggleMask.Name = "btn_ToggleMask";
            btn_ToggleMask.Size = new Size(100, 32);
            btn_ToggleMask.TabIndex = 2;
            btn_ToggleMask.Text = "Hiện giá trị";
            toolTip1.SetToolTip(btn_ToggleMask, "Hiện hoặc ẩn giá trị biến môi trường");
            btn_ToggleMask.UseVisualStyleBackColor = true;
            btn_ToggleMask.Click += btn_ToggleMask_Click;
            // 
            // txt_Search
            // 
            txt_Search.Location = new Point(80, 13);
            txt_Search.Name = "txt_Search";
            txt_Search.PlaceholderText = "Lọc theo Key...";
            txt_Search.Size = new Size(200, 23);
            txt_Search.TabIndex = 1;
            txt_Search.TextChanged += txt_Search_TextChanged;
            // 
            // lbl_Search
            // 
            lbl_Search.AutoSize = true;
            lbl_Search.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lbl_Search.Location = new Point(12, 16);
            lbl_Search.Name = "lbl_Search";
            lbl_Search.Size = new Size(62, 15);
            lbl_Search.TabIndex = 0;
            lbl_Search.Text = "Tìm kiếm:";
            // 
            // dgv_Envs
            // 
            dgv_Envs.AllowUserToAddRows = false;
            dgv_Envs.AllowUserToDeleteRows = false;
            dgv_Envs.AllowUserToResizeRows = false;
            dgv_Envs.BackgroundColor = Color.White;
            dgv_Envs.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgv_Envs.Columns.AddRange(new DataGridViewColumn[] { col_Key, col_Value, col_Type });
            dgv_Envs.Dock = DockStyle.Fill;
            dgv_Envs.Location = new Point(0, 48);
            dgv_Envs.MultiSelect = false;
            dgv_Envs.Name = "dgv_Envs";
            dgv_Envs.ReadOnly = true;
            dgv_Envs.RowHeadersVisible = false;
            dgv_Envs.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv_Envs.Size = new Size(988, 501);
            dgv_Envs.TabIndex = 1;
            dgv_Envs.CellDoubleClick += dgv_Envs_CellDoubleClick;
            dgv_Envs.CellFormatting += dgv_Envs_CellFormatting;
            // 
            // col_Key
            // 
            col_Key.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            col_Key.DataPropertyName = "Key";
            col_Key.FillWeight = 30F;
            col_Key.HeaderText = "Tên biến (Key)";
            col_Key.Name = "col_Key";
            col_Key.ReadOnly = true;
            // 
            // col_Value
            // 
            col_Value.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            col_Value.DataPropertyName = "DisplayValue";
            col_Value.FillWeight = 50F;
            col_Value.HeaderText = "Giá trị (Value)";
            col_Value.Name = "col_Value";
            col_Value.ReadOnly = true;
            // 
            // col_Type
            // 
            col_Type.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            col_Type.DataPropertyName = "TypeDescription";
            col_Type.FillWeight = 20F;
            col_Type.HeaderText = "Loại biến";
            col_Type.Name = "col_Type";
            col_Type.ReadOnly = true;
            // 
            // toolTip1
            // 
            toolTip1.IsBalloon = true;
            // 
            // UC_ConfigEnvCustom
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            Controls.Add(dgv_Envs);
            Controls.Add(pnl_Toolbar);
            Name = "UC_ConfigEnvCustom";
            Size = new Size(988, 549);
            Load += UC_ConfigEnvCustom_Load;
            pnl_Toolbar.ResumeLayout(false);
            pnl_Toolbar.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgv_Envs).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnl_Toolbar;
        private Label lbl_Search;
        private TextBox txt_Search;
        private Button btn_ToggleMask;
        private Button btn_Refresh;
        private Button btn_Add;
        private Button btn_Edit;
        private Button btn_Delete;
        private DataGridView dgv_Envs;
        private DataGridViewTextBoxColumn col_Key;
        private DataGridViewTextBoxColumn col_Value;
        private DataGridViewTextBoxColumn col_Type;
        private ToolTip toolTip1;
    }
}
