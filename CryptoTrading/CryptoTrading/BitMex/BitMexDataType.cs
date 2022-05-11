using CryptoTrading;
using CryptoTrading.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reactive.Subjects;

namespace BitMex
{
    public class OrderBookItem
    {
        public string Symbol { get; set; }
        public int Level { get; set; }
        public int BidSize { get; set; }
        public decimal BidPrice { get; set; }
        public int AskSize { get; set; }
        public decimal AskPrice { get; set; }
        public DateTime Timestamp { get; set; }
    }
    public class BMInstrumentData
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
    public enum MessageType
    {
        // Do not rename, used in requests
        Ping,
        AuthKey,
        Subscribe,
        Unsubscribe,
        CancelAllAfter,
        KLineRequest,
        // Can be renamed, only for responses
        Error,
        Info,
        Trade,
        OrderBook,
        Wallet,
        Order,
        Position,
        Quote,
        Execution,
        Margin,
        Transact
    }
    public class NonceClass
    {
        public long Nonce;
    }
    public enum ReconnectionType
    {
        /// <summary>
        /// Type used for initial connection to websocket stream
        /// </summary>
        Initial,

        /// <summary>
        /// Type used when connection to websocket was lost in meantime
        /// </summary>
        Lost,

        /// <summary>
        /// Type used when connection to websocket was lost by not receiving any message in given timerange
        /// </summary>
        NoMessageReceived,

        /// <summary>
        /// Type used after unsuccessful previous reconnection
        /// </summary>
        Error
    }
    public class MessageBase
    {
        public virtual MessageType Op { get; set; }
    }
    public abstract class SubscribeRequestBase : RequestBase
    {
        private readonly string _symbol = string.Empty;

        public override MessageType Operation
        {
            get
            {
                return MessageType.Subscribe;
            }
        }

        public string[] Args
        {
            get
            {
                return new[]
                {
                    string.IsNullOrWhiteSpace(Symbol) ? Topic : $"{Topic}:{Symbol}"
                };
            }
        }

        [JsonIgnore]
        public abstract string Topic { get; }

        [JsonIgnore]
        public virtual string Symbol
        {
            get
            {
                return _symbol;
            }
        }
    }
    public enum KLineInterval
    {
        m1,
        m5,
        h1,
        d1
    }

    

    public class Transact
    {
        public string transactID;
        public long account;
        public string currency;
        public string transactType;
        public long amount;
        public long? fee;
        public string transactStatus;
        public string address;
        public string tx;
        public string text;
        public DateTime transactTime;
        public DateTime timestamp;
    }
    public class TransactResponse: ResponseBase
    {
        public override MessageType Op => MessageType.Transact;

        public Transact[] Data { get; set; }

        internal static bool TryHandle(JObject response, Action<TransactResponse> subject)//ISubject<PositionResponse> subject)
        {
            CryptoTrading.Utility.WriteMemLog(response.ToString());
            if (response?["table"]?.Value<string>() != "transact")
                return false;

            var parsed = response.ToObject<TransactResponse>(BitmexJsonSerializer.Serializer);
            if (parsed.Data.Length==0) return true;
            subject?.Invoke(parsed);

            return true;
        }
    }
    public class AuthenticationRequest : RequestBase
    {
        private readonly string _apiKey;
        private readonly string _authSig;
        private readonly long _authNonce;
        private readonly string _authPayload;

        public AuthenticationRequest(string apiKey, string apiSecret)
        {
            BmxValidations.ValidateInput(apiKey, nameof(apiKey));
            BmxValidations.ValidateInput(apiSecret, nameof(apiSecret));
            _apiKey = apiKey;
            _authNonce = BitUtility.BitmexAuthentication.CreateAuthNonce();
            _authPayload = BitUtility.BitmexAuthentication.CreateAuthPayload(_authNonce);

            _authSig = BitUtility.BitmexAuthentication.CreateSignature(apiSecret, _authPayload);
        }

        public override MessageType Operation => MessageType.AuthKey;

        public object[] Args => new object[]
        {
            _apiKey,
            _authNonce,
            _authSig
        };
    }
    public class PositionResponse : ResponseBase
    {
        public override MessageType Op => MessageType.Position;
        public Position[] Data { get; set; }
        internal static bool TryHandle(JObject response, Action<PositionResponse> subject)
        {
            if (response?["table"]?.Value<string>() != "position")
                return false;
            Console.WriteLine(response);
            //SimpleLogger.Logger.Log("Debug: "+response);
            var parsed = response.ToObject<PositionResponse>(BitmexJsonSerializer.Serializer);            
            subject?.Invoke(parsed);
            return true;
        }
    }

    [DebuggerDisplay("Postion: {Symbol}, {Currency}. {LastPrice}, {CurrentQty}")]
    public class Position
    {
        public long Account { get; set; }
        public string Symbol { get; set; }
        public string Currency { get; set; }
        public string Underlying { get; set; }
        public string QuoteCurrency { get; set; }
        public double? Commission { get; set; }
        public double? InitMarginReq { get; set; }
        public double? MaintMarginReq { get; set; }
        public long? RiskLimit { get; set; }
        public double? Leverage { get; set; }
        public bool? CrossMargin { get; set; }
        public double? DeleveragePercentile { get; set; }

        public long? RebalancedPnl { get; set; }
        public long? PrevRealisedPnl { get; set; }
        public long? PrevUnrealisedPnl { get; set; }
        public double? PrevClosePrice { get; set; }

        public DateTime? OpeningTimestamp { get; set; }
        public long? OpeningQty { get; set; }
        public long? OpeningCost { get; set; }
        public long? OpeningComm { get; set; }
        public long? OpenOrderBuyQty { get; set; }
        public long? OpenOrderBuyCost { get; set; }
        public long? OpenOrderBuyPremium { get; set; }
        public long? OpenOrderSellQty { get; set; }
        public long? OpenOrderSellCost { get; set; }
        public long? OpenOrderSellPremium { get; set; }

        public long? ExecBuyQty { get; set; }
        public long? ExecBuyCost { get; set; }
        public long? ExecSellQty { get; set; }
        public long? ExecSellCost { get; set; }
        public long? ExecQty { get; set; }
        public long? ExecCost { get; set; }
        public long? ExecComm { get; set; }

        public DateTime? CurrentTimestamp { get; set; }
        public long? CurrentQty { get; set; }
        public long? CurrentCost { get; set; }
        public long? CurrentComm { get; set; }

        public long? RealisedCost { get; set; }
        public long? UnrealisedCost { get; set; }

        public long? GrossOpenCost { get; set; }
        public long? GrossOpenPremium { get; set; }
        public long? GrossExecCost { get; set; }

        public bool? IsOpen { get; set; }
        public double? MarkPrice { get; set; }
        public long? MarkValue { get; set; }
        public long? RiskValue { get; set; }
        public double? HomeNotional { get; set; }
        public double? ForeignNotional { get; set; }
        public string PosState { get; set; }
        public long? PosCost { get; set; }
        public long? PosCost2 { get; set; }
        public long? PosCross { get; set; }
        public long? PosInit { get; set; }
        public long? PosComm { get; set; }
        public long? PosLoss { get; set; }
        public long? PosMargin { get; set; }
        public long? PosMaint { get; set; }
        public long? PosAllowance { get; set; }
        public long? TaxableMargin { get; set; }
        public long? InitMargin { get; set; }
        public long? MaintMargin { get; set; }
        public long? SessionMargin { get; set; }
        public long? TargetExcessMargin { get; set; }
        public long? VarMargin { get; set; }
        public long? RealisedGrossPnl { get; set; }
        public long? RealisedTax { get; set; }
        public long? RealisedPnl { get; set; }
        public long? UnrealisedGrossPnl { get; set; }
        public long? LongBankrupt { get; set; }
        public long? ShortBankrupt { get; set; }
        public long? TaxBase { get; set; }
        public double? IndicativeTaxRate { get; set; }
        public long? IndicativeTax { get; set; }
        public long? UnrealisedTax { get; set; }
        public long? UnrealisedPnl { get; set; }
        public double? UnrealisedPnlPcnt { get; set; }
        public double? UnrealisedRoePcnt { get; set; }
        public double? SimpleQty { get; set; }
        public double? SimpleCost { get; set; }
        public double? SimpleValue { get; set; }
        public double? SimplePnl { get; set; }
        public double? SimplePnlPcnt { get; set; }
        public double? AvgCostPrice { get; set; }
        public double? AvgEntryPrice { get; set; }
        public double? BreakEvenPrice { get; set; }
        public double? MarginCallPrice { get; set; }
        public double? LiquidationPrice { get; set; }
        public double? BankruptPrice { get; set; }
        public DateTime? Timestamp { get; set; }
        public double? LastPrice { get; set; }
        public long? LastValue { get; set; }
    }
    public static class BitmexJsonSerializer
    {
        public static JsonSerializerSettings Settings
        {
            get
            {
                return new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    Formatting = Formatting.None,
                    Converters = new List<JsonConverter>() { new BitmexStringEnumConverter { CamelCaseText = true } },
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                };
            }
        }

        public static JsonSerializer Serializer => JsonSerializer.Create(Settings);

        public static T Deserialize<T>(string data)
        {
            return JsonConvert.DeserializeObject<T>(data, Settings);
        }

        public static string Serialize(object data)
        {
            return JsonConvert.SerializeObject(data, Settings);
        }
    }

    public class ErrorResponse : MessageBase
    {
        public override MessageType Op => MessageType.Error;

        public double? Status { get; set; }
        public string Error { get; set; }
        public Dictionary<string, object> Meta { get; set; }
        public Dictionary<string, object> Request { get; set; }

        internal static bool TryHandle(JObject response, Action<ErrorResponse> subject)//ISubject<ErrorResponse> subject)
        {
            if (response?["error"] != null)
            {
                var parsed = response.ToObject<ErrorResponse>(BitmexJsonSerializer.Serializer);
                subject?.Invoke(parsed);
                return true;
            }
            return false;
        }
    }
    public class AuthenticationResponse : MessageBase
    {
        public override MessageType Op => MessageType.AuthKey;

        public bool Success { get; set; }

        internal static bool TryHandle(JObject response, Action<AuthenticationResponse> r)//ISubject<AuthenticationResponse> subject)
        {
            try
            {

                if (response?["request"]?["op"]?.Value<string>() != "authKey")
                    return false;
                if (response["status"] != null)
                {
                    if (response["status"].Value<int>() == 400)
                    {                        
                        string path = Environment.CurrentDirectory + "\\configurations\\bitmexconfigs\\BitMexGetnonce.txt";                       
                        var i = response["error"].Value<string>().IndexOf("last nonce");
                        var ss = response["error"].Value<string>().Substring(i + 12, 16);
                        NonceClass nonce = new NonceClass()
                        {
                            Nonce = long.Parse(ss)
                        };
                        nonce.Nonce++;
                        var w = JsonConvert.SerializeObject(nonce);
                        File.WriteAllText(path, w);
                        return false;
                    }
                }
                var parsed = JsonConvert.DeserializeObject<AuthenticationResponse>(response.ToString());// response.ToObject<AuthenticationResponse>(JsonConvert.);
                
                r?.Invoke(parsed);                                                                                      //subject.OnNext(parsed);
                return true;
            }
            catch (Exception ce)
            {
                return false;
            }
        }      
    }

    [DebuggerDisplay("Wallet: {Currency} - {BalanceBtc}")]
    public class Wallet
    {
        public long Account { get; set; }
        public string Currency { get; set; }

        public long? PrevDeposited { get; set; }
        public long? PrevWithdrawn { get; set; }
        public long? PrevTransferIn { get; set; }
        public long? PrevTransferOut { get; set; }
        public long? PrevAmount { get; set; }
        public long? TransferIn { get; set; }
        public long? TransferOut { get; set; }
        public long? Amount { get; set; }
        public long? PendingCredit { get; set; }
        public long? PendingDebit { get; set; }
        public long? ConfirmedDebit { get; set; }

        public DateTime Timestamp { get; set; }
        public string Addr { get; set; }
        public string Script { get; set; }
        public string[] WithdrawalLock { get; set; }

        public double BalanceBtc => BitmexConverter.ConvertToBtc(Currency, Amount ?? 0);
    }
    [DebuggerDisplay("Order: {Symbol}, {OrderQty}. {Price}")]
    public class BMOrder
    {
        public string OrderId { get; set; }//Order ID
        public string origClOrdID { get; set; } //Client Order ID. See POST /order.
        public string ClOrdId { get; set; }//new Client Order ID, requires `origClOrdID
        public string ClOrdLinkId { get; set; }

        public long? Account { get; set; }
        public string Symbol { get; set; }
        public BitmexSide? Side { get; set; }

        public double? SimpleOrderQty { get; set; }  //order quantity in units of the underlying instrument(i.e.Bitcoin)
        public long? OrderQty { get; set; } //order quantity in units of the instrument (i.e. contracts)

        public double? Price { get; set; } //limit price for 'Limit', 'StopLimit', and 'LimitIfTouched' orders

        public long? DisplayQty { get; set; }
        public double? StopPx { get; set; } //trigger price for 'Stop', 'StopLimit', 'MarketIfTouched', and 'LimitIfTouched' orders. Use a price below the current price for stop-sell orders and buy-if-touched orders

        public double? PegOffsetValue { get; set; }//trailing offset from the current price for 'Stop', 'StopLimit', 'MarketIfTouched', and 'LimitIfTouched' orders; use a negative offset for stop-sell orders and buy-if-touched orders. Optional offset from the peg price for 'Pegged' orders.
        public string PegPriceType { get; set; }
        public string Currency { get; set; }
        public string SettlCurrency { get; set; }
        public string OrdType { get; set; }
        public string TimeInForce { get; set; }
        public string ExecInst { get; set; }
        public string ContingencyType { get; set; }
        public string ExDestination { get; set; }
        public BMOrderStatus? OrdStatus { get; set; }
        public string Triggered { get; set; }
        public bool? WorkingIndicator { get; set; }
        public string OrdRejReason { get; set; }
        public double? SimpleLeavesQty { get; set; }  //order quantity in units of the underlying instrument(i.e.Bitcoin)
        public long? leavesQty { get; set; }  //leaves quantity in units of the instrument (i.e. contracts). Useful for amending partially filled orders
        public double? SimpleCumQty { get; set; }
        public long? CumQty { get; set; }
        public double? AvgPx { get; set; }
        public string MultiLegReportingType { get; set; }
        public string Text { get; set; }

        public DateTime? TransactTime { get; set; }
        public DateTime? Timestamp { get; set; }

    }
    public enum BMOrderStatus
    {
        Undefined,
        New,
        Filled,
        PartiallyFilled,
        Canceled
    }
    public class OrderResponse : ResponseBase
    {
        public override MessageType Op => MessageType.Order;

        public BMOrder[] Data { get; set; }

        internal static bool TryHandle(JObject response, Action<OrderData[]> subject)
        {          
            OrderResponse parsed = response.ToObject<OrderResponse>(BitmexJsonSerializer.Serializer);
            if (parsed.Action == BitmexAction.Update)
            {
                if (!parsed.Data[0].OrdStatus.HasValue)
                    return false;
            }
            OrderData[] ordera = BitUtility.ConvertBMOrderOrderData(parsed.Data);
            subject?.BeginInvoke(ordera, (x) => subject(ordera), null);
            return true;
        }
    }
    
    public static class BitmexConverter
    {
        public static double ConvertToBtc(string from, double value)
        {
            var safe = (from ?? string.Empty).Trim();


            if (safe == "XBt")
            {
                return ConvertFromSatoshiToBtc(value);
            }

            if (string.IsNullOrWhiteSpace(safe) || safe.ToLower() == "btc" || safe.ToLower() == "xbt")
                return value;

            throw new BitmexException($"Can't convert from '{safe}' to BTC");
        }

        public static double ConvertToBtc(string from, long value)
        {
            var valueDouble = Convert.ToDouble(value);
            return ConvertToBtc(from, valueDouble);
        }

        public static double ConvertFromSatoshiToBtc(long satoshi)
        {
            return satoshi / 100000000.0;
        }

        public static double ConvertFromSatoshiToBtc(double satoshi)
        {
            return satoshi / 100000000.0;
        }
    }
   
    public class WalletResponse : ResponseBase
    {
        public override MessageType Op => MessageType.Wallet;

        public Wallet[] Data { get; set; }

        internal static bool TryHandle(JObject response, ISubject<WalletResponse> subject)
        {
            if (response?["table"]?.Value<string>() != "wallet")
                return false;

            var parsed = response.ToObject<WalletResponse>(JsonSerializer.Create(new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.None,
                Converters = new List<JsonConverter>() { new BitmexStringEnumConverter { CamelCaseText = true } },
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            }));//BitmexJsonSerializer.Serializer);
            //subject.OnNext(parsed);

            return true;
        }
    }
    public class WalletSubscribeRequest : SubscribeRequestBase
    {
        public override string Topic => "wallet";
    }
    public class ExecutionSubscribeRequest: SubscribeRequestBase
    {
        public override string Topic => "execution";
    }
    public class OrderSubscribeRequest: SubscribeRequestBase
    {
        public override string Topic => "order";
    }
    public class MarginSubscribeRequest : SubscribeRequestBase
    {
        public override string Topic => "margin";
    }
    public class PositionSubscribeRequest: SubscribeRequestBase
    {
        public override string Topic => "position";
    }
    public class TransactSubscribeRequest: SubscribeRequestBase
    {
        public override string Topic => "transact";
    }
    public class AffiliateSubscribeRequest: SubscribeRequestBase
    {
        public override string Topic => "affiliate";
    }
    public enum BitmexAction
    {
        Undefined,
        Partial,
        Insert,
        Update,
        Delete
    }
    public class ResponseBase : MessageBase
    {
        /// <summary>
        /// The type of the message. Types:
        /// 'partial'; This is a table image, replace your data entirely.
        /// 'update': Update a single row.
        /// 'insert': Insert a new row.
        /// 'delete': Delete a row.
        /// </summary>
        public BitmexAction Action { get; set; }

        /// <summary>
        /// Table name / Subscription topic.
        /// Could be "trade", "order", "instrument", etc.
        /// </summary>
        public string Table { get; set; }

        /// <summary>
        /// Attribute names that are guaranteed to be unique per object.
        /// If more than one is provided, the key is composite.
        /// Use these key names to uniquely identify rows. Key columns are guaranteed
        /// to be present on all data received.
        /// </summary>
        public string[] Keys { get; set; }

        /// <summary>
        /// This lists the shape of the table. The possible types:
        /// "symbol" - In most languages this is equal to "string"
        /// "guid"
        /// "timestamp"
        /// "timespan"
        /// "float"
        /// "long"
        /// "integer"
        /// "boolean"
        /// </summary>
        public Dictionary<string, string> Types { get; set; }

        /// <summary>
        /// This lists key relationships with other tables.
        /// For example, `quote`'s foreign key is {"symbol": "instrument"}
        /// </summary>
        public Dictionary<string, string> ForeignKeys { get; set; }


        /// <summary>
        /// These are internal fields that indicate how responses are sorted and grouped.
        /// </summary>
        public Dictionary<string, string> Attributes { get; set; }

        /// <summary>
        /// When multiple subscriptions are active to the same table, use the `filter` to correlate which datagram
        /// belongs to which subscription, as the `table` property will not contain the subscription's symbol.
        /// </summary>
        public FilterInfo Filter { get; set; }
    }
    public class FilterInfo
    {
        public long? Account { get; set; }
        public string Symbol { get; set; }
    }
    public class TradeResponse : ResponseBase
    {
        public override MessageType Op => MessageType.Trade;

        public BMTrade[] Data { get; set; }

        internal static bool TryHandle(JObject response, ISubject<TradeResponse> subject)
        {
            if (response?["table"]?.Value<string>() != "trade")
                return false;

            var parsed = response.ToObject<TradeResponse>(JsonSerializer.Create(new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.None,
                Converters = new List<JsonConverter>() { new BitmexStringEnumConverter { CamelCaseText = true } },
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            })); //BitmexJsonSerializer.Serializer);
            subject.OnNext(parsed);

            return true;
        }
    }
    public class Book10SubscribeRequest: SubscribeRequestBase
    {
        public Book10SubscribeRequest()
        {
            Symbol = string.Empty;
        }

        /// <summary>
        /// Subscribe to order book from selected pair ('XBTUSD', etc)
        /// </summary>
        public Book10SubscribeRequest(string pair)
        {
            BmxValidations.ValidateInput(pair, nameof(pair));
            Symbol = pair;
        }

        public override string Topic => "orderBook10";
        public override string Symbol { get; }
    }
    public class BookSubscribeRequest : SubscribeRequestBase
    {
        /// <summary>
        /// Subscribe to order book from all pairs
        /// </summary>
        public BookSubscribeRequest()
        {
            Symbol = string.Empty;
        }

        /// <summary>
        /// Subscribe to order book from selected pair ('XBTUSD', etc)
        /// </summary>
        public BookSubscribeRequest(string pair)
        {
            BmxValidations.ValidateInput(pair, nameof(pair));
            Symbol = pair;
        }

        public override string Topic => "orderBookL2";
        public override string Symbol { get; }
    }
    public class BookResponse : ResponseBase
    {
        public override MessageType Op => MessageType.OrderBook;

        public BookLevel[] Data { get; set; }

        internal static bool TryHandle(JObject response, ISubject<BookResponse> subject)
        {
            if (response?["table"]?.Value<string>() != "orderBookL2")
                return false;

            var parsed = JsonConvert.DeserializeObject<BookResponse>(response.ToString()); //response.ToObject<BookResponse>(BitmexJsonSerializer.Serializer);
            subject.OnNext(parsed);

            return true;
        }
    }
    
    public class Execution
    {
        public string execID;    //execution ID
        public string orderID;   //order ID
        public string clOrdID;   //client order id
        public string clOrdLinkID;
        public long account;
        public string symbol; 
        public string side;         
        public decimal? lastQty;
        public decimal? lastPx;
        public decimal? underlyingLastPx;
        public string lastMkt;
        public string lastLiquidityInd;
        public decimal? simpleOrderQty;    
        public decimal? orderQty;          
        public decimal? price;
        public decimal? displayQty;
        public decimal? stopPx;
        public decimal? pegOffsetValue;
        public string pegPriceType;
        public string currency;
        public string settlCurrency;
        public string execType;
        public string ordType;
        public string timeInForce;
        public string execInst;
        public string contingencyType;
        public string exDestination;
        public string ordStatus;    //Filled, Caneled, Partially filled, New
        public string triggered;
        public bool workingIndicator;
        public string ordRejReason;
        public decimal? simpleLeavesQty;     //
        public decimal? leavesQty;           //
        public decimal? simpleCumQty;
        public decimal? cumQty;
        public decimal? avgPx;
        public decimal? commission;
        public string tradePublishIndicator;
        public string multiLegReportingType;
        public string text;
        public string trdMatchID;
        public decimal? execCost;
        public decimal? execComm;
        public decimal? homeNotional;
        public decimal? foreignNotional;
        public DateTime? transactTime;
        public DateTime? timestamp;
    }
    public class Margin
    {
        public decimal? Account;
        public string Currency;
        public decimal? RiskLimit;
        public string PrevState;
        public string State;
        public string Action;
        public decimal? Amount;
        public decimal? PendingCredit;
        public decimal? PendingDebit;
        public decimal? ConfirmedDebit;
        public decimal? PrevRealisedPnl;
        public decimal? PrevUnrealisedPnl;
        public decimal? GrossComm;
        public decimal? GrossOpenCost;
        public decimal? GrossOpenPremium;
        public decimal? GrossExecCost;
        public decimal? GrossMarkValue;
        public decimal? RiskValue;
        public decimal? TaxableMargin;
        public decimal? InitMargin;
        public decimal? MaintMargin;
        public decimal? SessionMargin;
        public decimal? TargetExcessMargin;
        public decimal? VarMargin;
        public decimal? RealisedPnl;
        public decimal? UnrealisedPnl;
        public decimal? IndicativeTax;
        public decimal? UnrealisedProfit;
        public decimal? SyntheticMargin;
        public decimal? WalletBalance;
        public double? MarginBalancePcnt;
        public double? MarginLeverage;
        public double? MarginUsedPcnt;
        public decimal? ExcessMargin;
        public double? ExcessMarginPcnt;
        public decimal? AvailableMargin;
        public decimal? WithdrawableMargin;
        public DateTime? Timestamp;
        public decimal? GrossLastValue;
        public double? GrossCommission;
    }
    public class MarginResponse: ResponseBase
    {
        public override MessageType Op => MessageType.Margin;

        public Margin[] Data { get; set; }

        internal static bool TryHandle(JObject response, Action<MarginResponse> r)//ISubject<ExecutionResponse> subject)
        {
            if (response?["table"]?.Value<string>() != "margin")
                return false;

            var parsed = JsonConvert.DeserializeObject<MarginResponse>(response.ToString()); //response.ToObject<BookResponse>(BitmexJsonSerializer.Serializer);
            if (parsed.Data.Length == 0) return false;
            
            r?.BeginInvoke(parsed, x => r(parsed), null);

            return true;
        }
    }
    public class ExecutionResponse : ResponseBase
    {
        public override MessageType Op => MessageType.Execution;

        public Execution[] Data { get; set; }

        internal static bool TryHandle(JObject response, Action<TradeData[]> act)
        {
            if (response?["table"]?.Value<string>() != "execution")
                return false;

            ExecutionResponse parsed = JsonConvert.DeserializeObject<ExecutionResponse>(response.ToString()); //response.ToObject<BookResponse>(BitmexJsonSerializer.Serializer);
            if (parsed.Data.Length == 0) return false;
            TradeData[] td= new TradeData[parsed.Data.Length];
            for (int i = 0; i < parsed.Data.Length; i++)
            {
                var key = "BitMex" + parsed.Data[i].account + parsed.Data[i].orderID;
               
                var n = parsed.Data[i];
                if (n.orderID.Contains("0000")) return true;
                td[i] = new TradeData()
                {
                    InstrumentID = n.symbol,
                    InvestorID = n.account.ToString(),
                    Commission = (double)(n.commission ?? 0),
                    OrderTime = (n.timestamp ?? DateTime.UtcNow).ToString("yyyy-MM-dd HH:mm:ss.ff"),
                    UpdateTime = (n.transactTime ?? DateTime.UtcNow).ToString("yyyy-MM-dd HH:mm:ss.ff"),
                    AvgPrice = n.avgPx??0,
                    OrderPrice = (double)(n.price??0),                    
                    ExchangeID = "BitMex",
                    Direction = n.side == "Buy" ? TradeDirection.Long : TradeDirection.Short,
                    Quant = n.orderQty ?? 0,
                    QuantTraded = n.lastQty??0,//n.cumQty ?? 0,
                    TradeID = n.orderID,                                     
                };
                switch (n.ordStatus)
                {
                    case "Filled":
                        td[i].OrderStatus = OrderStatusType.全部成交;
                        break;
                    case "Canceled":
                        td[i].OrderStatus = OrderStatusType.已撤单;
                        break;
                    case "PartiallyFilled":
                        td[i].OrderStatus = OrderStatusType.部分成交;
                        break;
                    case "New":
                        td[i].OrderStatus = OrderStatusType.未成交;
                        break;
                    default:
                        td[i].OrderStatus = OrderStatusType.未知;
                        break;
                }
                switch (n.ordType)
                {
                    case "Limit":
                        td[i].Ordertype = OrderPriceType.限价;
                        break;
                    case "Market":
                        td[i].Ordertype = OrderPriceType.市价;
                        break;
                    case "Stop":
                        td[i].Ordertype = OrderPriceType.停损价;
                        break;
                    case "StopLimit":
                        td[i].Ordertype = OrderPriceType.停损限价;
                        break;
                }
            }                           
            act?.Invoke(td);

            return true;
        }
    }
    public class BookLevel
    {
        /// <summary>
        /// Order book level id (combination of price and symbol)
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Target symbol
        /// </summary>
        public string Symbol { get; set; }

        public BitmexSide Side { get; set; }

        /// <summary>
        /// Available only for 'partial', 'insert' and 'update' action
        /// </summary>
        public long? Size { get; set; }

        /// <summary>
        /// Available only for 'partial' and 'insert' action, use Id otherwise
        /// </summary>
        public double? Price { get; set; }
    }
    public enum BitmexSide
    {
        Undefined,
        Buy,
        Sell
    }
    public enum BitmexTickDirection
    {
        Undefined,
        MinusTick,
        PlusTick,
        ZeroMinusTick,
        ZeroPlusTick
    }
    public class BMTrade
    {
        public DateTime Timestamp { get; set; }
        public string Symbol { get; set; }
        public BitmexSide Side { get; set; }
        public long Size { get; set; }
        public decimal Price { get; set; }
        public BitmexTickDirection TickDirection { get; set; }
        public string TrdMatchId { get; set; }
        public long? GrossValue { get; set; }
        public double? HomeNotional { get; set; }
        public double? ForeignNotional { get; set; }
    }
    public class QuoteSubscribeRequest : SubscribeRequestBase
    {
        /// <summary>
        /// Subscribe to quote (top level of the book) from all pairs
        /// </summary>
        public QuoteSubscribeRequest()
        {
            Symbol = string.Empty;
        }

        /// <summary>
        /// Subscribe to quote (top level of the book) from selected pair ('XBTUSD', etc)
        /// </summary>
        public QuoteSubscribeRequest(string pair)
        {
            BmxValidations.ValidateInput(pair, nameof(pair));
            Symbol = pair;
        }

        public override string Topic => "quote";
        public override string Symbol { get; }
    }
    public class KLineRequest : RequestBase
    {
        public string binSize;
        public bool partial;
        public string symbol;
        public string filter;
        public string columns;
        public double count;
        public double start;
        public bool reverse;
        public DateTime startTime;
        public DateTime endTime;
        public override MessageType Operation { get { return MessageType.KLineRequest; } }
    }
    
    public class TradesSubscribeRequest : SubscribeRequestBase
    {
        /// <summary>
        /// Subscribe to all trades
        /// </summary>
        public TradesSubscribeRequest()
        {
            Symbol = string.Empty;
        }

        /// <summary>
        /// Subscribe to trades for selected pair ('XBTUSD', etc)
        /// </summary>
        public TradesSubscribeRequest(string pair)
        {
            BmxValidations.ValidateInput(pair, nameof(pair));
            Symbol = pair;
        }

        public override string Topic => "trade";
        public override string Symbol { get; }
    }
    public class BitmexStringEnumConverter : StringEnumConverter
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            try
            {
                return base.ReadJson(reader, objectType, existingValue, serializer);
            }
            catch
            {
                //Log.Warning($"Can't parse enum, value: {reader.Value}, target type: {objectType}, using default '{existingValue}'");
                return existingValue;
            }
        }
    }
    public class PingRequest : RequestBase
    {
        public override MessageType Operation => MessageType.Ping;

        public override bool IsRaw => true;
    }
    public abstract class RequestBase : MessageBase
    {
        public override MessageType Op
        {
            get => Operation;

            set { }
        }

        [JsonIgnore]
        public abstract MessageType Operation { get; }

        [JsonIgnore]
        public virtual string OperationString => Operation.ToString().ToLower();

        [JsonIgnore]
        public virtual bool IsRaw { get; } = false;
    }
}
