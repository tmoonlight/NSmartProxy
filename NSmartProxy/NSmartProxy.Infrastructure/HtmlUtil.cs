using System;
using System.Collections.Generic;
using System.Text;

namespace NSmartProxy
{
    public static class HtmlUtil
    {
        //简单html请求解析
        public static byte[] GetUtf8Content(byte[] topBytes)
        {
            //

            StringBuilder http = new StringBuilder();

            http.AppendLine("HTTP/1.0 200 OK");//http1.0实现的服务器，方便起见，不采取半连接。
            http.AppendLine("Content-type:text/html");
            http.AppendLine("Connection:close");


            StringBuilder html = new StringBuilder();

            html.AppendLine("<html>");
            html.AppendLine("<head>");
            html.AppendLine("<title>TMoonlight</title>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            html.AppendLine("Hello world!<form name='formshao' method='get' action='result#anc1'><input type='text' name='txtTest' /><input type='submit' value='testformsubmit'/></form>");
            html.AppendLine("</body>");
            html.AppendLine("</html>");

            http.AppendLine("Content-Length:" + html.Length);//由此计算出html长度
            http.AppendLine();
            http.AppendLine(html.ToString());

            return Encoding.UTF8.GetBytes(http.ToString());
        }
    }
}
