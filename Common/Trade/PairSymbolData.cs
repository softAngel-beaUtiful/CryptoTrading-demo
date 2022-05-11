using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Trade
{
    public class PairSymbolData
    {
        public string Symbol { get; set; }
        public decimal Bid { get; set; }
        public decimal Ask { get; set; }
        public decimal Last { get; set; }
        public decimal Bidsize { get; set; }
        public decimal Asksize { get; set; }
        public decimal BidAskSpread { get; set; }
        public decimal DiffPercentage { get; set; }
        public decimal QuoteCurrencyAvailable1 { get; set; }
        public decimal BaseCurrencyAvailable1 { get; set; }
        public decimal QuoteCurrencyAvailable2 { get; set; }
        public decimal BaseCurrencyAvailable2 { get; set; }
        public decimal CurrentPosition { get; set; }
        public PairCurrencyBalance PairCurrency {get; set;}


    }
}
