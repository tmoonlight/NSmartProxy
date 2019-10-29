using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Threading.Tasks;
using NSmartProxy.Shared;

namespace NSmartProxyWinService
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
            this.serviceInstaller1.DisplayName = Global.NSPClientServiceDisplayName;
            this.serviceInstaller1.ServiceName = Global.NSPClientServiceName;
        }
    }
}
