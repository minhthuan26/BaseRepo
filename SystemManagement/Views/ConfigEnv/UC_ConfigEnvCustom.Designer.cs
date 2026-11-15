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
            btn_Restore = new Button();
            btn_ToggleMask = new Button();
            txt_Search = new TextBox();
            lbl_Search = new Label();
            spc_Containers = new SplitContainer();
            dgv_System = new DataGridView();
            col_SysKey = new DataGridViewTextBoxColumn();
            col_SysValue = new DataGridViewTextBoxColumn();
            col_SysType = new DataGridViewTextBoxColumn();
            lbl_SystemTitle = new Label();
            dgv_User = new DataGridView();
            col_UserKey = new DataGridViewTextBoxColumn();
            col_UserValue = new DataGridViewTextBoxColumn();
            col_UserType = new DataGridViewTextBoxColumn();
            lbl_UserTitle = new Label();
            toolTip1 = new ToolTip(components);
            pnl_Toolbar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)spc_Containers).BeginInit();
            spc_Containers.Panel1.SuspendLayout();
            spc_Containers.Panel2.SuspendLayout();
            spc_Containers.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgv_System).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgv_User).BeginInit();
            SuspendLayout();
            // 
            // pnl_Toolbar
            // 
            pnl_Toolbar.BackColor = Color.FromArgb(245, 246, 248);
            pnl_Toolbar.Controls.Add(btn_Delete);
            pnl_Toolbar.Controls.Add(btn_Edit);
            pnl_Toolbar.Controls.Add(btn_Add);
            pnl_Toolbar.Controls.Add(btn_Refresh);
            pnl_Toolbar.Controls.Add(btn_Restore);
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
            btn_Delete.TabIndex = 7;
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
            btn_Edit.TabIndex = 6;
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
            btn_Add.TabIndex = 5;
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
            btn_Refresh.TabIndex = 4;
            btn_Refresh.Text = "Làm mới";
            toolTip1.SetToolTip(btn_Refresh, "Tải lại danh sách từ máy tính");
            btn_Refresh.UseVisualStyleBackColor = false;
            btn_Refresh.Click += btn_Refresh_Click;
            // 
            // btn_Restore
            // 
            btn_Restore.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btn_Restore.BackColor = Color.SandyBrown;
            btn_Restore.Cursor = Cursors.Hand;
            btn_Restore.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btn_Restore.Location = new Point(555, 8);
            btn_Restore.Name = "btn_Restore";
            btn_Restore.Size = new Size(80, 32);
            btn_Restore.TabIndex = 3;
            btn_Restore.Text = "Khôi phục";
            toolTip1.SetToolTip(btn_Restore, "Khôi phục biến cốt lõi từ bản sao lưu");
            btn_Restore.UseVisualStyleBackColor = false;
            btn_Restore.Click += btn_Restore_Click;
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
            // spc_Containers
            // 
            spc_Containers.Dock = DockStyle.Fill;
            spc_Containers.Location = new Point(0, 48);
            spc_Containers.Name = "spc_Containers";
            spc_Containers.Orientation = Orientation.Horizontal;
            // 
            // spc_Containers.Panel1
            // 
            spc_Containers.Panel1.Controls.Add(dgv_System);
            spc_Containers.Panel1.Controls.Add(lbl_SystemTitle);
            // 
            // spc_Containers.Panel2
            // 
            spc_Containers.Panel2.Controls.Add(dgv_User);
            spc_Containers.Panel2.Controls.Add(lbl_UserTitle);
            spc_Containers.Size = new Size(988, 501);
            spc_Containers.SplitterDistance = 240;
            spc_Containers.SplitterWidth = 4;
            spc_Containers.TabIndex = 1;
            // 
            // lbl_SystemTitle
            // 
            lbl_SystemTitle.BackColor = Color.FromArgb(240, 243, 246);
            lbl_SystemTitle.Dock = DockStyle.Top;
            lbl_SystemTitle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lbl_SystemTitle.Location = new Point(0, 0);
            lbl_SystemTitle.Name = "lbl_SystemTitle";
            lbl_SystemTitle.Padding = new Padding(8, 0, 0, 0);
            lbl_SystemTitle.Size = new Size(988, 26);
            lbl_SystemTitle.TabIndex = 0;
            lbl_SystemTitle.Text = "▶ Biến môi trường Hệ thống (System / Machine)";
            lbl_SystemTitle.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // dgv_System
            // 
            dgv_System.AllowUserToAddRows = false;
            dgv_System.AllowUserToDeleteRows = false;
            dgv_System.AllowUserToResizeRows = false;
            dgv_System.AutoGenerateColumns = false;
            dgv_System.BackgroundColor = Color.White;
            dgv_System.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgv_System.Columns.AddRange(new DataGridViewColumn[] { col_SysKey, col_SysValue, col_SysType });
            dgv_System.Dock = DockStyle.Fill;
            dgv_System.Location = new Point(0, 26);
            dgv_System.MultiSelect = false;
            dgv_System.Name = "dgv_System";
            dgv_System.ReadOnly = true;
            dgv_System.RowHeadersVisible = false;
            dgv_System.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv_System.Size = new Size(988, 214);
            dgv_System.TabIndex = 1;
            dgv_System.CellDoubleClick += dgv_System_CellDoubleClick;
            dgv_System.CellFormatting += dgv_System_CellFormatting;
            dgv_System.Enter += dgv_System_Enter;
            // 
            // col_SysKey
            // 
            col_SysKey.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            col_SysKey.DataPropertyName = "Key";
            col_SysKey.FillWeight = 30F;
            col_SysKey.HeaderText = "Tên biến (Key)";
            col_SysKey.Name = "col_SysKey";
            col_SysKey.ReadOnly = true;
            // 
            // col_SysValue
            // 
            col_SysValue.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            col_SysValue.DataPropertyName = "DisplayValue";
            col_SysValue.FillWeight = 50F;
            col_SysValue.HeaderText = "Giá trị (Value)";
            col_SysValue.Name = "col_SysValue";
            col_SysValue.ReadOnly = true;
            // 
            // col_SysType
            // 
            col_SysType.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            col_SysType.DataPropertyName = "TypeDescription";
            col_SysType.FillWeight = 20F;
            col_SysType.HeaderText = "Loại biến";
            col_SysType.Name = "col_SysType";
            col_SysType.ReadOnly = true;
            // 
            // lbl_UserTitle
            // 
            lbl_UserTitle.BackColor = Color.FromArgb(240, 243, 246);
            lbl_UserTitle.Dock = DockStyle.Top;
            lbl_UserTitle.Font = new Font("Segoe UI", 9F);
            lbl_UserTitle.Location = new Point(0, 0);
            lbl_UserTitle.Name = "lbl_UserTitle";
            lbl_UserTitle.Padding = new Padding(8, 0, 0, 0);
            lbl_UserTitle.Size = new Size(988, 26);
            lbl_UserTitle.TabIndex = 0;
            lbl_UserTitle.Text = "   Biến môi trường Người dùng (User)";
            lbl_UserTitle.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // dgv_User
            // 
            dgv_User.AllowUserToAddRows = false;
            dgv_User.AllowUserToDeleteRows = false;
            dgv_User.AllowUserToResizeRows = false;
            dgv_User.AutoGenerateColumns = false;
            dgv_User.BackgroundColor = Color.White;
            dgv_User.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgv_User.Columns.AddRange(new DataGridViewColumn[] { col_UserKey, col_UserValue, col_UserType });
            dgv_User.Dock = DockStyle.Fill;
            dgv_User.Location = new Point(0, 26);
            dgv_User.MultiSelect = false;
            dgv_User.Name = "dgv_User";
            dgv_User.ReadOnly = true;
            dgv_User.RowHeadersVisible = false;
            dgv_User.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv_User.Size = new Size(988, 231);
            dgv_User.TabIndex = 1;
            dgv_User.CellDoubleClick += dgv_User_CellDoubleClick;
            dgv_User.CellFormatting += dgv_User_CellFormatting;
            dgv_User.Enter += dgv_User_Enter;
            // 
            // col_UserKey
            // 
            col_UserKey.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            col_UserKey.DataPropertyName = "Key";
            col_UserKey.FillWeight = 30F;
            col_UserKey.HeaderText = "Tên biến (Key)";
            col_UserKey.Name = "col_UserKey";
            col_UserKey.ReadOnly = true;
            // 
            // col_UserValue
            // 
            col_UserValue.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            col_UserValue.DataPropertyName = "DisplayValue";
            col_UserValue.FillWeight = 50F;
            col_UserValue.HeaderText = "Giá trị (Value)";
            col_UserValue.Name = "col_UserValue";
            col_UserValue.ReadOnly = true;
            // 
            // col_UserType
            // 
            col_UserType.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            col_UserType.DataPropertyName = "TypeDescription";
            col_UserType.FillWeight = 20F;
            col_UserType.HeaderText = "Loại biến";
            col_UserType.Name = "col_UserType";
            col_UserType.ReadOnly = true;
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
            Controls.Add(spc_Containers);
            Controls.Add(pnl_Toolbar);
            Name = "UC_ConfigEnvCustom";
            Size = new Size(988, 549);
            Load += UC_ConfigEnvCustom_Load;
            pnl_Toolbar.ResumeLayout(false);
            pnl_Toolbar.PerformLayout();
            spc_Containers.Panel1.ResumeLayout(false);
            spc_Containers.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)spc_Containers).EndInit();
            spc_Containers.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgv_System).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgv_User).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnl_Toolbar;
        private Label lbl_Search;
        private TextBox txt_Search;
        private Button btn_ToggleMask;
        private Button btn_Restore;
        private Button btn_Refresh;
        private Button btn_Add;
        private Button btn_Edit;
        private Button btn_Delete;
        private SplitContainer spc_Containers;
        private Label lbl_SystemTitle;
        private DataGridView dgv_System;
        private DataGridViewTextBoxColumn col_SysKey;
        private DataGridViewTextBoxColumn col_SysValue;
        private DataGridViewTextBoxColumn col_SysType;
        private Label lbl_UserTitle;
        private DataGridView dgv_User;
        private DataGridViewTextBoxColumn col_UserKey;
        private DataGridViewTextBoxColumn col_UserValue;
        private DataGridViewTextBoxColumn col_UserType;
        private ToolTip toolTip1;
    }
}
