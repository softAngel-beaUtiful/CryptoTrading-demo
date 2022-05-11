using ExchangeSharp;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace KChart.Chart
{

    public sealed class ExchangeVolume
    {
        /// <summary>
        /// Last volume update timestamp
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Quote / Price currency - will equal base currency if exchange doesn't break it out by price unit and quantity unit
        /// In BTC-USD, this would be USD
        /// </summary>
        public string QuoteCurrency { get; set; }

        /// <summary>
        /// Amount in units of the QuoteCurrency - will equal BaseCurrencyVolume if exchange doesn't break it out by price unit and quantity unit
        /// In BTC-USD, this would be USD volume
        /// </summary>
        public decimal QuoteCurrencyVolume { get; set; }

        /// <summary>
        /// Base currency
        /// In BTC-USD, this would be BTC
        /// </summary>
        public string BaseCurrency { get; set; }

        /// <summary>
        /// Base currency amount (this many units total)
        /// In BTC-USD this would be BTC volume
        /// </summary>
        public decimal BaseCurrencyVolume { get; set; }

        /// <summary>
        /// Write to a binary writer
        /// </summary>
        /// <param name="writer">Binary writer</param>
        public void ToBinary(BinaryWriter writer)
        {
            writer.Write(Timestamp.ToUniversalTime().Ticks);
            writer.Write(QuoteCurrency);
            writer.Write((double)QuoteCurrencyVolume);
            writer.Write(BaseCurrency);
            writer.Write((double)BaseCurrencyVolume);
        }

        /// <summary>
        /// Read from a binary reader
        /// </summary>
        /// <param name="reader">Binary reader</param>
        public void FromBinary(BinaryReader reader)
        {
            Timestamp = new DateTime(reader.ReadInt64(), DateTimeKind.Utc);
            QuoteCurrency = reader.ReadString();
            QuoteCurrencyVolume = (decimal)reader.ReadDouble();
            BaseCurrency = reader.ReadString();
            BaseCurrencyVolume = (decimal)reader.ReadDouble();
        }
    }
    public class NewExchangeTicker
    {
        /// <summary>
        /// An exchange specific id if known, otherwise null
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The currency pair symbol that this ticker is in reference to
        /// </summary>
        public string MarketSymbol { get; set; }

        /// <summary>
        /// The bid is the price to sell at
        /// </summary>
        public decimal Bid { get; set; }

        /// <summary>
        /// The ask is the price to buy at
        /// </summary>
        public decimal Ask { get; set; }

        /// <summary>
        /// The last trade purchase price
        /// </summary>
        public decimal Last { get; set; }

        /// <summary>
        /// Volume info
        /// </summary>
        public ExchangeVolume Volume { get; set; }

        /// <summary>
        /// Get a string for this ticker
        /// </summary>
        /// <returns>String</returns>
        public override string ToString()
        {
            return string.Format("Bid: {0}, Ask: {1}, Last: {2}", Bid, Ask, Last);
        }

        /// <summary>
        /// Write to writer
        /// </summary>
        /// <param name="writer">Writer</param>
        public void ToBinary(BinaryWriter writer)
        {
            writer.Write((double)Bid);
            writer.Write((double)Ask);
            writer.Write((double)Last);
            Volume.ToBinary(writer);
        }

        /// <summary>
        /// Read from reader
        /// </summary>
        /// <param name="reader">Reader</param>
        public void FromBinary(BinaryReader reader)
        {
            Bid = (decimal)reader.ReadDouble();
            Ask = (decimal)reader.ReadDouble();
            Last = (decimal)reader.ReadDouble();
            Volume = Volume ?? new ExchangeVolume();
            Volume.FromBinary(reader);
        }
    }
    public class ChartViewModel
    {
        private int MaxBufferLength { get; set; }
        public SubscriberSocket subSocket1;// = new SubscriberSocket(">tcp://192.168.7.54:1234");
        public ObservableCollection<CandleStick> MajorKLines { get; set; }
        public MajorGeneralChartControl KChart;
        public bool loop;
        private List<NewExchangeTicker> exchangeTickers;
        public Task myTask;
        public ChartViewModel(MajorGeneralChartControl chart, string inst)
        {
            MaxBufferLength = 256;
            KChart = chart;
            exchangeTickers = new List<NewExchangeTicker>();
            MajorKLines = new ObservableCollection<CandleStick>();
            loop = true;
            myTask = Task.Factory.StartNew(() => ProcessTicker(inst));
        }

        private void ProcessTicker(string inst)
        {
            string messageTopicReceived;
            string messageReceived;


            using (subSocket1 = new SubscriberSocket(">tcp://192.168.2.2:1234"))
            {
                subSocket1.Options.ReceiveHighWatermark = 1000;

                inst = "tickers_" + inst.Substring(0, inst.IndexOf(':')).ToLower() + "_" + inst.Substring(inst.IndexOf(':') + 1).ToLower().Replace('-', '_');
                subSocket1.Subscribe(inst);
                while (loop)
                {
                    try
                    {
                        messageTopicReceived = subSocket1.ReceiveFrameString();
                        var key = messageTopicReceived.Substring(messageTopicReceived.IndexOf("_") + 1);
                        messageReceived = subSocket1.ReceiveFrameString();
                        ConcurrentQueue<NewExchangeTicker> tickers = ParseTickers(messageReceived);
                        foreach (var t1 in tickers)
                        {
                            exchangeTickers.Add(t1);
                            GenerateLastCandle(tickers);
                            DispatcherOperation dispatcherOperation = KChart.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
                            {
                                KChart.UpdateChart(new System.Windows.Size(), false);
                                KChart.UpdateAmount(new System.Windows.Size(), false);
                            }));
                        }
                    }
                    catch (Exception ex)
                    {
                        subSocket1.Dispose();
                        ProcessTicker(inst);
                    }
                }
            }
        }

        private void GenerateLastCandle(ConcurrentQueue<NewExchangeTicker> tickers)
        {
            if (tickers.Count == 0) return;


            foreach (var h in tickers)
            {
                if (MajorKLines.Count == 0 || MajorKLines.Count > 0 && h.Volume.Timestamp.Minute != MajorKLines.Last().Datetime.Minute)
                {
                    CandleStick candleStick = new CandleStick();
                    candleStick.High = candleStick.Low = candleStick.Open = candleStick.Close = (double)h.Last;
                    candleStick.Volume = (double)h.Volume.QuoteCurrencyVolume;
                    candleStick.Datetime = h.Volume.Timestamp;

                    MajorKLines.Add(candleStick);
                    if (MajorKLines.Count > MaxBufferLength)
                        MajorKLines.RemoveAt(0);
                }
                else
                {
                    if ((double)h.Last > MajorKLines.Last().High)
                        MajorKLines.Last().High = (double)h.Last;
                    if ((double)h.Last < MajorKLines.Last().Low)
                        MajorKLines.Last().Low = (double)h.Last;
                    MajorKLines.Last().Close = (double)h.Last;
                    MajorKLines.Last().Volume = (double)h.Volume.QuoteCurrencyVolume;
                }
            }

        }

        private ConcurrentQueue<NewExchangeTicker> ParseTickers(string messageReceived)
        {
            /*[{"Key":"BTCUSDT",
             * "Value":{"Id":null,"MarketSymbol":"BTCUSDT","Bid":7684.79,"Ask":7685.25,"Last":7685.44,
             * "Volume":{"Timestamp":"2020-04-28T04:13:41.001Z","QuoteCurrency":"USDT","QuoteCurrencyVolume":443408521.31803739,
             * "BaseCurrency":"BTC","BaseCurrencyVolume":57618.736366}}}]*/

            ConcurrentQueue<NewExchangeTicker> tickers = new ConcurrentQueue<NewExchangeTicker>();
            var j = JToken.Parse(messageReceived);
            foreach (var t in (JArray)j)
            {
                var l = t["Value"].ToString();
                var f = JsonConvert.DeserializeObject<NewExchangeTicker>(l);

                tickers.Enqueue(f);
            }
            return tickers;
        }

        private CandleStick ParseKLine(JToken jToken)
        {

            CandleStick cs = new CandleStick()
            {
                Close = jToken["ClosePrice"].ToObject<double>(),
                Datetime = jToken["Timestamp"].ToObject<DateTime>(),
                Open = jToken["OpenPrice"].ToObject<double>(),
                High = jToken["HighPrice"].ToObject<double>(),
                Low = jToken["LowPrice"].ToObject<double>(),
                Volume = jToken["BaseCurrencyVolume"].ToObject<double>()
            };
            return cs;
        }

        public void StopReceiving()
        {
            loop = false;
            //throw new NotImplementedException();
        }
    }
}
