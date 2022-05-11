using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using CryptoTrading.View;

namespace CryptoTrading.Model
{
    /// <summary>
    /// 组合或策略 初始下单参数
    /// </summary>
    public class OrderParams
    {
        /// <summary>
        /// 4位的 序列号
        /// 每一笔用户下单分配一个（可能会有多笔实际报单共用同一个序列号）
        /// </summary>
        [XmlAttribute]
        public string OrderRefPrefix { get; set; }
        /// <summary>
        /// 手工：Manual
        //  策略：GUID（不是 4位的 序列号）
        /// </summary>
        [XmlAttribute]
        public string StrategyId { get; set; }
        [XmlAttribute]
        public bool IsCombo { get; set; }
        private string _InstrumentID;
        /// <summary>
        /// 合约
        /// </summary>
        [XmlAttribute]
        public string InstrumentID
        {
            get { return _InstrumentID; }
            set
            {
                _InstrumentID = value;
            }
        }
        private OrderMode _OrderMode = OrderMode.Auto;
        /// <summary>
        /// 交易方式
        /// 平仓、开仓、自动
        /// </summary>
        [XmlAttribute]
        public OrderMode OrderMode
        {
            get { return _OrderMode; }
            set
            {
                _OrderMode = value;
            }
        }
        private HedgeType _HedgeType = HedgeType.投机;
        /// <summary>
        /// 套保类别
        /// 投机、套利、套保
        /// </summary>
        [XmlAttribute]
        public HedgeType HedgeType
        {
            get { return _HedgeType; }
            set
            {
                _HedgeType = value;
            }
        }
        private LimitMarket _PriceType = LimitMarket.限价;
        /// <summary>
        /// 限价（设定价，对张价，排队价）
        /// 市价
        /// </summary>
        [XmlAttribute]
        public LimitMarket priceType
        {
            get { return _PriceType; }
            set
            {
                _PriceType = value;
            }
        }
        /// <summary>
        /// FAK / FOK
        /// </summary>
        private FakFok _FakFok = FakFok.Default;
        [XmlAttribute]
        public FakFok FakFok
        {
            get { return _FakFok; }
            set
            {
                _FakFok = value;
            }
        }
        /// <summary>
        /// 下单数量，有默认值
        /// </summary>
        private int _Quant = -1;
        [XmlAttribute]
        public int Quant
        {
            get
            {
                return _Quant;
            }
            set
            {
                _Quant = value;
            }
        }
        /// <summary>
        /// 下单价
        /// </summary>
        private double _Price;
        [XmlAttribute]
        public double Price
        {
            get
            {
                return _Price;
            }
            set
            {
                _Price = value;
            }
        }
        private TradeDirection _TradeDirection;
        /// <summary>
        /// 方向（多、空）
        /// </summary>
        [XmlAttribute]
        public TradeDirection TradeDirection
        {
            get { return _TradeDirection; }
            set
            {
                _TradeDirection = value;
            }
        }
        [XmlAttribute]
        public string InvestorID { get; set; }
        /// <summary>
        /// 交易者ID
        /// </summary>
        private string _ExchangeID;
        [XmlAttribute]
        public string ExchangeID
        {
            set { _ExchangeID = value; }
            get
            {
                if (_ExchangeID == null)
                    return Utility.GetExchangeID(InstrumentID).ToString();
                else return _ExchangeID;
            }
        }
        protected double _PriceTick = 0;
        /// <summary>
        /// 一跳的点数
        /// </summary>
        [XmlAttribute]
        public virtual double PriceTick
        {
            get
            {
                return _PriceTick;
            }
            set
            {
                _PriceTick = value;
            }
        }
        /// <summary>
        /// 已成交数量
        /// </summary>
        [XmlAttribute]
        public int VolumeTraded { get; set; }
    }
}
