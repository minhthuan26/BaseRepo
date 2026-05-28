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
            pnl_Main = new Panel();
            progressBar1 = new ProgressBar();
            menuStrip1 = new MenuStrip();
            homeToolStripMenuItem = new ToolStripMenuItem();
            registModuleToolStripMenuItem = new ToolStripMenuItem();
            registIISToolStripMenuItem = new ToolStripMenuItem();
            centerConnectToolStripMenuItem = new ToolStripMenuItem();
            aboutToolStripMenuItem = new ToolStripMenuItem();
            tstrip_Main = new ToolStrip();
            tstrip_LblVersion = new ToolStripLabel();
            toolStripSeparator1 = new ToolStripSeparator();
            tstrip_LblTime = new ToolStripLabel();
            pnl_Main.SuspendLayout();
            menuStrip1.SuspendLayout();
            tstrip_Main.SuspendLayout();
            SuspendLayout();
            // 
            // pnl_Main
            // 
            pnl_Main.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            pnl_Main.BackColor = Color.White;
            pnl_Main.Controls.Add(progressBar1);
            pnl_Main.Location = new Point(3, 27);
            pnl_Main.Name = "pnl_Main";
            pnl_Main.Size = new Size(1002, 674);
            pnl_Main.TabIndex = 0;
            // 
            // progressBar1
            // 
            progressBar1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            progressBar1.Location = new Point(251, 265);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(500, 24);
            progressBar1.TabIndex = 0;
            progressBar1.Visible = false;
            // 
            // menuStrip1
            // 
            menuStrip1.BackColor = Color.LightSkyBlue;
            menuStrip1.Items.AddRange(new ToolStripItem[] { homeToolStripMenuItem, registModuleToolStripMenuItem, registIISToolStripMenuItem, centerConnectToolStripMenuItem, aboutToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(1008, 24);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // homeToolStripMenuItem
            // 
            homeToolStripMenuItem.Image = (Image)resources.GetObject("homeToolStripMenuItem.Image");
            homeToolStripMenuItem.Name = "homeToolStripMenuItem";
            homeToolStripMenuItem.Size = new Size(88, 20);
            homeToolStripMenuItem.Text = "Trang chủ";
            homeToolStripMenuItem.Click += homeToolStripMenuItem_Click;
            // 
            // registModuleToolStripMenuItem
            // 
            registModuleToolStripMenuItem.Image = (Image)resources.GetObject("registModuleToolStripMenuItem.Image");
            registModuleToolStripMenuItem.Name = "registModuleToolStripMenuItem";
            registModuleToolStripMenuItem.Size = new Size(122, 20);
            registModuleToolStripMenuItem.Text = "Đăng ký module";
            // 
            // registIISToolStripMenuItem
            // 
            registIISToolStripMenuItem.Image = (Image)resources.GetObject("registIISToolStripMenuItem.Image");
            registIISToolStripMenuItem.Name = "registIISToolStripMenuItem";
            registIISToolStripMenuItem.Size = new Size(93, 20);
            registIISToolStripMenuItem.Text = "Đăng ký IIS";
            // 
            // centerConnectToolStripMenuItem
            // 
            centerConnectToolStripMenuItem.Image = (Image)resources.GetObject("centerConnectToolStripMenuItem.Image");
            centerConnectToolStripMenuItem.Name = "centerConnectToolStripMenuItem";
            centerConnectToolStripMenuItem.Size = new Size(124, 20);
            centerConnectToolStripMenuItem.Text = "Kết nối tập trung";
            // 
            // aboutToolStripMenuItem
            // 
            aboutToolStripMenuItem.Image = (Image)resources.GetObject("aboutToolStripMenuItem.Image");
            aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            aboutToolStripMenuItem.Size = new Size(87, 20);
            aboutToolStripMenuItem.Text = "Thông tin";
            aboutToolStripMenuItem.Click += aboutToolStripMenuItem_Click;
            // 
            // tstrip_Main
            // 
            tstrip_Main.BackColor = Color.LightSkyBlue;
            tstrip_Main.Dock = DockStyle.Bottom;
            tstrip_Main.Items.AddRange(new ToolStripItem[] { tstrip_LblVersion, toolStripSeparator1, tstrip_LblTime });
            tstrip_Main.Location = new Point(0, 704);
            tstrip_Main.Name = "tstrip_Main";
            tstrip_Main.RenderMode = ToolStripRenderMode.Professional;
            tstrip_Main.RightToLeft = RightToLeft.Yes;
            tstrip_Main.Size = new Size(1008, 25);
            tstrip_Main.TabIndex = 0;
            tstrip_Main.Text = "toolStrip1";
            // 
            // tstrip_LblVersion
            // 
            tstrip_LblVersion.Name = "tstrip_LblVersion";
            tstrip_LblVersion.Size = new Size(45, 22);
            tstrip_LblVersion.Text = "Version";
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 25);
            // 
            // tstrip_LblTime
            // 
            tstrip_LblTime.Name = "tstrip_LblTime";
            tstrip_LblTime.Size = new Size(75, 22);
            tstrip_LblTime.Text = "System Time";
            // 
            // FrmMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1008, 729);
            Controls.Add(menuStrip1);
            Controls.Add(pnl_Main);
            Controls.Add(tstrip_Main);
            Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = menuStrip1;
            MaximumSize = new Size(1920, 1080);
            MinimumSize = new Size(1024, 768);
            Name = "FrmMain";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "QUẢN LÍ HỆ THỐNG";
            FormClosing += FrmHome_FormClosing;
            Load += FrmHome_Load;
            pnl_Main.ResumeLayout(false);
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            tstrip_Main.ResumeLayout(false);
            tstrip_Main.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Panel pnl_Main;
        private ToolStrip tstrip_Main;
        private ToolStripLabel tstrip_LblVersion;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripLabel tstrip_LblTime;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem homeToolStripMenuItem;
        private ToolStripMenuItem aboutToolStripMenuItem;
        private ProgressBar progressBar1;
        private ToolStripMenuItem registModuleToolStripMenuItem;
        private ToolStripMenuItem registIISToolStripMenuItem;
        private ToolStripMenuItem centerConnectToolStripMenuItem;
    }
}