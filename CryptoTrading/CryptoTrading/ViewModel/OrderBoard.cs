using CryptoTrading.Model;
using System;
using System.ComponentModel;

namespace CryptoTrading.ViewModel
{
    public class OrderBoard : INotifyPropertyChanged
    {
        ///
        /// 数量模式：可平，默认，设定
        ///
        public QuantMode quantMode
        {
            get;
            set;
        }

        /// <summary>
        /// 交易品种名称
        /// </summary>
        private string _tradename;
        public string TradeName
        {
            set
            {
                //if (value == null)
                //{
                //    _tradename = "";
                //    return;
                //}
                ////CurrentInstrument = value;
                //string inst;
                //char[] c = value.ToCharArray();
                //int i, ii = c.GetLength(0);
                //for (i = 0; i < ii; i++)
                //{
                //    if (char.IsDigit(c[i])) break;
                //}
                //string s = InstrumentID.Substring(0, i);

                ////if (TQMainVM.dicInstrumentData.TryGetValue(CurrentInstrument, out inst))
                //if (TQMain.dicInstrumentIDName.TryGetValue(s, out inst) || TQMain.dicInstrumentIDName.TryGetValue(InstrumentID, out inst))
                //    _tradename = inst;
                //else
                //{
                //    CustomProduct cp;
                //    if (TQMainVM.dicCustomProduct.TryGetValue(InstrumentID, out cp))
                //        _tradename = cp.InstrumentName;
                //    else _tradename = InstrumentID;
                //}
                NotifyPropertyChanged("TradeName");
            }
            get
            {
                return _tradename;
            }
        }

        //trade direction enum: Buy, Sell
        public TradeDirection TradeDirection { get; set; }
        public string InvestorID { get; set; }

        //private string exchangeID;
        //public string ExchangeID { set { exchangeID = value; }
        //    get { if (exchangeID == null) return Utility.GetExchangeID(_InstrumentID); else return exchangeID; } }


        //private string exchangeName;
        //public string ExchangeName { get { if (exchangeName == null) return Utility.GetExchangeName(_InstrumentID); else return exchangeName; }
        //    set { ExchangeName = value; } }

        //order limit price
        private double _orderprice;
        public double OrderPrice
        {
            get
            {
                return Math.Round(_orderprice, 3);
            }
            set
            {
                _orderprice = value;
                if (PropertyChanged != null)
                {
                    NotifyPropertyChanged("OrderPrice");
                }
            }
        }
        /// <summary>
        /// enum Limit Market FOK FAK
        /// </summary>
        private PricingMode ordertype;
        public PricingMode OrderType
        {
            get { return ordertype; }
            set
            {
                if (ordertype != value)
                {
                    ordertype = value;
                    if (PropertyChanged != null) { NotifyPropertyChanged("orderType"); }
                }
            }
        }
        //order quantity
        private int _quant;
        public int Quant
        {
            get { return _quant; }
            set
            {
                if (value != _quant)
                {
                    _quant = value;
                    if (PropertyChanged != null)
                    {
                        NotifyPropertyChanged("Quant");
                    }
                }
            }
        }
        //Order Valid DateTime Value
        public DateTime GoodTill { get; set; }
        ///
        /// 投机 套保 套利
        /// if it is a Hedge order : Speculation, Hedge, Arbitrage
        private HedgeType _hedgetype;
        public HedgeType HedgeType
        {
            get
            {
                return _hedgetype;
            }
            set
            {
                if (value != _hedgetype)
                {
                    _hedgetype = value;
                    if (PropertyChanged != null)
                    {
                        NotifyPropertyChanged("hedgeType");
                    }
                }
            }
        }
       
        /// <summary>
        /// 自定义的定单模式：平仓、平今、自动确定
        /// </summary>
        private OrderMode ordermode;
        //enum: Close, CloseToday, Auto
        public OrderMode orderMode
        {
            get
            {
                return ordermode;
            }
            set
            {
                if (value != ordermode)
                {
                    ordermode = value;
                    if (PropertyChanged != null)
                    {
                        NotifyPropertyChanged("orderMode");
                    }
                }
            }
        }
        ///// <summary>
        ///// 自定义的价格模式：排队价Ownside、对手价Opposite、指定价Assigned
        ///// </summary>
        //private PriceMode pricemode;
        ////Assigned Opposite Ownside
        //public PriceMode priceMode
        //{
        //    get
        //    {
        //        return pricemode;
        //    }
        //    set
        //    {
        //        if (value != pricemode)
        //        {
        //            pricemode = value;
        //            if (PropertyChanged != null)
        //            {
        //                NotifyPropertyChanged("priceMode");
        //            }
        //        }
        //    }
        //}
        public bool ScrollBarPriceRefresh { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        private double bidPrice1;
        public double BidPrice1
        {
            get
            {
                return bidPrice1;
            }
            set
            {
                if (bidPrice1 != value)
                {
                    bidPrice1 = value;
                    if (PropertyChanged != null)
                    {
                        NotifyPropertyChanged("BidPrice1");
                    }
                }
            }
        }
        private double askPrice1;
        public double AskPrice1
        {
            get { return askPrice1; }
            set
            {
                if (AskPrice1 != value)
                {
                    askPrice1 = value;
                    if (PropertyChanged != null)
                        NotifyPropertyChanged("AskPrice1");
                }
            }
        }
        private double lowerLimitPrice;
        public double LowerLimitPrice
        {
            get { return lowerLimitPrice; }
            set
            {
                if (lowerLimitPrice != value)
                {
                    lowerLimitPrice = value;
                    if (PropertyChanged != null)
                        NotifyPropertyChanged("LowerLimitPrice");
                }
            }
        }
        private double upperLimitPrice;
        public double UpperLimitPrice
        {
            get { return upperLimitPrice; }
            set
            {
                if (upperLimitPrice != value)
                {
                    upperLimitPrice = value;
                    if (PropertyChanged != null)
                        NotifyPropertyChanged("UpperLimitPrice");
                }
            }
        }
        private double askSize1;
        public double AskSize1
        {
            get { return askSize1; }
            set
            {
                askSize1 = value;
                if (PropertyChanged != null)
                    NotifyPropertyChanged("AskSize1");
            }
        }
        private double bidSize1;
        public double BidSize1
        {
            get { return bidSize1; }
            set
            {
                bidSize1 = value;
                if (PropertyChanged != null)
                    NotifyPropertyChanged("BidSize1");
            }
        }
    }
}
