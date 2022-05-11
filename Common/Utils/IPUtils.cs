using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace TickQuant.Common
{
    public class IPUtils
    {

        public static string GetInternalIP()
        {
            string ip = "127.0.0.1";
            try
            {
                WebClient MyWebClient = new WebClient();
                MyWebClient.Credentials = CredentialCache.DefaultCredentials;//获取或设置用于向Internet资源的请求进行身份验证的网络凭据

                Byte[] pageData = MyWebClient.DownloadData("http://www.net.cn/static/customercare/yourip.asp"); //从指定网站下载数据

                string pageHtml = Encoding.Default.GetString(pageData); 
                // 正则表达式匹配
                string myReg = @"<h2>(.*?)</h2>";
                Match mc = Regex.Match(pageHtml, myReg, RegexOptions.Singleline);
                if (mc.Success && mc.Groups.Count > 1)
                {
                       return mc.Groups[1].Value;
                }
            }
            catch (WebException webEx)
            {
                webEx.Message.ToString();
            }
            return ip;
        }
        public static string GetLocalIP()
        {
            try
            {

                string HostName = Dns.GetHostName(); //得到主机名
                IPHostEntry IpEntry = Dns.GetHostEntry(HostName);
                for (int i = 0; i < IpEntry.AddressList.Length; i++)
                {
                    //从IP地址列表中筛选出IPv4类型的IP地址
                    //AddressFamily.InterNetwork表示此IP为IPv4,
                    //AddressFamily.InterNetworkV6表示此地址为IPv6类型
                    if (IpEntry.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
                    {
                        return IpEntry.AddressList[i].ToString();
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetLocalIP Exception: {ex.Message}");
            }
            return "127.0.0.1";
        }
    }
}
