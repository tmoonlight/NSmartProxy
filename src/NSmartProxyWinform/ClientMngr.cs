using log4net;
using NSmartProxy.Client;
using NSmartProxy.Data;
using NSmartProxy.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using log4net.Appender;
using log4net.Repository;
using Newtonsoft.Json;
using NSmartProxy.Client.Authorize;
using NSmartProxy.Infrastructure;
using NSmartProxy.Shared;
using NSmartProxyWinform.Util;
using NSmartProxy.ClientRouter.Dispatchers;
using NSmartProxy.Data.Models;
using static NSmartProxy.Infrastructure.I18N;

namespace NSmartProxyWinform
{
    public partial class ClientMngr : Form
    {
        public Router clientRouter;
        private Log4netLogger logger;
        private NSPClientConfig config;

        private const string NULL_CLIENT_TEXT = "<未编辑节点>";
        //private const string RANDOM_PORT_TEXT = "<随机>";
        private const string START_TAG_TEXT = "1";
        private const string END_TAG_TEXT = "0";
        private const string SERVICE_PATH = "NSmartProxyWinService.exe";
        private const string LOG_FILE_PATH = "log-file.log";
        

        public bool IsServiceMode
        {
            get
            {
                return WinServiceHelper.IsServiceExisted(Global.NSPClientServiceName);
            }
        }

        public bool IsNSPClientStarted
        {
            get => btnStart.Tag.ToString() == END_TAG_TEXT;
        }

        public ClientMngr()
        {
            InitializeComponent();
            //将日志写入窗体中。
            logger = new Log4netLogger();
            logger.BeforeWriteLog = (msg) => { ShowLogInfo(msg.ToString()); };
            //右下角小图标
            notifyIconNSPClient.Icon = Properties.Resources.servicestopped;

            //界面的一些细节初始化
            btnLogin.Location = new Point(12, btnLogin.Location.Y);
            btnOpenInExplorer.Hide();



            this.notifyIconNSPClient.Text = NSPVersion.NSmartProxyClientName;

            UpdateText();
        }

        private void UpdateText()
        {
            this.notifyIconNSPClient.Text = NSPVersion.NSmartProxyClientName;
            btnStart.Text = L("开始");
            btnOpenInExplorer.Text = L("资源管理器中打开");
            启动内网穿透ToolStripMenuItem.Text = L("启动内网穿透");
            配置ToolStripMenuItem.Text = L("配置...");
            退出程序ToolStripMenuItem.Text = L("退出程序");
            btnExit.Text = L("退出程序");
            tabPage2.Text = L("日志");
            tabPage1.Text = L("应用");
            btnSaveConfig.Text = L("保存配置");
            btnRefresh.Text = L("还原配置");
            btnAddClient.Text = L("添加");
            btnDuplicate.Text = L("复制");
            btnDelete.Text = L("删除");
            grpNodeSettings.Text = L("节点配置");
            //label7.Text = L("* : 外网端口为0或者空则代表端口由服务端自动分派。");
            label4.Text = L("内网地址");
            label5.Text = L("内网端口");
            label6.Text = L("外网端口(*可选)");
            groupBox1.Text = L("外网服务器");
            btnTest.Text = L("测试");
            label1.Text = L("服务器地址");
            label2.Text = L("端口");
            tabPage3.Text = L("服务");
            btnUnRegWinSrv.Text = L("卸载windows服务");
            btnRegWinSrv.Text = L("注册为windows服务");
            btnLogin.Text = L("  未登录");
            tbxPort.PlaceHolderStr = L("<随机>");
            Text = L("配置对话框");
            cbxIsCompress.Text = L("启用传输压缩");
        }

        //private void RefreshLoginState()
        //{
        //    string endpoint = tbxProviderAddr.Text + ":" + tbxWebPort;
        //    ClientUserCacheItem cacheItem = UserCacheManager.GetUserCacheFromEndpoint(endpoint, Router.NspClientCachePath);
        //    if (cacheItem != null)
        //    {//TODO 3 显示出用户名
        //        btnLogin.Image = Properties.Resources.logined;
        //    }
        //    else
        //    {
        //        btnLogin.Image = Properties.Resources.unlogin;
        //    }
        //}

        private void btnStart_Click(object sender, EventArgs e)
        {
            RefreshLoginBtnState();
            //RefreshLoginState();

            if (ValidateConfig() && SaveFormDataToConfigFile())
            {
                StartOrStop();
            }
        }

        private bool ValidateConfig()
        {
            bool isValid = true;
            if (tbxProviderAddr.Text == "")
            {
                errorProvider1.SetError(tbxProviderAddr, L("必须填写服务器地址"));
                isValid = false;
            }

            if (ValidateRequired(tbxProviderAddr) &&
                ValidateRequired(tbxWebPort) &&
                ValidateMoreThanZero(tbxWebPort))
            {
                isValid = true;
            }
            else
            {
                isValid = false;
            }

            return isValid;
        }

        private bool ValidateMoreThanZero(Control ctrl)
        {
            if (int.Parse(ctrl.Text) < 1)
            {
                errorProvider1.SetError(ctrl, L("值必须大于0"));
                return false;
            }
            return true;

        }

        private bool ValidateRequired(Control ctrl)
        {
            if (ctrl.Text == "")
            {
                errorProvider1.SetError(ctrl, L("必填"));
                return false;
            }
            return true;
        }

        //状态控制
        private void StartOrStop()
        {
            btnStart.Enabled = false;
            var isService = this.IsServiceMode;


            if (btnStart.Tag.ToString() == START_TAG_TEXT)
            {
                if (isService)
                {
                    using (ServiceController control = new ServiceController(Global.NSPClientServiceName))
                    {
                        if (control.Status == ServiceControllerStatus.Stopped)
                        {
                            control.Start();
                        }
                        RefreshServiceBtnStatus();

                    }
                }
                else
                {
                    #region 启动客户端
                    StartClientRouter(config, (status, tunelStr) =>
                    {
                        btnStart.Invoke(new Action(
                            () =>
                            {
                                if (status == ClientStatus.Started)
                                {
                                    notifyIconNSPClient.BalloonTipText = L("内网穿透已启动");
                                    listBox1.ForeColor = Color.Green;
                                    foreach (ListViewItem item in listBox1.Items)
                                    {
                                        item.ImageKey = "run";
                                    }

                                    //listBox1.Items.Clear();//TODO 3 这里会导致tag丢失
                                    //这里需要保证顺序
                                    int ii = 0;
                                    foreach (var tunnel in tunelStr)
                                    {
                                        notifyIconNSPClient.BalloonTipText += "\r\n" + tunnel.ToString();
                                        listBox1.Items[ii].Text = tunnel.Substring(tunnel.IndexOf(':') + 1).Trim();
                                        ii++;
                                    }
                                    notifyIconNSPClient.ShowBalloonTip(5000);
                                    SetUIToRunning();
                                }
                                else if (status == ClientStatus.Stopped)
                                {
                                    MessageBox.Show(L("客户端连接失败，详情请查看日志。"));
                                    btnStart.Enabled = true;
                                }
                                else if (status == ClientStatus.LoginError)
                                {
                                    MessageBox.Show(L("客户端登录失败，详情请查看日志。"));
                                    btnStart.Enabled = true;
                                }
                            }
                            ));

                    });
                    #endregion
                }

            }
            else
            {
                if (isService)
                {
                    using (ServiceController control = new ServiceController(Global.NSPClientServiceName))
                    {
                        control.Stop();
                    }
                    RefreshServiceBtnStatus();
                }
                else
                {
                    #region 关闭客户端
                    var tsk = clientRouter.Close();
                    tsk.ContinueWith(t => btnStart.Invoke(new Action(
                        () =>
                        {
                            if (t.IsFaulted) { logger.Error(L("客户端关闭失败"), null); btnStart.Enabled = true; return; }
                            //listBox1.ForeColor = Color.Black;
                            foreach (ListViewItem item in listBox1.Items)
                            {
                                item.ImageKey = "stop";
                            }
                            SetUIToStop();
                        }
                    )));
                    #endregion
                }
            }

        }

        private void SetUIToStop()
        {
            btnStart.Text = L("开始");
            btnStart.Tag = START_TAG_TEXT;
            notifyIconNSPClient.Icon = Properties.Resources.servicestopped;
            btnStart.Enabled = true;
        }

        private void SetUIToRunning()
        {
            btnStart.Text = L("停止");
            btnStart.Tag = END_TAG_TEXT;
            notifyIconNSPClient.Icon = Properties.Resources.servicerunning;
            btnStart.Enabled = true;
        }

        private void RefreshServiceBtnStatus()
        {
            btnStart.Enabled = false;
            btnStart.Invoke(new Action(() =>
            {
                ServiceController control = new ServiceController(Global.NSPClientServiceName);

                Task tskRunning = Task.Run(() => { try { control.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0, 0, 4)); } catch { } });
                Task tskStopping = Task.Run(() => { try { control.WaitForStatus(ServiceControllerStatus.Stopped, new TimeSpan(0, 0, 4)); } catch { } });
                Task tskTimeout = Task.Delay(3000);
                Task resultTsk = Task.WhenAny(tskRunning, tskStopping, tskTimeout);
                resultTsk.ContinueWith(t => { control.Close(); });
                resultTsk.Wait();
                if (tskRunning.IsCompleted)
                {
                    string tip = $"内网穿透后台服务({Global.NSPClientServiceName})已启动，详情请查看文件日志";
                    SetUIToRunning();
                    notifyIconNSPClient.BalloonTipText = tip;
                    ShowLogInfo(tip);
                }
                else if (tskStopping.IsCompleted)
                {
                    SetUIToStop();
                    ShowLogInfo($"内网穿透后台服务({Global.NSPClientServiceName})已激活，但处于关闭状态。");
                }
                else
                {
                    string tip = L("服务状态异常。");
                }

            }));
        }

        private void FileWatcherForLog_Changed(object sender, FileSystemEventArgs e)
        {
            //   e.ChangeType = WatcherChangeTypes
        }

        private void StartClientRouter(NSPClientConfig config, Action<ClientStatus, List<string>> loaded)
        {
            clientRouter = new Router(logger);
            //read config from config file.
            SetConfig(clientRouter, config);
            clientRouter.StatusChanged = loaded;
            var tsk = clientRouter.Start();
            tsk.ConfigureAwait(false);//异步会导致无法抛出错误,同步又会导致锁死，必须再invoke一次？
        }

        private void SetConfig(Router clientRouter, NSPClientConfig config)
        {
            clientRouter.SetConfiguration(config);
        }

        public void ShowLogInfo(string info)
        {
            tbxLog.Invoke(
                new Action(() =>
                {
                    tbxLog.AppendText(info);
                    tbxLog.AppendText(Environment.NewLine);
                    tbxLog.ScrollToCaret();
                }
            ));
        }

        private void ShowInExplorer_Click(object sender, EventArgs e)
        {
            var appender = LogManager.GetRepository(Program.LOGGER_REPO_NAME).GetAppenders()
                .Where((o) => o.GetType() == typeof(FileAppender)).First();
            var filePath = ((FileAppender)appender).File;

            //记录日志
            string argument = "/select, \"" + filePath + "\"";
            System.Diagnostics.Process.Start("explorer.exe", argument);
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            ExitProgram();
        }

        private void ExitProgram()
        {
            string tip = L("确认退出？");
            if (IsServiceMode) tip += L("服务将在后台运行。");
            if (MessageBox.Show(tip, "NSmartProxy", MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                notifyIconNSPClient.Dispose();
                Environment.Exit(0);
            }
        }

        private void ClientMngr_FormClosing(object sender, FormClosingEventArgs e)
        {
            
            e.Cancel = true;
            this.Hide();
            this.ShowInTaskbar = false;
        }

        private void notifyIconNSPClient_DoubleClick(object sender, EventArgs e)
        {
            ShowForm();
        }

        private void ShowForm()
        {
            this.Show();
            this.TopMost = true;
            Application.DoEvents();
            this.TopMost = false;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            #region 解决listview失去焦点高亮问题
            this.listBox1.Items.Cast<ListViewItem>()
                .ToList().ForEach(item =>
                {
                    item.BackColor = SystemColors.Window;
                    item.ForeColor = SystemColors.WindowText;
                });
            this.listBox1.SelectedItems.Cast<ListViewItem>()
                .ToList().ForEach(item =>
                {
                    item.BackColor = SystemColors.Highlight;
                    item.ForeColor = SystemColors.HighlightText;
                });
            #endregion


            if (listBox1.SelectedItems.Count > 0 && listBox1.SelectedItems[0].Text == NULL_CLIENT_TEXT)
            {
                tbxPort.Clear();
                tbxTargetServerAddr.Clear();
                tbxTargetServerPort.Clear();
                tbxHost.Clear();
                tbxDescription.Clear();
                //TODO 3 清空文本
            }
            else//节点配置显示
            if (listBox1.SelectedItems.Count > 0)
            {
                //var strSelectedItemStr = listBox1.SelectedItems[0].Text;
                //var strParts = strSelectedItemStr.Split(new string[]
                //{
                //    "=>", ":"
                //}, StringSplitOptions.None);
                //if (strParts.Length != 4)
                //{
                //    MessageBox.Show(L("非法选择项"));
                //    return;
                //}

                //tbxPort.Text = strParts[1].Trim();
                //tbxTargetServerAddr.Text = strParts[2].Trim();
                //tbxTargetServerPort.Text = strParts[3].Trim();
                ClientApp app = listBox1.SelectedItems[0].Tag as ClientApp;
                if (app == null)
                {
                    MessageBox.Show("节点数据丢失！");
                    return;
                }
                tbxPort.Text = app.ConsumerPort.ToString();
                tbxTargetServerAddr.Text = app.IP;//  strParts[2].Trim();
                tbxTargetServerPort.Text = app.TargetServicePort.ToString(); //strParts[3].Trim();
                tbxHost.Text = app.Host;
                tbxDescription.Text = app.Description;
                cbxIsCompress.Checked = app.IsCompress;
                cbxProtocol.SelectedValue = Enum.GetName(typeof(Protocol), app.Protocol);
            }
        }



        private void 退出程序ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExitProgram();
        }

        private void ConfigValueChanged(object sender, EventArgs e)
        {
            //configChanged = true;
            //btnLogin.Text = " 未登录";
        }

        private void listBox1_Leave(object sender, EventArgs e)
        {
            listBox1.BackColor = Color.White;
        }

        private void btnAddClient_Click(object sender, EventArgs e)
        {
            var item = listBox1.Items.Add(NULL_CLIENT_TEXT);
            //listBox1.SelectedIndex = index;
            item.Selected = true;
        }



        private void targetServer_TextChanged(object sender, EventArgs e)
        {
            //刷新listview里的tag对象
            if (listBox1.SelectedItems.Count > 0)
            {
                listBox1.SelectedItems[0].Tag = GetClientAppFromForm();
            }

            PrintTextToList();
        }

        private void PrintTextToList()
        {
            if (listBox1.SelectedItems.Count > 0)
            {
                int originIndex = listBox1.SelectedItems[0].Index;
                var selectedItem = listBox1.SelectedItems[0];
                if (tbxPort.Text.Trim() == "") tbxPort.Text = "0";
                //listBox1.Items.Remove(preRemovedItem);
                //ListViewItem listViewItem = listBox1.Items.Insert(originIndex, preRemovedItem);
                selectedItem.Text =
                    $@"{tbxProviderAddr.Text}:{tbxPort.Text}  => {tbxTargetServerAddr.Text}:{tbxTargetServerPort.Text}";
                //listBox1.Items[originIndex].Selected = true;
            }

            //configChanged = true;
        }

        private ClientApp GetClientAppFromForm()
        {
            ClientApp app = new ClientApp();
            app.Protocol = (Protocol)Enum.Parse(typeof(Protocol), cbxProtocol.Text);
            if (tbxTargetServerAddr.Text != "")
            {
                app.IP = tbxTargetServerAddr.Text;
            }

            if (tbxTargetServerPort.Text != "")
            {
                app.TargetServicePort = int.Parse(tbxTargetServerPort.Text);
            }

            if (tbxPort.Text != "")
            {
                app.ConsumerPort = int.Parse(tbxPort.Text);
            }

            app.Host = tbxHost.Text;
            app.Description = tbxDescription.Text;
            app.IsCompress = cbxIsCompress.Checked;
            return app;
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItems.Count == 0) return;
            int originIndex = listBox1.SelectedItems[0].Index;
            //int willingSelectIndex = -1;
            listBox1.Items.Remove(listBox1.SelectedItems[0]);
            if (listBox1.Items.Count > 0)
            {
                if (originIndex == listBox1.Items.Count) //如果元素后面没有元素，则选中上一个元素
                {
                    listBox1.Items[listBox1.Items.Count - 1].Selected = true;
                }
                else
                {
                    listBox1.Items[originIndex].Selected = true;
                }
            }


            //if (originIndex < listBox1.Items.Count && originIndex > 0)

            //configChanged = true;
        }

        private void btnDuplicate_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItems.Count > 0)
            {
                var item = new ListViewItem(listBox1.SelectedItems[0].Text);
                item.Tag = ((ClientApp)listBox1.SelectedItems[0].Tag).Clone();
                //listBox1.SelectedIndex =
                listBox1.Items.Add(item);
                item.Selected = true;
            }
        }


        private void tbxTargetServerPort_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != 8 && !Char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void 启动内网穿透ToolStripMenuItem_Paint(object sender, PaintEventArgs e)
        {
            if (IsNSPClientStarted)
            {
                this.启动内网穿透ToolStripMenuItem.Image = global::NSmartProxyWinform.Properties.Resources.base_checkmark_32;
            }
            else
            {
                this.启动内网穿透ToolStripMenuItem.Image = null;
            }
        }

        private void 启动内网穿透ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //StartOrStop();
            btnStart_Click(sender, e);
        }

        private void 配置ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowForm();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            RefreshFormFromConfig();
        }

        private void RefreshFormFromConfig()
        {
            config = ConfigHelper.ReadAllConfig<NSPClientConfig>(Program.CONFIG_FILE_PATH);
            tbxProviderAddr.Text = config.ProviderAddress;
            tbxWebPort.Text = config.ProviderWebPort.ToString();

            listBox1.Items.Clear();
            foreach (var confClient in config.Clients)
            {
                ListViewItem listViewItem = listBox1.Items.Add(
                    $@"{tbxProviderAddr.Text}:{confClient.ConsumerPort}  => {confClient.IP}:{confClient.TargetServicePort}");
                listViewItem.Tag = confClient;
                listViewItem.ImageKey = "stop";
            }

            cbxUseServerControl.Checked = config.UseServerControl;
        }


        private void btnSaveConfig_Click(object sender, EventArgs e)
        {
            if (ValidateConfig())
            {
                SaveFormDataToConfigFile();
                MessageBox.Show(L("保存成功"));
            }
        }

        private bool SaveFormDataToConfigFile()
        {
            config = new NSPClientConfig();
            //1.刷新配置
            config.ProviderAddress = tbxProviderAddr.Text;
            config.ProviderWebPort = int.Parse(tbxWebPort.Text);
            config.UseServerControl = cbxUseServerControl.Checked;
            //解决热心网友提出的bug：空值时无法保存。

            //2.保存配置到文件
            foreach (ListViewItem item in listBox1.Items)
            {
                var strParts = item.Text.Split(new string[]
                {
                    "=>", ":"
                }, StringSplitOptions.None);
                if (strParts.Length != 4)
                {
                    MessageBox.Show($"发现非法节点数据：“{item.Text}”");
                    listBox1.BackColor = Color.LightCoral;

                    return false;
                }

                try
                {
                    //校验clientApp
                    var clientApp = (ClientApp)item.Tag;
                    if (clientApp.Protocol == Protocol.HTTP && String.IsNullOrEmpty(clientApp.Host))
                    {
                        MessageBox.Show($"节点“{item.Text}”为HTTP映射，必须输入域名！若没有域名，请使用TCP协议映射。");
                        return false;
                    }

                    config.Clients.Add((ClientApp)item.Tag);
                }
                catch
                {
                    MessageBox.Show($"发现非法节点数据：“{item.Text}”");
                    return false;
                }
            }
            config.SaveChanges(Program.CONFIG_FILE_PATH);
            return true;
        }

        private void ClientMngr_Load(object sender, EventArgs e)
        {
            RefreshFormFromConfig();
            RegisterHotKey();
            RefreshWinServiceState();
            BindDDL();
            RefreshLoginBtnState();
            RefreshUserServerControlState();
        }

        private void RefreshLoginBtnState()
        {
            //TODO 3 有登录缓存文件，则判断为“已登录”，需要修改
            var userCacheItem = UserCacheManager.GetUserCacheFromEndpoint(GetEndPoint(), Router.NspClientCachePath);
            if (userCacheItem != null)
            {
                btnLogin.Image = Properties.Resources.logined;
                btnLogin.Text = " ";
                if (userCacheItem.UserName == "")
                {
                    btnLogin.Text += "匿名登录";
                }
                else
                {
                    btnLogin.Text += userCacheItem.UserName;
                }
            }
            else
            {
                btnLogin.Image = Properties.Resources.unlogin;
            }
        }

        private void BindDDL()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("id");
            dt.Columns.Add("value");
            foreach (var protocolName in Enum.GetNames(typeof(Protocol)))
            {
                dt.Rows.Add(protocolName, protocolName);
            }

            cbxProtocol.Items.Clear();
            cbxProtocol.DataSource = dt;
            cbxProtocol.DisplayMember = "value";
            cbxProtocol.ValueMember = "id";
        }

        private void RefreshWinServiceState()
        {
            if (WinServiceHelper.IsServiceExisted(Global.NSPClientServiceName))
            {
                btnRegWinSrv.Enabled = false;
                btnUnRegWinSrv.Enabled = true;
                RefreshServiceBtnStatus();
            }
            else
            {

                btnRegWinSrv.Enabled = true;
                btnUnRegWinSrv.Enabled = false;
            }
        }

        private void RegisterHotKey()
        {
            HotKey.RegisterHotKey(Handle, 100, HotKey.KeyModifiers.Shift | HotKey.KeyModifiers.Ctrl, Keys.O);
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_SYSCOMMAND = 0x112;
            //const int SC_CLOSE = 0xF060;
            const int SC_MINIMIZE = 0xF020;
            //const int SC_MAXIMIZE = 0xF030;
            const int WM_HOTKEY = 0x0312;
            //按快捷键    
            switch (m.Msg)
            {
                case WM_HOTKEY:
                    switch (m.WParam.ToInt32())
                    {
                        case 100:    //按下的是Ctrl+Shift+O   
                            {
                                if (this.Visible == true)
                                {
                                    this.Hide();
                                    this.notifyIconNSPClient.Visible = false;
                                }
                                else
                                {
                                    this.Show();
                                    this.notifyIconNSPClient.Visible = true;
                                }
                            }; break;

                    }
                    break;
                case WM_SYSCOMMAND: //防止最小化跑到右下角
                    if (m.WParam.ToInt32() == SC_MINIMIZE)
                    {
                        this.Visible = false;
                        return;
                    }
                    break;
            }
            base.WndProc(ref m);
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (tbxProviderAddr.Text.Trim() == "" || tbxWebPort.Text.Trim() == "")
            {
                string msg = L("请将外网服务器地址和端口填写完整");
                MessageBox.Show(msg);
                errorProvider1.SetError(tbxProviderAddr, msg);
                errorProvider1.SetError(tbxWebPort, msg);
                return;

            }

            Login frmLogin = new Login(clientRouter, this);
            frmLogin.StartPosition = FormStartPosition.CenterScreen;
            frmLogin.ShowDialog();
            if (frmLogin.Success)
            {
                if (frmLogin.Username.Trim() == "")
                {
                    btnLogin.Text = "匿名登录";
                }
                else
                {
                    btnLogin.Text = frmLogin.Username;
                }
            }
        }


        //日志
        private void tabPage2_Enter(object sender, EventArgs e)
        {
            //滚动到最下行 TODO 不起作用？！
            tbxLog.SelectionStart = this.tbxLog.Text.Length;
            tbxLog.SelectionLength = 0;
            tbxLog.ScrollToCaret();
            btnOpenInExplorer.Show();
            btnLogin.Hide();
        }

        //应用
        private void tabPage1_Enter(object sender, EventArgs e)
        {
            btnOpenInExplorer.Hide();
            btnLogin.Show();
        }

        private void btnRegWinSrv_Click(object sender, EventArgs e)
        {
            //如果当前开启了服务则先关闭
            if (IsNSPClientStarted)
            {
                btnStart_Click(this, null);
            }
            if (WinServiceHelper.IsServiceExisted(Global.NSPClientServiceName))
                WinServiceHelper.UninstallService(SERVICE_PATH);
            WinServiceHelper.InstallService(SERVICE_PATH);
            RefreshWinServiceState();
        }

        private void btnUnRegWinSrv_Click(object sender, EventArgs e)
        {
            //如果当前开启了服务则先关闭
            if (IsNSPClientStarted)
            {
                btnStart_Click(this, null);
            }
            if (WinServiceHelper.IsServiceExisted(Global.NSPClientServiceName))
                WinServiceHelper.UninstallService(SERVICE_PATH);
            RefreshWinServiceState();
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            btnTest.Enabled = false;
            Application.DoEvents();
            NSPDispatcher clientDispatcher = new NSPDispatcher($"{tbxProviderAddr.Text}:{tbxWebPort.Text}");
            TcpClient tcpclient = new TcpClient();
            try
            {
                var result = clientDispatcher.GetServerPorts().Result;
                if (result.State == 1)
                {

                    var reversePort = result.Data.ReversePort;
                    var configPort = result.Data.ConfigPort;

                    string errMsg = "";
                    try
                    {
                        tcpclient.Connect(tbxProviderAddr.Text, reversePort);
                    }
                    catch
                    {
                        errMsg += $"端口 {reversePort} 测试不通过;";
                    }

                    try
                    {
                        tcpclient.Connect(tbxProviderAddr.Text, configPort);
                    }
                    catch
                    {
                        errMsg += $"端口 {configPort} 测试不通过;";
                    }
                    if (errMsg == "")
                    {
                        MessageBox.Show(errMsg);
                    }
                    else
                    {
                        MessageBox.Show($"配置端口：反向连接端口{reversePort},配置端口{configPort}，测试通过！");
                    }
                }
                else
                {
                    MessageBox.Show(L(" 获取端口配置失败，服务端返回错误如下：") + result.Msg);
                }
            }
            catch (Exception ex)
            {
                logger.Debug(ex.ToString());
                MessageBox.Show($"{tbxProviderAddr.Text}:{tbxWebPort.Text}连接失败。");
            }
            finally
            {
                tcpclient.Close();
            }


            //taskwhenall
            btnTest.Enabled = true;
        }

        public string GetEndPoint()
        {
            return tbxProviderAddr.Text + ":" +
                   tbxWebPort.Text; //ClientConfig.ProviderAddress + ":" + ClientConfig.ProviderWebPort;
        }

        private void ClientMngr_Activated(object sender, EventArgs e)
        {
            this.ShowInTaskbar = true;
        }

        private void cbxUseServerControl_CheckedChanged(object sender, EventArgs e)
        {
            RefreshUserServerControlState();
        }

        private void RefreshUserServerControlState()
        {
            if (cbxUseServerControl.Checked)
            {
                grpNodeSettings.Enabled = false;
            }
            else
            {
                grpNodeSettings.Enabled = true;
            }
        }
    }
}
