using NSmartProxyWinform.Util;

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
            System.Windows.Forms.ListViewItem listViewItem2 = new System.Windows.Forms.ListViewItem("2017studio.imwork.net:20001 => 127.0.0.1:80", 1);
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ClientMngr));
            this.btnStart = new System.Windows.Forms.Button();
            this.btnOpenInExplorer = new System.Windows.Forms.Button();
            this.notifyIconNSPClient = new System.Windows.Forms.NotifyIcon(this.components);
            this.cmsRightMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.启动内网穿透ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.配置ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.退出程序ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnExit = new System.Windows.Forms.Button();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tbxLog = new System.Windows.Forms.TextBox();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.btnSaveConfig = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnAddClient = new System.Windows.Forms.Button();
            this.btnDuplicate = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.grpNodeSettings = new System.Windows.Forms.GroupBox();
            this.cbxIsCompress = new System.Windows.Forms.CheckBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.cbxProtocol = new System.Windows.Forms.ComboBox();
            this.tbxTargetServerAddr = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.tbxTargetServerPort = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnTest = new System.Windows.Forms.Button();
            this.tbxProviderAddr = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.tbxWebPort = new System.Windows.Forms.TextBox();
            this.listBox1 = new System.Windows.Forms.ListView();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.tabServerConfig = new System.Windows.Forms.TabControl();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.btnUnRegWinSrv = new System.Windows.Forms.Button();
            this.btnRegWinSrv = new System.Windows.Forms.Button();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            this.btnLogin = new System.Windows.Forms.Button();
            this.cbxUseServerControl = new System.Windows.Forms.CheckBox();
            this.tbxDescription = new NSmartProxyWinform.Util.TextBoxEx();
            this.tbxHost = new NSmartProxyWinform.Util.TextBoxEx();
            this.tbxPort = new NSmartProxyWinform.Util.TextBoxEx();
            this.aboutBox1 = new NSmartProxyWinform.AboutBox();
            this.cmsRightMenu.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.grpNodeSettings.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tabServerConfig.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.tabPage4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
            this.SuspendLayout();
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(255, 455);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(75, 23);
            this.btnStart.TabIndex = 0;
            this.btnStart.Tag = "1";
            this.btnStart.Text = "开始";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnOpenInExplorer
            // 
            this.btnOpenInExplorer.Location = new System.Drawing.Point(20, 455);
            this.btnOpenInExplorer.Name = "btnOpenInExplorer";
            this.btnOpenInExplorer.Size = new System.Drawing.Size(113, 23);
            this.btnOpenInExplorer.TabIndex = 3;
            this.btnOpenInExplorer.Text = "资源管理器中打开";
            this.btnOpenInExplorer.UseVisualStyleBackColor = true;
            this.btnOpenInExplorer.Click += new System.EventHandler(this.ShowInExplorer_Click);
            // 
            // notifyIconNSPClient
            // 
            this.notifyIconNSPClient.ContextMenuStrip = this.cmsRightMenu;
            this.notifyIconNSPClient.Text = "NSmartProxy v0.3";
            this.notifyIconNSPClient.Visible = true;
            this.notifyIconNSPClient.DoubleClick += new System.EventHandler(this.notifyIconNSPClient_DoubleClick);
            // 
            // cmsRightMenu
            // 
            this.cmsRightMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.启动内网穿透ToolStripMenuItem,
            this.配置ToolStripMenuItem,
            this.toolStripSeparator1,
            this.退出程序ToolStripMenuItem});
            this.cmsRightMenu.Name = "contextMenuStrip1";
            this.cmsRightMenu.Size = new System.Drawing.Size(149, 76);
            // 
            // 启动内网穿透ToolStripMenuItem
            // 
            this.启动内网穿透ToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.White;
            this.启动内网穿透ToolStripMenuItem.Name = "启动内网穿透ToolStripMenuItem";
            this.启动内网穿透ToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
            this.启动内网穿透ToolStripMenuItem.Text = "启动内网穿透";
            this.启动内网穿透ToolStripMenuItem.Click += new System.EventHandler(this.启动内网穿透ToolStripMenuItem_Click);
            this.启动内网穿透ToolStripMenuItem.Paint += new System.Windows.Forms.PaintEventHandler(this.启动内网穿透ToolStripMenuItem_Paint);
            // 
            // 配置ToolStripMenuItem
            // 
            this.配置ToolStripMenuItem.Name = "配置ToolStripMenuItem";
            this.配置ToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
            this.配置ToolStripMenuItem.Text = "配置...";
            this.配置ToolStripMenuItem.Click += new System.EventHandler(this.配置ToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(145, 6);
            // 
            // 退出程序ToolStripMenuItem
            // 
            this.退出程序ToolStripMenuItem.Name = "退出程序ToolStripMenuItem";
            this.退出程序ToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
            this.退出程序ToolStripMenuItem.Text = "退出程序";
            this.退出程序ToolStripMenuItem.Click += new System.EventHandler(this.退出程序ToolStripMenuItem_Click);
            // 
            // btnExit
            // 
            this.btnExit.Location = new System.Drawing.Point(474, 455);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(75, 23);
            this.btnExit.TabIndex = 4;
            this.btnExit.Text = "退出程序";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.tbxLog);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(525, 411);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "日志";
            this.tabPage2.UseVisualStyleBackColor = true;
            this.tabPage2.Enter += new System.EventHandler(this.tabPage2_Enter);
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
            this.tbxLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbxLog.Size = new System.Drawing.Size(519, 405);
            this.tbxLog.TabIndex = 1;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.cbxUseServerControl);
            this.tabPage1.Controls.Add(this.btnSaveConfig);
            this.tabPage1.Controls.Add(this.btnRefresh);
            this.tabPage1.Controls.Add(this.btnAddClient);
            this.tabPage1.Controls.Add(this.btnDuplicate);
            this.tabPage1.Controls.Add(this.btnDelete);
            this.tabPage1.Controls.Add(this.grpNodeSettings);
            this.tabPage1.Controls.Add(this.groupBox1);
            this.tabPage1.Controls.Add(this.listBox1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(525, 411);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "应用";
            this.tabPage1.UseVisualStyleBackColor = true;
            this.tabPage1.Enter += new System.EventHandler(this.tabPage1_Enter);
            // 
            // btnSaveConfig
            // 
            this.btnSaveConfig.Location = new System.Drawing.Point(114, 374);
            this.btnSaveConfig.Name = "btnSaveConfig";
            this.btnSaveConfig.Size = new System.Drawing.Size(75, 23);
            this.btnSaveConfig.TabIndex = 14;
            this.btnSaveConfig.Text = "保存配置";
            this.btnSaveConfig.UseVisualStyleBackColor = true;
            this.btnSaveConfig.Click += new System.EventHandler(this.btnSaveConfig_Click);
            // 
            // btnRefresh
            // 
            this.btnRefresh.Location = new System.Drawing.Point(21, 374);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(75, 23);
            this.btnRefresh.TabIndex = 13;
            this.btnRefresh.Text = "还原配置";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // btnAddClient
            // 
            this.btnAddClient.Location = new System.Drawing.Point(21, 344);
            this.btnAddClient.Name = "btnAddClient";
            this.btnAddClient.Size = new System.Drawing.Size(75, 23);
            this.btnAddClient.TabIndex = 11;
            this.btnAddClient.Text = "添加";
            this.btnAddClient.UseVisualStyleBackColor = true;
            this.btnAddClient.Click += new System.EventHandler(this.btnAddClient_Click);
            // 
            // btnDuplicate
            // 
            this.btnDuplicate.Location = new System.Drawing.Point(206, 344);
            this.btnDuplicate.Name = "btnDuplicate";
            this.btnDuplicate.Size = new System.Drawing.Size(75, 23);
            this.btnDuplicate.TabIndex = 10;
            this.btnDuplicate.Text = "复制";
            this.btnDuplicate.UseVisualStyleBackColor = true;
            this.btnDuplicate.Click += new System.EventHandler(this.btnDuplicate_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.Location = new System.Drawing.Point(114, 344);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(75, 23);
            this.btnDelete.TabIndex = 9;
            this.btnDelete.Text = "删除";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // grpNodeSettings
            // 
            this.grpNodeSettings.Controls.Add(this.cbxIsCompress);
            this.grpNodeSettings.Controls.Add(this.tbxDescription);
            this.grpNodeSettings.Controls.Add(this.label9);
            this.grpNodeSettings.Controls.Add(this.label8);
            this.grpNodeSettings.Controls.Add(this.tbxHost);
            this.grpNodeSettings.Controls.Add(this.label3);
            this.grpNodeSettings.Controls.Add(this.cbxProtocol);
            this.grpNodeSettings.Controls.Add(this.tbxTargetServerAddr);
            this.grpNodeSettings.Controls.Add(this.label4);
            this.grpNodeSettings.Controls.Add(this.tbxPort);
            this.grpNodeSettings.Controls.Add(this.label5);
            this.grpNodeSettings.Controls.Add(this.tbxTargetServerPort);
            this.grpNodeSettings.Controls.Add(this.label6);
            this.grpNodeSettings.Location = new System.Drawing.Point(329, 77);
            this.grpNodeSettings.Name = "grpNodeSettings";
            this.grpNodeSettings.Size = new System.Drawing.Size(190, 290);
            this.grpNodeSettings.TabIndex = 8;
            this.grpNodeSettings.TabStop = false;
            this.grpNodeSettings.Text = "节点配置";
            // 
            // cbxIsCompress
            // 
            this.cbxIsCompress.AutoSize = true;
            this.cbxIsCompress.Location = new System.Drawing.Point(9, 209);
            this.cbxIsCompress.Name = "cbxIsCompress";
            this.cbxIsCompress.Size = new System.Drawing.Size(96, 16);
            this.cbxIsCompress.TabIndex = 13;
            this.cbxIsCompress.Text = "启用传输压缩";
            this.cbxIsCompress.UseVisualStyleBackColor = true;
            this.cbxIsCompress.CheckedChanged += new System.EventHandler(this.targetServer_TextChanged);
            // 
            // label9
            // 
            this.label9.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(7, 161);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(95, 12);
            this.label9.TabIndex = 11;
            this.label9.Text = "节点描述(*可选)";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label8
            // 
            this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(7, 134);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(95, 12);
            this.label8.TabIndex = 9;
            this.label8.Text = "主机域名(*可选)";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 25);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 8;
            this.label3.Text = "连接协议";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cbxProtocol
            // 
            this.cbxProtocol.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxProtocol.FormattingEnabled = true;
            this.cbxProtocol.Items.AddRange(new object[] {
            "TCP",
            "UDP",
            "HTTP"});
            this.cbxProtocol.Location = new System.Drawing.Point(102, 21);
            this.cbxProtocol.Name = "cbxProtocol";
            this.cbxProtocol.Size = new System.Drawing.Size(82, 20);
            this.cbxProtocol.TabIndex = 7;
            this.cbxProtocol.SelectedIndexChanged += new System.EventHandler(this.targetServer_TextChanged);
            // 
            // tbxTargetServerAddr
            // 
            this.tbxTargetServerAddr.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxTargetServerAddr.Location = new System.Drawing.Point(102, 50);
            this.tbxTargetServerAddr.Name = "tbxTargetServerAddr";
            this.tbxTargetServerAddr.Size = new System.Drawing.Size(82, 21);
            this.tbxTargetServerAddr.TabIndex = 3;
            this.tbxTargetServerAddr.Leave += new System.EventHandler(this.targetServer_TextChanged);
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(7, 53);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 12);
            this.label4.TabIndex = 0;
            this.label4.Text = "内网地址";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(7, 80);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 12);
            this.label5.TabIndex = 1;
            this.label5.Text = "内网端口";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tbxTargetServerPort
            // 
            this.tbxTargetServerPort.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxTargetServerPort.Location = new System.Drawing.Point(102, 77);
            this.tbxTargetServerPort.MaxLength = 5;
            this.tbxTargetServerPort.Name = "tbxTargetServerPort";
            this.tbxTargetServerPort.Size = new System.Drawing.Size(82, 21);
            this.tbxTargetServerPort.TabIndex = 4;
            this.tbxTargetServerPort.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tbxTargetServerPort_KeyPress);
            this.tbxTargetServerPort.Leave += new System.EventHandler(this.targetServer_TextChanged);
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(7, 107);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(95, 12);
            this.label6.TabIndex = 2;
            this.label6.Text = "外网端口(*可选)";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnTest);
            this.groupBox1.Controls.Add(this.tbxProviderAddr);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.tbxWebPort);
            this.groupBox1.Location = new System.Drawing.Point(6, 6);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(513, 57);
            this.groupBox1.TabIndex = 7;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "外网服务器";
            // 
            // btnTest
            // 
            this.btnTest.Location = new System.Drawing.Point(425, 18);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(75, 23);
            this.btnTest.TabIndex = 5;
            this.btnTest.Text = "测试";
            this.btnTest.UseVisualStyleBackColor = true;
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
            // 
            // tbxProviderAddr
            // 
            this.tbxProviderAddr.Location = new System.Drawing.Point(81, 20);
            this.tbxProviderAddr.Name = "tbxProviderAddr";
            this.tbxProviderAddr.Size = new System.Drawing.Size(184, 21);
            this.tbxProviderAddr.TabIndex = 3;
            this.tbxProviderAddr.Text = "2017studio.imwork.net";
            this.tbxProviderAddr.TextChanged += new System.EventHandler(this.ConfigValueChanged);
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
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(303, 23);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 12);
            this.label2.TabIndex = 1;
            this.label2.Text = "端口";
            // 
            // tbxWebPort
            // 
            this.tbxWebPort.Location = new System.Drawing.Point(342, 20);
            this.tbxWebPort.MaxLength = 5;
            this.tbxWebPort.Name = "tbxWebPort";
            this.tbxWebPort.Size = new System.Drawing.Size(55, 21);
            this.tbxWebPort.TabIndex = 4;
            this.tbxWebPort.Text = "12309";
            this.tbxWebPort.TextChanged += new System.EventHandler(this.ConfigValueChanged);
            this.tbxWebPort.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tbxTargetServerPort_KeyPress);
            // 
            // listBox1
            // 
            this.listBox1.FullRowSelect = true;
            this.listBox1.HideSelection = false;
            this.listBox1.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem2});
            this.listBox1.Location = new System.Drawing.Point(11, 84);
            this.listBox1.MultiSelect = false;
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(303, 244);
            this.listBox1.SmallImageList = this.imageList1;
            this.listBox1.TabIndex = 6;
            this.listBox1.UseCompatibleStateImageBehavior = false;
            this.listBox1.View = System.Windows.Forms.View.List;
            this.listBox1.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
            this.listBox1.Leave += new System.EventHandler(this.listBox1_Leave);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "run");
            this.imageList1.Images.SetKeyName(1, "stop");
            this.imageList1.Images.SetKeyName(2, "error");
            // 
            // tabServerConfig
            // 
            this.tabServerConfig.Controls.Add(this.tabPage1);
            this.tabServerConfig.Controls.Add(this.tabPage2);
            this.tabServerConfig.Controls.Add(this.tabPage3);
            this.tabServerConfig.Controls.Add(this.tabPage4);
            this.tabServerConfig.Location = new System.Drawing.Point(12, 12);
            this.tabServerConfig.Name = "tabServerConfig";
            this.tabServerConfig.SelectedIndex = 0;
            this.tabServerConfig.Size = new System.Drawing.Size(533, 437);
            this.tabServerConfig.TabIndex = 5;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.btnUnRegWinSrv);
            this.tabPage3.Controls.Add(this.btnRegWinSrv);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(525, 411);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "服务";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // btnUnRegWinSrv
            // 
            this.btnUnRegWinSrv.Location = new System.Drawing.Point(180, 155);
            this.btnUnRegWinSrv.Name = "btnUnRegWinSrv";
            this.btnUnRegWinSrv.Size = new System.Drawing.Size(154, 49);
            this.btnUnRegWinSrv.TabIndex = 1;
            this.btnUnRegWinSrv.Text = "卸载windows服务";
            this.btnUnRegWinSrv.UseVisualStyleBackColor = true;
            this.btnUnRegWinSrv.Click += new System.EventHandler(this.btnUnRegWinSrv_Click);
            // 
            // btnRegWinSrv
            // 
            this.btnRegWinSrv.Location = new System.Drawing.Point(180, 100);
            this.btnRegWinSrv.Name = "btnRegWinSrv";
            this.btnRegWinSrv.Size = new System.Drawing.Size(154, 49);
            this.btnRegWinSrv.TabIndex = 0;
            this.btnRegWinSrv.Text = "注册为windows服务";
            this.btnRegWinSrv.UseVisualStyleBackColor = true;
            this.btnRegWinSrv.Click += new System.EventHandler(this.btnRegWinSrv_Click);
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.aboutBox1);
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage4.Size = new System.Drawing.Size(525, 411);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "关于";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // errorProvider1
            // 
            this.errorProvider1.ContainerControl = this;
            // 
            // btnLogin
            // 
            this.btnLogin.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btnLogin.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnLogin.Location = new System.Drawing.Point(158, 455);
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new System.Drawing.Size(91, 23);
            this.btnLogin.TabIndex = 6;
            this.btnLogin.Text = "  未登录";
            this.btnLogin.UseVisualStyleBackColor = true;
            this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);
            // 
            // cbxUseServerControl
            // 
            this.cbxUseServerControl.AutoSize = true;
            this.cbxUseServerControl.Location = new System.Drawing.Point(338, 381);
            this.cbxUseServerControl.Name = "cbxUseServerControl";
            this.cbxUseServerControl.Size = new System.Drawing.Size(108, 16);
            this.cbxUseServerControl.TabIndex = 14;
            this.cbxUseServerControl.Text = "使用服务端配置";
            this.cbxUseServerControl.UseVisualStyleBackColor = true;
            this.cbxUseServerControl.CheckedChanged += new System.EventHandler(this.cbxUseServerControl_CheckedChanged);
            // 
            // tbxDescription
            // 
            this.tbxDescription.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxDescription.Location = new System.Drawing.Point(16, 182);
            this.tbxDescription.MaxLength = 32;
            this.tbxDescription.Name = "tbxDescription";
            this.tbxDescription.PlaceHolderStr = "<请输入32位以内的字符>";
            this.tbxDescription.Size = new System.Drawing.Size(168, 21);
            this.tbxDescription.TabIndex = 12;
            this.tbxDescription.Leave += new System.EventHandler(this.targetServer_TextChanged);
            // 
            // tbxHost
            // 
            this.tbxHost.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxHost.Location = new System.Drawing.Point(102, 131);
            this.tbxHost.MaxLength = 500;
            this.tbxHost.Name = "tbxHost";
            this.tbxHost.PlaceHolderStr = null;
            this.tbxHost.Size = new System.Drawing.Size(82, 21);
            this.tbxHost.TabIndex = 10;
            this.tbxHost.Leave += new System.EventHandler(this.targetServer_TextChanged);
            // 
            // tbxPort
            // 
            this.tbxPort.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxPort.Location = new System.Drawing.Point(102, 104);
            this.tbxPort.MaxLength = 5;
            this.tbxPort.Name = "tbxPort";
            this.tbxPort.PlaceHolderStr = "<随机>";
            this.tbxPort.Size = new System.Drawing.Size(82, 21);
            this.tbxPort.TabIndex = 5;
            this.tbxPort.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tbxTargetServerPort_KeyPress);
            this.tbxPort.Leave += new System.EventHandler(this.targetServer_TextChanged);
            // 
            // aboutBox1
            // 
            this.aboutBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.aboutBox1.Location = new System.Drawing.Point(3, 3);
            this.aboutBox1.Name = "aboutBox1";
            this.aboutBox1.Padding = new System.Windows.Forms.Padding(20);
            this.aboutBox1.Size = new System.Drawing.Size(519, 405);
            this.aboutBox1.TabIndex = 0;
            // 
            // ClientMngr
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(557, 490);
            this.Controls.Add(this.btnLogin);
            this.Controls.Add(this.tabServerConfig);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.btnOpenInExplorer);
            this.Controls.Add(this.btnStart);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ClientMngr";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "配置对话框";
            this.Activated += new System.EventHandler(this.ClientMngr_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ClientMngr_FormClosing);
            this.Load += new System.EventHandler(this.ClientMngr_Load);
            this.cmsRightMenu.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.grpNodeSettings.ResumeLayout(false);
            this.grpNodeSettings.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tabServerConfig.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.tabPage4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnOpenInExplorer;
        private System.Windows.Forms.NotifyIcon notifyIconNSPClient;
        private System.Windows.Forms.ContextMenuStrip cmsRightMenu;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TextBox tbxLog;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabControl tabServerConfig;
        private System.Windows.Forms.ListView listBox1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox grpNodeSettings;
        private System.Windows.Forms.TextBox tbxTargetServerAddr;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbxTargetServerPort;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnDuplicate;
        private System.Windows.Forms.Button btnAddClient;
        private System.Windows.Forms.ToolStripMenuItem 退出程序ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 配置ToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem 启动内网穿透ToolStripMenuItem;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnSaveConfig;
        private System.Windows.Forms.ErrorProvider errorProvider1;
        private System.Windows.Forms.Button btnLogin;
        public System.Windows.Forms.TextBox tbxProviderAddr;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.Button btnRegWinSrv;
        private System.Windows.Forms.Button btnUnRegWinSrv;
        private System.Windows.Forms.Button btnTest;
        private TextBoxEx tbxPort;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cbxProtocol;
        private System.Windows.Forms.Label label8;
        private TextBoxEx tbxHost;
        private System.Windows.Forms.Label label9;
        private TextBoxEx tbxDescription;
        public System.Windows.Forms.TextBox tbxWebPort;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.CheckBox cbxIsCompress;
        private System.Windows.Forms.TabPage tabPage4;
        private AboutBox aboutBox1;
        private System.Windows.Forms.CheckBox cbxUseServerControl;
    }
}

