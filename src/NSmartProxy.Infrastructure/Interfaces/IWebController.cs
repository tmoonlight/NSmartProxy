using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace NSmartProxy.Infrastructure.Interfaces
{
    public interface IWebController
    {
        void SetContext(HttpListenerContext context);
    }
}
