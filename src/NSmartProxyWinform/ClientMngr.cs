using log4net;
using NSmartProxy.Client;
using NSmartProxy.Data;
using NSmartProxy.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using log4net.Appender;

namespace NSmartProxyWinform
{
    public partial class ClientMngr : Form
    {
        Router clientRouter;
        private Log4netLogger logger;

        public ClientMngr()
        {
            InitializeComponent();
            //将日志写入窗体中。
            logger = new Log4netLogger();
            logger.BeforeWriteLog = (msg) => { ShowInfo(msg.ToString()); };
        }

        private void button1_Click(object sender, EventArgs e)
        {
            StartClientRouter();
            btnStart.Enabled = false;
            btnEnd.Enabled = true;
        }

        private void StartClientRouter()
        {
            clientRouter = new Router(logger);
            clientRouter.DoServerNoResponse = () =>
            {
                clientRouter.Close();
                logger.Info("服务器无响应，五秒后重试...");
                Thread.Sleep(5000);
                StartClientRouter(); 
            };
                
            //read config from config file.
            SetConfig(clientRouter);// clientRouter.SetConifiguration();
            var tsk = clientRouter.ConnectToProvider().ConfigureAwait(false);
        }

        private void SetConfig(Router clientRouter)
        {
            Config config = new Config();
            var Configuration = Program.Configuration;
            config.ProviderAddress = Configuration.GetSection("ProviderAddress").Value;
            config.ProviderPort = int.Parse(Configuration.GetSection("ProviderPort").Value);
            config.ProviderConfigPort = int.Parse(Configuration.GetSection("ProviderConfigPort").Value);
            var configClients = Configuration.GetSection("Clients").GetChildren();
            foreach (var cli in configClients)
            {
                int confConsumerPort = 0;
                if (cli["ConsumerPort"] != null) confConsumerPort = int.Parse(cli["ConsumerPort"]);
                config.Clients.Add(new ClientApp
                {
                    IP = cli["IP"],
                    TargetServicePort = int.Parse(cli["TargetServicePort"]),
                    ConsumerPort = confConsumerPort
                });
            }
            clientRouter.SetConifiguration(config);
        }

        public void ShowInfo(string info)
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

        private void button2_Click(object sender, EventArgs e)
        {
            btnStart.Enabled = true;
            btnEnd.Enabled = false;
            var tsk = clientRouter.Close().ConfigureAwait(false);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var appender = LogManager.GetRepository(Program.LOGGER_REPO_NAME).GetAppenders()
                .Where((o) => o.GetType() == typeof(FileAppender)).First();
            var filePath = ((FileAppender)appender).File;

            //记录日志
            string argument = "/select, \"" + filePath + "\"";
            //Logging.Debug(argument);
            System.Diagnostics.Process.Start("explorer.exe", argument);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            notifyIconNSPClient.Dispose();
            Application.Exit();
        }

        private void ClientMngr_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        private void notifyIconNSPClient_DoubleClick(object sender, EventArgs e)
        {
            this.Show();
        }
    }
}
