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
using NSmartProxy.Infrastructure;
using NSmartProxy.Shared;
using NSmartProxyWinform.Util;
using NSmartProxy.ClientRouter.Dispatchers;
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
                return WinServiceHelper.IsServiceExisted(Global.ServiceName);
            }
        }

        public bool IsStarted
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
            RefreshLoginState();


            //界面的一些细节初始化
            btnLogin.Location = new Point(12, btnLogin.Location.Y);
            btnOpenInExplorer.Hide();

            //有登录缓存文件，则判断为“已登录”
            if (File.Exists(Router.NspClientCachePath)) btnLogin.Text = " 已登录";

            this.notifyIconNSPClient.Text = Global.NSmartProxyClientName;

            UpdateText();
        }

        private void UpdateText()
        {
            this.notifyIconNSPClient.Text = Global.NSmartProxyClientName;
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
            groupBox2.Text = L("节点配置");
            label7.Text = L("* : 外网端口为0或者空则代表端口由服务端自动分派。");
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
        }

        private void RefreshLoginState()
        {
            if (File.Exists(Router.NspClientCachePath))
            {
                btnLogin.Image = Properties.Resources.logined;
            }
            else
            {
                btnLogin.Image = Properties.Resources.unlogin;
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
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
                    using (ServiceController control = new ServiceController(Global.ServiceName))
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
                                    listBox1.Items.Clear();
                                    foreach (var tunnel in tunelStr)
                                    {
                                        notifyIconNSPClient.BalloonTipText += "\r\n" + tunnel.ToString();
                                        listBox1.Items.Add(tunnel.Substring(tunnel.IndexOf(':') + 1).Trim());
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
                    using (ServiceController control = new ServiceController(Global.ServiceName))
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
                            listBox1.ForeColor = Color.Black;
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
                ServiceController control = new ServiceController(Global.ServiceName);

                Task tskRunning = Task.Run(() => { try { control.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0, 0, 4)); } catch { } });
                Task tskStopping = Task.Run(() => { try { control.WaitForStatus(ServiceControllerStatus.Stopped, new TimeSpan(0, 0, 4)); } catch { } });
                Task tskTimeout = Task.Delay(3000);
                Task resultTsk = Task.WhenAny(tskRunning, tskStopping, tskTimeout);
                resultTsk.ContinueWith(t => { control.Close(); });
                resultTsk.Wait();
                if (tskRunning.IsCompleted)
                {
                    string tip = $"内网穿透后台服务({Global.ServiceName})已启动，详情请查看文件日志";
                    SetUIToRunning();
                    notifyIconNSPClient.BalloonTipText = tip;
                    ShowLogInfo(tip);
                }
                else if (tskStopping.IsCompleted)
                {
                    SetUIToStop();
                    ShowLogInfo($"内网穿透后台服务({Global.ServiceName})已激活，但处于关闭状态。");
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
            //TaskScheduler.UnobservedTaskException +=
            //    (_, ev) => logger.Error(ev.Exception.Message, ev.Exception);
            //read config from config file.
            SetConfig(clientRouter, config);// clientRouter.SetConifiguration();
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
            if (listBox1.SelectedItem?.ToString() == NULL_CLIENT_TEXT)
            {
                tbxPort.Clear();
                tbxTargetServerAddr.Clear();
                tbxTargetServerPort.Clear();
            }
            else//节点配置显示
            if (listBox1.SelectedItem != null)
            {
                var strSelectedItemStr = listBox1.SelectedItem.ToString();
                var strParts = strSelectedItemStr.Split(new string[]
                {
                    "=>", ":"
                }, StringSplitOptions.None);
                if (strParts.Length != 4)
                {
                    MessageBox.Show(L("非法选择项"));
                    return;
                }

                tbxPort.Text = strParts[1].Trim();
                tbxTargetServerAddr.Text = strParts[2].Trim();
                tbxTargetServerPort.Text = strParts[3].Trim();
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
            int index = listBox1.Items.Add(NULL_CLIENT_TEXT);
            listBox1.SelectedIndex = index;
        }



        private void targetServer_TextChanged(object sender, EventArgs e)
        {
            printTextToList();
        }

        private void printTextToList()
        {
            if (listBox1.SelectedItem != null)
            {
                int originIndex = listBox1.SelectedIndex;
                if (tbxPort.Text.Trim() == "") tbxPort.Text = "0";
                listBox1.Items.Remove(listBox1.SelectedItem);

                listBox1.Items.Insert(originIndex,
                    $@"{tbxProviderAddr.Text}:{tbxPort.Text}  => {tbxTargetServerAddr.Text}:{tbxTargetServerPort.Text}");
                listBox1.SelectedIndex = originIndex;
            }

            //configChanged = true;
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            int originIndex = listBox1.SelectedIndex;
            listBox1.Items.Remove(listBox1.SelectedItem);
            if (originIndex < listBox1.Items.Count)
                listBox1.SelectedIndex = originIndex;
            //configChanged = true;
        }

        private void btnDuplicate_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                listBox1.SelectedIndex =
                    listBox1.Items.Add(listBox1.SelectedItem.ToString());
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
            if (IsStarted)
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
            NSPClientConfig conf = ConfigHelper.ReadAllConfig<NSPClientConfig>(Program.CONFIG_FILE_PATH);
            tbxProviderAddr.Text = conf.ProviderAddress;
            //tbxConfigPort.Text = conf.ProviderConfigPort.ToString();
            tbxWebPort.Text = conf.ProviderWebPort.ToString();
            //if(tbxReversePort.Text == "" ? tbxReversePort.Text = "0";

            listBox1.Items.Clear();
            foreach (var confClient in conf.Clients)
            {
                listBox1.Items.Add(
                    $@"{tbxProviderAddr.Text}:{confClient.ConsumerPort}  => {confClient.IP}:{confClient.TargetServicePort}");
            }
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
            //config.ProviderConfigPort = int.Parse(tbxConfigPort.Text);
            //config.ProviderPort = int.Parse(tbxReversePort.Text);
            config.ProviderWebPort = int.Parse(tbxWebPort.Text);
            //解决热心网友提出的bug：空值时无法保存。

            //2.保存配置到文件
            foreach (var item in listBox1.Items)
            {
                var strParts = item.ToString().Split(new string[]
                {
                    "=>", ":"
                }, StringSplitOptions.None);
                if (strParts.Length != 4)
                {
                    MessageBox.Show($"发现非法节点数据：“{item}”");
                    listBox1.BackColor = Color.LightCoral;

                    return false;
                }

                try
                {

                    config.Clients.Add(new ClientApp
                    {
                        ConsumerPort = Convert.ToInt32(strParts[1]),
                        IP = strParts[2].Trim(),
                        TargetServicePort = Convert.ToInt32(strParts[3])
                    });
                }
                catch
                {
                    MessageBox.Show($"发现非法节点数据：“{item}”");
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

        }

        private void RefreshWinServiceState()
        {
            if (WinServiceHelper.IsServiceExisted(Global.ServiceName))
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
            }
            base.WndProc(ref m);
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            Login frmLogin = new Login(clientRouter, this);
            frmLogin.StartPosition = FormStartPosition.CenterScreen;
            frmLogin.ShowDialog();
            if (frmLogin.Success) btnLogin.Text = L("已登录");
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
            if (WinServiceHelper.IsServiceExisted(Global.ServiceName))
                WinServiceHelper.UninstallService(SERVICE_PATH);
            WinServiceHelper.InstallService(SERVICE_PATH);
            //using (ServiceController control = new ServiceController(Global.ServiceName))
            //{
            //    if (control.Status == ServiceControllerStatus.Stopped)
            //    {
            //        control.Start();
            //    }
            //}
            RefreshWinServiceState();
        }

        private void btnUnRegWinSrv_Click(object sender, EventArgs e)
        {
            if (WinServiceHelper.IsServiceExisted(Global.ServiceName))
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
    }
}
