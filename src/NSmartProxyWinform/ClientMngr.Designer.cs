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
            this.tbxLog = new System.Windows.Forms.TextBox();
            this.btnEnd = new System.Windows.Forms.Button();
            this.btnOpenInExplorer = new System.Windows.Forms.Button();
            this.notifyIconNSPClient = new System.Windows.Forms.NotifyIcon(this.components);
            this.cmsRightMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.btnExit = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.btnStart.Location = new System.Drawing.Point(178, 371);
            this.btnStart.Name = "button1";
            this.btnStart.Size = new System.Drawing.Size(75, 23);
            this.btnStart.TabIndex = 0;
            this.btnStart.Text = "开始";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.button1_Click);
            // 
            // textBox1
            // 
            this.tbxLog.BackColor = System.Drawing.SystemColors.WindowText;
            this.tbxLog.ForeColor = System.Drawing.SystemColors.Info;
            this.tbxLog.Location = new System.Drawing.Point(12, 12);
            this.tbxLog.Multiline = true;
            this.tbxLog.Name = "textBox1";
            this.tbxLog.ReadOnly = true;
            this.tbxLog.Size = new System.Drawing.Size(488, 353);
            this.tbxLog.TabIndex = 1;
            // 
            // button2
            // 
            this.btnEnd.Location = new System.Drawing.Point(259, 371);
            this.btnEnd.Name = "button2";
            this.btnEnd.Size = new System.Drawing.Size(75, 23);
            this.btnEnd.TabIndex = 2;
            this.btnEnd.Text = "结束";
            this.btnEnd.UseVisualStyleBackColor = true;
            this.btnEnd.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.btnOpenInExplorer.Location = new System.Drawing.Point(13, 372);
            this.btnOpenInExplorer.Name = "button3";
            this.btnOpenInExplorer.Size = new System.Drawing.Size(113, 23);
            this.btnOpenInExplorer.TabIndex = 3;
            this.btnOpenInExplorer.Text = "资源管理器中打开";
            this.btnOpenInExplorer.UseVisualStyleBackColor = true;
            this.btnOpenInExplorer.Click += new System.EventHandler(this.button3_Click);
            // 
            // notifyIcon1
            // 
            this.notifyIconNSPClient.ContextMenuStrip = this.cmsRightMenu;
            this.notifyIconNSPClient.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIconNSPClient.Text = "notifyIcon1";
            this.notifyIconNSPClient.Visible = true;
            // 
            // contextMenuStrip1
            // 
            this.cmsRightMenu.Name = "contextMenuStrip1";
            this.cmsRightMenu.Size = new System.Drawing.Size(61, 4);
            // 
            // button4
            // 
            this.btnExit.Location = new System.Drawing.Point(425, 372);
            this.btnExit.Name = "button4";
            this.btnExit.Size = new System.Drawing.Size(75, 23);
            this.btnExit.TabIndex = 4;
            this.btnExit.Text = "退出程序";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.button4_Click);
            // 
            // ClientMngr
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(512, 406);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.btnOpenInExplorer);
            this.Controls.Add(this.btnEnd);
            this.Controls.Add(this.tbxLog);
            this.Controls.Add(this.btnStart);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "ClientMngr";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "日志查看器";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.TextBox tbxLog;
        private System.Windows.Forms.Button btnEnd;
        private System.Windows.Forms.Button btnOpenInExplorer;
        private System.Windows.Forms.NotifyIcon notifyIconNSPClient;
        private System.Windows.Forms.ContextMenuStrip cmsRightMenu;
        private System.Windows.Forms.Button btnExit;
    }
}

