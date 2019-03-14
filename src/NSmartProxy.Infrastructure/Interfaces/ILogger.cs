using System;
using System.Diagnostics;

namespace NSmartProxy.Interfaces
{
    public interface INSmartLogger
    {
        void Debug(string message);
       
        void Error(string message,Exception ex);
    }
}