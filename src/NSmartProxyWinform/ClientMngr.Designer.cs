namespace NSmartProxyWinform
{
    partial class ClientMngr
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ClientMngr));
            this.btnStart = new System.Windows.Forms.Button();
            this.btnEnd = new System.Windows.Forms.Button();
            this.btnOpenInExplorer = new System.Windows.Forms.Button();
            this.notifyIconNSPClient = new System.Windows.Forms.NotifyIcon(this.components);
            this.cmsRightMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.btnExit = new System.Windows.Forms.Button();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tbxLog = new System.Windows.Forms.TextBox();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.btnAddClient = new System.Windows.Forms.Button();
            this.btnDuplicate = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.tbxTargetServerAddr = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tbxPort = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tbxTargetServerPort = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tbxProviderAddr = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tbxConfigPort = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tbxReversePort = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.tabServerConfig = new System.Windows.Forms.TabControl();
            this.退出程序ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.配置ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.cmsRightMenu.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tabServerConfig.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(208, 371);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(75, 23);
            this.btnStart.TabIndex = 0;
            this.btnStart.Text = "开始";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnEnd
            // 
            this.btnEnd.Location = new System.Drawing.Point(289, 371);
            this.btnEnd.Name = "btnEnd";
            this.btnEnd.Size = new System.Drawing.Size(75, 23);
            this.btnEnd.TabIndex = 2;
            this.btnEnd.Text = "结束";
            this.btnEnd.UseVisualStyleBackColor = true;
            this.btnEnd.Click += new System.EventHandler(this.button2_Click);
            // 
            // btnOpenInExplorer
            // 
            this.btnOpenInExplorer.Location = new System.Drawing.Point(13, 372);
            this.btnOpenInExplorer.Name = "btnOpenInExplorer";
            this.btnOpenInExplorer.Size = new System.Drawing.Size(113, 23);
            this.btnOpenInExplorer.TabIndex = 3;
            this.btnOpenInExplorer.Text = "资源管理器中打开";
            this.btnOpenInExplorer.UseVisualStyleBackColor = true;
            this.btnOpenInExplorer.Click += new System.EventHandler(this.button3_Click);
            // 
            // notifyIconNSPClient
            // 
            this.notifyIconNSPClient.ContextMenuStrip = this.cmsRightMenu;
            this.notifyIconNSPClient.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIconNSPClient.Icon")));
            this.notifyIconNSPClient.Text = "notifyIcon1";
            this.notifyIconNSPClient.Visible = true;
            this.notifyIconNSPClient.DoubleClick += new System.EventHandler(this.notifyIconNSPClient_DoubleClick);
            // 
            // cmsRightMenu
            // 
            this.cmsRightMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.配置ToolStripMenuItem,
            this.toolStripSeparator1,
            this.退出程序ToolStripMenuItem});
            this.cmsRightMenu.Name = "contextMenuStrip1";
            this.cmsRightMenu.Size = new System.Drawing.Size(125, 54);
            // 
            // btnExit
            // 
            this.btnExit.Location = new System.Drawing.Point(466, 372);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(75, 23);
            this.btnExit.TabIndex = 4;
            this.btnExit.Text = "退出程序";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.button4_Click);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.tbxLog);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(525, 328);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "日志";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // tbxLog
            // 
            this.tbxLog.BackColor = System.Drawing.SystemColors.WindowText;
            this.tbxLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbxLog.ForeColor = System.Drawing.SystemColors.Info;
            this.tbxLog.Location = new System.Drawing.Point(3, 3);
            this.tbxLog.Multiline = true;
            this.tbxLog.Name = "tbxLog";
            this.tbxLog.ReadOnly = true;
            this.tbxLog.Size = new System.Drawing.Size(519, 322);
            this.tbxLog.TabIndex = 1;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.btnAddClient);
            this.tabPage1.Controls.Add(this.btnDuplicate);
            this.tabPage1.Controls.Add(this.btnDelete);
            this.tabPage1.Controls.Add(this.groupBox2);
            this.tabPage1.Controls.Add(this.groupBox1);
            this.tabPage1.Controls.Add(this.listBox1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(525, 328);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "应用";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // btnAddClient
            // 
            this.btnAddClient.Location = new System.Drawing.Point(21, 286);
            this.btnAddClient.Name = "btnAddClient";
            this.btnAddClient.Size = new System.Drawing.Size(75, 23);
            this.btnAddClient.TabIndex = 11;
            this.btnAddClient.Text = "添加";
            this.btnAddClient.UseVisualStyleBackColor = true;
            // 
            // btnDuplicate
            // 
            this.btnDuplicate.Location = new System.Drawing.Point(182, 286);
            this.btnDuplicate.Name = "btnDuplicate";
            this.btnDuplicate.Size = new System.Drawing.Size(75, 23);
            this.btnDuplicate.TabIndex = 10;
            this.btnDuplicate.Text = "复制";
            this.btnDuplicate.UseVisualStyleBackColor = true;
            // 
            // btnDelete
            // 
            this.btnDelete.Location = new System.Drawing.Point(101, 286);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(75, 23);
            this.btnDelete.TabIndex = 9;
            this.btnDelete.Text = "删除";
            this.btnDelete.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.tbxTargetServerAddr);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.tbxPort);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.tbxTargetServerPort);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Location = new System.Drawing.Point(329, 77);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(190, 232);
            this.groupBox2.TabIndex = 8;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "节点配置";
            // 
            // tbxTargetServerAddr
            // 
            this.tbxTargetServerAddr.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxTargetServerAddr.Location = new System.Drawing.Point(102, 20);
            this.tbxTargetServerAddr.Name = "tbxTargetServerAddr";
            this.tbxTargetServerAddr.Size = new System.Drawing.Size(82, 21);
            this.tbxTargetServerAddr.TabIndex = 3;
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(43, 23);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 12);
            this.label4.TabIndex = 0;
            this.label4.Text = "内网地址";
            // 
            // tbxPort
            // 
            this.tbxPort.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxPort.Location = new System.Drawing.Point(102, 74);
            this.tbxPort.Name = "tbxPort";
            this.tbxPort.Size = new System.Drawing.Size(82, 21);
            this.tbxPort.TabIndex = 5;
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(43, 50);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 12);
            this.label5.TabIndex = 1;
            this.label5.Text = "内网端口";
            // 
            // tbxTargetServerPort
            // 
            this.tbxTargetServerPort.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxTargetServerPort.Location = new System.Drawing.Point(102, 47);
            this.tbxTargetServerPort.Name = "tbxTargetServerPort";
            this.tbxTargetServerPort.Size = new System.Drawing.Size(82, 21);
            this.tbxTargetServerPort.TabIndex = 4;
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(7, 77);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(95, 12);
            this.label6.TabIndex = 2;
            this.label6.Text = "外网端口(*可选)";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.tbxProviderAddr);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.tbxConfigPort);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.tbxReversePort);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Location = new System.Drawing.Point(6, 6);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(513, 57);
            this.groupBox1.TabIndex = 7;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "外网服务器";
            // 
            // tbxProviderAddr
            // 
            this.tbxProviderAddr.Location = new System.Drawing.Point(81, 20);
            this.tbxProviderAddr.Name = "tbxProviderAddr";
            this.tbxProviderAddr.Size = new System.Drawing.Size(164, 21);
            this.tbxProviderAddr.TabIndex = 3;
            this.tbxProviderAddr.Text = "2017studio.imwork.net";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "服务器地址";
            // 
            // tbxConfigPort
            // 
            this.tbxConfigPort.Location = new System.Drawing.Point(448, 20);
            this.tbxConfigPort.Name = "tbxConfigPort";
            this.tbxConfigPort.Size = new System.Drawing.Size(55, 21);
            this.tbxConfigPort.TabIndex = 5;
            this.tbxConfigPort.Text = "12308";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(251, 23);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 1;
            this.label2.Text = "连接端口";
            // 
            // tbxReversePort
            // 
            this.tbxReversePort.Location = new System.Drawing.Point(310, 20);
            this.tbxReversePort.Name = "tbxReversePort";
            this.tbxReversePort.Size = new System.Drawing.Size(55, 21);
            this.tbxReversePort.TabIndex = 4;
            this.tbxReversePort.Text = "19974";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(389, 23);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 2;
            this.label3.Text = "配置端口";
            // 
            // listBox1
            // 
            this.listBox1.FormattingEnabled = true;
            this.listBox1.ItemHeight = 12;
            this.listBox1.Items.AddRange(new object[] {
            "2017studio.imwork.net:20001 => 127.0.0.1:80"});
            this.listBox1.Location = new System.Drawing.Point(11, 84);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(303, 196);
            this.listBox1.TabIndex = 6;
            this.listBox1.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
            // 
            // tabServerConfig
            // 
            this.tabServerConfig.Controls.Add(this.tabPage1);
            this.tabServerConfig.Controls.Add(this.tabPage2);
            this.tabServerConfig.Location = new System.Drawing.Point(12, 12);
            this.tabServerConfig.Name = "tabServerConfig";
            this.tabServerConfig.SelectedIndex = 0;
            this.tabServerConfig.Size = new System.Drawing.Size(533, 354);
            this.tabServerConfig.TabIndex = 5;
            // 
            // 退出程序ToolStripMenuItem
            // 
            this.退出程序ToolStripMenuItem.Name = "退出程序ToolStripMenuItem";
            this.退出程序ToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.退出程序ToolStripMenuItem.Text = "退出程序";
            this.退出程序ToolStripMenuItem.Click += new System.EventHandler(this.退出程序ToolStripMenuItem_Click);
            // 
            // 配置ToolStripMenuItem
            // 
            this.配置ToolStripMenuItem.Name = "配置ToolStripMenuItem";
            this.配置ToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.配置ToolStripMenuItem.Text = "配置...";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(177, 6);
            // 
            // ClientMngr
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(557, 406);
            this.Controls.Add(this.tabServerConfig);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.btnOpenInExplorer);
            this.Controls.Add(this.btnEnd);
            this.Controls.Add(this.btnStart);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "ClientMngr";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "配置对话框";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ClientMngr_FormClosing);
            this.cmsRightMenu.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.tabPage1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tabServerConfig.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnEnd;
        private System.Windows.Forms.Button btnOpenInExplorer;
        private System.Windows.Forms.NotifyIcon notifyIconNSPClient;
        private System.Windows.Forms.ContextMenuStrip cmsRightMenu;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TextBox tbxLog;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TextBox tbxConfigPort;
        private System.Windows.Forms.TextBox tbxReversePort;
        private System.Windows.Forms.TextBox tbxProviderAddr;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabControl tabServerConfig;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox tbxTargetServerAddr;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbxPort;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbxTargetServerPort;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnDuplicate;
        private System.Windows.Forms.Button btnAddClient;
        private System.Windows.Forms.ToolStripMenuItem 退出程序ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 配置ToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
    }
}

