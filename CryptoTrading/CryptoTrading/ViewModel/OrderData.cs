using CryptoTrading.Model;
using System.ComponentModel;

namespace CryptoTrading.Model
{
    /// <summary>
    /// 委托单信息
    /// </summary>
    public class OrderData : INotifyPropertyChanged
    {              
        #region Properties            
        /// <summary>
        /// 投资者代码
        /// </summary>
        public string InvestorID { get; set; }
        /// <summary>
        /// 合约代码
        /// </summary>
        public string InstrumentID { get; set; }    
        public string ClientOID { get; set; }
        /// <summary>
        /// 报单价格类型
        /// </summary>
        private char _OrderPriceType;
        public OrderPriceType OrderPriceType
        {
            get { return (OrderPriceType)_OrderPriceType; }
            set
            {
                switch (value)
                {
                    default:
                    case OrderPriceType.市价:
                        _OrderPriceType = '1';
                        break;
                    case OrderPriceType.限价:
                        _OrderPriceType = '2';
                        break;
                    case OrderPriceType.停损价:
                        _OrderPriceType = '3';
                        break;
                    case OrderPriceType.停损限价:
                        _OrderPriceType = '4';
                        break;
                }
            }
        }
        /// <summary>
        /// 多空方向
        /// </summary>
        //private char _Direction;
        public TradeDirection Direction
        {
            get;
            set;  
        }                   
        /// <summary>
        /// 组合开平标志
        /// </summary>
        public OffsetType Offset
        {
            get;
            set;
        }
        public decimal Commission
        { get; set; }
        /// <summary>
        /// 组合投机套保标志
        /// </summary>
        public HedgeType Hedge
        {
            get;
            set;
        }
        /// <summary>
        /// 报单价格
        /// </summary>
        public decimal OrderPrice { get; set;}
       /// <summary>
       /// 成交价格
       /// </summary>       
        public decimal AvgPrice { get; set; }
        /// <summary>
        /// 原始报单数量
        /// </summary>
        public decimal OrderSize { get; set; }                   
        /// <summary>
        /// 触发条件
        /// </summary>
        public ContingentConditionType ContingentCondition
        {
            get;
            set;
        }
        /// <summary>
        /// 止损价
        /// </summary>
        public double StopPrice { get; set; }

        /// <summary>
        /// 交易所代码
        /// </summary>
        public EnuExchangeID ExchangeID { get; set;}

        /// <summary>
        /// 报单提交状态
        /// </summary>
        private char _OrderSubmitStatus;
        public OrderSubmitStatusType OrderSubmitStatus
        {
            get { return (OrderSubmitStatusType)_OrderSubmitStatus; }
            set { _OrderSubmitStatus = (char)value; NotifyPropertyChanged("OrderSubmitStatus"); }
        }
        /// <summary>
        /// 报单状态
        /// </summary>
        private char _OrderStatus;
        public OrderStatusType OrderStatus
        {
            get { return (OrderStatusType)_OrderStatus; }
            set
            {
                _OrderStatus = (char)value;
                NotifyPropertyChanged("OrderStatus");
                NotifyPropertyChanged("UnsettledOrderVisibility");
                NotifyPropertyChanged("SettledOrderVisibility");
                NotifyPropertyChanged("CanceledOrderVisibility");
            }
        }
        /// <summary>
        /// 报单类型
        /// </summary>
        public OrderTypeType OrderType { get; set; }
        /// <summary>
        /// 已成交数量
        /// </summary>
        private decimal quantfilled;
        public decimal QuantFilled
        {
            get { return quantfilled; }
            set { quantfilled = value; NotifyPropertyChanged("QuantFilled"); }
        }
        /// <summary>
        /// 剩余数量
        /// </summary>
        private decimal quantunfilled;
        public decimal QuantUnfilled
        {
            get { return quantunfilled; }
            set
            {
                quantunfilled = value;
                NotifyPropertyChanged("QuantUnfilled");
            }
        }       
        //public string CreateDate { get; set; }
        /// <summary>
        /// 委托时间
        /// </summary>
        private string createtime;
        public string CreateTime { get { return createtime; } set { createtime = value; } }
        private string updatetime;
        public string UpdateTime { get { return updatetime; }
            set { updatetime = value;
                NotifyPropertyChanged("UpdateTime");
            }
        }
        /// <summary>
        /// 撤销时间
        /// </summary>
        public string CancelTime
        {
            get;
            set;
        }         
        public decimal PnL { get; set; }
        /// <summary>
        /// 状态信息
        /// </summary>
        public string StatusMsg
        {
            get;
            set;
        }
        public double LeverRate { get; set; }
        #endregion
              
        public string CanceledOrderVisibility
        {
            get
            {
                switch (OrderStatus)
                {
                    case OrderStatusType.已撤单:                    
                        return "Visible";
                    case OrderStatusType.未成交:
                    case OrderStatusType.部分成交:
                    default:
                        return "Collapsed";
                }
            }
        }
        public string UnsettledOrderVisibility
        {
            get
            {
                switch (OrderStatus)
                {
                    case OrderStatusType.已撤单:
                    case OrderStatusType.全部成交:
                        return "Collapsed";
                    default:
                    case OrderStatusType.尚未触发:
                    case OrderStatusType.未成交:
                    case OrderStatusType.未知:
                    case OrderStatusType.部分成交:
                        return "Visible";
                }
            }
        }
        private string _SettledOrderVisibility;

        public string SettledOrderVisibility
        {
            get
            {
                switch (OrderStatus)
                {
                    case OrderStatusType.全部成交:
                    case OrderStatusType.部分成交:                   
                        _SettledOrderVisibility = "Visible";
                        break;
                    default:
                        _SettledOrderVisibility = "Collapsed";
                        break;
                }
                return _SettledOrderVisibility;
            }
            set
            {
                _SettledOrderVisibility = value;
                NotifyPropertyChanged("UnsettledOrderVisibility");
            }
        }
        public string OrderID { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }        
    }
}
