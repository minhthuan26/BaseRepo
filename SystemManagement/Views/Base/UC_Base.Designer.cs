namespace SystemManagement.Views.Base
{
    partial class UC_Base
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UC_Base));
            tableLayoutPanel1 = new TableLayoutPanel();
            panel1 = new Panel();
            panel2 = new Panel();
            picBox_Back = new PictureBox();
            lbl_Title = new Label();
            pnl_PageMain = new Panel();
            toolTip1 = new ToolTip(components);
            tableLayoutPanel1.SuspendLayout();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picBox_Back).BeginInit();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.Controls.Add(panel1, 0, 0);
            tableLayoutPanel1.Controls.Add(pnl_PageMain, 0, 1);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 2;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Size = new Size(1002, 674);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // panel1
            // 
            panel1.Controls.Add(panel2);
            panel1.Controls.Add(picBox_Back);
            panel1.Controls.Add(lbl_Title);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(3, 3);
            panel1.Name = "panel1";
            panel1.Size = new Size(996, 54);
            panel1.TabIndex = 0;
            // 
            // panel2
            // 
            panel2.BackColor = SystemColors.ActiveCaptionText;
            panel2.Dock = DockStyle.Bottom;
            panel2.Location = new Point(0, 53);
            panel2.Name = "panel2";
            panel2.Size = new Size(996, 1);
            panel2.TabIndex = 1;
            // 
            // picBox_Back
            // 
            picBox_Back.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            picBox_Back.Cursor = Cursors.Hand;
            picBox_Back.Image = (Image)resources.GetObject("picBox_Back.Image");
            picBox_Back.Location = new Point(0, 0);
            picBox_Back.Name = "picBox_Back";
            picBox_Back.Size = new Size(52, 54);
            picBox_Back.TabIndex = 1;
            picBox_Back.TabStop = false;
            toolTip1.SetToolTip(picBox_Back, "Back");
            picBox_Back.Click += picBox_Back_Click;
            // 
            // lbl_Title
            // 
            lbl_Title.Dock = DockStyle.Fill;
            lbl_Title.Font = new Font("Segoe UI", 20F, FontStyle.Bold, GraphicsUnit.Point);
            lbl_Title.Location = new Point(0, 0);
            lbl_Title.Name = "lbl_Title";
            lbl_Title.Size = new Size(996, 54);
            lbl_Title.TabIndex = 0;
            lbl_Title.Text = "Trang chủ";
            lbl_Title.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // pnl_PageMain
            // 
            pnl_PageMain.Dock = DockStyle.Fill;
            pnl_PageMain.Location = new Point(3, 63);
            pnl_PageMain.Name = "pnl_PageMain";
            pnl_PageMain.Size = new Size(996, 608);
            pnl_PageMain.TabIndex = 1;
            // 
            // toolTip1
            // 
            toolTip1.IsBalloon = true;
            // 
            // UC_Base
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(tableLayoutPanel1);
            Name = "UC_Base";
            Size = new Size(1002, 674);
            Load += UC_Base_Load;
            tableLayoutPanel1.ResumeLayout(false);
            panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picBox_Back).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tableLayoutPanel1;
        private Panel panel1;
        private Label lbl_Title;
        private PictureBox picBox_Back;
        private Panel panel2;
        protected Panel pnl_PageMain;
        private ToolTip toolTip1;
    }
}
