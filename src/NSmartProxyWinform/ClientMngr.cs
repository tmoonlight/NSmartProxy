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
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NSmartProxyWinform
{
    public partial class ClientMngr : Form
    {

        public class Log4netLogger : INSmartLogger
        {
            public delegate void BeforeWriteLogDelegate(object message);
            public BeforeWriteLogDelegate BeforeWriteLog;
            public void Debug(object message)
            {
                BeforeWriteLog(message);
                Program.Logger.Debug(message);
            }

            public void Error(object message, Exception ex)
            {
                BeforeWriteLog(message);
                Program.Logger.Error(message, ex);
            }

            public void Info(object message)
            {
                BeforeWriteLog(message);
                Program.Logger.Info(message);
            }
        }
        Router clientRouter;
        public ClientMngr()
        {
            InitializeComponent();
            Log4netLogger logger = new Log4netLogger();
            logger.BeforeWriteLog = (msg) => { ShowInfo(msg.ToString()); };
            clientRouter = new Router(logger);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ((Button)sender).Enabled = false;
            
            //read config from config file.
            SetConfig(clientRouter);// clientRouter.SetConifiguration();
            var tsk = clientRouter.ConnectToProvider().ConfigureAwait(false);
            //try
            // {
            tsk.GetAwaiter();
            // }
            // catch (Exception ex)
            // {
            // Logger.Error(ex);
            //   throw;
            // }
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
            // Configuration.GetSection("1").
            clientRouter.SetConifiguration(config);
        }

        public void ShowInfo(string info)
        {
            textBox1.Invoke(
                new Action(()=>
                {
                    textBox1.AppendText(info);
                    textBox1.AppendText(Environment.NewLine);
                    textBox1.ScrollToCaret();
                }
            )); 
        }

        private void button2_Click(object sender, EventArgs e)
        {
            clientRouter.Close();
            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string x = "";
            foreach (var tcpclient in clientRouter.ConnnectionManager.ConnectedConnections)
            {
               x+= tcpclient.GetHashCode() + " " + tcpclient.Connected + " " + Environment.NewLine;
            }
            MessageBox.Show(x);
        }
    }
}
