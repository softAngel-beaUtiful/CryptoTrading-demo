using Newtonsoft.Json;
using System;
using Trady.Core;

namespace TickQuant.Common
{
    public class TQBar : Trady.Core.Infrastructure.IOhlcv
    {
        public string Symbol { get; set; }
        public decimal Open { get; set; }
        public decimal Close { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        [JsonProperty("CurrencyVolume")]
        public decimal Volume { get; set; }
        [JsonProperty("Timestamp")]
        public DateTimeOffset DateTime { get; set; }

        public override string ToString()
        {
            return this.DateTime.ToString("yyyy-MM-dd HH:mm:ss") + "=>High: " + this.High + " Open:" + this.Open + " Low:" + this.Low + " Close:" + this.Close;
        }
    }
}
