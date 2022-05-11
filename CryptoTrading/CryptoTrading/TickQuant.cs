//define
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Configuration;
using System.Xml.Serialization;
using CryptoTrading.TQLib;
using System.IO;
using System.Text;
using CryptoTrading.ViewModel;
using System.Windows.Media;
using System.Windows;

namespace CryptoTrading.Model
{
    public enum OrderMode { Close, Open, Auto };
    public enum QuantMode { Default, AllAvailable, Preset }
    public enum InstrumentStatusType : sbyte
    {
        /////////////////////////////////////////////////////////////////////////
        ///TFtdcInstrumentStatusType是一个合约交易状态类型
        /////////////////////////////////////////////////////////////////////////
        开盘前 = (sbyte)'0',
        非交易,
        连续交易,
        集合竞价报单,
        集合竞价价格平衡,
        集合竞价撮合,
        收盘
    }

    public enum LoginMode
    {
        Normal,
        DebugOffline, // 离线模式，会读取先前保存的数据
        PreDebugOffline // 这种模式下才会显示保存数据的按钮
    }


    [StructLayout(LayoutKind.Sequential)]
    public class ThostFtdcInstrumentStatusField
    {
        ///交易所代码
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string ExchangeID;
        ///合约在交易所的代码
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 31)]
        public string ExchangeInstID;
        ///结算组代码
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string SettlementGroupID;
        ///合约代码
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 31)]
        public string InstrumentID;
        ///合约交易状态
        public InstrumentStatusType InstrumentStatus;
        ///交易阶段编号
        public int TradingSegmentSN;
        ///进入本状态时间
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string EnterTime;
        ///进入本状态原因
        public char EnterReason;  //'1'自动切换 '2'手工切换 '3'熔断
    };

    public enum TradeSourceType
    {
        回报 = '0',
        查询 = '1'
    }
   
    /// <summary>
    /// 产品类型枚举
    /// </summary>
    [EnumDescription("产品类型枚举")]
    public enum ProductClassType
    {
        /// <summary>
        /// 期货
        /// </summary>
        期货 = '1',
        /// <summary>
        /// 期货期权
        /// </summary>
        期货期权 = '2',
        /// <summary>
        /// 组合
        /// </summary>
        组合 = '3',
        /// <summary>
        /// 即期（现货）
        /// </summary>
        即期 = '4',
        /// <summary>
        /// 期转现
        /// </summary>
        期转现 = '5',
        /// <summary>
        /// 现货期权
        /// </summary>
        现货期权 = '6',

    }


    [StructLayout(LayoutKind.Sequential)]
    public struct SystemTime
    {
        public ushort wYear;
        public ushort wMonth;
        public ushort wDayOfWeek;
        public ushort wDay;
        public ushort wHour;
        public ushort wMinute;
        public ushort wSecond;
        public ushort wMiliseconds;
    }

    /*/合约
    [StructLayout(LayoutKind.Sequential)]
    public class CThostFtdcInstrumentField
    {
        ///合约代码
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 31)]
        public string InstrumentID;
        ///交易所代码
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string ExchangeID;
        ///合约名称
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 21)]
        public string InstrumentName;
        ///合约在交易所的代码
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 31)]
        public string ExchangeInstID;
        ///产品代码
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 31)]
        public string ProductID;
        ///产品类型
        public char ProductClass;
        ///交割年份
        public int DeliveryYear;
        ///交割月
        public int DeliveryMonth;
        ///市价单最大下单量
        public int MaxMarketOrderVolume;
        ///市价单最小下单量
        public int MinMarketOrderVolume;
        ///限价单最大下单量
        public int MaxLimitOrderVolume;
        ///限价单最小下单量
        public int MinLimitOrderVolume;
        ///合约数量乘数
        public int VolumeMultiple;
        ///最小变动价位
        public double PriceTick;
        ///创建日
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string CreateDate;
        ///上市日
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string OpenDate;
        ///到期日
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string ExpireDate;
        ///开始交割日
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string StartDelivDate;
        ///结束交割日
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string EndDelivDate;
        ///合约生命周期状态
        public char InstLifePhase;
        ///当前是否交易
        public int IsTrading;
        ///持仓类型
        public char PositionType;
        ///持仓日期类型
        public char PositionDateType;
        ///多头保证金率
        public double LongMarginRatio;
        ///空头保证金率
        public double ShortMarginRatio;
        ///是否使用大额单边保证金算法
        public char MaxMarginSideAlgorithm;
        ///基础商品代码
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 31)]
        public string UnderlyingInstrID;
        ///执行价
        public double StrikePrice;
        ///期权类型
        public double OptionsType;
        ///合约基础商品乘数
        public char UnderlyingMultiple;
        ///组合类型
        public char CombinationType;
    }*/
    /// <summary>
    /// 合约明细C#Style Properties
    /// </summary>
    public class InstrumentData
    {
        /// <summary>
        /// 合约代码
        /// </summary>
        [XmlAttribute]
        public string InstrumentID { get; set; }
        private EnuExchangeID _exchangeID;
        /// <summary>
        /// 交易所
        /// </summary>
        [XmlAttribute]
        public EnuExchangeID ExchangeID
        {
            set
            {                
                _exchangeID = value;
            }
            get { return _exchangeID; }
        }        
        /// <summary>
        /// 最小变动价位
        /// </summary>
        [XmlAttribute]
        public double PriceTick { set; get; }               
        /// <summary>
        /// 合约价值 
        /// </summary> 
        [XmlAttribute]
        public int ContractValue { set; get; }
        /// <summary>
        /// 产品类型
        /// </summary>
        [XmlAttribute]
        public string ProductClass { set; get; }
        /// <summary>
        /// 合约名称
        /// </summary>
        [XmlAttribute]
        public string Name { get; set; }
        /// <summary>
        /// 合约在交易所的代码
        /// </summary>
        [XmlAttribute]
        public string ContractId { get; set; }
        /// <summary>
        /// 交割日期
        /// </summary>
        [XmlAttribute]
        public string DeliveryDate { get; set; }        
        /// <summary>
        /// 合约生命周期状态
        /// </summary>
        [XmlAttribute]
        public string InstLifePhase { get; set; }
        /// <summary>
        /// 当前是否交易
        /// </summary>
        [XmlAttribute]
        public string IsTrading { get; set; }
        
        /// <summary>
        /// 结算价
        /// </summary>
        private double deliveryPrice;
        [XmlAttribute]
        public double DeliveryPrice { get { if (deliveryPrice >= double.MaxValue || deliveryPrice<=double.MinValue) return 0; else return Math.Round(deliveryPrice, 0); }
            set { if (deliveryPrice != value) deliveryPrice = value; } }               
        /// <summary>
        /// 组合类型
        /// </summary>
        [XmlAttribute]
        public string CombinationType { get; set; }
        /// <summary>
        /// 格式化输出合约信息的属性信息
        /// </summary>
        /// <returns>合约信息对象所有的公开属性信息(属性名：属性值),以",\t"分割</returns>
        public override string ToString()
        {
            return Utility.GetObjectPropertyInfo(this);
        }

        public static implicit operator string(InstrumentData v)
        {
            throw new NotImplementedException();
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class OrderField
    {
        #region field
        /// <summary>
        /// 经纪公司代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 11)]
        public string _BrokerID;
        /// <summary>
        /// 投资者代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string _InvestorID;
        /// <summary>
        /// 合约代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 31)]
        public string _InstrumentID;
        /// <summary>
        /// 报单引用
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string _OrderRef;
        /// <summary>
        /// 用户代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string _UserID;
        /// <summary>
        /// 报单价格条件
        /// </summary>
        public char _OrderPriceType;
        /// <summary>
        /// 多空方向
        /// </summary>
        public char _Direction;
        /// <summary>
        /// 组合开平标志
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 5)]
        public string _CombOffsetFlag;
        /// <summary>
        /// 组合投机套保标志
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 5)]
        public string _CombHedgeFlag;
        /// <summary>
        /// 价格
        /// </summary>
        public double _LimitPrice;
        /// <summary>
        /// 原始报单数量
        /// </summary>
        public int _VolumeTotalOriginal;
        /// <summary>
        /// 有效期类型
        /// </summary>
        public char _TimeCondition;
        /// <summary>
        /// GTD日期
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string _GTDDate;
        /// <summary>
        /// 成交量类型
        /// </summary>
        public char _VolumeCondition;
        /// <summary>
        /// 最小成交量
        /// </summary>
        public int _MinVolume;
        /// <summary>
        /// 触发条件
        /// </summary>
        public char _ContingentCondition;
        /// <summary>
        /// 止损价
        /// </summary>
        public double _StopPrice;
        /// <summary>
        /// 强平原因
        /// </summary>
        public char _ForceCloseReason;
        /// <summary>
        /// 自动挂起标志
        /// </summary>
        public int _IsAutoSuspend;
        /// <summary>
        /// 业务单元
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 21)]
        public string _BusinessUnit;
        /// <summary>
        /// 请求编号
        /// </summary>
        public int _RequestID;
        /// <summary>
        /// 本地报单编号
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string _OrderLocalID;
        /// <summary>
        /// 交易所代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string _ExchangeID;
        /// <summary>
        /// 会员代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 11)]
        public string _ParticipantID;
        /// <summary>
        /// 客户代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 11)]
        public string _ClientID;
        /// <summary>
        /// 合约在交易所的代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 31)]
        public string _ExchangeInstID;
        /// <summary>
        /// 交易所交易员代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 21)]
        public string _TraderID;
        /// <summary>
        /// 安装编号
        /// </summary>
        public int _InstallID;
        /// <summary>
        /// 报单提交状态
        /// </summary>
        public char _OrderSubmitStatus;
        /// <summary>
        /// 报单提示序号
        /// </summary>
        public int _NotifySequence;
        /// <summary>
        /// 交易日
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string _TradingDay;
        /// <summary>
        /// 结算编号
        /// </summary>
        public int _SettlementID;
        /// <summary>
        /// 报单编号
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 21)]
        public string _OrderSysID;
        /// <summary>
        /// 报单来源
        /// </summary>
        public char _OrderSource;
        /// <summary>
        /// 报单状态
        /// </summary>
        public char _OrderStatus;
        /// <summary>
        /// 报单类型
        /// </summary>
        public char _OrderType;
        /// <summary>
        /// 已成交数量
        /// </summary>
        public int _VolumeTraded;
        /// <summary>
        /// 剩余数量
        /// </summary>
        public int _VolumeTotal;
        /// <summary>
        /// 报单日期
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string _InsertDate;
        /// <summary>
        /// 委托时间
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string _InsertTime;
        /// <summary>
        /// 最后修改时间
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string _UpdateTime;
        /// <summary>
        /// 撤销时间
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string _CancelTime;        
       
        /// <summary>
        /// 状态信息
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 81)]
        public string _StatusMsg;
       
        /// <summary>
        /// 委托价
        /// </summary>
        public double OrderPrice;
       
        #endregion
        public override string ToString()
        {
            return Utility.GetObjectFieldInfo(this);
        }
    }
    public class OrderFieldExt
    {
        public InputOrderField orderField;
        public string TQOrderSource { get; set; }
        public bool IsCombo { get; set; }
    }
    /// <summary>
    /// 投资者持仓明细
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class PositionField
    {
        ///合约代码
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 31)]
        public string instrumentID;

        ///经纪公司代码
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 11)]
        public string brokerID;
        ///投资者代码
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string investorID;
        ///持仓多空方向, '0' 多 '1' 空
        public char TradeDirection;
        ///投机套保标志
        public char hedgeFlag;
        ///持仓日期类型（'1'/今仓，'2'/昨仓）
        public char positionDate;
        ///上日持仓
        public int ydPosition;
        ///今日持仓
        public int position;
        ///多头冻结
        public int longFrozen;
        ///空头冻结
        public int shortFrozen;
        ///多头开仓冻结金额
        public double longFrozenAmount;
        ///空头开仓冻结金额
        public double shortFrozenAmount;
        ///开仓量
        public int openVolume;
        ///平仓量
        public int closeVolume;
        ///开仓金额
        public double openAmount;
        ///平仓金额
        public double closeAmount;
        ///持仓成本
        public double positionCost;
        ///上次占用的保证金
        public double preMargin;
        ///占用的保证金
        public double useMargin;
        ///冻结的保证金
        public double frozenMargin;
        ///冻结的资金
        public double frozenCash;
        ///冻结的手续费
        public double frozenCommission;
        ///资金差额
        public double cashIn;
        ///手续费
        public double commission;
        ///平仓盈亏
        public double closeProfit;
        ///持仓盈亏
        public double positionProfit;
        ///上次结算价
        public double preSettlementPrice;
        ///本次结算价
        public double settlementPrice;
        ///交易日
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string tradingDay;
        ///结算编号
        public int settlementID;
        ///开仓成本
        public double openCost;
        ///交易所保证金
        public double exchangeMargin;
        ///组合成交形成的持仓
        public int combPosition;
        ///组合多头冻结
        public int combLongFrozen;
        ///组合空头冻结
        public int combShortFrozen;
        ///逐日盯市平仓盈亏
        public double closeProfitByDate;
        ///逐笔对冲平仓盈亏
        public double closeProfitByTrade;
        ///今日持仓
        public int todayPosition;
        ///保证金率
        public double marginRateByMoney;
        ///保证金率(按手数)
        public double marginRateByVolume;
        ///执行冻结
        public int strikeFrozen;
        ///执行冻结金额
        public double strikeFrozenAmount;
        ///放弃执行冻结
        public double abandonFrozen;
        ///期权市值
        public double optionValue;
        /// <summary>
        /// 合约代码
        /// </summary>
        public string InstrumentID { get { return instrumentID; } }
        /// <summary>
        /// 经纪公司代码
        /// </summary>
        public string BrokerID { get { return brokerID; } }
        /// <summary>
        /// 投资者代码
        /// </summary>
        public string InvestorID { get { return investorID; } }
        /// <summary>
        /// 持仓多空方向
        /// ///净
        //#define THOST_FTDC_PD_Net '1'
        //        ///多头
        //#define THOST_FTDC_PD_Long '2'
        //        ///空头
        //#define THOST_FTDC_PD_Short '3'
        /// </summary>
        /*public TradeDirection Direction
        {
            get
            {
                switch (TradeDirection)
                {
                    default:
                    case '2': return TradeDirection.多;
                    case '3': return TradeDirection.空;
                }
            }
        }
        */
        //[EnumDescription("净")]
        //Net = '1',
        ///// <summary>
        ///// 多头
        ///// </summary>
        //[EnumDescription("多头")]
        //Long = '2',
        ///// <summary>
        /////空头
        ///// </summary>
        //[EnumDescription("空头")]
        //Short = '3'
        /// <summary>
        /// 投机套保标志
        /// </summary>
        public HedgeType Hedge
        {
            get
            {
                switch (hedgeFlag)
                {
                    default:
                    case '1': return HedgeType.投机;
                    case '2': return HedgeType.套利;
                    case '3': return HedgeType.套保;
                }
            }
        }

        /// <summary>
        /// 持仓类型(今仓/昨仓)
        /// </summary>
        public string PositionType
        {
            get
            {
                switch (positionDate)
                {
                    default:
                    case '1': return "今仓";
                    case '2': return "昨仓";
                }
            }
        }
        /// <summary>
        /// 上日持仓
        /// </summary>
        public int YdPosition { get { return ydPosition; } }
        /// <summary>
        /// 总持仓
        /// </summary>
        public int Position { get { return position; } }
        /// <summary>
        /// 多头冻结
        /// </summary>
        public int LongFrozen { get { return longFrozen; } }
        /// <summary>
        /// 空头冻结
        /// </summary>
        public int ShortFrozen { get { return shortFrozen; } }
        /// <summary>
        /// 多头开仓冻结金额
        /// </summary>
        public double LongFrozenAmount { get { return longFrozenAmount; } }
        /// <summary>
        /// 空头开仓冻结金额
        /// </summary>
        public double ShortFrozenAmount { get { return shortFrozenAmount; } }
        /// <summary>
        /// 开仓量
        /// </summary>
        public int OpenVolume { get { return openVolume; } }
        /// <summary>
        /// 平仓量
        /// </summary>
        public int CloseVolume { get { return closeVolume; } }
        /// <summary>
        /// 开仓金额
        /// </summary>
        public double OpenAmount { get { return openAmount; } }
        /// <summary>
        /// 平仓金额
        /// </summary>
        public double CloseAmount { get { return closeAmount; } }
        /// <summary>
        /// 持仓成本
        /// </summary>
        public double PositionCost { get { return positionCost; } set { positionCost = value; } }
        /// <summary>
        /// 上次占用的保证金
        /// </summary>
        public double PreMargin { get { return preMargin; } set { preMargin = value; } }
        /// <summary>
        /// 占用的保证金
        /// </summary>
        public double UseMargin { get { return useMargin; } set { useMargin = value; } }
        /// <summary>
        /// 冻结保证金
        /// </summary>
        public double FrozenMargin { get { return frozenMargin; } set { frozenMargin = value; } }
        /// <summary>
        /// 冻结资金
        /// </summary>
        public double FrozenCash { get { return frozenCash; } set { frozenCash = value; } }
        /// <summary>
        /// 冻结手续费
        /// </summary>
        public double FrozenCommission { get { return frozenCommission; } set { frozenCommission = value; } }
        /// <summary>
        /// 出入金
        /// </summary>
        public double CashIn { get { return cashIn; } set { cashIn = value; } }
        /// <summary>
        /// 手续费
        public double Commission { get { return Math.Round(commission, 2); } set { commission = value; } }
        /// <summary>
        /// 平仓盈亏
        public double CloseProfit { get { return Math.Round(closeProfit, 5); } set { closeProfit = value; } }
        /// <summary>
        /// 持仓盈亏
        //private double positionprofit;
        public double PositionProfit { get { return Math.Round(positionProfit, 2); } set { positionProfit = value; } }
      
        /// <summary>
        /// 开仓成本
        /// </summary>
        public double OpenCost { get { return openCost; } set { openCost = value; } }
      
        /// <summary>
        /// 组合成交形成的持仓
        /// </summary>
        public int CombPosition { get { return combPosition; } set { combPosition = value; } }                
                      
        /// <summary>
        /// 持仓均价
        /// </summary>
        private double avgPrice;
        public double AvgPrice
        {
            set { avgPrice = value; }
            get { return Math.Round(avgPrice, 2); }
        }
               
        /*public string ExchangeName
        {
            get
            { return Utility.GetExchangeName(instrumentID); }
        }*/
    }
    public class PositionData : PositionField, System.ComponentModel.INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public new int YdPosition
        { get { return ydPosition; } set { ydPosition = value; NotifyPropertyChanged("YdPosition"); } }
        public new int Position
        { get { return position; } set { position = value; NotifyPropertyChanged("Position"); } }

        public new TradeDirection TradeDirection
        {
            get;
            set;           
        }
        public string ExchangeID { get; set; }// NotifyPropertyChanged("ExchangeID"); } }
    }
    /// <summary>
    /// 持仓汇总C#Style Properties
    /// </summary>
    public class PositionDataSummary : ObservableObject
    {
        /// <summary>
        /// 合约代码
        /// </summary>
        [XmlAttribute]
        public string InstrumentID { get; set; }
        /// <summary>
        /// 交易所简称
        /// </summary>
        [XmlAttribute]
        public EnuExchangeID ExchangeID { get; set; }

        /// <summary>
        /// 投资者代码
        /// </summary>
        [XmlAttribute]
        public string InvestorID { get; set; }
        //private string leverrate;
        public double LeverRate
        {
            get;
            set;
        }
        private decimal longposition;
        /// <summary>
        /// 持仓
        /// </summary>
        public decimal LongPosition { get { return longposition; } set { longposition = value; NotifyPropertyChanged("LongPosition"); } }
        private decimal longavailable;
        public decimal LongAvailable
        {
            get { return longavailable; }
            set
            {
                longavailable = value;
                NotifyPropertyChanged("LongAvailable");
            }
        }
        private decimal longrealprofit;
        public decimal LongRealProfit
        {
            get { return Math.Round(longrealprofit,4); }
            set
            {
                longrealprofit = value;
                NotifyPropertyChanged("LongRealProfit");
            }
        }
        private decimal longunrealprofit;
        public decimal LongUnrealProfit
        {
            get { return Math.Round(longunrealprofit,4); }
            set { longunrealprofit = value;
                NotifyPropertyChanged("LongUnrealProfit");
            }
        }
        private decimal longavg;
        public decimal LongAvg
        {
            get { return Math.Round(longavg, 4);  }
            set { longavg = value;
                NotifyPropertyChanged("LongAvg");
            }
        }
        private decimal shortposition;
        public decimal ShortPosition
        {
            get
            {
                return shortposition;
            }
            set
            {
                if (shortposition != value)
                {
                    shortposition = value;
                    NotifyPropertyChanged("ShortPosition");
                }
            }
        }
        private decimal shortavailable;
        public decimal ShortAvailable
        {
            get { return shortavailable; }
            set { shortavailable = value;
                NotifyPropertyChanged("ShortAvailable");
            }
        }
        private decimal shortrealprofit;
        public decimal ShortRealProfit
        {
            get { return Math.Round(shortrealprofit,4); }
            set { shortrealprofit = value; NotifyPropertyChanged("ShortRealProfit"); }
        }
        private decimal shortunrealprofit;
        public decimal ShortUnrealProfit
        {
            get { return Math.Round(shortunrealprofit,4); }
            set { shortunrealprofit = value; NotifyPropertyChanged("ShortUnrealProfit"); }
        }
        private decimal shortavg;
        public decimal ShortAvg { get { return Math.Round(shortavg,4); } set { shortavg = value; NotifyPropertyChanged("ShortAvg"); } }

        private DateTime _createdate;
        public DateTime CreateDate
        {
            set
            {
                _createdate = value;
                NotifyPropertyChanged("CreateDate");
            }
            get
            { return _createdate; }
        }
        public bool CrossMargin { get; set; }
        private double forceprice;
        public double ForcePrice
        {
            get { return forceprice; }
            set { forceprice = value; NotifyPropertyChanged("ForcePrice"); }
        }
        private string contracttype;
        public string ContractType
        {
            get { return contracttype; }
            set { contracttype = value; NotifyPropertyChanged("ContractType"); }
        }
        public long ContractID
        {
            get;
            set;
        }
        /// <summary>
        /// 格式化输出持仓汇总的属性信息
        /// </summary>
        /// <returns>持仓汇总对象所有的公开属性信息(属性名：属性值),以",\t"分割</returns>
        public override string ToString()
        {
            return Utility.GetObjectPropertyInfo(this);
        }       
       
    }
   
    /// <summary>
    /// 组合及策略 持仓汇总
    /// </summary>
    [Serializable]
    public class CustomProductStrategyPositionSummary : ObservableObject
    {
        /// <summary>
        /// 合约代码
        /// </summary>
        public string InstrumentID { get; set; }

        /// <summary>
        /// 经纪公司代码
        /// </summary>
        public string BrokerID { get; set; }
        /// <summary>
        /// 投资者代码
        /// </summary>
        public string InvestorID { get; set; }
        /// <summary>
        /// 持仓多空方向
        /// </summary>
        public TradeDirection Direction { get; set; }

        private int ydPosition;
        /// <summary>
        /// 上日持仓
        /// </summary>
        public int YdPosition
        {
            get { return ydPosition; }
            set
            {
                ydPosition = value;
                NotifyPropertyChanged("YdPosition");
            }
        }

        private int position;
        /// <summary>
        /// 持仓
        /// </summary>
        public int Position { get { return position; } set { position = value; NotifyPropertyChanged("Position"); } }

        private int openVolume;
        /// <summary>
        /// 开仓量
        /// </summary>
        public int OpenVolume { get { return openVolume; } set { openVolume = value; NotifyPropertyChanged("OpenVolume"); } }

        private int closeVolume;
        /// <summary>
        /// 平仓量
        /// </summary>
        public int CloseVolume { get { return closeVolume; } set { closeVolume = value; NotifyPropertyChanged("CloseVolume"); } }
        /// <summary>
        /// 开仓金额
        /// </summary>
        private double openAmount;
        public double OpenAmount { get { return openAmount; } set { openAmount = value; NotifyPropertyChanged("OpenAmount"); } }
        /// <summary>
        /// 平仓金额
        /// </summary>
        private double closeAmount;
        public double CloseAmount { get { return closeAmount; } set { closeAmount = value; NotifyPropertyChanged("CloseAmount"); } }
        /// <summary>
        /// 开仓成本
        /// </summary>
        private double openCost;
        public double OpenCost { get { return openCost; } set { openCost = value; NotifyPropertyChanged("OpenCost"); } }
        /// <summary>
        /// 持仓成本
        /// </summary>
        private double positioncost;
        public double PositionCost { get { return Math.Round(positioncost, 2); } set { positioncost = value; NotifyPropertyChanged("PositionCost"); } }

        private double useMargin;
        /// <summary>
        /// 占用保证金
        /// </summary>
        public double UseMargin { get { return useMargin; } set { useMargin = value; NotifyPropertyChanged("UseMargin"); } }

        /// <summary>
        /// 手续费
        /// </summary>
        private double commission;
        public double Commission { get { return Math.Round(commission, 2); } set { commission = value; NotifyPropertyChanged("Commission"); } }
        /// <summary>
        /// 平仓盈亏
        /// </summary>
        private double closeprofit;
        public double CloseProfit { get { return Math.Round(closeprofit, 2); } set { closeprofit = value; NotifyPropertyChanged("CloseProfit"); } }
        /// <summary>
        /// 持仓盈亏
        /// </summary>
        private double positionprofit;
        public double PositionProfit { get { return Math.Round(positionprofit, 2); } set { positionprofit = value; NotifyPropertyChanged("PositionProfit"); } }
        /// <summary>
        /// 昨结价
        /// </summary>
        public double PreSettlementPrice { get; set; }
        /// <summary>
        /// 今结价
        /// </summary>
        public double SettlementPrice { get; set; }

        //TODO
        /// <summary>
        /// 交易所保证金
        /// </summary>
        public double ExchangeMargin { get; set; }

        private int todayPosition;
        /// <summary>
        /// 今日持仓
        /// </summary>
        public int TodayPosition { get { return todayPosition; } set { todayPosition = value; NotifyPropertyChanged("TodayPosition"); } }
        /// <summary>
        /// 开仓均价
        /// 平仓不影响
        /// </summary>
        private double avgprice;
        public double AvgPrice
        {
            get { return Math.Round(avgprice, 2); }
            set { avgprice = value; NotifyPropertyChanged("AvgPrice"); }
        }
        /// <summary>
        /// 格式化输出持仓汇总的属性信息
        /// </summary>
        /// <returns>持仓汇总对象所有的公开属性信息(属性名：属性值),以",\t"分割</returns>
        public override string ToString()
        {
            return Utility.GetObjectPropertyInfo(this);
        }

        private SolidColorBrush _PositionProfitForegroundBrush;
        /// <summary>
        /// 浮动盈亏前景色
        /// </summary>
        [XmlIgnore]
        public SolidColorBrush PositionProfitForegroundBrush
        {
            get
            {
                if (PositionProfit > 0)
                {
                    _PositionProfitForegroundBrush = new SolidColorBrush(Trader.Configuration.ColorSetModelObj.ChangeUpForeground.Color);
                }
                else if (PositionProfit < 0)
                {
                    _PositionProfitForegroundBrush = new SolidColorBrush(Trader.Configuration.ColorSetModelObj.ChangeDownForeground.Color);
                }
                else
                {
                    _PositionProfitForegroundBrush = new SolidColorBrush(Trader.Configuration.ColorSetModelObj.ChangeStableForeground.Color);
                }
                return _PositionProfitForegroundBrush;
            }
        }
    }

    public class PositionDataExt : PositionDataSummary
    {
        public string TradeOrderRef
        { get; set; }
        public string SessionID
        { get; set; }
        public ComboMarketData custproduct;
        public SpecialPositionType PositionType;
       // public Strategy strategy;
    }
    public enum SpecialPositionType
    {
        Custom,
        Manual,
        Strategy,
    }
   
    public class TradeData: ObservableObject
    {            
        /// </summary>
        public string InvestorID { get; set; }
        /// <summary>
        /// 合约代码
        /// </summary>
        public string InstrumentID { get; set; }        
        
        /// <summary>
        /// 交易所简称
        /// </summary>
        public string ExchangeID { get; set; }
        private OrderStatusType orderstatus;                      
        public OrderStatusType OrderStatus
        {
            get { return orderstatus; }
            set
            {
                if (orderstatus != value)
                {
                    orderstatus = value;
                    NotifyPropertyChanged("OrderStatus");
                }
            }
        }
        /// <summary>
        /// 成交编号
        /// </summary>
        public string TradeID { get; set; }
        /// <summary>
        /// 买卖方向
        /// [EnumDescription("买卖方向类型")]
        //public enum DirectionType
        //{
        //    /// <summary>
        //    /// 买
        //    /// </summary>
        //    [EnumDescription("买")]
        //    Buy = '0',
        //    /// <summary>
        //    /// 卖
        //    /// </summary>
        //    [EnumDescription("卖")]
        //    Sell = '1'
        //}
        /// </summary>
        public TradeDirection Direction { get; set; }
       
        /// <summary>
        /// 开平类别
        /// </summary>
         //case ('0'):
         //               order.Offset = OffsetType.开仓;
         //               break;
         //           case ('1'):
         //               order.Offset = OffsetType.平仓;
         //               break;
         //           case ('2'):
         //               order.Offset = OffsetType.强平;
         //               break;
         //           case ('3'):
         //               order.Offset = OffsetType.平今;
         //               break;
         //           case ('4'):
         //               order.Offset = OffsetType.平昨;
         //               break;
         //           case ('5'):
         //               order.Offset = OffsetType.强减;
         //               break;
         //           case ('6'):
         //               order.Offset = OffsetType.本地强平;
         //               break;
        public OffsetType Offset
        {
            get;
            set;
        }
        public OrderPriceType Ordertype
        {
            get;
            set;
        }
        private decimal avgprice;
        public decimal AvgPrice
        {
            get { return avgprice; }
            set
            {
                if (avgprice != value)
                {
                    avgprice = value;
                    NotifyPropertyChanged("AvgPrice");
                }
            }
        }
        /// <summary>
        /// 价格
        /// </summary>
        public double OrderPrice { get; set;  }
        /// <summary>
        /// 数量
        /// </summary>
        public decimal Quant { get; set; }
        private decimal quanttraded;
        public decimal QuantTraded
        {
            get { return quanttraded; }
            set
            {
                if (quanttraded != value)
                {
                    quanttraded = value;
                    NotifyPropertyChanged("QuantTraded");
                }
            }
        }
        /// <summary>
        /// 订单日期
        /// </summary>
        private string ordertime;
        public string OrderTime
        {
            get
            {
                return ordertime;
            }
            set
            {
                if (value != ordertime)
                {
                    ordertime = value;
                    NotifyPropertyChanged("OrderTime");
                }
            }
        }
        private string tradetime;
        /// <summary>
        /// 成交时间
        /// </summary>
        public string UpdateTime
        {
            get {
                return tradetime;
            }
            set
            {
                if (value != tradetime)
                { tradetime = value;
                    NotifyPropertyChanged("TradeTime");
                }
            }
        }
        private double commission;
        /// <summary>
        /// 手续费
        /// </summary>
        public double Commission
        {
            get { return commission; }
            set
            {
                if (value != commission)
                {
                    commission = value;
                    NotifyPropertyChanged("Commission");
                }
            }
        }
        
        //public double Commission { get {return comm; set; }
        /// <summary>
        /// 将类的各字段格式化输出
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Utility.GetObjectPropertyInfo(this);
        }
    }

    /// <summary>
    /// 成交汇总数据
    /// </summary>
    public class TradeDataSummary : TQLib.ObservableObject
    {
        /// <summary>
        /// 经纪公司代码
        /// </summary>
        public string BrokerID { get; set; }
        /// <summary>
        /// 投资者代码
        /// </summary>
        public string InvestorID { get; set; }
        /// <summary>
        /// 合约代码
        /// </summary>
        public string InstrumentID { get; set; }
        /// <summary>
        /// 合约名称
        /// </summary>
        public string InstrumentName { get; set; }
        /// <summary>
        /// 用户代码
        /// </summary>
        public string UserID { get; set; }
        /// <summary>
        /// 交易所简称
        /// </summary>
        public string ExchangeName { get; set; }
        /// <summary>
        /// 多空方向
        /// </summary>
        public TradeDirection Direction { get; set; }
        /// <summary>
        /// 开平标志 交易类别
        /// </summary>
        public OffsetType Offset { get; set; }
        /// <summary>
        /// 客户代码
        /// </summary>
        public string ClientID { get; set; }
        /// <summary>
        /// 合约在交易所的代码
        /// </summary>
        public string ExchangeInstID { get; set; }


        private int volume;
        /// <summary>
        /// 数量
        /// </summary>
        public int Volume
        {
            get { return volume; }
            set
            {
                volume = value;
                NotifyPropertyChanged("Volume");
            }
        }
        /// <summary>
        /// 交易日
        /// </summary>
        public string TradingDay { get; set; }
        /// <summary>
        /// 成交均价
        /// </summary>
        private double avgprice;
        public double AvgPrice
        {
            get { return Math.Round(avgprice, 2); }
            set
            {
                avgprice = value;
                NotifyPropertyChanged("AvgPrice");
            }
        }


        private double commission;
        /// <summary>
        /// 手续费
        /// </summary>
        public double Commission
        {
            get { return commission; }
            set
            {
                commission = value;
                NotifyPropertyChanged("Commission");
            }
        }
    }

    /// <summary>
    /// 交易账户信息
    /// </summary>
    public class TradingAccountData : ObservableObject
    {
        public TradingAccountData()
        {

        }
        ///<summary>
        ///经纪商名称
        ///</summary>
        private string _ExchangeID;
        public string ExchangeID {
            set { _ExchangeID = value;
                NotifyPropertyChanged("ExchangeID"); }
            get { return _ExchangeID; } }
        ///<summary>
        ///账号
        ///</summary>
        private string investorID;
        public string InvestorID
        {
            set
            {
                investorID = value;
                NotifyPropertyChanged("InvestorID");
            }
            get
            {
                return investorID;
            }
        }
        /// <summary>
        /// 持仓盈亏
        /// </summary>
        private double _positionprofit;
        public double PositionProfit
        {
            set
            {
                _positionprofit = Math.Round(value, 5);
                NotifyPropertyChanged("PositionProfit");
            }
            get
            {
                if (_positionprofit < 1000000000000) return _positionprofit;
                else return 0;
            }
        }
        /// <summary>
        /// 平仓盈亏
        /// </summary>
        private double closeprofit;
        public double CloseProfit
        {
            set
            {
                closeprofit = value;
                NotifyPropertyChanged("CloseProfit");
            }
            get
            {
                if (closeprofit < 10000000000000 && closeprofit > -10000000000) return Math.Round(closeprofit, 5);
                else return 0;
            }
        }
        /// <summary>
        /// 手续费
        /// </summary>
        private double commission;
        public double Commission { set { commission = value; NotifyPropertyChanged("Commission"); } get { return Math.Round(commission, 5); } }
                
        private double usedMargin;
        /// <summary>
        /// 占用的保证金
        /// </summary>
        public double UsedMargin { get { return Math.Round(usedMargin, 5); } set { usedMargin = value; NotifyPropertyChanged("UsedMargin"); } }
                         
        /// <summary>
        /// 可用资金
        /// </summary>
        private double available;
        public double Available
        {
            get { return Math.Round(available, 5); }
            set { available = value; NotifyPropertyChanged("Available"); }
        }
        
        ///<summary>
        ///风险度
        ///</summary>
        private double risk;
        public string Risk
        {
            set
            {
                if (value != "")
                    risk = double.Parse(value);
                else
                {
                    risk = Equity == 0 ? 0 : UsedMargin / Equity;
                }
                NotifyPropertyChanged("Risk");
            }
            get { return Math.Round(risk * 100, 2) + "%"; }
        }
        private DateTime? updatetime;
        public DateTime UpdateTime
        {
            get
            { return updatetime??DateTime.Now; }
            set
            {
                updatetime = value;
                NotifyPropertyChanged("UpdateTime");
            }
        }
        private double frozenMargin;
        /// <summary>
        /// 冻结的保证金
        /// </summary>
        public double FrozenMargin { get { return Math.Round(frozenMargin, 2); } set { frozenMargin = value; NotifyPropertyChanged("FrozenMargin"); } }

        private double frozenCommission;
        /// <summary>
        ///冻结的手续费
        /// </summary>
        public double FrozenCommission { get { return Math.Round(frozenCommission, 2); } set { frozenCommission = value; NotifyPropertyChanged("FrozenCommission"); } }

        private double equity;
        /// <summary>
        /// 当前权益
        /// </summary>
        public double Equity
        {
            set { equity = value;NotifyPropertyChanged("Balance"); }
            get { return Math.Round(equity, 5); }
        }
        private decimal balanceBtc;
        /// <summary>
        /// 比特币计价的权益
        /// </summary>
        public decimal BalanceBtc
        {
            set { balanceBtc = value; NotifyPropertyChanged("BalanceBtc"); }
            get { return Math.Round(balanceBtc, 5); }
        }
        /// <summary>
        /// 币种代码
        /// </summary>
        public string DigitalCurrencyID { get; set; }
       
        /// <summary>
        /// 格式化输出账户信息的属性信息
        /// </summary>
        /// <returns>账户信息对象所有的公开属性信息(属性名：属性值),以",\t"分割</returns>
        public override string ToString()
        {
            return Utility.GetObjectPropertyInfo(this);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class CThostFtdcTradingAccountField
    {
        ///经纪公司代码
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 11)]
        public string BrokerID;
        ///投资者帐号
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string AccountID;
        ///上次质押金额
        public double PreMortgage;
        ///上次信用额度
        public double PreCredit;
        ///上次存款额
        public double PreDeposit;
        ///上次结算准备金
        public double PreBalance;
        ///上次占用的保证金
        public double PreMargin;
        ///利息基数
        public double InterestBase;
        ///利息收入
        public double Interest;
        ///入金金额
        public double Deposit;
        ///出金金额
        public double Withdraw;
        ///冻结的保证金
        public double FrozenMargin;
        ///冻结的资金
        public double FrozenCash;
        ///冻结的手续费
        public double FrozenCommission;
        ///当前保证金总额
        public double CurrMargin;
        ///资金差额
        public double CashIn;
        ///手续费
        public double Commission;
        ///平仓盈亏
        public double CloseProfit;
        ///持仓盈亏
        public double PositionProfit;
        ///期货结算准备金
        public double Balance;
        ///可用资金
        public double Available;
        ///可取资金
        public double WithdrawQuota;
        ///基本准备金
        public double Reserve;
        ///交易日
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string TradingDay;
        ///结算编号
        public int SettlementID;
        ///信用额度
        public double Credit;
        ///质押金额
        public double Mortgage;
        ///交易所保证金
        public double ExchangeMargin;
        ///投资者交割保证金
        public double DeliveryMargin;
        ///交易所交割保证金
        public double ExchangeDeliveryMargin;
        ///保底期货结算准备金
        public double ReserveBalance;
        ///币种代码
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
        public string CurrencyID;
        ///上次货币质入金额
        public double PreFundMortgageIn;
        ///上次货币质出金额
        public double PreFundMortgageOut;
        ///货币质入金额
        public double FundMortgageIn;
        ///货币质出金额
        public double FundMortgageOut;
        ///货币质押余额
        public double FundMortgageAvailable;
        ///可质押货币金额
        public double MortgageableFund;
        ///特殊产品占用保证金
        public double SpecProductMargin;
        ///特殊产品冻结保证金
        public double SpecProductFrozenMargin;
        ///特殊产品手续费
        public double SpecProductCommission;
        ///特殊产品冻结手续费
        public double SpecProductFrozenCommission;
        ///特殊产品持仓盈亏
        public double SpecProductPositionProfit;
        ///特殊产品平仓盈亏
        public double SpecProductCloseProfit;
        ///根据持仓盈亏算法计算的特殊产品持仓盈亏
        public double SpecProductPositionProfitByAlg;
        ///特殊产品交易所保证金
        public double SpecProductExchangeMargin;
        ///期权平仓盈亏
        //public double OptionCloseProfit;
        ///期权市值
        //public double OptionValue;
    }
    [StructLayout(LayoutKind.Sequential)]
    public class CThostFtdcRspUserLoginField
    {
        ///交易日
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string TradingDay;
        ///登录成功时间
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string LoginTime;
        ///经纪公司代码
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 11)]
        public string BrokerID;
        ///用户代码
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string UserID;
        ///交易系统名称
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 41)]
        public string SystemName;
        ///前置编号
        public int FrontID;
        ///会话编号
        public int SessionID;
        ///最大报单引用
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string MaxOrderRef;
        ///上期所时间
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string SHFETime;
        ///大商所时间
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string DCETime;
        ///郑商所时间
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string CZCETime;
        ///中金所时间
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string FFEXTime;
        ///能源中心时间
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string INETime;
    }

    /// <summary>
    /// 输入报单操作
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class InputOrderActionField
    {
        ///经纪公司代码
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 11)]
        public string BrokerID;
        ///投资者代码
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string InvestorID;
        ///报单操作引用
        public int OrderActionRef;
        ///报单引用
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string OrderRef;
        ///请求编号
        public int RequestID;
        ///前置编号
        public int FrontID;
        ///会话编号
        public int SessionID;
        ///交易所代码
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string ExchangeID;
        ///报单编号
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 21)]
        public string OrderSysID;
        ///操作标志
        public char ActionFlag;
        ///价格
        public double LimitPrice;
        ///数量变化
        public int VolumeChange;
        ///用户代码
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string UserID;
        ///合约代码
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 31)]
        public string InstrumentID;

        public override string ToString()
        {
            return Utility.GetObjectFieldInfo(this);
        }
    }
    ///TFtdcForceCloseReasonType是一个强平原因类型
    /////////////////////////////////////////////////////////////////////////
    ///非强平
//#define THOST_FTDC_FCC_NotForceClose '0'
//    ///资金不足
//#define THOST_FTDC_FCC_LackDeposit '1'
//    ///客户超仓
//#define THOST_FTDC_FCC_ClientOverPositionLimit '2'
//    ///会员超仓
//#define THOST_FTDC_FCC_MemberOverPositionLimit '3'
//    ///持仓非整数倍
//#define THOST_FTDC_FCC_NotMultiple '4'
//    ///违规
//#define THOST_FTDC_FCC_Violation '5'
//    ///其它
//#define THOST_FTDC_FCC_Other '6'
//    ///自然人临近交割
//#define THOST_FTDC_FCC_PersonDeliv '7'

    //enum ForceCloseReasonType
    //{
    //    NotForceClose= '0',

    //}

   
    /// <summary>
    /// 开平
    /// </summary>
    public enum OffsetType
    {
        [EnumDescription("开仓")]
        开仓,
        [EnumDescription("平仓")]
        平仓, 
    }
    public interface TradeInterface
    {
        int ReqOrderInsert(MajorMarketData orderboard, TradeDirection Direction, int Quant);
        int ReqOrderInsert(MajorMarketData orderboard, TradeDirection Direction, int Quant, string OrderRef);
        int ReqOrderInsert(MajorMarketData orderboard, TradeDirection Direction, int Quant, PricingMode PriceMode, string OrderRef);    //for  openning positions, 不用输入OrderRef
        int ReqOrderInsert(ComboMarketData cust, TradeDirection Direction, int Quant, string OrderRef);
        int ReqOrderInsert(string InstrumentID, TradeDirection pDirection, OffsetType pOffset, double pPrice, int pVolume, LimitMarket pType,  //不用输入OrderRef
            HedgeType pHedge = HedgeType.投机, double pStopPrice = 0, ForceCloseReasonType forceclose = ForceCloseReasonType.非强平, FakFok pFakFok = FakFok.Default);
        int ReqOrderInsert(InputOrderField input); //用于自定义合约等的交易下单，自行生成OrderRef配置好InputOrderField
        //void ReqClosePosition(MarketData orderboard,TradeDirection Direction, int Quant=0, string OrderRef="");    //used for close positions
        void ReqClosePosition(MajorMarketData orderboard, TradeDirection Direction, int Quant = 0, string OrderRef = "", double PresetPrice = -1);
        int ReqOrderAction(int frontID, int sessionID, string orderRef, string instrumentID);   //key: FrontID+SessionID+OrderRef
        int ReqOrderAction(InputOrderActionField IOAC);
        void ReqOrderActionCancel();
        void ReqExchangePosition();
        void ReqReversePosition(PositionDataSummary position);
        void ReqForceClose(MajorMarketData orderboard);   //force to close all position ASAP
    }
    
}