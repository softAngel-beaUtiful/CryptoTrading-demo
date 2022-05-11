using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickQuant.Lib;
using TickQuant.Model;
using TickQuant.ViewModel;

namespace TickQuant
{
    public class OCO
    {
        public List<Strategy> OCOStrategies;


    }
    public class Strategy: TQNotifyPropertyChanged   //条件单，包括：条件单名称，条件单编号，价格逻辑条件组和时间逻辑条件组，及报单参数组
    {
        public int StrategyId
        {
            get; set;
        }
        private string _StrategyName;

        public string StrategyName
        {
            get { return _StrategyName; }
            set
            {
                _StrategyName = value;
                NotifyPropertyChanged("StrategyName");
            }
        }

        public string Descriptions;    
        public bool Dependent { get; set; }
        public List<OrderParameters> OrderParamsList = new List<OrderParameters>();
        public List<Condition> ConditionsList = new List<Condition>();
        //indicate if this conditional order is activated or not
        public bool Activated { get; set; }
        public Strategy(List<Condition> condlist, List<OrderParameters> parlist)
        {
            ConditionsList = condlist;
            OrderParamsList = parlist;
            StrategyId = ControllerBackground.CondOrderNo;
        }
        public Dictionary<string, PositionField> dicStategyPositions;
        private bool _IsConditionsMeeted;
        public bool IsConditionsMeeted
        {
            get
            {
                if (ConditionsList == null || ConditionsList.Count == 0)
                {
                    _IsConditionsMeeted = false;
                }
                else {
                    _IsConditionsMeeted = ConditionsList[0].State;

                    for (int i = 0; i < ConditionsList.Count; i++)
                    {
                        if (ConditionsList[i].conditionLogics == ConditionLogics.And)
                            _IsConditionsMeeted = _IsConditionsMeeted && ConditionsList[i].State;
                        else
                            _IsConditionsMeeted = _IsConditionsMeeted || ConditionsList[i].State;
                    }
                }

                return _IsConditionsMeeted;
            }
            set { _IsConditionsMeeted = value; }
        }
        public bool Contains(string sInstrumentID)
        {
            foreach (var condition in ConditionsList)
            {
                if (condition.Contains(sInstrumentID))
                    return true;
            }
            return false;
        }
        /// <summary>
        /// 初始驱动，如果条件满足，立即下单
        /// </summary>
        /// <returns></returns>
        public bool eventTrigger()
        {
            if (RefreshState())
            {
                OrderInsert();
                return true;
            }
            else {
                return false;
            }

        }
        /// <summary>
        /// 行情驱动，如果条件满足，立即下单
        /// </summary>
        /// <param name="md"></param>
        /// <returns></returns>
        public bool eventTrigger(DepthMarketDataField md)
        {
            if (RefreshState(md))
            {
                OrderInsert();
                return true;
            }
            else {
                return false;
            }
        }

        /// <summary>
        /// 时间驱动，如果条件满足，立即下单
        /// </summary>
        /// <returns></returns>
        public bool eventTrigger(DateTime dt)
        {
            if (this.RefreshState(dt))
            {
                this.OrderInsert();
                return true;
            }
            else {
                return false;
            }
        }

        private bool RefreshState(/*string sCondOrderNo*/)
        {
            // 自己，没有必要传 CondOrderNo
            //if(this.CondOrderNo == sCondOrderNo)
            //{
            foreach (var c in ConditionsList)
            {
                c.RefreshState();
            }

            return IsConditionsMeeted;
        }

        private bool RefreshState(DepthMarketDataField md)
        {
            if (this.Contains(md.InstrumentID))
            {
                foreach(var c in ConditionsList)
                {
                    c.RefreshState(md);
                }
            }
            return IsConditionsMeeted;
        }

        private bool RefreshState(DateTime dt)
        {
            foreach (var c in ConditionsList)
            {
                c.RefreshState(dt);
            }
            return IsConditionsMeeted;
        }

        public void OrderInsert()
        {
            foreach (var pa in OrderParamsList)
            {
                CustomProduct custProduct;
                if (TQMain.dicCustomProduct.TryGetValue(pa.InstrumentID, out custProduct))
                {
                    custProduct.OrderInsert(pa.TradeDirection, pa.offsetType, pa.Quant);
                }
                else
                {
                    TQMain.T.ReqOrderInsert(pa.InstrumentID, pa.TradeDirection, pa.offsetType, pa.Price, pa.Quant);
                }
            }
        }
    }
    public class OrderParameters
    {
        public string InstrumentID;
        public DirectionType TradeDirection;
        public OrderOffsetType offsetType;
        public double Price;
        public int Quant;
        public HedgeType hedgeType = HedgeType.投机;
        public OrderType orderType = OrderType.Limit;
    }

    /*
     [EnumDescription("触发条件类型")]
    public enum ContingentConditionType
    {
        /// <summary>
        /// 立即
        /// </summary>
        [EnumDescription("立即")]
        Immediately='1',
        /// <summary>
        /// 止损
        /// </summary>
        [EnumDescription("止损")]
        Touch='2',
        /// <summary>
        /// 止赢
        /// </summary>
        [EnumDescription("止赢")]
        TouchProfit='3',
        /// <summary>
        /// 预埋单
        /// </summary>
        [EnumDescription("预埋单")]
        ParkedOrder='4',
        /// <summary>
        /// 最新价大于条件价
        /// </summary>
        [EnumDescription("最新价大于条件价")]
        LastPriceGreaterThanStopPrice='5',
        /// <summary>
        /// 最新价大于等于条件价
        /// </summary>
        [EnumDescription("最新价大于等于条件价")]
        LastPriceGreaterEqualStopPrice='6',
        /// <summary>
        /// 最新价小于条件价
        /// </summary>
        [EnumDescription("最新价小于条件价")]
        LastPriceLesserThanStopPrice='7',
        /// <summary>
        /// 最新价小于等于条件价
        /// </summary>
        [EnumDescription("最新价小于等于条件价")]
        LastPriceLesserEqualStopPrice='8',
        /// <summary>
        /// 卖一价大于条件价
        /// </summary>
        [EnumDescription("卖一价大于条件价")]
        AskPriceGreaterThanStopPrice='9',
        /// <summary>
        /// 卖一价大于等于条件价
        /// </summary>
        [EnumDescription("卖一价大于等于条件价")]
        AskPriceGreaterEqualStopPrice='A',
        /// <summary>
        /// 卖一价小于条件价
        /// </summary>
        [EnumDescription("卖一价小于条件价")]
        AskPriceLesserThanStopPrice='B',
        /// <summary>
        /// 卖一价小于等于条件价
        /// </summary>
        [EnumDescription("卖一价小于等于条件价")]
        AskPriceLesserEqualStopPrice='C',
        /// <summary>
        /// 买一价大于条件价
        /// </summary>
        [EnumDescription("买一价大于条件价")]
        BidPriceGreaterThanStopPrice='D',
        /// <summary>
        /// 买一价大于等于条件价
        /// </summary>
        [EnumDescription("买一价大于等于条件价")]
        BidPriceGreaterEqualStopPrice='E',
        /// <summary>
        /// 买一价小于条件价
        /// </summary>
        [EnumDescription("买一价小于条件价")]
        BidPriceLesserThanStopPrice='F',
        /// <summary>
        /// 买一价小于等于条件价
        /// </summary>
        [EnumDescription("买一价小于等于条件价")]
        BidPriceLesserEqualStopPrice='H'
    }
        */

    public enum ConditionLogics : sbyte
    {
        And,
        Or
    }

    public enum ConditionType : sbyte
    {
        Price,
        Time
    }

    public enum TimeLogics : sbyte
    {
        Before,
        After,
        SharpAt
    }

    public class PriceStruct
    {
        public string InstrumentID;
        public ContingentConditionType contingentCondition;
        public double TrigerPrice;
    }

    public class TimeStruct
    {
        // 开市即成交
        public bool ExchangeIsOpen;

        // 满足特定时间之前或之后
        public TimeLogics timeLogics;
        public DateTime TrigerTime;
    }

    public class Condition
    {
        //string InstrumentID;
        public ConditionType conditionType;
        public ConditionLogics conditionLogics;
        public PriceStruct priceStruct;
        public TimeStruct timeStruct;


        //List<Condition> conditionList;
        public Condition(PriceStruct price)
        {
            conditionType = ConditionType.Price;
            priceStruct = price;
        }

        public Condition(TimeStruct time)
        {
            conditionType = ConditionType.Time;
            timeStruct = time;
        }

        private bool _State;
        public bool State
        {
            get { return _State; }
            set { _State = value; }
        }

        public bool ReverseState()
        {
            _State = !_State;
            return _State;
        }

        public bool Contains(string sInstrumentID)
        {
            if (conditionType == ConditionType.Price)
            {
                return (priceStruct.InstrumentID == sInstrumentID);
            }
            return false;
        }

        public void RefreshState()
        {

            switch (conditionType)
            {
                // 时间条件
                case ConditionType.Time:
                    if (timeStruct.ExchangeIsOpen)
                    {
                        _State = true;
                    }
                    else
                    {
                        switch (timeStruct.timeLogics)
                        {
                            case TimeLogics.After:
                                _State = (PreciseTimer.GetPreciseTime(0) >= timeStruct.TrigerTime);
                                break;
                            case TimeLogics.Before:
                                _State = (PreciseTimer.GetPreciseTime(0) <= timeStruct.TrigerTime);
                                break;
                        }

                    }
                    _State = true;
                    break;
                // 价格条件
                default:
                    if (priceStruct.contingentCondition == ContingentConditionType.Immediately
                        || priceStruct.contingentCondition == ContingentConditionType.ParkedOrder)
                    {
                        _State = true;
                    }
                    else
                    {
                        DepthMarketDataField md;
                        if (!TQMain.dicMarketData.TryGetValue(priceStruct.InstrumentID, out md))
                        {
                            _State = false;
                            return;
                        }

                        PositionDataSummary ps;
                        switch (priceStruct.contingentCondition)
                        {
                            case ContingentConditionType.AskPriceGreaterEqualStopPrice:
                                if (md.AskPrice1 >= priceStruct.TrigerPrice) _State = true;
                                break;
                            case ContingentConditionType.AskPriceGreaterThanStopPrice:
                                if (md.AskPrice1 > priceStruct.TrigerPrice) _State = true;
                                break;
                            case ContingentConditionType.AskPriceLesserEqualStopPrice:
                                if (md.AskPrice1 <= priceStruct.TrigerPrice) _State = true;
                                break;
                            case ContingentConditionType.AskPriceLesserThanStopPrice:
                                if (md.AskPrice1 < priceStruct.TrigerPrice) _State = true;
                                break;
                            case ContingentConditionType.BidPriceGreaterEqualStopPrice:
                                if (md.BidPrice1 >= priceStruct.TrigerPrice) _State = true;
                                break;
                            case ContingentConditionType.BidPriceGreaterThanStopPrice:
                                if (md.BidPrice1 > priceStruct.TrigerPrice) _State = true;
                                break;
                            case ContingentConditionType.BidPriceLesserEqualStopPrice:
                                if (md.BidPrice1 <= priceStruct.TrigerPrice) _State = true;
                                break;
                            case ContingentConditionType.BidPriceLesserThanStopPrice:
                                if (md.BidPrice1 < priceStruct.TrigerPrice) _State = true;
                                break;
                            //case ContingentConditionType.Immediately:
                            //    return true;
                            case ContingentConditionType.LastPriceGreaterEqualStopPrice:
                                if (md.LastPrice >= priceStruct.TrigerPrice) _State = true;
                                break;
                            case ContingentConditionType.LastPriceGreaterThanStopPrice:
                                if (md.LastPrice > priceStruct.TrigerPrice) _State = true;
                                break;
                            case ContingentConditionType.LastPriceLesserEqualStopPrice:
                                if (md.LastPrice <= priceStruct.TrigerPrice) _State = true;
                                break;
                            case ContingentConditionType.LastPriceLesserThanStopPrice:
                                if (md.LastPrice < priceStruct.TrigerPrice) _State = true;
                                break;
                            //case ContingentConditionType.ParkedOrder:
                            //    return true;
                            case ContingentConditionType.Touch:
                                if (TQMain.dicPositionSummary.TryGetValue(priceStruct.InstrumentID, out ps))
                                {
                                    if (ps.PosiDirection == DirectionType.多)
                                    {
                                        if (md.LastPrice <= priceStruct.TrigerPrice) _State = true;
                                    }
                                    else if (md.LastPrice >= priceStruct.TrigerPrice) _State = true;
                                }
                                break;
                            case ContingentConditionType.TouchProfit:
                                if (!TQMain.dicPositionSummary.TryGetValue(priceStruct.InstrumentID, out ps)) break;
                                if (ps.PosiDirection == DirectionType.多)
                                {
                                    if (md.LastPrice >= priceStruct.TrigerPrice) _State = true;
                                }
                                else
                                   if (md.LastPrice <= priceStruct.TrigerPrice) _State = true;
                                break;
                        }
                        _State = false;
                    }
                    break;
            }
        }

        public void RefreshState(DepthMarketDataField md)
        {

            switch (conditionType)
            {
                // 时间条件
                case ConditionType.Time:
                    //if (timeStruct.ExchangeIsOpen)
                    //{
                    //    _State = true;
                    //}
                    //else
                    //{
                    //    switch (timeStruct.timeLogics)
                    //    {
                    //        case TimeLogics.After:
                    //            _State = (DateTime.Now >= timeStruct.TrigerTime);
                    //            break;
                    //        case TimeLogics.Before:
                    //            _State = (DateTime.Now <= timeStruct.TrigerTime);
                    //            break;
                    //    }

                    //}
                    //_State = true;
                    break;
                // 价格条件
                default:
                    if (priceStruct.contingentCondition == ContingentConditionType.Immediately
                        || priceStruct.contingentCondition == ContingentConditionType.ParkedOrder)
                    {
                        _State = true;
                    }
                    else
                    {
                        if (md.InstrumentID != priceStruct.InstrumentID)
                        //if (!priceStruct.InstrumentID.Contains(md.InstrumentID))
                        {
                            return;
                        }

                        PositionDataSummary ps;
                        switch (priceStruct.contingentCondition)
                        {
                            case ContingentConditionType.AskPriceGreaterEqualStopPrice:
                                if (md.AskPrice1 >= priceStruct.TrigerPrice) _State = true;
                                break;
                            case ContingentConditionType.AskPriceGreaterThanStopPrice:
                                if (md.AskPrice1 > priceStruct.TrigerPrice) _State = true;
                                break;
                            case ContingentConditionType.AskPriceLesserEqualStopPrice:
                                if (md.AskPrice1 <= priceStruct.TrigerPrice) _State = true;
                                break;
                            case ContingentConditionType.AskPriceLesserThanStopPrice:
                                if (md.AskPrice1 < priceStruct.TrigerPrice) _State = true;
                                break;
                            case ContingentConditionType.BidPriceGreaterEqualStopPrice:
                                if (md.BidPrice1 >= priceStruct.TrigerPrice) _State = true;
                                break;
                            case ContingentConditionType.BidPriceGreaterThanStopPrice:
                                if (md.BidPrice1 > priceStruct.TrigerPrice) _State = true;
                                break;
                            case ContingentConditionType.BidPriceLesserEqualStopPrice:
                                if (md.BidPrice1 <= priceStruct.TrigerPrice) _State = true;
                                break;
                            case ContingentConditionType.BidPriceLesserThanStopPrice:
                                if (md.BidPrice1 < priceStruct.TrigerPrice) _State = true;
                                break;
                            //case ContingentConditionType.Immediately:
                            //    return true;
                            case ContingentConditionType.LastPriceGreaterEqualStopPrice:
                                if (md.LastPrice >= priceStruct.TrigerPrice) _State = true;
                                break;
                            case ContingentConditionType.LastPriceGreaterThanStopPrice:
                                if (md.LastPrice > priceStruct.TrigerPrice) _State = true;
                                break;
                            case ContingentConditionType.LastPriceLesserEqualStopPrice:
                                if (md.LastPrice <= priceStruct.TrigerPrice) _State = true;
                                break;
                            case ContingentConditionType.LastPriceLesserThanStopPrice:
                                if (md.LastPrice < priceStruct.TrigerPrice) _State = true;
                                break;
                            //case ContingentConditionType.ParkedOrder:
                            //    return true;
                            case ContingentConditionType.Touch:
                                if (TQMain.dicPositionSummary.TryGetValue(priceStruct.InstrumentID, out ps))
                                {
                                    if (ps.PosiDirection == EnumDescription.GetFieldText(PosiDirectionType.Long))
                                    {
                                        if (md.LastPrice <= priceStruct.TrigerPrice) _State = true;
                                    }
                                    else if (md.LastPrice >= priceStruct.TrigerPrice) _State = true;
                                }
                                break;
                            case ContingentConditionType.TouchProfit:
                                if (TQMain.dicPositionSummary.TryGetValue(priceStruct.InstrumentID, out ps))
                                {
                                    if (ps.PosiDirection == EnumDescription.GetFieldText(PosiDirectionType.Long))
                                    {
                                        if (md.LastPrice >= priceStruct.TrigerPrice) _State = true;
                                    }
                                    else
                                       if (md.LastPrice <= priceStruct.TrigerPrice) _State = true;
                                }
                                break;
                        }
                    }
                    break;
            }
        }

        public void RefreshState(DateTime dt)
        {

            switch (conditionType)
            {
                // 时间条件
                case ConditionType.Time:
                    if (timeStruct.ExchangeIsOpen)
                    {
                        _State = true;
                    }
                    else
                    {
                        if (this.timeStruct.TrigerTime.Subtract(dt).TotalSeconds < 1)
                            _State = this.ReverseState();
                    }
                    break;
                // 价格条件
                default:
                    break;
            }
        }
    }
}
