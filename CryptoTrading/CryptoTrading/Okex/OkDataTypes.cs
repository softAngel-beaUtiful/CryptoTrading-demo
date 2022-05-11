using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media;

namespace CryptoTrading
{
    public enum CryptoCurrency
    {
        btc,
        ltc,
        eth
    }
    public enum ContractType
    {
        this_week,
        next_week,
        quarter
    }
    public enum Interval
    {
        m1,
        m3,
        m5,
        m15,
        m30,
        h1,
        h2,
        h4,
        h6,
        h12,
        d,
        d3,
        w
    }
    public enum ExchangeId
    {
        Okex,
        Okcoin,
        Bitfinex,
        Poloniex,
        GDAX,
        AEX
    }
    public enum lever_rate
    {
        ten,
        twenty
    }
    public class ForecastPrice
    {
        public string data;
        public string channel;
    }
    public class FutureMarketData    //Okex future marketdata
    {              
        public double limitHigh;//limitHigh(string):最高买入限制价格
        public double vol; //vol(double):24小时成交量
        public double last;//last(double):最新价
        public double sell;//sell(double):卖一价格
        public double buy; //buy(double):买一价格
        public double unitAmount;//unitAmount(double):合约价值
        public int hold_amount;//hold_amount(double):当前持仓量
        public long contractId;//contractId(long):合约ID
        public double high; //high(double):24小时最高价格
        public double low;  //low(double):24小时最低价格         
        public double limitLow;//limitLow(string):最低卖出限制价;格    
        public string channel;   //货币行情信息标识
        public string updateTime;
        public string InstrumentID
        {  
            get
            {
                return channel.Substring(17, 3) + "_" + channel.Substring(28, channel.Length - 28);
            }
        }
    }
    public class FutureMarketDataV3    //Okex future marketdata
    {
        /// <summary>
        /// 
        /// </summary>
        public decimal last { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal best_bid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal high_24h { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal low_24h { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal volume_24h { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal best_ask { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string instrument_id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime timestamp { get; set; }
    }

    /*public class ResponseDepth
    {
        //asks(array):卖单深度 数组索引(string) [价格, 量(张), 量(币),累计量(币),累积量(张)]
        //bids(array):买单深度 数组索引(string) [价格, 量(张), 量(币), 累计量(币), 累积量(张)]
        //使用描述:
        //1，第一次返回全量数据
        //2，根据接下来数据对第一次返回数据进行，如下操作
        //删除（量为0时）
        //修改（价格相同量不同）
        //增加（价格不存在）
        public DepthStru data;
        public string channel;
    }*/
    public class Response
    {
        public string InstrumentID;
        public int binary;
        public string channel;
        public string UpdateTime;
        
    }
    public class ResponseDepth: Response
    {      
        public DepthStru data;
        
        
           // get { return Utility.ConvertLongDateTime(data.timestamp).ToString("HH:dd:ss.ff"); }
        
    }
    public class ResponseTicker: Response
    {
        public FutureMarketData data;
        //public string UpdateTime;
        //public new string InstrumentID;
    }
    public class SpreadBidAsk
    {
        public string spreadname;
        public double ASK;
        public double Bid;
        public double Spread { get { return ASK - Bid; } }
    }
    public class DepthStru
    {
        public long timestamp;
        public double[][] asks;
        public double[][] bids;
    }
    public class DepthStruV3
    {
        public DateTime timestamp;
        public double[][] asks;
        public double[][] bids;
        public string instrument_id;
        public DepthStru ToDepthStru()
        {
            return new DepthStru()
            {
                asks = this.asks,
                bids = this.bids,
                timestamp = Utility.ConvertDataTimeLong(this.timestamp)
            };
        }
    }
    public class FutureIndexData
    {
        public long timestamp;
        public double futureindex;
    }
    public class FutureIndex
    {
        public FutureIndexData data;
        public string channel;
    }
    public class ClassTradeResult
    {
        public TradeResult data;
        public string channel;
    }
    public class TradeResult
    {
        public bool result;
        public long order_id;
    }
    public class TradeResultV3
    {
        /// <summary>
        /// 
        /// </summary>
        public string side { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string trade_id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal price { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal qty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string instrument_id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime timestamp { get; set; }
    }
    public class ContractTrade
    {
        public string tradeid { get; set; }
        public string price { get; set; }
        public string quant { get; set; }
        public string time { get; set; }
        public string type { get; set; }
        //public string amount { get; set; }

    }
    public class ResponseLogin
    {
        public int binary;
        public string channel;
        public LoginResult data;
        public class LoginResult
        {
            public bool result;
        }
    }
    /*public class TradeRsp
    {
        public string base;
            public 
    }*/
    public class ContractTradeInfo
    {
        public int binary;
        public string[][] data;
        public string channel;
    }
    public class DepthData
    {
        public double price;
        public double quant;
        public double amount;
        public double totalquant;
        public double totalamount;
    }
    public class SpotMarketData
    {
        public double vol; //vol(double):24小时成交量
        public double last;//last(double):最新价
        public double sell;//sell(double):卖一价格
        public double buy; //buy(double):买一价格               
        public double high; //high(double):24小时最高价格
        public double low;  //low(double):24小时最低价格        
        public double dayhigh;  //当日最高价
        public double daylow;   //当日最低价
        public string channel;   //货币行情信息标识
        public long timestamp;   //时间戳标识}
        public double change;    //涨跌        
    }
    public class OrderResultV3
    {
        /// <summary>
        /// 
        /// </summary>
        public int leverage { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime last_fill_time { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal filled_qty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal fee { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal price_avg { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string type { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string client_oid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal last_fill_qty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string instrument_id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal last_fill_px { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal size { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal price { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string state { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string contract_val { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string order_id { get; set; }
        /// <summary>
        /// 0：普通委托 1：只做Maker（Post only） 2：全部成交或立即取消（FOK） 3：立即成交并取消剩余（IOC）
        /// </summary>
        public string order_type { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime timestamp { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string status { get; set; }
        public string side { get;  set; }
        public decimal pnl { get; set; }
    }
    public class FutureOrder
    {
        public string api_key;
        public string secret_key;

        public string sign
        {
            get
            {
                string sig = Utility.GenerateSign(new Dictionary<string, string>{
                { "api_key", api_key},
                { "symbol", Currency },
                { "contract_type", ContractType.ToString() },
                { "price", Price.ToString() },
                { "amount", Quant.ToString() },
                {"type", Type.ToString() },
                {"match_price",MatchPrice.ToString() },
                {"lever_rate", LeverRate }
                },
                secret_key);
                return sig;
            }
        }
        public string InstrumentID;
        public string Currency;
        public string ContractType;
        public double Price;
        public double Quant;
        public string Type;
        public string MatchPrice;
        public string LeverRate;
    }    
    public enum PriceMode
    {
        OppositePrice,
        OppositePricePlusOne,
        MiddlePrice,
        OwnsidePricePlusOne,
        OwnsidePrice
    }

    public class MarketAccess
    {
        public string Name;
        public string UserID;
        public string api_key;
        public string secret_key;

        public string passphrase { get;  set; }
    }
    public enum TradeType
    {
        Long = 1,
        Short,      //2
        CloseLong,  //3
        CloseShort  //4
    }

    public class CancelOrderParams
    {
        public string api_key;
        public string secret_key;
        public string sign
        {
            get { return Utility.GenerateSign(new Dictionary<string, string> { }, secret_key); }
        }
        public string Currency;
        public string OrderID;
        public ContractType ContractType;               
    }
    public class ContractAccount
    {
        public string api_key;
        public string secret_key;
        public string sign
        {
            get
            {                
                return Utility.GenerateSign(new Dictionary<string, string> { { "api_key", api_key } }, secret_key);               
            }
        }
    }
    public class HistoricTrade
    {
        public int amount;
        public long date;
        public long date_ms;
        public double price;
        public long tid;
        public string type;
    }
    public class QueryOrderInfo
    {
        public string api_key;
        public string secret_key;
        public string sign
        {
            get
            {
                return Utility.GenerateSign(new Dictionary<string, string>
                { }, secret_key);
            }
        }
        public string Currency;
        public string OrderID;
        public ContractType ContractType;
        public string Status;
        public string CurrentPage;
        public string PageLength;
    }
    public class FutureTradeData   //全仓模式
    {
        public double lever_rate;
        public double amount;
        public long orderid;
        public long contract_id;
        public double fee;
        public string contract_name;
        public double unit_amount;
        public double price_avg;
        public int type;  //order type 1: open long, 2: open short, 3: close long, 4: close short
        public double deal_amount; //deal_amount(string) : filled quantity
        public string contract_type;
        public string user_id;
        public int system_type;
        public double price;
        public string create_date_str;
        public long create_date;
        public string status; //-1 = cancelled, 0 = unfilled, 1 = partially filled, 2 = fully filled, 4 = cancel request in process
    }
    public class FutureTrade
    {
        public FutureTradeData data;
        public string channel;
    }
    public class CancelFutureOrderParams : Dictionary<string, string>
    { }

    public class ContractIndex
    { }
    public class ContractTransaction
    {

    }
    public class Login
    {
        public string api_key;
        public string sign;
    }
    /*public class FutureTradeInfo
    {
        public string api_key;
        public string sign;
        public string symbol;
        public string contract_type;
        public string price;
        public string amount;
        public string type;
        public string match_price;
        public string lever_rate;
    }*/
    public class RspTrade
    {
        public bool result;
        public long order_id;
    }
    public class WholePosition
    {
        public string position;
        public string contract_name;
        public double costprice;
        public double bondfreez;
        public double avgprice;
        public double contract_id;
        public string position_id;
        public string eveningup;
        public string hold_amount;
        public string margin;
        public string realized;
    }
    public class RspPosition
    {
        public double contractavgprice;    //平均持仓价格
        public double margin;             //固定保证金
        public double contractcostprice;  //开仓价格
        public long contract_id;           //合约id 
        public double holdAmount;          //持仓量
        public double contractbondfreez;  //当前合约冻结保证金
        public double contracteveningup; //可平仓量
        public string contract_name;    //合约名称
        public double realized;         //已实现盈亏
        public string contractposition;  //position(string): 仓位 1多仓 2空仓
        public long position_id;          //持仓编号
    }
    public class RestRspPosition
    {
        public bool result;
        public SummaryPosition[] holding;
        public double force_liqu_price;
    }
   
    public class ClassRspPosition
    {
        public long user_id;
        public RspPosition[] positions;
        public static ClassRspPosition GetClassRspPosition(PositionDetails ps)
        {
            ClassRspPosition cr = new ClassRspPosition();
            cr.user_id = ps.data.user_id;
            if (ps.data.positions.Length <= 0)
            {
                return null;
            }
            cr.positions = new RspPosition[ps.data.positions.Length];
            
            for (int i=0;i<ps.data.positions.Length;i++)
            {
                cr.positions[i] = new RspPosition();
                cr.positions[i].contractavgprice = ps.data.positions[i].avgprice;
                cr.positions[i].contractbondfreez = ps.data.positions[i].bondfreez;
                cr.positions[i].contractcostprice = ps.data.positions[i].costprice;
                cr.positions[i].contracteveningup = ps.data.positions[i].eveningup;
                cr.positions[i].contractposition = ps.data.positions[i].position;
                cr.positions[i].contract_id = ps.data.positions[i].contract_id;
                cr.positions[i].contract_name = ps.data.positions[i].contract_name;
                cr.positions[i].holdAmount = ps.data.positions[i].hold_amount;
                cr.positions[i].margin = ps.data.positions[i].margin;
                cr.positions[i].position_id = ps.data.positions[i].position_id;
                cr.positions[i].realized = ps.data.positions[i].realized;
            }
            return cr;
        }
    }
    public class PositionDetails
    {
        public PositionClass data;
        public string channel;
    }
    public class PositionClass
    {
        public string symbol;
        public long user_id;
        public PositionInfo[] positions;
    }
    public class PositionInfo    //全仓说明
    {
        public double margin; // margin(double): 固定保证金:0,
        public double avgprice;//  avgprice(string): 开仓均价:"994.89453079",
        public long contract_id;//contract_id(long): 合约id:20170630013,
        public double hold_amount; // hold_amount(string): 持仓量:"0",
        public string contract_name; //contract_name(string): 合约名称:"BTC0630",
        public double realized;// realized(double):已实现盈亏:0
        public double costprice;// costprice(string): 开仓价格 "994.89453079",
        public double bondfreez;// bondfreez(string): 当前合约冻结保证金:"0.0025",
        public double eveningup;//  eveningup(string): 可平仓量:"0",
        public string position; //position(string): 仓位 1多仓 2空仓
        public long position_id;//  position_id(long): 仓位id:27782857,
    }
    public class SinglePosition  //逐仓说明
    {
        public long contract_id;  //合约id
        public string contract_name;  // (string): 合约名称
        public double avgprice; // (string): 开仓均价
        public double balance;  // (string): 合约账户余额
        public double bondfreez; // (string): 当前合约冻结保证金
        public double costprice;   // (string): 开仓价格
        public double eveningup; // (string): 可平仓量
        public double forcedprice; // (string): 强平价格
        public string position; // (string): 仓位 1多仓 2空仓
        public double profitreal; // (string): 已实现盈亏
        public double fixmargin;  // (double): 固定保证金
        public double hold_amount; // (string): 持仓量
        public double lever_rate; // (double): 杠杆倍数
        public long position_id; // (long): 仓位id
        public string symbol; // (string): btc_usd ltc_usd
        public long user_id;  // (long):用户ID
    }
    public class SummaryPosition
    {
        public double buy_price_avg;
        public string symbol;
        public int lever_rate;
        public int buy_available;
        public long contract_id;
        public int buy_amount;
        public double buy_profit_real;
        public string contract_type;
        public int sell_amount;
        public double sell_price_cost;
        public double buy_price_cost;
        public long create_date;
        public double sell_price_avg;
        public double sell_profit_real;
        public int sell_available;
        public double force_liqu_price;
    }
    public class SummaryPositionSwapClass
    {
        public List<SummaryPositionSwapV3> holding;
        public string instrument_id;
        public string margin_mode;
    }
    public class SummaryPositionSwapV3
    {
        /*{ "holding": [
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
]}
       */
       public decimal avail_position { get; set; }
        public decimal avg_cost { get; set; }
        public string instrument_id { get; set; }
        public int leverage { get; set; }
        public decimal liquidation_price { get; set; }
        public decimal margin { get; set; }
        public int position { get; set; }
        public decimal realized_pnl { get; set; }
        public decimal settlement_price { get; set; }
        public string side { get; set; }
        public DateTime timestamp { get; set; }
    }
    
    public class SummaryPositionV3
    {
        /// <summary>
        /// 
        /// </summary>
        public decimal long_qty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal long_avail_qty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal long_avg_cost { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal long_settlement_price { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal realised_pnl { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal short_qty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal short_avail_qty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal short_avg_cost { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal short_settlement_price { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal liquidation_price { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string instrument_id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int leverage { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime created_at { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime updated_at { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string margin_mode { get; set; }        
    }
    public class ClassUserInfo
    {
        public int binary;
        public string channel;
        public UserInfo data;
    }
    public class UserInfo
    {
        public bool result;
        public Dictionary<string, userInfo> info;
    }
    public class AccountCurrency
    {        
        /*  {[
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
]}*/
        public decimal equity { get; set; }
        public string liqui_mode { get; set; }
        public string maint_margin_ratio { get; set; }
        public decimal margin { get; set; }
        public decimal margin_for_unfilled { get; set; }
        public decimal margin_frozen { get; set; }
        public string margin_mode { get; set; }
        public decimal margin_ratio { get; set; }
        public decimal realized_pnl { get; set; }
        public decimal total_avail_balance { get; set; }
        public decimal unrealized_pnl { get; set; }        
    }

    public class AccountCurrencyInfo
    {
        /// <summary>
        /// 
        /// </summary>
        public string auto_margin { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<AccountCurrency> contracts { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal equity { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string margin_mode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal total_avail_balance { get; set; }
    }

    public class OkFutureAccountV3Response
    {
        /// <summary>
        /// 
        /// </summary>
        public Dictionary<string, AccountCurrency> info { get; set; }
    }
    public class AccountInfoSwap
    {
        public List<SwapInfo> info;
   /* }"info": [
        {
                    "equity": "5000.0068",
          "fixed_balance": "0.0000",
          "instrument_id": "EOS-USD-SWAP",
          "margin": "0.0306",
          "margin_frozen": "0.0000",
          "margin_mode": "crossed",
          "margin_ratio": "1632.8597",
          "realized_pnl": "-0.0007",
          "timestamp": "2019-05-16T22:55:37.212Z",
          "total_avail_balance": "5000.0000",
          "unrealized_pnl": "0.0074"*/
        }
    public class SwapInfo
    {
        public decimal equity { get; set; }
        public decimal fixed_balance { get; set; }
        public string instrument_id { get; set; }
        public decimal margin { get; set; }
        public decimal margin_frozen { get; set; }
        public string margin_mode { get; set; }
        public decimal margin_ratio { get; set; }
        public decimal realized_pnl { get; set; }
        public DateTime timestamp { get; set; }
        public decimal total_avail_balance { get; set; }
        public decimal unrealized_pnl { get; set; }
    }
    public class userInfo
    {
        public double risk_rate;
        public double account_rights;
        public double profit_unreal;
        public double profit_real;
        public double keep_deposit;        
    }
    public class MyUserInfo
    {
        public FutureUserInfo btc;
        public FutureUserInfo ltc;
    }
    public struct FutureUserInfo   //全仓信息
    {
        public double risk_rate; // 保证金率
        public double account_rights;// 账户权益    
        public double profit_unreal; // 未实现盈亏
        public double profit_real; // 已实现盈亏
        public double keep_deposit; // (double)：保证金               
    }
    public class FutureUserInfoDetail  //逐仓信息
    {
        public double available; // (double):合约可用
        public double balance; // (double):合约余额
        public double bond; //:固定保证金
        public long contract_id; // (long):合约ID
        public string contract_type; //contract_type(string):合约类别
        public double freeze; // (double):冻结
        public double profit; // (double):已实现盈亏
        public double unprofit; // (double):未实现盈亏
        public double rights; // (double):账户权益
    }
    public class MDKLines
    {
        public string[][] data;
        public string channel;
    }
    public class MDKLine
    {
        public long Time;
        public double Open;
        public double High;
        public double Low;
        public double Close;
        public double ContractVol;
        public double Vol;
    }
    public enum SpotSymbol
    {
        ltc_btc,
        eth_btc,
        etc_btc,
        bch_btc,
        btc_usdt,
        ltc_usdt,
        etc_usdt,
        bch_usdt,
        etc_eth,
        bt1_btc,
        bt2_btc,
        btg_btc,
        qtum_btc,
        hsr_btc,
        neo_btc,
        gas_btc,
        qtum_usdt,
        hsr_usdt,
        neo_usdt,
        gas_usdt
    }
    public enum ClassType
    {
        FutureMarketData,
        SpotMarketData,
        TextResponse,
        FutureTrade,
        AccountInfo,
        None,
    }
    public class AccountInfo
    {
        public double balance;
        public long user_id;
        public double unit_amount;
        public double profit_real;
        public double keep_deposit;
    }
}