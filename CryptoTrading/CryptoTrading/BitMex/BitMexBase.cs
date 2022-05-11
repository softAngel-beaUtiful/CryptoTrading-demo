using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CryptoTrading.ViewModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using CryptoTrading;
using Newtonsoft.Json.Serialization;
using System.Reactive.Subjects;
using CryptoTrading.Model;
using SimpleLogger;

namespace BitMex
{
    public partial class BitMexBase
    {
        public BitMexWebsocketConnector clientWebSocketconnector;
        private MarketAccess marketaccess;
        private int rateLimit;
        internal System.Timers.Timer timer;
        private const string uri = "wss://www.bitmex.com/realtime";
        private const string domain = "https://www.bitmex.com";
        private byte[] buffer = new byte[1024 * 2];
        static List<string> listofmessage = new List<string>();
        public event EventHandler<TradingAccountData> OnRtnWallet;
        public event EventHandler<TradeData[]> OnRtnTradeData;
        public event EventHandler<TradeData[]> OnRtnExecution;
        public event EventHandler<Margin[]> OnRtnMargin;
        public event EventHandler<OrderData[]> OnRtnOrder;
        public event EventHandler<Position[]> OnRtnPosition;
        //public event EventHandler<Transact[]> OnRtnTransact;
        static object _forlock = new object();
        public List<string> SubInstrumentIDList = new List<string>() { ".BXBT" };
        private readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1);
        private DateTime INITIALIZEDTIME = DateTime.Now;
        private bool Initialized = false;
        private long Nonce { get; set; }
        public BitMexBase(string userid, Exchange exch, int rateLimit = 5000)
        {
            //string path = Environment.CurrentDirectory + "\\configurations\\BitMexConfigs";
            //var key = File.ReadAllText(path + "\\BitMexApiKey.txt");
            marketaccess = new MarketAccess() {
                UserID = userid,
                api_key = exch.PublicKey,
                secret_key = exch.PrivateKey,
                
            };//JsonConvert.DeserializeObject<MarketAccess>(key);            
            clientWebSocketconnector = new BitMexWebsocketConnector(new Uri(uri));
            this.rateLimit = rateLimit;
            clientWebSocketconnector.OnRtnTxtMsg += BM_OnMessage;
            
            //timer = new System.Timers.Timer();
            //timer.Interval = 2000;
            //timer.Elapsed += Timer_Elapsed;
        }

        #region RateLimiter

        private long lastTicks = 0;
        private object thisLock = new object();

        private void RateLimit()
        {
            lock (thisLock)
            {
                long elapsedTicks = DateTime.Now.Ticks - lastTicks;
                var timespan = new TimeSpan(elapsedTicks);
                if (timespan.TotalMilliseconds < rateLimit)
                    Thread.Sleep(rateLimit - (int)timespan.TotalMilliseconds);
                lastTicks = DateTime.Now.Ticks;
            }
        }

        #endregion RateLimiter
        public async Task GetAuthWS()
        {
            int expires = (Int32)(DateTime.UtcNow.AddDays(1).Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            byte[] sigByte = BitUtility.GetHashCode(marketaccess.api_key, "GET/realtime" + expires);
            string dd = "{\"op\":\"authKey\",\"args\":[\"" + marketaccess.api_key + "\"," + expires + ",\"" + BitUtility.ByteArrayToString(sigByte) + "\"]}";
            byte[] aa = Encoding.UTF8.GetBytes(dd);
            if (clientWebSocketconnector.State == WebSocketState.Open)
                Send(new AuthenticationRequest(marketaccess.api_key, marketaccess.secret_key));
        }
        /*
        void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Task<bool> re = Ping();
            if (!re.Result)
            {
                Task.Factory.StartNew(async () =>
                {
                    if (clientWebSocketconnector != null && clientWebSocketconnector.State != WebSocketState.Connecting)
                    {
                        StartAsync().Wait();
                        await SubscribeQuote10(SubInstrumentIDList);
                        Task<string> ts = GetAuthRest();//GetAuthWS();
                        ts.Wait();
                    }
                });
            }
        }
        */
        
        public async Task SubscribeQuote10(List<string> list)
        {
            Send(new AuthenticationRequest(marketaccess.api_key, marketaccess.secret_key));
            string[] insarray = new string[list.Count];
            if (clientWebSocketconnector.State == WebSocketState.Open)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    var d = new QuoteSubscribeRequest(list[i]);
                    Send(d);                    
                }               
            }           
            else
                throw new Exception("not connected");          
        }
        public async Task Send<T>(T request) where T : RequestBase
        {
            try
            {               
                BmxValidations.ValidateInput(request, nameof(request));
                JsonSerializerSettings Settings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    Formatting = Formatting.None,
                    Converters = new List<JsonConverter>() { new BitmexStringEnumConverter { CamelCaseText = true } },
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                };
                var serialized = request.IsRaw ? request.OperationString : JsonConvert.SerializeObject(request, Settings);
                await clientWebSocketconnector.Send(serialized);
            }
            catch (Exception e)
            {
                SimpleLogger.Logger.Log(new LogMessage { Data = "error for sending message"+e.Message, CallingClass="Send", DataCategory=LogCategory.Error});
                
                throw;
            }
        }  
        
        async Task<bool> Ping()
        {           
            try
            {
                await Send(new PingRequest());               
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        
        public Task Authenticate(string apiKey, string apiSecret)
        {
            return Send(new AuthenticationRequest(apiKey, apiSecret));
        }
        
        private async Task SendSubscriptionRequests()
        {
            Send(new PingRequest());
            //Send(new BookSubscribeRequest());
            //Send(new TradesSubscribeRequest("XBTUSD"));
            foreach (var s in SubInstrumentIDList)
            {
                await Send(new QuoteSubscribeRequest(s));
                //await Send(new Book10SubscribeRequest(s));
                //await Task.Delay(500);
                //Send(new BookSubscribeRequest());               
            }

            //await client.Send(new LiquidationSubscribeRequest());
            //var s = new QuoteSubscribeRequest("XBTUSD");
            if (!string.IsNullOrWhiteSpace(marketaccess.api_key))
            {
                //var a = new AuthenticationRequest(API_KEY, API_SECRET);
                Send(new AuthenticationRequest(marketaccess.api_key, marketaccess.secret_key));
            }
        }
        private void BM_OnMessage(object sender, string e)
        {
            if (string.IsNullOrEmpty(e)) return;
            
            JObject response = null;
            JArray ja;

            try
            {
                if (e.Contains("Welcome"))   //initially connected!
                {
                    if (!Initialized || DateTime.Now > INITIALIZEDTIME.AddSeconds(29))
                    {
                        GetAuthWS();
                        //GetAuthRest();
                        //Authenticate(marketaccess.api_key, marketaccess.secret_key).Wait();
                        SendSubscriptionRequests();
                        //var p = GetPosition();
                        //var a = GetQuote("XBTUSD");
                        var history = GetQuoteBucketed(new KLineRequest()
                        {
                            symbol = "XBTUSD",
                            binSize = "1m",
                            count = 100,
                            partial = true,
                            reverse = true,
                            Op = MessageType.KLineRequest
                            
                            
                        });
                        Initialized = true;
                    }
                    return;
                }
                else
                   if (e.Contains("pong"))
                {
                    return;
                }
                //else
                //    if (e.Contains(".BXBT"))
                //    Utility.WriteMemLog(e);
                response = JsonConvert.DeserializeObject<JObject>(e);
                if (!response.HasValues)
                    return;

                AuthenticationResponse.TryHandle(response, async (x) =>
                {
                    if (x.Success)
                    {
                        await Send(new WalletSubscribeRequest());  //订阅账户余额
                        //await Task.Delay(300);
                        await Send(new ExecutionSubscribeRequest());
                        await //Task.Delay(300);
                        Send(new MarginSubscribeRequest());
                        await //Task.Delay(300);
                        Send(new PositionSubscribeRequest());
                        await// Task.Delay(300);
                        Send(new OrderSubscribeRequest());
                        //await //Task.Delay(300);
                        //Send(new TransactSubscribeRequest());
                    }
                    else
                        return;
                });                               
            }
            catch (Exception ec)
            {

            }
            
            var v1 = response?["success"]?.Value<string>();
            if (v1 != null)
                return;


            var vv = response?["table"]?.Value<string>();
            if (vv == null)
            {
                Utility.WriteMemLog(e);
                return;
            }
            switch (vv)
            {
                case "trade":   //应该是交易所所有的交易信息，无需个人验证
                    TradeResponse parsed = response.ToObject<TradeResponse>(JsonSerializer.Create(new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        Formatting = Formatting.None,
                        Converters = new List<JsonConverter>() { new BitmexStringEnumConverter { CamelCaseText = true } },
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    }));
                    TradeData[] td = new TradeData[parsed.Data.Length];
                    for (int i=0;i<td.Length;i++)
                    {
                        td[i] = new TradeData()
                        {
                            AvgPrice = parsed.Data[i].Price,
                            Direction = parsed.Data[i].Side == BitmexSide.Buy ? TradeDirection.Long : TradeDirection.Short,
                            Quant = parsed.Data[i].Size,
                            OrderTime = parsed.Data[i].Timestamp.ToString("yyyy-MM-dd HH:mm:ss.ff"),
                            UpdateTime=DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ff")
                        };                        
                    }

                    OnRtnTradeData?.Invoke(this, td);
                    //TradeResponse.TryHandle(response, )
                    break;
                case "wallet":
                    WalletResponse wr = response.ToObject<WalletResponse>();
                    SimpleLogger.Logger.Log(response, SimpleLogger.LogCategory.Info);
                    TradingAccountData ai;
                    foreach (var w in wr.Data)
                    {
                        ai = new TradingAccountData()
                        {
                            Equity = (double)(w.Amount??0)/100000000,
                            BalanceBtc = (decimal)w.BalanceBtc,
                            InvestorID = w.Account.ToString(),                           
                            ExchangeID = "BitMex",                            
                            DigitalCurrencyID = w.Currency,
                            UpdateTime = w.Timestamp
                        };
                        OnRtnWallet?.Invoke(this, ai);
                        
                    }                    
                    break;
                case "margin":
                    SimpleLogger.Logger.Log(response, SimpleLogger.LogCategory.Info);
                    MarginResponse.TryHandle(response, y => OnRtnMargin?.Invoke(this, y.Data));
                    break;
                case "quote":
                    ja = response.Value<JArray>("data");
                    if (ja == null) return;
                    BMQuote[] bMQuotes = ja.ToObject<BMQuote[]>();
                    if (bMQuotes.Length == 0) return;
                    //Utility.WriteMemLog(e);
                    UpdateDicCustomMarketData(UpdatedicMarketData(bMQuotes));
                    break;
                case "instrument":
                    if ((ja = response.Value<JArray>("data")) == null)
                        return;
                    BMInstrumentData id = response.Value<JArray>("data").ToObject<BMInstrumentData>();
                    break;
                case "orderBookL2":
                    BookResponse book = response.ToObject<BookResponse>();
                    BMOrderBookL2 orderBookL2 = response.ToObject<BMOrderBookL2>();
                    break;
                case "orderBook10":
                    var fs = JsonConvert.DeserializeObject<BMOrderBook10>(e);
                    //var ff = response.ToObject<BMOrderBook10>();
                    UpdateDicCustomMarketData(UpdatedicMarketData(fs));
                    break;
                case "execution":
                    //CryptoTrading.Utility.WriteMemLog(e);
                    ExecutionResponse.TryHandle(response, x => OnRtnExecution?.Invoke(this, x));
                    break;
                case "order":
                    Utility.WriteMemLog(e);
                    OrderResponse.TryHandle(response, x => OnRtnOrder?.Invoke(this, x));
                    break;
              
                case "position":
                    //CryptoTrading.Utility.WriteMemLog(e);
                    PositionResponse.TryHandle(response, x => OnRtnPosition?.Invoke(this, x.Data));
                    break;
                case "transact"://资金提存信息更新
                    //TransactResponse.TryHandle(response, x=>OnRtnTransact?.Invoke(this, x.Data));
                    break;
                default:
                    break;
            }
        }

        private List<MajorMarketData> UpdatedicMarketData(BMOrderBook10 s)
        {
            List<MajorMarketData> listmm = new List<MajorMarketData>();
            MajorMarketData md;
            if (s.table != "orderBook10")
                return listmm;

            var bm = s.data[s.data.Length - 1];
            {
                if (!TQMainModel.dicMajorMarketData.TryGetValue(bm.symbol, out md)) return listmm;
                {
                    md.AskPrice1 = (decimal)bm.asks[0][0];
                    md.AskPrice2 = (decimal)bm.asks[1][0];
                    md.AskPrice3 = (decimal)bm.asks[2][0];
                    md.AskPrice4 = (decimal)bm.asks[3][0];
                    md.AskPrice5 = (decimal)bm.asks[4][0];
                    md.AskSize1 = (int)bm.asks[0][1];
                    md.AskSize2 = (int)bm.asks[1][1];
                    md.AskSize3 = (int)bm.asks[2][1];
                    md.AskSize4 = (int)bm.asks[3][1];
                    md.AskSize5 = (int)bm.asks[4][1];
                    md.BidPrice1 = (decimal)bm.bids[0][0];
                    md.BidPrice2 = (decimal)bm.bids[1][0];
                    md.BidPrice3 = (decimal)bm.bids[2][0];
                    md.BidPrice4 = (decimal)bm.bids[3][0];
                    md.BidPrice5 = (decimal)bm.bids[4][0];
                    md.BidSize1 = (int)bm.bids[0][1];
                    md.BidSize2 = (int)bm.bids[1][1];
                    md.BidSize3 = (int)bm.bids[2][1];
                    md.BidSize4 = (int)bm.bids[3][1];
                    md.BidSize5 = (int)bm.bids[4][1];
                    md.UpdateTime = bm.timestamp.ToString("HH:mm:ss.ff");
                    md.BaseName = bm.symbol; ;
                    //md.Change = Math.Round(value - last, 2)
                }
                listmm.Add(md);
            }
            return listmm;
        }

        private void UpdateDicCustomMarketData(List<MajorMarketData> listmarketdata)
        {
            if (listmarketdata == null || listmarketdata.Count == 0)
            {
                return; // MessageBox.Show("Alert: input data is null ");
            }
            IEnumerable<string> diccust;
            foreach (var marketdata in listmarketdata)
            {
                diccust = TQMainModel.dicCustomProduct.Keys.Where(x => x.Contains(marketdata.InstrumentID));
                //是否在自定义品种中？        
                if (diccust.Count() > 0)
                {
                    ComboMarketData cust;
                    foreach (var custt in diccust)
                    {
                        cust = TQMainModel.dicCustomProduct[custt];
                        cust.LastPrice = 0;
                        //cust.BaseName = TQMainModel.dicCustomProduct[custt].BaseName;
                        cust.BidPrice1 = 0;
                        cust.AskPrice1 = 0;
                        cust.BidSize1 = 0;
                        cust.AskSize1 = 0;
                        cust.UpdateTime = marketdata.UpdateTime;
                        cust.Change = 0;
                    }
                }
            }
        }
        private List<MajorMarketData> UpdatedicMarketData(BMQuote[] fm)
        {
            List<MajorMarketData> listmm = new List<MajorMarketData>();
            MajorMarketData md;// = new MajorMarketData();
            BMQuote bm = fm[fm.Length - 1];
            //foreach (var bm in fm)
            {
                md = new MajorMarketData();
                if (!TQMainModel.dicMajorMarketData.TryGetValue(bm.InstrumentID, out md)) return listmm;
                //get record, update the MajorMarketData info, and Notify the view to update
                //md.HighestPrice = fm.high;
                //mmd.LowestPrice = fm.low;
                //mmd.OpenInterest = fm.hold_amount;
                //mmd.LastPrice = fm.last;              
                md.AskPrice1 = (decimal)bm.AskPrice;
                md.BidPrice1 = (decimal)bm.BidPrice;
                md.BidSize1 = bm.BidSize;
                md.AskSize1 = bm.AskSize;
                //md.channel = fm.channel;
                //md.ContractId = bm.InstrumentID;
                //md.LimitLow = fm.limitLow;
                //md.LimitHigh = fm.limitHigh;
                //mmd.Volume = fm.vol;
                md.UpdateTime = bm.Timestamp.ToString("HH:mm:ss.ff");
                //mmd.UnitAmount = fm.unitAmount;
                md.BaseName = bm.InstrumentID; //Utility.GetBaseName(fm.instrumentID); 
                md.LastPrice = (decimal)bm.AskPrice;
                listmm.Add(md);
            }
            return listmm;
        }

        /*async Task Send(string jsons)
        {            
            clientWebSocketconnector.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(jsons)), WebSocketMessageType.Text, true, CancellationToken.None);            
        }*/
        public async Task Send(string op, string[] args)
        {
            string str = "{\"op\":\"" + op + "\",\"args\":[";
            if (args.Length > 0)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (i > 0)
                    {
                        str += ",";
                    }
                    str += "\"" + args[i] + "\"";
                }
                str += "]}";
            }
            clientWebSocketconnector.Send(str);
        }

    }
}
