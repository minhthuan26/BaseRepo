namespace SystemManagement.Views
{
    partial class FrmMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmMain));
            tpnl_Main = new TableLayoutPanel();
            pnl_Main = new Panel();
            pnl_Header = new Panel();
            menuStrip1 = new MenuStrip();
            homeToolStripMenuItem = new ToolStripMenuItem();
            thôngTinToolStripMenuItem = new ToolStripMenuItem();
            pnl_Footer = new Panel();
            tstrip_Main = new ToolStrip();
            tstrip_LblVersion = new ToolStripLabel();
            toolStripSeparator1 = new ToolStripSeparator();
            tstrip_LblTime = new ToolStripLabel();
            tpnl_Main.SuspendLayout();
            pnl_Header.SuspendLayout();
            menuStrip1.SuspendLayout();
            pnl_Footer.SuspendLayout();
            tstrip_Main.SuspendLayout();
            SuspendLayout();
            // 
            // tpnl_Main
            // 
            tpnl_Main.ColumnCount = 1;
            tpnl_Main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tpnl_Main.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            tpnl_Main.Controls.Add(pnl_Main, 0, 1);
            tpnl_Main.Controls.Add(pnl_Header, 0, 0);
            tpnl_Main.Controls.Add(pnl_Footer, 0, 2);
            tpnl_Main.Dock = DockStyle.Fill;
            tpnl_Main.Location = new Point(0, 0);
            tpnl_Main.Name = "tpnl_Main";
            tpnl_Main.RowCount = 3;
            tpnl_Main.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            tpnl_Main.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tpnl_Main.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            tpnl_Main.Size = new Size(1008, 729);
            tpnl_Main.TabIndex = 0;
            // 
            // pnl_Main
            // 
            pnl_Main.Dock = DockStyle.Fill;
            pnl_Main.Location = new Point(3, 43);
            pnl_Main.Name = "pnl_Main";
            pnl_Main.Size = new Size(1002, 643);
            pnl_Main.TabIndex = 0;
            // 
            // pnl_Header
            // 
            pnl_Header.Controls.Add(menuStrip1);
            pnl_Header.Dock = DockStyle.Fill;
            pnl_Header.Location = new Point(3, 3);
            pnl_Header.Name = "pnl_Header";
            pnl_Header.Size = new Size(1002, 34);
            pnl_Header.TabIndex = 1;
            // 
            // menuStrip1
            // 
            menuStrip1.BackColor = Color.LightSkyBlue;
            menuStrip1.Dock = DockStyle.Fill;
            menuStrip1.Items.AddRange(new ToolStripItem[] { homeToolStripMenuItem, thôngTinToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(1002, 34);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // homeToolStripMenuItem
            // 
            homeToolStripMenuItem.Image = (Image)resources.GetObject("homeToolStripMenuItem.Image");
            homeToolStripMenuItem.Name = "homeToolStripMenuItem";
            homeToolStripMenuItem.Size = new Size(88, 30);
            homeToolStripMenuItem.Text = "Trang chủ";
            // 
            // thôngTinToolStripMenuItem
            // 
            thôngTinToolStripMenuItem.Image = (Image)resources.GetObject("thôngTinToolStripMenuItem.Image");
            thôngTinToolStripMenuItem.Name = "thôngTinToolStripMenuItem";
            thôngTinToolStripMenuItem.Size = new Size(87, 30);
            thôngTinToolStripMenuItem.Text = "Thông tin";
            // 
            // pnl_Footer
            // 
            pnl_Footer.Controls.Add(tstrip_Main);
            pnl_Footer.Dock = DockStyle.Fill;
            pnl_Footer.Location = new Point(3, 692);
            pnl_Footer.Name = "pnl_Footer";
            pnl_Footer.Size = new Size(1002, 34);
            pnl_Footer.TabIndex = 2;
            // 
            // tstrip_Main
            // 
            tstrip_Main.BackColor = Color.LightSkyBlue;
            tstrip_Main.Dock = DockStyle.Fill;
            tstrip_Main.Items.AddRange(new ToolStripItem[] { tstrip_LblVersion, toolStripSeparator1, tstrip_LblTime });
            tstrip_Main.Location = new Point(0, 0);
            tstrip_Main.Name = "tstrip_Main";
            tstrip_Main.RenderMode = ToolStripRenderMode.Professional;
            tstrip_Main.RightToLeft = RightToLeft.Yes;
            tstrip_Main.Size = new Size(1002, 34);
            tstrip_Main.TabIndex = 0;
            tstrip_Main.Text = "toolStrip1";
            // 
            // tstrip_LblVersion
            // 
            tstrip_LblVersion.Name = "tstrip_LblVersion";
            tstrip_LblVersion.Size = new Size(45, 31);
            tstrip_LblVersion.Text = "Version";
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 34);
            // 
            // tstrip_LblTime
            // 
            tstrip_LblTime.Name = "tstrip_LblTime";
            tstrip_LblTime.Size = new Size(75, 31);
            tstrip_LblTime.Text = "System Time";
            // 
            // FrmMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1008, 729);
            Controls.Add(tpnl_Main);
            Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = menuStrip1;
            MaximumSize = new Size(1920, 1080);
            MinimumSize = new Size(1024, 768);
            Name = "FrmMain";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "QUẢN LÍ HỆ THỐNG";
            WindowState = FormWindowState.Maximized;
            FormClosing += FrmHome_FormClosing;
            Load += FrmHome_Load;
            tpnl_Main.ResumeLayout(false);
            pnl_Header.ResumeLayout(false);
            pnl_Header.PerformLayout();
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            pnl_Footer.ResumeLayout(false);
            pnl_Footer.PerformLayout();
            tstrip_Main.ResumeLayout(false);
            tstrip_Main.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tpnl_Main;
        private Panel pnl_Main;
        private Panel pnl_Header;
        private Panel pnl_Footer;
        private ToolStrip tstrip_Main;
        private ToolStripLabel tstrip_LblVersion;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripLabel tstrip_LblTime;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem homeToolStripMenuItem;
        private ToolStripMenuItem thôngTinToolStripMenuItem;
    }
}