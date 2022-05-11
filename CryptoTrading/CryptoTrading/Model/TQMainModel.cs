using CryptoTrading.TQLib;
using CryptoTrading.ViewModel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoTrading.Model
{
    class TQMainModel
    {
        public static readonly ConcurrentQueue<string> MemLog = new ConcurrentQueue<string>();
        public static readonly TQConcurrentDictionary<string, MajorMarketData> dicMajorMarketData = new TQConcurrentDictionary<string, MajorMarketData>();
        public static readonly Dictionary<string, string> dicProductIDName = new Dictionary<string, string>();
        //public static readonly Dictionary<string, string> dicInstrumentIDName = new Dictionary<string, string>();
        public static readonly Dictionary<string, InstrumentData> dicInstrumentData = new Dictionary<string, InstrumentData>();
        public static readonly TQConcurrentDictionary<string, OrderData> dicOrder = new TQConcurrentDictionary<string, OrderData>();
        public static readonly TQConcurrentDictionary<string, ParkedOrderField> dicParkedOrder = new TQConcurrentDictionary<string, ParkedOrderField>();
        public static readonly TQConcurrentDictionary<string, ComboMarketData> dicAllCustomProductList = new TQConcurrentDictionary<string, ComboMarketData>();
        public static readonly TQConcurrentDictionary<string, ComboMarketData> dicCustomProduct = new TQConcurrentDictionary<string, ComboMarketData>();
        public static ConcurrentQueue<MajorMarketData> QTicks = new ConcurrentQueue<MajorMarketData>();
        public static Dictionary<string, InstrumentData> SubscribedInstrumentIDs = new Dictionary<string, InstrumentData>();
        //public Dictionary<string, InstrumentData> SubscribedInstru = new Dictionary<string, InstrumentData>();
        public static readonly TQConcurrentDictionary<string, TradeData> dicTradeData = new TQConcurrentDictionary<string, TradeData>();
        public static readonly TQConcurrentDictionary<string, TradeDataSummary> dicTradeDataSum = new TQConcurrentDictionary<string, TradeDataSummary>();
        public static readonly TQConcurrentDictionary<string, PositionDetail> dicPositionDetails = new TQConcurrentDictionary<string, PositionDetail>();
        //public static readonly TQConcurrentDictionary<string, PositionData> dicPosition = new TQConcurrentDictionary<string, PositionData>();
        public static readonly TQConcurrentDictionary<string, PositionDataSummary> dicPositionSummary = new TQConcurrentDictionary<string, PositionDataSummary>();               
    }
}
