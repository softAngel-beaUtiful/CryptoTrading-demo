using ExchangeSharp;
using System;
using TickQuant.Common;

namespace TickQuant.Strategy
{
    public class TQOrder:ICloneable<TQOrder>
    {
        public string ClOrderID { get; set; }
        public string OrderID { get; set; }
        public string ExchangeID { get; set; }
        public string UserID { get; set; }
        public string StrategyID { get; set; }
        public string Symbol { get; set; }
        public decimal Price { get; set; }
        public decimal Quant { get; set; }
        public decimal FilledQuant { get; set; }
        public decimal AvgPrice { get; set; }
        public decimal Fees { get; set; }
        public EDirection Direction { get; set; }
        public EOrderPriceType PriceType { get; set; }
        public EOrderStatus Status { get; set; }
        public bool IsOpen { get; set; }
        public bool IsMargin { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime OrderTime { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public string FeesCurrency { get; set; }
        public string Message { get; set; }
        public string RunMode { get; set; }
        public string BackTestID { get; set; }
        public string OrderTag { get; set; }
        public string OffsetFlag { get; set; }
        public string OrderSource { get; set; }
        public string OrderType { get; set; }

        public TQOrder()
        {
        }

        public TQOrder(UserOrder orderResult)
        {
            Price = orderResult.Price;
            ExchangeID = orderResult.ExchangeID;
            Symbol = orderResult.Symbol;
            ClOrderID = orderResult.ClOrderId;
            OrderID = orderResult.OrderId;
            OrderTime = orderResult.TransactionTime;
            LastUpdateTime = orderResult.TransactionTime;
            FilledQuant = orderResult.FilledAmount;
            Message = orderResult.Message;
            Direction = orderResult.IsBuy ? EDirection.Long : EDirection.Short;
            Quant = orderResult.Amount;
            AvgPrice = orderResult.AveragePrice;
            Fees = orderResult.Fees;
            FeesCurrency = orderResult.SettlCurrency;
            Status = (EOrderStatus)orderResult.OrderState;
            PriceType = orderResult.PriceType;
        }

        public TQOrder Clone()
        {
          return (TQOrder)this.MemberwiseClone();
        }
    }
}
