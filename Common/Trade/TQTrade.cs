using ExchangeSharp;
using System;

namespace TickQuant.Strategy
{
    public class TQTrade
    {
        public string Symbol { get; set; }
        public string ExchangeID { get; set; }
        public string Account { get; set; }
        public string Currency { get; set; }
        public string SettlCurrency { get; set; }
        public string TradeID { get; set; }
        public string MatchID { get; set; }
        public string OrderID { get; set; }
        public string PriceType { get; set; }
        public string OrderType { get; set; }
        public string TradeType { get; set; }
        public string TimeInForce;
        public decimal LastTraded { get; set; }
        public decimal LastPrice { get; set; }
        public decimal AvgPrice { get; set; }
        public decimal Fees { get; set; }
        public decimal Commission { get; set; }
        public decimal TotalTraded { get; set; }
        public decimal ExecCost { get; set; }
        public decimal ExecComm { get; set; }
        public decimal Leverage { get; set; }
        public decimal Unfilled { get; set; }
        public bool IsBuy { get; set; }
        public string FeesCurrency { get; set; }
        public string OrderRejReason { get; set; }
        public string LastLiquidity { get; set; }
        public string Message { get; set; }
        public EOrderState OrderStatus { get; set; }
        public string ClOrderID { get; set; }
        public DateTime EntryTime { get; set; }
        public DateTime TradeTime { get; set; }

        /// <summary>
        /// Btc计价的交易额
        /// </summary>
        public decimal HomeNotional { get; set; }    //trade value in btc 
        /// <summary>
        /// USD计价的交易额
        /// </summary>
        public decimal ForeignNotional { get; set; }    //trade value in usd for xbt contracts        
        public string UserID { get; set; }
        public string RunMode { get; set; }
        public string StrategyID { get; set; }
        public string BackTestID { get; set; }
        public decimal TradePrice { get; set; }
        public decimal TradeQuant { get; set; }
        public string Direction { get; set; }
        public string OffsetFlag { get; set; }

        public TQTrade()
        {
        }

        public TQTrade(ExchangeOrderResult tradeResult, string exch)
        {
            ExchangeID = exch;
            Symbol = tradeResult.MarketSymbol;
            ClOrderID = tradeResult.ClientOrderId; //ClOrderID;
            OrderID = tradeResult.OrderId;//OrderID;
            TradeID = tradeResult.TradeId;
            TradeTime = tradeResult.TradeDate.Value; // LastFilledTime;
            EntryTime = tradeResult.OrderDate;//LastFilledTime;
            TotalTraded = tradeResult.AmountFilled.Value;//TotalFilledQty;
            Message = tradeResult.Message;
            IsBuy = tradeResult.IsBuy;
            TradePrice = tradeResult.Price.Value;
            TradeQuant = tradeResult.AmountFilled.Value;
            AvgPrice = tradeResult.AveragePrice.Value;
            Commission = tradeResult.Fees.Value;
            switch (tradeResult.Result)
            {
                case ExchangeAPIOrderResult.Filled:
                    OrderStatus = EOrderState.Filled;
                    break;
                case ExchangeAPIOrderResult.FilledPartially:
                    OrderStatus = EOrderState.PartiallyFilled;
                    break;
                case ExchangeAPIOrderResult.Canceled:
                    OrderStatus = EOrderState.Canceled;
                    break;
                case ExchangeAPIOrderResult.PendingOpen:
                    OrderStatus = EOrderState.PendingNew;
                    break;
                case ExchangeAPIOrderResult.Rejected:
                    OrderStatus = EOrderState.Unknown;
                    break;
            }           
            Direction = tradeResult.IsBuy ? "Long" : "Short";
        }
    }
}
