using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using CryptoTrading.TQClass;
using CryptoTrading.TQLib;
using CryptoTrading.ViewModel;
using CryptoTrading.OkexSpace;
using BitMex;
using CryptoTrading.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SimpleLogger;
using SimpleLogger.Logging.Handlers;
namespace CryptoTrading
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class TQMain : Window
    {
        public static Trade T;

        public LoginWindow LoginWindow;
        public static LoginConfiguration LoginConfig;
             
        #region DataGridView classes
        //public readonly ObservableCollection<MarketData> MarketDataView = new ObservableCollection<MarketData>();

        //逻辑仓位（人工、策略）
        //public readonly ObservableCollection<LogicPosition> LogicPositionView = new ObservableCollection<LogicPosition>();
        #endregion
        public static readonly TQConcurrentDictionary<string, InstrumentGroup> dicInstrumentIDsGroup = new TQConcurrentDictionary<string, InstrumentGroup>();               
        #region Exchange instances
        OkexBase Okex;
        BitMexBase Bitmex;
        public string CurrentUserID { get; set; }
        public List<Exchange> MyExchanges;
        #endregion
        object sendobject = new object();     

        public static readonly Dictionary<DataGridType, DataGridMapping> dicMainDataGridMapping = new Dictionary<DataGridType, DataGridMapping>();       
        public TQMainVM Viewmodel;
        public Queue<ComboOrder> ComboOrdersQueue = new Queue<ComboOrder>();
        public bool isModifyCustInst;
        private object lockaccount = new object();
        
        #region forwebsocket                      
        InstrumentInfoConfiguration instrumentCfg;       
        #endregion
        public TQMain()
        {
            InitializeComponent();
            try
            {
                Viewmodel = new TQMainVM(this);
                DataContext = Viewmodel;    //use ViewModel as DataContext                
                Logger.LoggerHandlerManager.AddHandler(new TradeLoggerHandler(OutputToLog));               
            }
            catch (Exception eee)
            {              
                Logger.Log(eee, LogCategory.Info);
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Hide();
                LoginWindow = new LoginWindow(this);
                var response = LoginWindow.ShowDialog();
                if (!(response.HasValue && response.Value==true))
                {
                    Close();
                }
                var okex = MyExchanges.FindLast((x) => x.ExchangeID == "Okex");
                Okex = new OkexBase(CurrentUserID, okex);                            
                Okex.OnHeartBeat += Wb_HeartBeat;
                Okex.OnRspFutureTicker += Okex.Wb_OnRspFutureTicker;
                Okex.OnRspFutureTickerV3 += Okex.Wb_OnRspFutureTickerV3;
                Okex.OnRtnAccInfo += Websocketbase_OnRtnAccInfo;
                Okex.OnRspDepthAll += Okex.Wb_OnRspDepthAll;
                Okex.OnRspDepthNew += Wb_OnRspDepthNew;
                Okex.OnRspMessage += Wb_OnRspMessage;
                Okex.OnRspContractTrade += Wb_OnRspContractTrade;
                Okex.OnRspKline += Wb_OnRspKline;
                Okex.OnRspTrade += OKWb_OnRspTrade;
                Okex.OnRtnTrade += wb_OnRtnTrade;
                Okex.OnRtnTradeV3 += wb_OnRtnTradeV3;
                Okex.OnRtnOrdersResultV3 += wb_OnRtnOrdersResultV3;
                Okex.OnRtnOrderInfo += Wb_OnRtnOrderInfo;
                Okex.OnRtnOrders += OkWb_OnRtnOrders;
                Okex.OnRspForecastPrice += Wb_OnRspForecastPrice;
                Okex.OnRspLogin += Wb_OnRspLogin;
                Okex.OnRtnPositions += ok_OnRtnPositions;
                Okex.OnRtnSummaryPositionSwapV3 += Okex_OnRtnSummaryPositionSwapV3;
                Okex.OnRtnSummaryPositionSwapV3Q += Okex_OnRtnSummaryPositionSwapV3Q;
                Okex.OnRtnSummaryPositionV3 += ok_OnRtnSPositionV3;              
                Okex.OnReconnected += Websocketbase_OnReconnected;
                //Okex.OnRtnUserInfo += OK_OnRtnUserInfo;
                Okex.OnRtnUserInfoV3 += OkexFutures_OnRtnAccountInfoV3;
                Okex.OnRtnAccountInfoSwap += Okex_OnRtnAccountInfoSwap;
                Okex.OnError += Websocketbase_OnError;
                Okex.OnFatal += Websocketbase_OnFatal;
                Okex.OnReqReconnect += Websocketbase_OnReqReconnect;
                InitUserDirectoryAndConfiguration("");
                InitTraderByConfig();
                Okex.SetSubscriptionList(TQMainModel.SubscribedInstrumentIDs);
                Task t1 = Okex.StartAsync();                
                Okex.ReqLogin();
                var bitmex = MyExchanges.FindLast((x) => x.ExchangeID == "BitMex");
                Bitmex = new BitMexBase(CurrentUserID, bitmex);
                
                Bitmex.SubInstrumentIDList.AddRange(TQMainModel.SubscribedInstrumentIDs.Where(x => x.Value.ExchangeID == EnuExchangeID.BitMex).ToDictionary(y => y.Key).Keys.ToList());
                Bitmex.clientWebSocketconnector.Start();
                Bitmex.OnRtnWallet += BitMex_OnRtnWallet;
                Bitmex.OnRtnTradeData += BitMex_OnRtnTradeData;
                Bitmex.OnRtnExecution += BitMex_OnRtnExecution;
                Bitmex.OnRtnMargin += BitMex_OnRtnMargin;
                Bitmex.OnRtnOrder += BitMex_OnRtnOrder;
                Bitmex.OnRtnPosition += BitMex_OnRtnPosition;                          
                InitializeMainDataGridMapping();
                #region load DataGrid column from Configuration.xml
                Utility.LoadConfiguration(AccountDataGrid, DataGridType.Account);
                Utility.LoadConfiguration(MarketDataGrid, DataGridType.MarketData);
                Utility.LoadConfiguration(InstrumentDataGrid, DataGridType.Instrument);
                Utility.LoadConfiguration(UnsettledOrdersGrid, DataGridType.UnsettledOrders);
                Utility.LoadConfiguration(TodayOrdersGrid, DataGridType.TodayOrders);
                //Utility.LoadConfiguration(ComplexOrderGrid, DataGridType.ComplexOrders);
                Utility.LoadConfiguration(SettledOrdersGrid, DataGridType.SettledOrders);
                Utility.LoadConfiguration(CanceledOrdersGrid, DataGridType.CanceledOrders);
                Utility.LoadConfiguration(PositionSummaryGrid, DataGridType.PositionSummary);               
                Utility.LoadConfiguration(TradeRecordDetailsGrid, DataGridType.TradeDetails);
                //Utility.LoadConfiguration(TradeRecordSummaryGrid, DataGridType.TradeSummary);
                #endregion
                AccountDataGrid.ItemsSource = Viewmodel.TradingAccountDataView;
                MarketDataGrid.ItemsSource = Viewmodel.MajorMarketDataView;
                MarketDataGrid.SelectedIndex = 0;
                MarketDataListBox.ItemsSource = Viewmodel.MajorMarketDataView;
                InstrumentDataGrid.ItemsSource = TQMainModel.dicInstrumentData.Values;
                TodayOrdersGrid.ItemsSource = Viewmodel.OrderDataView;
                UnsettledOrdersGrid.ItemsSource = Viewmodel.OrderDataView;
                SettledOrdersGrid.ItemsSource = Viewmodel.OrderDataView;
                CanceledOrdersGrid.ItemsSource = Viewmodel.OrderDataView;
                PositionSummaryGrid.ItemsSource = Viewmodel.PositionDataSummaryView;
                TradeRecordDetailsGrid.ItemsSource = Viewmodel.TradeDataView;                                              
                Show();                
            }
            catch (Exception ee)  
            {
                Logger.Log<string>(ee.Data.ToString(), LogCategory.Info);
                if (ee != null)
                {
                    Logger.Log(new LogMessage() {DataCategory=LogCategory.Fatal, CallingClass = this.ToString(), Data = ee.Message });
                    Utility.WriteMemFile(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ff") + " Exception Occured: " + ee.Message);
                    SaveConfiguration();
                    Close();
                }
            }
        }

        private void OutputToLog(object sender, string e)
        {
            var l = JsonConvert.DeserializeObject<LogMessage>(e);
            switch (l.DataCategory)
            {
                case (LogCategory.Debug):
                    Viewmodel.DebugLogs = DateTime.UtcNow.ToString("HH:mm:ss.ff") + "  " + e + Environment.NewLine + Viewmodel.DebugLogs;
                    break;
                case (LogCategory.Trade):
                    Viewmodel.TradeLogs = DateTime.UtcNow.ToString("HH:mm:ss.ff") + "  " + e + Environment.NewLine + Viewmodel.TradeLogs;
                    break;
                case (LogCategory.Error):
                case (LogCategory.Fatal):
                    Viewmodel.SystemLogs = DateTime.UtcNow.ToString("HH:mm:ss.ff") + "  " + e + Environment.NewLine + Viewmodel.SystemLogs;
                    break;
            }
            Utility.WriteMemLog(e);
        }
               
        private void Okex_OnRtnSummaryPositionSwapV3Q(object sender, List<SummaryPositionSwapV3> e)
        {
            PositionDataSummary pds;
            try
            {
                foreach (var s in e)
                {
                    string InstrumentID = s.instrument_id;
                    if (!TQMainModel.dicPositionSummary.TryGetValue(InstrumentID, out pds))
                    {
                        pds = new PositionDataSummary()
                        {
                            CreateDate = s.timestamp,
                            ExchangeID = EnuExchangeID.OkexSwap,
                            InstrumentID = InstrumentID,
                            InvestorID = Okex.UserID.ToString(),
                            LeverRate = s.leverage,
                            ForcePrice = (double)s.liquidation_price,

                        };
                        if (s.side == "long")
                        {
                            pds.LongAvailable = s.avail_position;
                            pds.LongPosition = s.position;
                            pds.LongAvg = s.avg_cost;
                            pds.LongRealProfit = s.realized_pnl;
                        }
                        else
                        {
                            pds.ShortAvailable = s.avail_position;
                            pds.ShortPosition = s.position;
                            pds.ShortAvg = s.avg_cost;
                            pds.ShortRealProfit = s.realized_pnl;
                        }
                        TQMainModel.dicPositionSummary.TQAddOrUpdate(InstrumentID, pds);
                        Dispatcher.BeginInvoke(new Action(() => Viewmodel.PositionDataSummaryView.Add(pds)));
                    }
                    else
                    {
                        if (s.side == "long")
                        {
                            pds.LongAvailable = s.avail_position;
                            pds.LongPosition = s.position;
                            pds.LongAvg = s.avg_cost;
                            pds.LongRealProfit = s.realized_pnl;
                            pds.LongUnrealProfit = 0;
                        }
                        else
                        {
                            pds.ShortAvailable = s.avail_position;
                            pds.ShortPosition = s.position;
                            pds.ShortAvg = s.avg_cost;
                            pds.ShortRealProfit = s.realized_pnl;
                        }
                    }
                    Logger.Log(pds, SimpleLogger.LogCategory.Info);
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void Okex_OnRtnSummaryPositionSwapV3(object sender, List<SummaryPositionSwapClass> e)
        {
            PositionDataSummary pds;
            try
            {
                foreach (var cv in e)
                {
                    string InstrumentID = cv.instrument_id;
                    foreach (var s in cv.holding)
                    {
                        if (!TQMainModel.dicPositionSummary.TryGetValue(InstrumentID, out pds))
                        {

                            pds = new PositionDataSummary()
                            {
                                CreateDate = s.timestamp,                                
                                ExchangeID = EnuExchangeID.OkexSwap,
                                InstrumentID = InstrumentID,
                                InvestorID = Okex.UserID.ToString(),
                                LeverRate = s.leverage,
                                ForcePrice = (double)s.liquidation_price,

                            };
                            if (s.side == "long")
                            {
                                pds.LongAvailable = s.avail_position;
                                pds.LongPosition = s.position;
                                pds.LongAvg = s.avg_cost;
                                pds.LongRealProfit = s.realized_pnl;
                            }
                            else
                            {
                                pds.ShortAvailable = s.avail_position;
                                pds.ShortPosition = s.position;
                                pds.ShortAvg = s.avg_cost;
                                pds.ShortRealProfit = s.realized_pnl;
                            }
                            TQMainModel.dicPositionSummary.TQAddOrUpdate(InstrumentID, pds);
                            Dispatcher.BeginInvoke(new Action(() => Viewmodel.PositionDataSummaryView.Add(pds)));
                        }
                        else
                        {
                            if (s.side == "long")
                            {
                                pds.LongAvailable = s.avail_position;
                                pds.LongPosition = s.position;
                                pds.LongAvg = s.avg_cost;
                                pds.LongRealProfit = s.realized_pnl;
                                pds.LongUnrealProfit = 0;
                            }
                            else
                            {
                                pds.ShortAvailable = s.avail_position;
                                pds.ShortPosition = s.position;
                                pds.ShortAvg = s.avg_cost;
                                pds.ShortRealProfit = s.realized_pnl;
                            }
                        }
                        Logger.Log(pds, SimpleLogger.LogCategory.Info);
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void BitMex_OnRtnPosition(object sender, Position[] e)
        {
            if (e.Length == 0) return;
            Logger.Log(e, SimpleLogger.LogCategory.Info);
            PositionDataSummary pd;
            foreach (var o in e)
            {               
                var key = "BitMex" + o.Account.ToString() + o.Symbol;
                if (!TQMainModel.dicPositionSummary.TryGetValue(key, out pd))
                {
                    pd = new PositionDataSummary()
                    {
                        ForcePrice = o.LiquidationPrice ?? 0,
                        LeverRate = (int)(o.Leverage ?? 0),
                        InvestorID = o.Account.ToString(),
                        InstrumentID = o.Symbol,
                        ExchangeID = EnuExchangeID.BitMex,
                        CreateDate = o.Timestamp ?? DateTime.UtcNow,
                        CrossMargin = o.CrossMargin ?? true,                        
                    };
                    TQMainModel.dicPositionSummary.TQAddOrUpdate(key, pd);
                    Dispatcher.BeginInvoke(new Action<PositionDataSummary>((x) => Viewmodel.PositionDataSummaryView.Add(x)), new object[] { pd });
                }
                if (o.UnrealisedPnl is null || o.AvgCostPrice is null || o.RealisedPnl is null)
                {
                    if (o.CurrentQty < 0)
                    {
                        pd.ShortAvailable = pd.ShortPosition = Math.Abs((int)o.CurrentQty);
                        pd.LongAvailable = 0;
                        pd.LongAvg = 0;
                        pd.LongPosition = 0;
                        pd.LongRealProfit = 0;
                        pd.LongUnrealProfit = 0;
                    }
                    else
                    {
                        pd.LongAvailable = pd.LongPosition = o.CurrentQty ?? 0;
                        pd.ShortAvailable = 0;
                        pd.ShortAvg = 0;
                        pd.ShortPosition = 0;
                        pd.ShortUnrealProfit = (decimal)(o.UnrealisedGrossPnl ?? 0) / 100000000;
                        pd.ShortRealProfit = (decimal)(o.RealisedPnl ?? 0) / 100000000;
                    }
                }
                else
                {
                    if (o.CurrentQty < 0)
                    {
                        pd.ShortAvailable = pd.ShortPosition = Math.Abs((int)o.CurrentQty);
                        pd.ShortAvg = (decimal)(o.AvgCostPrice ?? 0);
                        pd.ShortUnrealProfit = (decimal)(o.UnrealisedGrossPnl ?? 0) / 100000000;
                        pd.ShortRealProfit = (decimal)(o.RealisedPnl ?? 0) / 100000000;

                        pd.LongAvailable = 0;
                        pd.LongAvg = 0;
                        pd.LongPosition = 0;
                        pd.LongRealProfit = 0;
                        pd.LongUnrealProfit = 0;
                    }
                    else
                    {
                        pd.LongAvailable = pd.LongPosition = o.CurrentQty ?? 0;
                        pd.LongAvg = (decimal)(o.AvgCostPrice ?? 0);
                        pd.LongUnrealProfit = (decimal)(o.UnrealisedGrossPnl ?? 0) / 100000000;
                        pd.LongRealProfit = (decimal)(o.RealisedPnl ?? 0) / 100000000;
                        pd.ShortAvailable = 0;
                        pd.ShortAvg = 0;
                        pd.ShortPosition = 0;
                        pd.ShortUnrealProfit = (decimal)(o.UnrealisedGrossPnl ?? 0) / 100000000;
                        pd.ShortRealProfit = (decimal)(o.RealisedPnl ?? 0) / 100000000;
                    }
                }
                //Logger.Log(pd, "", "");              
            }            
        }

        private void BitMex_OnRtnOrder(object sender, OrderData[] e)
        {
            if (e.Length == 0) return;
            OrderData order;
            string updatetime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ff");
            foreach (var o in e)
            {
                var key = "BitMex" + o.InstrumentID + o.OrderID;

                o.UpdateTime = updatetime;
                if (TQMainModel.dicOrder.TryGetValue(key, out order))
                {                   
                    if (o.OrderPrice > 0) order.OrderPrice = o.OrderPrice;
                    if (o.OrderSize > 0) order.OrderSize = o.OrderSize;
                    if (o.AvgPrice > 0) order.AvgPrice = o.AvgPrice;
                    if (o.QuantFilled > 0) order.QuantFilled = o.QuantFilled;
                    if (order.QuantUnfilled > 0) order.QuantUnfilled = o.QuantUnfilled;
                    if (order.OrderStatus != OrderStatusType.全部成交)
                        order.OrderStatus = o.OrderStatus;
                }
                else
                {                    
                    TQMainModel.dicOrder.TQAddOrUpdate(key, o);
                    Dispatcher.BeginInvoke(new Action(() => Viewmodel.OrderDataView.Add(o)));
                }
                Logger.Log(o, LogCategory.Info);
            }
        }

        private void BitMex_OnRtnMargin(object sender, Margin[] e)
        {
            if (e.Length == 0) return;
            TradingAccountData td;
            lock (lockaccount)
            {
                foreach (var m in e)
                {
                    if ((td = Viewmodel.TradingAccountDataView.ToList().Find(x => x.ExchangeID == "BitMex" && x.DigitalCurrencyID == m.Currency && x.InvestorID == m.Account.ToString())) == null)
                    {
                        if (m.Amount.HasValue && ((m.Amount ?? 0) > (decimal)0.001))
                        {
                            td = new TradingAccountData()
                            {
                                ExchangeID = "BitMex",
                                InvestorID = m.Account.ToString(),
                                DigitalCurrencyID = m.Currency,
                                Equity = (double)(m.Amount ?? 0) / 100000000,
                                Available = (double)(m.AvailableMargin ?? 0) / 100000000,
                                CloseProfit = (double)(m.RealisedPnl ?? 0) / 100000000,
                                PositionProfit = (double)(m.UnrealisedPnl ?? 0) / 100000000,
                                UsedMargin = (double)(m.MaintMargin ?? 0) / 100000000,
                                Commission = (double)(m.GrossComm ?? 0) / 100000000,
                                Risk = (m.MarginUsedPcnt ?? 0).ToString(),
                                UpdateTime = (m.Timestamp ?? DateTime.UtcNow),
                            };
                            td.Equity = (double)((m.Amount ?? 0) / 100000000);                           
                            Dispatcher.Invoke(new Action(() => Viewmodel.TradingAccountDataView.Add(td)));
                        }
                    }
                    else
                    {
                        if (!m.Account.HasValue || !m.AvailableMargin.HasValue)
                            return;
                        td.Available = (double)(m.AvailableMargin ?? 0) / 100000000;
                        if (m.RealisedPnl.HasValue) td.CloseProfit = (double)(m.RealisedPnl ?? 0) / 100000000;
                        if (m.UnrealisedPnl.HasValue) td.PositionProfit = (double)(m.UnrealisedPnl ?? 0) / 100000000;
                        if (m.MaintMargin.HasValue) td.UsedMargin = (double)((m.MaintMargin ?? 0) / 100000000);
                        if (m.Amount.HasValue && ((m.Amount ?? 0) > (decimal)0.1))
                            td.Equity = (double)((m.Amount ?? 0) / 100000000);
                        td.Commission = (double)(m.GrossComm ?? 0) / (100000000);
                        if (m.MarginUsedPcnt.HasValue) td.Risk = m.MarginUsedPcnt.ToString();
                        td.UpdateTime = (m.Timestamp ?? DateTime.UtcNow);                        
                    }                    
                }
            }
        }

        private void BitMex_OnRtnExecution(object sender, TradeData[] e)
        {
            if (e.Length == 0) return;

            TradeData td;
            foreach (var exe in e)
            {
                var key = exe.ExchangeID + exe.InvestorID + exe.TradeID;
                if (!TQMainModel.dicTradeData.TryGetValue(key, out td))
                {
                    TQMainModel.dicTradeData.TQAddOrUpdate(exe.ExchangeID + exe.InvestorID + exe.TradeID, exe);
                    Dispatcher.BeginInvoke(new Action<TradeData>((tt) => Viewmodel.TradeDataView.Add(tt)), new object[] { exe });
                }
                else
                {
                    td.AvgPrice = exe.AvgPrice;
                    td.OrderStatus = exe.OrderStatus;
                    td.OrderTime = exe.OrderTime;
                    td.UpdateTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ff");                        
                    td.Quant = exe.Quant;
                    td.QuantTraded = exe.QuantTraded;
                }
                Logger.Log(exe, SimpleLogger.LogCategory.Info);
            }
        }

        private void BitMex_OnRtnTradeData(object sender, TradeData[] e)
        {
            TradeData trade;
            foreach (var t in e)
            {
                var key = t.ExchangeID + t.InvestorID + t.TradeID;
                if (!TQMainModel.dicTradeData.TryGetValue(key, out trade))
                {
                    TQMainModel.dicTradeData.AddOrUpdate(key, t, (x, y) => t);
                    Dispatcher.BeginInvoke(new Action(() => Viewmodel.TradeDataView.Add(t)));
                }
                else
                {
                    //trade.OrderPrice = t.OrderPrice;
                    trade.AvgPrice = t.AvgPrice;
                    trade.OrderStatus = t.OrderStatus;
                    trade.Commission = t.Commission;
                    trade.QuantTraded = t.QuantTraded;
                }
                Logger.Log(t, SimpleLogger.LogCategory.Info);
            }
        }

        private void BitMex_OnRtnWallet(object sender, TradingAccountData x)
        {
            lock (lockaccount)
            {
                var ta = Viewmodel.TradingAccountDataView.ToList().Find(
                          (tad) => tad.DigitalCurrencyID == x.DigitalCurrencyID && tad.InvestorID == x.InvestorID && tad.ExchangeID == "BitMex");
                if (ta != null)
                {
                    if (x.Equity > 0.00001)
                    {
                        ta.Equity = x.Equity;
                        ta.BalanceBtc = x.BalanceBtc;
                        ta.UpdateTime = x.UpdateTime;
                    }
                }                
            }
            //Logger.Log(x, LogCategory.Debug);
        }
        
        private void OkWb_OnRtnOrders(object sender, Dictionary<string, OrderData> e)
        {          
            foreach (var v in e)
            {
                if (!TQMainModel.dicOrder.TryGetValue(v.Key, out OrderData o))
                {
                    TQMainModel.dicOrder.TQAddOrUpdate(v.Key, v.Value);
                    Dispatcher.BeginInvoke(new Action(() => Viewmodel.OrderDataView.Add(v.Value)));
                }
                Logger.Log(v.Value, LogCategory.Info);
            }
        }
        private void Websocketbase_OnFatal(object sender, WebSocketSharp.ErrorEventArgs e)
        {
            var mm = e.errorType.ToString() + ": " + e.Message;
            Logger.Log(e.Message, LogCategory.Fatal);
            Utility.WriteMemLog(mm);            
        }

        private void Websocketbase_OnReqReconnect(object sender, EventArgs e)
        {
            Okex.ReqLogin();
        }

        private void Websocketbase_OnError(object sender, WebSocketSharp.ErrorEventArgs e)
        {
            Utility.WriteMemLog(e.Message);
            Logger.Log(e.Message, LogCategory.Error);         
        }

        #region websocket handlers
        
        private void ok_OnRtnSPositionV3(object sender, List<SummaryPositionV3> aa)
        {
            PositionDataSummary pds;
            foreach (var cv in aa)
            {
                string InstrumentID = cv.instrument_id.ToOkexFuturesInstrumentid();
                if (!TQMainModel.dicPositionSummary.TryGetValue(InstrumentID, out pds))
                {
                    pds = new PositionDataSummary()
                    {
                        CreateDate = cv.created_at,
                        //ContractID = cv.contract_id,//todo: del
                        ExchangeID = cv.instrument_id.Contains("SWAP")? EnuExchangeID.OkexSwap:EnuExchangeID.OkexFutures,
                        InstrumentID = InstrumentID,
                        //ContractType = cv.contract_type,
                        InvestorID = Okex.UserID.ToString(),
                        LeverRate = cv.leverage,
                        ForcePrice = (double)cv.liquidation_price,
                        LongAvailable = cv.long_avail_qty,
                        LongPosition = cv.long_qty,
                        LongAvg = (decimal)cv.long_avg_cost,
                        LongRealProfit = cv.realised_pnl,
                        LongUnrealProfit = 0,
                        ShortAvailable = cv.short_avail_qty,
                        ShortAvg = cv.short_avg_cost,
                        ShortPosition = cv.short_qty,
                        ShortRealProfit = cv.realised_pnl,
                        ShortUnrealProfit = 0,
                    };
                    TQMainModel.dicPositionSummary.TQAddOrUpdate(InstrumentID, pds);
                    Dispatcher.BeginInvoke(new Action(() => Viewmodel.PositionDataSummaryView.Add(pds)));
                }
                else
                {
                    pds.LongAvailable = cv.long_avail_qty;
                    pds.LongAvg = cv.long_avg_cost;
                    pds.LongPosition = cv.long_qty;
                    pds.LongRealProfit = cv.realised_pnl;
                    pds.ShortAvailable = cv.short_avail_qty;
                    pds.ShortPosition = cv.short_qty;
                    pds.ShortRealProfit = cv.realised_pnl;
                    pds.ShortAvg = (decimal)cv.short_avg_cost;
                }
            }
        }
        private void ok_OnRtnPositions(object sender, ClassRspPosition e)
        {
            PositionDetail p;
            PositionDataSummary pds;
            foreach (RspPosition v in e.positions)
            {
                if (v.holdAmount <= 0) continue;
                string key = ExchaneID.Okex + e.user_id.ToString() + v.contractposition + v.contract_id + v.position_id;

                if (TQMainModel.dicPositionDetails.TryGetValue(key, out p))
                {
                    p.OpenPrice = (decimal)v.contractavgprice; //contractcostprice; //v.contractavgprice                   
                    p.PositionSize = (decimal)v.holdAmount;
                    p.PositionAvailable = (decimal)v.contracteveningup;
                    p.Margin = (decimal)v.margin;
                    //p.PositionID = v.position_id.ToString();                                     
                    //p.InstrumentID = v.contract_name;
                    //p.OpenDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ff");
                    //p.ExchangeID = "Okex";
                    //p.Direction = v.contractposition == "1" ? TradeDirection.多 : TradeDirection.空;                   
                }
                else
                {
                    p = new PositionDetail()
                    {
                        OpenPrice = (decimal)v.contractavgprice,
                        Margin = (decimal)v.margin,
                        PositionID = v.position_id.ToString(),
                        PositionAvailable = (decimal)v.contracteveningup,
                        PositionSize = (int)v.holdAmount,
                        OpenDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ff"),
                        ExchangeID = "Okex",
                        InvestorID = Okex.UserID.ToString(),      //pd.Volume=(int)v.contracteveningup;
                        InstrumentID = v.contract_name,
                        Direction = v.contractposition == "1" ? TradeDirection.Long : TradeDirection.Short
                    };
                    if (p.PositionSize >= 0)
                    {
                        TQMainModel.dicPositionDetails.TQAddOrUpdate(key, p);
                        Dispatcher.Invoke(() => Viewmodel.PositionDetailView.Add(p));
                    }
                }
                //此处更新持仓汇总数据
                if (TQMainModel.dicPositionSummary.TryGetValue(p.InstrumentID, out pds))
                {
                    if (p.Direction == TradeDirection.Long)
                    {
                        pds.LongPosition = p.PositionSize;
                        pds.LongAvailable = p.PositionAvailable;
                        pds.LongAvg = p.OpenPrice;
                    }
                    else
                    {
                        pds.ShortPosition = (int)p.PositionSize;
                        pds.ShortAvg = p.OpenPrice;
                        pds.ShortAvailable = p.PositionAvailable;
                    }
                }
                else
                {
                    pds = new PositionDataSummary()
                    {
                        InstrumentID = p.InstrumentID,
                        InvestorID = p.InvestorID,
                        ExchangeID = (EnuExchangeID)Enum.Parse(typeof(EnuExchangeID), p.ExchangeID),
                        CreateDate = DateTime.Now,

                    };
                    if (p.Direction == TradeDirection.Long)
                    {
                        pds.LongAvg = p.OpenPrice;
                        pds.LongPosition = (int)p.PositionSize;
                    }
                    else
                    {
                        pds.ShortPosition = (int)p.PositionSize;
                        pds.LongAvg = p.OpenPrice;
                    }
                    TQMainModel.dicPositionSummary.TQAddOrUpdate(pds.InstrumentID, pds);
                    Dispatcher.BeginInvoke(new Action(() => Viewmodel.PositionDataSummaryView.Add(pds)));
                }
            }
        }

        private void Wb_OnRspLogin(object sender, ResponseLogin e)
        {
            Console.WriteLine("Login Response: " + e.data);
        }
        private void Websocketbase_OnReconnected(object sender, EventArgs e)
        {
            //websocketbase.ReqLogin(Exchange.Okex);
            ReqSubscription();

        }
        private void Wb_OnReconnected(object sender, EventArgs e)
        {

        }
        private void Wb_OnRspForecastPrice(object sender, ForecastPrice e)
        {
            Console.WriteLine("ForecastPrice for " + e.channel + ": " + e.data);
        }

        private void Wb_OnRtnTrade(object sender, ClassTradeResult e)
        {
            //处理order，在报单处，添加报单记录，在这里确认报单状态
            Utility.WriteMemLog("New Trade: " + e.channel + "  " + e.data.order_id);
            //ClassTradeResult cc = e;
            //Console.WriteLine(DateTime.Now.ToString() + "  " + e.ToString());
        }

        private void Wb_OnRspKline(object sender, MDKLines e)
        {
            MDKLine mdk;
            List<MDKLine> mdklist;// = new List<MDKLine>();
            var s = e.channel.Split('_');
            string ss = s[3] + (s[6] == "this" ? "this_week" : s[6] == "next" ? "next_week" : "quarter") + s[s.Length - 1];
            for (int i = 0; i < e.data.Length; i++)
            {
                mdk = new MDKLine()
                {
                    Time = long.Parse(e.data[i][0]),
                    Open = double.Parse(e.data[i][1]),
                    High = double.Parse(e.data[i][2]),
                    Low = double.Parse(e.data[i][3]),
                    Close = double.Parse(e.data[i][4]),
                    ContractVol = double.Parse(e.data[i][5]),
                    Vol = double.Parse(e.data[i][6])
                };
                if (Okex.dicMDKlines.TryGetValue(ss, out mdklist))
                    Okex.dicMDKlines[ss].Add(mdk);
                else
                    Okex.dicMDKlines.Add(ss, new List<MDKLine> { mdk });

            }
        }
        #endregion
        #region Response Event Handling Methods

        private void Wb_OnRspContractTrade(object sender, ContractTradeInfo e)
        {
            string[] strarr = e.channel.Split('_');
            string key = strarr[3] + (strarr[5] == "this" ? "this_week" : strarr[5] == "next" ? "next_week" : "quarter");
            List<ContractTrade> listcontracttrade = new List<ContractTrade>();
            for (int i = 0; i < e.data.Length; i++)
            {
                listcontracttrade.Add(new ContractTrade()
                {
                    tradeid = e.data[i][0],
                    price = e.data[i][1],
                    quant = e.data[i][2],
                    time = e.data[i][3],
                    type = e.data[i][4],
                    //amount = e.data[i][5]
                });
            }
            List<ContractTrade> diccontracttradelist = new List<ContractTrade>();
            if (Okex.DicContracttrade.TryGetValue(key, out diccontracttradelist))
                Okex.DicContracttrade[key].AddRange(listcontracttrade);
            else
                Okex.DicContracttrade.Add(key, listcontracttrade);

        }
        private void Wb_OnRspDepthNew(object sender, ResponseDepth e)
        {
            //process 200 addition data            
            Okex.dicDepth[e.channel] = e.data;
        }

        private void Wb_OnRspMessage(object sender, string e)
        {
            //Console.WriteLine(DateTime.Now.ToString() + "  " + e);
            //Console.WriteLine("----------------------------");
        }
        private void Wb_HeartBeat(object sender, string e)
        {
            //foreach (var v in e) Console.Write(v + " ");
            //Console.WriteLine(e+"   -------------");            
        }

       
        public async Task<bool> PlaceComboOrderAsync()
        {
            ComboMarketData combo;
            double offset = 1;
            if (!TQMainModel.dicCustomProduct.TryGetValue(Viewmodel.CurrentMarketData.InstrumentID, out combo) || combo.ItemList.Count <= 0)
                return false;
            //生成组合单
            ComboOrder comboorder = new ComboOrder() { InstrumentID = combo.InstrumentID, dicComboOrder = new Dictionary<string, OrderData>() };
            OrderData orderdata;
            int ORDERQUANT = (int)(Viewmodel.CurrentMarketData.OrderBoardTradeDirection == TradeDirection.Long ?
                (Viewmodel.CurrentMarketData.OrderBoardQuant > Viewmodel.CurrentMarketData.AskSize1 ? Viewmodel.CurrentMarketData.AskSize1 : Viewmodel.CurrentMarketData.OrderBoardQuant)
                : ((Viewmodel.CurrentMarketData.OrderBoardQuant > Viewmodel.CurrentMarketData.BidSize1) ? Viewmodel.CurrentMarketData.BidSize1 : Viewmodel.CurrentMarketData.OrderBoardQuant));
            foreach (var v in combo.ItemList)
            {
                orderdata = new OrderData();
                if (v.Weight > 0)
                {
                    orderdata.Direction = Viewmodel.CurrentMarketData.OrderBoardTradeDirection;
                }
                else
                    orderdata.Direction = Viewmodel.CurrentMarketData.OrderBoardTradeDirection == TradeDirection.Short ? TradeDirection.Long : TradeDirection.Short;

                
                MajorMarketData mmd;
                if (!TQMainModel.dicMajorMarketData.TryGetValue(v.InstrumentID, out mmd))
                    return false;
                switch (Viewmodel.CurrentMarketData.OrderBoardPricingMode)
                {
                    case PricingMode.Market:
                        //order.MatchPrice = "0";
                        if (mmd.ExchangeID == EnuExchangeID.BitMex)
                        {
                            orderdata.OrderPrice = orderdata.Direction == TradeDirection.Long ? mmd.AskPrice1 + 15 : mmd.BidPrice1 - 15;
                        }
                        else
                        {
                            if (orderdata.Direction == TradeDirection.Long)
                                orderdata.OrderPrice = mmd.AskPrice1 * (decimal)1.001;
                            else
                                orderdata.OrderPrice = mmd.BidPrice1 * (decimal)0.999;
                        }
                        break;
                    case PricingMode.MiddlePrice:
                        orderdata.OrderPrice = Math.Round((mmd.AskPrice1 + mmd.BidPrice1) / 2, 2);
                        break;
                    case PricingMode.OppositePlus:
                        //order.MatchPrice = "0";
                        if (orderdata.Direction == TradeDirection.Long)
                            orderdata.OrderPrice = mmd.AskPrice1 + (decimal)offset / 2;
                        else
                            orderdata.OrderPrice = mmd.BidPrice1 - (decimal)offset / 2;
                        break;
                    case PricingMode.Preset:
                        //here start the strategic trading, finish the code later
                        return false;
                }
                //生成数量
                orderdata.OrderSize = Math.Abs(v.Weight) * ORDERQUANT;
                orderdata.ExchangeID = mmd.ExchangeID;
                //orderdata.ClientOID = 
                if (orderdata.OrderSize == 0) return false;
                orderdata.InstrumentID = v.InstrumentID;  //.Substring(0, 3).ToUpper() + TQMainVM.dicMajorMarketData[v.InstrumentID].ContractId.ToString().Substring(4, 4);
                //orderdata.Offset = Viewmodel.CurrentMarketData.OrderBoardOrderMode == OrderMode.Open ? OffsetType.开仓 :
                //    Viewmodel.CurrentMarketData.OrderBoardOrderMode == OrderMode.Close ? OffsetType.平仓 : OffsetType.平仓;
                orderdata.LeverRate = Viewmodel.CurrentMarketData.OrderBoardHedgeRatio == HedgeRatio.Twenty ? 20 : 10;
                if (orderdata.InstrumentID.ToUpper().Contains("EOS"))
                    orderdata.LeverRate = 50;
                else
                if (orderdata.InstrumentID.ToUpper().Contains("BTC"))
                    orderdata.LeverRate = 100;
                comboorder.dicComboOrder.Add(orderdata.InstrumentID, orderdata);
            }
            ComboOrdersQueue.Enqueue(comboorder);
            ProcessComboOrder();
           
            return true;
        }
        internal void ProcessComboOrder()
        {
            {
                while (ComboOrdersQueue.Count > 0)
                {
                    var co = ComboOrdersQueue.Dequeue();
                    Task.Run(() =>
                    {
                        if (co.InstrumentID.Contains("ETHUSD"))
                        {
                            //calculate ETHUSD QUANTITY BY MARKETDATA OF ETHUSD AND XBTUSD
                            co.dicComboOrder["ETHUSD"].OrderSize = Math.Round((co.dicComboOrder["ETHUSD"].OrderSize * (decimal)Math.Pow(10, 6) / TQMainModel.dicMajorMarketData["ETHUSD"].LastPrice / TQMainModel.dicMajorMarketData["XBTUSD"].LastPrice));
                        }
                        foreach (var o in co.dicComboOrder.Values)
                        {
                            SendOrder(o);
                            
                        }
                    });
                }
            }
        }
        public void SendOrder(OrderData order)
        {
            switch (order.ExchangeID)
            {
                case (EnuExchangeID.OkexSwap):                    
                case (EnuExchangeID.OkexFutures):             
                    {
                        PositionDataSummary pds;
                        order.InstrumentID = order.ExchangeID==EnuExchangeID.OkexFutures?
                            order.InstrumentID.ToOkexFuturesInstrumentid() : order.InstrumentID.ToOkexSwapInstrumentid();
                        if (!TQMainModel.dicPositionSummary.TryGetValue(order.InstrumentID, out pds))
                        {
                            order.Offset = OffsetType.开仓;
                            var okexresponse = order.ExchangeID + " Order: " + Okex.SendOrderRest(order);
                            Utility.WriteMemLog(okexresponse);
                            Logger.Log(okexresponse, LogCategory.Trade);
                        }
                        else
                        {
                            if (order.Direction == TradeDirection.Long)
                            {
                                if (pds.ShortAvailable == 0)
                                {
                                    order.Offset = OffsetType.开仓;
                                    var okexresponse = order.ExchangeID + " Order: " + Okex.SendOrderRest(order);
                                    Utility.WriteMemLog(okexresponse);
                                    Logger.Log(okexresponse, LogCategory.Trade);
                                }
                                else
                                {
                                    if (pds.ShortAvailable >= order.OrderSize)
                                    {
                                        order.Offset = OffsetType.平仓;
                                        var okexresponse = order.ExchangeID + " Order: " + Okex.SendOrderRest(order);
                                        Utility.WriteMemLog(okexresponse);
                                        Logger.Log(okexresponse, LogCategory.Trade);
                                    }
                                    else
                                    {//处理反向持仓，先平后开仓
                                        order.Offset = OffsetType.平仓;
                                        decimal d = order.OrderSize;
                                        order.OrderSize = pds.ShortAvailable;
                                        var okexresponse = order.ExchangeID + " Order: " + Okex.SendOrderRest(order);
                                        Utility.WriteMemLog(okexresponse);
                                        Logger.Log(okexresponse, LogCategory.Trade);
                                        order.Offset = OffsetType.开仓;
                                        order.OrderSize = d - pds.ShortAvailable;
                                        var okexresponse1 = order.ExchangeID + " Order: " + Okex.SendOrderRest(order);
                                        Utility.WriteMemLog(okexresponse1);
                                        Logger.Log(okexresponse, LogCategory.Trade);
                                    }
                                }
                            }
                            else
                            {
                                if (pds.LongAvailable == 0)
                                {
                                    order.Offset = OffsetType.开仓;
                                    var okexresponse = order.ExchangeID + " Order: " + Okex.SendOrderRest(order);
                                    Utility.WriteMemLog(okexresponse);
                                    Logger.Log(okexresponse, LogCategory.Trade);
                                }
                                else
                                {
                                    if (pds.LongAvailable >= order.OrderSize)
                                    {
                                        order.Offset = OffsetType.平仓;
                                        var okexresponse = order.ExchangeID + " Order: " + Okex.SendOrderRest(order);
                                        Utility.WriteMemLog(okexresponse);
                                        Logger.Log(okexresponse, LogCategory.Trade);
                                    }
                                    else
                                    {//处理反向持仓，先平后开
                                        order.Offset = OffsetType.平仓;
                                        decimal d = order.OrderSize;
                                        order.OrderSize = pds.LongAvailable;
                                        var okexresponse = order.ExchangeID + " Order: " + Okex.SendOrderRest(order);
                                        Utility.WriteMemLog(okexresponse);
                                        Logger.Log(okexresponse, LogCategory.Trade);
                                        order.Offset = OffsetType.开仓;
                                        order.OrderSize = d - pds.LongAvailable;
                                        var okexresponse1 = order.ExchangeID + " Order: " + Okex.SendOrderRest(order);
                                        Utility.WriteMemLog(okexresponse1);
                                        Logger.Log(okexresponse1, LogCategory.Trade);
                                    }
                                }
                            }
                        }
                    }
                    break;
                case (EnuExchangeID.BitMex):
                    {
                        string response = "";
                        BMOrder bmo;
                        response = Bitmex.SendOrderRest(order);
                        var bitmexresponse = order.ExchangeID + " Order: " + response;
                        Utility.WriteMemLog(bitmexresponse);
                        Logger.Log(bitmexresponse, LogCategory.Trade);                                               
                        bmo = JsonConvert.DeserializeObject<JObject>(response).ToObject<BMOrder>();
                        BitMex_OnRtnOrder(this, BitUtility.ConvertBMOrderOrderData(new BMOrder[] { bmo }));
                        

                    }
                    break;
            }
        }
        public bool PlaceOrder()
        {
            OrderData order = new OrderData();
            order.InstrumentID = Viewmodel.CurrentMarketData.InstrumentID;
            if (order.OrderPriceType == OrderPriceType.市价)
                order.OrderSize = Viewmodel.CurrentMarketData.GetOrderBoardQty();
            else
                order.OrderSize = Viewmodel.CurrentMarketData.OrderBoardQuant;

            order.Direction = Viewmodel.CurrentMarketData.OrderBoardTradeDirection;

            //生成价格
            switch (Viewmodel.CurrentMarketData.OrderBoardPricingMode)
            {
                case PricingMode.Market:
                    order.OrderPrice = Viewmodel.CurrentMarketData.OrderBoardTradeDirection == TradeDirection.Long ? Viewmodel.CurrentMarketData.AskPrice5 : Viewmodel.CurrentMarketData.BidPrice5;
                    break;
                case PricingMode.MiddlePrice:
                    order.OrderPrice = (Viewmodel.CurrentMarketData.BidPrice1 + Viewmodel.CurrentMarketData.AskPrice1) / 2;
                    break;
                case PricingMode.Preset:
                    if ((order.OrderPrice = Viewmodel.CurrentMarketData.OrderBoardPrice) == 0)
                        if (Viewmodel.CurrentMarketData.OrderBoardTradeDirection == TradeDirection.Long) order.OrderPrice = Viewmodel.CurrentMarketData.AskPrice1;
                        else order.OrderPrice = Viewmodel.CurrentMarketData.BidPrice1;
                    break;
                case PricingMode.Ownside:
                    order.OrderPrice = Viewmodel.CurrentMarketData.OrderBoardTradeDirection == TradeDirection.Long ?
                        Viewmodel.CurrentMarketData.BidPrice1 : Viewmodel.CurrentMarketData.AskPrice1;
                    break;
                case PricingMode.OppositePlus:
                    order.OrderPrice = Viewmodel.CurrentMarketData.OrderBoardTradeDirection == TradeDirection.Long ? Viewmodel.CurrentMarketData.AskPrice2 : Viewmodel.CurrentMarketData.BidPrice2;
                    break;
            }
            order.LeverRate = Viewmodel.CurrentMarketData.OrderBoardHedgeRatio == HedgeRatio.Twenty ? 20 : 10;
            if (order.InstrumentID.ToUpper().Contains("EOS"))
                order.LeverRate = 50;
            if (order.InstrumentID.ToUpper().Contains("BTC"))
                order.LeverRate = 100;
            order.InstrumentID = Viewmodel.CurrentMarketData.InstrumentID;
            order.ExchangeID = Viewmodel.CurrentMarketData.ExchangeID;
            
            Task.Run(() => SendOrder(order));                                  
            return true;
        }
        #endregion

        private void Wb_OnRtnOrderInfo(object sender, FutureTrade e)
        {
            OrderDataAddOrUpdate(e, out OrderData order);
            Logger.Log(e, SimpleLogger.LogCategory.Info);

        }
        bool OrderDataAddOrUpdate(FutureTrade pOrder, out OrderData order)
        {
            //SWAP status">订单状态(-2":失败,"-1":撤单成功,"0":等待成交 ,"1":部分成交, "2":完全成交,"3":下单中,"4":撤单中,"6": 未完成（等待成交+部分成交），"7":已完成（撤单成功+完全成交））
            //Futures status">订单状态("-2":失败,"-1":撤单成功,"0":等待成交 ,"1":部分成交, "2":完全成交,"3":下单中,"4":撤单中,"6": 未完成（等待成交+部分成交），"7":已完成（撤单成功+完全成交））
          
            bool FutureOrSwap = pOrder.data.contract_name.Contains("SWAP") ? false : true;
            OrderData orderData;
            string orderKey = pOrder.data.user_id + pOrder.data.orderid;

            if (TQMainModel.dicOrder.TryGetValue(orderKey, out order))
                return true;
            {
                orderData = new OrderData()
                {
                    //UpdateTime = pOrder.data.create_date_str,
                    //CreateDate = pOrder.data.create_date_str.Substring(0, 10),
                    CreateTime = pOrder.data.create_date_str, //.Substring(11, 8),
                    OrderPrice = (decimal)pOrder.data.price,
                    QuantFilled = (decimal)pOrder.data.deal_amount,
                    QuantUnfilled = (decimal)(pOrder.data.amount - pOrder.data.deal_amount),
                    OrderSize = (decimal)pOrder.data.amount,
                    InvestorID = pOrder.data.user_id,
                    InstrumentID = pOrder.data.contract_name,
                    //StatusMsg = pOrder.StatusMsg,
                    //UserID = pOrder.data.user_id,
                    //OrderStatus = pOrder.data.status,                    
                    ExchangeID = EnuExchangeID.OkexFutures,
                    //Direction = pOrder.data.Direction,                   
                    OrderSubmitStatus = OrderSubmitStatusType.已经接受,
                    OrderPriceType = OrderPriceType.限价
                };
            }
            switch (pOrder.data.type)
            {
                case 1:
                    orderData.Direction = TradeDirection.Long;
                    orderData.Offset = OffsetType.开仓;
                    break;
                case 2:
                    orderData.Direction = TradeDirection.Short;
                    orderData.Offset = OffsetType.开仓;
                    break;
                case 3:
                    orderData.Direction = TradeDirection.Short;
                    orderData.Offset = OffsetType.平仓;
                    break;
                case 4:
                    orderData.Direction = TradeDirection.Long;
                    orderData.Offset = OffsetType.平仓;
                    break;
            }
            if (FutureOrSwap)
            {
                //Futures status">订单状态(-1.撤单成功；0:等待成交 1:部分成交 2:全部成交
                //6：未完成（等待成交+部分成交）7：已完成（撤单成功+全部成交）
                switch (pOrder.data.status)
                {
                    case "-1":
                        {
                            orderData.CancelTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ff");
                            orderData.OrderStatus = OrderStatusType.已撤单;
                            //order.OrderSubmitStatus = (OrderSubmitStatusType)pOrder._OrderSubmitStatus;
                            //order.StatusMsg = pOrder._StatusMsg;
                            orderData.QuantUnfilled = orderData.OrderSize - (decimal)pOrder.data.deal_amount;
                            orderData.QuantFilled = (int)pOrder.data.deal_amount;
                        }
                        break;
                    case "0":
                        orderData.OrderStatus = OrderStatusType.未成交;
                        orderData.QuantUnfilled = orderData.OrderSize - (decimal)pOrder.data.deal_amount;
                        orderData.QuantFilled = (int)pOrder.data.deal_amount;
                        break;
                    case "1":
                        orderData.OrderStatus = OrderStatusType.部分成交;
                        orderData.QuantUnfilled = orderData.OrderSize - (int)pOrder.data.deal_amount;
                        orderData.QuantFilled = (int)pOrder.data.deal_amount;
                        break;
                    case "2":
                        orderData.OrderStatus = OrderStatusType.全部成交;
                        orderData.QuantUnfilled = 0; // orderData.OrderQuant - (int)pOrder.data.deal_amount;
                        orderData.QuantFilled = (int)pOrder.data.deal_amount;
                        break;
                    case "6":
                        orderData.OrderStatus = OrderStatusType.部分成交;
                        orderData.QuantUnfilled = orderData.OrderSize - (decimal)pOrder.data.deal_amount;
                        orderData.QuantFilled = (int)pOrder.data.deal_amount;
                        break;
                    case "7": //7：已完成（撤单成功+全部成交）
                        orderData.OrderStatus = OrderStatusType.已完成;
                        orderData.QuantFilled = orderData.QuantFilled;
                        orderData.QuantUnfilled = 0;
                        break;
                }
            }
            else
            {
                //SWAP status">订单状态(-2:失败 -1:撤单成功 0:等待成交 1:部分成交 2:完全成交)
                switch (pOrder.data.status)
                {
                    case "-2":
                        {
                            orderData.OrderStatus = OrderStatusType.失败;
                            orderData.CancelTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ff");
                            orderData.StatusMsg = "报单失败";//_StatusMsg;
                        }
                        break;
                    case "-1":
                        {
                            orderData.CancelTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ff");
                            orderData.OrderStatus = OrderStatusType.已撤单;
                            //order.OrderSubmitStatus = (OrderSubmitStatusType)pOrder._OrderSubmitStatus;
                            //order.StatusMsg = pOrder._StatusMsg;
                            orderData.QuantUnfilled = orderData.OrderSize - (decimal)pOrder.data.deal_amount;
                            orderData.QuantFilled = (int)pOrder.data.deal_amount;
                        }
                        break;
                    case "0":
                        orderData.OrderStatus = OrderStatusType.未成交;
                        orderData.QuantUnfilled = orderData.OrderSize - (decimal)pOrder.data.deal_amount;
                        orderData.QuantFilled = (int)pOrder.data.deal_amount;
                        break;
                    case "1":
                        orderData.OrderStatus = OrderStatusType.部分成交;
                        orderData.QuantUnfilled = orderData.OrderSize - (int)pOrder.data.deal_amount;
                        orderData.QuantFilled = (int)pOrder.data.deal_amount;
                        break;
                    case "2":
                        orderData.OrderStatus = OrderStatusType.全部成交;
                        orderData.QuantUnfilled = 0; // orderData.OrderQuant - (int)pOrder.data.deal_amount;
                        orderData.QuantFilled = (int)pOrder.data.deal_amount;
                        break;                    
                }
            }
            orderData.OrderID = pOrder.data.orderid.ToString();
            TQMainModel.dicOrder.AddOrUpdate(orderKey, orderData, (x, y) => orderData);
            Dispatcher.BeginInvoke(new Action(() => { Viewmodel.OrderDataView.Add(orderData); }));
            order = orderData;
            return true;
        }
        
        private void OkexFutures_OnRtnAccountInfoV3(object sender, Dictionary<string, AccountCurrency> bole)
        {//逐仓模式，用AccountCurrencyInfo
            if (bole == null) return;
          /*  {[
  {
    "EOS": {
      "equity": "4937.68731448",
      "liqui_mode": "legacy",
      "maint_margin_ratio": "",
      "margin": "3.29272308",
      "margin_for_unfilled": "0",
      "margin_frozen": "3.29272308",
      "margin_mode": "crossed",
      "margin_ratio": "1499.57563831",
      "realized_pnl": "0.02477112",
      "total_avail_balance": "4936.13348289",
      "unrealized_pnl": "1.52906047"
    }
}
]}*/
            TradingAccountData ta;
            foreach (var e in bole)
            {
                Logger.Log(e, SimpleLogger.LogCategory.Info);
                ta = Viewmodel.TradingAccountDataView.ToList().Find(
                      (x) => e.Key == x.DigitalCurrencyID && Okex.UserID.ToString() == x.InvestorID && "OkexFutures" == x.ExchangeID);
                if (ta != null)
                {
                    if (e.Value.equity < 0.0001m)
                    {
                        Viewmodel.TradingAccountDataView.Remove(ta);
                        continue;
                    }
                    ta.ExchangeID = "OkexFutures";
                    ta.InvestorID = Okex.UserID.ToString();
                    ta.Available = (double)e.Value.total_avail_balance;
                    ta.CloseProfit = (double)e.Value.realized_pnl;
                    ta.PositionProfit =(double)e.Value.unrealized_pnl;
                    ta.UsedMargin = (double)e.Value.margin;
                    ta.Equity = (double)e.Value.equity;
                    ta.Risk = ""; // (v.unrealized_pnl / v.available_qty).ToString();
                    ta.UpdateTime = DateTime.UtcNow;
                    ta.DigitalCurrencyID = e.Key;                             
                }
                else
                {
                    if (e.Value.equity < 0.0001m)
                    {                        
                        continue;
                    }
                    ta = new TradingAccountData()
                    {
                        DigitalCurrencyID = e.Key,
                        InvestorID = Okex.UserID.ToString(),
                        ExchangeID = "OkexFutures",
                        Available = (double)e.Value.total_avail_balance,                       
                        CloseProfit = (double)e.Value.realized_pnl,
                        PositionProfit = (double)e.Value.unrealized_pnl,
                        UsedMargin = (double)e.Value.margin,
                        Equity = (double)e.Value.equity,
                        UpdateTime = DateTime.UtcNow,
                    };                    
                    ta.Risk = "";
                    Dispatcher.Invoke(new Action(() =>
                    {
                        Viewmodel.TradingAccountDataView.Add(ta);
                    }));
                }                                             
            }
        }
        private void Okex_OnRtnAccountInfoSwap(object sender, List<SwapInfo> bole)
        {
            if (bole == null) return;

            TradingAccountData ta;
            foreach (var e in bole)
            {
                Logger.Log( e, LogCategory.Info);
                string aa = e.instrument_id.Substring(0, e.instrument_id.ToList<char>().FindIndex(x => x == '-'));
                ta = Viewmodel.TradingAccountDataView.ToList().Find(
                      (x) => aa == x.DigitalCurrencyID && Okex.UserID.ToString() == x.InvestorID && "OkexSwap" == x.ExchangeID);
                if (ta != null)
                {
                    if (e.equity < 0.0001m)
                        Viewmodel.TradingAccountDataView.Remove(ta);
                    ta.ExchangeID = "OkexSwap";                  
                    ta.Available = (double)e.total_avail_balance; //(double)v.available_qty;                   
                    ta.CloseProfit = (double)e.realized_pnl;
                    ta.PositionProfit = (double)e.unrealized_pnl;
                    ta.UsedMargin = (double)(e.equity - e.total_avail_balance + e.margin_frozen);//margin_frozen;
                    ta.Equity = (double)e.equity;
                    ta.Risk = ""; //v.unrealized_pnl / v.available_qty).ToString();
                    ta.UpdateTime = DateTime.UtcNow;
                    int ind = e.instrument_id.ToList<char>().FindIndex(x => x == '-');
                    ta.DigitalCurrencyID = e.instrument_id.Substring(0,ind);                   
                }
                else
                {
                    if (e.equity < 0.0001m) continue;
                    ta = new TradingAccountData()
                    {                         
                        DigitalCurrencyID = e.instrument_id.Substring(0, e.instrument_id.ToList<char>().FindIndex(x => x == '-')),
                        InvestorID = Okex.UserID.ToString(),
                        ExchangeID = "OkexSwap",
                        Available = (double)e.total_avail_balance,                      
                        CloseProfit = (double)e.realized_pnl,
                        PositionProfit = (double)e.unrealized_pnl,
                        UsedMargin = (double)(e.equity - e.total_avail_balance + e.margin_frozen),
                        Equity = (double)e.equity,
                        UpdateTime = DateTime.Now,
                        Risk = "",                       
                    };
                    Dispatcher.Invoke(new Action(() =>
                    {
                        Viewmodel.TradingAccountDataView.Add(ta);
                    }));
                }                                            
            }           
        }

        private void Websocketbase_OnRtnAccInfo(object sender, AccountInfo e)
        {
            TradingAccountData tad = Viewmodel.TradingAccountDataView[0];
            //tad.ExchangeID = "Okex";
            tad.Available = e.balance +e.profit_real - e.keep_deposit;
            //tad.CurrMargin = e.keep_deposit;
            tad.UsedMargin = e.keep_deposit;
            tad.CloseProfit = e.profit_real;
            tad.InvestorID = e.user_id.ToString();
            //tad.PositionProfit = e.
            //tad.ExchangeMargin = e.keep_deposit;
            tad.Equity = e.balance + e.profit_real;
            tad.Risk = "";           
        }

        private void wb_OnRtnTrade(object sender, ClassTradeResult e)     //报单回报信息，确认报单有效
        {
            Utility.WriteMemLog("order confirmed "+e.data.order_id.ToString());
            Logger.Log(e, SimpleLogger.LogCategory.Info);
            //找对应报单，更新报单信息
        }
        private void wb_OnRtnTradeV3(object sender, List<TradeResultV3> e)     
        {

            this.Dispatcher.Invoke(() =>
            {
                foreach (var item in e)
                {
                    if (Viewmodel.TradeDataView.Count > 200)
                    {
                        var list = Viewmodel.TradeDataView.Take(Viewmodel.TradeDataView.Count - 200);
                        foreach (var ditem in list)
                        {
                            Viewmodel.TradeDataView.Remove(ditem);
                        }
                    }
                    Viewmodel.TradeDataView.Add(new TradeData()
                    {
                        OrderPrice = (double)(item.price*item.qty),
                        AvgPrice = item.price,
                        InstrumentID = item.instrument_id,
                        ExchangeID="Okex",
                        OrderTime = item.timestamp.ToString("yyyy-MM-dd HH:mm:ss.ff"),
                        UpdateTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ff"),                       
                        TradeID = item. trade_id,
                        Direction = item.side == "buy" ? TradeDirection.Long : TradeDirection.Short,
                        OrderStatus= OrderStatusType.全部成交,
                    });
                }

                var groupbys = Viewmodel.TradeDataView.GroupBy(x => x.InstrumentID);
                TradeDataSummary sitem = null;
                foreach (var item in groupbys)
                {
                    sitem = Viewmodel.TradeDataSummaryView.FirstOrDefault(x => x.InstrumentID == item.Key);
                    if (sitem == null)
                    {
                        sitem = new TradeDataSummary();
                    }
                    Viewmodel.TradeDataSummaryView.Add(sitem);
                    sitem.AvgPrice = (double)item.Sum(x => x.AvgPrice);
                    sitem.InstrumentID = item.Key;
                    sitem.ExchangeInstID = item.Key;
                    sitem.Volume = (int)item.Sum(x => x.Quant);
                    sitem.Direction = item.Sum(x => x.Direction == TradeDirection.Long ? x.OrderPrice : -x.OrderPrice) > 0 ? TradeDirection.Long : TradeDirection.Short;
                    sitem.TradingDay = (e.LastOrDefault())?.timestamp.ToString("MM-dd");
                }
            });
            Logger.Log(e, SimpleLogger.LogCategory.Info);
            //找对应报单，更新报单信息
        }
        private void wb_OnRtnOrdersResultV3(object sender, List<OrderResultV3> e)     
        {

            this.Dispatcher.InvokeAsync(() =>
            {                
                foreach (var item in e)
                {                   
                    OrderData orderData = new OrderData()
                    {
                        OrderID = item.order_id,
                        LeverRate = item.leverage,
                        AvgPrice = item.price_avg,                        
                        CreateTime = item.timestamp.ToString("yyyy-MM-dd HH:mm:ss.ff"),
                        UpdateTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ff"),
                        ExchangeID = item.instrument_id.Contains("SWAP")?EnuExchangeID.OkexSwap:EnuExchangeID.OkexFutures,
                        Direction = item.type == "1"|| item.type == "4" ? TradeDirection.Long : TradeDirection.Short,
                        Offset = (item.type == "1" || item.type == "2") ? OffsetType.开仓 : OffsetType.平仓,
                        InvestorID = item.client_oid,
                        InstrumentID=item.instrument_id,
                        OrderPrice = item.price,
                        OrderPriceType = item.order_type=="0"? OrderPriceType.限价:OrderPriceType.市价,
                        OrderSize = item.size,
                        OrderStatus = item.status == "0" ? OrderStatusType.未成交 : item.status == "1" ? OrderStatusType.部分成交 :
                                item.status == "2" ? OrderStatusType.全部成交 : item.status == "3" ? OrderStatusType.下单中 : item.status == "4" ? OrderStatusType.已撤单
                                : item.status == "-2" ? OrderStatusType.未成交 : item.status == "-1" ? OrderStatusType.已撤单: OrderStatusType.未知,
                      
                        QuantFilled= item.filled_qty,
                        QuantUnfilled=item.size-item.filled_qty,
                    };
                    var dd = Viewmodel.OrderDataView.ToList();
                    var d1 = dd.FindIndex(m => m.OrderID == orderData.OrderID);
                    var dorder=Viewmodel.OrderDataView.ToList().Find((x) => x.OrderID == orderData.OrderID);
                    if (dorder != null && dorder.OrderID!= null)
                    {
                        Viewmodel.OrderDataView.Remove(dorder);
                    }
                    Viewmodel.OrderDataView.Add(orderData);
                    //成交明细
                    int ii = -1;
                    if ((ii = Viewmodel.TradeDataView.ToList().FindIndex(x => x.TradeID == orderData.OrderID 
                    && x.OrderStatus==OrderStatusType.未成交)) > -1)
                        Viewmodel.TradeDataView.RemoveAt(ii);
                    Viewmodel.TradeDataView.Add(new TradeData() {
                        AvgPrice = orderData.AvgPrice,
                        Direction = orderData.Direction,
                        InstrumentID = orderData.InstrumentID,
                        Offset = orderData.Offset,
                        QuantTraded = orderData.QuantFilled,
                        TradeID = orderData.OrderID,
                        OrderTime = orderData.CreateTime,
                        UpdateTime= orderData.UpdateTime,
                        Ordertype = orderData.OrderPriceType,
                        OrderPrice = (double)orderData.OrderPrice,
                        OrderStatus = orderData.OrderStatus,
                        ExchangeID = orderData.ExchangeID.ToString(),
                        Quant = orderData.QuantFilled+orderData.QuantUnfilled,                       
                    });
                    Logger.Log(orderData, SimpleLogger.LogCategory.Info);
                }
            });
        }

        private void OKWb_OnRspTrade(object sender, FutureTrade e)    //成交详细信息,
        {
            if (e == null) return;
            long id;
            if (Okex.UserID is null)
            {       long.TryParse(e.data.user_id, out id);
                Okex.UserID = id.ToString();
            }
            FutureTradeData ftd = e.data;
            //status -1 = cancelled, 0 = unfilled, 1 = partially filled, 2 = fully filled, 4 = cancel request in process             
            //TradeField allreadyexists;
            //处理报单，增加/修改状态                       
            TradeData trade;// = new TradeField();
            string key = e.data.user_id + e.data.orderid;
            if (TQMainModel.dicTradeData.TryGetValue(key, out trade))
            {
                trade.Quant = (decimal)ftd.amount;
                trade.OrderPrice = ftd.price;
                trade.QuantTraded = (decimal)ftd.deal_amount;
                trade.Commission = ftd.fee;
                trade.AvgPrice = (decimal)ftd.price_avg;
                trade.OrderTime = ftd.create_date_str;
                trade.UpdateTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ff");
                var s = ftd.system_type;

                switch (ftd.status)
                {
                    case "-1":
                        trade.OrderStatus = OrderStatusType.已撤单;
                        break;
                    case "0":
                        trade.OrderStatus = OrderStatusType.未成交;
                        break;
                    case "1":
                        trade.OrderStatus = OrderStatusType.部分成交;
                        break;
                    case "2":
                        trade.OrderStatus = OrderStatusType.全部成交;
                        break;
                    case "4":
                        trade.OrderStatus = OrderStatusType.撤单中;
                        break;
                }
                return;
            }
            else
            {
                trade = new TradeData();                
                trade.InvestorID = e.data.user_id;
                trade.TradeID = e.data.orderid.ToString();
                trade.ExchangeID = ExchaneID.Okex.ToString();
                switch (ftd.status)
                {
                    case "-1":
                        trade.OrderStatus = OrderStatusType.已撤单;
                        break;
                    case "0":
                        trade.OrderStatus = OrderStatusType.未成交;
                        break;
                    case "1":
                        trade.OrderStatus = OrderStatusType.部分成交;
                        break;
                    case "2":
                        trade.OrderStatus = OrderStatusType.全部成交;
                        break;
                    case "4":
                        trade.OrderStatus = OrderStatusType.撤单中;
                        break;
                }
                switch (ftd.type)
                {
                    //Okwebsocket    order type 1: open long, 2: open short, 3: close long, 4: close short
                    case 1:
                        trade.Direction = TradeDirection.Long;
                        trade.Offset= OffsetType.开仓;
                        break;
                    case 2:
                        trade.Offset = OffsetType.开仓;
                        trade.Direction = TradeDirection.Short;
                        break;
                    case 3:
                        trade.Direction = TradeDirection.Short;
                        trade.Offset = OffsetType.平仓;
                        break;
                    case 4:
                        trade.Direction = TradeDirection.Long;
                        trade.Offset = OffsetType.平仓;
                        break;
                }
            }            
            trade.Quant = (int)ftd.amount;
            trade.QuantTraded = (int)ftd.deal_amount;
            trade.InstrumentID = ftd.contract_name;
            trade.OrderPrice = ftd.price;
            trade.AvgPrice = (decimal)ftd.price_avg;
            trade.OrderTime = ftd.create_date_str;
            trade.UpdateTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ff");
            Utility.WriteMemLog("tradedate : " + trade.OrderTime);  
            
            Utility.WriteMemLog("New Trade: "+trade.InstrumentID+" "+trade.OrderPrice+" "+trade.Direction);
            TQMainModel.dicTradeData.AddOrUpdate(key, trade, (x, y) => trade);
            Dispatcher.BeginInvoke(new Action(() => Viewmodel.TradeDataView.Add(trade)));                                       
        }
        //#region 成交信息的增加，使用异步方式更新主线程
        private bool TradeDataTryAdd(TradeData pTrade)
        {
            if (pTrade == null) return false;
            TradeData allreadyexists;
            //string key = pTrade.ExchangeID + pTrade.orderSysID;   //分笔成交的情况不行
            string key = pTrade.InvestorID + pTrade.TradeID;//pTrade.InvestorID + pTrade.ExchangeID + pTrade.TradeID; //键值，唯一地确定一个成交
            if (TQMainModel.dicTradeData.TryGetValue(key, out allreadyexists))
                return false;
            else
            {
                Dispatcher.BeginInvoke(new Action(() => Viewmodel.TradeDataView.Add(pTrade)));
                return TQMainModel.dicTradeData.TryAdd(key, pTrade);
            }
        }

        public void ReqSubscription()
        {                        
            
        }
        private void InitUserDirectoryAndConfiguration(string investorId)
        {           
            if (Trader.Configuration == null || Trader.Configuration.Investor.ID != investorId)
            {
                Trader.Configuration = Trader.Load("Okex");
            }           
        }        
        private void InitializeMainDataGridMapping()
        {
            dicMainDataGridMapping.Add(DataGridType.Account, new DataGridMapping()
            {
                DataGridType = DataGridType.Account,
                TQMainDataGrid = AccountDataGrid,
                ColumnSettingList = Trader.Configuration.AccountDataGrid
            });
            dicMainDataGridMapping.Add(DataGridType.MarketData, new DataGridMapping()
            {
                DataGridType = DataGridType.MarketData,
                TQMainDataGrid = MarketDataGrid,
                ColumnSettingList = Trader.Configuration.MarketDataGrid
            });
            dicMainDataGridMapping.Add(DataGridType.Instrument, new DataGridMapping()
            {
                DataGridType = DataGridType.Instrument,
                TQMainDataGrid = InstrumentDataGrid,
                ColumnSettingList = Trader.Configuration.InstrumentDataGrid
            });
            dicMainDataGridMapping.Add(DataGridType.UnsettledOrders, new DataGridMapping()
            {
                DataGridType = DataGridType.UnsettledOrders,
                TQMainDataGrid = UnsettledOrdersGrid,
                ColumnSettingList = Trader.Configuration.UnsettledOrdersGrid
            });
            dicMainDataGridMapping.Add(DataGridType.TodayOrders, new DataGridMapping()
            {
                DataGridType = DataGridType.TodayOrders,
                TQMainDataGrid = TodayOrdersGrid,
                ColumnSettingList = Trader.Configuration.TodayOrderGrid
            });
            
            dicMainDataGridMapping.Add(DataGridType.SettledOrders, new DataGridMapping()
            {
                DataGridType = DataGridType.SettledOrders,
                TQMainDataGrid = SettledOrdersGrid,
                ColumnSettingList = Trader.Configuration.SettledOrdersGrid
            });
            dicMainDataGridMapping.Add(DataGridType.CanceledOrders, new DataGridMapping()
            {
                DataGridType = DataGridType.CanceledOrders,
                TQMainDataGrid = CanceledOrdersGrid,
                ColumnSettingList = Trader.Configuration.CanceledOrdersGrid
            });

            dicMainDataGridMapping.Add(DataGridType.PositionSummary, new DataGridMapping()
            {
                DataGridType = DataGridType.PositionSummary,
                TQMainDataGrid = PositionSummaryGrid,
                ColumnSettingList = Trader.Configuration.PositionSummaryGrid
            });
                       
            dicMainDataGridMapping.Add(DataGridType.TradeDetails, new DataGridMapping()
            {
                DataGridType = DataGridType.TradeDetails,
                TQMainDataGrid = TradeRecordDetailsGrid,
                ColumnSettingList = Trader.Configuration.TradeDetailsGrid
            });            
        }       
       
        /// <summary>
        /// dicCustomProduct存放所有已定义的自定义品种
        /// </summary>
        private void InitCustomProductList()
        {
            if (File.Exists(Trader.ExtCfgFile))
            {
                Trader.LoadExtConfig();
                if (Trader.ExtConfig != null)
                {
                    try
                    {                        
                        foreach (XmlCombo xc in Trader.ExtConfig.Combos)
                        {
                            ComboMarketData cust = new ComboMarketData(xc.InstrumentID, xc.InstrumentName)
                            {
                                PriceTick = xc.PriceTick
                            };
                            TQMainModel.dicAllCustomProductList.TryAdd(xc.InstrumentID, cust);
                        }
                    }
                    catch (Exception ec)
                    {

                    }
                }
            }
            else
            {
                Trader.ExtConfig = new ExtensionConfiguration();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            SaveConfiguration();
            if (Okex != null)
                Okex.Stop();
            if (Bitmex != null)
                Bitmex.clientWebSocketconnector.Dispose();                               
                                            
            Utility.WriteMemLogToLogFile(TQMainModel.MemLog);            
            Thread.Sleep(300);
        }

        #region position handling
        /// <summary>
        /// 设置Tab的显示属性
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabPosition_GotFocus(object sender, RoutedEventArgs e)
        {
            var tabSender = (TabItem)sender;
            switch (tabSender.Name)
            {
                case "tabManual":
                case "tabStrategy":
                    spPositionOper.Visibility = Visibility.Visible;
                    break;
                case "tabSummary":
                case "tabPositionDetail":
                    spPositionOper.Visibility = Visibility.Hidden;
                    break;
            }
        }
        private void RdBtnPst_Checked(object sender, RoutedEventArgs e)
        {
            //btnOppositePriceClose.Visibility = Visibility.Visible;
            btnMarketPriceClose.Visibility = Visibility.Visible;
            btnMarketPriceReverse.Visibility = Visibility.Visible;

            this.PositionSummaryGrid.Visibility = Visibility.Visible;
            //this.PositionDetailsGrid.Visibility = Visibility.Visible;
            //this.ComboPositionGrid.Visibility = Visibility.Collapsed;

        }

        private void RdBtnPstDtl_Checked(object sender, RoutedEventArgs e)
        {
            //btnOppositePriceClose.Visibility = Visibility.Hidden;
            btnMarketPriceClose.Visibility = Visibility.Hidden;
            btnMarketPriceReverse.Visibility = Visibility.Hidden;

            ///this.PositionDetailsGrid.Visibility = Visibility.Visible;
            this.PositionSummaryGrid.Visibility = Visibility.Collapsed;
            //this.ComboPositionGrid.Visibility = Visibility.Collapsed;
        }

        private void RdBtnCbPst_Checked(object sender, RoutedEventArgs e)
        {           
            //this.PositionDetailsGrid.Visibility = Visibility.Collapsed;
            this.PositionSummaryGrid.Visibility = Visibility.Collapsed;
        }

        private void RdBtnDtl_Checked(object sender, RoutedEventArgs e)
        {
            this.TradeRecordDetailsGrid.Visibility = Visibility.Visible;
            //this.TradeRecordSummaryGrid.Visibility = Visibility.Collapsed;        
        }

        private void RdBtnSum_Checked(object sender, RoutedEventArgs e)
        {
            //this.TradeRecordSummaryGrid.Visibility = Visibility.Visible;
            this.TradeRecordDetailsGrid.Visibility = Visibility.Collapsed;          
        }
        //移仓
        private void PositionMovingForward_Click(object sender, RoutedEventArgs e)
        {
            MajorMarketData md;
            //if (PositionSummaryGrid.SelectedIndex >= 0)

            OrderData closeorder = new OrderData();
            OrderData openorder = new OrderData();
            PositionDataSummary pos;
            TQMainModel.dicPositionSummary.TryGetValue("btc_this_week", out pos);// PositionSummaryGrid.SelectedItem as PositionDataSummary;
            Task.Run(() =>
            {
                openorder.ExchangeID = closeorder.ExchangeID = (EnuExchangeID)Enum.Parse(typeof(EnuExchangeID), pos.ExchangeID.ToString());
                closeorder.InstrumentID = pos.InstrumentID;
                openorder.InstrumentID = pos.InstrumentID.Substring(0,4) + "next_week";
                if (pos.ShortPosition > 0)
                {
                    openorder.Direction = TradeDirection.Short;
                    closeorder.Direction = TradeDirection.Long;
                    openorder.OrderSize = pos.ShortPosition;
                    closeorder.OrderSize = pos.ShortPosition;
                }
                if (pos.LongPosition > 0)
                {
                    openorder.Direction = TradeDirection.Long;
                    closeorder.Direction = TradeDirection.Short;
                    openorder.OrderSize = pos.LongPosition;
                    closeorder.OrderSize = pos.LongPosition;
                }
                openorder.Offset = OffsetType.开仓;
                closeorder.Offset = OffsetType.平仓;
                closeorder.Direction = pos.ShortPosition > 0 ? TradeDirection.Long : TradeDirection.Short;
                int l = (int)pos.LeverRate;
                openorder.LeverRate = closeorder.LeverRate =(l == 0) ? 20 : pos.LeverRate;
                openorder.OrderPriceType = closeorder.OrderPriceType = OrderPriceType.限价;

                if (TQMainModel.dicMajorMarketData.TryGetValue(openorder.InstrumentID, out md))
                {
                    openorder.OrderPrice = openorder.Direction == TradeDirection.Long ? md.AskPrice1 * (decimal)1.001 : md.BidPrice1 * (decimal)0.999;
                    openorder.OrderSize = Math.Min((openorder.Direction == TradeDirection.Long ? md.AskSize1 : md.BidSize1), openorder.OrderSize);
                }
                else
                {
                    throw new Exception("no market data");
                }
                if (TQMainModel.dicMajorMarketData.TryGetValue(closeorder.InstrumentID, out md))
                {
                    closeorder.OrderPrice = closeorder.Direction == TradeDirection.Long ? md.AskPrice1 * (decimal)1.001 : md.BidPrice1 * (decimal)0.999;
                    closeorder.OrderSize = Math.Min((closeorder.Direction == TradeDirection.Long ? md.AskSize1 : md.BidSize1), closeorder.OrderSize);
                }
                else
                {
                    throw new Exception("no market data");
                }
                openorder.OrderSize = closeorder.OrderSize = Math.Min(openorder.OrderSize, closeorder.OrderSize);

                Okex.SendOrderRest(closeorder);
                Okex.SendOrderRest(openorder);
            });

        }
        //市价平仓
        private void btnMarketPriceClose_Click(object sender, RoutedEventArgs e)
        {
           
        }
        //市价反手
        private void btnMarketPriceReverse_Click(object sender, RoutedEventArgs e)
        {
            
        }
        //移仓
        private void miTransferPosition_Click(object sender, RoutedEventArgs e)
        {
            /*
            if (PositionSummaryGrid.SelectedIndex < 0)
            {
                PositionSummaryGrid.SelectedIndex = 0;
            }
            PositionDataSummary posSum = PositionSummaryGrid.SelectedItem as PositionDataSummary;

            //1.弹出窗口选择移仓的目标合约
            ExchangePositionWindow tranPosWin = new ExchangePositionWindow(posSum.InstrumentID);
            tranPosWin.ShowDialog();
            if (string.IsNullOrEmpty(tranPosWin.DesInstrument))
            {
                return;
            }
            string desInstrument = tranPosWin.DesInstrument;
            //2.检查目标合约是否已经订阅
            MajorMarketData md;

            bool isSHFE = (posSum.ExchangeName == Utility.GetExchangeName(posSum.InstrumentID)) ? true : false;

            if (!TQMainVM.dicMajorMarketData.TryGetValue(desInstrument, out md))
            {
                if (isSHFE)
                {
                    MessageBox.Show("该合约的移仓目标合约没有订阅行情，请订阅行情后再进行移仓");
                    return;
                }
            }

            //3.先市价平当前仓
            ClosePositionWithMarketPrice(posSum);

            //4.再市价开移仓的目标合约
            TradeDirection dir = (posSum.Direction == TradeDirection.空) ? TradeDirection.空 : TradeDirection.多;
            var orderboard = md;
            orderboard.OrderBoardPricingMode = PricingMode.Market;
            orderboard.OrderBoardOrderMode = OrderMode.Open;
            // = HedgeType.投机;//need to do
            orderboard.Exchange = Utility.GetExchangeID(posSum.ExchangeName);
            T.ReqOrderInsert(orderboard, dir, posSum.Position);

            #region old method
            //DirectionType direction;
            //double price;
            //if (posSum.Direction == DirectionType.空)
            //{
            //    direction = DirectionType.多;
            //    price = isSHFE ? md.UpperLimitPrice : 0;
            //}
            //else
            //{
            //    direction = DirectionType.空;
            //    price = isSHFE ? md.LowerLimitPrice : 0;
            //}
            ////如有挂单，先撤单,再行移仓
            //var unsettledOrders = OrderDataView.Where(x => x.InstrumentID == posSum.InstrumentID &&
            //    x.Direction == direction && (x.Offset == OffsetType.平仓 || x.Offset == OffsetType.平今 || x.Offset == OffsetType.平昨)).ToList();
            //if (unsettledOrders != null && unsettledOrders.Count > 0)
            //{
            //    Task.Run(() =>
            //    {
            //        //先撤单
            //        int num = 0;
            //        foreach (var order in unsettledOrders)
            //        {
            //            num++;
            //            T.ReqOrderAction(order.FrontID, order.SessionID, order.OrderRef, order.InstrumentID);
            //        }
            //        string key = posSum.InstrumentID + direction.ToString();
            //        T.dicUnsettledCloseOrderNum.AddOrUpdate(key, num, (k, v) => num);
            //        while (true)
            //        {
            //            num = -1;
            //            if (!T.dicUnsettledCloseOrderNum.TryGetValue(key, out num))
            //            {
            //                //撤单成功后，再进行平仓操作
            //                if (isSHFE)
            //                {
            //                    if (posSum.YdPosition > 0)
            //                    {
            //                        T.ReqOrderInsert(posSum.InstrumentID, direction == DirectionType.多 ? TradeDirection.多 : TradeDirection.空, OffsetType.平仓, price, posSum.YdPosition, LimitMarketFakFok.Limit);
            //                    }
            //                    if (posSum.TodayPosition > 0)
            //                    {
            //                        T.ReqOrderInsert(posSum.InstrumentID, direction == DirectionType.多 ? TradeDirection.多 : TradeDirection.空, OffsetType.平仓, price, posSum.TodayPosition, LimitMarketFakFok.Limit);
            //                    }
            //                }
            //                else
            //                {
            //                    T.ReqOrderInsert(posSum.InstrumentID, direction == DirectionType.多 ? TradeDirection.多 : TradeDirection.空, OffsetType.平仓, price, posSum.Position, LimitMarketFakFok.Limit);
            //                }
            //                break;
            //            }
            //            Thread.Sleep(1000);
            //        }
            //        while (true)
            //        {
            //            //平仓成功后，最后进行移仓操作 //todo:huangrongyu
            //            if (true)
            //            {
            //                direction = posSum.Direction;
            //                if (isSHFE)
            //                {
            //                    T.ReqOrderInsert(desInstrument, direction == DirectionType.多 ? TradeDirection.多 : TradeDirection.空, OffsetType.开仓, price, posSum.Position, LimitMarketFakFok.Market);
            //                }
            //                else
            //                {
            //                    T.ReqOrderInsert(desInstrument, direction == DirectionType.多 ? TradeDirection.多 : TradeDirection.空, OffsetType.开仓, 0, posSum.Position, LimitMarketFakFok.Market, HedgeType.投机 );
            //                }
            //            }
            //            Thread.Sleep(1000);
            //        }
            //    });
            //}
            //else
            //{
            //    Task.Run(() =>
            //    {
            //        if (isSHFE)
            //        {
            //            if (posSum.YdPosition > 0)
            //            {
            //                T.ReqOrderInsert(posSum.InstrumentID, direction==DirectionType.多?TradeDirection.多:TradeDirection.空, OffsetType.平仓, price, posSum.YdPosition,LimitMarketFakFok.Limit);
            //            }
            //            if (posSum.TodayPosition > 0)
            //            {
            //                T.ReqOrderInsert(posSum.InstrumentID, direction == DirectionType.多 ? TradeDirection.多 : TradeDirection.空, OffsetType.平仓, price, posSum.TodayPosition, LimitMarketFakFok.Limit);
            //            }
            //        }
            //        else
            //        {
            //            T.ReqOrderInsert(posSum.InstrumentID, direction == DirectionType.多 ? TradeDirection.多 : TradeDirection.空, OffsetType.平仓, 0, posSum.Position, LimitMarketFakFok.Market, HedgeType.投机);
            //        }
            //        while (true)
            //        {
            //            //平仓成功后，最后进行移仓操作 //todo:huangrongyu
            //            if (true)
            //            {
            //                direction = posSum.Direction;
            //                if (isSHFE)
            //                {
            //                    T.ReqOrderInsert(desInstrument, direction==DirectionType.多?TradeDirection.多:TradeDirection.空, OffsetType.开仓, price, posSum.Position, LimitMarketFakFok.Limit);
            //                }
            //                else
            //                {
            //                    T.ReqOrderInsert(desInstrument, direction == DirectionType.多 ? TradeDirection.多 : TradeDirection.空, OffsetType.开仓, 0, posSum.Position, LimitMarketFakFok.Market, HedgeType.投机 );
            //                }
            //            }
            //            Thread.Sleep(1000);
            //        }
            //    });
            //}
            #endregion
*/
        }

        //清仓
        private void miCloseAll_Click(object sender, RoutedEventArgs e)
        {
            if (Viewmodel.PositionDataSummaryView.Count < 1)
            {
                return;
            }
            var posSumList = Viewmodel.PositionDataSummaryView.ToList();

            //ForceCloseAllPositions();
        }

        private void DataGrid_CurrentCellChanged(object sender, EventArgs e)
        {
            string sInstrumentID = null;

            switch (((DataGrid)sender).Name)
            {
                case "PositionSummaryGrid":

                    try {
                        sInstrumentID = (PositionSummaryGrid.CurrentCell.Item as PositionDataSummary).InstrumentID;
                    }
                    catch { }

                    break;                
            }

            if (!string.IsNullOrEmpty(sInstrumentID))
            {
                MajorMarketData m;
                TQMainModel.dicMajorMarketData.TryGetValue(sInstrumentID, out m);
                if (m != null)
                {
                    Viewmodel.CurrentMarketData = m;
                }
            }
        }
        #endregion

       

        private void MarketDataListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MarketDataGrid.SelectedIndex = MarketDataListBox.SelectedIndex;
        }

        #region 行情、持仓、成交等数据列显示条目的设置
        private void AccountDataGridColumnSet_Click(object sender, RoutedEventArgs e)
        {
            SetDataGridColumn(SettingsType.TradingAccountColumn);
        }

        private void MarketDataColumnSet_Click(object sender, RoutedEventArgs e)
        {
            SetDataGridColumn(SettingsType.MarketColumn);
        }

        private void InstrumentListColumnSet_Click(object sender, RoutedEventArgs e)
        {
            SetDataGridColumn(SettingsType.InstrumentColumn);
        }

        private void TodayOrdersColumnSet_Click(object sender, RoutedEventArgs e)
        {
            SetDataGridColumn(SettingsType.OrderColumn);

        }

        private void UnsettledOrderColumnSet_Click(object sender, RoutedEventArgs e)
        {
            SetDataGridColumn(SettingsType.UnsettledOrderColumn);

        }
       

        private void SettledOrderColumnSet_Click(object sender, RoutedEventArgs e)
        {
            SetDataGridColumn(SettingsType.SettledOrderColumn);

        }
        private void GeneralPositionGridColumnSet_Click(object sender, RoutedEventArgs e)
        {
            SetDataGridColumn(SettingsType.GeneralPositionColumn);
        }
        private void PositionSummaryGridColumnSet_Click(object sender, RoutedEventArgs e)
        {
            SetDataGridColumn(SettingsType.PositionSummaryColumn);
        }

        private void PositionDetailsGridColumnSet_Click(object sender, RoutedEventArgs e)
        {
            SetDataGridColumn(SettingsType.PositionColumn);
        }

        private void ComboPositionGridColumnSet_Click(object sender, RoutedEventArgs e)
        {
            SetDataGridColumn(SettingsType.ComboPositionColumn);
        }

        private void TradeRecordSummaryColumnSet_Click(object sender, RoutedEventArgs e)
        {
            SetDataGridColumn(SettingsType.TradeSummaryColumn);

        }

        private void TradeRecordDetailsColumnSet_Click(object sender, RoutedEventArgs e)
        {
            MultipleSettingsWindow settingWin = new MultipleSettingsWindow(SettingsType.TradeColumn, this);
            settingWin.ShowDialog();
        }
        #endregion

        //实现点击“报价表”时显示指定价功能（动态跟盘价还未实现）

        #region 账户 系统菜单
        private void EnqTradingAccount_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (T == null || !T.IsLogin)
                {
                    TQMainModel.MemLog.Enqueue(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ff") + " system has logout");
                    Ring("指令单错误");
                    Logger.Log("账户登出, 请重新登入!", LogCategory.Info);
                    return;
                }
                else
                {
                    AccountInfoWindow accountWin = new AccountInfoWindow();
                    accountWin.ShowDialog();
                }
                //T.ReqQryAccount();
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                Logger.Log(ex.Message, LogCategory.Info);
            }
        }

        private void EnqControlCenter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                InvestorServiceWindow investorServiceWin = new InvestorServiceWindow();
                investorServiceWin.ShowDialog();
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message, LogCategory.Info);
            }
        }

        private void PasswordUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                UserPwdUpdateWindow userPwdUpdate = new UserPwdUpdateWindow(this);
                userPwdUpdate.ShowDialog();
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message,LogCategory.Info);
            }
        }

        private void Relogin_Click(object sender, RoutedEventArgs e)
        {
            if (Bitmex.clientWebSocketconnector.State != System.Net.WebSockets.WebSocketState.Open && Bitmex.clientWebSocketconnector.State !=System.Net.WebSockets.WebSocketState.Connecting)
           {
                Bitmex.clientWebSocketconnector.Start();
                //bitMex.clientWebSocketconnector.Start().Wait();
            }
            ReqSubscription();

        }
        
        public void InitTraderByConfig()
        {
            #region open configuration.xml and init Trader's Config
            if (File.Exists(Trader.InstrumentInfoCfgFile))
            {
                try
                {
                    instrumentCfg = TQXmlHelper.XmlDeserializeFromFile<InstrumentInfoConfiguration>(Trader.InstrumentInfoCfgFile, Encoding.UTF8);
                    foreach (var v in instrumentCfg.ListInstrument)
                    {
                        InstrumentData id;
                        if (!TQMainModel.dicInstrumentData.TryGetValue(v.InstrumentID, out id))
                            TQMainModel.dicInstrumentData.Add(v.InstrumentID, new InstrumentData()
                            {
                                InstrumentID = v.InstrumentID,
                                Name = v.Name,
                                PriceTick = v.PriceTick,
                                ExchangeID = v.ExchangeID,
                                ContractValue = v.ContractValue
                            });
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            else throw new FileNotFoundException(Trader.InstrumentInfoCfgFile);

            if (Trader.Configuration.InstrumentGroupList != null && Trader.Configuration.InstrumentGroupList.Count > 0)
            {            
                foreach (var v in Trader.Configuration.InstrumentGroupList)
                {
                    foreach (var it in v.InstrumentDataList)
                    {
                        InstrumentData ins;
                        if (TQMainModel.dicInstrumentData.TryGetValue(it.InstrumentID, out ins))
                            it.ExchangeID = ins.ExchangeID;
                    }
                    dicInstrumentIDsGroup.TryAdd(v.Name, v);
                }
                foreach (var v in dicInstrumentIDsGroup[Trader.Configuration.DefaultInstrumentIDGroup].InstrumentDataList)
                {
                    Trader.CurrInstrumentDict[v.InstrumentID] = v;
                }
                InitCustomProductList();
                //generate SubscribedInstrumentIDs
                ComboMarketData cust;
                MajorMarketData md;
                try
                {
                    foreach (var v in Trader.CurrInstrumentDict)
                    {
                        if (v.Key.Length < 15)   //basic contract
                        {
                            if (!TQMainModel.SubscribedInstrumentIDs.Keys.Contains(v.Key))
                                TQMainModel.SubscribedInstrumentIDs.Add(v.Key, v.Value);
                            md = new MajorMarketData(v.Key) { IsCombo = false, ExchangeID = v.Value.ExchangeID };
                            TQMainModel.dicMajorMarketData.AddOrUpdate(v.Key, md, (k, s) => s);
                        }
                        else   //customized product
                        {
                            if (!TQMainModel.dicAllCustomProductList.TryGetValue(v.Key, out cust))  //dicCustomProduct is initialized
                            {
                                cust = new ComboMarketData(v.Key) { IsCombo = true };                                
                            }
                            TQMainModel.dicCustomProduct.TryAdd(cust.InstrumentID, cust);
                            InstrumentData id;
                            for (int i = 0; i < cust.ItemList.Count; i++)   //add more contract that arenot included in customized instrument
                            {
                                var sd = TQMainModel.dicInstrumentData.TryGetValue(cust.ItemList[i].InstrumentID, out id);
                                if (!sd)
                                    throw new Exception("unable to find this InstrumentID in TQMainModel.dicInstrumentID");
                                if (!TQMainModel.SubscribedInstrumentIDs.Keys.Contains(cust.ItemList[i].InstrumentID))     //update subscripted instrument market data list
                                {
                                    TQMainModel.SubscribedInstrumentIDs.Add(cust.ItemList[i].InstrumentID, new InstrumentData()
                                    {
                                        InstrumentID = cust.ItemList[i].InstrumentID,
                                        ExchangeID = sd ? id.ExchangeID : id.InstrumentID.ToUpper().Contains("SWAP")?EnuExchangeID.OkexSwap:EnuExchangeID.OkexFutures,
                                    });
                                    md = new MajorMarketData(cust.ItemList[i].InstrumentID) { PriceTick = id.PriceTick, IsCombo = false, ExchangeID = id.ExchangeID };
                                    TQMainModel.dicMajorMarketData.TryAdd(cust.ItemList[i].InstrumentID, md);
                                }                                                               
                            }
                            TQMainModel.dicMajorMarketData.TryAdd(v.Key, cust);
                        }
                    }
                }
                catch (Exception cd)
                {

                }
            }                        
            foreach (var ar in Trader.CurrInstrumentDict)           
            {
                Viewmodel.MajorMarketDataView.Add(TQMainModel.dicMajorMarketData[ar.Key]);                
            }
            
            Trader.DefaultInstrumentQuant = new Dictionary<string, DefaultQuantSet>();
           
            if (Trader.Configuration.DefaultQuantSettings != null && Trader.Configuration.DefaultQuantSettings.Count > 0)
            {
                foreach (var v in Trader.Configuration.DefaultQuantSettings)
                    Trader.DefaultInstrumentQuant.Add(v.InstrumentID, v);
            }
            if (Trader.Configuration.ProductList != null && Trader.Configuration.ProductList.Count > 0)
            {
                TQMainModel.dicProductIDName.Clear();
                foreach (var item in Trader.Configuration.ProductList)
                {
                    TQMainModel.dicProductIDName.Add(item.ProductID, item.ProductName);
                }
            }
            #endregion
        }
        private void SwitchAccount_Click(object sender, RoutedEventArgs e)
        {
            try
            {               
                TQMainModel.dicMajorMarketData.Clear();
                Task t = Task.Run(() =>
                {
                   
                   
                });
                t.Wait();
                string msg = string.Format("TradingAccount({0}) has logout", Trader.Configuration.Investor.ID);
                TQMainModel.MemLog.Enqueue(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ff") + msg);
                ShowMsg(msg);
                //switch user
                LoginWindow = new LoginWindow(this);
                if (LoginWindow.ShowDialog() != true)
                {
                    Relogin_Click(sender, e);                    
                    return;
                }
                msg = string.Format("user has switched to {0}", Trader.Configuration.Investor.ID);
                TQMainModel.MemLog.Enqueue("{0}\t" + msg);
                ShowMsg(msg);
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message, SimpleLogger.LogCategory.Info);
            }
        }

        private void Exit(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #endregion

        #region Trade Board

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            Viewmodel.CurrentMarketData.ResetOrderBoard();
        }

        private void OrderBtn_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).Name == "LongBtn" || ((Button)sender).Name == "btnAsk")
                Viewmodel.CurrentMarketData.OrderBoardTradeDirection = TradeDirection.Long;
            else
                Viewmodel.CurrentMarketData.OrderBoardTradeDirection = TradeDirection.Short;

            //another thread to run dialog
            if (Trader.Configuration.ConfirmOrder)
            {
                var orderdetailsinfo = (Viewmodel.CurrentMarketData.OrderBoardTradeDirection == TradeDirection.Long ? "Long " : "Short ")
                                   + " (" + Viewmodel.CurrentMarketData.InstrumentID + ") " +
                                   Viewmodel.CurrentMarketData.OrderBoardQuant.ToString() + "张?";
                Task.Run(() =>
                {
                    var mr = System.Windows.MessageBox.Show(orderdetailsinfo, "Confirm Order", MessageBoxButton.YesNo);
                    if (mr != MessageBoxResult.Yes)
                        return;
                    Dispatcher.BeginInvoke(DispatcherPriority.Send, new Action(() =>
                    {
                        if (Viewmodel.CurrentMarketData.IsCombo)
                        {
                            PlaceComboOrderAsync();
                        }
                        else PlaceOrder();
                    }));
                });
            }
            else if (Viewmodel.CurrentMarketData.IsCombo)
            {
                PlaceComboOrderAsync();                
            }
            else
            {                
                if (PlaceOrder())  return;
            }
        }

        private void OrderBoardTabCtrl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyboardDevice.Modifiers.Equals(Key.LeftAlt) && e.Key == Key.B)
                Logger.Log(e, SimpleLogger.LogCategory.Info);
        }

        private void tglbtnQuantMode_Click(object sender, RoutedEventArgs e)
        {
            //string posKey = Trader.Configuration.Investor.ID + orderboard.InstrumentID;
            //switch (tglbtnQuantMode.IsChecked)
            //{
            //    case null:
            //        ViewModel.CurrentMarketData.OrderBoardQuantMode = QuantMode.Default;
            //        break;
            //    case true:
            //        ViewModel.CurrentMarketData.OrderBoardQuantMode = QuantMode.Preset;
            //        break;
            //    case false:
            //        ViewModel.CurrentMarketData.OrderBoardQuantMode = QuantMode.AllAvailable;
            //        break;
            //}

            ////orderboard.Quant = 0;
            //switch (orderboard.quantMode)
            //{
            //    case (QuantMode.AllAvailable):
            //        {
            //            //setup the orderboard.Quant as inquiry all available quant of this instrument
            //            PositionDataSummary posi;
            //            orderboard.Quant = 0;
            //            if (dicPositionSummary.TryGetValue(posKey + DirectionType.多, out posi))
            //                orderboard.Quant = posi.Position;
            //            if (dicPositionSummary.TryGetValue(posKey + DirectionType.空, out posi))
            //                orderboard.Quant = posi.Position > orderboard.Quant ? posi.Position
            //                   : orderboard.Quant;
            //            //if (orderboard.Quant == 0)
            //            //{
            //            //    orderboard.Quant = 0;//(itemIndex < 0) ? 1 : Trader.DefaultInstrumentQuant[itemIndex].DefaultQuant;
            //            //}
            //        }
            //        break;
            //    case (QuantMode.Preset):
            //        //orderboard.Quant = 1;
            //        break;
            //    case (QuantMode.Default):
            //    default:
            //        InstrumentData inst;
            //        if(dicInstrumentData.TryGetValue(orderboard.InstrumentID,out inst))
            //        {
            //            orderboard.Quant = Trader.Configuration.GetDefaultQuant(inst.InstrumentID, inst.ProductID);
            //        }
            //        else
            //        {
            //            orderboard.Quant = Trader.Configuration.DefaultQuant;
            //        }
            //        break;
            //}
        }
        /*private void tglbtnPriceMode_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.RefreshOrderBoardPrice(this);
            //if (!tglbtnPriceMode.IsChecked.HasValue)
            //{
            //    orderboard.priceMode = PriceMode.Opposite;
            //    orderboard.OrderPrice = 0;
            //    //orderboard.ScrollBarPriceRefresh = false;
            //}
            //else
            //    if (tglbtnPriceMode.IsChecked == false)
            //{
            //    orderboard.priceMode = PriceMode.PreSet;
            //    orderboard.ScrollBarPriceRefresh = true;
            //    MarketData dmd;
            //    if (dicMarketData.TryGetValue(orderboard.InstrumentID, out dmd))
            //        orderboard.OrderPrice = dmd.LastPrice;
            //}
            //else
            //{
            //    orderboard.priceMode = PriceMode.Ownside;
            //    orderboard.OrderPrice = 0;
            //    orderboard.ScrollBarPriceRefresh = false;
            //}
        }
        
        private void scrollbarNum_Scroll(object sender, ScrollEventArgs e)
        {
            if (scrollbarNum.Value == 0)
            {
                scrollbarNum.Value = 1;
            }
            //ViewModel.CurrentMarketData.OrderBoardQuant = (int)e.NewValue;
                       
        }*/
                     
        private void TQScrollValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //ViewModel.CurrentMarketData.OrderBoardPricingMode = PricingMode.Preset;
            //ViewModel.CurrentMarketData.OrderBoardPriceCanBeRefresh = false;
            Viewmodel.CurrentMarketData.OrderBoardPrice = (decimal)e.NewValue; 
            //scrollbarPrice.Value += 1;
            Viewmodel.CurrentMarketData.NotifyPropertyChanged("OrderBoardPriceString");
                     
            //scrollbarPrice.SelectAll();            
        }
        private void scrollbarPrice_Scroll(object sender, ScrollEventArgs e)
        {
            Viewmodel.UserJustInputOrderBoardPrice();
        }
        /*private void Text_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            double dd;
            if (double.TryParse(scrollbarPrice.ValueText, out dd))
                scrollbarPrice.Value = dd;
            else scrollbarPrice.Value = 0;
            //ViewModel.UserJustInputOrderBoardPrice();
        }*/
        private void Text_KeyDown(object sender, KeyEventArgs e)
        {
            Viewmodel.UserJustInputOrderBoardPrice();
        }
        private void txtblkBidPrice_MouseEnter(object sender, MouseEventArgs e)
        {
            //txtblkBidPrice.Cursor = Cursors.Hand;
        }
        private void txtblkAskPrice_MouseEnter(object sender, MouseEventArgs e)
        {
            //txtblkBidPrice.Cursor = Cursors.Hand;
        }
        private void SelectInstrument_Click(object sender, EventArgs e)
        {
            //MarketData md;
            //if (sender is Button)
            //{
            //    string instrumentID = ((Button)sender).Content.ToString();
            //    //if (dicMarketData.TryGetValue(instrumentID, out md))
            //    //    ;// UpdateOrderBoard(dicMarketData[instrumentID]);
            //    //else
            //    //{
            //    //    //md = new MarketData(instrumentID);
            //    //    //dicMarketData.TryAdd(instrumentID, md));
            //    //    //MarketDataView.Add(md);
            //    //}
            //}
        }
        private void txtInstrumentID_GotMouseCapture(object sender, MouseEventArgs e)
        {
            InitProductPopup();
            popProduct.IsOpen = true;
        }
        private void popProduct_MouseLeave(object sender, MouseEventArgs e)
        {
            if (txtBoxInstrumentID.IsMouseOver == true | svProduct.IsMouseOver == true)
                popProduct.StaysOpen = true;
            else popProduct.IsOpen = false;
        }

        private void SelectProduct_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button)
            {
                popProduct.IsOpen = false;
                Button btn = sender as Button;
                string productName = btn.Content.ToString();
                string productID = btn.Tag.ToString();
                InitInstrumentPopup(productID);
                popInstrument.IsOpen = true;
            }
        }
        #endregion
       

        #region Enquiry system menu

        private void EnqInstruments_Click(object sender, RoutedEventArgs e)
        {
            InstrumentWindow instWin = new InstrumentWindow();
            instWin.ShowInTaskbar = false;
            instWin.Show();
            T.ReqQryInstrument(new QryInstrumentField());

        }

        private void EnqPositions_Click(object sender, RoutedEventArgs e)
        {
            TQMainModel.dicPositionSummary.Clear();
            Viewmodel.PositionDataSummaryView.Clear();
            T.ReqQryPosition();
        }

        private void EnqTrades_Click(object sender, RoutedEventArgs e)
        {
            T.ReqQryTrade();
        }

        private void EnqOrders_Click(object sender, RoutedEventArgs e)
        {
            T.ReqQryOrder();
        }

        private void EnqHistoricalSettlementInfo_Click(object sender, RoutedEventArgs e)
        {
            SettlementWindow settlement = new SettlementWindow(this, QueryRangeType.UserDefined);
            settlement.ShowDialog();
        }
       

        private void miQryTradeLog_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TradeLogWindow tradeLogWin = new TradeLogWindow();
                tradeLogWin.Show();
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message, SimpleLogger.LogCategory.Info);
            }
        }


        private void miQryInstMgr_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Thread thread = new Thread(() =>
                {
                    var instMgrWin = new InstMgrWindow();
                    instMgrWin.ShowInTaskbar = false;
                    instMgrWin.Show();
                    System.Windows.Threading.Dispatcher.Run();
                    instMgrWin.Closed += (sender2, e2) =>
                    instMgrWin.Dispatcher.InvokeShutdown();
                });

                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message, SimpleLogger.LogCategory.Info);
            }
        }

        private void miQryInstCommission_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Thread thread = new Thread(() =>
                {
                    var instCommissionWin = new InstCommissionWindow ();
                    instCommissionWin.ShowInTaskbar = false;
                    instCommissionWin.Show();
                    System.Windows.Threading.Dispatcher.Run();
                    instCommissionWin.Closed += (sender2, e2) =>
                    instCommissionWin.Dispatcher.InvokeShutdown();
                });

                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message, SimpleLogger.LogCategory.Error);
            }
        }
        #endregion

        #region 报价表 区域处理

        //private void MarketDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    if (e.AddedItems.Count > 0)
        //    {
        //        MarketData m = e.AddedItems[0] as MarketData;
        //        if (m == null) return;
        //        UpdateOrderBoard(m, true);
        //    }
        //}
        private void MarketDataGrid_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            /*
            //展示合约日内分时图
            string winname="";
            e.Source
            //if ((winname = Trader.CurrInstrumentDict[MarketDataGrid.SelectedIndex]) != null) return;
            Thread thread1 = new Thread(() =>
            {
                SingleInstruWin w = new SingleInstruWin(this);
                w.Show();
                //(MarketData)((DataGrid)sender).SelectedIndex
                w.Closed += (o, e1) =>
                {
                    w.Dispatcher.InvokeShutdown();
                    DicWindow.Remove(w.InstrumentID);
                };
                System.Windows.Threading.Dispatcher.Run();
                DicWindow.Add(winname, w);
            });

            thread1.SetApartmentState(ApartmentState.STA);
            thread1.IsBackground = true;
            thread1.Start();
            */
        }

        private void miSelectCustCntrGrp_MouseEnter(object sender, MouseEventArgs e)
        {
            //生成所有自选合约组的选项
            miSelectCustCntrGrp.Items.Clear();

            List<string> groupList = new List<string>();
            groupList = Trader.Configuration.GetInstrumentGroupNames();
            groupList.Remove(Trader.Configuration.DefaultInstrumentIDGroup);
            foreach (var group in groupList)
            {
                MenuItem miGroup = new MenuItem();
                miGroup.Header = group;
                miGroup.Click += miGroup_Click;
                miGroup.IsEnabled = true;
                miSelectCustCntrGrp.Items.Add(miGroup);
            }
        }

        void miGroup_Click(object sender, RoutedEventArgs e)
        {
            string group = (e.OriginalSource as MenuItem).Header.ToString();
            SwitchOrUpdateInstrumentIDsGroup(group);
        }

        private void MarketDataAutoAdjustColumnWidth_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        #region 报告查询

        /// <summary>
        /// 自定义查询日期范围查询报告
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        
        /// <summary>
        /// 查询今日报告
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        

        /// <summary>
        /// 查询最近一周报告
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
       
        
        #endregion

        #region 未成交单处理
        private void btnOrderCancel_Click(object sender, RoutedEventArgs e)
        {           
            if (UnsettledOrdersGrid.SelectedIndex >= 0)
            {
                OrderData order = UnsettledOrdersGrid.SelectedItem as OrderData;
                DeleteUnsettledOrder(order);
            }            
        }
        private void btnOrdersCancel_Click(object sender, RoutedEventArgs e)
        {
            if (TQMainModel.dicOrder.Count < 1)
            {
                return;
            }
            var orders = TQMainModel.dicOrder.Values.ToList().FindAll(x => x.OrderStatus == OrderStatusType.未成交
            || x.OrderStatus == OrderStatusType.部分成交 || x.OrderStatus == OrderStatusType.未成交 || x.OrderStatus == OrderStatusType.未知);
            DeleteAllUnsettledOrder(orders);
        }
        #endregion
        #region 所有委托单处理

        #endregion
        private void CanceledOrdersColumnSet_Click(object sender, RoutedEventArgs e)
        {
            SetDataGridColumn(SettingsType.CanceledOrdersColumn);
        }
        #region 预埋-条件单委托事件处理

        private void ComplexOrderGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        //全部
        private void condAllRadBtn_Checked(object sender, RoutedEventArgs e)
        {

        }
        //预埋
        private void condParkedRadBtn_Checked(object sender, RoutedEventArgs e)
        {
            // ParkedOrderView = ParkedOrderView.ToList().Where(c => c.StopPrice == 0);
            var tempList = Viewmodel.ParkedOrderView.ToList().Where(c => c.StopPrice == 0);
            Viewmodel.ParkedOrderView.Clear();
            foreach (var item in tempList)
            {
                Viewmodel.ParkedOrderView.Add(item);
            }
        }
        //条件
        private void condCondRadBtn_Checked(object sender, RoutedEventArgs e)
        {
            //ParkedOrderView = ParkedOrderView.ToList().Where(c => c.StopPrice !=0);
            var tempList = Viewmodel.ParkedOrderView.ToList().Where(c => c.StopPrice != 0);
            Viewmodel.ParkedOrderView.Clear();
            foreach (var item in tempList)
            {
                Viewmodel.ParkedOrderView.Add(item);
            }
        }

        //已发送
        private void condSendRadBtn_Checked(object sender, RoutedEventArgs e)
        {
            //ParkedOrderView = ParkedOrderView.ToList().Where(c => c.OrderStatus == EnumDescription.GetFieldText(ParkedOrderStatusType.Send));
            var tempList = Viewmodel.ParkedOrderView.ToList().Where(c =>c.OrderStatus == OrderStatusType.下单中);
            Viewmodel.ParkedOrderView.Clear();
            foreach (var item in tempList)
            {
                Viewmodel.ParkedOrderView.Add(item);
            }
        }

        

       

        #endregion


        private void miInstructions_Click(object sender, RoutedEventArgs e)
        {
            Logger.Log("trigger the help command", SimpleLogger.LogCategory.Info);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.F1://报价表
                    tabOfferingList.IsSelected = true;
                    break;
                case Key.F2://合约列表
                    tabInstrumentList.IsSelected = true;
                    break;
                //case Key.F3://持仓记录
                //    tabNormal.IsSelected = true;
                //    //RdBtnPst.IsChecked = true;
                //    break;
                case Key.F4://成交记录
                    tabTradeRecord.IsSelected = true;                  
                    break;
                case Key.F5://未成交单
                    tabUnsettledOrder.IsSelected = true;
                    break;
                case Key.F6://所有委托单
                    tabAllOrder.IsSelected = true;
                    break;                
                case Key.F8://撤单错单
                    tabCanceledOrders.IsSelected = true;
                    break;
                case Key.F7://已成交单
                    tabSettledOrder.IsSelected = true;
                    //RdBtnDtl.IsChecked = true;
                    break;
                case Key.Home://标准下单板
                    tabStardOrderBoard.IsSelected = true;
                    break;
            }
        }
        #region 选项 系统菜单

        //保存配置到文件
        private void miSaveConfiguration_Click(object sender, RoutedEventArgs e)
        {

        }
        //加载配置文件
        private void miLoadConfiguration_Click(object sender, RoutedEventArgs e)
        {

        }

        private void miDefaultQuant_Click(object sender, RoutedEventArgs e)
        {
            SetDataGridColumn(SettingsType.DefaultQuant);
        }

        private void MultipleSetup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                isModifyCustInst = true;
                MultipleSettingsWindow settingsWin = new MultipleSettingsWindow(SettingsType.EditingInstrumentData, this);
                settingsWin.Show();
                isModifyCustInst = false;
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message, SimpleLogger.LogCategory.Error);
            }
            finally { isModifyCustInst = false; }
        }

        private void OptionSettings_Click(object sender, RoutedEventArgs e)
        {
            SetDataGridColumn(SettingsType.SettingInstrumentGroup);
        }
        #endregion

        #region 帮助 系统菜单

        private void miErrorReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ErrorReportWindow errReportWin = new ErrorReportWindow();
                errReportWin.Show();
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message, SimpleLogger.LogCategory.Info);
            }
        }
        private void AboutTQ_Click(object sender, RoutedEventArgs e)
        {
            AboutTickQuant abouttq = new AboutTickQuant();
            abouttq.Show();
        }

        #endregion

        #region 资金账户操作

        private void btnRefreshAccount_Click(object sender, RoutedEventArgs e)
        {
            Okex.RefreshUserInfo();          
        }

        private void btnQryAccount_Click(object sender, RoutedEventArgs e)
        {
            //查询持仓和成交            
            Okex.ReqAllPosition();            
        }                    
        #endregion
        #region 窗体私有函数
      
        
        /// <summary>
        /// 保存账户目录下的用户配置和扩展配置信息
        /// </summary>
        private void SaveConfiguration()
        {
            try
            {
                if (Trader.Configuration != null)
                {
                    Trader.Configuration.Save();
                }
                if (Trader.ExtConfig != null)
                {
                    Trader.ExtConfig.ConditionalOrderNo = Trader.CustProductOrderNo;
                    Trader.ExtConfig.Save();
                }
                //交易配置信息
                InstrumentInfoConfiguration instCfg = new InstrumentInfoConfiguration();
                instCfg.ListInstrument.AddRange(TQMainModel.dicInstrumentData.Values);
               
                TQXmlHelper.XmlSerializeToFile(instCfg, Trader.InstrumentInfoCfgFile, Encoding.UTF8);              
            }
            catch (Exception ex)
            {
                Utility.WriteMemFile(ex.ToString());
            }
        }
        /// <summary>
        /// 设置DataGrid需要显示的列
        /// </summary>
        /// <param name="settingsType"></param>
        private void SetDataGridColumn(SettingsType settingsType)
        {
            try
            {
                MultipleSettingsWindow settingsWin = new MultipleSettingsWindow(settingsType, this);
                settingsWin.Show();
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message, SimpleLogger.LogCategory.Info);
            }
        }

        /// <summary>
        /// 切换或者更新管理自选合约组
        /// </summary>
        /// <param name="groupName"></param>
        public void SwitchOrUpdateInstrumentIDsGroup(string groupName)//, TQConcurrentDictionary<string, InstrumentData> dicTQInstru)
        {
            //首先设置标志位，不再接收行情推送
            //再退订当前合约组的行情
            //清空 行情的View
            //订阅所选合约组的行情          


            InstrumentGroup thisInstrumentgroup = Trader.Configuration.InstrumentGroupList.Find(x => x.Name == groupName);
            if (thisInstrumentgroup == null)
            {
                Utility.WriteMemLog("error on finding InstrumentGroup: " + groupName);
            }
            else
            {
                //总结一下：Trader.Configuration.InstrumentGroupList, 自定义合约组列表（含自定义品种）
                //Trader.InstrumentIDList, 保留的当前合约组合约列表，可以合并入Trader.Configuration.InstrumentGroupList
                //Q.SubscibedInstrumentIDs，保存当前订阅的合约列表（基本合约，自定义品种拆分为基本合约）
                //根据合约组InstrumentDataList合约信息列表（List<InstrumentData>, 含自定义品种)生成当前需要订阅合约列表字典（基础合约列表）

                Trader.Configuration.DefaultInstrumentIDGroup = groupName;                
                TQMainModel.SubscribedInstrumentIDs.Clear();

                TQMainModel.dicMajorMarketData.Clear();
                Viewmodel.MajorMarketDataView.Clear();
                TQMainModel.dicCustomProduct.Clear();

                foreach (var v in thisInstrumentgroup.InstrumentDataList)
                {
                    if (!TQMainModel.SubscribedInstrumentIDs.Keys.Contains(v.InstrumentID))
                        TQMainModel.SubscribedInstrumentIDs.Add(v.InstrumentID, v);

                    if (v.InstrumentID.Length <= 14)
                    {
                       MajorMarketData md = new MajorMarketData(v.InstrumentID) { IsCombo = false };
                        TQMainModel.dicMajorMarketData.TryAdd(v.InstrumentID, md);
                    }
                }
                Trader.CurrInstrumentDict = TQMainModel.SubscribedInstrumentIDs;     //产生当前自定义合约组合约列表（含自定义品种，未拆分，未包含当前持仓合约）
                if (TQMainModel.dicPositionSummary.Count > 0)   //包含持仓中的合约
                {
                    foreach (var item in TQMainModel.dicPositionSummary.Values)
                    {
                        if (item == null || (item.ShortPosition == 0 && item.LongPosition == 0))
                        {
                            continue;
                        }
                        if (!TQMainModel.SubscribedInstrumentIDs.Keys.Contains(item.InstrumentID))
                        {
                            TQMainModel.SubscribedInstrumentIDs.Add(item.InstrumentID, new InstrumentData() { InstrumentID = item.InstrumentID, ExchangeID = item.ExchangeID });
                        }

                        TQMainModel.dicMajorMarketData.TryAdd(item.InstrumentID, new MajorMarketData(item.InstrumentID) { IsCombo = false });
                    }
                }
                //生成阶段性Q.SubscribeInstrumentIDs,下一步需要拆分自定义品种
                ComboMarketData cust;
                //for (int i =  TQMainModel.SubscribedInstrumentIDs.Count-1;i>=0; i--)
                foreach (var aa in TQMainModel.SubscribedInstrumentIDs)
                {
                    if (aa.Key.Length <14)   //基本合约
                    {
                        continue;
                    }
                    else     //customproduct
                    {
                        if (!TQMainModel.dicAllCustomProductList.TryGetValue(aa.Key, out cust))  //异常情况，通常应该在里面了
                        {
                            cust = new ComboMarketData(aa.Key) { IsCombo = true };

                            TQMainModel.dicAllCustomProductList.TryAdd(aa.Key, cust);
                        }
                        foreach (var v in cust.ItemList)
                        {
                            if (!TQMainModel.SubscribedInstrumentIDs.Keys.Contains(v.InstrumentID))   //组合里的合约没在订阅合约中
                            {
                                TQMainModel.SubscribedInstrumentIDs.Add(v.InstrumentID, new InstrumentData() { InstrumentID = v.InstrumentID, ExchangeID = v.MajorMarketData.ExchangeID });
                                TQMainModel.dicMajorMarketData.TryAdd(v.InstrumentID, new MajorMarketData(v.InstrumentID) { IsCombo = false, ExchangeID = Utility.GetExchangeID(v.InstrumentID) });
                            }
                        }                      
                        foreach (var m in cust.ItemList)
                        {
                            if (m.MajorMarketData == null) m.MajorMarketData = TQMainModel.dicMajorMarketData[m.InstrumentID];
                        }
                        TQMainModel.dicMajorMarketData.TryAdd(aa.Key, cust);
                        TQMainModel.dicCustomProduct.TryAdd(cust.InstrumentID, cust);
                        //MarketDataView.Add(cust);
                        TQMainModel.SubscribedInstrumentIDs.Remove(aa.Key);
                    }
                }
                for (int i = 0; i < thisInstrumentgroup.InstrumentDataList.Count; i++)
                   Viewmodel.MajorMarketDataView.Add(TQMainModel.dicMajorMarketData[thisInstrumentgroup.InstrumentDataList[i].InstrumentID]);

                if (thisInstrumentgroup.InstrumentDataList.Count > 0)
                    MarketDataGrid.SelectedIndex = 0;
                //InitMarketDataSerial();
               
                QryInstrumentCommissionRate(TQMainModel.SubscribedInstrumentIDs.Keys.ToList());
                QryInstrumentMarginRate(TQMainModel.SubscribedInstrumentIDs.Keys.ToList());
            }          
        }

      
        //对价平仓
        private void ClosePositionWithOppositePrice(PositionDataSummary posSum)
        {
            
        }
        
        //市价平仓
        private void ClosePositionWithMarketPrice(PositionDataSummary posSum)
        {
            
        }

        private void ClosePositionWithMarketPrice()
        {
           
        }

       
        //市价反手
        //private void ReversePositionMarketPrice(PositionDataSummary posSum)
        //{
        //    //1.先对当前持仓汇总平仓
        //    TradeDirection dir = (posSum.Direction == DirectionType.空) ? TradeDirection.多 : TradeDirection.空;
        //    var orderboard = dicMarketData[posSum.InstrumentID];
        //    orderboard.OrderBoardPricingMode = PricingMode.Market;
        //    orderboard.OrderBoardOrderMode = OrderMode.Close;
        //    orderboard.OrderBoardPriceMode = PriceMode.PreSet;
        //    // = HedgeType.投机;//need to do
        //    orderboard.exchangeID = Utility.GetExchangeID(posSum.ExchangeName);
        //    T.ReqClosePosition(orderboard, dir);
        //    //2.再开反向仓
        //    orderboard.OrderBoardPricingMode = PricingMode.Market;
        //    orderboard.OrderBoardOrderMode = OrderMode.Open;
        //    orderboard.OrderBoardPriceMode = PriceMode.PreSet;
        //    orderboard.OrderBoardHedgeType = HedgeType.投机;//need to do
        //    orderboard.exchangeID = Utility.GetExchangeID(posSum.ExchangeName);
        //    T.ReqOrderInsert(orderboard, dir, posSum.Position);

        //    #region old method
        //    //bool isSHFE = (posSum.ExchangeName == "上期所") ? true : false;
        //    //MarketData md;
        //    //if (!dicMarketData.TryGetValue(posSum.InstrumentID, out md))
        //    //{
        //    //    if (isSHFE)
        //    //    {
        //    //        Logger.Log("该合约没有订阅行情，请订阅行情后再进行市价反手");
        //    //        return;
        //    //    }
        //    //}
        //    //DirectionType direction;
        //    //double price;
        //    //if (posSum.Direction == DirectionType.空)
        //    //{
        //    //    direction = DirectionType.多;
        //    //    price = isSHFE ? md.UpperLimitPrice : 0;
        //    //}
        //    //else
        //    //{
        //    //    direction = DirectionType.空;
        //    //    price = isSHFE ? md.LowerLimitPrice : 0;
        //    //}
        //    ////如有挂单，先撤单,再行反手
        //    //var unsettledOrders = GetUnsettledCloseOrders(posSum.InstrumentID, direction);
        //    //if (unsettledOrders != null && unsettledOrders.Count > 0)
        //    //{
        //    //    Task.Run(() =>
        //    //    {
        //    //        //先撤单
        //    //        int num = 0;
        //    //        foreach (var order in unsettledOrders)
        //    //        {
        //    //            num++;
        //    //            T.ReqOrderAction(order.FrontID, order.SessionID, order.OrderRef, order.InstrumentID);
        //    //        }
        //    //        string key = posSum.InstrumentID + direction.ToString();
        //    //        T.dicUnsettledCloseOrderNum.AddOrUpdate(key, num, (k, v) => num);
        //    //        while (true)
        //    //        {
        //    //            num = -1;
        //    //            if (!T.dicUnsettledCloseOrderNum.TryGetValue(key, out num))
        //    //            {
        //    //                //撤单成功后，再进行平仓操作
        //    //                if (isSHFE)
        //    //                {
        //    //                    if (posSum.YdPosition > 0)
        //    //                    {
        //    //                        T.ReqOrderInsert(posSum.InstrumentID, direction == DirectionType.多 ? TradeDirection.多 : TradeDirection.空, OffsetType.平昨, price, posSum.YdPosition, LimitMarketFakFok.Limit);

        //    //                        T.ReqClosePosition(dicMarketData[posSum.InstrumentID], direction == DirectionType.多 ? TradeDirection.多 : TradeDirection.空, posSum.YdPosition);
        //    //                    }
        //    //                    if (posSum.TodayPosition > 0)
        //    //                    {
        //    //                        T.ReqOrderInsert(posSum.InstrumentID, direction == DirectionType.多 ? TradeDirection.多 : TradeDirection.空, OffsetType.平今, price, posSum.TodayPosition, LimitMarketFakFok.Limit);

        //    //                        T.ReqClosePosition(dicMarketData[posSum.InstrumentID], direction == DirectionType.多 ? TradeDirection.多 : TradeDirection.空, posSum.TodayPosition);

        //    //                    }
        //    //                }
        //    //                else
        //    //                {
        //    //                    T.ReqOrderInsert(posSum.InstrumentID, direction == DirectionType.多 ? TradeDirection.多 : TradeDirection.空, OffsetType.平仓, price, posSum.Position, LimitMarketFakFok.Limit);
        //    //                }
        //    //            }
        //    //            Thread.Sleep(1000);
        //    //        }
        //    //        while (true)
        //    //        {
        //    //            //平仓成功后，最后进行反向开仓操作 //todo:huangrongyu
        //    //            if (true)
        //    //            {
        //    //                if (isSHFE)
        //    //                {
        //    //                    T.ReqOrderInsert(posSum.InstrumentID, direction == DirectionType.多 ? TradeDirection.多 : TradeDirection.空, OffsetType.开仓, price, posSum.Position, LimitMarketFakFok.Limit);
        //    //                }
        //    //                else
        //    //                {
        //    //                    T.ReqOrderInsert(posSum.InstrumentID, direction == DirectionType.多 ? TradeDirection.多 : TradeDirection.空, OffsetType.开仓, 0, posSum.Position, LimitMarketFakFok.Market, HedgeType.投机);
        //    //                }
        //    //            }
        //    //            Thread.Sleep(1000);
        //    //        }
        //    //    });
        //    //}
        //    //else
        //    //{
        //    //    Task.Run(() =>
        //    //    {
        //    //        if (isSHFE)
        //    //        {
        //    //            if (posSum.YdPosition > 0)
        //    //            {
        //    //                T.ReqOrderInsert(posSum.InstrumentID, direction == DirectionType.多 ? TradeDirection.多 : TradeDirection.空, OffsetType.平昨, price, posSum.YdPosition, LimitMarketFakFok.Limit);
        //    //            }
        //    //            if (posSum.TodayPosition > 0)
        //    //            {
        //    //                T.ReqOrderInsert(posSum.InstrumentID, direction == DirectionType.多 ? TradeDirection.多 : TradeDirection.空, OffsetType.平今, price, posSum.TodayPosition, LimitMarketFakFok.Limit);
        //    //            }
        //    //        }
        //    //        else
        //    //        {
        //    //            T.ReqOrderInsert(posSum.InstrumentID, direction == DirectionType.多 ? TradeDirection.多 : TradeDirection.空, OffsetType.平仓, 0, posSum.Position, LimitMarketFakFok.Market, HedgeType.投机);
        //    //        }
        //    //        while (true)
        //    //        {
        //    //            //平仓成功后，最后进行反向开仓操作 //todo:huangrongyu
        //    //            if (true)
        //    //            {
        //    //                if (isSHFE)
        //    //                {
        //    //                    T.ReqOrderInsert(posSum.InstrumentID, direction == DirectionType.多 ? TradeDirection.多 : TradeDirection.空, OffsetType.开仓, price, posSum.Position, LimitMarketFakFok.Limit);
        //    //                }
        //    //                else
        //    //                {
        //    //                    T.ReqOrderInsert(posSum.InstrumentID, direction == DirectionType.多 ? TradeDirection.多 : TradeDirection.空, OffsetType.开仓, 0, posSum.Position, LimitMarketFakFok.Market, HedgeType.投机);
        //    //                }
        //    //            }
        //    //            Thread.Sleep(1000);
        //    //        }
        //    //    });
        //    //}
        //    #endregion
        //}
        /// <summary>
        /// 获取未成交的平仓单列表
        /// </summary>
        /// <param name="instrumentID">合约ID</param>
        /// <param name="direction">平仓方向</param>
        /// <returns></returns>
        private List<OrderData> GetUnsettledCloseOrders(string instrumentID, TradeDirection direction)
        {
            List<OrderData> unsettledCloseOrders = new List<OrderData>();
            TQMainModel.dicOrder.Values.ToList().FindAll(x => x.InstrumentID == instrumentID && x.Direction == direction &&
                (x.Offset == OffsetType.平仓 )
                && (x.OrderStatus == OrderStatusType.尚未触发 || x.OrderStatus == OrderStatusType.下单中 || x.OrderStatus == OrderStatusType.未知 || x.OrderStatus == OrderStatusType.未成交)).ToList();
            if (unsettledCloseOrders == null)
            {
                unsettledCloseOrders = new List<OrderData>();
            }
            return unsettledCloseOrders;
        }

        //清仓
        private void ForceCloseAllPositions()
        {
            if (Trader.Configuration.ConfirmOrder == true)
            {
                MessageBoxResult mr = MessageBoxResult.None;

                Task t = Task.Run(new Action(() =>
                {
                    mr = System.Windows.MessageBox.Show("确认平掉所有持仓 ? ", "确认平仓", MessageBoxButton.YesNo);
                    if (mr == MessageBoxResult.Yes)
                    {
                        T.ReqForceClose(Viewmodel.CurrentMarketData);                            }
                }));
            }
            else
            {
                T.ReqForceClose(Viewmodel.CurrentMarketData);
            }
        }
        /// <summary>
        /// 撤单
        /// </summary>
        /// <param name="order"></param>
        private void DeleteUnsettledOrder(OrderData order )
        {

            switch (order.ExchangeID)
            {
                case EnuExchangeID.BitMex:
                    Task.Run(() =>
                    Bitmex.DeleteOrders(order.OrderID));
                    break;
                case EnuExchangeID.OkexFutures:
                case EnuExchangeID.OkexSwap:
                    Task.Run(() =>
                    Okex.CancelOrder(order.InstrumentID, order.OrderID));            
                    break;
            }           
        }
        private void DeleteAllUnsettledOrder(List<OrderData> orders)
        {
            InputOrderActionField inputOrderAction = new InputOrderActionField();
            if (Trader.Configuration.ConfirmOrder == true)
            {
                MessageBoxResult mr = MessageBoxResult.None;

                Task t = Task.Run(new Action(() =>
                {
                    mr = System.Windows.MessageBox.Show("确认撤消所有未成交单 ? ", "确认撤单", MessageBoxButton.YesNo);
                    if (mr == MessageBoxResult.Yes)
                    {
                        foreach (OrderData order in orders)
                        {
                            inputOrderAction = new InputOrderActionField()
                            {
                                ActionFlag = (char)ActionFlagType.Delete,
                                InstrumentID = order.InstrumentID,
                                ExchangeID = order.ExchangeID.ToString(),                               
                                //OrderRef = order.OrderRef,
                               // OrderSysID = order.OrderSysID,                               
                            };
                            T.ReqOrderAction(inputOrderAction);
                        }
                    }
                }));
            }
            else
                foreach (OrderData order in orders)
                {
                    inputOrderAction = new InputOrderActionField()
                    {
                        ActionFlag = (char)ActionFlagType.Delete,
                        InstrumentID = order.InstrumentID,
                        ExchangeID = order.ExchangeID.ToString(),//Utility.GetExchangeID(order.ExchangeID.ToString()),                     
                        //OrderRef = order.OrderRef,
                        //OrderSysID = order.OrderSysID                     
                    };
                    T.ReqOrderAction(inputOrderAction);
                }
        }
       

        /// <summary>
        /// 初始化品种下拉框
        /// </summary>
        private void InitProductPopup()
        {
            List<string> productIds = new List<string> { "比特币", "莱特币", "比特现金", "以太币", "柚子"};
            productIds = TQMainModel.dicInstrumentData.Values.Select(x => x.Name).Distinct().ToList();
            productIds.Sort();
            int columns = 3;
            double dRow = productIds.Count / columns;
            int rows = (int)Math.Ceiling(dRow);
            gridProduct.Children.Clear();
            gridProduct.RowDefinitions.Clear();
            gridProduct.ColumnDefinitions.Clear();
            for (int r = 0; r < rows; r++)
            {
                gridProduct.RowDefinitions.Add(new RowDefinition() );
            }
            for (int c = 0; c < columns; c++)
            {
                gridProduct.ColumnDefinitions.Add(new ColumnDefinition());
            }
            string productName = string.Empty;
            List<Button> buttons = new List<Button>();
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    buttons.Add(new Button());
                    if (TQMainModel.dicProductIDName.TryGetValue(productIds[r * columns + c], out productName))
                    {
                        buttons[r * columns + c].Content = productName;
                    }
                    else
                    {
                        buttons[r * columns + c].Content = productIds[r * columns + c];
                    }
                    buttons[r * columns + c].Tag = productIds[r * columns + c];
                    buttons[r * columns + c].Width = 60;
                    buttons[r * columns + c].Height = 30;
                    buttons[r * columns + c].Margin = new Thickness(0, 1, 1, 1);
                    gridProduct.Children.Add(buttons[r * columns + c]);
                    buttons[r * columns + c].SetValue(Grid.RowProperty, r);
                    buttons[r * columns + c].SetValue(Grid.ColumnProperty, c);
                    buttons[r * columns + c].Click += new RoutedEventHandler(SelectProduct_Click);
                }
            }
        }

        /// <summary>
        /// 初始化合约下拉框
        /// </summary>
        /// <param name="productID"></param>
        private void InitInstrumentPopup(string productID)
        {
            List<string>  instrumentIds = TQMainModel.dicInstrumentData.Values.Where(x => x.Name== productID).Select(x=>x.InstrumentID).ToList();
            instrumentIds.Sort();
            int columns = 1;
            double dRow = instrumentIds.Count / columns;
            int rows = (int)Math.Ceiling(dRow);
            gridInstrument.Children.Clear();
            gridInstrument.RowDefinitions.Clear();
            gridInstrument.ColumnDefinitions.Clear();
            for (int r = 0; r < rows; r++)
            {
                gridInstrument.RowDefinitions.Add(new RowDefinition() );
            }
            for (int c = 0; c < columns; c++)
            {
                gridInstrument.ColumnDefinitions.Add(new ColumnDefinition());
            }
            List<Button> buttons = new List<Button>();
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    buttons.Add(new Button());
                    buttons[r * columns + c].Content = instrumentIds[r * columns + c];
                    buttons[r * columns + c].Width = 60;
                    buttons[r * columns + c].Height = 30;
                    buttons[r * columns + c].Margin = new Thickness(0, 1, 0, 1);
                    gridInstrument.Children.Add(buttons[r * columns + c]);
                    buttons[r * columns + c].SetValue(Grid.RowProperty, r);
                    buttons[r * columns + c].SetValue(Grid.ColumnProperty, c);
                    buttons[r * columns + c].Click += new RoutedEventHandler(SelectInstrument_Click);
                }
            }
        }
       
        #endregion

        #region 窗体公有函数

        public void Ring(string pRing)
        {
            if (File.Exists("wav\\" + pRing + ".wav"))
            {
                new SoundPlayer("wav\\" + pRing + ".wav").Play();
            }
        }

        public void ShowMsg(string pMsg)
        {
            /*if (this.txtbox11.CheckAccess())
                txtbox11.Text = DateTime.Now.ToString("HH:mm:ss") + " " + pMsg;
            else
                Dispatcher.BeginInvoke(new Action(() => txtbox11.Text = DateTime.Now.ToString("HH:mm:ss") + " " + pMsg));
                */
        }


        /// <summary>
        /// 请求查询合约的交易费（合约为需要订阅行情的合约代码）
        /// </summary>
        public void QryInstrumentCommissionRate(List<string> instrumentIds)
        {
            if(instrumentIds==null || instrumentIds.Count<1)
            { return; }
            return;            
        }

        /// <summary>
        /// 请求查询合约的保证金（合约为需要订阅行情的合约代码）
        /// </summary>
        public void QryInstrumentMarginRate(List<string> instrumentIds)
        {
            
        }
                
        #endregion

        #region 自定义命令的CanExecute,Execute函数

        public void DefaultQuant_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }
        public void DefaultQuant_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var cmd = e.Command as RoutedUICommand;
            txtBoxInstrumentID.Text = cmd.Name;
            int defaultQuant = 1;
            DefaultQuantSet quantset;
            if(Trader.DefaultInstrumentQuant.TryGetValue(cmd.Name, out quantset)) //.Where(x => x.Value.InstrumentID == cmd.Name);
            {
                defaultQuant = quantset.Quant;
            }
            tglbtnQuantMode.IsChecked = null;
            scrollbarNum.Value = defaultQuant;

            //orderboard.quantMode = QuantMode.Default;
            //orderboard.Quant = defaultQuant;
            //orderboard.InstrumentID = cmd.Name;

            if (cmd.Name.Length < 3)
            {
                txtBoxInstrumentID.Focus();
            }
            e.Handled = true;
        }

        public void ChangeGroup_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }
        public void ChangeGroup_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var cmd = e.Command as RoutedUICommand;
            SwitchOrUpdateInstrumentIDsGroup(cmd.Name);
            e.Handled = true;
        }

        public void TradeCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            RoutedUICommand cmd = e.Command as RoutedUICommand;
            switch (cmd.Name)
            {
                case "全撤":
                    e.CanExecute = (UnsettledOrdersGrid.Items.Count < 1) ? false : true;
                    break;
                case "撤单":
                    e.CanExecute = (UnsettledOrdersGrid.SelectedIndex < 0) ? false : true;
                    break;
                case "对价平仓":
                case "市价平仓":
                case "市价反手":
                    e.CanExecute = (PositionSummaryGrid.SelectedIndex < 0) ? false : true;
                    break;
                case "清仓":
                    e.CanExecute = (PositionSummaryGrid.Items.Count < 1) ? false : true;
                    break;
                case "买":
                case "卖":
                    e.CanExecute = (!string.IsNullOrEmpty(txtBoxInstrumentID.Text) && scrollbarNum.Value > 0);
                    break;
            }
            e.Handled = true;
        }
        /// <summary>
        /// 交易命令Execute函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void TradeCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            RoutedUICommand cmd = e.Command as RoutedUICommand;
            PositionDataSummary pos;
            switch (cmd.Name)
            {
                case "全撤":
                    //var orders = UnsettledOrdersGrid.ToList();
                    if (TQMainModel.dicOrder.Count < 1)
                    {
                        return;
                    }
                    var orders = TQMainModel.dicOrder.Values.ToList().FindAll(x => x.OrderStatus == OrderStatusType.未成交 || x.OrderStatus == OrderStatusType.部分成交
                    || x.OrderStatus == OrderStatusType.未知);
                    DeleteAllUnsettledOrder(orders);
                    break;
                case "撤单":
                    var order = UnsettledOrdersGrid.SelectedItem as OrderData;
                    DeleteUnsettledOrder(order);
                    break;
                case "对价平仓":
                    pos = PositionSummaryGrid.SelectedItem as PositionDataSummary;
                    ClosePositionWithOppositePrice(pos);
                    break;
                case "市价平仓":
                    pos = PositionSummaryGrid.SelectedItem as PositionDataSummary;
                    ClosePositionWithMarketPrice(pos);
                    break;
                case "市价反手":
                    pos = PositionSummaryGrid.SelectedItem as PositionDataSummary;
                    T.ReqReversePosition(pos); //ReversePositionMarketPrice(pos);
                    break;
                case "清仓":
                    T.ReqForceClose(Viewmodel.MajorMarketDataView[MarketDataGrid.SelectedIndex]);
                    break;
                case "买":
                    T.ReqOrderInsert(Viewmodel.MajorMarketDataView[MarketDataGrid.SelectedIndex],TradeDirection.Long, Viewmodel.MajorMarketDataView[MarketDataGrid.SelectedIndex].OrderBoardQuant); //(TradeDirection.多);
                    break;
                case "卖":
                    T.ReqOrderInsert(Viewmodel.MajorMarketDataView[MarketDataGrid.SelectedIndex], TradeDirection.Short, Viewmodel.MajorMarketDataView[MarketDataGrid.SelectedIndex].OrderBoardQuant);
                    break;
            }
            e.Handled = true;
        }
        #endregion
        private void popInstrument_MouseLeave(object sender, MouseEventArgs e)
        {
            if (txtBoxInstrumentID.IsMouseOver == true |svInstrument.IsMouseOver == true)
                popInstrument.StaysOpen = true;
            else popInstrument.IsOpen = false;
        }              
        
        private void txtBoxInstrumentID_MouseEnter(object sender, MouseEventArgs e)
        {
            this.txtBoxInstrumentID.SelectAll();
            this.txtBoxInstrumentID.Focus();
        }
        private void StrategyWin_Click(object sender, RoutedEventArgs e)
        {
            //if (strategyWindow == null)
            //    strategyWindow = new StrategyWindow(this);
            //strategyWindow.Show();
        }
        private void scrollbarPrice_MouseEnter(object sender, MouseEventArgs e)
        {
            /*Dispatcher.Invoke(
                 new Action(() =>
             scrollbarPrice.Value = (double)Viewmodel.CurrentMarketData.LastPrice));
             */
        }       
        private void InternalTransfer_Click(object sender, RoutedEventArgs e)
        {
            Logger.Log("Please input transfer amount: ", SimpleLogger.LogCategory.Info);
        }

        private void OrderBoardTabCtrl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Plus12_Click(object sender, RoutedEventArgs e)
        {
            Viewmodel.UserJustInputOrderBoardPrice();
            Viewmodel.CurrentMarketData.OrderBoardPrice = PriceQualification((decimal)Viewmodel.CurrentMarketData.PriceTick,
                Viewmodel.CurrentMarketData.OrderBoardPrice*1.002m);
          
            scrollbarPrice.Value = (double)Viewmodel.CurrentMarketData.OrderBoardPrice;
            Viewmodel.CurrentMarketData.NotifyPropertyChanged("OrderBoardPriceString");
        }

        private void Minus12_Click(object sender, RoutedEventArgs e)
        {
            Viewmodel.UserJustInputOrderBoardPrice();
            Viewmodel.CurrentMarketData.OrderBoardPrice= PriceQualification((decimal) Viewmodel.CurrentMarketData.PriceTick,
                Viewmodel.CurrentMarketData.OrderBoardPrice*0.998m);
            scrollbarPrice.Value = (double)Viewmodel.CurrentMarketData.OrderBoardPrice;
            Viewmodel.CurrentMarketData.NotifyPropertyChanged("OrderBoardPriceString");
        }
        private decimal PriceQualification(decimal pricetick, decimal price)
        {
            if (pricetick > 0) return (Math.Round(price / pricetick)) * pricetick;
            else return 0; ;
        }

        private void IncreaseQuant_Click(object sender, RoutedEventArgs e)
        {
            Viewmodel.CurrentMarketData.OrderBoardQuant += 50;
        }

        private void ReduceQuant_Click(object sender, RoutedEventArgs e)
        {
            Viewmodel.CurrentMarketData.OrderBoardQuant -= 50;
            if (Viewmodel.CurrentMarketData.OrderBoardQuant < 1)
                Viewmodel.CurrentMarketData.OrderBoardQuant = 1;
        }

        private void TradeLog_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
