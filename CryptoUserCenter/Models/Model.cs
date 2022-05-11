using ExchangeSharp;
using ExchangeSharp.BinanceGroup;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

public class ExchangeAPICred
{
    public string Public { get; set; }
    public string Private { get; set; }
    public string PassPhrase { get; set; }
}

public class Model 
{
    public Dictionary<string, ExchangeAPICred> Dictapi; 
    public ObservableCollection<GeneralAccount> ObservableAccounts { get; set; }
    public ObservableCollection<GeneralPosition> ObservablePositions { get; set; }  
    public ObservableCollection<ExchangeFutureOrder> ObservableOrders { get; set; }
    public ObservableCollection<ExecutionRecord> ObservableTrades { get; set; }
    Dictionary<string, IWebSocket> dictWebSocket = new Dictionary<string, IWebSocket>();
    public Model()
    {
        ObservableAccounts = new ObservableCollection<GeneralAccount>();
        ObservablePositions = new ObservableCollection<GeneralPosition>();
        ObservableOrders = new ObservableCollection<ExchangeFutureOrder>();
        ObservableTrades = new ObservableCollection<ExecutionRecord>();
        InitializeAllExchangeApis();
        EstablishConnectionAsync();       
    }  

    private async Task EstablishConnectionAsync()
    {
        foreach (var api in Dictapi)
        {
            var exchangeAPI = ExchangeAPI.GetExchangeAPI(api.Key);
            exchangeAPI.LoadAPIKeysUnsecure(api.Value.Public, api.Value.Private);

            if (exchangeAPI.Name.Contains("Binance"))
            {

                var o = await ((BinanceGroupCommon)exchangeAPI).GetAccountAndPositionsAsync();
                foreach (var a in o.Item1)
                {
                    ObservableAccounts.Add(a.Value);
                }
                foreach (var t in o.Item2)
                {
                    ObservablePositions.Add(t.Value);
                }
                if (exchangeAPI.Name == "BinanceCOINFuture" || exchangeAPI.Name == "BinanceUSDFuture")
                {
                    var p = await ((BinanceGroupCommon)exchangeAPI).GetAllOpenOrdersAsync();
                    
                    foreach (var order in p)
                    {
                        order.ExchangeID = "Binance";
                        order.AccountType = exchangeAPI.Name.Substring(7);
                        ObservableOrders.Add(order);
                    }
                }                
                var BinanceListenKey = await ((BinanceGroupCommon)exchangeAPI).GetListenKeyAsync();
                var kvws = await ((BinanceGroupCommon)exchangeAPI).GetUserDataWebSocketAsync
                    ((x) => CallbackUserData(x, exchangeAPI.Name), BinanceListenKey);
                dictWebSocket[exchangeAPI.Name]=kvws;
            }
            if (exchangeAPI.Name=="Bybit")            
            {
                IEnumerable<KeyValuePair<string, GeneralAccount>> kvaccount = await ((ExchangeBybitAPI)exchangeAPI).GetGeneralAccountsAsync();               
                foreach (var s in kvaccount)
                {
                    if (s.Value.Equity>0)
                    ObservableAccounts.Add(new GeneralAccount()
                    {
                        ExchangeID = "Bybit",
                        Symbol = s.Key,
                        UserID = s.Value.UserID,
                        Equity = s.Value.Equity,
                        Available = s.Value.Available,
                        OrderMargin = s.Value.OrderMargin,
                        PositionMargin = s.Value.PositionMargin,
                        UnrealizedPNL = s.Value.UnrealizedPNL,
                        RealizedPNL = s.Value.RealizedPNL,               
                        AccountType = s.Value.AccountType,
                        UpdateTime = s.Value.UpdateTime
                    });                    
                }
                var oo = await ((ExchangeBybitAPI)exchangeAPI).GetFullOpenOrdersAsync();
                foreach(var o in oo)
                ObservableOrders.Add(o);
                var t = await ((ExchangeBybitAPI)exchangeAPI).GetAllOpenPositionsAsync(null);
                var kvw = await ((ExchangeBybitAPI)exchangeAPI).GetPositionWebSocketAsync(new Action<GeneralPosition>((x) => { HandleRealtimePosition(x, exchangeAPI.Name); }), new Action<GeneralAccount>((y)=> { UpdateAccount(y); }));                       
                dictWebSocket[exchangeAPI.Name] = kvw;
                var kk = new KeyValuePair<string, IWebSocket>("ByBit", await ((ExchangeBybitAPI)exchangeAPI).GetExecutionWebSocketAsync((y) => HandleRealtimeExecution(y, "Bybit")));
                var jj = new KeyValuePair<string, IWebSocket>("ByBit1", await ((ExchangeBybitAPI)exchangeAPI).GetOrderWebSocketAsync((e) => UpdateOrders(e, "Bybit")));
            }                
        }       
    }

    private void UpdateAccount(GeneralAccount e)
    {
        e.Symbol = e.Symbol.Substring(0, 3);
        var v = ObservableAccounts.Where(z => z.Symbol == e.Symbol
                           && z.ExchangeID == "Bybit");
        if (v.Count() > 0)
        {
            var a = v.First();
            e.RealizedPNL = a.RealizedPNL;
            a = e;
        }
        else
            Application.Current.Dispatcher.BeginInvoke(new Action(() => ObservableAccounts.Add(e)));
    }

    private void UpdateOrders(ExchangeFutureOrder e, string exchangeName)
    {
        var v = ObservableOrders.Where(z => z.Symbol == e.Symbol
                          && z.OrderId == e.OrderId && z.ExchangeID==exchangeName);
        if (v.Count() > 0)
        {            
            v.First().AvgPrice = e.AvgPrice;
            v.First().FilledQty = e.FilledQty;
            v.First().UpdateTime = e.UpdateTime; 
            v.First().LastFilledPrice = e.LastFilledPrice;
            v.First().LastFilledQty = e.LastFilledQty;
            v.First().Commission = e.Commission;
            v.First().CommissionAsset = e.CommissionAsset;
            v.First().MarginAsset = e.MarginAsset;
            v.First().OrderStatus = e.OrderStatus;
            v.First().ExecutionType = e.ExecutionType;
            v.First().RealizedProfit = e.RealizedProfit;
            v.First().ClientOrderId = e.ClientOrderId;
        }
        else
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            ObservableOrders.Add(new ExchangeFutureOrder()
            {
                AvgPrice = e.AvgPrice,//TradePrice;
                FilledQty = e.FilledQty,
                UpdateTime = e.UpdateTime,
                LastFilledPrice = e.LastFilledPrice,
                Symbol = e.Symbol,
                ExchangeID = "Bybit",
                OrderTime = e.OrderTime,
                OrderId = e.OrderId,
                ClientOrderId = e.ClientOrderId,
                Commission = e.Commission,
                CommissionAsset = e.CommissionAsset,
                AccountType = "Future",
                IsReduceOnly = e.IsReduceOnly,
                Side = e.Side,
                OrderType = e.OrderType,
                OriginalOrderType = e.OrderType,
                ExecutionType = e.ExecutionType,
                IsMaker = e.IsMaker,
                OrderStatus = e.OrderStatus,
                OrderTradeId = e.OrderTradeId,
                OrigQty = e.OrigQty,
                MarginAsset = e.MarginAsset,
                RealizedProfit = e.RealizedProfit,
                TimeInForce = e.TimeInForce,
                ACTIVATIONPRICE = e.ACTIVATIONPRICE,
                LastFilledQty = e.LastFilledQty,
                OrigPrice = e.OrigPrice,
                StopPrice = e.StopPrice,
                StopPriceType = e.StopPriceType                
            })));
        }
    }

    private void HandleRealtimeExecution(IEnumerable<ExecutionRecord> y, string exchangeName)
    {
        foreach (var x in y)
        {
            var v = ObservableTrades.Where(z => z.symbol == x.symbol
                           && z.OrderID == x.OrderID && z.TradeID ==x.TradeID);
            if (v.Count() > 0)
            {
                v.First().TradePrice = x.TradePrice;
                v.First().TradedQty = x.TradedQty;
                v.First().TradeTime = x.TradeTime;
                v.First().TradeType = x.TradeType;
                v.First().UnTraded = x.UnTraded;
                v.First().TradeID = x.TradeID;
                v.First().Fee = x.Fee;
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                ObservableTrades.Add(new ExecutionRecord()
                {
                    symbol = x.symbol,
                    OrderQty = x.OrderQty,
                    TradePrice = x.TradePrice,
                    TradedQty = x.TradedQty,
                    TradeTime = x.TradeTime,
                    TradeType = x.TradeType,
                    IsMaker = x.IsMaker,
                    UnTraded = x.UnTraded,
                    Fee = x.Fee,                    
                    OrderID = x.OrderID,
                    LongOrShort = x.LongOrShort,
                    TradeID = x.TradeID
                })));
            }
        }
    }

    private void HandleRealtimePosition(GeneralPosition x, string exchangeName)
    {
        if (x.Position == 0) return;
        var v = ObservablePositions.Where(z => z.ExchangeID == exchangeName
                       && z.Symbol == x.Symbol);
        if (v.Count() > 0)
        {
            //v.First().UnrealizedPnl = x.//decimal.Parse(c.UnrealizedPnL);
            //v.First().RealizedPnl = x.//decimal.Parse(c.AccumulatedRealized);
            v.First().Position = x.Position;
            v.First().AvgCost = x.AvgCost;
            v.First().UpdateTime = x.UpdateTime;
            v.First().UnrealizedPnl = x.UnrealizedPnl;
            v.First().RealizedPnl = x.RealizedPnl;
            v.First().LiquidationPrice = x.LiquidationPrice;
            v.First().OrderMargin = x.OrderMargin;
            v.First().PositionMargin = x.PositionMargin;
            //v.First().IsolatedOrCross = x.IsolatedOrCross;
        }
        else
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            ObservablePositions.Add(new GeneralPosition()
            {
                Symbol = x.Symbol,
                UpdateTime = x.UpdateTime,
                ExchangeID = exchangeName,
                Position = x.Position,
                AvgCost = x.AvgCost,
                UnrealizedPnl = x.UnrealizedPnl,
                RealizedPnl = x.RealizedPnl,
                LiquidationPrice = x.LiquidationPrice,
                OrderMargin = x.OrderMargin,
                PositionMargin = x.PositionMargin,
                Leverage = x.Leverage,
                IsolatedOrCross = x.IsolatedOrCross
            })));
        }
    }

    private void CallbackUserData(object x, string exchangeName)
    {        
        string disp = string.Empty;
        switch (exchangeName)
        {
            case "Binance":
                {
                    if (x is Balance balance)
                    {
                        var v = ObservableAccounts.Where(z => z.ExchangeID == exchangeName
                        && z.Symbol == balance.Asset);
                        if (v != null)
                        {
                            v.First().Equity = balance.Free + balance.Locked;
                            v.First().Available = balance.Free;
                            v.First().PositionMargin = balance.Locked;
                        }
                    }
                    else
                        if (x is ExecutionReport executionreport)
                    {
                        
                    }
                    else
                        if (x is ListStatus liststatus)
                    {
                        disp = liststatus.ToString();
                    }
                    else
                        if (x is OutboundAccount outbaoundaccount)
                    {
                        disp = outbaoundaccount.ToString();
                    }
                    else
                    {
                        if (x.ToString()== "listenKeyExpired")
                        {

                        }
                    }
                }
                break;
            case "BinanceCOINFuture":
            case "BinanceUSDFuture":
                {
                    if (x is OrderUpdate orderupdate)
                    {/*{Symbol: BNBUSD_211231, OrderType: LIMIT, Side: SELL, OriginalPrice:470.679}*/                       
                        var v = ObservableOrders.Where(z => z.ExchangeID == "Binance"
                               && z.Symbol == orderupdate.order.Symbol && z.OrderId == orderupdate.order.OrderId);
                        if (v.Count() > 0)
                        {
                            v.First().AvgPrice = orderupdate.order.AveragePrice;
                            v.First().UpdateTime = CryptoUtility.UnixTimeStampToDateTimeMilliseconds(orderupdate.EventTime);
                            v.First().LastFilledQty = orderupdate.order.OrderLastFilledQuantity;
                            v.First().OrderStatus = orderupdate.order.OrderStatus;
                            v.First().OrderTradeId = orderupdate.order.OrderTradeId;
                            v.First().LastFilledPrice = orderupdate.order.LastFilledPrice;
                            v.First().FilledQty = orderupdate.order.OrderFilledAcumulatedQuantity;
                            v.First().Commission = orderupdate.order.CommissionOfTrade;
                            v.First().CommissionAsset = orderupdate.order.CommissionAsset;
                            v.First().ExecutionType = orderupdate.order.ExecutionType;
                            v.First().RealizedProfit = orderupdate.order.RealizedProfitOfTheTrade;                           
                            v.First().OrderTradeId = orderupdate.order.OrderTradeId;                            
                            v.First().OrderTime = CryptoUtility.UnixTimeStampToDateTimeMilliseconds(orderupdate.order.OrderTradeTime);
                        }
                        else
                        {
                            Application.Current.Dispatcher.BeginInvoke(new Action(() => ObservableOrders.Add(
                                new ExchangeFutureOrder() {
                                    AvgPrice = orderupdate.order.AveragePrice,
                                    OrderId = orderupdate.order.OrderId,
                                    ClientOrderId = orderupdate.order.ClientOrderId,
                                    Symbol = orderupdate.order.Symbol,
                                    ExchangeID = "Binance",
                                    OrderStatus = orderupdate.order.OrderStatus,
                                    ExecutionType = orderupdate.order.ExecutionType,
                                    OrigQty = orderupdate.order.OriginalQuantity,
                                    LastFilledQty = orderupdate.order.OrderLastFilledQuantity,
                                    UpdateTime = CryptoUtility.UnixTimeStampToDateTimeMilliseconds(orderupdate.EventTime),
                                    LastFilledPrice = orderupdate.order.LastFilledPrice,
                                    OrderType = orderupdate.order.OrderType,
                                    OrderTime = CryptoUtility.UnixTimeStampToDateTimeMilliseconds(orderupdate.order.OrderTradeTime),
                                    CommissionAsset = orderupdate.order.CommissionAsset,                                   
                                    MarginAsset = (orderupdate.order.MarginAsset is null)? "USDT":orderupdate.order.MarginAsset,
                                    TimeInForce = orderupdate.order.TimeInForce,
                                    OrigPrice = orderupdate.order.OriginalPrice,
                                    Side = orderupdate.order.Side, 
                                    RealizedProfit = orderupdate.order.RealizedProfitOfTheTrade,
                                    AccountType = exchangeName.Substring(7),
                                    IsReduceOnly = orderupdate.order.IsReduceOnly,
                                    IsMaker = orderupdate.order.IsMaker,
                                    ACTIVATIONPRICE = orderupdate.order.ACTIVATIONPRICE,
                                    Commission = orderupdate.order.CommissionOfTrade,
                                    OriginalOrderType = orderupdate.order.OriginalOrderType,
                                    OrderTradeId = orderupdate.order.OrderTradeId,
                                    StopPriceType = orderupdate.order.StopPriceType,
                                    StopPrice = orderupdate.order.StopPrice,
                                    FilledQty = orderupdate.order.OrderLastFilledQuantity
                                }
                                )));
                        }
                    }
                    else
                    {
                        if (x is AccountUpdate accountupdate)
                        {
                            foreach (var b in accountupdate.updateData.Balances)
                            {
                                if (b.WalletBalance == 0) continue;

                                var v = ObservableAccounts.Where(z => z.ExchangeID == "Binance"
                                && z.Symbol == b.Asset && z.AccountType == exchangeName.Substring(7));
                                if (v.Count() > 0)
                                {
                                    v.First().Equity = b.WalletBalance;
                                    v.First().Available = b.WalletBalance;
                                    v.First().UpdateTime = CryptoUtility.UnixTimeStampToDateTimeMilliseconds(accountupdate.TransactionTime);
                                }
                                else
                                {
                                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                                   ObservableAccounts.Add(new GeneralAccount()
                                   {
                                       ExchangeID = exchangeName,
                                       AccountType = exchangeName.Substring(7),
                                       Equity = b.WalletBalance,
                                       Available = b.WalletBalance,
                                       Symbol = b.Asset,
                                       UpdateTime = CryptoUtility.UnixTimeStampToDateTimeMilliseconds(accountupdate.TransactionTime),
                                   })));
                                }
                            }

                            foreach (var c in accountupdate.updateData.Positions)
                            {
                                if (c.PositionAmount is null || decimal.Parse(c.PositionAmount)==0) continue;
                                var v = ObservablePositions.Where(z => z.ExchangeID == "Binance" && z.Symbol == c.Symbol);
                                if (v.Count() > 0)
                                {
                                    v.First().UnrealizedPnl = decimal.Parse(c.UnrealizedPnL);
                                    v.First().RealizedPnl = decimal.Parse(c.AccumulatedRealized);
                                    v.First().Position = decimal.Parse(c.PositionAmount);
                                    v.First().AvgCost = decimal.Parse(c.EntryPrice);
                                    v.First().UpdateTime = DateTime.UtcNow;
                                }
                                else
                                {
                                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                                    ObservablePositions.Add(new GeneralPosition()
                                    {
                                        UnrealizedPnl = decimal.Parse(c.UnrealizedPnL),
                                        RealizedPnl = decimal.Parse(c.AccumulatedRealized),
                                        ExchangeID = "Binance",
                                        IsolatedOrCross = c.MarginType == "cross"?false:true,
                                        Symbol = c.Symbol,
                                        Position = decimal.Parse(c.PositionAmount),
                                        AvgCost = decimal.Parse(c.EntryPrice),
                                        UpdateTime = DateTime.UtcNow                                                                               
                                    })));
                                }
                            }
                        }
                        else {
                            if (x is MarginCallUpdate margincall)
                            {
                            }
                            else
                            {
                                if (x is ACCOUNT_CONFIG_UPDATE accountconfig)
                                {

                                }
                                else { 
                                    if (x.ToString()=="listenKeyExpired")
                                    {
                                        var exchangeAPI = ExchangeAPI.GetExchangeAPI(exchangeName);
                                        var listenkey = ((BinanceGroupCommon)exchangeAPI).GetListenKeyAsync();
                                        var aa = ((BinanceGroupCommon)exchangeAPI).GetUserDataWebSocketAsync( x => CallbackUserData(x, exchangeAPI.Name), listenkey.Result);
                                        dictWebSocket[exchangeName] = aa.Result;
                                    }
                                }
                            }
                        }
                    }
                    
                }
                break;
        }       
    }

    private void InitializeAllExchangeApis()
    {
        if (!File.Exists("ExchangeAccountsAPI.json"))
        {
            MessageBox.Show("ExchangeAccountsAPI.json doesn't exist");
            return;        
        }
        string str = File.ReadAllText("ExchangeAccountsAPI.json");        
        Dictapi = JsonConvert.DeserializeObject<Dictionary<string,ExchangeAPICred>>(str);            
    }
}