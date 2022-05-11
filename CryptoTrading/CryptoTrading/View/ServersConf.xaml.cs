using System;
using System.Text;
using System.Windows;
using System.Net.NetworkInformation;



namespace CryptoTrading
{
    /// <summary>
    /// Interaction logic for ProxyServersConf.xaml
    /// </summary>
    public partial class ServersConf : Window
    {
        #region variables

        #endregion

        public ServersConf()
        {
            InitializeComponent();

        }

        public void pingtest()
        {
            /*if (args.Length == 0)
                throw new ArgumentException("Ping needs a host or IP Address.");

            string who = args[0];
           
            IPAddress address = IPAddress.Loopback;
            */
            // Create a buffer of 32 bytes of data to be transmitted.
            string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            Ping pingSender = new Ping();
            // Wait 5 seconds for a reply.
            int timeout = 5000;

            // Set options for transmission:
            // The data can go through 64 gateways or routers
            // before it is destroyed, and the data packet
            // cannot be fragmented.
            PingOptions options = new PingOptions(64, true);
            PingReply[] tradingreply = new PingReply[Trader.TradingServer.Count];
            PingReply[] marketreply = new PingReply[Trader.MarketServer.Count];            
            MarketTestReply[] marketserver = new MarketTestReply[Trader.MarketServer.Count];
            TradingTestReply[] tradingserver = new TradingTestReply[Trader.TradingServer.Count];
            string s, t;
            try
            {
                for (int i = 0; i < Trader.TradingServer.Count; i++)
                {
                    s = Trader.TradingServer[i];
                    int j = s.IndexOf(':');
                    t = s.Substring(j + 1);
                    s = s.Substring(0, j);

                    tradingreply[i] = pingSender.Send(s, timeout, buffer, options);

                    tradingserver[i] = new TradingTestReply();
                    tradingserver[i].交易服务器地址 = s;
                    tradingserver[i].端口 = t;
                    if (tradingreply[i].Status == IPStatus.Success)
                    {
                        tradingserver[i].网络状况 = (tradingreply[i].RoundtripTime / 2).ToString() + "ms";
                    }
                    else
                    {
                        tradingserver[i].网络状况 = "连接失败";
                    }
                }
                for (int i = 0; i < Trader.MarketServer.Count; i++)
                {
                    s = Trader.MarketServer[i];
                    int j = s.IndexOf(':');
                    t = s.Substring(j + 1);
                    s = s.Substring(0, j);
                    marketreply[i] = pingSender.Send(s, timeout, buffer, options);
                    marketserver[i] = new MarketTestReply();
                    marketserver[i].行情服务器地址 = s;
                    marketserver[i].端口 = t;
                    if (marketreply[i].Status == IPStatus.Success)
                    {
                        marketserver[i].网络状况 = (tradingreply[i].RoundtripTime / 2).ToString() + "ms";
                    }
                    else
                    {
                        marketserver[i].网络状况 = "连接失败";
                    }
                }

            }
            catch (Exception e)
            {
                Utility.WriteMemLogToLogFile(new string []{ e.ToString()});
            }
            //////////MarketData.
            MarketData.ItemsSource = marketserver;
            Trading.ItemsSource = tradingserver;
        }


        private void Grid_Initialized(object sender, EventArgs e)
        {
            pingtest();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
    class TradingTestReply
    {
        public string 交易服务器地址 { get; set; }
        public string 端口 { get; set; }
        public string 网络状况 { get; set; }

    }
    class MarketTestReply
    {
        public string 行情服务器地址
        { get; set; }

        public string 端口 { get; set; }
        public string 网络状况 { get; set; }

    }
}
