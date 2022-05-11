using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoUserCenter.Models
{
    public class OrderBook
    {
        public string ExchangeID { get; set; }
        public string Symbol { get; set; }
        public decimal BidPrice1 { get; set; }
        public decimal BidAmount1 { get; set; }
        public decimal AskPrice1 { get; set; }
        public decimal AskAmount1 { get; set; }
        public decimal BidPrice2 { get; set; }
        public decimal BidAmount2 { get; set; }
        public decimal AskPrice2 { get; set; }
        public decimal AskAmount2 { get; set; }
        public DateTime timestamp { get; set; }
    }
}
