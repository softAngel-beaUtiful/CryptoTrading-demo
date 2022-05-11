using ExchangeSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TickQuant.Common
{
    public class TQOrderBook
    {
        private ExchangeOrderBook OrderBook;

        public TQOrderBook(ExchangeOrderBook orderBook)
        {
            this.OrderBook = orderBook;
        }        
        public long SequenceId { get { return OrderBook.SequenceId; } }

        public DateTime TimeStamp { get { return OrderBook.Timestamp; } }
        public string Symbol { get { return OrderBook.MarketSymbol; } }

        public decimal SumAskAmount { get { return GetSumAmount(true); } }
        public decimal SumBidAmount { get { return GetSumAmount(false); } }
        public decimal AvgAskPrice { get { return GetAvgPrice(true); } }
        public decimal AvgBidPrice { get { return GetAvgPrice(false); } }

        public decimal AskPrice { get { return GetFirstPrice(true); } }
        public decimal BidPrice { get { return GetFirstPrice(false); } }

        public decimal AskAmount { get { return GetFirstAmount(true); } }
        public decimal BidAmount { get { return GetFirstAmount(false); } }

        public SortedDictionary<decimal, ExchangeOrderPrice> Asks { get { return OrderBook.Asks; } }

        public SortedDictionary<decimal, ExchangeOrderPrice> Bids { get { return OrderBook.Bids; } }

        public void GetPriceToBuy(decimal amount, out decimal allowBuyAmount, out decimal adviceBuyPrice)
        {
            this.OrderBook.GetPriceToBuy(amount, out allowBuyAmount, out adviceBuyPrice);
        }

        public decimal GetPriceToSell(decimal amount)
        {
            return this.OrderBook.GetPriceToSell(amount);
        }

        private decimal GetSumAmount(bool IsAsk=true)
        {
            if (this.OrderBook == null || this.OrderBook.Asks == null || this.OrderBook.Bids == null) return 0;
            if (IsAsk == true)
            {
                return OrderBook.Asks.Values.Sum(x => x.Amount);
            }
            else
            {
                return OrderBook.Bids.Values.Sum(x => x.Amount);
            }
        }
        private decimal GetAvgPrice(bool IsAsk = true)
        {
            if (this.OrderBook == null || this.OrderBook.Asks == null || this.OrderBook.Bids == null) return 0;
            if (IsAsk == true)
            {
                return OrderBook.Asks.Values.Average(x => x.Price);
            }
            else
            {
                return OrderBook.Bids.Values.Average(x => x.Price);
            }
        }
        private decimal GetFirstPrice(bool IsAsk=true)
        {
            if (this.OrderBook == null || this.OrderBook.Asks == null || this.OrderBook.Bids == null) return 0;
            if (IsAsk == true)
            {
                return OrderBook.Asks.First().Value.Price;
            }
            else
            {
                return OrderBook.Bids.First().Value.Price;
            }
        }

        private decimal GetFirstAmount(bool IsAsk = true)
        {
            if (this.OrderBook == null || this.OrderBook.Asks == null || this.OrderBook.Bids == null) return 0;
            if (IsAsk == true)
            {
                return OrderBook.Asks.First().Value.Amount;
            }
            else
            {
                return OrderBook.Bids.First().Value.Amount;
            }
        }
    }
}
