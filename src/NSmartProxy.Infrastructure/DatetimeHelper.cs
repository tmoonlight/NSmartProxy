using System;
using System.Collections.Generic;
using System.Text;

namespace NSmartProxy.Infrastructure
{
    public class DateTimeHelper
    {
        public static double TimeRange(DateTime From, DateTime To)
        {
            return (To - From).TotalMilliseconds;
        }
    }
}
