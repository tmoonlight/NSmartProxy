using System;
using System.Diagnostics;

namespace NSmartProxy.Interfaces
{
    public interface INSmartLogger
    {
        void Debug(object message);
       
        void Error(object message,Exception ex);

        void Info(object message);
    }
}