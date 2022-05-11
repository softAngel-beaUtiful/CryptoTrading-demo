using Common.Trade;
using MySql.Data.MySqlClient;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using TickQuant.Common;
using Trady.Analysis;
using Trady.Analysis.Extension;
using Trady.Core.Infrastructure;

namespace MajorControl
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
    public class PairTicker
    {
        /// <summary>
        /// An exchange specific id if known, otherwise null
        /// </summary>
        public string ExchangeID { get; set; }

        /// <summary>
        /// The currency pair symbol that this ticker is in reference to
        /// </summary>
        public string Symbol { get; set; }

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

        public decimal Bidsize { get; set; }
        public decimal Asksize { get; set; }
        /// <summary>
        /// Volume info
        /// </summary>
        public ExchangeVolume Volume { get; set; }
        public PairCurrencyBalance CurrencyBalance { get; set; }
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
        private int MaxBufferLength { get; set; } = 400;
        public SubscriberSocket subSocket1, subSocket2;                
        public MajorChartControl Chart;
        public bool loop;
        
        public Task myTask, myTask1, myTask2;
        public Thread thread1, thread2;
        public List<BidAskSpreadData> MajorBidAskSpreadCollection { get; set; } = new List<BidAskSpreadData>();
        public List<PairTicker> ExchangeTickers = new List<PairTicker>();
        public List<PairTicker> ExchangeTickers0 = new List<PairTicker>();
        public List<PairTicker> ExchangeTickers1 = new List<PairTicker>();
        public string Instrument;
        public ArbitragePair arbitragePair;

        //public MYSQLHELPER helper;
        public MysqlConfig mySqlConfig = Config.GetMysqlConfig();
        string InitconnectionString = "Server={0};port=3306;database={1};UID={2};Password={3}";
        string connectionString;
        CancellationTokenSource canceller;
        public List<ArbitrageCandle> CurrentKLines { get; set; } = new List<ArbitrageCandle>();        
        public List<ArbitrageCandle> MinuteKLines { get; set; } = new List<ArbitrageCandle>();           
        public ChartConfig chartconfig;
        public bool ContinueReceivingData = true;
        public ChartViewModel(MajorChartControl chart, ChartConfig config)
        {
            MaxBufferLength = 200;
            Chart = chart;
            chartconfig = config;
            
            //chartconfig = chart.myChartConfig;
            connectionString = string.Format(InitconnectionString, mySqlConfig.Ip, "setting", mySqlConfig.User, mySqlConfig.PassWord);
            loop = true;
            Instrument = config.ComplexInstrument.Key;
            if (Instrument.Contains("Spread") || Instrument.Contains("Ratio"))
            {
                arbitragePair = new ArbitragePair(Instrument);               
                var inst1 = arbitragePair.Instrument1;
                var inst2 = arbitragePair.Instrument2;
                inst1 = "tickers_" + inst1.Substring(0, inst1.IndexOf(':')).ToLower() + "_" + inst1.Substring(inst1.IndexOf(':') + 1).ToLower().Replace('-', '_');
                inst2 = "tickers_" + inst2.Substring(0, inst2.IndexOf(':')).ToLower() + "_" + inst2.Substring(inst2.IndexOf(':') + 1).ToLower().Replace('-', '_');                
                MinuteKLines.AddRange(MYSQLHELPER.QueryKlineFromTable("histpair", Instrument, Chart.myChartConfig.MaxLookBackPeriod));               
                ConvertKLinesFromBaseKLines();
                canceller = new CancellationTokenSource();
                myTask1 = Task.Factory.StartNew(() =>
                {
                    try
                    {
                        ProcessTicker1(inst1);
                    }
                    catch (ThreadAbortException)
                    {
                    }
                    //normally, it won't go here until some exceptions occur.
                    //if (canceller.Token.IsCancellationRequested)
                    //    canceller.Token.ThrowIfCancellationRequested();
                }, canceller.Token);
                myTask2 = Task.Factory.StartNew(() =>
                {
                    try
                    {
                        ProcessTicker2(inst2);
                    }
                    catch (ThreadAbortException)
                    {
                    }
                    //if (canceller.Token.IsCancellationRequested)
                    //    canceller.Token.ThrowIfCancellationRequested();
                }, canceller.Token);               
            }
            else
            {
                canceller = new CancellationTokenSource();
                var instr = "tickers_"+Instrument.Substring(0, Instrument.IndexOf(':')).ToLower() + "_" + Instrument.Substring(Instrument.IndexOf(':') + 1).ToLower().Replace('-', '_');
                MinuteKLines.AddRange(MYSQLHELPER.QueryKlineFromTable("his_kline", Instrument, Chart.myChartConfig.MaxLookBackPeriod));
                ConvertKLinesFromBaseKLines();
                myTask = Task.Factory.StartNew(() => 
                {
                    try
                    {
                        ProcessTicker(instr);
                    }
                    catch (Exception exception)
                    {
                    }
                    //execution never comes here           
                    //if (canceller.Token.IsCancellationRequested)
                    //    canceller.Token.ThrowIfCancellationRequested();
                }, canceller.Token);               
            }
            //myTask.Dispose();
        }

        public void ConvertKLinesFromBaseKLines()
        {                             
            if (MinuteKLines is null || MinuteKLines.Count == 0) return;
            //ArbitrageCandle cs;
            //CandleStick minute period number: 1,5,15,30,60,240,720 etc.
            lock (CurrentKLines)
            {
                CurrentKLines.Clear();
            }
            foreach (var h in MinuteKLines)
            {
                if (CurrentKLines.Count == 0 || (int)(h.Datetime.Minute/Chart.myChartConfig.CandlePeriod)*Chart.myChartConfig.CandlePeriod==h.Datetime.Minute)
                {
                    ArbitrageCandle candleStick = new ArbitrageCandle();
                    candleStick.High = h.High;
                    candleStick.Low = h.Low;
                    candleStick.Open = h.Open;
                    candleStick.Close = h.Close;
                    candleStick.SClose = h.SClose;
                    candleStick.SOpen = h.SOpen;
                    candleStick.SHigh = h.SHigh;
                    candleStick.SLow = h.SLow;  // (double)h.;                   
                    candleStick.Datetime = h.Datetime;
                    lock (CurrentKLines)
                    {
                        CurrentKLines.Add(candleStick);
                        var dd = (chartconfig.ComplexInstrument.Value[EIndicatorType.BOLLBAND] as Tuple<int, double>).Item1;
                        if (CurrentKLines.Count >= dd)
                        {
                            var a = chartconfig.ComplexInstrument.Value.Values.Last() as Tuple<int, double>;                                                       
                            var hh = CurrentKLines.Select(new Func<ArbitrageCandle, decimal>(x => x.High)).Bb(a.Item1, (decimal)a.Item2, CurrentKLines.Count - 10);
                            var ll = CurrentKLines.Select(new Func<ArbitrageCandle, decimal>(x => x.Low)).Bb(a.Item1, (decimal)a.Item2, CurrentKLines.Count - 10);
                            var mm = CurrentKLines.Select(new Func<ArbitrageCandle, decimal>(t => t.Close)).Bb(a.Item1, (decimal)a.Item2, CurrentKLines.Count - 10);

                            AnalyzableTick<(decimal?, decimal?, decimal?)> analyzableTick = new AnalyzableTick<(decimal?, decimal?, decimal?)>
                                (CurrentKLines[CurrentKLines.Count - 2].Datetime,
                                (ll[ll.Count - 2].LowerBand, mm[mm.Count - 2].MiddleBand, hh[hh.Count - 2].UpperBand));
                            AnalyzableTick<(decimal?, decimal?, decimal?)>[] analyzableTicks = new AnalyzableTick<(decimal?, decimal?, decimal?)>[] { analyzableTick };
                            CurrentKLines[CurrentKLines.Count - 2].IndicatorValues = analyzableTicks;
                            CurrentKLines.Last().IndicatorValues = analyzableTicks;
                        }
                    }
                }
                else
                {
                    var s = CurrentKLines.Last();
                    if (h.High > CurrentKLines.Last().High)
                        s.High = h.High;
                    if (h.Low < CurrentKLines.Last().Low)
                        s.Low = h.Low;
                    if (h.SHigh> CurrentKLines.Last().SHigh)
                        s.SHigh = h.SHigh;
                    if (h.SLow < CurrentKLines.Last().SLow)
                        s.SLow = h.SLow;
                    s.SClose = h.SClose;
                    s.Close = h.Close;
                }               
            }            
        }
        
        bool ProcessTicker(string inst)
        {
            string messageTopicReceived;
            string messageReceived;

            using (subSocket1 = new SubscriberSocket(">tcp://192.168.2.2:1234"))
            {
                subSocket1.Options.ReceiveHighWatermark = 1000;

                subSocket1.Subscribe(inst);
                try
                {
                    while (ContinueReceivingData)
                    {
                        messageTopicReceived = subSocket1.ReceiveFrameString();
                        
                        messageReceived = subSocket1.ReceiveFrameString();
                        if (messageTopicReceived != inst)
                            continue;
                        ConcurrentQueue<PairTicker> tickers = ParseTickers(messageReceived);
                                               
                        GenerateLastCandle(tickers);
                        if (ExchangeTickers.Count > MaxBufferLength)
                            ExchangeTickers.RemoveRange(0, ExchangeTickers.Count -MaxBufferLength);
                        DispatcherOperation dispatcherOperation = Chart.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
                        {
                            Chart.UpdateChart(new System.Windows.Size(), false);
                            Chart.UpdateIndicator(new System.Windows.Size(), false);
                        }));
                    }
                }
                catch (ThreadAbortException e)
                {
                    Trace.WriteLine("1ThreadAbortException is caught");
                }
                finally
                {
                    Trace.WriteLine("1couldnot catch threadabort exception");
                }
                return true;
            }
        }

        private bool ProcessTicker1(string inst)
        {
            string messageTopicReceived;
            string messageReceived;
            SubscriberSocket subSocket1;
            using (subSocket1 = new SubscriberSocket(">tcp://192.168.2.2:1234"))
            {
                subSocket1.Options.ReceiveHighWatermark = 1000;                
                subSocket1.Subscribe(inst);
                try
                {
                    while (ContinueReceivingData)
                    {
                        //continue;
                        messageTopicReceived = subSocket1.ReceiveFrameString();
                        messageReceived = subSocket1.ReceiveFrameString();
                        if (messageTopicReceived != inst)
                            continue;
                        var tickers = ParseTickers(messageReceived);
                        ExchangeTickers0.AddRange(tickers);
                        
                        if (ExchangeTickers0.Count > MaxBufferLength)
                        {
                            ExchangeTickers0.RemoveRange(0, ExchangeTickers0.Count - MaxBufferLength);
                        }
                        GenerateLastPairData(tickers);
                        Trace.TraceInformation(tickers.Last().Last.ToString());
                    }
                    Trace.TraceInformation("stop receiving market data");
                }
                catch (ThreadAbortException e)
                {
                    Trace.WriteLine("2ThreadAbortException is caught");
                }
                finally
                {
                    Trace.WriteLine("2couldnot catch threadabort exception");
                }
                return true;
            }
        }
        private bool ProcessTicker2(string inst)
        {
            string messageTopicReceived;
            string messageReceived;
            SubscriberSocket subSocket2;
            using (subSocket2 = new SubscriberSocket(">tcp://192.168.2.2:1234"))
            {
                subSocket2.Options.ReceiveHighWatermark = 1000;               
                subSocket2.Subscribe(inst);
                try
                {
                    while (ContinueReceivingData)
                    {
                        messageTopicReceived = subSocket2.ReceiveFrameString();                        
                        messageReceived = subSocket2.ReceiveFrameString();
                        if (messageTopicReceived != inst)
                            continue;
                        var tickers = ParseTickers(messageReceived);
                        ExchangeTickers1.AddRange(tickers);                           
                        
                    if (ExchangeTickers1.Count > MaxBufferLength)
                        ExchangeTickers1.RemoveRange(0, ExchangeTickers1.Count - MaxBufferLength);
                    GenerateLastPairData(tickers);
                    }
                    Trace.TraceInformation("stop receiving 2");
                }
                catch (ThreadAbortException ex)
                {
                    Trace.WriteLine("3ThreadAbortException is caught");
                }
                finally { Trace.WriteLine("3ThreadAbortException couldn't be caught"); }
                return true;
            }
        }

        void GenerateLastPairData(ConcurrentQueue<PairTicker> tickers)
        {
            if (tickers is null || tickers.Count == 0)
                return;
            BidAskSpreadData temppair= new BidAskSpreadData();
            if (ExchangeTickers0.Count > 0 && ExchangeTickers1.Count > 0 && tickers.Last().Volume.Timestamp < ExchangeTickers1.Last().Volume.Timestamp.AddMinutes(1)
                && tickers.Last().Volume.Timestamp > ExchangeTickers1.Last().Volume.Timestamp.AddMinutes(-1))
            {
                if (arbitragePair.DiffOrRatio == "-")
                {
                    temppair.BidAskSpread = ExchangeTickers0.Last().Ask - ExchangeTickers1.Last().Bid - ExchangeTickers0.Last().Bid + ExchangeTickers1.Last().Ask;
                    temppair.MidPrice = (ExchangeTickers0.Last().Bid - ExchangeTickers1.Last().Ask + ExchangeTickers0.Last().Ask - ExchangeTickers1.Last().Bid) / 2;
                    temppair.Datetime = tickers.Last().Volume.Timestamp;                    
                }
                else                    
                {
                    temppair.BidAskSpread = (ExchangeTickers0.Last().Ask - ExchangeTickers1.Last().Bid - ExchangeTickers0.Last().Bid + ExchangeTickers1.Last().Ask)
                        / ExchangeTickers1.Last().Bid;
                    temppair.MidPrice = (ExchangeTickers0.Last().Ask * ExchangeTickers1.Last().Ask + ExchangeTickers0.Last().Bid * ExchangeTickers1.Last().Bid)
                        / (ExchangeTickers1.Last().Bid * ExchangeTickers1.Last().Ask) / 2;
                    temppair.Datetime = tickers.Last().Volume.Timestamp;                    
                }
                lock (MajorBidAskSpreadCollection)
                {
                    MajorBidAskSpreadCollection.Add(temppair);
                    if (MajorBidAskSpreadCollection.Count > MaxBufferLength)
                        MajorBidAskSpreadCollection.RemoveAt(0);
                    GenerateLastCandle(temppair);
                }
            }
        }

        private void GenerateLastCandle(BidAskSpreadData pairdata)
        {//using BidAskSpreadData to update the arbitrage candle data, using midprice instead of last price or close price to get more acurate
            if (pairdata is null) return;
            //CandleStick minute period number: 1,5,15,30,60,240,720 etc.           
            if (MinuteKLines.Count == 0 || MinuteKLines.Count > 0 && pairdata.Datetime.Minute != MinuteKLines.Last().Datetime.Minute)
            //&& h.Datetime.Minute != MajorPairDataCollection[MajorPairDataCollection.Count-2].Datetime.Minute)
            {
                ArbitrageCandle candleStick = new ArbitrageCandle();
                candleStick.High = candleStick.Low = candleStick.Open = candleStick.Close = pairdata.MidPrice;
                candleStick.SHigh = candleStick.SLow = candleStick.SOpen = candleStick.SClose = pairdata.BidAskSpread;//candleStick.Volume = (double)h.Volume.QuoteCurrencyVolume;
                candleStick.Datetime = pairdata.Datetime;

                MinuteKLines.Add(candleStick);
                while (MinuteKLines.Count > 1200)
                    MinuteKLines.RemoveAt(0);
                if (MinuteKLines.Count > 1 && MajorBidAskSpreadCollection.Count > 1 &&
                    MajorBidAskSpreadCollection[MajorBidAskSpreadCollection.Count - 2].Datetime.AddMinutes(1).Minute == MajorBidAskSpreadCollection.Last().Datetime.Minute)
                {
                    try
                    {
                        MYSQLHELPER.InsertKlineIntoTable(Instrument, MinuteKLines[MinuteKLines.Count - 2]);
                    }
                    catch (Exception ex)
                    { }
                }
            }
            else
            {
                var s = MinuteKLines.Last();
                if (pairdata.MidPrice > MinuteKLines.Last().High)
                    s.High = pairdata.MidPrice;
                if (pairdata.MidPrice < MinuteKLines.Last().Low)
                    s.Low = pairdata.MidPrice;
                s.Close = pairdata.MidPrice;
                if (pairdata.BidAskSpread > MinuteKLines.Last().SHigh)
                    s.SHigh = pairdata.BidAskSpread;
                if (pairdata.BidAskSpread < MinuteKLines.Last().SLow)
                    s.SLow = pairdata.BidAskSpread;
                s.SClose = pairdata.BidAskSpread;
            }

            if (CurrentKLines.Count == 0)      //initialize first candle
            {
                ArbitrageCandle candleStick = new ArbitrageCandle();
                candleStick.High = candleStick.Low = candleStick.Open = candleStick.Close = pairdata.MidPrice;
                candleStick.SHigh = candleStick.SLow = candleStick.SOpen = candleStick.SClose = pairdata.BidAskSpread;
                candleStick.Datetime = pairdata.Datetime;
                candleStick.Datetime.AddMinutes((int)(pairdata.Datetime.Minute / Chart.myChartConfig.CandlePeriod) * Chart.myChartConfig.CandlePeriod - candleStick.Datetime.Minute);
                CurrentKLines.Add(candleStick);
             }
            else
            {
                var currentdatetime = DateTime.Parse(pairdata.Datetime.ToString("yyyy-MM-dd HH:mm"));
                var lastdatetime = DateTime.Parse(CurrentKLines.Last().Datetime.AddMinutes(Chart.myChartConfig.CandlePeriod).ToString("yyyy-MM-dd HH:mm"));
                //handle a gap between downloaded data and the newest data
                if (pairdata.Datetime < lastdatetime)   //update current candle data
                {
                    var s = CurrentKLines.Last();
                    if (pairdata.MidPrice > CurrentKLines.Last().High)
                        s.High = pairdata.MidPrice;
                    if (pairdata.MidPrice < CurrentKLines.Last().Low)
                        s.Low = pairdata.MidPrice;
                    s.Close = pairdata.MidPrice;
                    if (pairdata.BidAskSpread > CurrentKLines.Last().SHigh)
                        s.SHigh = pairdata.BidAskSpread;
                    if (pairdata.BidAskSpread < CurrentKLines.Last().SLow)
                        s.SLow = pairdata.BidAskSpread;
                    s.SClose = pairdata.BidAskSpread;
                }
                else
                {
                    if (currentdatetime == lastdatetime)
                    {
                        ArbitrageCandle candleStick = new ArbitrageCandle();
                        candleStick.High = candleStick.Low = candleStick.Open = candleStick.Close = pairdata.MidPrice;
                        candleStick.SHigh = candleStick.SLow = candleStick.SOpen = candleStick.SClose = pairdata.BidAskSpread;
                        candleStick.Datetime = pairdata.Datetime;
                        candleStick.Datetime.AddMinutes((int)(pairdata.Datetime.Minute / Chart.myChartConfig.CandlePeriod) * Chart.myChartConfig.CandlePeriod - candleStick.Datetime.Minute);
                        CurrentKLines.Add(candleStick);
                        var a = chartconfig.ComplexInstrument.Value.Values.Last() as Tuple<int, double>;
                        var hh = CurrentKLines.Select(new Func<ArbitrageCandle, decimal>(x => x.High)).Bb(a.Item1, (decimal)a.Item2, CurrentKLines.Count - 10);
                        var ll = CurrentKLines.Select(new Func<ArbitrageCandle, decimal>(x => x.Low)).Bb(a.Item1, (decimal)a.Item2, CurrentKLines.Count - 10);
                        var mm = CurrentKLines.Select(new Func<ArbitrageCandle, decimal>(t => t.Close)).Bb(a.Item1, (decimal)a.Item2, CurrentKLines.Count - 10);

                        var analyzableTick = new AnalyzableTick<(decimal?, decimal?, decimal?)>
                            (CurrentKLines[CurrentKLines.Count - 2].Datetime, (ll[ll.Count - 2].LowerBand, mm[mm.Count - 2].MiddleBand, hh[hh.Count - 2].UpperBand));
                        var analyzableTicks = new AnalyzableTick<(decimal?, decimal?, decimal?)>[] { analyzableTick };

                        CurrentKLines[CurrentKLines.Count - 2].IndicatorValues = analyzableTicks;
                        CurrentKLines.Last().IndicatorValues = analyzableTicks;
                        if (CurrentKLines.Count > 400)
                            CurrentKLines.RemoveAt(0);
                    }
                    else //cutdatetime> lastdatetime
                    {
                        int y = (int)((currentdatetime - lastdatetime).TotalMinutes / Chart.myChartConfig.CandlePeriod);
                        if (y > 0)
                        {
                            var l = CurrentKLines.Last();
                            for (int i = 0; i < y; i++)
                                CurrentKLines.Add(new ArbitrageCandle()
                                {
                                    Close = l.Close,
                                    Datetime = l.Datetime.AddSeconds((-1) * l.Datetime.Second).AddMinutes((i + 1) * Chart.myChartConfig.CandlePeriod),
                                    High = l.Close,
                                    Open = l.Close,
                                    Low = l.Close,
                                    SClose = l.SClose,
                                    SHigh = l.SClose,
                                    SLow = l.SClose,
                                    SOpen = l.SClose
                                });
                        }
                        ArbitrageCandle candleStick = new ArbitrageCandle();
                        candleStick.High = candleStick.Low = candleStick.Open = candleStick.Close = pairdata.MidPrice;
                        candleStick.SHigh = candleStick.SLow = candleStick.SOpen = candleStick.SClose = pairdata.BidAskSpread;
                        candleStick.Datetime = pairdata.Datetime;
                        candleStick.Datetime.AddMinutes((int)(pairdata.Datetime.Minute / Chart.myChartConfig.CandlePeriod) * Chart.myChartConfig.CandlePeriod - candleStick.Datetime.Minute);

                        CurrentKLines.Add(candleStick);
                        var a = chartconfig.ComplexInstrument.Value.Values.Last() as Tuple<int, double>;
                        var hh = CurrentKLines.Select(new Func<ArbitrageCandle, decimal>(x => x.High)).Bb(a.Item1, (decimal)a.Item2, CurrentKLines.Count - 10);
                        var ll = CurrentKLines.Select(new Func<ArbitrageCandle, decimal>(x => x.Low)).Bb(a.Item1, (decimal)a.Item2, CurrentKLines.Count - 10);
                        var mm = CurrentKLines.Select(new Func<ArbitrageCandle, decimal>(t => t.Close)).Bb(a.Item1, (decimal)a.Item2, CurrentKLines.Count - 10);

                        AnalyzableTick<(decimal?, decimal?, decimal?)> analyzableTick = new AnalyzableTick<(decimal?, decimal?, decimal?)>
                            (CurrentKLines[CurrentKLines.Count - 2].Datetime,
                            (ll[ll.Count - 2].LowerBand, mm[mm.Count - 2].MiddleBand, hh[hh.Count - 2].UpperBand));
                        AnalyzableTick<(decimal?, decimal?, decimal?)>[] analyzableTicks = new AnalyzableTick<(decimal?, decimal?, decimal?)>[] { analyzableTick };
                        CurrentKLines[CurrentKLines.Count - 2].IndicatorValues = analyzableTicks;
                        CurrentKLines.Last().IndicatorValues = analyzableTicks;
                    }
                }
            }

            //DispatcherOperation dispatcherOperation = 
            Chart.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() =>
            {
                Chart.UpdateChart(new System.Windows.Size(), false);
                Chart.UpdateIndicator(new System.Windows.Size(), false);
            }));                  
        }


        private void GenerateLastCandle(ConcurrentQueue<PairTicker> tickers)
        {
            if (tickers.Count == 0) return;

            foreach (var h in tickers)
            {
                var l = MinuteKLines.Last();
                if (MinuteKLines.Count == 0 || (MinuteKLines.Count > 0 && 
                    h.Volume.Timestamp > MinuteKLines.Last().Datetime.AddSeconds((-1) * l.Datetime.Second).AddMinutes(1)))
                {
                    ArbitrageCandle candleStick = new ArbitrageCandle();
                    candleStick.High = candleStick.Low = candleStick.Open = candleStick.Close = h.Last;                   
                    candleStick.Datetime = h.Volume.Timestamp;
                    MinuteKLines.Add(candleStick);
                    if (MinuteKLines.Count > 1000)
                        MinuteKLines.RemoveRange(0, MinuteKLines.Count - 1000);
                }
                else
                {
                    if (h.Last > MinuteKLines.Last().High)
                        MinuteKLines.Last().High = h.Last;
                    if (h.Last < MinuteKLines.Last().Low)
                        MinuteKLines.Last().Low = h.Last;
                    MinuteKLines.Last().Close = h.Last;                    
                }

                if (CurrentKLines.Count == 0 || (CurrentKLines.Count > 0 &&  h.Volume.Timestamp.Minute ==
                    CurrentKLines.Last().Datetime.AddSeconds((-1) * l.Datetime.Second).AddMinutes(Chart.myChartConfig.CandlePeriod).Minute))
                {
                    ArbitrageCandle candleStick = new ArbitrageCandle();
                    candleStick.High = candleStick.Low = candleStick.Open = candleStick.Close = h.Last;
                    candleStick.Datetime = h.Volume.Timestamp;
                    candleStick.Datetime.AddMinutes(candleStick.Datetime.Minute-(int)(h.Volume.Timestamp.Minute/Chart.myChartConfig.CandlePeriod));
                    CurrentKLines.Add(candleStick);
                    var d = CurrentKLines.Bb(100, 2);
                    var d1 = d[d.Count - 2];
                    CurrentKLines[CurrentKLines.Count-2].IndicatorValues = new AnalyzableTick<(decimal?, decimal?, decimal?)>[] { d1 };
                    CurrentKLines[CurrentKLines.Count - 1].IndicatorValues = new AnalyzableTick<(decimal?, decimal?, decimal?)>[] { d1 };
                }
                else
                {
                    if (h.Last > CurrentKLines.Last().High)
                        CurrentKLines.Last().High = h.Last;
                    if (h.Last < CurrentKLines.Last().Low)
                        CurrentKLines.Last().Low = h.Last;
                    CurrentKLines.Last().Close = h.Last;
                }
            }
        }

        private ConcurrentQueue<PairTicker> ParseTickers(string messageReceived)
        {
            /*[{"Key":"BTCUSDT",
             * "Value":{"Id":null,"MarketSymbol":"BTCUSDT","Bid":7684.79,"Ask":7685.25,"Last":7685.44,
             * "Volume":{"Timestamp":"2020-04-28T04:13:41.001Z","QuoteCurrency":"USDT","QuoteCurrencyVolume":443408521.31803739,
             * "BaseCurrency":"BTC","BaseCurrencyVolume":57618.736366}}}]*/

            ConcurrentQueue<PairTicker> tickers = new ConcurrentQueue<PairTicker>();
            var j = JToken.Parse(messageReceived);
            foreach (var t in (JArray)j)
            {
                var l = t["Value"].ToString();
                var f = JsonConvert.DeserializeObject<PairTicker>(l);

                tickers.Enqueue(f);
            }
            return tickers;
        }

        private ArbitrageCandle ParseKLine(JToken jToken)
        {

            ArbitrageCandle cs = new ArbitrageCandle()
            {
                Close = jToken["ClosePrice"].ToObject<decimal>(),
                Datetime = jToken["Timestamp"].ToObject<DateTime>(),
                Open = jToken["OpenPrice"].ToObject<decimal>(),
                High = jToken["HighPrice"].ToObject<decimal>(),
                Low = jToken["LowPrice"].ToObject<decimal>(),
                //Volume = jToken["BaseCurrencyVolume"].ToObject<double>()
            };
            return cs;
        }

        public void StopReceiving()
        {
            ContinueReceivingData = false;
            canceller.Cancel();
            
            //myTask.Dispose();
            //myTask1.Dispose();           
        }

        private string GetConnectionString(string dbName)
        {
            var mysqlConfig = mySqlConfig;
            string UserId = mysqlConfig.User;
            string PassWord = mysqlConfig.PassWord;
            string Mysql_IP = mysqlConfig.Ip;
            string InitconnectionString = "Server={0};port=3306;database={1};UID={2};Password={3}";
            return string.Format(InitconnectionString, Mysql_IP, dbName, UserId, PassWord);
        }       
    }

    public class BidAskSpreadData
    {
        public decimal MidPrice;
        public decimal BidAskSpread;
        /*public double BidQuant; 
        public double AskQuant;    */
        public DateTime Datetime;
    }
   
    public class ArbitragePair
    {
        public string Instrument1;
        public string Instrument2;
        public string DiffOrRatio;
        public ArbitragePair(string str)
        {
            if (str.Length > 25)
            {
                var i = str.Split(new char[] { ' ' });
                if (i.Length > 2)
                {
                    Instrument1 = i[0];
                    Instrument2 = i[2];
                    DiffOrRatio = i[1] == "Spread" ? "-" : "/";               
                    return;
                }
            }
            Instrument1 = str;
        }
    }        
}
