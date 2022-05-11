using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
namespace CryptoTrading.BitMex
{
    public partial class BitMexBase
    {
        //TQMain tqmain;        
        public ClientWebSocket clientWebSocket = null;
        private MarketAccess marketaccess;
        private int rateLimit;
        internal System.Timers.Timer timer;
        private const string uri ="wss://www.bitmex.com/realtime";
        private const string domain = "https://www.bitmex.com";
        private byte[] buffer = new byte[1024*2];
        static List<string> listofmessage = new List<string>();        
        event EventHandler<string> OnMessage;
        public List<string> InstrumentIDList;
        static object _forlock = new object();
        private readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1);
        public BitMexBase(int rateLimit = 5000)
        {
            //tqmain = main;
            string path = Environment.CurrentDirectory+"\\configurations\\BitMexConfigs";
            var key = File.ReadAllText(path+"\\BitMexApiKey.txt");
            marketaccess = JsonConvert.DeserializeObject<MarketAccess>(key);
            this.rateLimit = rateLimit;
            OnMessage += BM_OnMessage;
            timer = new System.Timers.Timer();
            timer.Interval =2000 ;
            timer.Elapsed += Timer_Elapsed;                       
        }
        private string Query(string method, string function, Dictionary<string, string> param = null, bool auth = false, bool json = false)
        {
            string paramData = json ? Utility.BuildJSON(param) : Utility.BuildQueryData(param);
            string url = "/api/v1" + function + ((method == "GET" && paramData != "") ? "?" + paramData : "");
            string postData = (method != "GET") ? paramData : "";

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(domain + url);
            webRequest.Method = method;
            if (auth)
            {
                int expires = (Int32)(DateTime.UtcNow.AddDays(7).Subtract(new DateTime(1970, 1, 1))).TotalSeconds;            
                string message = method + url + expires.ToString() + postData;
                byte[] signatureBytes = Utility.hmacsha256(Encoding.UTF8.GetBytes(marketaccess.secret_key), Encoding.UTF8.GetBytes(message));
                string signatureString = Utility.ByteArrayToString(signatureBytes);
                webRequest.Headers.Add("api-expires", expires.ToString());
                webRequest.Headers.Add("api-key", marketaccess.api_key);
                webRequest.Headers.Add("api-signature", signatureString);
            }
            try
            {
                if (postData != "")
                {
                    webRequest.ContentType = json ? "application/json" : "application/x-www-form-urlencoded";
                    var data = Encoding.UTF8.GetBytes(postData);
                    using (var stream = webRequest.GetRequestStream())
                    {
                        stream.Write(data, 0, data.Length);
                    }
                }

                using (WebResponse webResponse = webRequest.GetResponse())
                using (Stream str = webResponse.GetResponseStream())
                using (StreamReader sr = new StreamReader(str))
                {
                    return sr.ReadToEnd();
                }
            }
            catch (WebException wex)
            {
                using (HttpWebResponse response = (HttpWebResponse)wex.Response)
                {
                    if (response == null)
                        throw;

                    using (Stream str = response.GetResponseStream())
                    {
                        using (StreamReader sr = new StreamReader(str))
                        {
                            return sr.ReadToEnd();
                        }
                    }
                }
            }
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

        void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Task<bool> re =  Ping();
            if (!re.Result)
            {
                Task.Factory.StartNew(async () =>
                {
                    if (clientWebSocket != null && clientWebSocket.State != WebSocketState.Connecting)
                    {
                        StartAsync().Wait();
                        SubscribeQuote10();
                        Task<string> ts =  GetAuthRest();//GetAuthWS();
                        ts.Wait();
                    }
                });
            }
        }
        public async Task<string> GetAuthRest()
        {
            var param = new Dictionary<string, string>();
            param["symbol"] = "XBTUSD";
            return Query("GET", "", param, true);
        }
        public async Task GetAuthWS()
        {
            int expires = (Int32)(DateTime.UtcNow.AddDays(7).Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            byte[] sigByte = Utility.GetHashCode(marketaccess.api_key, "GET/realtime" + expires);
            string dd = "{\"op\":\"authKeyExpires\",\"args\":[\"" + marketaccess.api_key + "\"," + expires + ",\"" + Utility.ByteArrayToString(sigByte) + "\"]}";
            byte[] aa = Encoding.UTF8.GetBytes(dd);
            if (clientWebSocket.State == WebSocketState.Open)
                clientWebSocket.SendAsync(new ArraySegment<byte>(aa), WebSocketMessageType.Text, true, CancellationToken.None);
        }
        public void SubscribeQuote10()
        {            
            //
            List<string> listInst= new List<string>();
            List<string> listL2 = new List<string>();
            foreach (var t in InstrumentIDList)
            {
                listInst.Add("quote:" + t);
            }
            if (clientWebSocket.State == WebSocketState.Open)
                 Send("subscribe", listInst.ToArray<string>());
             else   
                throw new Exception("not connected");
            var v = GetAuthRest();
            v.Wait();
        }        

        async Task<bool> Ping()
        {
            string ping = "ping";
            try
            {
                await clientWebSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(ping)), WebSocketMessageType.Text, true, CancellationToken.None);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public async Task StartAsync()
        {//
            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter(8000);
            int q = 1;
            await Task.Factory.StartNew(async () =>
            {
                if (clientWebSocket != null) clientWebSocket.Dispose();
                using (clientWebSocket = new ClientWebSocket())
                {
                    do
                    {
                        try
                        {                            
                            if (clientWebSocket.State != WebSocketState.Connecting)
                            {                                
                                Task tt = clientWebSocket.ConnectAsync(new Uri(uri), cts.Token);
                                await tt;
                                if (clientWebSocket.State == WebSocketState.Open)
                                    SubscribeQuote10();
                            }
                        }
                        catch (Exception ex)
                        {
                            CryptoTrading.Utility.WriteMemFile(ex.Message);
                            await Task.Delay(TimeSpan.FromSeconds(q++));
                            if (q > 300)
                                ;
                            continue;
                        }                                            
                    }
                    while (!(clientWebSocket.State == WebSocketState.Open || clientWebSocket.State == WebSocketState.Connecting));
                    timer.Stop();
                    timer.Start();

                    while (clientWebSocket.State == WebSocketState.Open)
                    {
                        await OnOpen();
                    }
                    if (clientWebSocket.State != WebSocketState.Open)
                    {
                        CryptoTrading.Utility.WriteMemFile("clientWebSocket is not open");
                    }
                }                
            });
        }
             

        async Task OnOpen()
        {
            string msg = "";
            Task<WebSocketReceiveResult> tr = null;
            try
            {
                //await semaphoreSlim.WaitAsync();
                tr = clientWebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                tr.Wait();
            }
            catch (Exception ex)
            {
                //continue;
                //throw ex;
                CryptoTrading.Utility.WriteMemFile(ex.Message);
                return;
            }
            finally
            {
                //semaphoreSlim.Release();
            }
            if (tr.Result.MessageType == WebSocketMessageType.Text)
            {
                if (!tr.Result.EndOfMessage)
                {
                    int totalLength = 0;
                    while (!tr.Result.EndOfMessage)
                    {
                        var st = Encoding.UTF8.GetString(buffer, 0, tr.Result.Count);
                        msg += st;
                        totalLength += tr.Result.Count;
                        await semaphoreSlim.WaitAsync();
                        try
                        {
                            tr = clientWebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                            tr.Wait();
                        }
                        finally
                        {
                            semaphoreSlim.Release();
                        }
                    }
                    msg += Encoding.UTF8.GetString(buffer, 0, tr.Result.Count);
                }
                else
                {
                    msg = Encoding.UTF8.GetString(buffer, 0, tr.Result.Count);
                }
                if (msg.Contains("pong"))
                {
                    //Utility.WriteMemFile("pong");
                    return;
                }
                else if (msg.Contains("success"))
                {
                    return;
                }
                BM_OnMessage(this, msg);
                //OnMessage?.Invoke(null, msg);
            }
        }
        private void BM_OnMessage(object sender, string e)
        {
            JObject j;
            JArray ja;
            OrderBookL2Data obl2 = new OrderBookL2Data();
            List<MajorMarketData> listmmd;
            BMQuote mQuote=new BMQuote();                    
            try
            {                
                j = (JObject)JsonConvert.DeserializeObject(e);
                
                if (j.HasValues)
                {
                    var r = j.Value<string>("table");
                    if (r != null)
                    {
                        switch (r)
                        {
                            case "quote":
                                try
                                {
                                    ja = j.Value<JArray>("data");
                                    if (ja == null) return;
                                    BMQuote[] bMQuotes = ja.ToObject<BMQuote[]>();                                     
                                    MajorMarketData md = new MajorMarketData();
                                    UpdateDicCustomMarketData(UpdatedicMarketData(bMQuotes));                                 
                                }
                                catch (Exception exx)
                                {

                                }
                                break;
                            case "instrument":
                                if ((ja = j.Value<JArray>("data")) == null)
                                    return;                                
                                InstrumentData id = j.Value<JArray>("data").ToObject<InstrumentData>(); 
                                break;
                            case "orderBookL2":                                
                                BMOrderBook10 s = JsonConvert.DeserializeObject<BMOrderBook10>(e);
                                //BMQuote bMQuote = new BMQuote();
                                //UpdateDicCustomMarketData(UpdatedicMarketData(s));
                                break;
                            case "orderBook10":                                
                                var fs = JsonConvert.DeserializeObject<BMOrderBook10>(e);
                                UpdateDicCustomMarketData(UpdatedicMarketData(fs));
                                break;
                        }                        
                    }
                }                
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
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
                if (!TQMainVM.dicMajorMarketData.TryGetValue(bm.symbol, out md)) return listmm;                
                {
                    md.AskPrice1 = bm.asks[0][0];
                    md.AskPrice2 = bm.asks[1][0];
                    md.AskPrice3 = bm.asks[2][0];
                    md.AskPrice4 = bm.asks[3][0];
                    md.AskPrice5 = bm.asks[4][0];
                    md.AskVolume1 = (int)bm.asks[0][1];
                    md.AskVolume2 = (int)bm.asks[1][1];
                    md.AskVolume3 = (int)bm.asks[2][1];
                    md.AskVolume4 = (int)bm.asks[3][1];
                    md.AskVolume5 = (int)bm.asks[4][1];
                    md.BidPrice1 = bm.bids[0][0];
                    md.BidPrice2 = bm.bids[1][0];
                    md.BidPrice3 = bm.bids[2][0];
                    md.BidPrice4 = bm.bids[3][0];
                    md.BidPrice5 = bm.bids[4][0];                   
                    md.BidVolume1 = (int) bm.bids[0][1];
                    md.BidVolume2 = (int)bm.bids[1][1];
                    md.BidVolume3 = (int)bm.bids[2][1];
                    md.BidVolume4 = (int)bm.bids[3][1];
                    md.BidVolume5 = (int)bm.bids[4][1];                 
                    md.UpdateTime = bm.timestamp.ToString("HH:mm:ss.ffff");
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
                diccust = TQMainVM.dicCustomProduct.Keys.Where(x => x.Contains(marketdata.instrumentID));
                //是否在自定义品种中？        
                if (diccust.Count() > 0)
                {
                    ComboMarketData cust;
                    foreach (var custt in diccust)
                    {
                        cust = TQMainVM.dicCustomProduct[custt];
                        cust.LastPrice = 0;
                        cust.BaseName = TQMainVM.dicCustomProduct[custt].BaseName;
                        cust.BidPrice1 = 0;
                        cust.AskPrice1 = 0;
                        cust.BidVolume1 = 0;
                        cust.AskVolume1 = 0;
                        cust.UpdateTime = marketdata.updateTime;
                        cust.Change = 0;
                    }
                }
            }
        }
        private List<MajorMarketData> UpdatedicMarketData(BMQuote[] fm)
        {
            List<MajorMarketData> listmm= new List<MajorMarketData>();
            MajorMarketData md;// = new MajorMarketData();
            BMQuote bm = fm[fm.Length-1];
            //foreach (var bm in fm)
            {
                md = new MajorMarketData();
                if (!TQMainVM.dicMajorMarketData.TryGetValue(bm.InstrumentID, out md)) return listmm;                
                //get record, update the MajorMarketData info, and Notify the view to update
                //md.HighestPrice = fm.high;
                //mmd.LowestPrice = fm.low;
                //mmd.OpenInterest = fm.hold_amount;
                //mmd.LastPrice = fm.last;              
                md.AskPrice1 = bm.AskPrice;
                md.BidPrice1 = bm.BidPrice;
                md.BidVolume1 = bm.BidSize;
                md.AskVolume1 = bm.AskSize;
                //md.channel = fm.channel;
                //md.ContractId = bm.InstrumentID;
                //md.LimitLow = fm.limitLow;
                //md.LimitHigh = fm.limitHigh;
                //mmd.Volume = fm.vol;
                md.UpdateTime = bm.Timestamp.ToLocalTime().ToString("HH:mm:ss.fff"); ;
                //mmd.UnitAmount = fm.unitAmount;
                md.BaseName = bm.InstrumentID; //Utility.GetBaseName(fm.instrumentID); 
                md.LastPrice = Math.Round((bm.BidPrice + bm.AskPrice)/2, 5);
                listmm.Add(md);
            }
            return listmm;
        }        

        async Task Send(string jsons)
        {            
            clientWebSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(jsons)), WebSocketMessageType.Text, true, CancellationToken.None);            
        }
        public async Task Send( string op, string[] args)
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
                    str += "\""+args[i]+"\"";                    
                }
                str += "]}";
            }
            Send(str);
            //ArraySegment<byte> ab = new ArraySegment<byte>(Encoding.UTF8.GetBytes(str));
            //clientWebSocket.SendAsync(ab, WebSocketMessageType.Text, true, CancellationToken.None);
           
        }
        private static void BMOnMessage(object sender, string e)
        {
            JObject j;
            JArray ja;
            try
            {
                lock (_forlock)
                {
                    listofmessage.Add(e);
                }
                j = (JObject)JsonConvert.DeserializeObject(e);
                if (j.HasValues)
                {
                    var r = j.Value<string>("table");
                    if (r != null)
                    {
                        switch (r)
                        {
                            case "quote":
                                ja = j.Value<JArray>("data");
                                foreach (var ss in ja)
                                {

                                    BMQuote mQuote = ss.ToObject<BMQuote>();
                                    ;
                                }
                                break;
                            case "orderBookL2":
                                ja = j.Value<JArray>("data");
                                var s = JsonConvert.DeserializeObject<BMOrderBookL2>(e);
                                foreach (var ss in ja)
                                {
                                    OrderBookL2Data obl2 = ss.ToObject<OrderBookL2Data>();
                                    Console.Write(ss.Value<string>("symbol"));
                                    Console.WriteLine(" size : " + ss.Value<string>("size"));
                                }
                                break;
                            case "orderBook10":
                                ja = j.Value<JArray>("data");
                                var jj = JsonConvert.DeserializeObject<BMOrderBook10>(e);
                                break;
                        }
                    }
                }
                CryptoTrading.Utility.WriteMemFile(e);
                //Console.WriteLine("finish write to file");
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
            }
        }
    }

}
