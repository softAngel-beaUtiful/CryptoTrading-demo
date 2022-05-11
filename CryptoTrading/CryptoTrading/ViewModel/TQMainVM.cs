using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Timers;
using CryptoTrading.Model;
using CryptoTrading.TQLib;

namespace CryptoTrading.ViewModel
{
    public class TQMainVM : ObservableObject
    {
        /// <summary>
        /// 用户输入后，价格恢复延迟秒数
        /// </summary>
        private const int _UserInputPriceDelaySeconds = 4;
        public readonly ObservableCollection<TradingAccountData> TradingAccountDataView = new ObservableCollection<TradingAccountData>();
        public readonly ObservableCollection<OrderData> OrderDataView = new ObservableCollection<OrderData>();
        public readonly ObservableCollection<OrderData> ParkedOrderView = new ObservableCollection<OrderData>();
        public readonly ObservableCollection<TradeData> TradeDataView = new ObservableCollection<TradeData>();
        public readonly ObservableCollection<TradeDataSummary> TradeDataSummaryView = new ObservableCollection<TradeDataSummary>();
        public readonly ObservableCollection<PositionDetail> PositionDetailView = new ObservableCollection<PositionDetail>();
        public readonly ObservableCollection<PositionDataSummary> PositionDataSummaryView = new ObservableCollection<PositionDataSummary>();
        public readonly ObservableCollection<MajorMarketData> MajorMarketDataView = new ObservableCollection<MajorMarketData>();
        /// <summary>
        /// 用户输入价格的时间
        /// </summary>
        public TQMain mainw;
        private Timer _TimerUserInputPrice;
        private PricingMode _OrderBoardOldPricingMode = PricingMode.Preset;
        private string notes;
        public string TradeLogs { get { return notes; } set { notes = value; NotifyPropertyChanged("TradeLogs"); } }
        private string systemlogs;
        public string SystemLogs { get { return systemlogs; } set { systemlogs = value; NotifyPropertyChanged("SystemLogs"); } }
        private string debuglogs;
        public string DebugLogs { set { debuglogs = value; NotifyPropertyChanged("DebugLogs"); } get { return debuglogs; } }

        public TQMainVM(TQMain mainwindow)
        {
            mainw = mainwindow;
             var model = new TQMainModel();
            _TimerUserInputPrice = new Timer();            
            _TimerUserInputPrice.Interval = _UserInputPriceDelaySeconds * 8000;
            _TimerUserInputPrice.Elapsed += _TimerUserInputPrice_Elapsed;
            TQMainModel.dicCustomProduct.OnDataUpdated += CustomProductUpdated;
            TQMainModel.dicMajorMarketData.OnDataUpdated += MarketDataUpdated;        
        }
        public void MarketDataUpdated(MajorMarketData fmd)
        {
            //更新自定义品种行情
            
            IEnumerable<string> CustContains = TQMainModel.dicCustomProduct.Keys.Where(x => x.Contains(fmd.InstrumentID));
            if (TQMainModel.dicCustomProduct.Count > 0 && CustContains.Count() > 0)
            {/*
                CustomProduct _customproduct;

                int ii;
                foreach (var v in CustContains)
                {
                    if ((ii = Trader.CurrInstrumentIDGroupIDLIst.FindIndex(x => x == v)) < 0) continue;
                    else
                    {
                        dicCustomProduct[v].AskPrice1 = 0;
                        dicCustomProduct[v].AskSize1 = 0;
                        dicCustomProduct[v].BidPrice1 = 0;
                        dicCustomProduct[v].BidSize1 = 0;
                        dicCustomProduct[v].OpenPrice = 0;
                        dicCustomProduct[v].PreClosePrice = 0;
                        dicCustomProduct[v].PreSettlementPrice = 0;
                        dicCustomProduct[v].LastPrice = 0;
                        dicCustomProduct[v].updateTime = DepthDataUpdated.updateTime;
                        dicCustomProduct[v].UpdateMillisec = DepthDataUpdated.updateMillisec;
                        dicCustomProduct[v].ItemIndex = ii;
                    }                }*/
            }
            //条件单触发机制   controller.eventTrigger(DataUpdated);
        }

        private void CustomProductUpdated(ComboMarketData DataUpdated)
        {
            //QTicks.Enqueue(DataUpdated);
        }
        private void _TimerUserInputPrice_Elapsed(object sender, ElapsedEventArgs e)
        {
            _TimerUserInputPrice.Stop();
            _OrderBoardOldPricingMode = CurrentMarketData.OrderBoardPricingMode;
            /*CurrentMarketData.OrderBoardPriceString = CurrentMarketData.LastPrice.ToString();*/                                                  
            mainw.Dispatcher.Invoke(
                new Action(() => 
             mainw.scrollbarPrice.Value = (double)mainw.Viewmodel.CurrentMarketData.LastPrice));
           
            //打开更新开关
            CurrentMarketData.OrderBoardPriceCanBeRefresh = true;
            CurrentMarketData.RefreshOrderBoardPrice();
        }
        /*public void RefreshOrderBoardPrice(TQMain main)
        {
            _TimerUserInputPrice.Stop();
            CurrentMarketData.OrderBoardPriceCanBeRefresh = true;       
            CurrentMarketData.RefreshOrderBoardPrice();
            main.Dispatcher.BeginInvoke(new Action(()=>main.scrollbarPrice.Value = CurrentMarketData.LastPrice));
        }*/
        public void UserJustInputOrderBoardPrice()
        {            
            CurrentMarketData.OrderBoardPriceCanBeRefresh = false;
            CurrentMarketData.OrderBoardPricingMode = PricingMode.Preset;
            _TimerUserInputPrice.Stop();
            _TimerUserInputPrice.Start();           
        }

        /// <summary>
        /// 排队价、对方价 时点击滚动条
        /// 存在一个问题:
        /// 点击向下键时不起作用，因为这时“自动”会被视为0，不能再向下变动了。
        /// 所以结合
        /// “用户鼠标进入输入框就显示当前价”功能
        /// 就可正常使用
        /// </summary>
        /// <param name="e"></param>
        public void UserJustScrollOrderBoardPrice(System.Windows.Controls.Primitives.ScrollEventArgs e)
        {
            CurrentMarketData.OrderBoardPriceCanBeRefresh = false;
            CurrentMarketData.RefreshOrderBoardPrice();
            CurrentMarketData.OrderBoardPricingMode = PricingMode.Preset;
            _TimerUserInputPrice.Stop();
            _TimerUserInputPrice.Start();
        }
        /// <summary>
        /// 用户鼠标移入价格输入框时，
        /// 若价格模式不是“限定价”时，改为限定价，并且根据最新价刷新ScrollBarPrice内容（Text和Value）
        /// 已经是“限定价”时不做任何处理
        /// </summary>
        public void UserMouseEnterOrderBoardPrice()
        {
            if (CurrentMarketData.OrderBoardPricingMode != PricingMode.Preset && !CurrentMarketData.IsCombo)
            {
                //_OrderBoardOldPricingMode = CurrentMarketData.OrderBoardPricingMode;
                //CurrentMarketData.OrderBoardPricingMode = PricingMode.Preset;               
                //CurrentMarketData.OrderBoardPriceCanBeRefresh = true;
                //CurrentMarketData.RefreshOrderBoardPrice();
            }
            else //如果是自定义组合
            {
                //_OrderBoardOldPricingMode = PricingMode.Market;
                //CurrentMarketData.OrderBoardPricingMode = PricingMode.Preset;
            }
        }
        /*public void UserMouseOutOrderBoardPrice()
        {
            if (CurrentMarketData.OrderBoardPriceCanBeRefresh //用户未修改价格
                && PricingMode.Preset != _OrderBoardOldPricingMode)
            {
                //CurrentMarketData.OrderBoardPricingMode = _OrderBoardOldPricingMode;
                //_OrderBoardOldPricingMode = PricingMode.Preset;            
                //CurrentMarketData.RefreshOrderBoardPrice();
            }
        }*/
        
        private MajorMarketData _CurrentMarketData;
        public MajorMarketData CurrentMarketData
        {
            get { return _CurrentMarketData; }
            set
            {
                if (_CurrentMarketData !=null)
                    _CurrentMarketData.IsCurrent = false;
                
                if (value != null)
                {
                    _CurrentMarketData = value;
                    _CurrentMarketData.IsCurrent = true;
                    mainw.Dispatcher.Invoke(new Action(() => mainw.scrollbarPrice.Value = (double)value.LastPrice));
                    if (_CurrentMarketData.IsCombo)
                    {
                        //_CurrentMarketData.OrderBoardOrderMode = OrderMode.Open;
                        _CurrentMarketData.OrderBoardPricingMode = PricingMode.Market;
                    }
                    NotifyPropertyChanged("CurrentMarketData");
                    NotifyPropertyChanged("OrderBoardPriceString");                    
                }               
            }
        }
    }
}
