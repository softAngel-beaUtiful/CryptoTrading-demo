using System;
using System.Collections.Generic;
using System.Text;

namespace ExchangeSharp
{
	public class ExecutionRecord
	{
		public string symbol { get; set; }
		public bool LongOrShort { get; set; }
		public string OrderID { get; set; }
		public string TradeID { get; set; }
		public decimal TradePrice { get; set; }
		public decimal OrderQty { get; set; }
		public string TradeType { get; set; }
		public decimal Fee { get; set; }
		public decimal TradedQty { get; set; }
		public decimal UnTraded { get; set; }
		public bool IsMaker { get; set; }
		public DateTime TradeTime { get; set; }
	}
}
