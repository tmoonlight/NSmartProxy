using System;
using System.Collections.Generic;
using System.Text;

namespace NSmartProxy
{
    public static class HtmlUtil
    {
        public static byte[] GetUtf8Content()
        {
            StringBuilder http = new StringBuilder();

            http.AppendLine("HTTP/1.0 200 OK");//这些字，就代表了是http协议。
            http.AppendLine("Content-type:text/html");
            http.AppendLine("Connection:close");


            StringBuilder html = new StringBuilder();

            html.AppendLine("<html>");
            html.AppendLine("<head>");
            html.AppendLine("<title>hello</title>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            html.AppendLine("Hello world!");
            html.AppendLine("</body>");
            html.AppendLine("</html>");

            http.AppendLine("Content-Length:" + html.Length);//由此计算出html长度
            http.AppendLine();
            http.AppendLine(html.ToString());

            return Encoding.UTF8.GetBytes(http.ToString());
        }
    }
}
