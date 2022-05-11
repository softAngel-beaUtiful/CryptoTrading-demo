using ExchangeSharp;
using System;

namespace TickQuant.Common
{
    public interface ICloneable<T>
    {
        T Clone();
    }

    public class TQTick : ICloneable<TQTick>
    {
        public string ExchangeID { get; set; }
        /// <summary>
        /// An exchange specific id if known, otherwise null
        /// </summary>
        public string Symbol { get; set; }
        public long TimestampLong
        {
            get { return CryptoUtility.GetTimestampLong(this.Timestamp); }
        }
        public DateTime Timestamp { get; set; }
        public decimal BestBid { get; set; }
        public decimal BestAsk { get; set; }
        public decimal LastPrice { get; set; }
        public ExchangeVolume Volume { get; set; }

        public TQTick Clone()
        {
            return new TQTick
            {
                Timestamp = Timestamp,
                Symbol = Symbol,
                LastPrice = LastPrice,
                Volume = Volume,
                BestBid = BestBid,
                BestAsk = BestAsk,
            };
        }

        public override string ToString()
        {
            return this.Symbol + "=>timestamp: " + this.Timestamp.ToString("yyyy-MM-dd HH:mm:ss") + ", LastPrice: " + this.LastPrice + ", Volume: " + this.Volume;
        }
    }
}
