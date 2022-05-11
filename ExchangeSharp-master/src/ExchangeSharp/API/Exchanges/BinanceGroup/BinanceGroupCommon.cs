/*
MIT LICENSE

Copyright 2017 Digital Ruby, LLC - http://www.digitalruby.com

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#nullable enable
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeSharp.BinanceGroup
{
	public abstract class BinanceGroupCommon : ExchangeAPI
	{
		public abstract string BaseUrlPrivate { get; set; }
		public abstract string WithdrawalUrlPrivate { get; set; }
		/// <summary>
		/// base address for APIs used by the Binance website and not published in the API docs
		/// </summary>
		public abstract string BaseWebUrl { get; set; }

		public const string GetCurrenciesUrl = "/assetWithdraw/getAllAsset.html";

		protected async Task<string> GetWebSocketStreamUrlForSymbolsAsync(string suffix, params string[] marketSymbols)
		{
			if (marketSymbols == null || marketSymbols.Length == 0)
			{
				marketSymbols = (await GetMarketSymbolsAsync()).ToArray();
			}

			StringBuilder streams = new StringBuilder("/stream?streams=");
			for (int i = 0; i < marketSymbols.Length; i++)
			{
				string marketSymbol = NormalizeMarketSymbol(marketSymbols[i]).ToLowerInvariant();
				streams.Append(marketSymbol);
				streams.Append(suffix);
				streams.Append('/');
			}
			streams.Length--; // remove last /

			return streams.ToString();
		}

		protected BinanceGroupCommon()
		{
			// give binance plenty of room to accept requests
			RequestWindow = TimeSpan.FromMilliseconds(60000);  // 60000 is max value = max request time window of 60 seconds
			NonceStyle = NonceStyle.UnixMilliseconds;
			NonceOffset = TimeSpan.FromSeconds(15); // 15 seconds are deducted from current UTCTime as base of the request time window
			MarketSymbolSeparator = "";
			WebSocketOrderBookType = WebSocketOrderBookType.DeltasOnly;
			ExchangeGlobalCurrencyReplacements["BCC"] = "BCH";
		}

		public override Task<string> ExchangeMarketSymbolToGlobalMarketSymbolAsync(string marketSymbol)
		{
			// All pairs in Binance end with BTC, ETH, BNB or USDT
			if (marketSymbol.EndsWith("BTC") || marketSymbol.EndsWith("ETH") || marketSymbol.EndsWith("BNB"))
			{
				string baseSymbol = marketSymbol.Substring(marketSymbol.Length - 3);
				return ExchangeMarketSymbolToGlobalMarketSymbolWithSeparatorAsync((marketSymbol.Replace(baseSymbol, "") + GlobalMarketSymbolSeparator + baseSymbol), GlobalMarketSymbolSeparator);
			}
			if (marketSymbol.EndsWith("USDT"))
			{
				string baseSymbol = marketSymbol.Substring(marketSymbol.Length - 4);
				return ExchangeMarketSymbolToGlobalMarketSymbolWithSeparatorAsync((marketSymbol.Replace(baseSymbol, "") + GlobalMarketSymbolSeparator + baseSymbol), GlobalMarketSymbolSeparator);
			}
			return ExchangeMarketSymbolToGlobalMarketSymbolWithSeparatorAsync(marketSymbol.Substring(0, marketSymbol.Length - 3) + GlobalMarketSymbolSeparator + (marketSymbol.Substring(marketSymbol.Length - 3, 3)), GlobalMarketSymbolSeparator);
		}

		/// <summary>
		/// Get the details of all trades
		/// </summary>
		/// <param name="marketSymbol">Symbol to get trades for or null for all</param>
		/// <returns>All trades for the specified symbol, or all if null symbol</returns>
		public async Task<IEnumerable<ExchangeOrderResult>> GetMyTradesAsync(string? marketSymbol = null, DateTime? afterDate = null)
		{
			await new SynchronizationContextRemover();
			return await OnGetMyTradesAsync(marketSymbol, afterDate);
		}

		protected override async Task<IEnumerable<string>> OnGetMarketSymbolsAsync()
		{
			List<string> symbols = new List<string>();
			JToken? obj = await MakeJsonRequestAsync<JToken>("/ticker/price");
			if (!(obj is null))
			{
				foreach (JToken token in obj)
				{
					symbols.Add(token["symbol"].ToStringInvariant());
				}
			}
			return symbols;
		}

		protected internal override async Task<IEnumerable<ExchangeMarket>> OnGetMarketSymbolsMetadataAsync()
		{
			/*
             *         {
                "symbol": "ETHBTC",
                "status": "TRADING",
                "baseAsset": "ETH",
                "baseAssetPrecision": 8,
                "quoteAsset": "BTC",
                "quotePrecision": 8,
                "orderTypes": [
                    "LIMIT",
                    "MARKET",
                    "STOP_LOSS",
                    "STOP_LOSS_LIMIT",
                    "TAKE_PROFIT",
                    "TAKE_PROFIT_LIMIT",
                    "LIMIT_MAKER"
                ],
                "icebergAllowed": false,
                "filters": [
                    {
                        "filterType": "PRICE_FILTER",
                        "minPrice": "0.00000100",
                        "maxPrice": "100000.00000000",
                        "tickSize": "0.00000100"
                    },
                    {
                        "filterType": "LOT_SIZE",
                        "minQty": "0.00100000",
                        "maxQty": "100000.00000000",
                        "stepSize": "0.00100000"
                    },
                    {
                        "filterType": "MIN_NOTIONAL",
                        "minNotional": "0.00100000"
                    }
                ]
        },
             */

			var markets = new List<ExchangeMarket>();
			JToken obj = await MakeJsonRequestAsync<JToken>("/exchangeInfo");
			JToken allSymbols = obj["symbols"];
			foreach (JToken marketSymbolToken in allSymbols)
			{
				var market = new ExchangeMarket
				{
					MarketSymbol = marketSymbolToken["symbol"].ToStringUpperInvariant(),
					IsActive = ParseMarketStatus(marketSymbolToken["status"].ToStringUpperInvariant()),
					QuoteCurrency = marketSymbolToken["quoteAsset"].ToStringUpperInvariant(),
					BaseCurrency = marketSymbolToken["baseAsset"].ToStringUpperInvariant()
				};

				// "LOT_SIZE"
				JToken filters = marketSymbolToken["filters"];
				JToken? lotSizeFilter = filters?.FirstOrDefault(x => string.Equals(x["filterType"].ToStringUpperInvariant(), "LOT_SIZE"));
				if (lotSizeFilter != null)
				{
					market.MaxTradeSize = lotSizeFilter["maxQty"].ConvertInvariant<decimal>();
					market.MinTradeSize = lotSizeFilter["minQty"].ConvertInvariant<decimal>();
					market.QuantityStepSize = lotSizeFilter["stepSize"].ConvertInvariant<decimal>();
				}

				// PRICE_FILTER
				JToken? priceFilter = filters?.FirstOrDefault(x => string.Equals(x["filterType"].ToStringUpperInvariant(), "PRICE_FILTER"));
				if (priceFilter != null)
				{
					market.MaxPrice = priceFilter["maxPrice"].ConvertInvariant<decimal>();
					market.MinPrice = priceFilter["minPrice"].ConvertInvariant<decimal>();
					market.PriceStepSize = priceFilter["tickSize"].ConvertInvariant<decimal>();
				}

				// MIN_NOTIONAL
				JToken? minNotionalFilter = filters?.FirstOrDefault(x => string.Equals(x["filterType"].ToStringUpperInvariant(), "MIN_NOTIONAL"));
				if (minNotionalFilter != null)
				{
					market.MinTradeSizeInQuoteCurrency = minNotionalFilter["minNotional"].ConvertInvariant<decimal>();
				}
				markets.Add(market);
			}

			return markets;
		}

		protected override async Task<IReadOnlyDictionary<string, ExchangeCurrency>> OnGetCurrenciesAsync()
		{
			// https://www.binance.com/assetWithdraw/getAllAsset.html
			Dictionary<string, ExchangeCurrency> allCoins = new Dictionary<string, ExchangeCurrency>(StringComparer.OrdinalIgnoreCase);

			List<Currency> currencies = await MakeJsonRequestAsync<List<Currency>>(GetCurrenciesUrl, BaseWebUrl);
			foreach (Currency coin in currencies)
			{
				allCoins[coin.AssetCode] = new ExchangeCurrency
				{
					CoinType = coin.ParentCode,
					DepositEnabled = coin.EnableCharge,
					FullName = coin.AssetName,
					MinConfirmations = coin.ConfirmTimes.ConvertInvariant<int>(),
					Name = coin.AssetCode,
					TxFee = coin.TransactionFee,
					WithdrawalEnabled = coin.EnableWithdraw,
					MinWithdrawalSize = coin.MinProductWithdraw.ConvertInvariant<decimal>(),
				};
			}

			return allCoins;
		}

		protected override async Task<ExchangeTicker> OnGetTickerAsync(string marketSymbol)
		{
			JToken obj = await MakeJsonRequestAsync<JToken>("/ticker/24hr?symbol=" + marketSymbol);
			return await ParseTickerAsync(marketSymbol, obj);
		}

		protected override async Task<IEnumerable<KeyValuePair<string, ExchangeTicker>>> OnGetTickersAsync()
		{
			List<KeyValuePair<string, ExchangeTicker>> tickers = new List<KeyValuePair<string, ExchangeTicker>>();
			string marketSymbol;
			JToken obj = await MakeJsonRequestAsync<JToken>("/ticker/24hr");
			foreach (JToken child in obj)
			{
				try
				{
					marketSymbol = child["symbol"].ToStringInvariant();
					tickers.Add(new KeyValuePair<string, ExchangeTicker>(marketSymbol, await ParseTickerAsync(marketSymbol, child)));
				}
				catch (Exception ex)
				{
					continue;
				}
			}
			return tickers;
		}
		public override string NormalizeMarketSymbol(string? marketSymbol)
		{
			if (marketSymbol is null) return "";
			return marketSymbol;
		}

		protected override Task<IWebSocket> OnGetTickersWebSocketAsync(Action<IReadOnlyCollection<KeyValuePair<string, ExchangeTicker>>> callback, params string[] marketSymbols)
		{
			string str = string.Empty;
			foreach (var v in marketSymbols)
			{
				str += v.ToLower() + "@ticker/";
			}
			var str1 = str.Substring(0, str.Length - 1);
			string str2 = BaseUrlWebSocket+ "/stream?streams=" + str1;

			return ConnectWebSocketAsync(str2, async (_socket, msg) =>
			{
				JToken token = JToken.Parse(msg.ToStringFromUTF8());
				List<KeyValuePair<string, ExchangeTicker>> tickerList = new List<KeyValuePair<string, ExchangeTicker>>();
				ExchangeTicker ticker;
				/*	{ e": "24hrTicker",  "E": 1585769182124,  "s": "BTCUSDT",  "p": "-277.62000000",  "P": "-4.307",  "w": "6294.82185042",  "x": "6447.75000000",
  "c": "6168.71000000",  "Q": "0.01499200",  "b": "6168.25000000",  "B": "0.00810400",  "a": "6168.71000000",  "A": "2.17082400",  "o": "6446.33000000",
  "h": "6494.90000000",  "l": "6150.11000000",  "v": "70722.28920800",  "q": "445184211.41823310",  "O": 1585682782094,  "C": 1585769182094,  "F": 283501871,  "L": 284085419,
  "n": 583549   }*/
				var datatoken = token["data"];
				if (datatoken is null || !datatoken.HasValues) return;
				ticker = await ParseTickerWebSocketAsync(datatoken);
				tickerList.Add(new KeyValuePair<string, ExchangeTicker>(ticker.MarketSymbol, ticker));
				callback(tickerList);				
			});
		}		

		protected override async Task<IWebSocket> OnGetTradesWebSocketAsync(Func<KeyValuePair<string, ExchangeTrade>, Task> callback, params string[] marketSymbols)
		{
			/*
	    {
	      "e": "aggTrade",  // Event type
	      "E": 123456789,   // Event time
	      "s": "BNBBTC",    // Symbol
	      "a": 12345,       // Aggregate trade ID
	      "p": "0.001",     // Price
	      "q": "100",       // Quantity
	      "f": 100,         // First trade ID
	      "l": 105,         // Last trade ID
	      "T": 123456785,   // Trade time
	      "m": true,        // Is the buyer the market maker?
	      "M": true         // Ignore
	    }
            */

			if (marketSymbols == null || marketSymbols.Length == 0)
			{
				marketSymbols = (await GetMarketSymbolsAsync()).ToArray();
			}
			string url = await GetWebSocketStreamUrlForSymbolsAsync("@aggTrade", marketSymbols);
			return await ConnectWebSocketAsync(url, messageCallback: async (_socket, msg) =>
			{
				JToken token = JToken.Parse(msg.ToStringFromUTF8());
				string name = token["stream"].ToStringInvariant();
				token = token["data"];
				string marketSymbol = NormalizeMarketSymbol(name.Substring(0, name.IndexOf('@')));

				// buy=0 -> m = true (The buyer is maker, while the seller is taker).
				// buy=1 -> m = false(The seller is maker, while the buyer is taker).
				await callback(new KeyValuePair<string, ExchangeTrade>(marketSymbol,
					token.ParseTradeBinance(amountKey: "q", priceKey: "p", typeKey: "m",
						timestampKey: "T", // use trade time (T) instead of event time (E)
						timestampType: TimestampType.UnixMilliseconds, idKey: "a", typeKeyIsBuyValue: "false")));
			});
		}

		protected override async Task<IWebSocket> OnGetDeltaOrderBookWebSocketAsync(Action<ExchangeOrderBook> callback, int maxCount = 20, params string[] marketSymbols)
		{
			if (marketSymbols == null || marketSymbols.Length == 0)
			{
				marketSymbols = (await GetMarketSymbolsAsync()).ToArray();
			}
			string combined = string.Join("/", marketSymbols.Select(s => s.ToLowerInvariant() + "@depth@100ms"));
			return await ConnectWebSocketAsync($"/stream?streams={combined}", (_socket, msg) =>
			{/*{"stream":"btcusdt@depth20@100ms","data":{"lastUpdateId":11655824136,
			  * "bids":[["35496.06000000","0.29859300"],["35494.02000000","0.00056300"],["35492.76000000","0.29281400"],["35492.74000000","0.33317500"],["35492.69000000","0.24197100"],["35490.13000000","0.31000000"],["35488.55000000","0.11438100"],["35487.72000000","0.43845000"],["35487.71000000","2.32424100"],["35487.70000000","0.00400000"],["35487.69000000","0.30708800"],["35487.68000000","0.04250000"],["35487.35000000","0.13353700"],["35487.21000000","3.94000000"],["35486.50000000","0.30708400"],["35486.48000000","0.10000000"],["35486.01000000","0.75000000"],["35485.93000000","0.04225100"],["35485.90000000","2.88000000"],["35484.70000000","0.35200000"]],
			  * "asks":[["35496.07000000","0.34867100"],["35496.66000000","0.26443500"],["35496.67000000","0.00112600"],["35497.04000000","0.00095000"],["35497.27000000","1.08723100"],["35499.61000000","0.23423000"],["35499.62000000","0.10000000"],["35499.83000000","0.00155500"],["35500.00000000","0.06332300"],["35500.15000000","0.01127600"],["35500.51000000","0.77017700"],["35500.52000000","0.25000000"],["35500.69000000","0.01289800"],["35500.84000000","0.00947400"],["35500.94000000","0.00400000"],["35500.95000000","0.32000000"],["35501.09000000","0.30707600"],["35501.10000000","0.89300000"],["35501.77000000","0.30126300"],["35502.92000000","0.30707600"]]}}*/
				string json = msg.ToStringFromUTF8();
				var update = JsonConvert.DeserializeObject<MultiDepthStream>(json);
				string marketSymbol = update.Data.MarketSymbol;
				ExchangeOrderBook book = new ExchangeOrderBook { SequenceId = update.Data.FinalUpdate, MarketSymbol = marketSymbol, LastUpdatedUtc = CryptoUtility.UnixTimeStampToDateTimeMilliseconds(update.Data.EventTime) };
				foreach (List<object> ask in update.Data.Asks)
				{
					var depth = new ExchangeOrderPrice { Price = ask[0].ConvertInvariant<decimal>(), Amount = ask[1].ConvertInvariant<decimal>() };
					book.Asks[depth.Price] = depth;
				}
				foreach (List<object> bid in update.Data.Bids)
				{
					var depth = new ExchangeOrderPrice { Price = bid[0].ConvertInvariant<decimal>(), Amount = bid[1].ConvertInvariant<decimal>() };
					book.Bids[depth.Price] = depth;
				}
				callback(book);
				return Task.CompletedTask;
			});
		}


		protected override async Task<ExchangeOrderBook> OnGetOrderBookAsync(string marketSymbol, int maxCount = 100)
		{
			JToken obj = await MakeJsonRequestAsync<JToken>("/depth?symbol=" + marketSymbol + "&limit=" + maxCount);
			return ExchangeAPIExtensions.ParseOrderBookFromJTokenArrays(obj, sequence: "lastUpdateId", maxCount: maxCount);
		}

		protected override async Task OnGetHistoricalTradesAsync(Func<IEnumerable<ExchangeTrade>, bool> callback, string marketSymbol, DateTime? startDate = null, DateTime? endDate = null, int? limit = null)
		{
			/* [ {
            "a": 26129,         // Aggregate tradeId
		    "p": "0.01633102",  // Price
		    "q": "4.70443515",  // Quantity
		    "f": 27781,         // First tradeId
		    "l": 27781,         // Last tradeId
		    "T": 1498793709153, // Timestamp
		    "m": true,          // Was the buyer the maker?
		    "M": true           // Was the trade the best price match?
            } ] */

			//if(startDate == null && endDate == null) {
			//	await OnGetRecentTradesAsync(marketSymbol, limit);
			//}
			//else {
			//System.Windows.Forms.MessageBox.Show("Duplicate MessageId " + MessageId, "HMMMM RETURN?");


		   ExchangeHistoricalTradeHelper state = new ExchangeHistoricalTradeHelper(this)
			{
					Callback = callback,
					EndDate = endDate,
					ParseFunction = (JToken token) => token.ParseTrade("q", "p", "m", "T", TimestampType.UnixMilliseconds, "a", "false"),
					StartDate = startDate,
					MarketSymbol = marketSymbol,
					TimestampFunction = (DateTime dt) => ((long)CryptoUtility.UnixTimestampFromDateTimeMilliseconds(dt)).ToStringInvariant(),
					Url = "/aggTrades?symbol=[marketSymbol]&startTime={0}&endTime={1}",
				};
				await state.ProcessHistoricalTrades();
			//}
		}

		protected override async Task<IEnumerable<ExchangeTrade>> OnGetRecentTradesAsync(string marketSymbol, int? limit = null)
		{
			List<ExchangeTrade> trades = new List<ExchangeTrade>();
			//var maxRequestLimit = 1000; //hard coded for now, should add limit as an argument
			//https://github.com/binance-exchange/binance-official-api-docs/blob/master/rest-api.md#compressedaggregate-trades-list
			int maxRequestLimit = (limit == null || limit < 1 || limit > 1000) ? 1000 : (int)limit;

			JToken obj = await MakeJsonRequestAsync<JToken>($"/aggTrades?symbol={marketSymbol}&limit={maxRequestLimit}");
			//JToken obj = await MakeJsonRequestAsync<JToken>("/public/trades/" + marketSymbol + "?limit=" + maxRequestLimit + "?sort=DESC");
			if(obj.HasValues) { //
				foreach(JToken token in obj) {
					var trade = token.ParseTrade("q", "p", "m", "T", TimestampType.UnixMilliseconds, "a", "false");
					trades.Add(trade);
				}
			}
			return trades.AsEnumerable().Reverse(); //Descending order (ie newest trades first)
			//return trades;
		}

		public async Task OnGetHistoricalTradesAsync(Func<IEnumerable<ExchangeTrade>, bool> callback, string marketSymbol, long startId, long? endId = null)
		{
			/* [ {
            "a": 26129,         // Aggregate tradeId
		    "p": "0.01633102",  // Price
		    "q": "4.70443515",  // Quantity
		    "f": 27781,         // First tradeId
		    "l": 27781,         // Last tradeId
		    "T": 1498793709153, // Timestamp
		    "m": true,          // Was the buyer the maker?
		    "M": true           // Was the trade the best price match?
            } ] */

			// TODO : Refactor into a common layer once more Exchanges implement this pattern

			var fromId = startId;
			var maxRequestLimit = 1000;
			var trades = new List<ExchangeTrade>();
			var processedIds = new HashSet<long>();
			marketSymbol = NormalizeMarketSymbol(marketSymbol);

			do
			{
				if (fromId > endId)
					break;

				trades.Clear();
				var limit = Math.Min(endId - fromId ?? maxRequestLimit, maxRequestLimit);
				var obj = await MakeJsonRequestAsync<JToken>($"/aggTrades?symbol={marketSymbol}&fromId={fromId}&limit={limit}");

				foreach (var token in obj)
				{
					var trade = token.ParseTrade("q", "p", "m", "T", TimestampType.UnixMilliseconds, "a", "false");
					long tradeId = (long)trade.Id.ConvertInvariant<ulong>();
					if (tradeId < fromId)
						continue;
					if (tradeId > endId)
						continue;
					if (!processedIds.Add(tradeId))
						continue;

					trades.Add(trade);
					fromId = tradeId;
				}

				fromId++;
			} while (callback(trades) && trades.Count > 0);
		}

		public async Task OnGetHistoricalTradesAsync(Func<IEnumerable<ExchangeTrade>, bool> callback, string marketSymbol, int limit = 100)
		{
			/* [ {
            "a": 26129,         // Aggregate tradeId
		    "p": "0.01633102",  // Price
		    "q": "4.70443515",  // Quantity
		    "f": 27781,         // First tradeId
		    "l": 27781,         // Last tradeId
		    "T": 1498793709153, // Timestamp
		    "m": true,          // Was the buyer the maker?
		    "M": true           // Was the trade the best price match?
            } ] */

			// TODO : Refactor into a common layer once more Exchanges implement this pattern
			// https://github.com/binance-exchange/binance-official-api-docs/blob/master/rest-api.md#compressedaggregate-trades-list
			if(limit > 1000) limit = 1000;	//Binance max = 1000
			var maxRequestLimit = 1000; 
			var trades = new List<ExchangeTrade>();
			var processedIds = new HashSet<long>();
			marketSymbol = NormalizeMarketSymbol(marketSymbol);

			do {
				//if(fromId > endId)
				//	break;

				trades.Clear();
				//var limit = Math.Min(endId - fromId ?? maxRequestLimit, maxRequestLimit);
				var obj = await MakeJsonRequestAsync<JToken>($"/aggTrades?symbol={marketSymbol}&limit={limit}");

				foreach(var token in obj) {
					var trade = token.ParseTrade("q", "p", "m", "T", TimestampType.UnixMilliseconds, "a", "false");
					//long tradeId = (long)trade.Id.ConvertInvariant<ulong>();
					//if(tradeId < fromId)
					//	continue;
					//if(tradeId > endId)
					//	continue;
					//if(!processedIds.Add(tradeId))
					//	continue;

					trades.Add(trade);
					//fromId = tradeId;
				}

				//fromId++;
			} while(callback(trades) && trades.Count > 0);
		}

		public async Task<IEnumerable<ExchangeFutureOrder>> GetAllOpenOrdersAsync()
		{
			List<ExchangeFutureOrder> orders = new List<ExchangeFutureOrder>();
			Dictionary<string, object> payload = await GetNoncePayloadAsync();
			var symbollist = await GetMarketSymbolsAsync();
			foreach (var symbol in symbollist)
			{
				if (symbol.Contains("ETH") || symbol.Contains("BTC") || symbol.Contains("BNB"))
				{
					payload["symbol"] = symbol!;
					try
					{
						JToken token = await MakeJsonRequestAsync<JToken>("/openOrders", BaseUrlPrivate, payload);
						foreach (JToken order in token)
						{
							orders.Add(ParseFutureOrder(order));
						}
					}
					catch (Exception d)
					{ }
				}
			}
			return orders;
		}

		private ExchangeFutureOrder ParseFutureOrder(JToken token)
		{/*{{
  "orderId": 354485184,
  "symbol": "BTCUSD_211231",
  "pair": "BTCUSD",
  "status": "NEW",
  "clientOrderId": "electron_nUCXipyfOd1nt4BhdD35",
  "price": "48047.1",
  "avgPrice": "0",
  "origQty": "2",
  "executedQty": "0",
  "cumBase": "0",
  "timeInForce": "GTC",
  "type": "LIMIT",
  "reduceOnly": false,
  "closePosition": false,
  "side": "BUY",
  "positionSide": "BOTH",
  "stopPrice": "0",
  "workingType": "CONTRACT_PRICE",
  "priceProtect": false,
  "origType": "LIMIT",
  "time": 1630069318133,
  "updateTime": 1630069318133
}}*/
			ExchangeFutureOrder order = new ExchangeFutureOrder()
			{
				Symbol = token["symbol"].ToStringInvariant(),
				TimeInForce = token["timeInForce"].ToStringInvariant(),
				OrderType = token["type"].ToStringInvariant(),
				OrderStatus = token["status"].ToStringInvariant(),
				//IsMaker = token[""],
				IsReduceOnly = token["reduceOnly"].ConvertInvariant<bool>(),
				//AccountType = token[""]
				ACTIVATIONPRICE = token["activatePrice"].ConvertInvariant<decimal>(),
				StopPrice = token["stopPrice"].ConvertInvariant<decimal>(),
				OrigQty = token["origQty"].ConvertInvariant<decimal>(),
				FilledQty = token["executedQty"].ConvertInvariant<decimal>(),
				OrigPrice = token["price"].ConvertInvariant<decimal>(),
				AvgPrice = token["avgPrice"].ConvertInvariant<decimal>(),
				Side = token["side"].ToStringInvariant(),
				OrderTime = CryptoUtility.UnixTimeStampToDateTimeMilliseconds(token["time"].ConvertInvariant<long>()),
				UpdateTime = CryptoUtility.UnixTimeStampToDateTimeMilliseconds(token["updateTime"].ConvertInvariant<long>()),
				OrderId = token["orderId"].ToStringInvariant(),
				ClientOrderId = token["clientOrderId"].ToStringInvariant(),
				CommissionAsset = token["commissionAsset"].ToStringInvariant(),
				Commission = token["commision"].ConvertInvariant<decimal>()
			};
			return order;
		}
		protected override async Task<IEnumerable<MarketCandle>> OnGetCandlesAsync(string marketSymbol, int periodSeconds, DateTime? startDate = null, DateTime? endDate = null, int? limit = null)
		{
			/* [
            [
		    1499040000000,      // Open time
		    "0.01634790",       // Open
		    "0.80000000",       // High
		    "0.01575800",       // Low
		    "0.01577100",       // Close
		    "148976.11427815",  // Volume
		    1499644799999,      // Close time
		    "2434.19055334",    // Quote asset volume
		    308,                // Number of trades
		    "1756.87402397",    // Taker buy base asset volume
		    "28.46694368",      // Taker buy quote asset volume
		    "17928899.62484339" // Can be ignored
		    ]] */

			List<MarketCandle> candles = new List<MarketCandle>();
			string url = "/klines?symbol=" + marketSymbol;
			if (startDate != null)
			{
				url += "&startTime=" + (long)startDate.Value.UnixTimestampFromDateTimeMilliseconds();
				url += "&endTime=" + ((endDate == null ? long.MaxValue : (long)endDate.Value.UnixTimestampFromDateTimeMilliseconds())).ToStringInvariant();
			}
			if (limit != null)
			{
				url += "&limit=" + (limit.Value.ToStringInvariant());
			}
			url += "&interval=" + PeriodSecondsToString(periodSeconds);
			JToken obj = await MakeJsonRequestAsync<JToken>(url);
			foreach (JToken token in obj)
			{
				candles.Add(this.ParseCandle(token, marketSymbol, periodSeconds, 1, 2, 3, 4, 0, TimestampType.UnixMilliseconds, 5, 7));
			}

			return candles;
		}
		protected override async Task<Dictionary<string, decimal>> OnGetAmountsAsync()
		{
			JToken token = await MakeJsonRequestAsync<JToken>("/account", BaseUrlPrivate, await GetNoncePayloadAsync());
			//Dictionary<string, decimal> balances = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
			List<KeyValuePair<string, GeneralAccount>> balances = new List<KeyValuePair<string, GeneralAccount>>();
			if (token["balances"] != null)
			{
				var exchangeid = "Binance";
				var accounttype = token["accountType"].ToString();
				foreach (JToken balance in token["balances"])
				{
					if (balance["free"].ConvertInvariant<decimal>() > 0m)
					{
						string sym = balance["asset"].ToStringInvariant();
						balances.Add(new KeyValuePair<string, GeneralAccount>(exchangeid + ":" + accounttype + ":" + sym, new GeneralAccount()
						{
							Available = balance["availableBalance"].ConvertInvariant<decimal>(),
							ExchangeID = "Binance",
							Symbol = sym,
							PositionMargin = balance["positionInitialMargin"].ConvertInvariant<decimal>(),
							Equity = balance["marginBalance"].ConvertInvariant<decimal>(),
							RealizedPNL = balance["AccumulatedRealized"].ConvertInvariant<decimal>(),
							UnrealizedPNL = balance["unrealizedProfit"].ConvertInvariant<decimal>(),
							OrderMargin = balance["openOrderInitialMargin"].ConvertInvariant<decimal>(),
							AccountType = Name.Substring(7)
						}));
					}
				}
			}			
			return null;
		}
		protected override async Task<IEnumerable<KeyValuePair<string, GeneralAccount>>> OnGetGeneralAccountsAsync()
		{
			JToken token = await MakeJsonRequestAsync<JToken>("/account", BaseUrlPrivate, await GetNoncePayloadAsync());
			List<KeyValuePair<string, GeneralAccount>> balances = new List<KeyValuePair<string, GeneralAccount>>();

			foreach (JToken balance in token["balances"])
			{
				decimal amount = balance["free"].ConvertInvariant<decimal>();
				decimal locked = balance["locked"].ConvertInvariant<decimal>();
				if (amount > 0m)
				{
					string sym = balance["asset"].ToStringInvariant();
					string accounttype = token["accountType"].ToStringInvariant();
					var tt = token["updateTime"].ConvertInvariant<double>();
					try
					{
						DateTime dt = CryptoUtility.UnixTimeStampToDateTimeMilliseconds(tt);
					}
					catch (Exception e)
					{ }
					balances.Add(new KeyValuePair<string, GeneralAccount>(sym, new GeneralAccount()
					{
						Equity = amount,
						Available = amount,
						ExchangeID = "Binance",
						Symbol = sym,
						OrderMargin = locked,
						AccountType = accounttype,
						UpdateTime = CryptoUtility.UnixTimeStampToDateTimeMilliseconds(token["updateTime"].ConvertInvariant<double>())
					}));
				}
			}
			if (!(token["positions"] is null))
			{

			}
			return balances;
		}
		protected override async Task<Dictionary<string, decimal>> OnGetAmountsAvailableToTradeAsync()
		{
			JToken token = await MakeJsonRequestAsync<JToken>("/account", BaseUrlPrivate, await GetNoncePayloadAsync());
			Dictionary<string, decimal> balances = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
			foreach (JToken balance in token["balances"])
			{
				decimal amount = balance["free"].ConvertInvariant<decimal>();
				if (amount > 0m)
				{
					balances[balance["asset"].ToStringInvariant()] = amount;
				}
			}
			return balances;
		}

		protected override async Task<ExchangeOrderResult> OnPlaceOrderAsync(ExchangeOrderRequest order)
		{
			Dictionary<string, object> payload = await GetNoncePayloadAsync();
			payload["symbol"] = order.MarketSymbol;
			payload["newClientOrderId"] = order.ClientOrderId;
			payload["side"] = order.IsBuy ? "BUY" : "SELL";
			if (order.OrderType == OrderType.Stop)
				payload["type"] = "STOP_LOSS";//if order type is stop loss/limit, then binance expect word 'STOP_LOSS' inestead of 'STOP'
			else
				payload["type"] = order.OrderType.ToStringUpperInvariant();

			// Binance has strict rules on which prices and quantities are allowed. They have to match the rules defined in the market definition.
			decimal outputQuantity = await ClampOrderQuantity(order.MarketSymbol, order.Amount);
			decimal outputPrice = await ClampOrderPrice(order.MarketSymbol, order.Price.Value);

			// Binance does not accept quantities with more than 20 decimal places.
			payload["quantity"] = Math.Round(outputQuantity, 20);
			payload["newOrderRespType"] = "FULL";

			if (order.OrderType != OrderType.Market)
			{
				payload["timeInForce"] = "GTC";
				payload["price"] = outputPrice;
			}
			order.ExtraParameters.CopyTo(payload);

			JToken? token = await MakeJsonRequestAsync<JToken>("/order", BaseUrlPrivate, payload, "POST");
            if (token is null)
            {
                return null;
            }
			return ParseOrder(token);
		}

		protected override async Task<ExchangeOrderResult> OnGetOrderDetailsAsync(string orderId, string? marketSymbol = null, bool isClientOrderId = false)
		{
			Dictionary<string, object> payload = await GetNoncePayloadAsync();
			if (string.IsNullOrWhiteSpace(marketSymbol))
			{
				throw new InvalidOperationException("Binance single order details request requires symbol");
			}
			payload["symbol"] = marketSymbol!;

			if (isClientOrderId) // Either orderId or origClientOrderId must be sent.
				payload["origClientOrderId"] = orderId;
			else
				payload["orderId"] = orderId;

			JToken token = await MakeJsonRequestAsync<JToken>("/order", BaseUrlPrivate, payload);
			ExchangeOrderResult result = ParseOrder(token);

			// Add up the fees from each trade in the order
			Dictionary<string, object> feesPayload = await GetNoncePayloadAsync();
			feesPayload["symbol"] = marketSymbol!;
			JToken feesToken = await MakeJsonRequestAsync<JToken>("/myTrades", BaseUrlPrivate, feesPayload);
			ParseFees(feesToken, result);

			return result;
		}
		public async Task<(IEnumerable<KeyValuePair<string, GeneralAccount>>, IEnumerable<KeyValuePair<string, GeneralPosition>>)> GetAccountAndPositionsAsync()
		{
			List<KeyValuePair<string, GeneralAccount>> balances = new List<KeyValuePair<string, GeneralAccount>>();
			List<KeyValuePair<string, GeneralPosition>> positions = new List<KeyValuePair<string, GeneralPosition>>();
			JToken token = await MakeJsonRequestAsync<JToken>("/account", BaseUrlPrivate, await GetNoncePayloadAsync());
			if (token is null)
				return (balances, positions);
			if (Name == "Binance")
			{
				foreach (var asset in token["balances"])
				{
					if (asset["free"].ConvertInvariant<decimal>() > 0)
					{
						balances.Add(new KeyValuePair<string, GeneralAccount>(asset["asset"].ToStringInvariant(), new GeneralAccount()
						{
							AccountType = token["accountType"].ToStringInvariant(),
							Available = asset["free"].ConvertInvariant<decimal>(),
							OrderMargin = asset["locked"].ConvertInvariant<decimal>(),
							UpdateTime = CryptoUtility.UnixTimeStampToDateTimeMilliseconds(token["updateTime"].ConvertInvariant<long>()),
							ExchangeID = Name,
							Symbol = asset["asset"].ToStringInvariant(),
							Equity = asset["free"].ConvertInvariant<decimal>() + asset["locked"].ConvertInvariant<decimal>()
						}));
					}
				}
				return (balances, positions);
			}
			else
			{
				foreach (JToken balance in token["assets"])
				{
					if (balance["walletBalance"].ConvertInvariant<decimal>() > 0m)
					{
						string sym = balance["asset"].ToStringInvariant();
						var kv = new KeyValuePair<string, GeneralAccount>(sym, new GeneralAccount()
						{
							Available = balance["availableBalance"].ConvertInvariant<decimal>(),
							ExchangeID = "Binance",
							Symbol = sym,
							PositionMargin = balance["positionInitialMargin"].ConvertInvariant<decimal>(),
							Equity = balance["marginBalance"].ConvertInvariant<decimal>(),
							RealizedPNL = balance["AccumulatedRealized"].ConvertInvariant<decimal>(),
							UnrealizedPNL = balance["unrealizedProfit"].ConvertInvariant<decimal>(),
							OrderMargin = balance["openOrderInitialMargin"].ConvertInvariant<decimal>(),
							AccountType = Name.Substring(7)
						});
						if (Name == "BinanceCOINFuture") kv.Value.UpdateTime = DateTime.UtcNow;
						if (Name == "BinanceUSDFuture") kv.Value.UpdateTime = CryptoUtility.UnixTimeStampToDateTimeMilliseconds(balance["updateTime"].ConvertInvariant<long>());
						balances.Add(kv);
					}
				}
				foreach (JToken position in token["positions"])
				{
					var ps = position["positionAmt"].ConvertInvariant<decimal>();
					if (ps != 0)
					{
						//var ut = position["updateTime"].ConvertInvariant<decimal>();
						//var uTime = CryptoUtility.UnixTimeStampToDateTimeMilliseconds((double)ut);
						positions.Add(new KeyValuePair<string, GeneralPosition>(position["symbol"].ToStringInvariant(),
							new GeneralPosition()
							{
								AvgCost = position["entryPrice"].ConvertInvariant<decimal>(),
								ExchangeID = "Binance",
								Symbol = position["symbol"].ToStringInvariant(),
								Position = ps,
								UnrealizedPnl = position["unrealizedProfit"].ConvertInvariant<decimal>(),
								OrderMargin = position["openOrderInitialMargin"].ConvertInvariant<decimal>(),
								PositionMargin = position["maintMargin"].ConvertInvariant<decimal>(),
								UpdateTime = CryptoUtility.UnixTimeStampToDateTimeMilliseconds(position["updateTime"].ConvertInvariant<double>())
							}));
					}
				}
				return (balances, positions);
			}
		}
		/// <summary>Process the trades that executed as part of your order and sum the fees.</summary>
		/// <param name="feesToken">The trades executed for a specific currency pair.</param>
		/// <param name="result">The result object to append to.</param>
		private static void ParseFees(JToken feesToken, ExchangeOrderResult result)
		{
			var tradesInOrder = feesToken.Where(x => x["orderId"].ToStringInvariant() == result.OrderId);

			bool currencySet = false;
			foreach (var trade in tradesInOrder)
			{
				result.Fees += trade["commission"].ConvertInvariant<decimal>();

				// TODO: Not sure how to handle commissions in different currencies, for example if you run out of BNB mid-trade
				if (!currencySet)
				{
					result.FeesCurrency = trade["commissionAsset"].ToStringInvariant();
					currencySet = true;
				}
			}
		}

		protected override async Task<IEnumerable<ExchangeOrderResult>> OnGetOpenOrderDetailsAsync(string? marketSymbol = null)
		{
			List<ExchangeOrderResult> orders = new List<ExchangeOrderResult>();
			Dictionary<string, object> payload = await GetNoncePayloadAsync();
			if (!string.IsNullOrWhiteSpace(marketSymbol))
			{
				payload["symbol"] = marketSymbol!;
			}
			JToken token = await MakeJsonRequestAsync<JToken>("/openOrders", BaseUrlPrivate, payload);
			foreach (JToken order in token)
			{
				orders.Add(ParseOrder(order));
			}

			return orders;
		}

		private async Task<IEnumerable<ExchangeOrderResult>> GetCompletedOrdersForAllSymbolsAsync(DateTime? afterDate)
		{
			// TODO: This is a HACK, Binance API needs to add a single API call to get all orders for all symbols, terrible...
			List<ExchangeOrderResult> orders = new List<ExchangeOrderResult>();
			Exception? ex = null;
			string? failedSymbol = null;
			var syms= await GetMarketSymbolsAsync();
			//var syms = symb.Where(s => s.IndexOf("BTC", StringComparison.OrdinalIgnoreCase) >= 0);
			Parallel.ForEach(syms, async (s) =>
			{
				try
				{
					foreach (ExchangeOrderResult order in await GetCompletedOrderDetailsAsync(s, afterDate))
					{
						lock (orders)
						{
							orders.Add(order);
						}
					}
				}
				catch (Exception _ex)
				{
					failedSymbol = s;
					ex = _ex;
				}
			});

			if (ex != null)
			{
				throw new APIException("Failed to get completed order details for symbol " + failedSymbol, ex);
			}

			// sort timestamp desc
			orders.Sort((o1, o2) =>
			{
				return o2.OrderDate.CompareTo(o1.OrderDate);
			});
			return orders;
		}

		protected override async Task<IEnumerable<ExchangeOrderResult>> OnGetCompletedOrderDetailsAsync(string? marketSymbol = null, DateTime? afterDate = null)
		{
			//new way
			List<ExchangeOrderResult> trades = new List<ExchangeOrderResult>();
			try
			{
				if (string.IsNullOrWhiteSpace(marketSymbol))
				{
					trades.AddRange(await GetCompletedOrdersForAllSymbolsAsync(afterDate));
				}
				else
				{
					Dictionary<string, object> payload = await GetNoncePayloadAsync();
					payload["symbol"] = marketSymbol!;
					if (afterDate != null)
					{
						payload["startTime"] = Math.Round(afterDate.Value.UnixTimestampFromDateTimeMilliseconds());
					}
					JToken token = await MakeJsonRequestAsync<JToken>("/myTrades", BaseUrlPrivate, payload);
					foreach (JToken trade in token)
					{
						trades.Add(ParseTrade(trade, marketSymbol!));
					}
				}
				return trades;
			}
			catch (Exception ex)
			{
				throw ex;
			}

			//old way

			//List<ExchangeOrderResult> orders = new List<ExchangeOrderResult>();
			//if (string.IsNullOrWhiteSpace(marketSymbol))
			//{
			//    orders.AddRange(await GetCompletedOrdersForAllSymbolsAsync(afterDate));
			//}
			//else
			//{
			//    Dictionary<string, object> payload = await GetNoncePayloadAsync();
			//    payload["symbol"] = marketSymbol;
			//    if (afterDate != null)
			//    {
			//        payload["startTime"] = Math.Round(afterDate.Value.UnixTimestampFromDateTimeMilliseconds());
			//    }
			//    JToken token = await MakeJsonRequestAsync<JToken>("/allOrders", BaseUrlPrivate, payload);
			//    foreach (JToken order in token)
			//    {
			//        orders.Add(ParseOrder(order));
			//    }
			//}
			//return orders;
		}

		private async Task<IEnumerable<ExchangeOrderResult>> GetMyTradesForAllSymbols(DateTime? afterDate)
		{
			// TODO: This is a HACK, Binance API needs to add a single API call to get all orders for all symbols, terrible...
			List<ExchangeOrderResult> trades = new List<ExchangeOrderResult>();
			Exception? ex = null;
			string? failedSymbol = null;
			Parallel.ForEach((await GetMarketSymbolsAsync()).Where(s => s.IndexOf("BTC", StringComparison.OrdinalIgnoreCase) >= 0), async (s) =>
			{
				try
				{
					foreach (ExchangeOrderResult trade in (await GetMyTradesAsync(s, afterDate)))
					{
						lock (trades)
						{
							trades.Add(trade);
						}
					}
				}
				catch (Exception _ex)
				{
					failedSymbol = s;
					ex = _ex;
				}
			});

			if (ex != null)
			{
				throw new APIException("Failed to get my trades for symbol " + failedSymbol, ex);
			}

			// sort timestamp desc
			trades.Sort((o1, o2) =>
			{
				return o2.OrderDate.CompareTo(o1.OrderDate);
			});
			return trades;
		}

		private async Task<IEnumerable<ExchangeOrderResult>> OnGetMyTradesAsync(string? marketSymbol = null, DateTime? afterDate = null)
		{
			List<ExchangeOrderResult> trades = new List<ExchangeOrderResult>();
			if (string.IsNullOrWhiteSpace(marketSymbol))
			{
				trades.AddRange(await GetCompletedOrdersForAllSymbolsAsync(afterDate));
			}
			else
			{
				Dictionary<string, object> payload = await GetNoncePayloadAsync();
				payload["symbol"] = marketSymbol!;
				if (afterDate != null)
				{
					payload["timestamp"] = afterDate.Value.UnixTimestampFromDateTimeMilliseconds();
				}
				JToken token = await MakeJsonRequestAsync<JToken>("/myTrades", BaseUrlPrivate, payload);
				foreach (JToken trade in token)
				{
					trades.Add(ParseTrade(trade, marketSymbol!));
				}
			}
			return trades;
		}

		protected override async Task OnCancelOrderAsync(string orderId, string? marketSymbol = null)
		{
			Dictionary<string, object> payload = await GetNoncePayloadAsync();
			if (string.IsNullOrWhiteSpace(marketSymbol))
			{
				throw new InvalidOperationException("Binance cancel order request requires symbol");
			}
			payload["symbol"] = marketSymbol!;
			payload["orderId"] = orderId;
            _ = await MakeJsonRequestAsync<JToken>("/order", BaseUrlPrivate, payload, "DELETE");
		}

		/// <summary>A withdrawal request. Fee is automatically subtracted from the amount.</summary>
		/// <param name="withdrawalRequest">The withdrawal request.</param>
		/// <returns>Withdrawal response from Binance</returns>
		protected override async Task<ExchangeWithdrawalResponse> OnWithdrawAsync(ExchangeWithdrawalRequest withdrawalRequest)
		{
			if (string.IsNullOrWhiteSpace(withdrawalRequest.Currency))
			{
				throw new ArgumentException("Symbol must be provided for Withdraw");
			}
			else if (string.IsNullOrWhiteSpace(withdrawalRequest.Address))
			{
				throw new ArgumentException("Address must be provided for Withdraw");
			}
			else if (withdrawalRequest.Amount <= 0)
			{
				throw new ArgumentException("Withdrawal amount must be positive and non-zero");
			}

			Dictionary<string, object> payload = await GetNoncePayloadAsync();
			payload["asset"] = withdrawalRequest.Currency;
			payload["address"] = withdrawalRequest.Address;
			payload["amount"] = withdrawalRequest.Amount;
			payload["name"] = withdrawalRequest.Description ?? "apiwithdrawal"; // Contrary to what the API docs say, name is required

			if (!string.IsNullOrWhiteSpace(withdrawalRequest.AddressTag))
			{
				payload["addressTag"] = withdrawalRequest.AddressTag;
			}

			JToken response = await MakeJsonRequestAsync<JToken>("/withdraw.html", WithdrawalUrlPrivate, payload, "POST");
			ExchangeWithdrawalResponse withdrawalResponse = new ExchangeWithdrawalResponse
			{
				Id = response["id"].ToStringInvariant(),
				Message = response["msg"].ToStringInvariant(),
			};

			return withdrawalResponse;
		}

		private bool ParseMarketStatus(string status)
		{
			bool isActive = false;
			if (!string.IsNullOrWhiteSpace(status))
			{
				switch (status)
				{
					case "TRADING":
						isActive = true;
						break;
						/*
                            case "PRE_TRADING":
                            case "POST_TRADING":
                            case "END_OF_DAY":
                            case "HALT":
                            case "AUCTION_MATCH":
                            case "BREAK": */
				}
			}

			return isActive;
		}

		private async Task<ExchangeTicker> ParseTickerAsync(string symbol, JToken token)
		{
			// {"priceChange":"-0.00192300","priceChangePercent":"-4.735","weightedAvgPrice":"0.03980955","prevClosePrice":"0.04056700","lastPrice":"0.03869000","lastQty":"0.69300000","bidPrice":"0.03858500","bidQty":"38.35000000","askPrice":"0.03869000","askQty":"31.90700000","openPrice":"0.04061300","highPrice":"0.04081900","lowPrice":"0.03842000","volume":"128015.84300000","quoteVolume":"5096.25362239","openTime":1512403353766,"closeTime":1512489753766,"firstId":4793094,"lastId":4921546,"count":128453}
			return await this.ParseTickerAsync(token, symbol, "askPrice", "bidPrice", "lastPrice", "volume", "quoteVolume", "closeTime", TimestampType.UnixMilliseconds);
		}

		private async Task<ExchangeTicker> ParseTickerWebSocketAsync(JToken token)
		{
			string marketSymbol = token["s"].ToStringInvariant();
			return await this.ParseTickerAsync(token, marketSymbol, "a", "b", "c", "v", "q", "E", TimestampType.UnixMilliseconds);
		}

		private ExchangeOrderResult ParseOrder(JToken token)
		{
			/*
              "symbol": "IOTABTC",
              "orderId": 1,
              "clientOrderId": "12345",
              "transactTime": 1510629334993,
              "price": "1.00000000",
              "origQty": "1.00000000",
              "executedQty": "0.00000000",
              "status": "NEW",
              "timeInForce": "GTC",
              "type": "LIMIT",
              "side": "SELL",
              "fills": [
                  {
                    "price": "4000.00000000",
                    "qty": "1.00000000",
                    "commission": "4.00000000",
                    "commissionAsset": "USDT"
                  },
                  {
                    "price": "3999.00000000",
                    "qty": "5.00000000",
                    "commission": "19.99500000",
                    "commissionAsset": "USDT"
                  },
                  {
                    "price": "3998.00000000",
                    "qty": "2.00000000",
                    "commission": "7.99600000",
                    "commissionAsset": "USDT"
                  },
                  {
                    "price": "3997.00000000",
                    "qty": "1.00000000",
                    "commission": "3.99700000",
                    "commissionAsset": "USDT"
                  },
                  {
                    "price": "3995.00000000",
                    "qty": "1.00000000",
                    "commission": "3.99500000",
                    "commissionAsset": "USDT"
                  }
                ]
            */
			ExchangeOrderResult result = new ExchangeOrderResult
			{
				Amount = token["origQty"].ConvertInvariant<decimal>(),
				AmountFilled = token["executedQty"].ConvertInvariant<decimal>(),
				Price = token["price"].ConvertInvariant<decimal>(),
				IsBuy = token["side"].ToStringInvariant() == "BUY",
				OrderDate = CryptoUtility.UnixTimeStampToDateTimeMilliseconds(token["time"].ConvertInvariant<long>(token["transactTime"].ConvertInvariant<long>())),
				OrderId = token["orderId"].ToStringInvariant(),
				MarketSymbol = token["symbol"].ToStringInvariant(),
				ClientOrderId = token["clientOrderId"].ToStringInvariant()
			};

			result.Result = ParseExchangeAPIOrderResult(token["status"].ToStringInvariant(), result.AmountFilled.Value);

			return result;
		}

		private ExchangeAPIOrderResult ParseExchangeAPIOrderResult(string status, decimal amountFilled)
		{
			switch (status)
			{
				case "NEW":
					return ExchangeAPIOrderResult.Open;
				case "PARTIALLY_FILLED":
					return ExchangeAPIOrderResult.FilledPartially;
				case "FILLED":
					return ExchangeAPIOrderResult.Filled;
				case "CANCELED":
					return amountFilled > 0 ? ExchangeAPIOrderResult.FilledPartiallyAndCancelled : ExchangeAPIOrderResult.Canceled;
				case "PENDING_CANCEL":
				case "EXPIRED":
				case "REJECTED":
					return ExchangeAPIOrderResult.Canceled;
				default:
					throw new NotImplementedException($"Unexpected status type: {status}");
			}
		}

		public ExchangeOrderResult ParseTrade(JToken token, string symbol)
		{
			/*
              {{
  "orderId": 20837512096,
  "symbol": "ETHUSD_PERP",
  "pair": "ETHUSD",
  "status": "FILLED",
  "clientOrderId": "electron_7aKRlAeJRZGbrViUj3Ye",
  "price": "4150.08",
  "avgPrice": "4150.08",
  "origQty": "21",
  "executedQty": "21",
  "cumBase": "0.05060143",
  "timeInForce": "GTC",
  "type": "LIMIT",
  "reduceOnly": false,
  "closePosition": false,
  "side": "SELL",
  "positionSide": "BOTH",
  "stopPrice": "0",
  "workingType": "CONTRACT_PRICE",
  "priceProtect": false,
  "origType": "LIMIT",
  "time": 1638678825552,
  "updateTime": 1638678863903
}}
            */
			ExchangeOrderResult result = new ExchangeOrderResult
			{
				Result = ExchangeAPIOrderResult.Filled, //token["status"].ToStringInvariant(),
				Amount = token["origQty"].ConvertInvariant<decimal>(),
				AmountFilled = token["executedQty"].ConvertInvariant<decimal>(),
				Price = token["price"].ConvertInvariant<decimal>(),
				AveragePrice = token["avgPrice"].ConvertInvariant<decimal>(),
				IsBuy = token["side"].ToStringInvariant()=="BUY"? true:false,
				OrderDate = CryptoUtility.UnixTimeStampToDateTimeMilliseconds(token["time"].ConvertInvariant<long>()),
				TradeDate = CryptoUtility.UnixTimeStampToDateTimeMilliseconds(token["updateTime"].ConvertInvariant<long>()),
				OrderId = token["orderId"].ToStringInvariant(),
				TradeId = token["id"].ToStringInvariant(),
				Fees = token["commission"].ConvertInvariant<decimal>(),
				FeesCurrency = token["commissionAsset"].ToStringInvariant(),
				MarketSymbol = symbol
			};

			return result;
		}

		public static void ParseAveragePriceAndFeesFromFills(ExchangeOrderResult result, JToken fillsToken)
		{
			decimal totalCost = 0;
			decimal totalQuantity = 0;

			bool currencySet = false;
			if (fillsToken is JArray)
			{
				foreach (var fill in fillsToken)
				{
					if (!currencySet)
					{
						result.FeesCurrency = fill["commissionAsset"].ToStringInvariant();
						currencySet = true;
					}

					result.Fees += fill["commission"].ConvertInvariant<decimal>();

					decimal price = fill["price"].ConvertInvariant<decimal>();
					decimal quantity = fill["qty"].ConvertInvariant<decimal>();
					totalCost += price * quantity;
					totalQuantity += quantity;
				}
			}

			result.AveragePrice = (totalQuantity == 0 ? 0 : totalCost / totalQuantity);
		}

		protected override Task ProcessRequestAsync(IHttpWebRequest request, Dictionary<string, object>? payload)
		{
            if (CanMakeAuthenticatedRequest(payload) ||
                (payload == null && request.RequestUri.AbsoluteUri.Contains("userDataStream")))
            {
                request.AddHeader("X-MBX-APIKEY", PublicApiKey!.ToUnsecureString());
            }
			return base.ProcessRequestAsync(request, payload);
		}

		protected override Uri ProcessRequestUrl(UriBuilder url, Dictionary<string, object>? payload, string? method)
		{
			if (CanMakeAuthenticatedRequest(payload))
			{
				// payload is ignored, except for the nonce which is added to the url query - bittrex puts all the "post" parameters in the url query instead of the request body
				var query = (url.Query ?? string.Empty).Trim('?', '&');
				string newQuery = "timestamp=" + payload!["nonce"].ToStringInvariant() + (query.Length != 0 ? "&" + query : string.Empty) +
					(payload.Count > 1 ? "&" + CryptoUtility.GetFormForPayload(payload, false) : string.Empty);
				string signature = CryptoUtility.SHA256Sign(newQuery, CryptoUtility.ToUnsecureBytesUTF8(PrivateApiKey));
				newQuery += "&signature=" + signature;
				url.Query = newQuery;
				return url.Uri;
			}
			return base.ProcessRequestUrl(url, payload, method);
		}

		/// <summary>
		/// Gets the address to deposit to and applicable details.
		/// </summary>
		/// <param name="currency">Currency to get address for</param>
		/// <param name="forceRegenerate">(ignored) Binance does not provide the ability to generate new addresses</param>
		/// <returns>
		/// Deposit address details (including tag if applicable, such as XRP)
		/// </returns>
		protected override async Task<ExchangeDepositDetails> OnGetDepositAddressAsync(string currency, bool forceRegenerate = false)
		{
			/*
            * TODO: Binance does not offer a "regenerate" option in the API, but a second IOTA deposit to the same address will not be credited
            * How does Binance handle GetDepositAddress for IOTA after it's been used once?
            * Need to test calling this API after depositing IOTA.
            */

			Dictionary<string, object> payload = await GetNoncePayloadAsync();
			payload["asset"] = currency;

			JToken response = await MakeJsonRequestAsync<JToken>("/depositAddress.html", WithdrawalUrlPrivate, payload);
			ExchangeDepositDetails depositDetails = new ExchangeDepositDetails
			{
				Currency = response["asset"].ToStringInvariant(),
				Address = response["address"].ToStringInvariant(),
				AddressTag = response["addressTag"].ToStringInvariant()
			};

			return depositDetails;
		}

		/// <summary>Gets the deposit history for a symbol</summary>
		/// <param name="currency">The currency to check. Null for all symbols.</param>
		/// <returns>Collection of ExchangeCoinTransfers</returns>
		protected override async Task<IEnumerable<ExchangeTransaction>> OnGetDepositHistoryAsync(string currency)
		{
			// TODO: API supports searching on status, startTime, endTime
			Dictionary<string, object> payload = await GetNoncePayloadAsync();
			if (!string.IsNullOrWhiteSpace(currency))
			{
				payload["asset"] = currency;
			}
			JToken response = await MakeJsonRequestAsync<JToken>("/depositHistory.html", WithdrawalUrlPrivate, payload);
			var transactions = new List<ExchangeTransaction>();
			foreach (JToken token in response["depositList"])
			{
				var transaction = new ExchangeTransaction
				{
					Timestamp = token["insertTime"].ConvertInvariant<double>().UnixTimeStampToDateTimeMilliseconds(),
					Amount = token["amount"].ConvertInvariant<decimal>(),
					Currency = token["asset"].ToStringUpperInvariant(),
					Address = token["address"].ToStringInvariant(),
					AddressTag = token["addressTag"].ToStringInvariant(),
					BlockchainTxId = token["txId"].ToStringInvariant()
				};
				int status = token["status"].ConvertInvariant<int>();
				switch (status)
				{
					case 0:
						transaction.Status = TransactionStatus.Processing;
						break;

					case 1:
						transaction.Status = TransactionStatus.Complete;
						break;

					default:
						// If new states are added, see https://github.com/binance-exchange/binance-official-api-docs/blob/master/wapi-api.md
						transaction.Status = TransactionStatus.Unknown;
						transaction.Notes = "Unknown transaction status: " + status;
						break;
				}

				transactions.Add(transaction);
			}

			return transactions;
		}

		protected override async Task<IWebSocket> OnUserDataWebSocketAsync(Action<object> callback)
		{
			var listenKey = await GetListenKeyAsync();
			return await ConnectWebSocketAsync($"/stream?streams={listenKey}", (_socket, msg) =>
			{/*
			  token = {{
  "stream": "BTtVnEtw13TrgIke6TpdT1NRAjsYYlGFKfuWtWBAXA1znx8QiPE8nhc0RYSq",
  "data": {First = {{
  "e": "executionReport",
  "E": 1624068030726,
  "s": "1INCHUSDT",
  "c": "web_b62f88d4c2244f33a6365d445f1f2a8d",
  "S": "BUY",
  "o": "LIMIT",
  "f": "GTC",
  "q": "10.00000000",
  "p": "3.19000000",
  "P": "0.00000000",
  "F": "0.00000000",...
			  */
				JToken Token = JToken.Parse(msg.ToStringFromUTF8());
				JToken token = Token["data"];
				var eventType = token["e"].ToStringInvariant();
				if (eventType == "executionReport")
				{
					var update = JsonConvert.DeserializeObject<ExecutionReport>(token.ToStringInvariant());
					callback(update);
				}
				else if (eventType == "outboundAccountPosition")
				{
					var update = JsonConvert.DeserializeObject<OutboundAccount>(token.ToStringInvariant());
					callback(update);
				}
				else if (eventType == "listStatus")
				{
					var update = JsonConvert.DeserializeObject<ListStatus>(token.ToStringInvariant());
					callback(update);
				}
				return Task.CompletedTask;
			});
		}		
		public virtual async Task<string> GetListenKeyAsync()
		{
			JToken response = await MakeJsonRequestAsync<JToken>("/userDataStream", BaseUrl, null, "POST");
			var listenKey = response["listenKey"].ToStringInvariant();
			return listenKey;
		}
		protected override async Task<IWebSocket> OnUserDataWebSocketAsync(Action<object> callback, string listenKey)
		{
			return await ConnectWebSocketAsync($"{BaseUrlWebSocket}/stream?streams={listenKey}", (_socket, msg) =>
			{/*
			  token = {{
  "stream": "BTtVnEtw13TrgIke6TpdT1NRAjsYYlGFKfuWtWBAXA1znx8QiPE8nhc0RYSq",
  "data": {First = {{
  "e": "executionReport",
  "E": 1624068030726,
  "s": "1INCHUSDT",
  "c": "web_b62f88d4c2244f33a6365d445f1f2a8d",
  "S": "BUY",
  "o": "LIMIT",
  "f": "GTC",
  "q": "10.00000000",
  "p": "3.19000000",
  "P": "0.00000000",
  "F": "0.00000000",...
			  */
				JToken Token = JToken.Parse(msg.ToStringFromUTF8());
				JToken token = Token["data"];
				var eventType = token["e"].ToStringInvariant();
				if (eventType == "executionReport")
				{
					var update = JsonConvert.DeserializeObject<ExecutionReport>(token.ToStringInvariant());
					callback(update);
				}
				else if (eventType == "outboundAccountPosition")
				{
					var update = JsonConvert.DeserializeObject<OutboundAccount>(token.ToStringInvariant());
					callback(update);
				}
				else if (eventType == "listStatus")
				{
					var update = JsonConvert.DeserializeObject<ListStatus>(token.ToStringInvariant());
					callback(update);
				}
				return Task.CompletedTask;
			});
		}
	}
}
