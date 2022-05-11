using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;

namespace CryptoTrading
{
    /// <summary>
    /// Interaction logic for TradeLogWindow.xaml
    /// </summary>
    public partial class TradeLogWindow : Window
    {
        public TradeLogWindow()
        {
            InitializeComponent();
            dpTrade.SelectedDate = DateTime.Now;
        }

        private void dpTrade_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
           
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string date = dpTrade.SelectedDate.Value.ToString("yyyyMMdd");
                string  tradelogFile = string.Format("{0}/{1}/{2}.log", Trader.Configuration.Investor.ID, "TradeLog", date);
                if (File.Exists(tradelogFile))
                {
                    string tradeLog = File.ReadAllText(tradelogFile, Encoding.UTF8);
                    txtTradeLog.Text = tradeLog;
                }
            }
            catch (Exception ex)
            { SimpleLogger.Logger.Log(ex.Message, SimpleLogger.LogCategory.Info); }
        }
    }
}
