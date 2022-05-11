using Microsoft.Win32;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace CryptoTrading
{
    /// <summary>
    /// Interaction logic for SettlementWindow.xaml
    /// </summary>
    public partial class SettlementWindow : System.Windows.Window
    {
        private QueryRangeType queryType = QueryRangeType.Today;

       private string tradingDay;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type">查询范围类型</param>
       public SettlementWindow(TQMain mainwin, QueryRangeType type)
        {
            InitializeComponent();

            queryType = type;        
            Owner = mainwin;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TQMain.T.settlementWin = (SettlementWindow)this;
            SettlementDateDP.SelectedDate = DateTime.Now;
            tradingDay = DateTime.Now.Date.ToString("yyyyMMdd");
            //if (queryType==QueryRangeType.UserDefined)
            //{
            //    SettlementDateDP.SelectedDate = DateTime.Now;
            //    tradingDay = DateTime.Now.Date.ToString("yyyyMMdd");
            //}
            //else
            //{
            //    //如果不是自定义的结算单数据查询日期范围，则不显示 日期选择，查询日结算单、查询月结算单的功能空间
            //    SettlementDateDP.Visibility = Visibility.Collapsed;
            //    TodaySettlementBtn.Visibility = Visibility.Collapsed;
            //    MonthSettlementBtn.Visibility = Visibility.Collapsed;
            //    Title = EnumDescription.GetFieldText(queryType) + Title;
            //    switch (queryType)
            //    {
            //        case QueryRangeType.Today:
            //            tradingDay = DateTime.Now.Date.ToString("yyyyMMdd");
            //            break;
            //        case QueryRangeType.NearWeek:
            //            tradingDay = DateTime.Now.AddDays(-7).Date.ToString("yyyyMMdd");
            //            break;
            //        case QueryRangeType.NearMonth:
            //            tradingDay = DateTime.Now.Date.ToString("yyyyMM");
            //            break;
            //    }
            //}
        }
        private void DaySettlementBtn_Click(object sender, RoutedEventArgs e)
        {
            SettlementTB.Text = string.Empty;
            if (!SettlementDateDP.SelectedDate.HasValue)
            {
                return;
            }
            tradingDay = SettlementDateDP.SelectedDate.Value.ToString("yyyyMMdd");
            string settleFile = Trader.Configuration.Investor.ID + "/Settlements/" + tradingDay + ".txt";
            if (File.Exists(settleFile))
            {
                SettlementTB.Text = File.ReadAllText(settleFile, Encoding.UTF8);
            }
            else
            {
                QrySettlementInfoField field = new QrySettlementInfoField()
                {
                    BrokerID = Trader.DefaultBrokerID,
                    InvestorID = Trader.Configuration.Investor.ID,
                    TradingDay = tradingDay
                };
                TQMain.T.ReqQrySettlementInfo(field);
            }
        }

        private void MonthSettlementBtn_Click(object sender, RoutedEventArgs e)
        {
            SettlementTB.Text = string.Empty;
            if (!SettlementDateDP.SelectedDate.HasValue)
            {
                return;
            }
            tradingDay = SettlementDateDP.SelectedDate.Value.ToString("yyyyMM");
            string settleFile = Trader.Configuration.Investor.ID + "/Settlements/" + tradingDay + ".txt";
            if (File.Exists(settleFile))
            {
                SettlementTB.Text = File.ReadAllText(settleFile, Encoding.UTF8);
            }
            else
            {
                QrySettlementInfoField field = new QrySettlementInfoField()
                {
                    BrokerID = Trader.DefaultBrokerID,
                    InvestorID = Trader.Configuration.Investor.ID,
                    TradingDay = tradingDay
                };
                TQMain.T.ReqQrySettlementInfo(field);
            }
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            // Configure save file dialog
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.FileName =string.Format("结算单-{0}",tradingDay); // Default file name
            dlg.DefaultExt = ".txt"; // Default file extension
            dlg.Filter = "Text documents (.txt)|*.txt|XPS Documents|*.xps"; // Filter files by extension

            // Show save file dialog
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog results
            if (result == true)
            {
                // Save document
                string filename = dlg.FileName;
                File.WriteAllText(filename, SettlementTB.Text, Encoding.UTF8);
            }
        }

        private void PrintBtn_Click(object sender, RoutedEventArgs e)
        {          
            // Configure printer dialog
            PrintDialog pDialog = new PrintDialog();
            pDialog.PageRangeSelection = PageRangeSelection.AllPages;
            pDialog.UserPageRangeEnabled = true;
            pDialog.UserPageRangeEnabled = true;
            // Show save file dialog
            Nullable<bool> result = pDialog.ShowDialog();

            // Process save file dialog results
            if (result == true)
            {   
                // Print document
                pDialog.PrintVisual(SettlementTB, "Print Settlement");              
            }
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            TQMain.T.settlementWin = null;
        }
    }
}
