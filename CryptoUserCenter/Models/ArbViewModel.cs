using CryptoUserCenter.Views;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoUserCenter.Models
{
    public class ChartViewModel
    {
        //private ObservableCollection<CandleStick> _klines;
        // SubscriberSocket subSocket = new SubscriberSocket(">tcp://192.168.2.2:12345");
        public SubscriberSocket subSocket1, subSocket2;
        //public List<ExchangeOrderBook> MajorKLines { get { return _klines; } set { _klines = value; NotifyPropertyChanged("MajorKLines"); } }
        public Dictionary<string, ConcurrentQueue<OrderBook>> DictOrderBookList = new Dictionary<string, ConcurrentQueue<OrderBook>>();

        public List<OrderBook> ComboOrderBook = new List<OrderBook>();
        public (string, string) arbPair;

        public ArbitrageWindow ArbitrageChart;
        public ChartViewModel(ArbitrageWindow chart, (string, string) tulpsymbols)
        {
            arbPair = tulpsymbols;
            DictOrderBookList.Add(arbPair.Item1, new ConcurrentQueue<OrderBook>());
            DictOrderBookList.Add(arbPair.Item2, new ConcurrentQueue<OrderBook>());

            ArbitrageChart = chart;
            subSocket1 = new SubscriberSocket(">tcp://192.168.2.2:12345");
            subSocket2 = new SubscriberSocket(">tcp://192.168.2.2:12345");
            subSocket1.Options.ReceiveHighWatermark = 1000;
            subSocket1.Subscribe("orderbook_" + arbPair.Item1);
            subSocket2.Options.ReceiveHighWatermark = 1000;
            subSocket2.Subscribe("orderbook_" + arbPair.Item2);
            Task.Factory.StartNew(() => ProcessOrderBook1());
            Task.Factory.StartNew(() => ProcessOrderBook2());
        }

        private void ProcessOrderBook1()
        {
            string messageTopicReceived;
            string messageReceived;
            while (true)
            {
                try
                {
                    messageTopicReceived = subSocket1.ReceiveFrameString();
                    var key = messageTopicReceived.Substring(messageTopicReceived.IndexOf("_") + 1);
                    messageReceived = subSocket1.ReceiveFrameString();

                    var t = ParseOrderBook(JToken.Parse(messageReceived), key.Substring(0, key.IndexOf('_')));

                    ProcessForComboOrderbook(t);

                    ArbitrageChart.Dispatcher.Invoke(() =>
                    {
                        ArbitrageChart.UpdateChart(new System.Windows.Size(), false);                      
                    });
                }
                catch (Exception ex)
                {
                    //subSocket1.Disconnect("tcp://192.168.2.2:1234"); 
                    subSocket1.Dispose();
                    return;
                }
            }
        }
        private void ProcessOrderBook2()
        {
            string messageTopicReceived;
            string messageReceived;
            while (true)
            {
                try
                {
                    messageTopicReceived = subSocket2.ReceiveFrameString();

                    var key = messageTopicReceived.Substring(messageTopicReceived.IndexOf("_") + 1);
                    if (key != arbPair.Item2)
                        continue;
                    messageReceived = subSocket2.ReceiveFrameString();

                    var t = ParseOrderBook(JToken.Parse(messageReceived), key.Substring(0, key.IndexOf('_')));

                    ProcessForComboOrderbook(t);

                    ArbitrageChart.Dispatcher.Invoke(() =>
                    {
                        ArbitrageChart.UpdateChart(new System.Windows.Size(), false);
                        //KChart.UpdateAmount(new System.Windows.Size(), false);
                    });
                }
                catch (Exception ex)
                {
                    //subSocket2.Disconnect("tcp://192.168.2.2:12345");
                    subSocket2.Dispose();
                    return;
                }
            }
        }

        private void ProcessForComboOrderbook(OrderBook t)
        {
            string key = (t.ExchangeID + "_" + t.Symbol).ToLower().Replace("-", "_");

            DictOrderBookList[key].Enqueue(t);
            if (DictOrderBookList[arbPair.Item1].Count == 0 || DictOrderBookList[arbPair.Item2].Count == 0)
                return;
            if (DictOrderBookList[key].Count > 2000)
                DictOrderBookList[key].TryDequeue(out OrderBook oo);

            OrderBook book = new OrderBook();
            try
            {
                var book1 = DictOrderBookList[arbPair.Item1].Last();
                var book2 = DictOrderBookList[arbPair.Item2].Last();
                book = new OrderBook()
                {
                    AskPrice1 = book1.AskPrice1 - book2.BidPrice1,
                    BidPrice1 = book1.BidPrice1 - book2.AskPrice1,
                    AskAmount1 = Math.Min(book1.AskAmount1, book2.BidAmount1),
                    BidAmount1 = Math.Min(book1.BidAmount1, book2.AskAmount1),
                    timestamp = t.timestamp
                };
            }
            catch (Exception ex)
            { }
            lock (ComboOrderBook)
            {
                ComboOrderBook.Add(book);
                if (ComboOrderBook.Count > 2000)
                {
                    ComboOrderBook.RemoveAt(0);
                }
            }
        }

        private OrderBook ParseOrderBook(JToken jToken, string exchangeid)
        {
            /*{{ "SequenceId": 0,"MarketSymbol": "BTC-USDT-SWAP","LastUpdatedUtc": "2020-04-24T16:06:51.992883Z", "Asks": {
                        "7512.3": { "Price": 7512.3, "Amount": 612.0 },"7512.5": { "Price": 7512.5, "Amount": 6.0 },
                        "7512.8": {  "Price": 7512.8,"Amount": 310.0 },"7513": { "Price": 7513.0,  "Amount": 1002.0}, 
                        "7513.1": {Price": 7513.1, "Amount": 29.0  } },
                 "Bids": {  "7512.2": { "Price": 7512.2, "Amount": 875.0 },"7512.1": { "Price": 7512.1,"Amount": 34.0 },
                 "7512": { "Price": 7512.0,"Amount": 55.0 }, "7511.8": { "Price": 7511.8,"Amount": 40.0},
                   "7511.7": { "Price": 7511.7, "Amount": 30.0 } } } }
             {{
  "SequenceId": 0,
  "MarketSymbol": "BTC-USD-200626",
  "LastUpdatedUtc": "2020-04-27T02:22:02.1601806Z",
  "Asks": {
    "7678.28": {
      "Price": 7678.28,
      "Amount": 362.0
    },
    "7678.31": {
      "Price": 7678.31,
      "Amount": 136.0
    },
    "7678.58": {
      "Price": 7678.58,
      "Amount": 1.0
    },
    "7678.59": {
      "Price": 7678.59,
      "Amount": 8.0
    },
    "7678.67": {
      "Price": 7678.67,
      "Amount": 1.0
    }
  },
  "Bids": {
    "7681.26": {
      "Price": 7681.26,
      "Amount": 1.0
    },
    "7680.69": {
      "Price": 7680.69,
      "Amount": 1.0
    },
    "7680.25": {
      "Price": 7680.25,
      "Amount": 6.0
    },
    "7678.27": {
      "Price": 7678.27,
      "Amount": 21.0
    },
    "7677.15": {
      "Price": 7677.15,
      "Amount": 3.0
    }
  }
}}      
             */
            var Bids = jToken["Bids"].ToObject<Dictionary<decimal, Dictionary<string, decimal>>>();
            var Asks = jToken["Asks"].ToObject<Dictionary<decimal, Dictionary<string, decimal>>>();
            var ob = new OrderBook()
            {
                BidPrice1 = Bids.First().Key,
                BidAmount1 = Bids.ToList().First().Value["Amount"],
                AskPrice1 = Asks.First().Key,
                AskAmount1 = Asks.ToList().First().Value["Amount"],
                BidPrice2 = Bids.ToList()[1].Key,
                BidAmount2 = Bids.ToList()[1].Value["Amount"],
                AskAmount2 = Asks.ToList()[1].Value["Amount"],
                AskPrice2 = Asks.ToList()[1].Key,
                ExchangeID = exchangeid,
                Symbol = jToken["MarketSymbol"].ToString(),
                timestamp = jToken["LastUpdatedUtc"].ToObject<DateTime>()
            };
            return ob;
        }
    }
}
