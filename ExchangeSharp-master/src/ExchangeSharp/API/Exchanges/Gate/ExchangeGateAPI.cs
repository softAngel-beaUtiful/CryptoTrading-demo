/*
MIT LICENSE

Copyright 2017 Digital Ruby, LLC - http://www.digitalruby.com

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ExchangeSharp
{
    
    public sealed partial class ExchangeGateAPI : ExchangeAPI
    {
        public static class APIKeys
        {
            /// <summary>
            /// Key
            /// </summary>
            internal static string KEY { get; set; }
            /// <summary>
            /// 秘钥
            /// </summary>
            internal static string SECRET { get; set; }
        }
		public override string BaseUrl { get; set; } = "https://data.gateapi.io/api2/1";
        public string BaseUrlPrivate { get; set; } = "https://api.gateio.co/api2/1/private";
        public override string BaseUrlWebSocket { get; set; } = "wss://ws.gate.io/v3/";
        //public override EExchangeType ExchangeType => EExchangeType.Spot;
        //public override string ExchangeName => Exchanges.Gate;
        //public override string[] Symbols { get; set; }               
        //public override string RequestContentType { get; set; } = "text/plain";     
        
        const string QUERY_URL = "https://data.gate.io/";
        const string TRADE_URL = "https://api.gate.io/";
        const string PAIRS_URL = "api2/1/pairs";
        const string MARKET_INFO_URL = "api2/1/marketinfo";
        const string MARKET_LIST_URL = "api2/1/marketlist";
        const string TICKETS_URL = "api2/1/tickers";
        const string TICKER_URL = "api2/1/ticker";
        const string ORDER_BOOKS_URL = "api2/1/orderBooks";
        const string ORDER_BOOK_URL = "api2/1/orderBook";
        const string TRADE_HISTORY_URL = "api2/1/tradeHistory";
        const string BALANCE_URL = "api2/1/private/balances";
        const string DEPOSIT_ADDRESS_URL = "api2/1/private/depositAddress";
        const string DEPOSITS_WITHDRAWALS_URL = "api2/1/private/depositsWithdrawals";
        const string BUY_URL = "api2/1/private/buy";
        const string SELL_URL = "api2/1/private/sell";
        const string CANCEL_ORDER_URL = "api2/1/private/cancelOrder";
        const string CANCEL_ORDERS_URL = "api2/1/private/cancelOrders";
        const string CANCEL_ALL_ORDERS_URL = "api2/1/private/cancelAllOrders";
        const string GET_ORDER_URL = "api2/1/private/getOrder";
        const string OPEN_ORDERS_URL = "api2/1/private/openOrders";
        const string MY_TRADE_HISTORY_URL = "api2/1/private/tradeHistory";
        const string WITHDRAW_URL = "api2/1/private/withdraw";

        public ExchangeGateAPI()
        {
            MarketSymbolSeparator = "_";
            MarketSymbolIsUppercase = false;
            WebSocketOrderBookType = WebSocketOrderBookType.FullBookFirstThenDeltas;
        }
        public static void SetKey(string key, string secret)
        {
            APIKeys.KEY = key;
            APIKeys.SECRET = secret;
        }
        private string NormalizeSymbolWebsocket(string symbol)
        {
            if (symbol == null) return symbol;

            return (symbol ?? string.Empty).ToLowerInvariant().Replace(MarketSymbolSeparator, string.Empty);
        }
        #region publicAPI

        private async Task<Tuple<JToken, string>> MakeRequestGateAsync(string marketSymbol, string subUrl, string baseUrl = null)
        {
            JToken obj = await MakeJsonRequestAsync<JToken>(subUrl.Replace("$SYMBOL$", marketSymbol ?? string.Empty), baseUrl);
            return new Tuple<JToken, string>(obj, marketSymbol);
        }

        private async Task<ExchangeTicker> ParseTickerAsync(string symbol, JToken data)
        {           
            ExchangeTicker ticker = await this.ParseTickerAsync(data["ticker"], symbol, "sell", "buy", "last", "vol");
            ticker.Volume.Timestamp = CryptoUtility.UnixTimeStampToDateTimeMilliseconds(data["date"].ConvertInvariant<long>());
            return ticker;
        }

        private async Task<ExchangeTicker> ParseTickerV2Async(string symbol, JToken data)
        {           
            return await this.ParseTickerAsync(data.First, symbol, "sell", "buy", "last", "vol");
        }

        protected override async Task<IEnumerable<string>> OnGetMarketSymbolsAsync()
        {
            var data = await MakeRequestGateAsync(string.Empty, "/markets");
            List<string> symbols = new List<string>();
            foreach (JProperty prop in data.Item1)
            {
                symbols.Add(prop.Name);
            }
            return symbols;
        }

		protected internal override async Task<IEnumerable<ExchangeMarket>> OnGetMarketSymbolsMetadataAsync()
		{			
			var data = await MakeRequestGateAsync(string.Empty, "/markets");
			List<ExchangeMarket> symbols = new List<ExchangeMarket>();
			foreach (JProperty prop in data.Item1)
			{
				var split = prop.Name.Split('_');
				var priceScale = prop.First.Value<Int16>("priceScale");
				var priceScaleDecimals = (decimal)Math.Pow(0.1, priceScale);
				var amountScale = prop.First.Value<Int16>("amountScale");
				var amountScaleDecimals = (decimal)Math.Pow(0.1, amountScale);
				symbols.Add(new ExchangeMarket()
				{
					IsActive = true, // ZB.com does not provide this info, assuming all are active
					MarketSymbol = prop.Name,
					BaseCurrency = split[0],
					QuoteCurrency = split[1],
					MinPrice = priceScaleDecimals,
					PriceStepSize = priceScaleDecimals,
					MinTradeSize = amountScaleDecimals,
					QuantityStepSize = amountScaleDecimals,
				});
			}
			return symbols;
		}
		protected override async Task<ExchangeOrderBook> OnGetOrderBookAsync(string symbol, int num=20)
		{
			var data = await MakeRequestGateAsync(symbol, "orderBook/$SYMBOL$");
			return ParseOrderBook(data);
		}

		private ExchangeOrderBook ParseOrderBook(Tuple<JToken, string> data)
		{/*+		data.Item1["bids"]	{[ [   "0.9993",   "284.7247" ], [ "0.9992", "687.3692" ], [ "0.9991", "1360.23" ], [  "0.9985",
    "26381.6224" ], [ "0.9984", "481.08469"],[ "0.9981","189.89933974" ], [ "0.998", "104.1924"],["0.9976","5617.518751585" ], [ "0.9974", "406.406"
  ], [
    "0.9972",
    "5260.147941"
  ],
  [
    "0.9971",
    "501.6246"
  ],
  [
    "0.9969",
    "1563.7356"
  ],
  [
    "0.9968",
    "4979.3845"
  ],
  [
    "0.9962",
    "1.0411"
  ],
  [
    "0.996",
    "2.01034136"
  ],
  [
    "0.9952",
    "6.6666"
  ],
  [
    "0.995",
    "6"
  ],
  [
    "0.9941",
    "1.0917"
  ],
  [
    "0.994",
    "2.03521126"
  ],
  [
    "0.9931",
    "9.6926"
  ],
  [
    "0.9921",
    "1.0411"
  ],
  [
    "0.992",
    "2.03931451"
  ],
  [
    "0.991",
    "6.6666"
  ],
  [
    "0.9908",
    "6"
  ],
  [
    "0.99",
    "21.58673434"
  ],
  [
    "0.9898",
    "1.0917"
  ],
  [
    "0.9892",
    "2203.0414"
  ],
  [
    "0.989",
    "9.6926"
  ],
  [
    "0.9881",
    "1.0411"
  ],
  [
    "0.9867",
    "6.6666"
  ],
  [
    "0.9866",
    "6"
  ],
  [
    "0.9855",
    "1.0917"
  ],
  [
    "0.9849",
    "2.5628"
  ],
  [
    "0.9848",
    "7.1298"
  ],
  [
    "0.9842",
    "1.0411"
  ],
  [
    "0.9825",
    "6.6666"
  ],
  [
    "0.9824",
    "6"
  ],
  [
    "0.9812",
    "1.0917"
  ],
  [
    "0.9807",
    "9.6926"
  ],
  [
    "0.9802",
    "1.0411"
  ],
  [
    "0.98",
    "19.5433"
  ],
  [
    "0.9783",
    "448.75610391"
  ],
  [
    "0.9782",
    "12.6666"
  ],
  [
    "0.9769",
    "1.0917"
  ],
  [
    "0.9766",
    "9.6926"
  ],
  [
    "0.9762",
    "1.0411"
  ],
  [
    "0.9741",
    "6"
  ],
  [
    "0.9739",
    "6.6666"
  ],
  [
    "0.9727",
    "1.0917"
  ],
  [
    "0.9725",
    "9.6926"
  ],
  [
    "0.9723",
    "1.0411"
  ],
  [
    "0.97",
    "19.5433"
  ],
  [
    "0.9699",
    "6"
  ],
  [
    "0.9697",
    "6.6666"
  ],
  [
    "0.9684",
    "10.7337"
  ],
  [
    "0.9658",
    "6"
  ],
  [
    "0.9654",
    "6.6666"
  ],
  [
    "0.9644",
    "1.0411"
  ],
  [
    "0.9643",
    "2.5628"
  ],
  [
    "0.9617",
    "6"
  ],
  [
    "0.9612",
    "6.6666"
  ],
  [
    "0.9606",
    "2.0821"
  ],
  [
    "0.9577",
    "6"
  ],
  [
    "0.5991",
    "39.1623"
  ],
  [
    "0.509",
    "1.9646"
  ],
  [
    "0.49",
    "12.5625"
  ],
  [
    "0.3296",
    "199.95"
  ],
  [
    "0.3295",
    "3186.279"
  ],
  [
    "0.1085",
    "9.2165"
  ],
  [
    "0.0999",
    "53.4667"
  ],
  [
    "0.0998",
    "100.2004"
  ],
  [
    "0.0853",
    "11.7233"
  ],
  [
    "0.0532",
    "18.7969"
  ],
  [
    "0.049",
    "125.625"
  ],
  [
    "0.032",
    "31.25"
  ],
  [
    "0.025",
    "80"
  ],
  [
    "0.0206",
    "48.5436"
  ],
  [
    "0.0106",
    "94.3396"
  ],
  [
    "0.009",
    "222.2222"
  ],
  [
    "0.007",
    "285.7142"
  ],
  [
    "0.0063",
    "158.7301"
  ],
  [
    "0.005",
    "400"
  ],
  [
    "0.0032",
    "312.5"
  ],
  [
    "0.0025",
    "1200"
  ],
  [
    "0.002",
    "500"
  ],
  [
    "0.0016",
    "625"
  ],
  [
    "0.0012",
    "833.3333"
  ],
  [
    "0.001",
    "4000"
  ],
  [
    "0.0009",
    "2222.2222"
  ],
  [
    "0.0008",
    "1250"
  ],
  [
    "0.0007",
    "2857.1428"
  ],
  [
    "0.0005",
    "6000"
  ],
  [
    "0.0003",
    "9999.9997"
  ],
  [
    "0.0002",
    "22500"
  ],
  [
    "0.0001",
    "10000"
  ]
]}	Newtonsoft.Json.Linq.JToken {Newtonsoft.Json.Linq.JArray}
*/
			ExchangeOrderBook pair = new ExchangeOrderBook();
			pair.MarketSymbol = data.Item2.ToStringInvariant();
			//pair.Bids = new SortedDictionary<decimal, ExchangeOrderPrice>();
			foreach (var b in data.Item1["asks"].ToArray())
			{
				decimal p = b.ToArray()[0].ConvertInvariant<decimal>();
				decimal a = b.ToArray()[1].ConvertInvariant<decimal>();
				pair.Bids.Add(p, new ExchangeOrderPrice { Price = p, Amount = a });
			}
			//pair.Asks = new SortedDictionary<decimal, ExchangeOrderPrice>();
			foreach (var a in data.Item1["bids"].ToArray())
			{
				decimal p = a.ToArray()[0].ConvertInvariant<decimal>();
				decimal s = a.ToArray()[1].ConvertInvariant<decimal>();
				pair.Asks.Add(p, new ExchangeOrderPrice { Price = p, Amount = s });
			}
			return pair;
		}

		protected override async Task<ExchangeTicker> OnGetTickerAsync(string marketSymbol)
        {
            var data = await MakeRequestGateAsync(marketSymbol, "/ticker?market=$SYMBOL$");
            return await ParseTickerAsync(data.Item2, data.Item1);
        }

		protected override async Task<IEnumerable<KeyValuePair<string, ExchangeTicker>>> OnGetTickersAsync()
		{
			var data = await MakeRequestGateAsync(null, "/tickers", BaseUrl);
			if (data.Item1["code"].ConvertInvariant<decimal>() == 1)
				throw (new Exception("invalid data"));
			List<KeyValuePair<string, ExchangeTicker>> tickers = new List<KeyValuePair<string, ExchangeTicker>>();
			var symbolLookup = await Cache.GetOrCreate(nameof(GetMarketSymbolsAsync) + "_Set", async () =>
			{
				IEnumerable<string> symbols = await GetMarketSymbolsAsync();
				Dictionary<string, string> lookup = symbols.ToDictionary((symbol) => symbol.Replace(MarketSymbolSeparator, string.Empty));
				if (lookup.Count == 0)
				{
					return new CachedItem<Dictionary<string, string>>();
				}
				return new CachedItem<Dictionary<string, string>>(lookup, CryptoUtility.UtcNow.AddHours(4.0));
			});
			if (!symbolLookup.Found)
			{
				throw new APIException("Unable to get symbols for exchange " + Name);
			}

			foreach (JToken token in data.Item1)
			{
				//KeyValuePair<string, List<KeyValuePair<string, object>>> keyValuePair = token.ConvertInvariant<KeyValuePair<string, List<KeyValuePair<string, object>>>>();//; // as KeyValuePair<string, JToken>; 
				//tickers.Add(new KeyValuePair<string, ExchangeTicker>(keyValuePair.Key,l, await ParseTickerV2Async(marketSymbol, token)));
				try
				{
					ExchangeTicker ticker = new ExchangeTicker()
					{
						MarketSymbol = token.First().Path,
						Ask = decimal.TryParse(token.First["highestBid"].ToString(), out decimal a)?a:0,
						//Convert.ToDecimal(token.First["highestBid"]),
						Bid = decimal.TryParse(token.First["lowestAsk"].ToString(), out decimal b)?b:0,
						Last = decimal.TryParse(token.First["last"].ToString(), out decimal l)?l:0,
						Volume = new ExchangeVolume
						{
							BaseCurrencyVolume = decimal.TryParse(token.First["baseVolume"].ToString(), out decimal bv)? bv:0,
							QuoteCurrencyVolume = decimal.TryParse(token.First["quoteVolume"].ToString(), out decimal qv)?qv:0
						}
					};
					tickers.Add(new KeyValuePair<string, ExchangeTicker>(ticker.MarketSymbol, ticker));
				}
				catch (FormatException e)
				{ }
				catch (Exception ee)
				{ }
			}
			return tickers;
		}

		protected override async Task<IWebSocket> OnGetTradesWebSocketAsync(Func<KeyValuePair<string, ExchangeTrade>, Task> callback, params string[] marketSymbols)
        {
			if (marketSymbols == null || marketSymbols.Length == 0)
			{
				marketSymbols = (await GetMarketSymbolsAsync()).ToArray();
			}
			return await ConnectWebSocketAsync(string.Empty, async (_socket, msg) =>
            {
                JToken token = JToken.Parse(msg.ToStringFromUTF8());
                if (token["dataType"].ToStringInvariant() == "trades")
                {
                    var channel = token["channel"].ToStringInvariant();
                    var sArray = channel.Split('_');
                    string marketSymbol = sArray[0];
                    var data = token["data"];
                    var trades = ParseTradesWebsocket(data);
                    foreach (var trade in trades)
                    {
                        await callback(new KeyValuePair<string, ExchangeTrade>(marketSymbol, trade));
                    }
                }
            }, async (_socket) =>
            {
                foreach (var marketSymbol in marketSymbols)
                {
                    string normalizedSymbol = NormalizeSymbolWebsocket(marketSymbol);
                    await _socket.SendMessageAsync(new { @event = "addChannel", channel = normalizedSymbol + "_trades" });
                }
            });
        }

        protected override async Task<IWebSocket> OnGetTickersWebSocketAsync(Action<IReadOnlyCollection<KeyValuePair<string, ExchangeTicker>>> callback, params string[] symbols)
        {
            if (symbols == null || symbols.Length == 0)
            {
                symbols = (await GetMarketSymbolsAsync()).ToArray();
            }
            return await ConnectWebSocketAsync(string.Empty, async (_socket, msg) =>
            {               
                JToken token = JToken.Parse(msg.ToStringFromUTF8());
                if (token["method"].ToStringInvariant() == "ticker.update")
                {
                    var jt = token["params"];
                    var symbol = jt.First().ToStringInvariant(); ;
                    var tickertoken = jt.Last();
                    List<KeyValuePair<string, ExchangeTicker>> tickers = ParseTickerWebsocket(tickertoken, symbol);                  
                    callback(tickers);
                }
            }, async (_socket) =>
            {                
                int id = 12312;
                await _socket.SendMessageAsync(new { id, method = "ticker.subscribe", @params = symbols });
            });
        }

        /*protected override async Task<bool> OnUnsubscribeTickersWebSocketAsync(IWebSocket _socket, params string[] symbols)
        {
            if (symbols ==null || symbols.Length ==0)
            {
                symbols = (await GetMarketSymbolsAsync()).ToArray();
            }
            int id = 12312;
            List<object[]> ps = new List<object[]>();
            foreach (var dd in symbols)
            {
                ps.Add(new object[] { dd, 5, "0" });
            }
            var a1 = new { id, method = "ticker.unsubscribe", @params = ps };
            return await _socket.SendMessageAsync(a1);
        }*/
        protected override async Task<IWebSocket> OnGetDeltaOrderBookWebSocketAsync(Action<ExchangeOrderBook> callback, int maxCount = 20, params string[] symbols)
        {
            if (symbols == null || symbols.Length == 0)
            {
                symbols = (await GetMarketSymbolsAsync()).ToArray();
            }
            return await ConnectWebSocketAsync(string.Empty, async (_socket, msg) =>
            {               
                JToken token = JToken.Parse(msg.ToStringFromUTF8());
                if (token["method"].ToStringInvariant() == "depth.update")
                {
                    var jt = token["params"];
                    var symbol = jt.Last().ToStringInvariant(); ;
                    var orderbook = jt[1];
                    ExchangeOrderBook ticker = ParseOrderBookWebsocket(orderbook, symbol);
                    callback(ticker);
                }
            }, async (_socket) =>
            {
                int id = 12312;
                List<object[]> ps= new List<object[]>();
                foreach(var symbol in symbols)
                {
                    ps.Add(new object[] { symbol.ToUpper(), 5, "0.000001"});
                }
                var a1 = new { id, method = "depth.subscribe", @params = ps };
                await _socket.SendMessageAsync(a1);
            });
        }

        private ExchangeOrderBook ParseOrderBookWebsocket(JToken t, string symbol)
        {
			ExchangeOrderBook orderBook = new ExchangeOrderBook();
			//SortedDictionary<decimal, ExchangeOrderPrice> bids = new SortedDictionary<decimal, ExchangeOrderPrice>(), 
            //    asks = new SortedDictionary<decimal, ExchangeOrderPrice>();
            if (t["bids"] != null && t["bids"].HasValues)
            {
                foreach (var v1 in t["bids"])
                {
					decimal AMOUNT = v1[1].ConvertInvariant<decimal>();
					//if ((AMOUNT = v1[1].ConvertInvariant<decimal>()) == 0)
					//	continue;
                    orderBook.Bids[v1[0].ConvertInvariant<decimal>()]=
                        new ExchangeOrderPrice() { Amount = AMOUNT, Price = v1[0].ConvertInvariant<decimal>() };
                }
            }
            if (t["asks"] !=null && t["asks"].HasValues)
            {
                foreach (var v2 in t["asks"])
                {
					decimal AMOUNT = v2[1].ConvertInvariant<decimal>();
					//if ((AMOUNT = v2[1].ConvertInvariant<decimal>()) == 0)
					//	continue;
					orderBook.Asks[v2[0].ConvertInvariant<decimal>()] =
                        new ExchangeOrderPrice() { Amount = AMOUNT, Price = v2[0].ConvertInvariant<decimal>() };
                }
            }
			orderBook.MarketSymbol = symbol;
			orderBook.LastUpdatedUtc = CryptoUtility.UtcNow; //DateTime.UtcNow;            
            return orderBook;
        }

        private List<KeyValuePair<string, ExchangeTicker>> ParseTickerWebsocket(JToken data, string symbol)
        {
            ExchangeTicker ticker = new ExchangeTicker() {                 
                Last = data["last"].ConvertInvariant<decimal>(),
                Volume = new ExchangeVolume() {
                Timestamp = DateTime.UtcNow,
                BaseCurrencyVolume = data["baseVolume"].ConvertInvariant<decimal>(),
                QuoteCurrencyVolume = data["quoteVolume"].ConvertInvariant<decimal>()
                },
                MarketSymbol = symbol
            };
            List<KeyValuePair<string, ExchangeTicker>> returnvalue = new List<KeyValuePair<string, ExchangeTicker>>();
            returnvalue.Add(new KeyValuePair<string, ExchangeTicker>(ticker.MarketSymbol, ticker));
            return returnvalue;
        }

        private IEnumerable<ExchangeTrade> ParseTradesWebsocket(JToken token)
        {
            //{ "amount":"0.0372","price": "7509.7","tid": 153806522,"date": 1532103901,"type": "sell","trade_type": "ask"},{"amount": "0.0076", ...
            var trades = new List<ExchangeTrade>();
            foreach (var t in token)
            {
                trades.Add(t.ParseTrade("amount", "price", "type", "date", TimestampType.UnixSeconds, "tid"));
            }
            return trades;
        }
    
        private ExchangeOrderBook ParseDepth(JToken token, string marketSymbol, DateTime date)
        {
            ExchangeOrderBook orderBook = new ExchangeOrderBook()
            { 
                LastUpdatedUtc = date,
                MarketSymbol = marketSymbol,
                
            };
            foreach (var ask in token["asks"])
            {
                var v1 = ask.First().ConvertInvariant<decimal>();
                var v2 = ask.Last().ConvertInvariant<decimal>();
                orderBook.Asks.Add(v1, new ExchangeOrderPrice() { Amount = v2, Price = v1 });
            }
            foreach (var bid in token["bids"])
            {
                var v1 = bid.First().ConvertInvariant<decimal>();
                var v2 = bid.Last().ConvertInvariant<decimal>();
                orderBook.Bids.Add(v1, new ExchangeOrderPrice() { Amount = v2, Price = v1 });
            }            
            return orderBook;
        }
        #endregion

        #region Error processing

        private string StatusToError(string status)
        {
            switch (status)
            {
                case "1000": return "Success";
                case "1001": return "Error Tips";
                case "1002": return "Internal Error";
                case "1003": return "Validate No Pass";
                case "1004": return "Transaction Password Locked";
                case "1005": return "Transaction Password Error";
                case "1006": return "Real - name verification is pending approval or not approval";
                case "1009": return "This interface is in maintaining";
                case "1010": return "Not open yet";
                case "1012": return "Permission denied.";
                case "2001": return "Insufficient CNY Balance";
                case "2002": return "Insufficient BTC Balance";
                case "2003": return "Insufficient LTC Balance";
                case "2005": return "Insufficient ETH Balance";
                case "2006": return "Insufficient ETC Balance";
                case "2007": return "Insufficient BTS Balance";
                case "2009": return "Insufficient account balance";
                case "3001": return "Not Found Order";
                case "3002": return "Invalid Money";
                case "3003": return "Invalid Amount";
                case "3004": return "No Such User";
                case "3005": return "Invalid Parameters";
                case "3006": return "Invalid IP or Differ From the Bound IP";
                case "3007": return "Invalid Request Time";
                case "3008": return "Not Found Transaction Record";
                case "4001": return "API Interface is locked or not enabled";
                case "4002": return "Request Too Frequently";

                default: return status;
            }
        }

        #endregion
    }
    public partial class ExchangeName { public const string Gate = "Gate"; }
}
