using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Media;
using CryptoTrading.Model;
using CryptoTrading.ViewModel;

namespace CryptoTrading.Model
{
    public class MajorMarketData : INotifyPropertyChanged
    {
        const double _MaxPrice = 10000000;
        public MajorMarketData()
        { }
        public MajorMarketData(FutureMarketData fmd)
        {
            InstrumentID = fmd.channel.Substring(17, 3) + fmd.contractId.ToString().Substring(2, 6);
            Volume = (decimal)fmd.vol;
            AskPrice1 = (decimal)fmd.sell;
            BidPrice1 = (decimal)fmd.buy;
            LastPrice = (decimal)fmd.last;
            LimitHigh = (decimal)fmd.limitHigh;
            LimitLow = (decimal)fmd.limitLow;
            ContractValue = (decimal)fmd.unitAmount;
            ContractId = fmd.contractId;
            HighestPrice = (decimal)fmd.high;
            LowestPrice = (decimal)fmd.low;
            OpenInterest = fmd.hold_amount;
            UpdateTime = fmd.updateTime;
            ExchangeID = EnuExchangeID.OkexFutures;
            BaseName = Utility.GetBaseName(InstrumentID);
            Change = 0;
        }
        public MajorMarketData(string inst)
        {
            if (inst == null || inst.Length == 0)
            {
                Utility.WriteMemLog("InstrumentID is null");
                return;
            }
            InstrumentID = inst;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public string InstrumentID { get; set; }
        private string baseName;
        public string BaseName
        { get { return baseName; } set { baseName = value; NotifyPropertyChanged("BaseName"); } }
        private decimal limitHigh;
        public decimal LimitHigh//limitHigh(string):最高买入限制价格        
        {
            get { return limitHigh; }
            set
            {
                if (limitHigh != value)
                {
                    limitHigh = value > 10000000 ? 0 : value;
                    //NotifyPropertyChanged("LimitHigh");
                }
            }
        }
        private decimal limitLow;
        public decimal LimitLow
        {
            get { return limitLow; }
            set
            {
                if (limitLow != value)
                {
                    limitLow = value > 100000000 ? 0 : value;
                    //NotifyPropertyChanged("LimitLow");
                }
            }
        }//limitLow(string):最低卖出限制价格  
        private decimal vol;
        public decimal Volume //vol(double):24小时成交量       
        {
            get
            {
                return vol;
            }
            set
            {
                if (vol != value)
                {
                    vol = value;
                    //NotifyPropertyChanged("Volume");
                }
            }
        }
        internal decimal last;
        public virtual decimal LastPrice
        {
            set
            {
                Change = Math.Round(value - last, 2);//_last = value;
                last = value;
                NotifyPropertyChanged("LastPrice");
                if (_OrderBoardPriceCanBeRefresh && IsCurrent)
                {
                    RefreshOrderBoardPrice();
                }
                NotifyPropertyChanged("ChangeForegroundBrush");
            }
            get { return last; }
        }
        private decimal sell;
        public virtual decimal AskPrice1
        {
            get { return sell; }
            set
            {
                sell = value > 10000000 ? 0 : value;
                NotifyPropertyChanged("AskPrice1");
            }
        }
        public decimal ContractValue { get; set; }//unitAmount(double):合约价值
                                                  //private double BidPrice1;
        private decimal buy;
        public virtual decimal BidPrice1
        {
            get { return buy; }
            set
            {
                buy = value;
                NotifyPropertyChanged("BidPrice1");
            }
        }//buy(double):买一价格 
        private decimal hold_amount;
        public decimal OpenInterest
        {
            get { return hold_amount; }
            set
            {
                if (hold_amount != value)
                {
                    hold_amount = value;
                    //NotifyPropertyChanged("OpenInterest");
                }
            }
        }//hold_amount(double):当前持仓量
        private long contractId;
        public long ContractId
        {
            get { return contractId; }
            set
            {
                if (contractId != value)
                {
                    contractId = value;
                    //NotifyPropertyChanged("ContractId");
                }
            }
        }
        private decimal high;
        public decimal HighestPrice
        {
            get { return high > 10000000 ? 0 : high; }
            set
            {
                if (high != value)
                {
                    high = value > 1000000 ? 0 : value;
                    //NotifyPropertyChanged("HighestPrice");
                }
            }
        } //high(double):24小时最高价格
          //private double LowestPrice;
        private decimal low;
        public decimal LowestPrice
        {
            get { return low > 1000000 ? 0 : low; }
            set
            {
                if (low != value)
                {
                    low = value > 1000000 ? 0 : value;
                    //NotifyPropertyChanged("LowestPrice");
                }
            }
        }  //low(double):24小时最低价格
        private decimal bidSize1;
        public virtual decimal BidSize1
        {
            get { return bidSize1; }
            set
            {
                if (bidSize1 != value)
                {
                    bidSize1 = value > 10000 * 10000 ? 0 : value;
                    NotifyPropertyChanged("BidSize1");
                }
            }
        }
        private decimal bidSize2;
        public virtual decimal BidSize2
        {
            get { return bidSize2; }
            set
            {
                if (bidSize2 != value)
                {
                    bidSize2 = value > 10000 * 10000 ? 0 : value;
                    //NotifyPropertyChanged("BidSize2");
                }
            }
        }
        private decimal bidSize3;
        public virtual decimal BidSize3
        {
            get { return bidSize3; }
            set
            {
                if (bidSize3 != value)
                {
                    bidSize3 = value > 10000000 ? 0 : value;
                    //NotifyPropertyChanged("BidSize3");
                }
            }
        }
        private decimal bidSize4;
        public virtual decimal BidSize4
        {
            get { return bidSize4; }
            set
            {
                if (bidSize4 != value)
                {
                    bidSize4 = value > 10000000 ? 0 : value;
                    //NotifyPropertyChanged("BidSize4");
                }
            }
        }
        private decimal bidSize5;
        public virtual decimal BidSize5
        {
            get { return bidSize5; }
            set
            {
                if (bidSize5 != value)
                {
                    bidSize5 = value > 1000000 ? 0 : value;
                    //NotifyPropertyChanged("BidSize5");
                }
            }
        }
        private decimal askSize1;
        public virtual decimal AskSize1
        {
            get { return askSize1; }
            set
            {
                if (askSize1 != value)
                {
                    askSize1 = value > 100000000 ? 0 : value;
                    NotifyPropertyChanged("AskSize1");
                }
            }
        }
        private decimal askSize2;
        public virtual decimal AskSize2
        {
            get { return askSize2; }
            set
            {
                if (askSize2 != value)
                {
                    askSize2 = value > 100000000 ? 0 : value;
                    //NotifyPropertyChanged("AskSize2");
                }
            }
        }
        private decimal askSize3;
        public virtual decimal AskSize3
        {
            get { return askSize3; }
            set
            {
                if (askSize3 != value)
                {
                    askSize3 = value > 1000000 ? 0 : value;
                    //NotifyPropertyChanged("AskSize3");
                }
            }
        }
        private decimal askSize4;
        public virtual decimal AskSize4
        {
            get { return askSize4; }
            set
            {
                if (askSize4 != value)
                {
                    askSize4 = value > 1000000 ? 0 : value;
                    //NotifyPropertyChanged("AskSize4");
                }
            }
        }
        private decimal askSize5;
        public virtual decimal AskSize5
        {
            get { return askSize5; }
            set
            {
                if (askSize5 != value)
                {
                    askSize5 = value > 1000000 ? 0 : value;
                    //NotifyPropertyChanged("AskSize5");
                }
            }
        }
        private decimal bidPrice2;
        public virtual decimal BidPrice2
        {
            get { return bidPrice2; }
            set
            {
                if (bidPrice2 != value)
                {
                    bidPrice2 = value > 10000000 ? 0 : value;
                    //NotifyPropertyChanged("BidPrice2");
                }
            }
        }
        private decimal bidPrice3;
        public virtual decimal BidPrice3
        {
            get { return bidPrice3; }
            set
            {
                if (bidPrice3 != value)
                {
                    bidPrice3 = value > 10000000 ? 0 : value; //NotifyPropertyChanged("BidPrice3");
                }
            }
        }
        private decimal bidPrice4;
        public virtual decimal BidPrice4
        {
            get { return bidPrice4; }
            set
            {
                if (bidPrice4 != value)
                {
                    bidPrice4 = value > 10000000 ? 0 : value; //NotifyPropertyChanged("BidPrice4");
                }
            }
        }
        private decimal bidPrice5;
        public virtual decimal BidPrice5
        {
            get { return bidPrice5; }
            set
            {
                if (bidPrice5 != value)
                {
                    bidPrice5 = value > 10000000 ? 0 : value; //NotifyPropertyChanged("BidPrice5");
                }
            }
        }
        private decimal askPrice2;
        public virtual decimal AskPrice2
        {
            get { return askPrice2; }
            set
            {
                if (askPrice2 != value)
                {
                    askPrice2 = value > 10000000 ? 0 : value; //NotifyPropertyChanged("AskPrice2");
                }
            }
        }
        private decimal askPrice3;
        public virtual decimal AskPrice3
        {
            get { return askPrice3; }
            set
            {
                if (askPrice3 != value)
                {
                    askPrice3 = value > 10000000 ? 0 : value; //NotifyPropertyChanged("AskPrice3");
                }
            }
        }
        private decimal askPrice4;
        public virtual decimal AskPrice4
        {
            get { return askPrice4; }
            set
            {
                if (askPrice4 != value)
                {
                    askPrice4 = value > 10000000 ? 0 : value; //NotifyPropertyChanged("AskPrice4");
                }
            }
        }
        private decimal askPrice5;
        public virtual decimal AskPrice5
        {
            get { return askPrice5; }
            set
            {
                if (askPrice5 != value)
                {
                    askPrice5 = value > 10000000 ? 0 : value; //NotifyPropertyChanged("AskPrice5"); 
                }
            }
        }
        private decimal _Change;
        public decimal Change
        {
            get { return _Change; }
            set
            {
                if (_Change != value)
                {
                    _Change = value;
                    //NotifyPropertyChanged("Change");
                    //NotifyPropertyChanged("ChangeForegroundBrush");
                }
            }
        }
        private string updateTime;
        public string UpdateTime
        {
            get { return updateTime; }
            set
            {
                updateTime = value; NotifyPropertyChanged("UpdateTime");
            }
        } //时间戳标识  
        public bool IsCombo { get; set; }
        public EnuExchangeID ExchangeID { get; set; }

        #region 给下单版 OrderBoard 添加属性

        public bool IsCurrent { get; set; }

        private bool _OrderBoardPriceCanBeRefresh = true;

        public bool OrderBoardPriceCanBeRefresh
        {
            get
            {
                return _OrderBoardPriceCanBeRefresh;
            }

            set
            {
                _OrderBoardPriceCanBeRefresh = value;
            }
        }
        /// <summary>
        /// 交易品种名称
        /// </summary>
        private string _OrderBoardTradeName;
        public string OrderBoardTradeName
        {
            get
            {
                if (string.IsNullOrEmpty(_OrderBoardTradeName))
                {
                    try
                    {

                        if (TQMainModel.dicInstrumentData.Count > 0)
                        {
                            if (TQMainModel.dicInstrumentData.Keys.Contains(InstrumentID))
                                _OrderBoardTradeName = TQMainModel.dicInstrumentData[InstrumentID].Name;
                            else if (TQMainModel.dicCustomProduct.Keys.Contains(InstrumentID))
                                _OrderBoardTradeName = TQMainModel.dicCustomProduct[InstrumentID].InstrumentName;
                            else
                                return InstrumentID;
                            return _OrderBoardTradeName;
                        }
                        else
                            return InstrumentID;
                    }
                    catch
                    {
                        _OrderBoardTradeName = null;
                        return InstrumentID;
                    }
                    //NotifyPropertyChanged("OrderBoardTradeName");
                }

                return _OrderBoardTradeName;
            }
            set
            {
                _OrderBoardTradeName = value;
                NotifyPropertyChanged("OrderBoardTradeName");
            }
        }
        /*
        private OrderMode _OrderBoardOrderMode = OrderMode.Open;
        /// <summary>
        /// 交易方式
        /// 平仓、开仓、自动
        /// </summary>
        public OrderMode OrderBoardOrderMode
        {
            get { return _OrderBoardOrderMode; }
            set
            {
                _OrderBoardOrderMode = value;
                //NotifyPropertyChanged("OrderBoardOrderMode");

                //NotifyPropertyChanged("OrderBoardLongButtonContent");
                //NotifyPropertyChanged("OrderBoardShortButtonContent");

                NotifyPropertyChanged("OrderBoardLongButtonToolTips");
                NotifyPropertyChanged("OrderBoardShortButtonToolTips");
            }
        }
        */
        private HedgeRatio _OrderBoardHedgeRatio = HedgeRatio.Twenty;
        /// <summary>
        /// 套保类别
        /// 投机、套利、套保
        /// </summary>
        public HedgeRatio OrderBoardHedgeRatio
        {
            get { return _OrderBoardHedgeRatio; }
            set
            {
                _OrderBoardHedgeRatio = value;
                NotifyPropertyChanged("OrderBoardHedgeRatio");
            }
        }
        /// <summary>
        /// 开仓、平仓、平昨、平今、强平、强减、今日强平
        /// </summary>
        /*
        private OffsetType _OrderBoardOffsetFlag;

        public OffsetType OrderBoardOffsetFlag
        {
            get { return _OrderBoardOffsetFlag; }
            set
            {
                _OrderBoardOffsetFlag = value;
                NotifyPropertyChanged("OrderBoardOffsetFlag");
            }
        }
        */
        private PricingMode _OrderBoardPricingMode = PricingMode.Preset;
        /// <summary>
        /// 限价（设定价，对张价，排队价）
        /// 市价
        /// </summary>
        public PricingMode OrderBoardPricingMode
        {
            get { return _OrderBoardPricingMode; }
            set
            {
                _OrderBoardPricingMode = value;
                OrderBoardPriceCanBeRefresh = (value != PricingMode.Preset) || OrderBoardPriceCanBeRefresh;   //定价方式为非预设的时候更新下单板价格
                NotifyPropertyChanged("OrderBoardPricingMode");
                if (OrderBoardPricingMode == PricingMode.Market)
                    OrderBoardPrice = last;
                NotifyPropertyChanged("OrderBoardPrice");
                NotifyPropertyChanged("OrderBoardShortPrice");
                NotifyPropertyChanged("OrderBoardLongPrice");
            }
        }
        private QuantMode _OrderBoardQuantMode = QuantMode.Default;
        /// <summary>
        /// 价格模式
        /// 默认数量、可平仓量、设定数量
        /// </summary>
        public QuantMode OrderBoardQuantMode
        {
            get { return _OrderBoardQuantMode; }
            set
            {
                _OrderBoardQuantMode = value;

                switch (_OrderBoardQuantMode)
                {
                    case QuantMode.Default:// 默认数量

                        if (InstrumentID != null)
                        {
                            string productID = Utility.GetProductID(InstrumentID);
                            _OrderBoardQuant = Trader.Configuration.GetDefaultQuant(InstrumentID, productID);

                            NotifyPropertyChanged("OrderBoardQuant");
                        }
                        break;
                    case QuantMode.AllAvailable:// 可平仓量
                        _OrderBoardQuant = OrderBoardPositionQuant;
                        NotifyPropertyChanged("OrderBoardQuant");
                        break;
                }

                NotifyPropertyChanged("OrderBoardQuantMode");
            }
        }

        //private PriceMode _OrderBoardPriceMode= PriceMode.PreSet;

        //public PriceMode OrderBoardPriceMode
        //{
        //    get {

        //        return _OrderBoardPriceMode;
        //    }
        //    set
        //    {
        //        _OrderBoardPriceMode = value;

        //        if (OrderBoardPriceMode == PriceMode.PreSet)
        //            _OrderBoardPriceModeCheckedValue = null;
        //        else
        //            _OrderBoardPriceModeCheckedValue = OrderBoardPriceMode == PriceMode.Opposite ? false : true;

        //        NotifyPropertyChanged("OrderBoardPriceMode");
        //        NotifyPropertyChanged("OrderBoardPriceModeCheckedValue");
        //    }
        //}

        //private bool? _OrderBoardPriceModeCheckedValue;
        //public bool? OrderBoardPriceModeCheckedValue
        //{
        //    get
        //    {
        //        return _OrderBoardPriceModeCheckedValue;
        //    }
        //    set
        //    {
        //        _OrderBoardPriceModeCheckedValue = value;

        //        _OrderBoardPriceMode = _OrderBoardPriceModeCheckedValue == null ? PriceMode.PreSet : _OrderBoardPriceModeCheckedValue == false ? PriceMode.Opposite : PriceMode.Ownside;

        //        NotifyPropertyChanged("OrderBoardPriceMode");
        //        NotifyPropertyChanged("OrderBoardPriceModeCheckedValue");
        //    }
        //}

        /// <summary>
        /// 下单数量，有默认值
        /// </summary>
        private int _OrderBoardQuant = -1;

        public int OrderBoardQuant
        {
            get
            {
                if (_OrderBoardQuant == -1)
                {
                    if (InstrumentID != null)
                    {
                        string productID = Utility.GetProductID(InstrumentID);
                        _OrderBoardQuant = Trader.Configuration.GetDefaultQuant(InstrumentID, productID);
                    }
                }
                return _OrderBoardQuant;
            }
            set
            {
                _OrderBoardQuant = value;

                if (OrderBoardQuantMode != QuantMode.Preset)
                {
                    OrderBoardQuantMode = QuantMode.Preset;
                }

                NotifyPropertyChanged("OrderBoardQuant");
                NotifyPropertyChanged("OrderBoardButtonQuant");

                NotifyPropertyChanged("OrderBoardLongButtonToolTips");
                NotifyPropertyChanged("OrderBoardShortButtonToolTips");
            }
        }
        private int _OrderBoardLongPositionQuant = -1;
        /// <summary>
        /// 可平仓量-多
        /// </summary>
        public string OrderBoardLongPositionQuant
        {
            get
            {
                if (Trader.Configuration != null)
                {
                    if (_OrderBoardLongPositionQuant == -1)
                    {
                        string posKey = Trader.Configuration.Investor.ID + InstrumentID;
                        PositionDataSummary posi;
                        if (TQMainModel.dicPositionSummary.TryGetValue(posKey + TradeDirection.Long, out posi))
                            _OrderBoardLongPositionQuant = (int)posi.LongAvailable;
                        else
                            _OrderBoardLongPositionQuant = 0;
                    }
                }
                return "多 " + _OrderBoardLongPositionQuant;
            }
            set
            {
                int.TryParse(value, out _OrderBoardLongPositionQuant);
                if (_OrderBoardLongPositionQuant > _OrderBoardPositionQuant)
                {
                    OrderBoardPositionQuant = _OrderBoardLongPositionQuant;
                }
                if (IsCurrent)
                {
                    NotifyPropertyChanged("OrderBoardLongPositionQuant");
                }
            }
        }
        private int _OrderBoardShortPositionQuant = -1;
        /// <summary>
        /// 可平仓量-空
        /// </summary>
        public string OrderBoardShortPositionQuant
        {
            get
            {
                if (Trader.Configuration != null)
                {
                    if (_OrderBoardShortPositionQuant == -1)
                    {
                        string posKey = Trader.Configuration.Investor.ID + InstrumentID;
                        PositionDataSummary posi;
                        if (TQMainModel.dicPositionSummary.TryGetValue(posKey + TradeDirection.Short, out posi))
                            _OrderBoardShortPositionQuant = (int)posi.ShortAvailable;
                        else
                            _OrderBoardShortPositionQuant = 0;
                    }
                }
                return "空 " + _OrderBoardShortPositionQuant;
            }
            set
            {
                int.TryParse(value, out _OrderBoardShortPositionQuant);
                if (_OrderBoardShortPositionQuant > _OrderBoardPositionQuant)
                {
                    OrderBoardPositionQuant = _OrderBoardShortPositionQuant;
                }
                if (IsCurrent)
                {
                    NotifyPropertyChanged("OrderBoardShortPositionQuant");
                }
            }
        }

        private int _OrderBoardPositionQuant = -1;
        /// <summary>
        /// 可平仓量
        /// </summary>
        public int OrderBoardPositionQuant
        {
            get
            {
                if (Trader.Configuration != null)
                {
                    if (_OrderBoardPositionQuant == -1)
                    {
                        string posKey = Trader.Configuration.Investor.ID + InstrumentID;
                        PositionDataSummary posi;
                        if (TQMainModel.dicPositionSummary.TryGetValue(posKey + TradeDirection.Long, out posi))
                            _OrderBoardPositionQuant = (int)posi.LongAvailable;
                        if (TQMainModel.dicPositionSummary.TryGetValue(posKey + TradeDirection.Short, out posi) && posi.ShortAvailable > _OrderBoardPositionQuant)
                            _OrderBoardPositionQuant = (int)posi.ShortAvailable;
                    }
                    if (_OrderBoardPositionQuant == -1)
                    {
                        _OrderBoardPositionQuant = 0;
                    }
                    return _OrderBoardPositionQuant;
                }
                else
                    return 0;
            }
            set
            {
                if (IsCurrent && _OrderBoardQuantMode == QuantMode.AllAvailable)
                    _OrderBoardQuant = OrderBoardPositionQuant;
                else
                    _OrderBoardPositionQuant = value;
                NotifyPropertyChanged("OrderBoardQuant");
                //NotifyPropertyChanged("OrderBoardAvailablePosition");
            }
        }
        public string OrderBoardLongPrice
        {
            get
            {
                switch (_OrderBoardPricingMode)
                {
                    case PricingMode.Market:
                        return "市价";
                    case PricingMode.OppositePlus:
                        return AskPrice1.ToString();
                    case PricingMode.Ownside:
                        return BidPrice1.ToString();
                    default:
                        return OrderBoardPrice.ToString();
                }
            }
        }
        public string OrderBoardShortPrice
        {
            get
            {
                switch (_OrderBoardPricingMode)
                {
                    case PricingMode.Market:
                        return "市价";
                    case PricingMode.OppositePlus:
                        return BidPrice1.ToString();
                    case PricingMode.Ownside:
                        return AskPrice1.ToString();
                    default:
                        return OrderBoardPrice.ToString();
                }
            }
        }
        /*
        private string _OrderBoardLongButtonContent;
        /// <summary>
        /// 买（做多） 下单按钮显示内容
        /// </summary>
        public string OrderBoardLongButtonContent
        {
            get
            {
                // 指定开仓
                if (_OrderBoardOrderMode == OrderMode.Open)
                {
                    // 多单有仓位
                    if (_OrderBoardLongPositionQuant > 0)
                        _OrderBoardLongButtonContent = "加多";
                    // 空单有仓位
                    else if (_OrderBoardShortPositionQuant > 0)
                        _OrderBoardLongButtonContent = "锁仓";
                    // 多单空单 均无仓位
                    else
                        _OrderBoardLongButtonContent = "买开";
                }
                // 自动开平
                else //_OrderBoardOrderMode == OrderMode.Auto;
                {
                    _OrderBoardLongButtonContent = string.Empty;
                    // 空单有仓位
                    if (_OrderBoardShortPositionQuant > 0)
                        _OrderBoardLongButtonContent = "买平";

                    // 平仓 之后还需要 做多
                    if(GetOrderBoardQty(OrderMode.Open, DirectionType.多) > 0)
                    {
                        // 已有 多仓
                        if (_OrderBoardLongPositionQuant > 0)
                            _OrderBoardLongButtonContent += "加多";
                        else
                            _OrderBoardLongButtonContent += _OrderBoardShortPositionQuant > 0 ? "再开" : "买开";
                    }
                }

                return _OrderBoardLongButtonContent;
            }
        }

        private string _OrderBoardShortButtonContent;
        /// <summary>
        /// 卖（做空） 下单按钮显示内容
        /// </summary>
        public string OrderBoardShortButtonContent
        {
            get
            {
                // 指定开仓
                if (_OrderBoardOrderMode == OrderMode.Open)
                {
                    // 空单有仓位
                    if (_OrderBoardShortPositionQuant > 0)
                        _OrderBoardShortButtonContent = "加空";
                    // 多单有仓位
                    else if (_OrderBoardLongPositionQuant > 0)
                        _OrderBoardShortButtonContent = "锁仓";
                    // 多单空单 均无仓位
                    else
                        _OrderBoardShortButtonContent = "卖开";
                }
                // 自动开平
                else //_OrderBoardOrderMode == OrderMode.Auto;
                {
                    _OrderBoardShortButtonContent = string.Empty;
                    // 多单有仓位
                    if (_OrderBoardLongPositionQuant > 0)
                        _OrderBoardShortButtonContent = "卖平";

                    // 平仓 之后还需要 做空
                    if (GetOrderBoardQty(OrderMode.Open, DirectionType.空) > 0)
                    {
                        // 已有 空仓
                        if (_OrderBoardShortPositionQuant > 0)
                            _OrderBoardShortButtonContent += "加空";
                        else
                            _OrderBoardShortButtonContent += _OrderBoardLongPositionQuant > 0 ? "再开" : "卖开";
                    }
                }

                return _OrderBoardShortButtonContent;
            }
        }
        */

        /// <summary>
        /// 下单价
        /// </summary>
        private decimal _OrderBoardPrice = 0;
        public decimal OrderBoardPrice
        {
            set { _OrderBoardPrice = value; NotifyPropertyChanged("OrderBoardPrice"); }
            get
            {
                return _OrderBoardPrice;
            }
        }


        private TradeDirection _OrderBoardTradeDirection;

        public TradeDirection OrderBoardTradeDirection
        {
            get { return _OrderBoardTradeDirection; }
            set
            {
                _OrderBoardTradeDirection = value;
                NotifyPropertyChanged("OrderBoardTradeDirection");
            }
        }

        public string OrderBoardInvestorID
        {
            get
            {
                return null;//TQMain.T.Investor;
            }
            set { }
        }

        /*private string _OrderBoardExchangeID;
        public string OrderBoardExchangeID
        {
            set { _OrderBoardExchangeID = value; }
            get
            {
                if (_OrderBoardExchangeID == null)
                    return Utility.GetExchangeID(InstrumentID).ToString();
                else return _OrderBoardExchangeID;
            }
        }*/
        /*private string _OrderBoardExchangeName;
        public string OrderBoardExchangeName
        {
            get
            {
                if (_OrderBoardExchangeName == null)
                    return Utility.GetExchangeName(InstrumentID);
                else
                    return _OrderBoardExchangeName;
            }
            set { _OrderBoardExchangeName = value; }
        }*/
        protected double _PriceTick = 0;
        public virtual double PriceTick
        {
            get
            {
                try
                {
                    if (TQMainModel.dicInstrumentData.Keys.Contains(InstrumentID))
                        _PriceTick = TQMainModel.dicInstrumentData[InstrumentID].PriceTick;
                }
                catch { }

                return _PriceTick;
            }
            set
            {
                _PriceTick = value;
                NotifyPropertyChanged("PriceTick");
            }
        }
        private SolidColorBrush _ChangeForegroundBrush;
        public virtual SolidColorBrush ChangeForegroundBrush
        {
            get
            {
                if (Change > 0)
                {
                    _ChangeForegroundBrush = new SolidColorBrush(Trader.Configuration.ColorSetModelObj.ChangeUpForeground.Color);
                    //_ChangeForegroundBrush = new SolidColorBrush(Color.FromRgb(246, 1, 0));//#F60100 闪电王 涨 颜色
                }
                else if (Change < 0)
                {
                    _ChangeForegroundBrush = new SolidColorBrush(Trader.Configuration.ColorSetModelObj.ChangeDownForeground.Color);
                    //_ChangeForegroundBrush = new SolidColorBrush(Color.FromRgb(0, 153, 1));//#009901 闪电王 跌 颜色
                }
                else if (Change == 0 && LastPrice != 0)
                {
                    _ChangeForegroundBrush = new SolidColorBrush(Trader.Configuration.ColorSetModelObj.ChangeStableForeground.Color);
                    //_ChangeForegroundBrush = new SolidColorBrush(Color.FromRgb(255, 0, 255));//#FF00FF 闪电王 变涨临时 颜色
                }
                else
                {
                    _ChangeForegroundBrush = new SolidColorBrush(Trader.Configuration.ColorSetModelObj.GridForeground.Color);
                    //_ChangeForegroundBrush = new SolidColorBrush(Color.FromRgb(49, 49, 49));//#313131 闪电王 正常 颜色
                }
                return _ChangeForegroundBrush;
            }
        }
        #endregion

        #region 给下单版 OrderBoard 添加方法

        /// <summary>
        /// 重置下单版
        /// </summary>
        public void ResetOrderBoard()
        {
            OrderBoardHedgeRatio = HedgeRatio.Twenty;
            //OrderBoardOrderMode = OrderMode.Auto;
            OrderBoardPricingMode = PricingMode.Preset; // 会自动设置 OrderBoardPriceCanBeRefresh = true;
            OrderBoardQuantMode = QuantMode.Default;
            //OrderBoardQuant = 0; // 设置 OrderBoardQuantMode 即会设置 OrderBoardQuant
        }
        public void RefreshOrderBoardPrice()
        {
            //根据情况更新价格
            if (!OrderBoardPriceCanBeRefresh)
                return;
            switch (OrderBoardPricingMode)
            {
                case PricingMode.Preset:
                case PricingMode.OppositePlus:
                case PricingMode.Ownside:   //如果采取对手价，排队价，以最新价更新
                    if (OrderBoardPrice != LastPrice)
                    {
                        OrderBoardPrice = LastPrice;
                    }
                    break;
                case PricingMode.Market:
                    OrderBoardPrice = 0;
                    break;
                case PricingMode.MiddlePrice:
                    OrderBoardPrice = Math.Round((BidPrice1 + AskPrice1) / 2, 4);
                    break;
            }
            //NotifyPropertyChanged("OrderBoardPriceString");
        }

        public int GetOrderBoardQty()
        {
            if (OrderBoardTradeDirection == TradeDirection.Long)
            {
                return (int)Math.Min(AskSize1, _OrderBoardQuant);
            }
            else
            {
                return (int)Math.Min(BidSize1, _OrderBoardQuant);
            }
        }
    }
    #endregion

}
