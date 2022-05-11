using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Trade
{
    public class PairCurrencyBalance
    {
        public string ExchangeID { get; set; }
        public string Symbol { get; set; }

        public decimal BaseCurrencyAvailable { get; set; }
        public decimal QuoteCurrencyAvailable { get; set; }

        public decimal BaseCurrencyFrozen { get; set; }
        public decimal QuoteCurrencyFrozen { get; set; }       

    }
}
