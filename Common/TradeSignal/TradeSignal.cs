using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace TickQuant.Common
{
    public class TradeSignal
    {
        [JsonProperty(PropertyName ="signaltime")]
        public DateTime SignalTime { get; set; }
        [JsonProperty(PropertyName = "id")]
        public string ID { get; set; }
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }   //描述+symbol
        [JsonProperty(PropertyName ="exchangeid")]
        public string ExchangeID { get; set; }    //for arbitrage: ex. okexfutures-bitmex
        [JsonProperty(PropertyName = "symbol")]   //
        public string Symbol { get; set; }
        [JsonProperty(PropertyName = "params")]
        public Dictionary<ENumberClass, decimal> Params { get; set; }  // 开仓值，止盈值,移动止盈值,止损值
    }
}
