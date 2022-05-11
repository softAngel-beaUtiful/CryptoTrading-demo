using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoTrading.BitMex
{
    public class InstrumentData
    {
        public string symbol;
        public long totalVolume;
        public long volume;
        public long totalTurnover;
        public long turnover;
        public long openInterest;
        public long openValue;
        public DateTime timestamp;
    }   
    public class BMOrderBook10
    {
        public string table;
        public string action;
        public BMOrderBook10Data[] data;
    }
    public class BMOrderBook10Data
    {
        public string symbol;
        public double[][] bids;
        public DateTime timestamp;
        public double[][] asks;
    }
    public class BMQuote
    {
        public DateTime timestamp;
        public DateTime Timestamp => timestamp;
        public string InstrumentID { get => symbol; set => symbol = value; }
        public string symbol;
        public int BidSize;      
        public double BidPrice;
        public int AskSize;
        public double AskPrice;       
    }
    public class OrderBookL2Data
    {
        public string symbol;
        public long id;
        public string side;
        public int size;
    }   
    public class BMOrderBookL2
    {
        public string table;
        public string action;
        public OrderBookL2Data[] data;
    }    
}
