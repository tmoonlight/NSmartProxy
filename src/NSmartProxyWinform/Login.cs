using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NSmartProxy.Client;
using NSmartProxy.ClientRouter.Dispatchers;
using NSmartProxy.Shared;

namespace NSmartProxyWinform
{
    public partial class Login : Form
    {
        private Router clientRouter;
        private ClientMngr parentForm;
        public bool Success = false;

        public const int DEFAULT_WEB_PORT = 12309;//TODO 暂时写死，以后再改
        public Login(Router router, ClientMngr frm)
        {
            InitializeComponent();
            clientRouter = router;
            parentForm = frm;
        }

        private void cbxIsAnonymous_CheckedChanged(object sender, EventArgs e)
        {
            var obj = (CheckBox)sender;
            //tbxPassword.Enabled = obj.Checked;
            tbxPassword.Enabled = tbxUser.Enabled = !obj.Checked;
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (cbxIsAnonymous.Checked)
            {
                ClearLoginCache();
                this.Close();
                return;
            }

            if (tbxPassword.Text == "" || tbxUser.Text == "")
            {
                MessageBox.Show("请将用户名和密码输入完整", "提示",
                    MessageBoxButtons.OK
                    , MessageBoxIcon.Warning);
                return;

            }


            string baseHttpPath = null;
            if (clientRouter == null)
            {
                var providerAddr = parentForm.tbxProviderAddr.Text;
                if (string.IsNullOrEmpty(providerAddr))
                {
                    MessageBox.Show("请先在主窗体设置“服务器地址”");
                }

                baseHttpPath = $"{providerAddr}:{DEFAULT_WEB_PORT}";
            }
            else
            {
                var config = clientRouter.ConnectionManager.ClientConfig;
                baseHttpPath = $"http://{config.ProviderAddress}:{config.ProviderWebPort}";
            }

            btnLogin.Enabled = false;
            NSPDispatcher dispatcher = new NSPDispatcher(baseHttpPath);
            var connectAsync = dispatcher.LoginFromClient(tbxPassword.Text, tbxUser.Text);
            var delayDispose = Task.Delay(TimeSpan.FromSeconds(5000));
            var comletedTask = Task.WhenAny(delayDispose, connectAsync).Result;
            if (!connectAsync.IsCompleted) //超时
            {
                MessageBox.Show("连接超时");
            }
            else if (connectAsync.IsFaulted)//出错
            {
                MessageBox.Show(connectAsync.Exception.ToString());
            }
            else
            {
                MessageBox.Show("登录成功");

                CreateLoginCache(connectAsync.Result.Data.Token);
                Success = true;
                this.Close();
            }
            btnLogin.Enabled = true;
        }


        public void ClearLoginCache()
        {
            File.Delete(Router.NSMART_CLIENT_CACHE_PATH);
        }
        public void CreateLoginCache(string token)
        {
            File.WriteAllText(Router.NSMART_CLIENT_CACHE_PATH, token);
        }
    }
}
