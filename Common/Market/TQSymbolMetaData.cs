using System;
using System.Collections.Generic;
using System.Text;

namespace TickQuant.Common
{
    public class TQSymbolMetaData
    {
        public string ExchangeName { get; set; }
        public string Symbol { get; set; }
        public string SymbolAlias { get; set; }

        public string Currency { get; set; }

        public string QuoteCurrency { get; set; }

        public decimal MinTradeQuant { get; set; }
        public decimal PriceStep { get; set; }

        public decimal QuantityStep { get; set; }


        public decimal ContractValue { get; set; }
    }
}

