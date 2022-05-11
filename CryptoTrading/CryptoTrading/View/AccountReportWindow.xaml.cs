using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;


namespace CryptoTrading
{
    /// <summary>
    /// Interaction logic for AccountReportWindow.xaml
    /// </summary>
    public partial class AccountReportWindow : System.Windows.Window
    {
        private System.Windows.Forms.Integration.WindowsFormsHost host = new System.Windows.Forms.Integration.WindowsFormsHost();

        private System.Windows.Forms.WebBrowser reportBrowser = new System.Windows.Forms.WebBrowser();

        private QueryRangeType queryRangeType = QueryRangeType.UserDefined;

        private TradingAccountData tradingAccountData;
        private DateTime startDate;
        private DateTime endDate;
        private int TradingDayNum=0;//报告范围内的交易日个数
        private string htmlFile = "include/Report.html";
        private string accountID = string.Empty;
        private string brokerID = string.Empty;
        public static readonly ConcurrentDictionary<string, List<TradeField>> dicTradeData = new ConcurrentDictionary<string, List<TradeField>>();

        /// <summary>
        /// 结算信息的内容（Key:交易账号_交易日）
        /// </summary>
        public static readonly ConcurrentDictionary<string, string> dicSettlementContentData = new ConcurrentDictionary<string, string>();
        /// <summary>
        ///
        /// </summary>
        /// <param name="main"></param>
        /// <param name="rangeType"></param>
        public AccountReportWindow(TQMain main,QueryRangeType rangeType)
        {
            InitializeComponent();

            Owner = main;
            queryRangeType = rangeType;
            if(main.TradingAccountDataView!=null && main.TradingAccountDataView.Count>0)
            {
                tradingAccountData = main.TradingAccountDataView[0];
            }
            accountID = Trader.Configuration.Investor.ID;
            brokerID = Trader.DefaultBrokerID;
        }

        public AccountReportWindow(QueryRangeType rangeType)
        {
            InitializeComponent();

            queryRangeType = rangeType;
            //if (main.TradingAccountDataView != null && main.TradingAccountDataView.Count > 0)
            //{
            //    tradingAccountData = main.TradingAccountDataView[0];
            //}
            accountID = Trader.Configuration.Investor.ID;
            brokerID = Trader.DefaultBrokerID;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (File.Exists(htmlFile))
            {
                File.Delete(htmlFile);
            }
            

            startDate = endDate = DateTime.Now.Date;
            //init the control
            switch (queryRangeType)
            {
                case QueryRangeType.UserDefined:
                    userdefinedRadBtn.IsChecked = true;
                    startDatePicker.SelectedDate = startDate;
                    endDatePicker.SelectedDate = endDate;
                    break;
                case QueryRangeType.Today:
                    todayRadBtn.IsChecked = true;
                    break;
                case QueryRangeType.NearWeek:
                    weekRadBtn.IsChecked = true;
                    startDate = endDate.AddDays(-7);
                    break;
                case QueryRangeType.NearMonth:
                    monthRadBtn.IsChecked = true;
                    startDate = endDate.AddMonths(-1);
                    break;
            }
            accountSummaryChkBox.IsChecked = tradeChkBox.IsChecked
                =positionChkBox.IsChecked=closeRecordChkBox.IsChecked= true;

            host.Child = reportBrowser;

            //add the browser to the grid.
            host.SetValue(Grid.RowProperty, 2);
            host.SetValue(Grid.ColumnProperty, 1);
          //  this.winGrid.Children.Clear();
            this.winGrid.Children.Add(host);

            string holidayFile = "include/Holidays.txt";
            if (File.Exists(holidayFile))
            {
                Utility.Holidays = File.ReadAllText(holidayFile, Encoding.Default);
            }
        }
            
        private void printBtn_Click(object sender, RoutedEventArgs e)
        {
            reportBrowser.Print();
            return;
            /*System.Windows.Controls.PrintDialog pDialog = new System.Windows.Controls.PrintDialog();
            pDialog.PageRangeSelection = PageRangeSelection.AllPages;
            pDialog.UserPageRangeEnabled = true;
            pDialog.UserPageRangeEnabled = true;
            Nullable<bool> result = pDialog.ShowDialog();

            if (result == true)
            {
                //pDialog.PrintVisual(reportRichTxt, "Print Report");
            }*/
        }

        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            reportBrowser.ShowSaveAsDialog();
            return;
            /*Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = string.Format("交易及账户报告单");
            dlg.DefaultExt = ".txt";
            dlg.Filter = "Text documents (.txt)|*.txt|XPS Documents|*.xps";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                string filename = dlg.FileName;
                //todo:huangrongyu how to save the reportRichTxt's content.
                //File.WriteAllText(filename, reportRichTxt.Name, Encoding.UTF8);
            }*/
        }

        private void closeBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            reportBrowser.Dispose();
            host.Dispose();
            if(File.Exists(htmlFile))
            {
                File.Delete(htmlFile);
            }
        }
        #region 交易账户报告窗体私有方法

        /// <summary>
        /// 验证输入
        /// </summary>
        /// <returns>验证成功返回true，失败返回FALSE</returns>
        private bool ValidateInput()
        {
            bool bResult = true;
            if (todayRadBtn.IsChecked.Value)
            {
                if (!Utility.IsWorkDay(DateTime.Now))
                {
                    System.Windows.MessageBox.Show("今天为非交易日，无账户交易报告", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    bResult = false;
                }
                if (DateTime.Now.Hour < 17)
                {
                    System.Windows.MessageBox.Show("当天的报告单还未生成", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    bResult = false;
                }
                startDate = endDate = DateTime.Today;
                TradingDayNum = 1;
                bResult = true;
            }
            else if (weekRadBtn.IsChecked.Value)
            {
                endDate = DateTime.Now;
                startDate = endDate.AddDays(-7);

            }
            else if (monthRadBtn.IsChecked.Value)
            {
                endDate = DateTime.Now;
                startDate = endDate.AddMonths(-1);
            }
            else if (userdefinedRadBtn.IsChecked.Value)
            {
                if (startDatePicker.SelectedDate != null && endDatePicker.SelectedDate != null)
                {
                    startDate = startDatePicker.SelectedDate.Value.Date;
                    endDate = endDatePicker.SelectedDate.Value.Date;
                }
                else
                {
                    startDate = DateTime.Today;
                    endDate = DateTime.Today;
                }
                if (startDate > endDate)
                {
                    System.Windows.MessageBox.Show("开始时间不能大于结束时间");
                    bResult = false;
                }
            }
            if (bResult)
            {
                for (DateTime dt = startDate; dt <= endDate; )
                {
                    while (!Utility.IsWorkDay(dt))
                    {
                        dt = dt.AddDays(1);
                        Thread.Sleep(500);
                    }
                    TradingDayNum++;
                    dt = dt.AddDays(1);
                }
            }
            else
            {
                TradingDayNum = 0;
            }
            return bResult;
        }

        /// <summary>
        /// 格式化报告的html标题
        /// </summary>
        /// <param name="templateHtml">模板HTML文件内容</param>
        /// <param name="settlementInfo">结算单信息</param>
        /// <param name="startDate">开始日期</param>
        /// <param name="endDate">结束日期</param>
        /// <param name="custName">客户名称</param>
        /// <returns>标题的html字符串</returns>
        private string FormatTitle(string templateHtml,string settlementInfo, DateTime startDate,DateTime endDate,out string custName)
        {
            string titleHtml = string.Empty;
            custName = string.Empty;
            string startMark ,endMark;

            //获取客户名称
            startMark = "Client Name";
            endMark = "日期";
            int nameStartIndex = settlementInfo.IndexOf(startMark);
            string nameInfo = settlementInfo.Substring(nameStartIndex + startMark.Length+1);
            int nameEndIndex = nameInfo.IndexOf(endMark);
            nameInfo = nameInfo.Substring(0, nameEndIndex);
            custName = nameInfo.Trim();
            //获取报告标题的模板
            startMark = "<!BEGIN头部!>";
            endMark = "<!END头部!>";
            int titleStartIndex = templateHtml.IndexOf(startMark);
            int titleEndIndex = templateHtml.IndexOf(endMark);
            titleHtml = templateHtml.Substring(titleStartIndex + startMark.Length + 2, titleEndIndex - titleStartIndex - endMark.Length - 4);
            string title = string.Format("{0}交易统计报表_{1}-{2}",custName, startDate.ToString("yyyyMMdd"), endDate.ToString("yyyyMMdd"));
            titleHtml=titleHtml.Replace("$Title$", title);

            return titleHtml;
        }
        /// <summary>
        /// 格式化报告客户信息
        /// </summary>
        /// <param name="templateHtml">模板HTML文件内容</param>
        /// <param name="accountID">交易账号</param>
        /// <param name="custName">用户名</param>
        /// <param name="startDate">报告开始日期</param>
        /// <param name="endDate">报告结束日期</param>
        /// <returns>客户信息的html字符串</returns>
        private string FormatCustomerInfo(string templateHtml, string accountID, string custName, DateTime startDate, DateTime endDate)
        {
            string custInfoHtml = string.Empty;
            //获取报告标题的模板
            string startMark = "<!BEGIN报告标题!>";
            string endMark = "<!END报告标题!>";
            int titleStartIndex = templateHtml.IndexOf(startMark);
            int titleEndIndex = templateHtml.IndexOf(endMark);
            custInfoHtml = templateHtml.Substring(titleStartIndex + startMark.Length + 2, titleEndIndex - titleStartIndex - endMark.Length - 4);

            custInfoHtml = custInfoHtml.Replace("$AccountID$", accountID);
            custInfoHtml = custInfoHtml.Replace("$CustName$", custName);
            custInfoHtml = custInfoHtml.Replace("$StartDate$", startDate.ToString("yyyyMMdd"));
            custInfoHtml = custInfoHtml.Replace("$EndDate$", endDate.ToString("yyyyMMdd"));
            return custInfoHtml;
        }
        /// <summary>
        /// 根据结算单获取资金状况信息
        /// </summary>
        /// <param name="settlementInfo">结算单</param>
        /// <returns>资金状况信息</returns>
        private List<TradingAccountData> GetTradingAccount(List<string> settlementInfos)
        {
            List<TradingAccountData> tradingAccountList = new List<TradingAccountData>();
            if (settlementInfos == null || settlementInfos.Count == 0)
            {
                return tradingAccountList;
            }
            foreach (var item in dicSettlementContentData.Keys)
            {
                string settlementInfo = string.Empty;
                dicSettlementContentData.TryGetValue(item,out settlementInfo);
                string tradingDay = item.Split('_')[1];
                TradingAccountData tradingAccount = new TradingAccountData();
                //获取结算单中的资金状况文本
                int tradingAccountStartIndex = settlementInfo.IndexOf("资金状况");
                if (tradingAccountStartIndex < 0)
                {
                    continue;
                }
                string tradingAccountContent = settlementInfo.Substring(tradingAccountStartIndex);
                int tradingAccountEndIndex = tradingAccountContent.IndexOf("应追加资金");
                string temp = tradingAccountContent.Substring(tradingAccountEndIndex, tradingAccountContent.Length - tradingAccountEndIndex);
                tradingAccountEndIndex += temp.IndexOf("\r\n");
                tradingAccountContent = tradingAccountContent.Substring(0, tradingAccountEndIndex - 1);

                //找出资金状况对应的数值，并以此对TradingAccountData赋值（是否使用TradingAccountData还需进一步确认）
                //MatchCollection matches = Regex.Matches(tradingAccountContent, @"\d+", RegexOptions.ECMAScript);
                MatchCollection matches = Regex.Matches(tradingAccountContent, @" (-?\d+)(\.\d+)?", RegexOptions.ECMAScript);

                if (matches == null)
                { continue; }
                tradingAccount.InvestorID = accountID;
                //tradingAccount.TradingDay = tradingDay;
                //tradingAccount.PreBalance = double.Parse(matches[0].Value); //期初结存 Balance b/f
                //tradingAccount.Reserve = double.Parse(matches[1].Value); // 基础保证金 Initial Margin
                //tradingAccount.CashIn = double.Parse(matches[2].Value); //出 入 金 Deposit/Withdrawal
                tradingAccount.Balance = double.Parse(matches[3].Value); //期末结存 Balance c/f
                tradingAccount.CloseProfit = double.Parse(matches[4].Value); //平仓盈亏 Realized P/L
                //tradingAccount.Mortgage = double.Parse(matches[5].Value); //质 押 金 Pledge Amount  ???
                tradingAccount.PositionProfit = double.Parse(matches[6].Value); //持仓盯市盈亏 MTM P/L
                tradingAccount.Balance = double.Parse(matches[7].Value); //客户权益 Client Equity  ???
                //tradingAccount.OptionCloseProfit = double.Parse(matches[8].Value); //期权执行盈亏 Exercise P/L
                //tradingAccount.PreFundMortgageOut = double.Parse(matches[9].Value); //货币质押保证金占用 FX Pledge Occ. ???
                tradingAccount.Commission = double.Parse(matches[10].Value); //手 续 费 Commission
                tradingAccount.CurrMargin = double.Parse(matches[11].Value); //保证金占用 Margin Occupied
                //tradingAccount. = double.Parse(matches[12].Value); //行权手续费 Exercise Fee
                //tradingAccount.DeliveryMargin = double.Parse(matches[13].Value); //交割保证金 Delivery Margin

                //tradingAccount. = double.Parse(matches[14].Value); //交割手续费 Delivery Fee
                //tradingAccount. = double.Parse(matches[15].Value); //多头期权市值 Market value(long)
                tradingAccount.FundMortgageIn = double.Parse(matches[16].Value); //货币质入 New FX Pledge
                //tradingAccount. = double.Parse(matches[17].Value); //空头期权市值 Market value(short)
                tradingAccount.FundMortgageOut = double.Parse(matches[18].Value); //货币质出 FX Redemption
                //tradingAccount. = double.Parse(matches[19].Value); //市值权益 Market value(equity)
                //tradingAccount. = double.Parse(matches[20].Value); //质押变化金额 Chg in Pledge Amt
                tradingAccount.Available = double.Parse(matches[21].Value); //可用资金 Fund Avail.
                //tradingAccount. = double.Parse(matches[22].Value); //权利金收入 Premium received
                string str = matches[23].Value;
                //  tradingAccount.Risk =double.Parse(matches[23].Value.Substring(0, matches[23].Value.Length - 1)).ToString()) ; //风 险 度 Risk Degree
                //tradingAccount. = double.Parse(matches[24].Value); //权利金支出 Premium paid
                //tradingAccount. = double.Parse(matches[25].Value); //应追加资金 Margin Call
                tradingAccountList.Add(tradingAccount);
            }
            return tradingAccountList;
        }
      
        /// <summary>
        /// 根据结算单信息获取期末持仓列表
        /// </summary>
        /// <param name="settlementInfo">结算单信息</param>
        /// <returns>期末持仓列表</returns>
     

        private string FormatEndPosition(HtmlDocument htmlDoc,string templateHtml, List<PositionField> positionList)
        {
            string positionHtml = string.Empty;
            string startMark = "<!BEGIN期末持仓!>";
            string endMark = "<!END期末持仓!>";
            int titleStartIndex = templateHtml.IndexOf(startMark);
            int titleEndIndex = templateHtml.IndexOf(endMark);
            if (titleStartIndex < 0 || titleEndIndex < 0)
            {
                return positionHtml;
            }
            string positionTemplate = templateHtml.Substring(titleStartIndex + startMark.Length + 2, titleEndIndex - titleStartIndex - endMark.Length - 4);

            if (positionList == null || positionList.Count == 0)
            {
                positionHtml = positionTemplate;// positionHeaderHtml + @"</table></div>";
                return positionHtml;
            }
            HtmlElement tableElem = htmlDoc.CreateElement("TABLE");
            HtmlElement tableRow, tableCell;
            foreach (var item in positionList)
            {
                tableRow = htmlDoc.CreateElement("TR");
                tableElem.AppendChild(tableRow);
                //交易所
                tableCell = htmlDoc.CreateElement("TD");
                //tableCell.InnerText = item.ExchangeID;
                tableCell.SetAttribute("bgcolor", "#f5f5f5");
                tableRow.AppendChild(tableCell);
                //合约
                tableCell = htmlDoc.CreateElement("TD");
                tableCell.InnerText = item.InstrumentID;
                tableCell.SetAttribute("bgcolor", "#f5f5f5");
                tableRow.AppendChild(tableCell);
                //开仓日期
                tableCell = htmlDoc.CreateElement("TD");
                tableCell.InnerText = item.TradingDay;
                tableCell.SetAttribute("bgcolor", "#f5f5f5");
                tableRow.AppendChild(tableCell);
                //投保标志
                tableCell = htmlDoc.CreateElement("TD");
                tableCell.InnerText = item.Hedge.ToString();
                tableCell.SetAttribute("bgcolor", "#f5f5f5");
                tableRow.AppendChild(tableCell);
                //多空方向
                tableCell = htmlDoc.CreateElement("TD");
                tableCell.InnerText =item.Direction.ToString();
                tableCell.SetAttribute("bgcolor", "#f5f5f5");
                tableRow.AppendChild(tableCell);
                //持仓量
                tableCell = htmlDoc.CreateElement("TD");
                tableCell.InnerText = item.Position.ToString();
                tableCell.SetAttribute("bgcolor", "#f5f5f5");
                tableRow.AppendChild(tableCell);
                //开仓价
                tableCell = htmlDoc.CreateElement("TD");
                tableCell.InnerText =Math.Round(item.AvgPrice,2).ToString();
                tableCell.SetAttribute("bgcolor", "#f5f5f5");
                tableRow.AppendChild(tableCell);
                //昨结价
                tableCell = htmlDoc.CreateElement("TD");
                tableCell.InnerText =Math.Round(item.PreSettlementPrice,2).ToString();
                tableCell.SetAttribute("bgcolor", "#f5f5f5");
                tableRow.AppendChild(tableCell);
                //结算价
                tableCell = htmlDoc.CreateElement("TD");
                tableCell.InnerText =Math.Round(item.SettlementPrice,2).ToString();
                tableCell.SetAttribute("bgcolor", "#f5f5f5");
                tableRow.AppendChild(tableCell);
                //浮动盈亏
                tableCell = htmlDoc.CreateElement("TD");
                //tableCell.InnerText = item.ExchangeID;
                if (item.PositionProfit > 0)
                {
                    tableCell.InnerHtml = string.Format("<font color='#FF0000'>{0}</font>", Math.Round(item.PositionProfit, 2).ToString());
                }
                else if (item.PositionProfit < 0)
                {
                    tableCell.InnerHtml = string.Format("<font color='#00FF00'>{0}</font>", Math.Round(item.PositionProfit, 2).ToString());
                }
                tableCell.SetAttribute("bgcolor", "#f5f5f5");
                tableRow.AppendChild(tableCell);
            }
           positionHtml= positionTemplate.Replace("<!期末持仓!>", tableElem.InnerHtml);
            return positionHtml;
        }
        /// <summary>
        /// 根据结算单信息获取平仓明细列表
        /// </summary>
        /// <param name="settlementInfo">结算单信息</param>
        /// <returns>平仓明细列表</returns>
        private List<ClosedPosition> GetClosedPosition(out int lots,out double totalClosedPforit,out string msg)
        {
            List<ClosedPosition> closedPositionList = new List<ClosedPosition>();
            lots = 0;
            totalClosedPforit = 0.0;
            msg = string.Empty;
            int nLots = 0;
            double dTotalClosedProfit = 0.0;
            string settlementInfo;
            foreach (var key in dicSettlementContentData.Keys)
            {
                settlementInfo = string.Empty;
                dicSettlementContentData.TryGetValue(key, out settlementInfo);
                //截取平仓明细部分
                int closedPosIndex = settlementInfo.IndexOf("平仓明细");
                if (closedPosIndex < 0)
                {
                    continue;
                }
                string closedPosContent = settlementInfo.Substring(closedPosIndex);
                int closedPosEndIndex = closedPosContent.IndexOf("共");

                //获取汇总数据
                string statContent = closedPosContent.Substring(closedPosEndIndex);
                int statEndIndex = statContent.IndexOf("--");
                statContent = statContent.Substring(0, statEndIndex);
                string[] stats = statContent.Trim('|').Split('|');
                int.TryParse(stats[6], out nLots);
                double.TryParse(stats[10], out dTotalClosedProfit);
                lots += nLots;
                totalClosedPforit += dTotalClosedProfit;

                closedPosContent = closedPosContent.Substring(0, closedPosEndIndex - 1);
                closedPosContent = closedPosContent.Replace("--", string.Empty);

                string[] closedPosDetails = closedPosContent.Split("\r\n".ToCharArray());
                List<string> closedPosList = new List<string>();
                for (int i = 10; i < closedPosDetails.Count(); i++)
                {
                    if (!(String.IsNullOrEmpty(closedPosDetails[i].Trim()) || closedPosDetails[i] == "-"))
                    {
                        closedPosList.Add(closedPosDetails[i]);
                    }
                }
                //将平仓记录转化为成交列表
                ClosedPosition closedPosition;
                foreach (var item in closedPosList)
                {
                    string[] closeData = item.Trim('|').Split('|');
                    if (closeData.Count() < 12)
                    {
                        string[] keys = key.Split('_');
                        msg = string.Format("资金账号的结算单的平仓明细部分出现乱码，请检查该结算单信息");
                        return closedPositionList;
                    }
                    closedPosition = new ClosedPosition()
                    {
                        CloseDate = closeData[0],
                        Exchange = closeData[1].Trim(),//交易所名称
                        //InstrumentName = closeData[2],//品种
                        InstrumentID = closeData[3].Trim(),
                        OpenDate = closeData[4].Trim(),//开仓日期
                        Direction = (closeData[5].Trim() == "买") ? PosiDirection.多 : PosiDirection.空,//买、卖
                        Volume = int.Parse(closeData[6]),//手数
                        OpenPrice = double.Parse(closeData[7]),//开仓价
                        //closeData[8] //昨结价
                        ClosePrice = double.Parse(closeData[9]), //成交价
                        CloseProfit = double.Parse(closeData[10])//平仓盈亏
                        //closeData[11] //权利金收支
                    };
                    closedPositionList.Add(closedPosition);
                }
            }
            return closedPositionList;
        }

        private string FormatClosedPosition(System.Windows.Forms.HtmlDocument doc,string templateHtml,  List<ClosedPosition> closedPositionList,int lots,double totalClosedProfit)
        {
            string closedPositionHtmlStr = string.Empty;
            string startMark = "<!BEGIN平仓记录!>";
            string endMark = "<!END平仓记录!>";
            int titleStartIndex = templateHtml.IndexOf(startMark);
            int titleEndIndex = templateHtml.IndexOf(endMark);
            if(titleStartIndex <0 || titleStartIndex<0)
            {
                return closedPositionHtmlStr;
            }
            string closedPositionTemplate = templateHtml.Substring(titleStartIndex + startMark.Length + 2, titleEndIndex - titleStartIndex - endMark.Length - 4);

            if(closedPositionList==null || closedPositionList.Count==0)
            {
                closedPositionHtmlStr = closedPositionTemplate;
                return closedPositionHtmlStr;
            }
            string closedPositions = string.Empty;
            HtmlElement tableElem = doc.CreateElement("TABLE");
          //  doc.Body.AppendChild(tableElem);
            HtmlElement tableRow1,tableRow2;
            HtmlElement tableCell;
            if (closedPositionList != null && closedPositionList.Count > 0)
            {
                foreach (var item in closedPositionList)
                {
                    //平仓信息
                    tableRow1 = doc.CreateElement("TR");
                    tableElem.AppendChild(tableRow1);

                    tableCell = doc.CreateElement("TD");
                    tableCell.InnerText = item.Exchange;
                    tableCell.SetAttribute("bgcolor", "#f5f5f5");
                    tableRow1.AppendChild(tableCell);
                    tableCell = doc.CreateElement("TD");
                    tableCell.InnerText = item.InstrumentID;
                    tableCell.SetAttribute("bgcolor", "#f5f5f5");
                    tableRow1.AppendChild(tableCell);
                    tableCell = doc.CreateElement("TD");
                    tableCell.InnerText = item.Volume.ToString();
                    tableCell.SetAttribute("bgcolor", "#f5f5f5");
                    tableRow1.AppendChild(tableCell);
                    tableCell = doc.CreateElement("TD");
                    tableCell.InnerText = item.CloseDate;   //平仓日期
                    tableCell.SetAttribute("bgcolor", "#f5f5f5");
                    tableRow1.AppendChild(tableCell);
                    tableCell = doc.CreateElement("TD");
                    tableCell.InnerText = "";
                    tableCell.SetAttribute("bgcolor", "#f5f5f5");
                    tableRow1.AppendChild(tableCell);
                    tableCell = doc.CreateElement("TD");
                    tableCell.InnerText = "";
                    tableCell.SetAttribute("bgcolor", "#f5f5f5");
                    tableRow1.AppendChild(tableCell);
                    tableCell = doc.CreateElement("TD");
                    tableCell.InnerText = Math.Round( item.ClosePrice,2).ToString();
                    tableCell.SetAttribute("bgcolor", "#f5f5f5");
                    tableRow1.AppendChild(tableCell);
                    tableCell = doc.CreateElement("TD");
                    tableCell.InnerText = "";
                    tableCell.SetAttribute("bgcolor", "#f5f5f5");
                    tableRow1.AppendChild(tableCell);
                    //开仓信息
                    tableRow2 = doc.CreateElement("TR");
                    tableElem.AppendChild(tableRow2);

                    tableCell = doc.CreateElement("TD");
                    tableCell.InnerText = "";
                    tableCell.SetAttribute("bgcolor", "#f5f5f5");
                    tableRow2.AppendChild(tableCell);
                    tableCell = doc.CreateElement("TD");
                    tableCell.InnerText = "";
                    tableCell.SetAttribute("bgcolor", "#f5f5f5");
                    tableRow2.AppendChild(tableCell);
                    tableCell = doc.CreateElement("TD");
                    tableCell.InnerText = "";
                    tableCell.SetAttribute("bgcolor", "#f5f5f5");
                    tableRow2.AppendChild(tableCell);
                    tableCell = doc.CreateElement("TD");
                    tableCell.InnerText = item.OpenDate;   //开仓日期
                    tableCell.SetAttribute("bgcolor", "#f5f5f5");
                    tableRow2.AppendChild(tableCell);
                    tableCell = doc.CreateElement("TD");
                    tableCell.InnerText = Math.Round(item.OpenPrice,2).ToString();
                    tableCell.SetAttribute("bgcolor", "#f5f5f5");
                    tableRow2.AppendChild(tableCell);
                    tableCell = doc.CreateElement("TD");
                    tableCell.InnerText = "";
                    tableCell.SetAttribute("bgcolor", "#f5f5f5");
                    tableRow2.AppendChild(tableCell);
                    tableCell = doc.CreateElement("TD");
                    tableCell.InnerText = "";
                    tableCell.SetAttribute("bgcolor", "#f5f5f5");
                    tableRow2.AppendChild(tableCell);
                    tableCell = doc.CreateElement("TD");
                    tableCell.InnerHtml = Math.Round(item.CloseProfit, 2).ToString();
                    string closeProfitHtml = string.Empty; ;
                    if (item.CloseProfit > 0)
                    {
                        closeProfitHtml = string.Format("<font color='#FF0000'>{0}</font>", Math.Round(item.CloseProfit, 2).ToString());
                    }
                    else
                    {
                        closeProfitHtml = string.Format("<font color='#00FF00'>{0}</font>", Math.Round(item.CloseProfit, 2).ToString());
                    }
                    tableCell.InnerHtml = closeProfitHtml;
                    tableCell.SetAttribute("bgcolor", "#f5f5f5");
                    tableRow2.AppendChild(tableCell);
                }
            }
            //汇总数据
            tableRow1 = doc.CreateElement("TR");
            tableElem.AppendChild(tableRow1);
            for (int i = 0; i < 8; i++)
            {
                tableCell = doc.CreateElement("TD");
                tableCell.InnerText = string.Empty;
                if (i == 0)
                {
                    tableCell.InnerText = "合计";
                }
                else if (i == 2)
                {
                    tableCell.InnerText = lots.ToString();
                }
                else if (i == 7)
                {
                    tableCell.InnerText = Math.Round(totalClosedProfit, 2).ToString();
                }
                tableCell.SetAttribute("bgcolor", "#f5f5f5");
                tableRow1.AppendChild(tableCell);
            }

            closedPositionHtmlStr =closedPositionTemplate = closedPositionTemplate.Replace("<!平仓记录!>", tableElem.InnerHtml);
            return closedPositionHtmlStr;
        }

        private string FormatTradeStat(HtmlDocument doc,string templateHtml, List<ClosedPosition> closedPositionList)
        {
            string tradeStatHtml = string.Empty;
            string startMark = "<!BEGIN交易统计!>";
            string endMark = "<!END交易统计!>";
            int titleStartIndex = templateHtml.IndexOf(startMark);
            int titleEndIndex = templateHtml.IndexOf(endMark);
            string tradeStatTemplate = templateHtml.Substring(titleStartIndex + startMark.Length + 2, titleEndIndex - titleStartIndex - endMark.Length - 4);

            if (closedPositionList == null || closedPositionList.Count == 0)
            {
                tradeStatHtml = tradeStatTemplate;// tradeStatHeader + "</table></div>";
                return tradeStatHtml;
            }
            HtmlElement tableElem = doc.CreateElement("TABLE");
            HtmlElement tableRow, tableCell;
            //合约ID为Key,建立字典，在进行html生成
            Dictionary<string, List<TradeStat>> dicTradeStat = new Dictionary<string, List<TradeStat>>();
            string key = string.Empty;
            List<TradeStat> tsList;
            TradeStat ts;
            foreach (var item in closedPositionList)
            {
                key = item.InstrumentID;
                string profitloss=(item.CloseProfit>=0)?"盈利":"亏损";
                tsList = new List<TradeStat>();
                if (dicTradeStat.TryGetValue(key, out tsList))
                {
                    var tmp = tsList.Where(t => t.InstrumentID == item.InstrumentID && t.Direction == item.Direction && t.ProfitLoss == profitloss);
                    if (tmp == null || tmp.Count() == 0)
                    {
                        ts = new TradeStat()
                        {
                            AvgProfitByHand =(item.Volume==0)?0.0: Math.Round(item.CloseProfit / item.Volume, 2),
                            AvgProfitByTime = Math.Round(item.CloseProfit,2),
                            Direction = item.Direction,
                            InstrumentID = item.InstrumentID,
                            NetProfit = item.CloseProfit,
                            ProfitLoss = (item.CloseProfit >= 0) ? "盈利" : "亏损",
                            Volume = item.Volume,
                            TradeCount = 1
                        };
                        tsList.Add(ts);
                    }
                    else
                    {
                        ts = TradeStat.DeepCopy(tmp.ToList()[0]);
                        ts.AvgProfitByTime = (ts.AvgProfitByTime * ts.TradeCount + item.CloseProfit) / (ts.TradeCount + 1);
                        ts.AvgProfitByHand = (ts.AvgProfitByHand * ts.Volume + item.CloseProfit) / (ts.Volume + 1);
                        ts.TradeCount += 1;
                        ts.Volume += item.Volume;
                        ts.NetProfit += item.CloseProfit;
                        int tsIndex = -1;
                        tsIndex = tsList.FindIndex(t => t.InstrumentID == item.InstrumentID
                            && t.Direction == item.Direction && t.ProfitLoss == profitloss);
                        tsList[tsIndex] = ts;
                    }
                    dicTradeStat.Remove(key);
                    dicTradeStat.Add(key, tsList);
                }
                else
                {
                    ts = new TradeStat()
                    {
                        AvgProfitByHand =(item.Volume==0)?0.0: Math.Round(item.CloseProfit / item.Volume, 2),
                        AvgProfitByTime = Math.Round(item.CloseProfit,2),
                        Direction = item.Direction,
                        InstrumentID = item.InstrumentID,
                        NetProfit = item.CloseProfit,
                        ProfitLoss = (item.CloseProfit >= 0) ? "盈利" : "亏损",
                        Volume = item.Volume,
                        TradeCount = 1
                    };
                    tsList = new List<TradeStat>() { ts };
                    dicTradeStat.Add(key, tsList);
                }
            }
            foreach (var item in dicTradeStat.Keys)
            {
                tsList = dicTradeStat[item];
                int longTradeCount, longVolume, shortTradeCount, shortVolume ;
                double shortNetProfit, longNetProfit;
                longTradeCount= longVolume= shortTradeCount= shortVolume= 0;
                shortNetProfit= longNetProfit=0.0;
                foreach(var i in tsList)
                {
                    if(i.Direction==PosiDirection.多)
                    {
                        longTradeCount += i.TradeCount;
                        longVolume += i.Volume;
                        longNetProfit += i.NetProfit;
                    }
                    else
                    {
                        shortTradeCount += i.TradeCount;
                        shortVolume += i.Volume;
                        shortNetProfit += i.NetProfit;
                    }
                }
                #region 汇总行
                tableRow = doc.CreateElement("TR");
                tableRow.SetAttribute("bgcolor", "#FFFF00");//黄色背景色
                tableElem.AppendChild(tableRow);

                tableCell = doc.CreateElement("TD");//品种
                tableCell.InnerHtml =string.Format("<b>{0}</b>", tsList[0].InstrumentID);
                tableCell.SetAttribute("bgcolor", "#f5f5f5");
                tableElem.AppendChild(tableCell);
                tableCell = doc.CreateElement("TD");//多空方向
                tableCell.InnerText = string.Empty;
                tableCell.SetAttribute("bgcolor", "#f5f5f5");
                tableElem.AppendChild(tableCell);
                tableCell = doc.CreateElement("TD");//盈亏标志
                tableCell.InnerText = string.Empty;
                tableCell.SetAttribute("bgcolor", "#f5f5f5");
                tableElem.AppendChild(tableCell);
                tableCell = doc.CreateElement("TD");//交易次数
                tableCell.InnerHtml =string.Format("<b>{0}</b>", (longTradeCount+shortTradeCount).ToString());
                tableCell.SetAttribute("bgcolor", "#f5f5f5");
                tableElem.AppendChild(tableCell);
                tableCell = doc.CreateElement("TD");//手数
                tableCell.InnerHtml =string.Format("<b>{0}</b>", (longVolume+shortVolume).ToString());
                tableCell.SetAttribute("bgcolor", "#f5f5f5");
                tableElem.AppendChild(tableCell);
                tableCell = doc.CreateElement("TD");//净利润
                tableCell.InnerHtml =string.Format("<b>{0}</b>", Math.Round (longNetProfit+shortNetProfit,2).ToString());
                tableCell.SetAttribute("bgcolor", "#f5f5f5");
                tableElem.AppendChild(tableCell);
                tableCell = doc.CreateElement("TD");//平均每次利润
                tableCell.InnerHtml =string.Format("<b>{0}</b>",(longNetProfit+shortNetProfit==0.0 || longTradeCount+shortTradeCount==0)?"0.0":
                    Math.Round((longNetProfit+shortNetProfit)/(double)(longTradeCount+ shortTradeCount),2).ToString());
                tableCell.SetAttribute("bgcolor", "#f5f5f5");
                tableElem.AppendChild(tableCell);
                tableCell = doc.CreateElement("TD");//平均每手利润
                tableCell.InnerHtml = string.Format("<b>{0}</b>", (longNetProfit + shortNetProfit == 0.0 || longVolume + shortVolume == 0) ? "0.0" :
                    Math.Round((longNetProfit + shortNetProfit) / (double)(longVolume + shortVolume), 2).ToString());
                tableCell.SetAttribute("bgcolor", "#f5f5f5");
                tableElem.AppendChild(tableCell);
                #endregion

                #region 多头汇总行

                tableRow = doc.CreateElement("TR");
                tableRow.SetAttribute("bgcolor", "#C0C0C0");//银色背景色
                tableElem.AppendChild(tableRow);

                tableCell = doc.CreateElement("TD");//品种
                tableCell.InnerText=string.Empty;
                tableCell.SetAttribute("bgcolor", "#f5f5f5");
                tableElem.AppendChild(tableCell);
                tableCell = doc.CreateElement("TD");//多空方向
                tableCell.InnerHtml =string.Format("<font color='#FF0000'><b>{0}</b></font>",PosiDirection.多.ToString());
                tableCell.SetAttribute("bgcolor", "#f5f5f5");
                tableElem.AppendChild(tableCell);
                tableCell = doc.CreateElement("TD");//盈亏标志
                tableCell.InnerText = string.Empty;
                tableCell.SetAttribute("bgcolor", "#f5f5f5");
                tableElem.AppendChild(tableCell);
                tableCell = doc.CreateElement("TD");//交易次数
                tableCell.InnerText = longTradeCount.ToString();
                tableCell.SetAttribute("bgcolor", "#f5f5f5");
                tableElem.AppendChild(tableCell);
                tableCell = doc.CreateElement("TD");//手数
                tableCell.InnerText = longVolume.ToString();
                tableCell.SetAttribute("bgcolor", "#f5f5f5");
                tableElem.AppendChild(tableCell);
                tableCell = doc.CreateElement("TD");//净利润
                tableCell.InnerText = Math.Round(longNetProfit , 2).ToString();
                tableCell.SetAttribute("bgcolor", "#f5f5f5");
                tableElem.AppendChild(tableCell);
                tableCell = doc.CreateElement("TD");//平均每次利润
                tableCell.InnerText = (longNetProfit == 0.0 || longTradeCount == 0) ? "0.0" : Math.Round(longNetProfit / (double)longTradeCount, 2).ToString();
                tableCell.SetAttribute("bgcolor", "#f5f5f5");
                tableElem.AppendChild(tableCell);
                tableCell = doc.CreateElement("TD");//平均每手利润
                tableCell.InnerText = (longNetProfit == 0.0 || longVolume == 0) ? "0.0" : Math.Round(longNetProfit / (double)longVolume, 2).ToString();
                tableCell.SetAttribute("bgcolor", "#f5f5f5");
                tableElem.AppendChild(tableCell);
                #endregion

                #region 多头盈利行
                var tmp = tsList.Where(t => t.InstrumentID == tsList[0].InstrumentID && t.Direction == PosiDirection.多 && t.ProfitLoss == "盈利");
                if (tmp == null || tmp.Count() == 0)
                {
                    ts = new TradeStat()
                    {
                        AvgProfitByHand = 0.0,
                        AvgProfitByTime = 0.0,
                        Direction = PosiDirection.多,
                        InstrumentID = tsList[0].InstrumentID,
                         ProfitLoss="盈利",
                        NetProfit = 0.0,
                        TradeCount = 0,
                        Volume = 0
                    };
                }
                else
                { ts = tmp.ToList()[0]; }
                tableRow = doc.CreateElement("TR");
                tableRow.SetAttribute("bgcolor", "#FFFFFF");//白色背景色
                tableElem.AppendChild(tableRow);

                tableCell = doc.CreateElement("TD");//品种
                tableCell.InnerText = string.Empty;
                tableCell.SetAttribute("bgcolor", "#f5f5f5");
                tableElem.AppendChild(tableCell);
                tableCell = doc.CreateElement("TD");//多空方向
                tableCell.InnerText = string.Empty;
                tableCell.SetAttribute("bgcolor", "#f5f5f5");
                tableElem.AppendChild(tableCell);
                tableCell = doc.CreateElement("TD");//盈亏标志
                tableCell.InnerHtml = string.Format("<font color='#FF0000'>{0}</font>",ts.ProfitLoss);
                tableCell.SetAttribute("bgcolor", "#f5f5f5");
                tableElem.AppendChild(tableCell);
                tableCell = doc.CreateElement("TD");//交易次数
                tableCell.InnerText = ts.TradeCount.ToString();
                tableCell.SetAttribute("bgcolor", "#f5f5f5");
                tableElem.AppendChild(tableCell);
                tableCell = doc.CreateElement("TD");//手数
                tableCell.InnerText = ts.Volume.ToString();
                tableCell.SetAttribute("bgcolor", "#f5f5f5");
                tableElem.AppendChild(tableCell);
                tableCell = doc.CreateElement("TD");//净利润
                tableCell.InnerText = Math.Round(ts.NetProfit, 2).ToString();
                tableCell.SetAttribute("bgcolor", "#f5f5f5");
                tableElem.AppendChild(tableCell);
                tableCell = doc.CreateElement("TD");//平均每次利润
                tableCell.InnerText =(ts.NetProfit==0.0 || ts.TradeCount==0)?"0.0": Math.Round(ts.NetProfit / (double)ts.TradeCount, 2).ToString();
                tableCell.SetAttribute("bgcolor", "#f5f5f5");
                tableElem.AppendChild(tableCell);
                tableCell = doc.CreateElement("TD");//平均每手利润
                tableCell.InnerText = (ts.NetProfit == 0.0 || ts.Volume == 0) ? "0.0" : Math.Round(ts.NetProfit / (double)ts.Volume, 2).ToString();
                tableCell.SetAttribute("bgcolor", "#f5f5f5");
                tableElem.AppendChild(tableCell);
                #endregion

                #region 多头亏损行
                tmp = tsList.Where(t => t.InstrumentID == tsList[0].InstrumentID && t.Direction == PosiDirection.多 && t.ProfitLoss == "亏损");
                if (tmp == null || tmp.Count() == 0)
                {
                    ts = new TradeStat()
                    {
                        AvgProfitByHand = 0.0,
                        AvgProfitByTime = 0.0,
                        Direction = PosiDirection.多,
                        InstrumentID = tsList[0].InstrumentID,
                        NetProfit = 0.0,
                        ProfitLoss="亏损",
                        TradeCount = 0,
                        Volume = 0
                    };
                }
                else
                { ts = tmp.ToList()[0]; }
                tableRow = doc.CreateElement("TR");
                tableRow.SetAttribute("bgcolor", "#FFFFFF");//白色背景色
                tableElem.AppendChild(tableRow);

                tableCell = doc.CreateElement("TD");//品种
                tableCell.InnerText = string.Empty;
                tableCell.SetAttribute("bgcolor", "#f5f5f5");
                tableElem.AppendChild(tableCell);
                tableCell = doc.CreateElement("TD");//多空方向
                tableCell.InnerText = string.Empty;
                tableCell.SetAttribute("bgcolor", "#f5f5f5");
                tableElem.AppendChild(tableCell);
                tableCell = doc.CreateElement("TD");//盈亏标志
                tableCell.InnerHtml = string.Format("<font color='#00FF00'>{0}</font>", ts.ProfitLoss);
                tableCell.SetAttribute("bgcolor", "#f5f5f5");
                tableElem.AppendChild(tableCell);
                tableCell = doc.CreateElement("TD");//交易次数
                tableCell.InnerText = ts.TradeCount.ToString();
                tableCell.SetAttribute("bgcolor", "#f5f5f5");
                tableElem.AppendChild(tableCell);
                tableCell = doc.CreateElement("TD");//手数
                tableCell.InnerText = ts.Volume.ToString();
                tableCell.SetAttribute("bgcolor", "#f5f5f5");
                tableElem.AppendChild(tableCell);
                tableCell = doc.CreateElement("TD");//净利润
                tableCell.InnerText = Math.Round(ts.NetProfit, 2).ToString();
                tableCell.SetAttribute("bgcolor", "#f5f5f5");
                tableElem.AppendChild(tableCell);
                tableCell = doc.CreateElement("TD");//平均每次利润
                tableCell.InnerText = (ts.NetProfit == 0.0 || ts.TradeCount == 0) ? "0.0" : Math.Round(ts.NetProfit / (double)ts.TradeCount, 2).ToString();
                tableCell.SetAttribute("bgcolor", "#f5f5f5");
                tableElem.AppendChild(tableCell);
                tableCell = doc.CreateElement("TD");//平均每手利润
                tableCell.InnerText = (ts.NetProfit == 0.0 || ts.Volume == 0) ? "0.0" : Math.Round(ts.NetProfit / (double)ts.Volume, 2).ToString();
                tableCell.SetAttribute("bgcolor", "#f5f5f5");
                tableElem.AppendChild(tableCell);
                #endregion

                #region 空头汇总行

                tableRow = doc.CreateElement("TR");
                tableRow.SetAttribute("bgcolor", "#C0C0C0");//银色背景色
                tableElem.AppendChild(tableRow);

                tableCell = doc.CreateElement("TD");//品种
                tableCell.InnerText = string.Empty;
                tableCell.SetAttribute("bgcolor", "#f5f5f5");
                tableElem.AppendChild(tableCell);
                tableCell = doc.CreateElement("TD");//多空方向
                tableCell.InnerHtml = string.Format("<font color='#00FF00'><b>{0}</b></font>", PosiDirection.空.ToString());
                tableCell.SetAttribute("bgcolor", "#f5f5f5");
                tableElem.AppendChild(tableCell);
                tableCell = doc.CreateElement("TD");//盈亏标志
                tableCell.InnerText = string.Empty;
                tableCell.SetAttribute("bgcolor", "#f5f5f5");
                tableElem.AppendChild(tableCell);
                tableCell = doc.CreateElement("TD");//交易次数
                tableCell.InnerText = shortTradeCount.ToString();
                tableCell.SetAttribute("bgcolor", "#f5f5f5");
                tableElem.AppendChild(tableCell);
                tableCell = doc.CreateElement("TD");//手数
                tableCell.InnerText = shortVolume.ToString();
                tableCell.SetAttribute("bgcolor", "#f5f5f5");
                tableElem.AppendChild(tableCell);
                tableCell = doc.CreateElement("TD");//净利润
                tableCell.InnerText = Math.Round(shortNetProfit, 2).ToString();
                tableCell.SetAttribute("bgcolor", "#f5f5f5");
                tableElem.AppendChild(tableCell);
                tableCell = doc.CreateElement("TD");//平均每次利润
                tableCell.InnerText =(shortNetProfit==0.0 || shortTradeCount==0)?"0.0": Math.Round(shortNetProfit / (double)shortTradeCount, 2).ToString();
                tableCell.SetAttribute("bgcolor", "#f5f5f5");
                tableElem.AppendChild(tableCell);
                tableCell = doc.CreateElement("TD");//平均每手利润
                tableCell.InnerText = (shortNetProfit == 0.0 || shortVolume == 0) ? "0.0" : Math.Round(shortNetProfit / (double)shortVolume, 2).ToString();
                tableCell.SetAttribute("bgcolor", "#f5f5f5");
                tableElem.AppendChild(tableCell);
                #endregion

                #region 空头盈利行
                tmp = tsList.Where(t => t.InstrumentID == tsList[0].InstrumentID && t.Direction == PosiDirection.空 && t.ProfitLoss == "盈利");
                if (tmp == null || tmp.Count() == 0)
                {
                    ts = new TradeStat()
                    {
                        AvgProfitByHand = 0.0,
                        AvgProfitByTime = 0.0,
                        Direction = PosiDirection.空,
                        InstrumentID = tsList[0].InstrumentID,
                        NetProfit = 0.0,
                         ProfitLoss="盈利",
                        TradeCount = 0,
                        Volume = 0
                    };
                }
                else
                { ts = tmp.ToList()[0]; }
                tableRow = doc.CreateElement("TR");
                tableRow.SetAttribute("bgcolor", "#FFFFFF");//白色背景色
                tableElem.AppendChild(tableRow);

                tableCell = doc.CreateElement("TD");//品种
                tableCell.InnerText = string.Empty;
                tableCell.SetAttribute("bgcolor", "#f5f5f5");
                tableElem.AppendChild(tableCell);
                tableCell = doc.CreateElement("TD");//多空方向
                tableCell.InnerText = string.Empty;
                tableCell.SetAttribute("bgcolor", "#f5f5f5");
                tableElem.AppendChild(tableCell);
                tableCell = doc.CreateElement("TD");//盈亏标志
                tableCell.InnerHtml = string.Format("<font color='#FF0000'>{0}</font>", ts.ProfitLoss);
                tableCell.SetAttribute("bgcolor", "#f5f5f5");
                tableElem.AppendChild(tableCell);
                tableCell = doc.CreateElement("TD");//交易次数
                tableCell.InnerText = ts.TradeCount.ToString();
                tableCell.SetAttribute("bgcolor", "#f5f5f5");
                tableElem.AppendChild(tableCell);
                tableCell = doc.CreateElement("TD");//手数
                tableCell.InnerText = ts.Volume.ToString();
                tableCell.SetAttribute("bgcolor", "#f5f5f5");
                tableElem.AppendChild(tableCell);
                tableCell = doc.CreateElement("TD");//净利润
                tableCell.InnerText = Math.Round(ts.NetProfit, 2).ToString();
                tableCell.SetAttribute("bgcolor", "#f5f5f5");
                tableElem.AppendChild(tableCell);
                tableCell = doc.CreateElement("TD");//平均每次利润
                tableCell.InnerText = (ts.NetProfit == 0.0 || ts.TradeCount == 0) ? "0.0" : Math.Round(ts.NetProfit / (double)ts.TradeCount, 2).ToString();
                tableCell.SetAttribute("bgcolor", "#f5f5f5");
                tableElem.AppendChild(tableCell);
                tableCell = doc.CreateElement("TD");//平均每手利润
                tableCell.InnerText = (ts.NetProfit == 0.0 || ts.Volume == 0) ? "0.0" : Math.Round(ts.NetProfit / (double)ts.Volume, 2).ToString();
                tableCell.SetAttribute("bgcolor", "#f5f5f5");
                tableElem.AppendChild(tableCell);
                #endregion

                #region  空头亏损行
                tmp = tsList.Where(t => t.InstrumentID == tsList[0].InstrumentID && t.Direction == PosiDirection.空 && t.ProfitLoss == "亏损");
                if (tmp == null || tmp.Count() == 0)
                {
                    ts = new TradeStat()
                    {
                        AvgProfitByHand = 0.0,
                        AvgProfitByTime = 0.0,
                        Direction = PosiDirection.空,
                        InstrumentID = tsList[0].InstrumentID,
                        NetProfit = 0.0,
                        ProfitLoss = "亏损",
                        TradeCount = 0,
                        Volume = 0
                    };
                }
                else
                { ts = tmp.ToList()[0]; }
                tableRow = doc.CreateElement("TR");
                tableRow.SetAttribute("bgcolor", "#FFFFFF");//白色背景色
                tableElem.AppendChild(tableRow);

                tableCell = doc.CreateElement("TD");//品种
                tableCell.InnerText = string.Empty;
                tableCell.SetAttribute("bgcolor", "#f5f5f5");
                tableElem.AppendChild(tableCell);
                tableCell = doc.CreateElement("TD");//多空方向
                tableCell.InnerText = string.Empty;
                tableCell.SetAttribute("bgcolor", "#f5f5f5");
                tableElem.AppendChild(tableCell);
                tableCell = doc.CreateElement("TD");//盈亏标志
                tableCell.InnerHtml = string.Format("<font color='#00FF00'>{0}</font>", ts.ProfitLoss);
                tableCell.SetAttribute("bgcolor", "#f5f5f5");
                tableElem.AppendChild(tableCell);
                tableCell = doc.CreateElement("TD");//交易次数
                tableCell.InnerText = ts.TradeCount.ToString();
                tableCell.SetAttribute("bgcolor", "#f5f5f5");
                tableElem.AppendChild(tableCell);
                tableCell = doc.CreateElement("TD");//手数
                tableCell.InnerText = ts.Volume.ToString();
                tableCell.SetAttribute("bgcolor", "#f5f5f5");
                tableElem.AppendChild(tableCell);
                tableCell = doc.CreateElement("TD");//净利润
                tableCell.InnerText = Math.Round(ts.NetProfit, 2).ToString();
                tableCell.SetAttribute("bgcolor", "#f5f5f5");
                tableElem.AppendChild(tableCell);
                tableCell = doc.CreateElement("TD");//平均每次利润
                tableCell.InnerText = (ts.NetProfit == 0.0 || ts.TradeCount == 0) ? "0.0" : Math.Round(ts.NetProfit / (double)ts.TradeCount, 2).ToString();
                tableCell.SetAttribute("bgcolor", "#f5f5f5");
                tableElem.AppendChild(tableCell);
                tableCell = doc.CreateElement("TD");//平均每手利润
                tableCell.InnerText = (ts.NetProfit == 0.0 || ts.Volume == 0) ? "0.0" : Math.Round(ts.NetProfit / (double)ts.Volume, 2).ToString();
                tableCell.SetAttribute("bgcolor", "#f5f5f5");
                tableElem.AppendChild(tableCell);
                #endregion
            }
            tradeStatHtml=tradeStatTemplate = tradeStatTemplate.Replace("<!交易统计!>", tableElem.InnerHtml);
            return tradeStatHtml;
        }

        private bool CreateTradeAccountImg(List<TradingAccountData> tradingAccountList, string imgFile)
        {
            try
            {
                if (File.Exists(imgFile))
                {
                    File.Delete(imgFile);
                }
                List<string> tradingDays = new List<string>();
                List<float> balances = new List<float>();
                foreach (var item in tradingAccountList)
                {
                    tradingDays.Add(item.TradingDay);
                    balances.Add((float)item.Balance);
                }
                //画图初始化
                Bitmap bmap = new Bitmap(500, 500);
                Graphics gph = Graphics.FromImage(bmap);
                gph.Clear(System.Drawing.Color.White);

                PointF cpt = new PointF(40, 420);//中心点
                Font font = new Font("宋体", 14);
                gph.DrawString("权益曲线图", font, System.Drawing.Brushes.Black, new PointF(cpt.X + 60, cpt.X));//图表标题
                //画x轴
                Pen pen = new Pen(Color.Black, 3);
                pen.Width = 4.0f;
                pen.EndCap = LineCap.ArrowAnchor;
                gph.DrawLine(pen, cpt.X, cpt.Y, cpt.Y, cpt.Y);
                font = new Font("宋体", 12);
                gph.DrawString("日期", font, System.Drawing.Brushes.Black, new PointF(cpt.Y + 10, cpt.Y + 10));
                //画y轴
                gph.DrawLine(pen, cpt.X, cpt.Y, cpt.X, cpt.X);
                gph.DrawString("(万元)", font, System.Drawing.Brushes.Black, new PointF(0, 7));
                int numbers = tradingAccountList.Count;
                float yTick = (float)Math.Round((cpt.Y - cpt.X) / (numbers + 1), 2); //Y轴每个刻度的值
                float minBalance = balances.Min();
                float maxBalance = balances.Max();
                float yTickValue = (float)Math.Round((maxBalance - minBalance) / numbers, 2);
                int scaleLength = 10;

                font = new Font("宋体", 8);
                gph.DrawString(Math.Round(minBalance / 10000, 2).ToString(), font, Brushes.Black, new PointF(cpt.X - 30, cpt.Y));
                if (numbers == 1)
                {
                    //保存输出图片
                    bmap.Save(imgFile, System.Drawing.Imaging.ImageFormat.Bmp);
                    return true;
                }
                PointF dotA, dotB;
                for (int i = 1; i < numbers + 1; i++)
                {
                    //画Y轴刻度
                    dotA = new PointF(cpt.X - 30, (float)Math.Round(cpt.Y - i * yTick, 2));
                    gph.DrawString(Math.Round((minBalance + i * yTickValue) / 10000, 2).ToString(), font, Brushes.Black, dotA);
                    dotA = new PointF(cpt.X, (float)Math.Round(cpt.Y - i * yTick, 2));
                    dotB = new PointF(cpt.X + scaleLength, (float)Math.Round(cpt.Y - i * yTick, 2));
                    gph.DrawLine(Pens.Black, dotA, dotB);
                    //画X轴刻度
                    dotA = new PointF((float)Math.Round(cpt.X + i * yTick, 2), cpt.Y + 30);
                    gph.DrawString(tradingDays[i - 1], font, Brushes.Black, dotA);
                    dotA = new PointF((float)Math.Round(cpt.X + i * yTick, 2), cpt.Y);
                    dotB = new PointF((float)Math.Round(cpt.X + i * yTick, 2), cpt.Y - scaleLength);
                    gph.DrawLine(Pens.Black, dotA, dotB);
                    //画点
                    float dotX = (float)Math.Round(cpt.X + i * yTick, 2);
                    float dotY = (float)Math.Round(cpt.Y - (balances[i - 1] - minBalance) / yTickValue * yTick, 2);
                    gph.DrawEllipse(Pens.Black, dotX, dotY, 5, 5);
                    gph.FillEllipse(new SolidBrush(Color.Black), dotX, dotY, 5, 5);
                    //画数值
                    //gph.DrawString(balances[i - 1].ToString(), new Font("宋体", 11), Brushes.Black, new PointF(cpt.X + i * 30, cpt.Y - balances[i - 1] * 3));
                    //画折线
                    if (i > 1)
                    {
                        float preDotX = (float)Math.Round(cpt.X + (i - 1) * yTick, 2);
                        float preDotY = (float)Math.Round(cpt.Y - (balances[i - 2] - minBalance) / yTickValue * yTick, 2);

                        //pen.Width = 3f;
                        //pen.Color = Color.Red;
                        pen = new Pen(Color.Red, 3.0f);
                        pen.EndCap = LineCap.NoAnchor;
                        gph.DrawLine(pen, dotX, dotY, preDotX, preDotY);
                    }
                }
                //保存输出图片
                bmap.Save(imgFile, System.Drawing.Imaging.ImageFormat.Bmp);
            }
            catch (Exception ex)
            {
                Utility.WriteMemFile(ex.ToString());
                return false;
            }
            return true;
        }

        #endregion

    }
}
