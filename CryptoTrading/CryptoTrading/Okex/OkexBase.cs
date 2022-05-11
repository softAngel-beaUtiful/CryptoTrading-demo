using System;
using System.Text;
using System.IO;
using System.Threading;
using WebSocketSharp;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using CryptoTrading.ViewModel;
using System.Windows;
using System.Linq;
using System.Threading.Tasks;
using CryptoTrading.Model;
using System.IO.Compression;
using OKExSDK;
using CryptoTradingMaster.View.Transfer;
using SimpleLogger;
using System.Diagnostics;

namespace CryptoTrading.OkexSpace
{
    //数据结构
    public partial class OkexBase : IOkex
    {
        internal const string ExchangeName = "Okex";
        internal string UserID;// = 0;
        public WebSocketor webSocketor; // = new WebSocketor();
        public SpotMarketData smd;
       
        //private Logger pLog;
        private System.Timers.Timer timer = new System.Timers.Timer(19000);
        //private const string url = "wss://real.okex.com:10440/websocket/okexapi";
        //private const string url_prex = "https://www.okex.com";
        //private const string resturl = "https://www.okex.com/api/v1";
        //private object lockorder = new object();
        //FutureRestApiV1 postRequest;
        FuturesApi FuturePostRequest;
        SwapApi SwapPostRequest;
        public Dictionary<string, SpreadBidAsk> DicSpread = new Dictionary<string, SpreadBidAsk>();

        public MarketAccess marketacc = new MarketAccess();
        public Exchange MyOkex;
        public List<InstrumentData> FuturesSubscribed= new List<InstrumentData>();
        public List<InstrumentData> SwapSubscribed = new List<InstrumentData>();
        public Dictionary<string, DepthStru> dicDepth = new Dictionary<string, DepthStru>();
        public Dictionary<string, List<ContractTrade>> DicContracttrade = new Dictionary<string, List<ContractTrade>>();
        //public ConcurrentDictionary<string, List<FutureMarketData>> dicfutureticks = new ConcurrentDictionary<string, List<FutureMarketData>>();
        public Dictionary<string, List<MDKLine>> dicMDKlines = new Dictionary<string, List<MDKLine>>();
        public SpreadBidAsk spreadbidask;
        //public EventHandler<Dictionary<string, SPosition>> OnRtnSPosition;
        public bool Initialized = false;
        public OkexBase(string userid, Exchange exch)
        {
            //futureOrders = new Queue<OrderData>();
            MyOkex = exch;//JsonConvert.DeserializeObject<List<MarketAccess>>(File.ReadAllText(@"accessKeys.txt")).Find(x => x.Name ==okuser);
            UserID = userid;

            //webSocketor.LoginAsync(MyOkex.PublicKey, MyOkex.PrivateKey, MyOkex.PassPhrase).Wait();
            FuturePostRequest = new FuturesApi(MyOkex.PublicKey, MyOkex.PrivateKey, MyOkex.PassPhrase);
            SwapPostRequest = new SwapApi(MyOkex.PublicKey, MyOkex.PrivateKey, MyOkex.PassPhrase);

        }
        public void SetSubscriptionList(Dictionary<string, InstrumentData> dict)
        {
            foreach (var a in dict)
            {
                if (a.Value.ExchangeID == EnuExchangeID.OkexFutures)
                    FuturesSubscribed.Add(a.Value);
                else
                    if (a.Value.ExchangeID == EnuExchangeID.OkexSwap)
                    SwapSubscribed.Add(a.Value);
            }
        }
        public async Task Subscribe()
        {
            List<string> list = new List<string>();
            foreach (var vv in FuturesSubscribed)
            {
                string newinstrumentid = vv.InstrumentID.ToOkexFuturesInstrumentid();
                list.Add($"futures/ticker:{newinstrumentid}");
                list.Add($"futures/order:{newinstrumentid}");
                list.Add($"futures/depth5:{newinstrumentid}");
                list.Add($"futures/position:{newinstrumentid}");
            }
            list.Add("futures/account:BTC");
            list.Add("futures/account:ETH");
            list.Add("futures/account:EOS");

            foreach (var vv in SwapSubscribed)
            {
                string newinstrumentid = vv.InstrumentID.ToOkexSwapInstrumentid();

                list.Add($"swap/ticker:{newinstrumentid}");
                list.Add($"swap/order:{newinstrumentid}");
                list.Add($"swap/depth5:{newinstrumentid}");
                list.Add($"swap/position:{newinstrumentid}");
                list.Add($"swap/account:{newinstrumentid}");
            }
            try
            {
                await SubscribeAsync(list);
            }
            catch (AggregateException ae)
            { }
            catch (Exception)
            {
                await Task.Delay(10000);
                //await SubscribeAsync(list);
            }
            await Task.Delay(100);
        }
       
        public void ReqAllPosition()
        {
            RestRspPosition pc;          
            Dictionary<string, SummaryPosition> dicrestPos= new Dictionary<string, SummaryPosition>();
            Task.Run(async () =>
            {
                foreach (var vv in FuturesSubscribed)
                {
                    {
                        if (vv.ExchangeID == EnuExchangeID.OkexFutures)
                        {
                            try
                            {                                
                                var result = FuturePostRequest.getPositionByIdAsync(vv.InstrumentID.ToOkexFuturesInstrumentid());
                                await result;
                                var summarypositionv3 = result.Result["holding"].ToObject<List<SummaryPositionV3>>();

                                if (summarypositionv3.Count == 0) continue;
                                OnRtnSummaryPositionV3?.Invoke(this, summarypositionv3);
                            }
                            catch (AggregateException ex)
                            {
                                ex.WriteToLog();
                                SimpleLogger.Logger.Log(new LogMessage() { CallingMethod = "ReqAllPosition", CallingClass = "OkexBase", Data = ex.Message, DataCategory = SimpleLogger.LogCategory.Error });
                                Thread.Sleep(1000);
                            }
                            catch (Exception ex)
                            {
                                SimpleLogger.Logger.Log(new LogMessage() { CallingMethod = "ReqAllPosition", CallingClass = "OkexBase", Data = ex.Message, DataCategory = SimpleLogger.LogCategory.Error });

                                //pLog.Fatal(ex.ToString());
                                Thread.Sleep(1000);
                            }
                            await Task.Delay(3000);
                        }
                    }
                    foreach (var v1 in SwapSubscribed)
                    { /*{ "holding": [
  {
                "avail_position": "103",
    "avg_cost": "5.887",
    "instrument_id": "EOS-USD-SWAP",
    "leverage": "100",
    "liquidation_price": "0.200",
    "margin": "1.7229",
    "position": "103",
    "realized_pnl": "-0.0353",
    "settlement_price": "5.885",
    "side": "long",
    "timestamp": "2019-05-17T05:06:00.199Z"
  }
]}       */
                        try
                        {
                            var result = SwapPostRequest.getPositionByInstrumentAsync(v1.InstrumentID.ToOkexSwapInstrumentid());
                            await result;
                            List<SummaryPositionSwapV3> summarypositionswapv3 = result.Result["holding"].ToObject<List<SummaryPositionSwapV3>>();
                            OnRtnSummaryPositionSwapV3Q?.Invoke(this, summarypositionswapv3);
                        }
                        catch (AggregateException ex)
                        {
                            SimpleLogger.Logger.Log(new SimpleLogger.LogMessage() { CallingMethod = "SwapGetPosition", CallingClass = "OkexBase", Data = ex.Message, DataCategory = SimpleLogger.LogCategory.Error });

                            ex.WriteToLog();
                            Thread.Sleep(1000);
                        }
                        catch (Exception ex)
                        {
                            SimpleLogger.Logger.Log(new LogMessage() { CallingMethod = "swapgetPosition", CallingClass = "OkexBase", Data = ex.Message, DataCategory = SimpleLogger.LogCategory.Error });

                            //pLog.Fatal(ex.ToString());
                            Thread.Sleep(1000);
                        }
                        await Task.Delay(3000);
                    }

                }
            });       
        }
        public Dictionary<string, OrderData> QueryOrdersInfo()
        {
            OrderData od;
            //RestOrders ro;
            Dictionary<string, OrderData> dicorder = new Dictionary<string, OrderData>();
            foreach (var s in FuturesSubscribed)
            {
                try
                {
                    int[] arr = { -1, 0, 1, 2, 6, 7 };//(-1.撤单成功；0:等待成交 1:部分成交 2:全部成交 6：未完成（等待成交+部分成交）7：已完成（撤单成功+全部成交）
                    foreach (var i in arr)
                    {
                        var ii = i.ToString();
                        var symbol = s.InstrumentID.ToOkexFuturesInstrumentid();

                        var result = FuturePostRequest.getOrdersAsync(symbol, ii,null,null,null);
                        result.Wait();
                        
                        Utility.CheckOkexResult(result.Result);
                        List<OrderResultV3> list = new List<OrderResultV3>();
                        if (result.Result == null || result.Result.Count==0) continue;

                        var jods = result.Result.SelectToken("order_info");
                        list = jods.ToObject<List<OrderResultV3>>();
                        if (list.Count > 0)
                        {
                            foreach (var e in list)
                            {
                                Console.WriteLine(e.ToString());
                            }
                        }
                        else
                            continue;
                        
                        foreach (var rr in list)
                        {
                            od = new OrderData()
                            {
                                OrderID = rr.order_id,
                                LeverRate = rr.leverage,
                                AvgPrice = rr.price_avg,
                                //CancelTime = rr.last_fill_time.ToString("MM-dd HH:mm:ss"),                               
                                ExchangeID = EnuExchangeID.OkexFutures,
                                Direction = rr.type == "1" || rr.type == "4" ? TradeDirection.Long : TradeDirection.Short,
                                Offset = rr.type == "1" || rr.type == "2" ? OffsetType.开仓 : OffsetType.平仓,
                                InvestorID = UserID.ToString(),
                                ClientOID = rr.client_oid,
                                UpdateTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ff"),
                                CreateTime = rr.timestamp.ToString("yyyy-MM-dd HH:mm:ss.ff"),
                                Commission = rr.fee,
                                InstrumentID = rr.instrument_id,
                                OrderPrice = rr.price,
                                OrderPriceType = rr.order_type == "0" ? OrderPriceType.限价: OrderPriceType.市价,
                                OrderSize = rr.size,
                                OrderStatus = rr.state == "0" ? OrderStatusType.未成交 : rr.status == "1" ? OrderStatusType.部分成交 :
                                    rr.status == "2" ? OrderStatusType.全部成交 : rr.status == "3" ? OrderStatusType.下单中 : rr.status == "4" ? OrderStatusType.撤单中
                                    : rr.status == "-2" ? OrderStatusType.失败 : rr.status == "-1" ? OrderStatusType.已撤单 : OrderStatusType.未知,
                                //OrderPriceType= OrderPriceType.停损价
                                QuantFilled = rr.filled_qty,
                                QuantUnfilled = rr.size - rr.filled_qty,
                                PnL = rr.pnl
                            };
                            dicorder[od.OrderID] = od;
                        }
                    }
                    
                }
                catch (Exception ex)
                {
                    SimpleLogger.Logger.Log(new LogMessage() { CallingMethod = "QueryOrdersInfo", Data = ex.Message, DataCategory = LogCategory.Error });
                    ex.WriteToLog();
                }
            }
            try
            {
                foreach (var t in SwapSubscribed)
                {
                    //status 订单状态(-2:失败 -1:撤单成功 0:等待成交 1:部分成交 2:完全成交)
                    int[] arr = { -2, -1, 0, 1, 2, 3, 4, 6, 7 };
                    foreach (var i in arr)
                    {
                        var ii = i.ToString();

                        var result = SwapPostRequest.getOrdersAsync(t.InstrumentID.ToOkexSwapInstrumentid(), ii, null, null, null);
                        result.Wait();
                        Utility.CheckOkexResult(result.Result);
                        List<OrderResultV3> list = new List<OrderResultV3>();
                        if (result.Result == null) continue;

                        var jods = result.Result.SelectToken("order_info");
                        list = jods.ToObject<List<OrderResultV3>>();
                        if (list.Count > 0)
                        {
                            foreach (var e in list)
                            {
                                Console.WriteLine(e.ToString());
                            }
                        }
                        else
                            continue;
                        if (list == null) continue;
                        foreach (var rr in list)
                        {
                            od = new OrderData()
                            {
                                OrderID = rr.order_id,
                                LeverRate = rr.leverage,
                                AvgPrice = rr.price_avg,
                                //CancelTime = rr.last_fill_time.ToString("MM-dd HH:mm:ss"),                               
                                ExchangeID = EnuExchangeID.OkexSwap,
                                Direction = rr.type == "1" || rr.type == "4" ? TradeDirection.Long : TradeDirection.Short,
                                InvestorID = UserID.ToString(),
                                ClientOID = rr.client_oid,
                                UpdateTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ff"),
                                CreateTime = rr.timestamp.ToString("yyyy-MM-dd HH:mm:ss.ff"),
                                Commission = rr.fee,
                                InstrumentID = rr.instrument_id,
                                OrderPrice = rr.price,
                                OrderPriceType = rr.order_type == "0" ? OrderPriceType.限价 : OrderPriceType.市价,
                                OrderSize = rr.size,
                                OrderStatus = rr.state == "0" ? OrderStatusType.未成交 : rr.status == "1" ? OrderStatusType.部分成交 :
                                    rr.status == "2" ? OrderStatusType.全部成交 : rr.status == "-2" ? OrderStatusType.失败 : rr.status == "-1" ? OrderStatusType.已撤单 : OrderStatusType.未知,                               
                                QuantFilled = rr.filled_qty,
                                QuantUnfilled = rr.size - rr.filled_qty,
                                PnL = rr.pnl,
                            };
                            dicorder[od.OrderID] = od;
                        }
                    }
                }
                
            }
            catch (Exception ex)
            {
                SimpleLogger.Logger.Log(new LogMessage() {
                    Data = ex.Message, DataCategory = LogCategory.Error, CallingMethod="queryswaporders"
                });
            }        
            return dicorder;
        }
        public void RefreshUserInfo()
        {
            Task.Run(() =>
            {
                try
                {
                    /*+{{
  "info": {
    "eth": {
      "equity": "34.8345123",
      "liqui_mode": "legacy",
      "maint_margin_ratio": "",
      "margin": "0",
      "margin_for_unfilled": "0",
      "margin_frozen": "0",
      "margin_mode": "crossed",
      "margin_ratio": "10000",
      "realized_pnl": "-0.23264956",
      "total_avail_balance": "35.06716186",
      "unrealized_pnl": "0"
    },
    "eos": {
      "equity": "4937.38962716",
      "liqui_mode": "legacy",
      "maint_margin_ratio": "",
      "margin": "3.22527291",
      "margin_for_unfilled": "0",
      "margin_frozen": "3.22527291",
      "margin_mode": "crossed",
      "margin_ratio": "1530.8439828",
      "realized_pnl": "0.06266836",
      "total_avail_balance": "4936.13348289",
      "unrealized_pnl": "1.19347591"
    }
  }
}}	        */
                    var result = FuturePostRequest.getAccountsAsync();
                    result.Wait();
                    var dd = result.Result.FromOkexJson<OkFutureAccountV3Response>();
                    //查询当前所有持仓情况？
                    //OnRtnHistoricTrade?.Invoke(this, ht);                                      
                    var result1 = SwapPostRequest.getAccountsAsync();//Result;
                    result1.Wait();
                    OnRtnUserInfoV3?.Invoke(this, dd.info);
                    var v1 = result1.Result.FromOkexJson<AccountInfoSwap>();                    
                    OnRtnAccountInfoSwap?.Invoke(this, v1.info);
                    ReqAllPosition();
                    //var rr = FuturePostRequest.getTradesAsync("ETH-USD-200327", 1, null, 100);
                    //rr.Wait();
                    //var rr1 = FuturePostRequest.getOrdersAsync("ETH-USD-200327","6",null,null,100);
                    //rr1.Wait();
                    var ods = QueryOrdersInfo();
                    if (ods.Count>0)
                        OnRtnOrders?.Invoke(this, ods);
                }
                catch(Exception e)
                {
                    e.WriteToLog();
                    SimpleLogger.Logger.Log(e.Message, SimpleLogger.LogCategory.Error);
                    return; }
            });
        }
        public bool ReqLogin()
        {
            Task.Run(() =>
            {
                //生成标准的Json格式的数据格式
                try
                {
                    webSocketor.LoginAsync(MyOkex.PublicKey, MyOkex.PrivateKey, MyOkex.PassPhrase).Wait();
                }
                catch (Exception e1)
                {
                    e1.WriteToLog();
                    SimpleLogger.Logger.Log(new LogMessage() { Data = e1.Message, CallingClass = "OkexBase", CallingMethod = "ReqLogin", DataCategory = LogCategory.Error });
                }
                try
                {                  
                    RefreshUserInfo();
                    ReqAllPosition();
                    
                }
                catch (Exception ee)
                {
                    ee.WriteToLog();
                    SimpleLogger.Logger.Log(new LogMessage() { Data = ee.Message, CallingClass = "OkexBase", CallingMethod = "ReqLogin", DataCategory = LogCategory.Error });
                    // return false;
                }
                
            });
            return true;
        }
       
        public void Wb_OnRspDepthAll(object sender, ResponseDepth e)
        {
            //处理全市场的20个深度行情，业务上只需要用到20个深度行情
            dicDepth[e.channel] = e.data;   //if the key exists, it will overwrite the value, otherwise, it will add the key value pairs.
                                            //process of FutureDepth data, update the list， all market 20,
            var d = UpdateDepth5LevelAll(e);
            if (d is null) return;
            UpdatedicCustomProduct(d);
        }
        private MajorMarketData UpdateDepth5LevelAll(ResponseDepth fd)
        {           
            List<KeyValuePair<string, MajorMarketData>> list;
            if (!fd.InstrumentID.Contains("SWAP"))
             list = TQMainModel.dicMajorMarketData.ToList().FindAll(x => x.Key.ToOkexFuturesInstrumentid() == fd.InstrumentID);
            else
            list = TQMainModel.dicMajorMarketData.ToList().FindAll(x => x.Key.ToOkexSwapInstrumentid() == fd.InstrumentID);
            if (list.Count == 0) return new MajorMarketData();
            foreach(var item1 in list)
            {
                var item = item1.Value;
                item.BidPrice1 = (decimal)fd.data.bids[0][0];
                item.BidSize1 = (int)fd.data.bids[0][1];
                item.AskPrice5 = (decimal)fd.data.asks[4][0];
                item.AskSize5 = (int)fd.data.asks[4][1];
                item.BidPrice2 = (decimal)fd.data.bids[1][0];
                item.BidSize2 = (int)fd.data.bids[1][1];
                item.AskPrice4 = (decimal)fd.data.asks[3][0];
                item.AskSize4 = (int)fd.data.asks[3][1];
                item.BidPrice3 = (decimal)fd.data.bids[2][0];
                item.BidSize3 = (int)fd.data.bids[2][1];
                item.AskPrice3 = (decimal)fd.data.asks[2][0];
                item.AskSize3 = (int)fd.data.asks[2][1];
                item.BidPrice4 = (decimal)fd.data.bids[3][0];
                item.BidSize4 = (int)fd.data.bids[3][1];
                item.AskPrice2 = (decimal)fd.data.asks[1][0];
                item.AskSize2 = (int)fd.data.asks[1][1];
                item.BidPrice5 = (decimal)fd.data.bids[4][0];
                item.BidSize5 = (int)fd.data.bids[4][1];
                item.AskPrice1 = (decimal)fd.data.asks[0][0];
                item.AskSize1 = (int)fd.data.asks[0][1];
                item.UpdateTime = fd.UpdateTime;
            }
            return list.Last().Value;
        }        
       
        
        public string SendOrderRest(OrderData order)
        {
            string ordertype = "", match = "0";          
           
            JObject r;

            switch (order.Offset)
            {
                case (OffsetType.开仓):
                    ordertype = (order.Direction == TradeDirection.Long) ? "1" : "2";
                    break;
                case (OffsetType.平仓):
                    ordertype = (order.Direction == TradeDirection.Long) ? "4" : "3";
                    break;
            }
            string clientoId ="TQ"+ Utility.GetClientId();
            
            if (order.ExchangeID == EnuExchangeID.OkexSwap)
            {               
                r = SwapPostRequest.makeOrderAsync(order.InstrumentID, ordertype, order.OrderPrice, size: order.OrderSize.ToString(), client_oid: clientoId, match_price: match).Result;
            }
            else
            {                
                r = FuturePostRequest.makeOrderAsync(order.InstrumentID, ordertype, order.OrderPrice, Convert.ToInt32(order.OrderSize), Convert.ToInt32(order.LeverRate), client_oid: clientoId, match_price: match).Result;
            }        

            string resultjson = r.ToString();
            Utility.WriteMemLog("New OK Order: " + resultjson);
            SimpleLogger.Logger.Log(resultjson, LogCategory.Info);
            string errorcode = r.Value<string>("code");

            /*if (errorcode == "32007")//杠杆设置未初始化
            {
                //string setCrossedLeverageAsync = FuturePostRequest.setFixedLeverageAsync(order.InstrumentID.Substring(0, order.InstrumentID.IndexOf("-")), (int)order.LeverRate, order.InstrumentID, order.Direction== TradeDirection.多?"long":"short").Result.ToString();
                r = FuturePostRequest.makeOrderAsync(order.InstrumentID.ToOkexFuturesInstrumentid(), ordertype, order.OrderPrice, size: Convert.ToInt32(order.OrderSize), leverage: 20, client_oid: clientoId, match_price: match).Result;
                resultjson = r.ToString();
                Utility.WriteMemLog("New OK Order: " + resultjson);
                errorcode = r.Value<string>("code");              
            }*/
            if (errorcode != null)
            {
                SimpleLogger.Logger.Log("报单有错： errorcode= " + errorcode, SimpleLogger.LogCategory.Error);
                string message = r.Value<string>("message");               
            }
            return resultjson; 
        }
        public void WriteToLogFile(LogData data, string file)
        {
            StreamWriter writer = null;            
            string fi = string.Format("{0}/Log/{1}.log", Environment.CurrentDirectory, DateTime.Now.ToString("yyMMdd"));

            try
            {
                using (writer = new StreamWriter(fi, true))
                {
                    writer.WriteLine("{0}\r\n-------------------------------------\r\n", data.Message);
                }
            }
            catch (Exception e)
            {               
                OnFatal?.Invoke(this, new WebSocketSharp.ErrorEventArgs(MessageType.ErrorWriteFile, e.Message ));
            }
            finally
            {
                if (writer != null) writer.Close();
            }
        }
        /// <summary>
        /// WebSocket消息推送侦听
        /// </summary>
        /// <param name="msg">WebSocket消息</param>
        private void handleWebsocketMessage(string msg)
        {            
            object sender = null;
            Type ty;
            try
            {
                string dd = msg;
                var ja = JsonConvert.DeserializeObject(msg);
                ty = ja.GetType();
                switch (ty.Name)
                {
                    case "JObject":
                        var eventname = ((JObject)ja).Value<string>("event"); 
                        if(eventname=="login")
                        {                            
                            if (((JObject)ja).Value<string>("success")=="True")
                            {
                                Subscribe().Wait();
                                SimpleLogger.Logger.Log(msg, LogCategory.Debug);
                            }
                            return;
                        }
                        if (eventname == "subscribe")
                        {
                            //
                            return;
                        }
                        var gf = ((JObject)ja).SelectToken("data");
                        var table = ((JObject)ja).Value<string>("table");
                        switch (table)
                        {/*  {[
  {
    "long_qty": "0",
    "long_avail_qty": "0",
    "long_avg_cost": "6.74441599",
    "long_settlement_price": "6.74441599",
    "realised_pnl": "0",
    "short_qty": "0",
    "short_avail_qty": "0",
    "short_avg_cost": "6.14749801",
    "short_settlement_price": "6.14749801",
    "liquidation_price": "0.0",
    "instrument_id": "EOS-USD-190628",
    "leverage": "20",
    "created_at": "2019-05-14T15:19:14Z",
    "updated_at": "2019-05-17T10:03:29Z",
    "margin_mode": "crossed",
    "short_margin": "0.0",
    "short_pnl": "0.0",
    "short_pnl_ratio": "0.08658136197284",
    "short_unrealised_pnl": "0.0",
    "long_margin": "0.0",
    "long_pnl": "0.0",
    "long_pnl_ratio": "-2.036974146080562",
    "long_unrealised_pnl": "0.0"
  }
]}
Newtonsoft.Json.Linq.JToken {Newtonsoft.Json.Linq.JArray}
*/
                            case "futures/position":
                                List<SummaryPositionV3> summaryPositionV3s = gf.ToObject<List<SummaryPositionV3>>();
                                OnRtnSummaryPositionV3?.Invoke(this, summaryPositionV3s);
                                break;
                            case "swap/position":
                                /*      "holding": [
          {
            "avail_position": "221",
            "avg_cost": "5.875",
            "leverage": "100",
            "liquidation_price": "0.414",
            "margin": "3.6887",
            "position": "221",
            "realized_pnl": "0.0949",
            "settlement_price": "5.874",
            "side": "long",
            "timestamp": "2019-05-17T09:41:53.947Z"
          }
        ],
        "instrument_id": "EOS-USD-SWAP",
        "margin_mode": "crossed"
      }*/                               
                                   List<SummaryPositionSwapClass> summaryPositionSwapV3s = gf.ToObject<List<SummaryPositionSwapClass>>();
                                    OnRtnSummaryPositionSwapV3?.Invoke(this, summaryPositionSwapV3s);                                
                                break;
                            case "futures/account": /*+gf  {[
  {
    "EOS": {
      "equity": "4937.68731448",
      "liqui_mode": "legacy",
      "maint_margin_ratio": "",
      "margin": "3.29272308",
      "margin_for_unfilled": "0",
      "margin_frozen": "3.29272308",
      "margin_mode": "crossed",
      "margin_ratio": "1499.57563831",
      "realized_pnl": "0.02477112",
      "total_avail_balance": "4936.13348289",
      "unrealized_pnl": "1.52906047"
    }
}
]}	Newtonsoft.Json.Linq.JToken {Newtonsoft.Json.Linq.JArray}*/

                                var v = gf.ToObject<Dictionary<string, AccountCurrency>>();                                
                                OnRtnUserInfoV3?.Invoke(this, v);
                                break;                           
                            case "swap/account":
                                /*+gf  {[
  {
    "equity": "5004.9136",
    "fixed_balance": "0.0000",
    "instrument_id": "EOS-USD-SWAP",
    "margin": "3.7801",
    "margin_frozen": "0.4147",
    "margin_mode": "crossed",
    "margin_ratio": "11.9309",
    "realized_pnl": "0.0927",
    "timestamp": "2019-05-17T10:06:18.35Z",
    "total_avail_balance": "4999.9254",
    "unrealized_pnl": "4.8954"
  }
]}
Newtonsoft.Json.Linq.JToken {Newtonsoft.Json.Linq.JArray}
*/
                                var g = gf.ToObject<List<SwapInfo>>();                                
                                OnRtnAccountInfoSwap?.Invoke(this, g);                                
                                break;
                            case "futures/ticker":                               
                            case "swap/ticker":
                                var data = gf.ToObject<List<FutureMarketDataV3>>();
                                OnRspFutureTickerV3?.Invoke(sender, data);
                                break;
                            case "swap/depth5":
                            case "swap/depth":
                                var datad = gf.ToObject<List<DepthStruV3>>();
                                foreach (var dataDepth in datad)
                                {

                                    var de = new ResponseDepth();
                                    de.data = dataDepth.ToDepthStru();
                                    de.UpdateTime = Utility.ConvertLongDateTime(de.data.timestamp).ToString("HH:mm:ss.ff");
                                    de.channel = dataDepth.instrument_id;
                                    //区分200增量数据还是全市场20深度数据
                                    if (table == "swap/depth5")   //全市场指定深度数据 
                                    {                                       
                                        de.InstrumentID = dataDepth.instrument_id;
                                        OnRspDepthAll?.Invoke(sender, de);
                                    }
                                    else   //200增量数据                                    
                                    {
                                        OnRspDepthNew?.Invoke(sender, de);
                                    }
                                }
                                break;
                            case "futures/depth5":
                            case "futures/depth":
                                var datas=gf.ToObject<List<DepthStruV3>>();
                                foreach(var dataDepth in datas)
                                {

                                    var de = new ResponseDepth();
                                    de.data = dataDepth.ToDepthStru();
                                    de.UpdateTime = Utility.ConvertLongDateTime(de.data.timestamp).ToString("HH:mm:ss.ff");
                                    de.channel = dataDepth.instrument_id;
                                    //区分200增量数据还是全市场20深度数据
                                    if (table == "futures/depth5")   //全市场指定深度数据 
                                    {
                                        de.InstrumentID = dataDepth.instrument_id;
                                        OnRspDepthAll?.Invoke(sender, de);
                                    }
                                    else   //200增量数据
                                    {
                                        OnRspDepthNew?.Invoke(sender, de);
                                    }
                                }
                                break;                           
                            case "futures/trade":
                                var ctr = gf.ToObject<List<TradeResultV3>>();
                                OnRtnTradeV3?.Invoke(sender, ctr);
                                break;
                            case "swap/order":
                            case "futures/order":
                                var ods = gf.ToObject<List<OrderResultV3>>();
                                OnRtnOrdersResultV3?.Invoke(sender, ods);
                                break;
                            default:
                                Console.WriteLine("未知类型table：" + msg + Environment.NewLine);
                                SimpleLogger.Logger.Log(new LogMessage() { CallingMethod = "HandleWebsocketmessage1", CallingClass = "OkexBase", Data = msg, DataCategory = SimpleLogger.LogCategory.Error });

                                break;
                        }
                        break;
                    default:
                        Console.WriteLine("未知类型"+msg + Environment.NewLine);
                        SimpleLogger.Logger.Log(new LogMessage() { CallingMethod = "HandleWebsocketmessage2", CallingClass = "OkexBase", Data = msg, DataCategory = SimpleLogger.LogCategory.Error });

                        break;
                }
                if (ty.Name == "JArray")
                {
                    JToken gf;
                    int nn = ((JArray)ja).Count;

                    if (dd.Contains("channel"))
                    {
                        for (int i = 0; i < nn; i++)
                        {
                            gf = ((JArray)ja)[i];
                            string stchannel = gf.SelectToken("table").ToString();
                            if (stchannel == null || stchannel.Length < 1)
                            {
                                continue;
                            }
                            if (stchannel.Contains("ticker"))   //处理ticker数据
                            {
                                var rt = gf.ToObject<ResponseTicker>();
                                rt.UpdateTime = rt.data.updateTime = DateTime.Now.ToString("HH:mm:ss.ff");
                                rt.data.channel = stchannel;
                                rt.InstrumentID = stchannel.Substring(17, 3) + "_" + stchannel.Substring(28, stchannel.Length - 28);
                                OnRspFutureTicker?.Invoke(sender, rt.data);
                                return;
                            }
                            else
                            if (stchannel.Contains("depth"))    //case ("ok_sub_future_btc_depth_quarter_usd"):
                            {
                                var de = gf.ToObject<ResponseDepth>();
                                de.UpdateTime = Utility.ConvertLongDateTime(de.data.timestamp).ToString("HH:mm:ss.ff");

                                //区分200增量数据还是全市场20深度数据
                                if (!stchannel.Substring(stchannel.Length - 3, 3).Equals("usd"))   //全市场指定深度数据 
                                {
                                    de.InstrumentID = stchannel.Substring(17, 3) + stchannel.Substring(26, stchannel.Length - 29);
                                    OnRspDepthAll?.Invoke(sender, de);
                                }
                                else   //200增量数据
                                {
                                    OnRspDepthNew?.Invoke(sender, de);
                                }
                                return;
                            }
                            else if (stchannel.Contains("kline"))
                            {
                                MDKLines mdk = gf.ToObject<MDKLines>();
                                OnRspKline?.Invoke(sender, mdk); ;
                                return;
                            }
                            Utility.WriteMemLog(stchannel);
                            switch (stchannel)
                            {
                                case "addChannel":
                                    {
                                        OnRspMessage?.Invoke(sender, dd);
                                        //return;
                                    }
                                    break;
                                case "login":
                                    {
                                        if (Initialized)
                                        {
                                            OnReconnected?.Invoke(sender, new EventArgs());
                                        }
                                        else
                                        {
                                            Initialized = true;
                                            OnRspLogin?.Invoke(sender, gf.ToObject<ResponseLogin>());
                                        }
                                    }
                                    break;                               
                                case ("btc_forecast_price"):
                                    ForecastPrice fp = gf.ToObject<ForecastPrice>();
                                    OnRspForecastPrice?.Invoke(sender, fp);
                                    break;
                               
                                default:
                                    break;
                            }
                        }
                        return;
                    }                    
                }
                else
                if (ty.Name == "JObject")
                {
                    OnHeartBeat?.Invoke(sender, dd);
                    return;
                }
            }
            catch (Exception ee)
            {
                Console.WriteLine("error" + msg + Environment.NewLine+ee.ToString()+ Environment.NewLine);
                SimpleLogger.Logger.Log(new SimpleLogger.LogMessage() { CallingMethod = "HandleWebsocketmessage", CallingClass = "OkexBase", Data = msg, DataCategory = SimpleLogger.LogCategory.Error });

                Utility.WriteMemFile(ee.Message);
            }
        }
        public async Task StartAsync() {            
            webSocketor = new WebSocketor();
            webSocketor.WebSocketPush -= this.handleWebsocketMessage;
            webSocketor.WebSocketPush += this.handleWebsocketMessage;
            webSocketor.OnError += WebSocketor_OnErrorAsync;
            await webSocketor.ConnectAsync();           
            int u = 1;
            Utility.WriteMemLog("Connecting....");
            await Task.Delay(TimeSpan.FromSeconds(u++));
            timer.Elapsed += HeartBeat;
            timer.Start();
        }

        private async void WebSocketor_OnErrorAsync(object sender, string e)
        {
            await webSocketor.rebootAsync();
            SimpleLogger.Logger.Log(new LogMessage() { Data = e, CallingMethod = "WebSocketor_OnErrorAsync", DataCategory=LogCategory.Error });
            //await webSocketor.ConnectAsync();
        }

        //private void WebSocketClient_OnError(object sender, WebSocketSharp.ErrorEventArgs e)
        //{ 
        //    //this is the major procedure to handle the error messages 
        //    try
        //    {                
        //        switch (e.errorType)
        //        {
        //            case MessageType.UnableConnected:
        //                OnFatal?.Invoke(this, new WebSocketSharp.ErrorEventArgs(MessageType.UnableConnected, "Unable to Connected, Please handle it manually!" ));
        //                break;
        //            case MessageType.Connecting:
        //                break;
        //            case MessageType.ConnectionLost:
        //            case MessageType.ConnectionNotOpen:
        //            case MessageType.ErrorOnReceivingMessage:
        //            case MessageType.InterruptedSend:
        //                {
        //                    if (webSocketClient.ReadyState == WebSocketState.Connecting ||
        //                        webSocketClient.ReadyState == WebSocketState.Closing)
        //                        return;
        //                    else
        //                    {
        //                        webSocketClient.SetReadySate(WebSocketState.Connecting);
        //                    }

        //                    OnFatal?.Invoke(this, new WebSocketSharp.ErrorEventArgs(e.errorType, e.errorType.ToString() ));
        //                    OnReqReconnect?.Invoke(this, new EventArgs());                                                     
        //                }
        //                break;
        //            case MessageType.ErrorProtocol:
        //            case MessageType.ErrorSend:
        //            case MessageType.ErrorWriteFile:                    
        //            case MessageType.NullOrUndefined:
        //            case MessageType.Reconnected:
        //            case MessageType.UnableClosed:
        //            case MessageType.ErrorOnHandlingOnOpen:
        //            case MessageType.Callback:
        //            case MessageType.Accepting:
        //                break;                       
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }        
        //}

        public void Wb_OnRspFutureTicker(object sender, FutureMarketData fm)
        {
            UpdatedicCustomProduct(UpdatedicMajorMarketData(fm));
            //TQMainModel.QTicks.Enqueue(mmd);

            #region 维护Tick数据集合，及保存Tick 数据磁盘文件，json格式
            /*List<FutureMarketData> lfm;// = new List<FutureMarketData>();
            if (dicfutureticks.TryGetValue(fm.InstrumentID, out lfm))
            {
                //lfm.Add(fm);
                if (lfm.Count > 10)
                {
                    Task.Run(new Action(() =>
                    {
                        Utility.WriteTickToFile(lfm);
                        lfm.Clear();
                    }));
                }
            }
            else
            {
                while (!dicfutureticks.TryAdd(fm.InstrumentID, new List<FutureMarketData>() { fm }))
                    Thread.Sleep(200);
            }*/
            #endregion
        }
        public void Wb_OnRspFutureTickerV3(object sender, List<FutureMarketDataV3> fm)
        {
            UpdatedicCustomProduct(UpdatedicMajorMarketData(fm));
            //TQMainModel.QTicks.Enqueue(mmd);

            #region 维护Tick数据集合，及保存Tick 数据磁盘文件，json格式
            /*List<FutureMarketData> lfm;// = new List<FutureMarketData>();
            if (dicfutureticks.TryGetValue(fm.InstrumentID, out lfm))
            {
                //lfm.Add(fm);
                if (lfm.Count > 10)
                {
                    Task.Run(new Action(() =>
                    {
                        Utility.WriteTickToFile(lfm);
                        lfm.Clear();
                    }));
                }
            }
            else
            {
                while (!dicfutureticks.TryAdd(fm.InstrumentID, new List<FutureMarketData>() { fm }))
                    Thread.Sleep(200);
            }*/
            #endregion
        }
        public bool CancelOrder(string InstrumentID, string OrderID)
        {
            if (OrderID == null)
                Utility.WriteMemFile("no OrderID available! cannot process cancel order");
            if (InstrumentID.Contains("SWAP"))
                SwapPostRequest.cancelOrderAsync(InstrumentID, OrderID.ToString()).Wait();
            else
            FuturePostRequest.cancelOrderAsync(InstrumentID, OrderID.ToString()).Wait();
            
            return true;
        }
        private MajorMarketData UpdatedicMajorMarketData(FutureMarketData fm)//DepthMarketDataField depthMarket, out MajorMarketData marketdata)
        {
            try
            {
                MajorMarketData mmd;
                //var mmd = new MajorMarketData(fm.InstrumentID);
                if (TQMainModel.dicMajorMarketData.TryGetValue(fm.InstrumentID, out mmd))
                {
                    //get record, update the MajorMarketData info, and Notify the view to update
                    mmd.HighestPrice = (decimal)fm.high;
                    mmd.LowestPrice = (decimal)fm.low;
                    mmd.OpenInterest = fm.hold_amount;
                    mmd.LastPrice = (decimal)fm.last;
                    mmd.AskPrice1 = (decimal)fm.sell;
                    mmd.BidPrice1 = (decimal)fm.buy;
                    //mmd.channel = fm.data.channel;
                    mmd.ContractId = fm.contractId;
                    mmd.LimitLow = (decimal)fm.limitLow;
                    mmd.LimitHigh = (decimal)fm.limitHigh;
                    mmd.Volume = (decimal)fm.vol;
                    mmd.UpdateTime = fm.updateTime;                    
                    mmd.ContractValue = (decimal)fm.unitAmount;
                    mmd.BaseName = Utility.GetBaseName(fm.InstrumentID);
                    return mmd;
                }
                else
                {
                    //QTicks.Enqueue(mmd);
                    return null;
                }
            }
            catch (Exception ne)
            {
                //          MessageBox.Show("Alert:  " + ne.ToString());
                return null;// MajorMarketData;
            }
        }

        private MajorMarketData UpdatedicMajorMarketData(List<FutureMarketDataV3> fms)
        {
            foreach (var fm in fms)
            {
                try
                {
                    List<KeyValuePair<string, MajorMarketData>> list;
                    if (fm.instrument_id.Contains("SWAP"))
                        list = TQMainModel.dicMajorMarketData.ToList().FindAll(x => x.Key.ToOkexSwapInstrumentid() == fm.instrument_id);
                    else 
                        list = TQMainModel.dicMajorMarketData.ToList().FindAll(x => x.Key.ToOkexFuturesInstrumentid() == fm.instrument_id);
                    foreach (var item in list)
                    {
                        var mmd = item.Value;
                        //get record, update the MajorMarketData info, and Notify the view to update
                        mmd.HighestPrice = fm.high_24h;
                        mmd.LowestPrice = fm.low_24h;
                        // mmd.OpenInterest = fm.hold_amount;
                        mmd.LastPrice = fm.last;
                        mmd.AskPrice1 = fm.best_ask;
                        mmd.BidPrice1 = fm.best_bid;
                        //mmd.channel = fm.data.channel;
                        if (!fm.instrument_id.Contains("SWAP")) mmd.ContractId = long.Parse(fm.instrument_id.Remove(0, fm.instrument_id.Length - 6));//todo: not finish
                        mmd.LimitLow = fm.low_24h;
                        mmd.LimitHigh = fm.high_24h;
                        mmd.Volume = fm.volume_24h;
                        mmd.UpdateTime = fm.timestamp.ToString("HH:mm:ss.ff");
                        mmd.ContractValue = 0;//todo: not finish
                        mmd.BaseName = Utility.GetBaseName(item.Key);
                        return mmd;
                    }
                }
                catch (Exception ne)
                {
                    SimpleLogger.Logger.Log(new LogMessage() {  Data= ne.Message, CallingMethod="OkexUpdateMarketData"});
                    return null;
                }
            }
            return null;
        }
        private static void UpdatedicCustomProduct(MajorMarketData marketdata)
        {
            if (marketdata == null)
            {
                //MessageBox.Show("Alert: input marketdata is null ");
                SimpleLogger.Logger.Log("Null MarketData is received", SimpleLogger.LogCategory.Info);
                return;
            }
            var diccust = TQMainModel.dicCustomProduct.Keys.Where(x => x.Contains(marketdata.InstrumentID));  //是否在自定义品种中？           
            if (diccust.Count() > 0)
            {
                ComboMarketData cust;
                foreach (var custt in diccust)
                {
                    cust = TQMainModel.dicCustomProduct[custt];
                    cust.LastPrice = 0;
                    cust.BaseName = TQMainModel.dicCustomProduct[custt].BaseName;
                    cust.BidPrice1 = 0;
                    cust.AskPrice1 = 0;
                    cust.BidSize1 = 0;
                    cust.AskSize1 = 0;
                    cust.UpdateTime = marketdata.UpdateTime;
                    cust.Change = 0;
                }
            }
        }
       
        private void HeartBeat(object sender, System.Timers.ElapsedEventArgs e)
        {
        }
        private void webSocketClient_Error(object sender, WebSocketSharp.ErrorEventArgs e)
        {            
            OnError?.Invoke(sender, e);            
        }            
                   
        public async Task SubscribeAsync(string channle)
        {
            await webSocketor.Subscribe(new List<string>() { channle });            
        }
        public async Task SubscribeAsync(List<string> data)
        {
            await webSocketor.Subscribe(data);
        }
        public void Stop() {

            if (webSocketor!=null)
                webSocketor.Dispose();
        }      
    }
}
