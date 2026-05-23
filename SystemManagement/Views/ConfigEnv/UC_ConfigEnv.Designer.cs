namespace SystemManagement.Views.ConfigEnv
{
    partial class UC_ConfigEnv
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
            panel1 = new Panel();
            label1 = new Label();
            panel2 = new Panel();
            panel9 = new Panel();
            button2 = new Button();
            button1 = new Button();
            btn_AutoAll = new Button();
            btn_Save = new Button();
            panel8 = new Panel();
            btn_LoadPublicKeyPath = new Button();
            btn_RandomPublicKeyPath = new Button();
            label7 = new Label();
            txt_PublicKeyPath = new TextBox();
            panel7 = new Panel();
            btn_LoadPrivateKeyPath = new Button();
            btn_RandomPrivateKeyPath = new Button();
            txt_PrivateKeyPath = new TextBox();
            label6 = new Label();
            panel6 = new Panel();
            btn_RandomIssuer = new Button();
            label5 = new Label();
            txt_Issuer = new TextBox();
            panel5 = new Panel();
            btn_RandomAudience = new Button();
            label4 = new Label();
            txt_Audience = new TextBox();
            panel4 = new Panel();
            btn_RandomRefreshKey = new Button();
            label3 = new Label();
            txt_RefreshKey = new TextBox();
            panel3 = new Panel();
            btn_RandomAccessKey = new Button();
            txt_AccessKey = new TextBox();
            label2 = new Label();
            toolTip1 = new ToolTip(components);
            panel1.SuspendLayout();
            panel2.SuspendLayout();
            panel9.SuspendLayout();
            panel8.SuspendLayout();
            panel7.SuspendLayout();
            panel6.SuspendLayout();
            panel5.SuspendLayout();
            panel4.SuspendLayout();
            panel3.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.Controls.Add(label1);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(3, 3);
            panel1.Name = "panel1";
            panel1.Size = new Size(996, 34);
            panel1.TabIndex = 0;
            // 
            // label1
            // 
            label1.Dock = DockStyle.Fill;
            label1.Font = new Font("Segoe UI", 15F, FontStyle.Bold, GraphicsUnit.Point);
            label1.Location = new Point(0, 0);
            label1.Name = "label1";
            label1.Size = new Size(996, 34);
            label1.TabIndex = 0;
            label1.Text = "Cấu hình Environment Variable";
            label1.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // panel2
            // 
            panel2.BackColor = Color.White;
            panel2.Controls.Add(panel9);
            panel2.Controls.Add(panel8);
            panel2.Controls.Add(panel7);
            panel2.Controls.Add(panel6);
            panel2.Controls.Add(panel5);
            panel2.Controls.Add(panel4);
            panel2.Controls.Add(panel3);
            panel2.Dock = DockStyle.Fill;
            panel2.Location = new Point(0, 0);
            panel2.Name = "panel2";
            panel2.Size = new Size(996, 577);
            panel2.TabIndex = 1;
            // 
            // panel9
            // 
            panel9.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panel9.Controls.Add(button2);
            panel9.Controls.Add(button1);
            panel9.Controls.Add(btn_AutoAll);
            panel9.Controls.Add(btn_Save);
            panel9.Location = new Point(3, 513);
            panel9.Name = "panel9";
            panel9.Size = new Size(990, 45);
            panel9.TabIndex = 9;
            // 
            // button2
            // 
            button2.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            button2.BackColor = Color.Coral;
            button2.Cursor = Cursors.Hand;
            button2.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            button2.Location = new Point(528, 3);
            button2.Name = "button2";
            button2.Size = new Size(134, 39);
            button2.TabIndex = 12;
            button2.Text = "Tải lên tập trung";
            toolTip1.SetToolTip(button2, "Auto Create All");
            button2.UseVisualStyleBackColor = false;
            // 
            // button1
            // 
            button1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            button1.BackColor = Color.MediumSpringGreen;
            button1.Cursor = Cursors.Hand;
            button1.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            button1.Location = new Point(668, 3);
            button1.Name = "button1";
            button1.Size = new Size(134, 39);
            button1.TabIndex = 11;
            button1.Text = "Đồng bộ về cục bộ";
            toolTip1.SetToolTip(button1, "Auto Create All");
            button1.UseVisualStyleBackColor = false;
            // 
            // btn_AutoAll
            // 
            btn_AutoAll.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btn_AutoAll.BackColor = Color.Gold;
            btn_AutoAll.Cursor = Cursors.Hand;
            btn_AutoAll.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            btn_AutoAll.Location = new Point(808, 3);
            btn_AutoAll.Name = "btn_AutoAll";
            btn_AutoAll.Size = new Size(116, 39);
            btn_AutoAll.TabIndex = 10;
            btn_AutoAll.Text = "Tự động tất cả";
            toolTip1.SetToolTip(btn_AutoAll, "Auto Create All");
            btn_AutoAll.UseVisualStyleBackColor = false;
            btn_AutoAll.Click += btn_AutoAll_Click;
            // 
            // btn_Save
            // 
            btn_Save.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btn_Save.BackColor = Color.LimeGreen;
            btn_Save.Cursor = Cursors.Hand;
            btn_Save.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            btn_Save.Location = new Point(927, 3);
            btn_Save.Name = "btn_Save";
            btn_Save.Size = new Size(57, 39);
            btn_Save.TabIndex = 9;
            btn_Save.Text = "Lưu";
            toolTip1.SetToolTip(btn_Save, "Save");
            btn_Save.UseVisualStyleBackColor = false;
            btn_Save.Click += btn_Save_Click;
            // 
            // panel8
            // 
            panel8.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panel8.Controls.Add(btn_LoadPublicKeyPath);
            panel8.Controls.Add(btn_RandomPublicKeyPath);
            panel8.Controls.Add(label7);
            panel8.Controls.Add(txt_PublicKeyPath);
            panel8.Location = new Point(3, 428);
            panel8.Name = "panel8";
            panel8.Size = new Size(990, 79);
            panel8.TabIndex = 5;
            // 
            // btn_LoadPublicKeyPath
            // 
            btn_LoadPublicKeyPath.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btn_LoadPublicKeyPath.BackColor = Color.LightSkyBlue;
            btn_LoadPublicKeyPath.Cursor = Cursors.Hand;
            btn_LoadPublicKeyPath.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            btn_LoadPublicKeyPath.Location = new Point(927, 13);
            btn_LoadPublicKeyPath.Name = "btn_LoadPublicKeyPath";
            btn_LoadPublicKeyPath.Size = new Size(57, 31);
            btn_LoadPublicKeyPath.TabIndex = 8;
            btn_LoadPublicKeyPath.Text = "...";
            toolTip1.SetToolTip(btn_LoadPublicKeyPath, "Load Public Key");
            btn_LoadPublicKeyPath.UseVisualStyleBackColor = false;
            btn_LoadPublicKeyPath.Click += btn_LoadPublicKeyPath_Click;
            // 
            // btn_RandomPublicKeyPath
            // 
            btn_RandomPublicKeyPath.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btn_RandomPublicKeyPath.BackColor = Color.Yellow;
            btn_RandomPublicKeyPath.Cursor = Cursors.Hand;
            btn_RandomPublicKeyPath.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            btn_RandomPublicKeyPath.Location = new Point(927, 45);
            btn_RandomPublicKeyPath.Name = "btn_RandomPublicKeyPath";
            btn_RandomPublicKeyPath.Size = new Size(57, 31);
            btn_RandomPublicKeyPath.TabIndex = 8;
            btn_RandomPublicKeyPath.Text = "Auto";
            toolTip1.SetToolTip(btn_RandomPublicKeyPath, "Generate Public Key");
            btn_RandomPublicKeyPath.UseVisualStyleBackColor = false;
            btn_RandomPublicKeyPath.Click += btn_RandomPublicKeyPath_Click;
            // 
            // label7
            // 
            label7.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label7.Location = new Point(0, 0);
            label7.Name = "label7";
            label7.Size = new Size(103, 20);
            label7.TabIndex = 5;
            label7.Text = "Public Key Path";
            // 
            // txt_PublicKeyPath
            // 
            txt_PublicKeyPath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txt_PublicKeyPath.Location = new Point(0, 18);
            txt_PublicKeyPath.Multiline = true;
            txt_PublicKeyPath.Name = "txt_PublicKeyPath";
            txt_PublicKeyPath.Size = new Size(924, 58);
            txt_PublicKeyPath.TabIndex = 7;
            txt_PublicKeyPath.TextChanged += txt_PublicKeyPath_TextChanged;
            // 
            // panel7
            // 
            panel7.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panel7.Controls.Add(btn_LoadPrivateKeyPath);
            panel7.Controls.Add(btn_RandomPrivateKeyPath);
            panel7.Controls.Add(txt_PrivateKeyPath);
            panel7.Controls.Add(label6);
            panel7.Location = new Point(0, 343);
            panel7.Name = "panel7";
            panel7.Size = new Size(993, 79);
            panel7.TabIndex = 4;
            // 
            // btn_LoadPrivateKeyPath
            // 
            btn_LoadPrivateKeyPath.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btn_LoadPrivateKeyPath.BackColor = Color.LightSkyBlue;
            btn_LoadPrivateKeyPath.Cursor = Cursors.Hand;
            btn_LoadPrivateKeyPath.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            btn_LoadPrivateKeyPath.Location = new Point(930, 13);
            btn_LoadPrivateKeyPath.Name = "btn_LoadPrivateKeyPath";
            btn_LoadPrivateKeyPath.Size = new Size(57, 31);
            btn_LoadPrivateKeyPath.TabIndex = 7;
            btn_LoadPrivateKeyPath.Text = "...";
            toolTip1.SetToolTip(btn_LoadPrivateKeyPath, "Load Private Key");
            btn_LoadPrivateKeyPath.UseVisualStyleBackColor = false;
            btn_LoadPrivateKeyPath.Click += btn_LoadPrivateKeyPath_Click;
            // 
            // btn_RandomPrivateKeyPath
            // 
            btn_RandomPrivateKeyPath.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btn_RandomPrivateKeyPath.BackColor = Color.Yellow;
            btn_RandomPrivateKeyPath.Cursor = Cursors.Hand;
            btn_RandomPrivateKeyPath.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            btn_RandomPrivateKeyPath.Location = new Point(930, 45);
            btn_RandomPrivateKeyPath.Name = "btn_RandomPrivateKeyPath";
            btn_RandomPrivateKeyPath.Size = new Size(57, 31);
            btn_RandomPrivateKeyPath.TabIndex = 6;
            btn_RandomPrivateKeyPath.Text = "Auto";
            toolTip1.SetToolTip(btn_RandomPrivateKeyPath, "Generate Private Key");
            btn_RandomPrivateKeyPath.UseVisualStyleBackColor = false;
            btn_RandomPrivateKeyPath.Click += btn_RandomPrivateKeyPath_Click;
            // 
            // txt_PrivateKeyPath
            // 
            txt_PrivateKeyPath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txt_PrivateKeyPath.Location = new Point(0, 18);
            txt_PrivateKeyPath.Multiline = true;
            txt_PrivateKeyPath.Name = "txt_PrivateKeyPath";
            txt_PrivateKeyPath.Size = new Size(927, 58);
            txt_PrivateKeyPath.TabIndex = 5;
            txt_PrivateKeyPath.TextChanged += txt_PrivateKeyPath_TextChanged;
            // 
            // label6
            // 
            label6.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label6.Location = new Point(0, 0);
            label6.Name = "label6";
            label6.Size = new Size(103, 20);
            label6.TabIndex = 4;
            label6.Text = "Private Key Path";
            // 
            // panel6
            // 
            panel6.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panel6.Controls.Add(btn_RandomIssuer);
            panel6.Controls.Add(label5);
            panel6.Controls.Add(txt_Issuer);
            panel6.Location = new Point(3, 258);
            panel6.Name = "panel6";
            panel6.Size = new Size(990, 79);
            panel6.TabIndex = 3;
            // 
            // btn_RandomIssuer
            // 
            btn_RandomIssuer.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btn_RandomIssuer.BackColor = Color.Yellow;
            btn_RandomIssuer.Cursor = Cursors.Hand;
            btn_RandomIssuer.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            btn_RandomIssuer.Location = new Point(927, 45);
            btn_RandomIssuer.Name = "btn_RandomIssuer";
            btn_RandomIssuer.Size = new Size(57, 31);
            btn_RandomIssuer.TabIndex = 4;
            btn_RandomIssuer.Text = "Auto";
            toolTip1.SetToolTip(btn_RandomIssuer, "Random Issuer");
            btn_RandomIssuer.UseVisualStyleBackColor = false;
            btn_RandomIssuer.Click += btn_RandomIssuer_Click;
            // 
            // label5
            // 
            label5.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label5.Location = new Point(0, 0);
            label5.Name = "label5";
            label5.Size = new Size(75, 20);
            label5.TabIndex = 3;
            label5.Text = "Issuer";
            // 
            // txt_Issuer
            // 
            txt_Issuer.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txt_Issuer.Location = new Point(0, 23);
            txt_Issuer.Multiline = true;
            txt_Issuer.Name = "txt_Issuer";
            txt_Issuer.Size = new Size(924, 53);
            txt_Issuer.TabIndex = 3;
            txt_Issuer.TextChanged += txt_Issuer_TextChanged;
            // 
            // panel5
            // 
            panel5.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panel5.Controls.Add(btn_RandomAudience);
            panel5.Controls.Add(label4);
            panel5.Controls.Add(txt_Audience);
            panel5.Location = new Point(3, 173);
            panel5.Name = "panel5";
            panel5.Size = new Size(990, 79);
            panel5.TabIndex = 2;
            // 
            // btn_RandomAudience
            // 
            btn_RandomAudience.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btn_RandomAudience.BackColor = Color.Yellow;
            btn_RandomAudience.Cursor = Cursors.Hand;
            btn_RandomAudience.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            btn_RandomAudience.Location = new Point(927, 45);
            btn_RandomAudience.Name = "btn_RandomAudience";
            btn_RandomAudience.Size = new Size(57, 31);
            btn_RandomAudience.TabIndex = 6;
            btn_RandomAudience.Text = "Auto";
            toolTip1.SetToolTip(btn_RandomAudience, "Random Audience");
            btn_RandomAudience.UseVisualStyleBackColor = false;
            btn_RandomAudience.Click += btn_RandomAudience_Click;
            // 
            // label4
            // 
            label4.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label4.Location = new Point(0, 0);
            label4.Name = "label4";
            label4.Size = new Size(75, 20);
            label4.TabIndex = 2;
            label4.Text = "Audience";
            // 
            // txt_Audience
            // 
            txt_Audience.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txt_Audience.Location = new Point(0, 23);
            txt_Audience.Multiline = true;
            txt_Audience.Name = "txt_Audience";
            txt_Audience.Size = new Size(924, 53);
            txt_Audience.TabIndex = 5;
            txt_Audience.TextChanged += txt_Audience_TextChanged;
            // 
            // panel4
            // 
            panel4.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panel4.Controls.Add(btn_RandomRefreshKey);
            panel4.Controls.Add(label3);
            panel4.Controls.Add(txt_RefreshKey);
            panel4.Location = new Point(3, 88);
            panel4.Name = "panel4";
            panel4.Size = new Size(990, 79);
            panel4.TabIndex = 1;
            // 
            // btn_RandomRefreshKey
            // 
            btn_RandomRefreshKey.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btn_RandomRefreshKey.BackColor = Color.Yellow;
            btn_RandomRefreshKey.Cursor = Cursors.Hand;
            btn_RandomRefreshKey.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            btn_RandomRefreshKey.Location = new Point(927, 45);
            btn_RandomRefreshKey.Name = "btn_RandomRefreshKey";
            btn_RandomRefreshKey.Size = new Size(57, 31);
            btn_RandomRefreshKey.TabIndex = 4;
            btn_RandomRefreshKey.Text = "Auto";
            toolTip1.SetToolTip(btn_RandomRefreshKey, "Random Refresh Key");
            btn_RandomRefreshKey.UseVisualStyleBackColor = false;
            btn_RandomRefreshKey.Click += btn_RandomRefreshKey_Click;
            // 
            // label3
            // 
            label3.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label3.Location = new Point(0, 0);
            label3.Name = "label3";
            label3.Size = new Size(75, 20);
            label3.TabIndex = 1;
            label3.Text = "Refresh Key";
            // 
            // txt_RefreshKey
            // 
            txt_RefreshKey.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txt_RefreshKey.Location = new Point(0, 23);
            txt_RefreshKey.Multiline = true;
            txt_RefreshKey.Name = "txt_RefreshKey";
            txt_RefreshKey.Size = new Size(924, 53);
            txt_RefreshKey.TabIndex = 3;
            txt_RefreshKey.TextChanged += txt_RefreshKey_TextChanged;
            // 
            // panel3
            // 
            panel3.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panel3.Controls.Add(btn_RandomAccessKey);
            panel3.Controls.Add(txt_AccessKey);
            panel3.Controls.Add(label2);
            panel3.Location = new Point(3, 3);
            panel3.Name = "panel3";
            panel3.Size = new Size(990, 79);
            panel3.TabIndex = 0;
            // 
            // btn_RandomAccessKey
            // 
            btn_RandomAccessKey.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btn_RandomAccessKey.BackColor = Color.Yellow;
            btn_RandomAccessKey.Cursor = Cursors.Hand;
            btn_RandomAccessKey.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            btn_RandomAccessKey.Location = new Point(927, 45);
            btn_RandomAccessKey.Name = "btn_RandomAccessKey";
            btn_RandomAccessKey.Size = new Size(57, 31);
            btn_RandomAccessKey.TabIndex = 2;
            btn_RandomAccessKey.Text = "Auto";
            toolTip1.SetToolTip(btn_RandomAccessKey, "Random Access Key");
            btn_RandomAccessKey.UseVisualStyleBackColor = false;
            btn_RandomAccessKey.Click += btn_RandomAccessKey_Click;
            // 
            // txt_AccessKey
            // 
            txt_AccessKey.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txt_AccessKey.Location = new Point(0, 23);
            txt_AccessKey.Multiline = true;
            txt_AccessKey.Name = "txt_AccessKey";
            txt_AccessKey.Size = new Size(924, 53);
            txt_AccessKey.TabIndex = 1;
            txt_AccessKey.TextChanged += txt_AccessKey_TextChanged;
            // 
            // label2
            // 
            label2.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label2.Location = new Point(0, 0);
            label2.Name = "label2";
            label2.Size = new Size(75, 20);
            label2.TabIndex = 0;
            label2.Text = "Access Key";
            // 
            // toolTip1
            // 
            toolTip1.IsBalloon = true;
            // 
            // UC_ConfigEnv
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(panel2);
            Name = "UC_ConfigEnv";
            Size = new Size(996, 577);
            Load += UC_ConfigEnv_Load;
            panel1.ResumeLayout(false);
            panel2.ResumeLayout(false);
            panel9.ResumeLayout(false);
            panel8.ResumeLayout(false);
            panel8.PerformLayout();
            panel7.ResumeLayout(false);
            panel7.PerformLayout();
            panel6.ResumeLayout(false);
            panel6.PerformLayout();
            panel5.ResumeLayout(false);
            panel5.PerformLayout();
            panel4.ResumeLayout(false);
            panel4.PerformLayout();
            panel3.ResumeLayout(false);
            panel3.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private Label label1;
        private Panel panel1;
        private Panel panel2;
        private ToolTip toolTip1;
        private Panel panel9;
        private Button button2;
        private Button button1;
        private Button btn_AutoAll;
        private Button btn_Save;
        private Panel panel8;
        private Button btn_LoadPublicKeyPath;
        private Button btn_RandomPublicKeyPath;
        private Label label7;
        private TextBox txt_PublicKeyPath;
        private Panel panel7;
        private Button btn_LoadPrivateKeyPath;
        private Button btn_RandomPrivateKeyPath;
        private TextBox txt_PrivateKeyPath;
        private Label label6;
        private Panel panel6;
        private Button btn_RandomIssuer;
        private Label label5;
        private TextBox txt_Issuer;
        private Panel panel5;
        private Button btn_RandomAudience;
        private Label label4;
        private TextBox txt_Audience;
        private Panel panel4;
        private Button btn_RandomRefreshKey;
        private Label label3;
        private TextBox txt_RefreshKey;
        private Panel panel3;
        private Button btn_RandomAccessKey;
        private TextBox txt_AccessKey;
        private Label label2;
    }
}
