/*
MIT LICENSE

Copyright 2020 Digital Ruby, LLC - http://www.digitalruby.com

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
/*
namespace ExchangeSharp
{
	public sealed partial class ExchangeBybitAPI : ExchangeAPI
	{
		private int _recvWindow = 30000;

		public override string BaseUrl { get; set; } = "https://api.bybit.com";
		public override string BaseUrlWebSocket { get; set; } = "wss://stream.bybit.com/realtime";
		// public override string BaseUrl { get; set; } = "https://api-testnet.bybit.com/";
		// public override string BaseUrlWebSocket { get; set; } = "wss://stream-testnet.bybit.com/realtime";

		private ExchangeBybitAPI()
		{
			NonceStyle = NonceStyle.UnixMilliseconds;
			NonceOffset = TimeSpan.FromSeconds(1.0);

			MarketSymbolSeparator = string.Empty;
			RequestContentType = "application/json";
			WebSocketOrderBookType = WebSocketOrderBookType.FullBookFirstThenDeltas;

			RateLimit = new RateGate(500, TimeSpan.FromMinutes(1));
		}

		public override Task<string> ExchangeMarketSymbolToGlobalMarketSymbolAsync(string marketSymbol)
		{
			throw new NotImplementedException();
		}

		public override Task<string> GlobalMarketSymbolToExchangeMarketSymbolAsync(string marketSymbol)
		{
			throw new NotImplementedException();
		}

		// Was initially struggling with 10002 timestamp errors, so tried calcing clock drift on every request.
		// Settled on positive NonceOffset so our clock is not likely ahead of theirs on arrival (assuming accurate client/server side clocks)
		// And larger recv_window so our packets have plenty of time to arrive
		// protected override async Task OnGetNonceOffset()
		// {
		//     string stringResult = await MakeRequestAsync("/v2/public/time");
		//     var token  = JsonConvert.DeserializeObject<JToken>(stringResult);
		//     DateTime serverDate = CryptoUtility.UnixTimeStampToDateTimeSeconds(token["time_now"].ConvertInvariant<Double>());
		//     var now = CryptoUtility.UtcNow;
		//     NonceOffset = now - serverDate + TimeSpan.FromSeconds(1); // how much time to substract from Nonce when making a request
		// }

		protected override async Task ProcessRequestAsync(IHttpWebRequest request, Dictionary<string, object> payload)
		{
			if ((payload != null) && payload.ContainsKey("sign") && request.Method == "POST")
			{
				await CryptoUtility.WritePayloadJsonToRequestAsync(request, payload);
			}
		}

#nullable enable
		//Not using MakeJsonRequest... so we can perform our own check on the ret_code 
		private async Task<T> DoMakeJsonRequestAsync<T>(string url, string? baseUrl = null, Dictionary<string, object>? payload = null, string? requestMethod = null)
		{
			await new SynchronizationContextRemover();

			string stringResult = await MakeRequestAsync(url, baseUrl, payload, requestMethod);
			return JsonConvert.DeserializeObject<T>(stringResult);
		}
#nullable disable

		private JToken CheckRetCode(JToken response, string[] allowedRetCodes)
		{
			var result = GetResult(response, out var retCode, out var retMessage);
			if (!allowedRetCodes.Contains(retCode))
			{
				throw new Exception($"Invalid ret_code {retCode}, ret_msg {retMessage}");
			}
			return result;
		}

		private JToken CheckRetCode(JToken response)
		{
			return CheckRetCode(response, new string[] { "0" });
		}

		private JToken GetResult(JToken response, out string retCode, out string retMessage)
		{
			retCode = response["ret_code"].ToStringInvariant();
			retMessage = response["ret_msg"].ToStringInvariant();
			return response["result"];
		}

		private async Task SendWebsocketAuth(IWebSocket socket)
		{
			var payload = await GetNoncePayloadAsync();
			var nonce = (payload["nonce"].ConvertInvariant<long>() + 5000).ToStringInvariant();
			var signature = CryptoUtility.SHA256Sign($"GET/realtime{nonce}", CryptoUtility.ToUnsecureBytesUTF8(PrivateApiKey));
			await socket.SendMessageAsync(new { op = "auth", args = new[] { PublicApiKey.ToUnsecureString(), nonce, signature } });
		}

		private async Task<Dictionary<string, object>> GetAuthenticatedPayload(Dictionary<string, object> requestPayload = null)
		{
			var payload = await GetNoncePayloadAsync();
			var nonce = payload["nonce"].ConvertInvariant<long>();
			payload.Remove("nonce");
			payload["api_key"] = PublicApiKey.ToUnsecureString();
			payload["timestamp"] = nonce.ToStringInvariant();
			payload["recv_window"] = _recvWindow;
			if (requestPayload != null)
			{
				payload = payload.Concat(requestPayload).ToDictionary(p => p.Key, p => p.Value);
			}

			string form = CryptoUtility.GetFormForPayload(payload, false, true);
			form = form.Replace("=False", "=false");
			form = form.Replace("=True", "=true");
			payload["sign"] = CryptoUtility.SHA256Sign(form, CryptoUtility.ToUnsecureBytesUTF8(PrivateApiKey));
			return payload;
		}

		private async Task<string> GetAuthenticatedQueryString(Dictionary<string, object> requestPayload = null)
		{
			var payload = await GetAuthenticatedPayload(requestPayload);
			var sign = payload["sign"].ToStringInvariant();
			payload.Remove("sign");
			string form = CryptoUtility.GetFormForPayload(payload, false, true);
			form += "&sign=" + sign;
			return form;
		}

		private Task<IWebSocket> DoConnectWebSocketAsync(Func<IWebSocket, Task> connected, Func<IWebSocket, JToken, Task> callback, int symbolArrayIndex = 3)
		{
			Timer pingTimer = null;
			return ConnectPublicWebSocketAsync(url: string.Empty, messageCallback: async (_socket, msg) =>
			{
				var msgString = msg.ToStringFromUTF8();
				JToken token = JToken.Parse(msgString);

				if (token["ret_msg"]?.ToStringInvariant() == "pong")
				{ // received reply to our ping
					return;
				}

				if (token["topic"] != null)
				{
					var data = token["data"];
					await callback(_socket, data);
				}
				else
				{
					/*
					subscription response:
					{
						"success": true, // Whether subscription is successful
						"ret_msg": "",   // Successful subscription: "", otherwise it shows error message
						"conn_id":"e0e10eee-4eff-4d21-881e-a0c55c25e2da",// current connection id
						"request": {     // Request to your subscription
							"op": "subscribe",
							"args": [
								"kline.BTCUSD.1m"
							]
						}
					}
					
					JToken response = token["request"];
					var op = response["op"]?.ToStringInvariant();
					if ((response != null) && ((op == "subscribe") || (op == "auth")))
					{
						var responseMessage = token["ret_msg"]?.ToStringInvariant();
						if (responseMessage != "")
						{
							Logger.Info("Websocket unable to connect: " + msgString);
							return;
						}
						else if (pingTimer == null)
						{
							/*
							ping response:
							{
								"success": true, // Whether ping is successful
								"ret_msg": "pong",
								"conn_id": "036e5d21-804c-4447-a92d-b65a44d00700",// current connection id
								"request": {
									"op": "ping",
									"args": null
								}
							}
							
							pingTimer = new Timer(callback: async s => await _socket.SendMessageAsync(new { op = "ping" }),
								state: null, dueTime: 0, period: 15000); // send a ping every 15 seconds
							return;
						}
					}
				}
			},
			connectCallback: async (_socket) =>
			{
				await connected(_socket);
				_socket.ConnectInterval = TimeSpan.FromHours(0);
			},
			disconnectCallback: s =>
			{
				pingTimer.Dispose();
				pingTimer = null;
				return Task.CompletedTask;
			});
		}

		private async Task AddMarketSymbolsToChannel(IWebSocket socket, string argsPrefix, string[] marketSymbols)
		{
			string fullArgs = argsPrefix;
			if (marketSymbols == null || marketSymbols.Length == 0)
			{
				fullArgs += "*";
			}
			else
			{
				foreach (var symbol in marketSymbols)
				{
					fullArgs += symbol + "|";
				}
				fullArgs = fullArgs.TrimEnd('|');
			}

			await socket.SendMessageAsync(new { op = "subscribe", args = new[] { fullArgs } });
		}

		protected override async Task<IWebSocket> OnGetTradesWebSocketAsync(Func<KeyValuePair<string, ExchangeTrade>, Task> callback, params string[] marketSymbols)
		{
			/*
			request:
			{"op":"subscribe","args":["trade.BTCUSD|XRPUSD"]}
			*/
			/*
				response:
				{
					"topic": "trade.BTCUSD",
					"data": [
						{
							"timestamp": "2020-01-12T16:59:59.000Z",
							"trade_time_ms": 1582793344685, // trade time in millisecond
							"symbol": "BTCUSD",
							"side": "Sell",
							"size": 328,
							"price": 8098,
							"tick_direction": "MinusTick",
							"trade_id": "00c706e1-ba52-5bb0-98d0-bf694bdc69f7",
							"cross_seq": 1052816407
						}
					]
				}
			 
			return await DoConnectWebSocketAsync(async (_socket) =>
			{
				await AddMarketSymbolsToChannel(_socket, "trade.", marketSymbols);
			}, async (_socket, token) =>
			{
				foreach (var dataRow in token)
				{
					ExchangeTrade trade = dataRow.ParseTrade(
						amountKey: "size",
						priceKey: "price",
						typeKey: "side",
						timestampKey: "timestamp",
						timestampType: TimestampType.Iso8601,
						idKey: "trade_id");
					await callback(new KeyValuePair<string, ExchangeTrade>(dataRow["symbol"].ToStringInvariant(), trade));
				}
			});
		}
		/*public async Task<IWebSocket> GetPositionWebSocketAsync(Action<GeneralPosition> callbackposition, Action<GeneralAccount> callbackaccount)
		{
			
			return await DoConnectWebSocketAsync(async (_socket) =>
			{
				await SendWebsocketAuth(_socket);
				await _socket.SendMessageAsync(new { op = "subscribe", args = new[] { "position" } });
			}, async (_socket, token) =>
			{
				foreach (var dataRow in token)
				{
					callbackposition(ParsePosition(dataRow));
				}
				callbackaccount(ParseAccount(token));
				await Task.CompletedTask;
			});
		}
		*/
		/*protected override async Task<IWebSocket> OnGetPositionsWebSocketAsync(Action<ExchangePosition> callback)
		{			
			return await DoConnectWebSocketAsync(async (_socket) =>
			{
				await SendWebsocketAuth(_socket);
				await _socket.SendMessageAsync(new { op = "subscribe", args = new[] { "position" } });
			}, async (_socket, token) =>
			{
				foreach (var dataRow in token)
				{
					callback(ParsePosition(dataRow));
				}
				await Task.CompletedTask;
			});
		}

		protected override async Task<IEnumerable<string>> OnGetMarketSymbolsAsync()
		{
			var m = await GetMarketSymbolsMetadataAsync();
			return m.Select(x => x.MarketSymbol);
		}

		protected internal override async Task<IEnumerable<ExchangeMarket>> OnGetMarketSymbolsMetadataAsync()
		{
			/*
			{
			"ret_code": 0,
			"ret_msg": "OK",
			"ext_code": "",
			"ext_info": "",
			"result": [
				{
				"name": "BTCUSD",
				"base_currency": "BTC",
				"quote_currency": "USD",
				"price_scale": 2,
				"taker_fee": "0.00075",
				"maker_fee": "-0.00025",
				"leverage_filter": {
					"min_leverage": 1,
					"max_leverage": 100,
					"leverage_step": "0.01"
				},
				"price_filter": {
					"min_price": "0.5",
					"max_price": "999999.5",
					"tick_size": "0.5"
				},
				"lot_size_filter": {
					"max_trading_qty": 1000000,
					"min_trading_qty": 1,
					"qty_step": 1
				}
				},
				{
				"name": "ETHUSD",
				"base_currency": "ETH",
				"quote_currency": "USD",
				"price_scale": 2,
				"taker_fee": "0.00075",
				"maker_fee": "-0.00025",
				"leverage_filter": {
					"min_leverage": 1,
					"max_leverage": 50,
					"leverage_step": "0.01"
				},
				"price_filter": {
					"min_price": "0.05",
					"max_price": "99999.95",
					"tick_size": "0.05"
				},
				"lot_size_filter": {
					"max_trading_qty": 1000000,
					"min_trading_qty": 1,
					"qty_step": 1
				}
				},
				{
				"name": "EOSUSD",
				"base_currency": "EOS",
				"quote_currency": "USD",
				"price_scale": 3,
				"taker_fee": "0.00075",
				"maker_fee": "-0.00025",
				"leverage_filter": {
					"min_leverage": 1,
					"max_leverage": 50,
					"leverage_step": "0.01"
				},
				"price_filter": {
					"min_price": "0.001",
					"max_price": "1999.999",
					"tick_size": "0.001"
				},
				"lot_size_filter": {
					"max_trading_qty": 1000000,
					"min_trading_qty": 1,
					"qty_step": 1
				}
				},
				{
				"name": "XRPUSD",
				"base_currency": "XRP",
				"quote_currency": "USD",
				"price_scale": 4,
				"taker_fee": "0.00075",
				"maker_fee": "-0.00025",
				"leverage_filter": {
					"min_leverage": 1,
					"max_leverage": 50,
					"leverage_step": "0.01"
				},
				"price_filter": {
					"min_price": "0.0001",
					"max_price": "199.9999",
					"tick_size": "0.0001"
				},
				"lot_size_filter": {
					"max_trading_qty": 1000000,
					"min_trading_qty": 1,
					"qty_step": 1
				}
				}
			],
			"time_now": "1581411225.414179"
			}}
			 

			List<ExchangeMarket> markets = new List<ExchangeMarket>();
			JToken allSymbols = CheckRetCode(await DoMakeJsonRequestAsync<JToken>("/v2/public/symbols"));
			foreach (JToken marketSymbolToken in allSymbols)
			{
				var market = new ExchangeMarket
				{
					MarketSymbol = marketSymbolToken["name"].ToStringUpperInvariant(),
					IsActive = true,
					QuoteCurrency = marketSymbolToken["quote_currency"].ToStringUpperInvariant(),
					BaseCurrency = marketSymbolToken["base_currency"].ToStringUpperInvariant(),
				};

				try
				{
					JToken priceFilter = marketSymbolToken["price_filter"];
					market.MinPrice = priceFilter["min_price"].ConvertInvariant<decimal>();
					market.MaxPrice = priceFilter["max_price"].ConvertInvariant<decimal>();
					market.PriceStepSize = priceFilter["tick_size"].ConvertInvariant<decimal>();

					JToken lotSizeFilter = marketSymbolToken["lot_size_filter"];
					market.MinTradeSize = lotSizeFilter["min_trading_qty"].ConvertInvariant<decimal>();
					market.MaxTradeSize = lotSizeFilter["max_trading_qty"].ConvertInvariant<decimal>();
					market.QuantityStepSize = lotSizeFilter["qty_step"].ConvertInvariant<decimal>();
				}
				catch
				{

				}
				markets.Add(market);
			}
			return markets;
		}


		private async Task<Dictionary<string, decimal>> DoGetAmountsAsync(string field)
		{
			/*
			{
				"ret_code": 0,
				"ret_msg": "OK",
				"ext_code": "",
				"ext_info": "",
				"result": {
					"BTC": {
						"equity": 1002,                         //equity = wallet_balance + unrealised_pnl
						"available_balance": 999.99987471,      //available_balance
						//In Isolated Margin Mode:
						// available_balance = wallet_balance - (position_margin + occ_closing_fee + occ_funding_fee + order_margin)
						//In Cross Margin Mode:
						//if unrealised_pnl > 0:
						//available_balance = wallet_balance - (position_margin + occ_closing_fee + occ_funding_fee + order_margin)；
						//if unrealised_pnl < 0:
						//available_balance = wallet_balance - (position_margin + occ_closing_fee + occ_funding_fee + order_margin) + unrealised_pnl
						"used_margin": 0.00012529,              //used_margin = wallet_balance - available_balance
						"order_margin": 0.00012529,             //Used margin by order
						"position_margin": 0,                   //position margin
						"occ_closing_fee": 0,                   //position closing fee
						"occ_funding_fee": 0,                   //funding fee
						"wallet_balance": 1000,                 //wallet balance. When in Cross Margin mod, the number minus your unclosed loss is your real wallet balance.
						"realised_pnl": 0,                      //daily realized profit and loss
						"unrealised_pnl": 2,                    //unrealised profit and loss
							//when side is sell:
							// unrealised_pnl = size * (1.0 / mark_price -  1.0 / entry_price）
							//when side is buy:
							// unrealised_pnl = size * (1.0 / entry_price -  1.0 / mark_price）
						"cum_realised_pnl": 0,                  //total relised profit and loss
						"given_cash": 0,                        //given_cash
						"service_cash": 0                       //service_cash
					}
				},
				"time_now": "1578284274.816029",
				"rate_limit_status": 98,
				"rate_limit_reset_ms": 1580885703683,
				"rate_limit": 100
			}
			
			Dictionary<string, decimal> amounts = new Dictionary<string, decimal>();
			var queryString = await GetAuthenticatedQueryString();
			JToken currencies = CheckRetCode(await DoMakeJsonRequestAsync<JToken>($"/v2/private/wallet/balance?" + queryString, BaseUrl, null, "GET"));
			foreach (JProperty currency in currencies.Children<JProperty>())
			{
				var balance = currency.Value[field].ConvertInvariant<decimal>();
				if (amounts.ContainsKey(currency.Name))
				{
					amounts[currency.Name] += balance;
				}
				else
				{
					amounts[currency.Name] = balance;
				}
			}
			return amounts;
		}

		protected override async Task<Dictionary<string, decimal>> OnGetAmountsAsync()
		{
			return await DoGetAmountsAsync("equity");
		}
		public async Task<IWebSocket> GetPositionWebSocketAsync(Action<GeneralPosition> callbackposition, Action<GeneralAccount> callbackaccount)
		{
			
			return await DoConnectWebSocketAsync(async (_socket) =>
			{
				await SendWebsocketAuth(_socket);
				await _socket.SendMessageAsync(new { op = "subscribe", args = new[] { "position" } });
			}, async (_socket, token) =>
			{
				foreach (var dataRow in token)
				{
					callbackposition(ParsePosition(dataRow));
				}
				callbackaccount(ParseAccount(token));
				await Task.CompletedTask;
			});
		}
/*private GeneralPosition ParsePosition(JToken token)
{
	/*
	"id": 27913,
	"user_id": 1,
	"risk_id": 1,
	"symbol": "BTCUSD",
	"side": "Buy",
	"size": 5,
	"position_value": "0.0006947",
	"entry_price": "7197.35137469",
	"is_isolated":true,
	"auto_add_margin": 0,
	"leverage": "1",  //In Isolated Margin mode, the value is set by user. In Cross Margin mode, the value is the max leverage at current risk level
	"effective_leverage": "1", // Effective Leverage. In Isolated Margin mode, its value equals `leverage`; In Cross Margin mode, The formula to calculate:
		effective_leverage = position size / mark_price / (wallet_balance + unrealised_pnl)
	"position_margin": "0.0006947",
	"liq_price": "3608",
	"bust_price": "3599",
	"occ_closing_fee": "0.00000105",
	"occ_funding_fee": "0",
	"take_profit": "0",
	"stop_loss": "0",
	"trailing_stop": "0",
	"position_status": "Normal",
	"deleverage_indicator": 4,
	"oc_calc_data": "{\"blq\":2,\"blv\":\"0.0002941\",\"slq\":0,\"bmp\":6800.408,\"smp\":0,\"fq\":-5,\"fc\":-0.00029477,\"bv2c\":1.00225,\"sv2c\":1.0007575}",
	"order_margin": "0.00029477",
	"wallet_balance": "0.03000227",
	"realised_pnl": "-0.00000126",
	"unrealised_pnl": 0,
	"cum_realised_pnl": "-0.00001306",
	"cross_seq": 444081383,
	"position_seq": 287141589,
	"created_at": "2019-10-19T17:04:55Z",
	"updated_at": "2019-12-27T20:25:45.158767Z

	GeneralPosition result = new GeneralPosition
	{
		Symbol = token["symbol"].ToStringUpperInvariant(),
		Position = token["size"].ConvertInvariant<decimal>(),
		AvgCost = token["entry_price"].ConvertInvariant<decimal>(),
		PositionMargin = token["position_margin"].ConvertInvariant<decimal>(),
		OrderMargin = token["order_margin"].ConvertInvariant<decimal>(),
		RealizedPnl = token["realised_pnl"].ConvertInvariant<decimal>(),
		//UnrealizedPnl = token["unrealise_pnl"].ConvertInvariant<decimal>(),
		LiquidationPrice = token["liq_price"].ConvertInvariant<decimal>(),
		Leverage = token["leverage"].ConvertInvariant<int>(),
		IsolatedOrCross = token["Isolated"].ConvertInvariant<bool>(),
		UpdateTime = CryptoUtility.ParseTimestamp(token["updated_at"], TimestampType.Iso8601),
		UserID = token["user_id"].ToStringInvariant(),
	};
	if (token["side"].ToStringInvariant() == "Sell")
		result.Position *= -1;
	return result;
}
private GeneralAccount ParseAccount(JToken data)
{
	GeneralAccount ga = new GeneralAccount();
	foreach (var Row in data)
	{
		ga.RealizedPNL += Row["cum_realised_pnl"].ConvertInvariant<decimal>();
	}
	var dataRow = data.Last();
	ga.UserID = dataRow["user_id"].ToStringInvariant();
	ga.AccountType = "Future";
	ga.ExchangeID = "Bybit";
	ga.Available = dataRow["available_balance"].ConvertInvariant<decimal>();
	ga.Equity = dataRow["wallet_balance"].ConvertInvariant<decimal>();
	ga.OrderMargin = dataRow["order_margin"].ConvertInvariant<decimal>();
	ga.PositionMargin = dataRow["position_margin"].ConvertInvariant<decimal>();
	ga.Symbol = dataRow["symbol"].ToStringInvariant().Substring(0, 3);
	ga.UpdateTime = DateTime.UtcNow;
	return ga;
}
protected override async Task<Dictionary<string, decimal>> OnGetAmountsAvailableToTradeAsync()
{
	return await DoGetAmountsAsync("available_balance");
}
protected override async Task<ExchangeOrderBook> OnGetOrderBookAsync(string marketSymbol, int maxCount = 100)
{
	/*
	{
		"ret_code": 0,                              // return code
		"ret_msg": "OK",                            // error message
		"ext_code": "",                             // additional error code
		"ext_info": "",                             // additional error info
		"result": [
			{
				"symbol": "BTCUSD",                 // symbol
				"price": "9487",                    // price
				"size": 336241,                     // size (in USD contracts)
				"side": "Buy"                       // side
			},
			{
				"symbol": "BTCUSD",                 // symbol
				"price": "9487.5",                  // price
				"size": 522147,                     // size (in USD contracts)
				"side": "Sell"                      // side
			}
		],
		"time_now": "1567108756.834357"             // UTC timestamp
	}

	var tokens = CheckRetCode(await DoMakeJsonRequestAsync<JToken>($"/v2/public/orderBook/L2?symbol={marketSymbol}"));
	var orderBook = new ExchangeOrderBook();
	foreach (var token in tokens)
	{
		var orderPrice = new ExchangeOrderPrice();
		orderPrice.Price = token["price"].ConvertInvariant<decimal>();
		orderPrice.Amount = token["size"].ConvertInvariant<decimal>();
		if (token["side"].ToStringInvariant() == "Sell")
			orderBook.Asks.Add(orderPrice.Price, orderPrice);
		else
			orderBook.Bids.Add(orderPrice.Price, orderPrice);
	}

	return orderBook;
}

public async Task<IEnumerable<GeneralPosition>> GetCurrentPositionsAsync()
{
	/*
	{
		"ret_code": 0,
		"ret_msg": "OK",
		"ext_code": "",
		"ext_info": "",
		"result": {
			"id": 27913,
			"user_id": 1,
			"risk_id": 1,
			"symbol": "BTCUSD",
			"side": "Buy",
			"size": 5,
			"position_value": "0.0006947",
			"entry_price": "7197.35137469",
			"is_isolated":true,
			"auto_add_margin": 0,
			"leverage": "1",  //In Isolated Margin mode, the value is set by user. In Cross Margin mode, the value is the max leverage at current risk level
			"effective_leverage": "1", // Effective Leverage. In Isolated Margin mode, its value equals `leverage`; In Cross Margin mode, The formula to calculate:
				effective_leverage = position size / mark_price / (wallet_balance + unrealised_pnl)
			"position_margin": "0.0006947",
			"liq_price": "3608",
			"bust_price": "3599",
			"occ_closing_fee": "0.00000105",
			"occ_funding_fee": "0",
			"take_profit": "0",
			"stop_loss": "0",
			"trailing_stop": "0",
			"position_status": "Normal",
			"deleverage_indicator": 4,
			"oc_calc_data": "{\"blq\":2,\"blv\":\"0.0002941\",\"slq\":0,\"bmp\":6800.408,\"smp\":0,\"fq\":-5,\"fc\":-0.00029477,\"bv2c\":1.00225,\"sv2c\":1.0007575}",
			"order_margin": "0.00029477",
			"wallet_balance": "0.03000227",
			"realised_pnl": "-0.00000126",
			"unrealised_pnl": 0,
			"cum_realised_pnl": "-0.00001306",
			"cross_seq": 444081383,
			"position_seq": 287141589,
			"created_at": "2019-10-19T17:04:55Z",
			"updated_at": "2019-12-27T20:25:45.158767Z"
		},
		"time_now": "1577480599.097287",
		"rate_limit_status": 119,
		"rate_limit_reset_ms": 1580885703683,
		"rate_limit": 120
	}

	var queryString = await GetAuthenticatedQueryString();
	JToken token = CheckRetCode(await DoMakeJsonRequestAsync<JToken>($"/v2/private/position/list?" + queryString, BaseUrl, null, "GET"));
	List<GeneralPosition> positions = new List<GeneralPosition>();
	foreach (var item in token)
	{
		positions.Add(ParsePosition(item["data"]));
	}
	return positions;
}

public async Task<ExchangeFunding> GetCurrentFundingRateAsync(string marketSymbol)
{
	/*
	{
		"ret_code": 0,
		"ret_msg": "ok",
		"ext_code": "",
		"result": {
			"symbol": "BTCUSD",
			"funding_rate": "0.00010000",
			"funding_rate_timestamp": 1577433600
		},
		"ext_info": null,
		"time_now": "1577445586.446797",
		"rate_limit_status": 119,
		"rate_limit_reset_ms": 1577445586454,
		"rate_limit": 120
	}

	JToken token = CheckRetCode(await DoMakeJsonRequestAsync<JToken>($"/v2/public/funding/prev-funding-rate?symbol={marketSymbol}"));
	var funding = new ExchangeFunding();
	funding.MarketSymbol = token["symbol"].ToStringInvariant();
	funding.Rate = token["funding_rate"].ConvertInvariant<decimal>();
	// funding.TimeStamp = Convert.ToDateTime(TimeSpan.FromSeconds(token["funding_rate_timestamp"].ConvertInvariant<int>()));
	funding.TimeStamp = CryptoUtility.UnixTimeStampToDateTimeSeconds(token["funding_rate_timestamp"].ConvertInvariant<int>());

	return funding;
}

public async Task<ExchangeFunding> GetPredictedFundingRateAsync(string marketSymbol)
{
	/*
	{
		"ret_code": 0,
		"ret_msg": "ok",
		"ext_code": "",
		"result": {
			"predicted_funding_rate": 0.0001,
			"predicted_funding_fee": 0
		},
		"ext_info": null,
		"time_now": "1577447415.583259",
		"rate_limit_status": 118,
		"rate_limit_reset_ms": 1577447415590,
		"rate_limit": 120
	}

	var extraParams = new Dictionary<string, object>();
	extraParams["symbol"] = marketSymbol;
	var queryString = await GetAuthenticatedQueryString(extraParams);
	JToken token = CheckRetCode(await DoMakeJsonRequestAsync<JToken>($"/v2/private/funding/predicted-funding?" + queryString, BaseUrl, null, "GET"));
	var funding = new ExchangeFunding();
	funding.MarketSymbol = marketSymbol;
	funding.Rate = token["predicted_funding_rate"].ConvertInvariant<decimal>();

	return funding;
}
protected override async Task<IEnumerable<KeyValuePair<string, ExchangeTicker>>> OnGetTickersAsync()
{
	var s = await DoMakeJsonRequestAsync<JToken>("/v2/public/tickers", BaseUrl, null, "GET");
	return ParseTickers(s);
}

private IEnumerable<KeyValuePair<string, ExchangeTicker>> ParseTickers(JToken s)
{
	List<KeyValuePair<string, ExchangeTicker>> v = new List<KeyValuePair<string, ExchangeTicker>>();
	/*Root = {{
"ret_code": 0,
"ret_msg": "OK",
"ext_code": "",
"ext_info": "",
"result": [    
"symbol": "BTCUSD",
"bid_price": "36797",
"ask_price": "36797.5",
"last_price": "36797.00",
"last_tick_direction": "ZeroMinusTick",
"prev_price_24h": "36374.50",
"price_24h_pcnt": "0.011615",
"high_price_24h": "38477.00",
"low_...

	foreach (var t in s["result"].ConvertInvariant<JArray>())
	{
		var r = new ExchangeTicker();
		r.MarketSymbol = t["symbol"].ToString();
		r.Bid = t["bid_price"].ConvertInvariant<decimal>();
		r.Ask = t["ask_price"].ConvertInvariant<decimal>();
		r.Last = t["last_price"].ConvertInvariant<decimal>();
		r.Volume = new ExchangeVolume() { BaseCurrencyVolume = t["total_volume"].ConvertInvariant<decimal>() };
		v.Add(new KeyValuePair<string, ExchangeTicker>(r.MarketSymbol, r));
	}
	return v;
}
private async Task<IEnumerable<ExchangeOrderResult>> DoGetOrderDetailsAsync(string orderId, bool isClientOrderId = false, string marketSymbol = null)
{
	var extraParams = new Dictionary<string, object>();

	if (orderId != null)
	{
		if (isClientOrderId)
			extraParams["order_link_id"] = orderId;
		else
			extraParams["order_id"] = orderId;
	}

	if (!string.IsNullOrWhiteSpace(marketSymbol))
	{
		extraParams["symbol"] = marketSymbol;
	}
	else
	{
		throw new Exception("marketSymbol is required");
	}

	var queryString = await GetAuthenticatedQueryString(extraParams);
	JToken token = GetResult(await DoMakeJsonRequestAsync<JToken>($"/v2/private/order?" + queryString, BaseUrl, null, "GET"), out var retCode, out var retMessage);

	List<ExchangeOrderResult> orders = new List<ExchangeOrderResult>();
	if (orderId == null)
	{
		foreach (JToken order in token)
		{
			orders.Add(ParseOrder(order, retCode, retMessage));
		}
	}
	else
	{
		orders.Add(ParseOrder(token, retCode, retMessage));
	}

	return orders;
}

//Note, Bybit is not recommending the use of "/v2/private/order/list" now that "/v2/private/order" is capable of returning multiple results
protected override async Task<IEnumerable<ExchangeOrderResult>> OnGetOpenOrderDetailsAsync(string marketSymbol = null)
{
	var orders = await DoGetOrderDetailsAsync(null, isClientOrderId: false, marketSymbol: marketSymbol);
	return orders;
}

protected override async Task<ExchangeOrderResult> OnGetOrderDetailsAsync(string orderId, string marketSymbol = null, bool isClientOrderId = false)
{
	var orders = await DoGetOrderDetailsAsync(orderId, isClientOrderId: isClientOrderId, marketSymbol: marketSymbol);
	if (orders.Count() > 0)
	{
		return orders.First();
	}
	else
	{
		return null;
	}
}

protected override async Task OnCancelOrderAsync(string orderId, string marketSymbol = null)
{
	var extraParams = new Dictionary<string, object>();
	extraParams["order_id"] = orderId;
	if (!string.IsNullOrWhiteSpace(marketSymbol))
	{
		extraParams["symbol"] = marketSymbol;
	}
	else
	{
		throw new Exception("marketSymbol is required");
	}

	var payload = await GetAuthenticatedPayload(extraParams);
	CheckRetCode(await DoMakeJsonRequestAsync<JToken>($"/v2/private/order/cancel", BaseUrl, payload, "POST"));
	// new string[] {"0", "30032"});
	//30032: order has been finished or canceled
}

public async Task CancelAllOrdersAsync(string marketSymbol)
{
	var extraParams = new Dictionary<string, object>();
	extraParams["symbol"] = marketSymbol;
	var payload = await GetAuthenticatedPayload(extraParams);
	CheckRetCode(await DoMakeJsonRequestAsync<JToken>($"/v2/private/order/cancelAll", BaseUrl, payload, "POST"));
}

protected override async Task<ExchangeOrderResult> OnPlaceOrderAsync(ExchangeOrderRequest order)
{
	var payload = new Dictionary<string, object>();
	await AddOrderToPayload(order, payload);
	payload = await GetAuthenticatedPayload(payload);
	JToken token = GetResult(await DoMakeJsonRequestAsync<JToken>("/v2/private/order/create", BaseUrl, payload, "POST"), out var retCode, out var retMessage);
	return ParseOrder(token, retCode, retMessage);
}

public async Task<ExchangeOrderResult> OnAmendOrderAsync(ExchangeOrderRequest order)
{
	if (order.IsPostOnly != null) throw new NotImplementedException("Post Only orders are not supported by this exchange or not implemented in ExchangeSharp. Please submit a PR if you are interested in this feature.");
	var payload = new Dictionary<string, object>();
	payload["symbol"] = order.MarketSymbol;
	if (order.OrderId != null)
		payload["order_id"] = order.OrderId;
	else if (order.ClientOrderId != null)
		payload["order_link_id"] = order.ClientOrderId;
	else
		throw new Exception("Need either OrderId or ClientOrderId");

	payload["p_r_qty"] = (long)await ClampOrderQuantity(order.MarketSymbol, order.Amount);
	if (order.OrderType != OrderType.Market)
		payload["p_r_price"] = order.Price;

	payload = await GetAuthenticatedPayload(payload);
	JToken token = GetResult(await DoMakeJsonRequestAsync<JToken>("/v2/private/order/replace", BaseUrl, payload, "POST"), out var retCode, out var retMessage);

	var result = new ExchangeOrderResult();
	result.ResultCode = retCode;
	result.Message = retMessage;
	if (retCode == "0")
		result.OrderId = token["order_id"].ToStringInvariant();
	return result;
}

private async Task AddOrderToPayload(ExchangeOrderRequest order, Dictionary<string, object> payload)
{
	/*
	side	true	string	Side
	symbol	true	string	Symbol
	order_type	true	string	Active order type
	qty	true	integer	Order quantity in USD
	price	false	number	Order price
	time_in_force	true	string	Time in force
	take_profit	false	number	Take profit price, only take effect upon opening the position
	stop_loss	false	number	Stop loss price, only take effect upon opening the position
	reduce_only	false	bool	What is a reduce-only order? True means your position can only reduce in size if this order is triggered
	close_on_trigger	false	bool	What is a close on trigger order? For a closing order. It can only reduce your position, not increase it. If the account has insufficient available balance when the closing order is triggered, then other active orders of similar contracts will be cancelled or reduced. It can be used to ensure your stop loss reduces your position regardless of current available margin.
	order_link_id	false	string	Customised order ID, maximum length at 36 characters, and order ID under the same agency has to be unique.


	payload["side"] = order.IsBuy ? "Buy" : "Sell";
	payload["symbol"] = order.MarketSymbol;
	payload["order_type"] = order.OrderType.ToStringInvariant();
	payload["qty"] = await ClampOrderQuantity(order.MarketSymbol, order.Amount);

	if (order.OrderType != OrderType.Market)
		payload["price"] = order.Price;

	if (order.ClientOrderId != null)
		payload["order_link_id"] = order.ClientOrderId;

	if (order.ExtraParameters.TryGetValue("reduce_only", out var reduceOnly))
	{
		payload["reduce_only"] = reduceOnly;
	}

	if (order.ExtraParameters.TryGetValue("time_in_force", out var timeInForce))
	{
		payload["time_in_force"] = timeInForce;
	}
	else
	{
		payload["time_in_force"] = "GoodTillCancel";
	}
}

private GeneralPosition ParsePosition(JToken token)
{
	/*
	"id": 27913,
	"user_id": 1,
	"risk_id": 1,
	"symbol": "BTCUSD",
	"side": "Buy",
	"size": 5,
	"position_value": "0.0006947",
	"entry_price": "7197.35137469",
	"is_isolated":true,
	"auto_add_margin": 0,
	"leverage": "1",  //In Isolated Margin mode, the value is set by user. In Cross Margin mode, the value is the max leverage at current risk level
	"effective_leverage": "1", // Effective Leverage. In Isolated Margin mode, its value equals `leverage`; In Cross Margin mode, The formula to calculate:
		effective_leverage = position size / mark_price / (wallet_balance + unrealised_pnl)
	"position_margin": "0.0006947",
	"liq_price": "3608",
	"bust_price": "3599",
	"occ_closing_fee": "0.00000105",
	"occ_funding_fee": "0",
	"take_profit": "0",
	"stop_loss": "0",
	"trailing_stop": "0",
	"position_status": "Normal",
	"deleverage_indicator": 4,
	"oc_calc_data": "{\"blq\":2,\"blv\":\"0.0002941\",\"slq\":0,\"bmp\":6800.408,\"smp\":0,\"fq\":-5,\"fc\":-0.00029477,\"bv2c\":1.00225,\"sv2c\":1.0007575}",
	"order_margin": "0.00029477",
	"wallet_balance": "0.03000227",
	"realised_pnl": "-0.00000126",
	"unrealised_pnl": 0,
	"cum_realised_pnl": "-0.00001306",
	"cross_seq": 444081383,
	"position_seq": 287141589,
	"created_at": "2019-10-19T17:04:55Z",
	"updated_at": "2019-12-27T20:25:45.158767Z

	GeneralPosition result = new GeneralPosition
	{
		Symbol = token["symbol"].ToStringUpperInvariant(),
		Position = token["size"].ConvertInvariant<decimal>(),
		AvgCost = token["entry_price"].ConvertInvariant<decimal>(),
		PositionMargin = token["position_margin"].ConvertInvariant<decimal>(),
		OrderMargin = token["order_margin"].ConvertInvariant<decimal>(),
		RealizedPnl = token["realised_pnl"].ConvertInvariant<decimal>(),
		//UnrealizedPnl = token["unrealise_pnl"].ConvertInvariant<decimal>(),
		LiquidationPrice = token["liq_price"].ConvertInvariant<decimal>(),
		Leverage = token["leverage"].ConvertInvariant<int>(),
		IsolatedOrCross = token["Isolated"].ConvertInvariant<bool>(),
		UpdateTime = CryptoUtility.ParseTimestamp(token["updated_at"], TimestampType.Iso8601),
		UserID = token["user_id"].ToStringInvariant(),
	};
	if (token["side"].ToStringInvariant() == "Sell")
		result.Position *= -1;
	return result;
}

private ExchangeOrderResult ParseOrder(JToken token, string resultCode, string resultMessage)
{
	/*
	Active Order:
	{
	"ret_code": 0,
	"ret_msg": "OK",
	"ext_code": "",
	"ext_info": "",
	"result": {
		"user_id": 106958,
		"symbol": "BTCUSD",
		"side": "Buy",
		"order_type": "Limit",
		"price": "11756.5",
		"qty": 1,
		"time_in_force": "PostOnly",
		"order_status": "Filled",
		"ext_fields": {
			"o_req_num": -68948112492,
			"xreq_type": "x_create"
		},
		"last_exec_time": "1596304897.847944",
		"last_exec_price": "11756.5",
		"leaves_qty": 0,
		"leaves_value": "0",
		"cum_exec_qty": 1,
		"cum_exec_value": "0.00008505",
		"cum_exec_fee": "-0.00000002",
		"reject_reason": "",
		"cancel_type": "",
		"order_link_id": "",
		"created_at": "2020-08-01T18:00:26Z",
		"updated_at": "2020-08-01T18:01:37Z",
		"order_id": "e66b101a-ef3f-4647-83b5-28e0f38dcae0"
	},
	"time_now": "1597171013.867068",
	"rate_limit_status": 599,
	"rate_limit_reset_ms": 1597171013861,
	"rate_limit": 600
	}

	Active Order List:
	{
		"ret_code": 0,
		"ret_msg": "OK",
		"ext_code": "",
		"ext_info": "",
		"result": {
			"data": [ 
				{
					"user_id": 160861,
					"order_status": "Cancelled",
					"symbol": "BTCUSD",
					"side": "Buy",
					"order_type": "Market",
					"price": "9800",
					"qty": "16737",
					"time_in_force": "ImmediateOrCancel",
					"order_link_id": "",
					"order_id": "fead08d7-47c0-4d6a-b9e7-5c71d5df8ba1",
					"created_at": "2020-07-24T08:22:30Z",
					"updated_at": "2020-07-24T08:22:30Z",
					"leaves_qty": "0",
					"leaves_value": "0",
					"cum_exec_qty": "0",
					"cum_exec_value": "0",
					"cum_exec_fee": "0",
					"reject_reason": "EC_NoImmediateQtyToFill"
				}
			],
			"cursor": "w01XFyyZc8lhtCLl6NgAaYBRfsN9Qtpp1f2AUy3AS4+fFDzNSlVKa0od8DKCqgAn"
		},
		"time_now": "1604653633.173848",
		"rate_limit_status": 599,
		"rate_limit_reset_ms": 1604653633171,
		"rate_limit": 600
	}

	ExchangeOrderResult result = new ExchangeOrderResult();
	if (token.Count() > 0)
	{
		result.Amount = token["qty"].ConvertInvariant<Decimal>();
		result.AmountFilled = token["cum_exec_qty"].ConvertInvariant<decimal>();
		result.Price = token["price"].ConvertInvariant<decimal>();
		result.IsBuy = token["side"].ToStringInvariant().EqualsWithOption("Buy");
		result.OrderDate = token["created_at"].ConvertInvariant<DateTime>();
		result.OrderId = token["order_id"].ToStringInvariant();
		result.ClientOrderId = token["order_link_id"].ToStringInvariant();
		result.MarketSymbol = token["symbol"].ToStringInvariant();

		switch (token["order_status"].ToStringInvariant())
		{ // https://bybit-exchange.github.io/docs/inverse/#order-status-order_status
			case "Created":
				result.Result = ExchangeAPIOrderResult.PendingOpen;
				break;
			case "Rejected":
				result.Result = ExchangeAPIOrderResult.Rejected;
				break;
			case "New":
				result.Result = ExchangeAPIOrderResult.Open;
				break;
			case "PartiallyFilled":
				result.Result = ExchangeAPIOrderResult.FilledPartially;
				break;
			case "Filled":
				result.Result = ExchangeAPIOrderResult.Filled;
				break;
			case "Cancelled":
				result.Result = ExchangeAPIOrderResult.Canceled;
				break;
			case "PendingCancel":
				result.Result = ExchangeAPIOrderResult.PendingCancel;
				break;

			default:
				throw new NotImplementedException($"Unexpected status type: {token["order_status"].ToStringInvariant()}");
		}
	}
	result.ResultCode = resultCode;
	result.Message = resultMessage;

	return result;
}
public async Task<IWebSocket> GetExecutionWebSocketAsync(Action<IEnumerable<ExecutionRecord>> displayexecution)
{
	return await DoConnectWebSocketAsync(async (_socket) =>
	{
		await SendWebsocketAuth(_socket);
		await _socket.SendMessageAsync(new { op = "subscribe", args = new[] { "execution" } });
	}, async (_socket, token) =>
	{
		List<ExecutionRecord> le = new List<ExecutionRecord>();
		foreach (var b in token)
		{
			var f = new ExecutionRecord()
			{
				TradedQty = b["exec_qty"].ConvertInvariant<decimal>(),
				TradePrice = b["price"].ConvertInvariant<decimal>(),
				LongOrShort = b["side"].ToStringInvariant() == "Buy" ? true : false,
				symbol = b["symbol"].ToStringInvariant(),
				Fee = b["exec_fee"].ConvertInvariant<decimal>(),
				TradeID = b["exec_id"].ToStringInvariant(),
				OrderID = b["order_id"].ToStringInvariant(),
				TradeTime = b["trade_time"].ToDateTimeInvariant(),
				IsMaker = b["is_maker"].ConvertInvariant<bool>(),
				UnTraded = b["leaves_qty"].ConvertInvariant<decimal>(),
				TradeType = b["exec_type"].ToStringInvariant(),
				OrderQty = b["order_qty"].ConvertInvariant<decimal>()
			};
			le.Add(f);
		}
		displayexecution(le);
		await Task.CompletedTask;
	});
}
public async Task<IEnumerable<ExecutionRecord>> GetTrades(string marketSymbol)
{
	List<ExecutionRecord> trades = new List<ExecutionRecord>();
	var extraParams = new Dictionary<string, object>();
	if (!string.IsNullOrWhiteSpace(marketSymbol))
	{
		extraParams["symbol"] = marketSymbol;

		extraParams["start_time"] = (int)CryptoUtility.UnixTimestampFromDateTimeSeconds(DateTime.Now.AddDays(-7));
		extraParams["end_time"] = (int)CryptoUtility.UnixTimestampFromDateTimeSeconds(DateTime.Now);
		extraParams["limit"] = 200;
	}
	else
	{
		throw new Exception("marketSymbol is required");
	}
	var queryString = await GetAuthenticatedQueryString(extraParams);
	JToken token = GetResult(await DoMakeJsonRequestAsync<JToken>($"/v2/private/execution/list?" + queryString, BaseUrl, null, "GET"), out var retCode, out var retMessage);
	var list = token["trade_list"];
	foreach (var b in list)
	{     //ExchangeTrade struct is too simple for this case
		trades.Add(ParseExecution(b));
	}
	return trades;
}
private ExecutionRecord ParseExecution(JToken b)
{
	return new ExecutionRecord()
	{
		TradedQty = b["exec_value"].ConvertInvariant<decimal>(),
		TradePrice = b["exec_price"].ConvertInvariant<decimal>(),
		LongOrShort = b["side"].ToStringInvariant() == "Buy" ? true : false,
		symbol = b["symbol"].ToStringInvariant(),
		Fee = b["exec_fee"].ConvertInvariant<decimal>(),
		TradeID = b["exec_id"].ToStringInvariant(),
		OrderID = b["order_id"].ToStringInvariant(),
		TradeTime = CryptoUtility.UnixTimeStampToDateTimeMilliseconds(b["trade_time_ms"].ConvertInvariant<long>()),
		IsMaker = b["is_maker"].ConvertInvariant<bool>(),
		UnTraded = b["leaves_qty"].ConvertInvariant<decimal>(),
		TradeType = b["exec_type"].ToStringInvariant(),
		OrderQty = b["order_qty"].ConvertInvariant<decimal>()
	};
}
}

public partial class ExchangeName { public const string Bybit = "Bybit"; }
}
*/
namespace ExchangeSharp
{
	public sealed partial class ExchangeBybitAPI : ExchangeAPI
	{
		private int _recvWindow = 30000;

		public override string BaseUrl { get; set; } = "https://api.bybit.com";
		public override string BaseUrlWebSocket { get; set; } = "wss://stream.bybit.com/realtime";
		// public override string BaseUrl { get; set; } = "https://api-testnet.bybit.com/";
		// public override string BaseUrlWebSocket { get; set; } = "wss://stream-testnet.bybit.com/realtime";

		private ExchangeBybitAPI()
		{
			NonceStyle = NonceStyle.UnixMilliseconds;
			NonceOffset = TimeSpan.FromSeconds(1.0);

			MarketSymbolSeparator = string.Empty;
			RequestContentType = "application/json";
			WebSocketOrderBookType = WebSocketOrderBookType.FullBookFirstThenDeltas;

			RateLimit = new RateGate(100, TimeSpan.FromMinutes(1));
		}

		public override Task<string> ExchangeMarketSymbolToGlobalMarketSymbolAsync(string marketSymbol)
		{
			throw new NotImplementedException();
		}

		public override Task<string> GlobalMarketSymbolToExchangeMarketSymbolAsync(string marketSymbol)
		{
			throw new NotImplementedException();
		}

		// Was initially struggling with 10002 timestamp errors, so tried calcing clock drift on every request.
		// Settled on positive NonceOffset so our clock is not likely ahead of theirs on arrival (assuming accurate client/server side clocks)
		// And larger recv_window so our packets have plenty of time to arrive
		// protected override async Task OnGetNonceOffset()
		// {
		//     string stringResult = await MakeRequestAsync("/v2/public/time");
		//     var token  = JsonConvert.DeserializeObject<JToken>(stringResult);
		//     DateTime serverDate = CryptoUtility.UnixTimeStampToDateTimeSeconds(token["time_now"].ConvertInvariant<Double>());
		//     var now = CryptoUtility.UtcNow;
		//     NonceOffset = now - serverDate + TimeSpan.FromSeconds(1); // how much time to substract from Nonce when making a request
		// }

		protected override async Task ProcessRequestAsync(IHttpWebRequest request, Dictionary<string, object> payload)
		{
			if ((payload != null) && payload.ContainsKey("sign") && request.Method == "POST")
			{
				await CryptoUtility.WritePayloadJsonToRequestAsync(request, payload);
			}
		}

#nullable enable
		//Not using MakeJsonRequest... so we can perform our own check on the ret_code 
		private async Task<T> DoMakeJsonRequestAsync<T>(string url, string? baseUrl = null, Dictionary<string, object>? payload = null, string? requestMethod = null)
		{
			await new SynchronizationContextRemover();
			string stringResult = await MakeRequestAsync(url, baseUrl, payload, requestMethod);
			return JsonConvert.DeserializeObject<T>(stringResult);
		}
#nullable disable

		private JToken CheckRetCode(JToken response, string[] allowedRetCodes)
		{
			var result = GetResult(response, out var retCode, out var retMessage);
			if (!allowedRetCodes.Contains(retCode))
			{
				throw new Exception($"Invalid ret_code {retCode}, ret_msg {retMessage}");
			}
			return result;
		}

		private JToken CheckRetCode(JToken response)
		{
			return CheckRetCode(response, new string[] { "0" });
		}

		private JToken GetResult(JToken response, out string retCode, out string retMessage)
		{
			retCode = response["ret_code"].ToStringInvariant();
			retMessage = response["ret_msg"].ToStringInvariant();
			return response["result"];
		}

		private async Task SendWebsocketAuth(IWebSocket socket)
		{
			var payload = await GetNoncePayloadAsync();
			var nonce = (payload["nonce"].ConvertInvariant<long>() + 5000).ToStringInvariant();
			var signature = CryptoUtility.SHA256Sign($"GET/realtime{nonce}", CryptoUtility.ToUnsecureBytesUTF8(PrivateApiKey));
			await socket.SendMessageAsync(new { op = "auth", args = new[] { PublicApiKey.ToUnsecureString(), nonce, signature } });
		}

		private async Task<Dictionary<string, object>> GetAuthenticatedPayload(Dictionary<string, object> requestPayload = null)
		{
			var payload = await GetNoncePayloadAsync();
			var nonce = payload["nonce"].ConvertInvariant<long>();
			payload.Remove("nonce");
			payload["api_key"] = PublicApiKey.ToUnsecureString();
			payload["timestamp"] = nonce.ToStringInvariant();
			payload["recv_window"] = _recvWindow;
			if (requestPayload != null)
			{
				payload = payload.Concat(requestPayload).ToDictionary(p => p.Key, p => p.Value);
			}

			string form = CryptoUtility.GetFormForPayload(payload, false, true);
			form = form.Replace("=False", "=false");
			form = form.Replace("=True", "=true");
			payload["sign"] = CryptoUtility.SHA256Sign(form, CryptoUtility.ToUnsecureBytesUTF8(PrivateApiKey));
			return payload;
		}

		private async Task<string> GetAuthenticatedQueryString(Dictionary<string, object> requestPayload = null)
		{
			var payload = await GetAuthenticatedPayload(requestPayload);
			var sign = payload["sign"].ToStringInvariant();
			payload.Remove("sign");
			string form = CryptoUtility.GetFormForPayload(payload, false, true);
			form += "&sign=" + sign;
			return form;
		}

		private Task<IWebSocket> DoConnectWebSocketAsync(Func<IWebSocket, Task> connected, Func<IWebSocket, JToken, Task> callback, int symbolArrayIndex = 3)
		{
			Timer pingTimer = null;
			return ConnectWebSocketAsync(fullurl: BaseUrlWebSocket, messageCallback: async (_socket, msg) =>
			{
				var msgString = msg.ToStringFromUTF8();
				JToken token = JToken.Parse(msgString);

				if (token["ret_msg"]?.ToStringInvariant() == "pong")
				{ // received reply to our ping
					return;
				}

				if (token["topic"] != null)
				{
					var data = token["data"];
					await callback(_socket, data);
				}
				else
				{
					/*
                    subscription response:
                    {
                        "success": true, // Whether subscription is successful
                        "ret_msg": "",   // Successful subscription: "", otherwise it shows error message
                        "conn_id":"e0e10eee-4eff-4d21-881e-a0c55c25e2da",// current connection id
                        "request": {     // Request to your subscription
                            "op": "subscribe",
                            "args": [
                                "kline.BTCUSD.1m"
                            ]
                        }
                    }
                    */
					JToken response = token["request"];
					var op = response["op"]?.ToStringInvariant();
					if ((response != null) && ((op == "subscribe") || (op == "auth")))
					{
						var responseMessage = token["ret_msg"]?.ToStringInvariant();
						if (responseMessage != "")
						{
							Logger.Info("Websocket unable to connect: " + msgString);
							return;
						}
						else if (pingTimer == null)
						{
							/*
                            ping response:
                            {
                                "success": true, // Whether ping is successful
                                "ret_msg": "pong",
                                "conn_id": "036e5d21-804c-4447-a92d-b65a44d00700",// current connection id
                                "request": {
                                    "op": "ping",
                                    "args": null
                                }
                            }
                            */
							pingTimer = new Timer(callback: async s => await _socket.SendMessageAsync(new { op = "ping" }),
								state: null, dueTime: 0, period: 15000); // send a ping every 15 seconds
							return;
						}
					}
				}
			},
			connectCallback: async (_socket) =>
			{
				await connected(_socket);
				_socket.ConnectInterval = TimeSpan.FromHours(0);
			},
			disconnectCallback: s =>
			{
				pingTimer.Dispose();
				pingTimer = null;
				return Task.CompletedTask;
			});
		}

		private async Task AddMarketSymbolsToChannel(IWebSocket socket, string argsPrefix, string[] marketSymbols)
		{
			string fullArgs = argsPrefix;
			if (marketSymbols == null || marketSymbols.Length == 0)
			{
				fullArgs += "*";
			}
			else
			{
				foreach (var symbol in marketSymbols)
				{
					fullArgs += symbol + "|";
				}
				fullArgs = fullArgs.TrimEnd('|');
			}

			await socket.SendMessageAsync(new { op = "subscribe", args = new[] { fullArgs } });
		}
		protected override async Task<IWebSocket> OnGetDeltaOrderBookWebSocketAsync(Action<ExchangeOrderBook> callback, int maxCount = 20, params string[] marketSymbols)
		{
			if (marketSymbols == null || marketSymbols.Length == 0)
			{
				marketSymbols = (await GetMarketSymbolsAsync()).ToArray();
			}
			return await DoConnectWebSocketAsync(async (_socket) =>
			{
				await AddMarketSymbolsToChannel(_socket, "orderBookL2_25.", marketSymbols);
			}, async (_socket, token) =>
			{
				ExchangeOrderBook book = new ExchangeOrderBook();
				if (token is JArray)
				{
					foreach (var t in token)
					{
						if (t["side"].ToString() == "Sell")
							book.Asks.Add(t["price"].ConvertInvariant<decimal>(), new ExchangeOrderPrice()
							{
								Amount = t["size"].ConvertInvariant<decimal>(),
								Price = t["price"].ConvertInvariant<decimal>()
							});
						else if (t["side"].ToString() == "Buy")
							book.Bids.Add(t["price"].ConvertInvariant<decimal>(), new ExchangeOrderPrice()
							{
								Amount = t["size"].ConvertInvariant<decimal>(),
								Price = t["size"].ConvertInvariant<decimal>()
							});
					}
					try
					{
						var dd = token.Root["timestamp_e6"].ConvertInvariant<long>().UnixTimeStampToDateTimeMicroseconds();
						book.Timestamp = dd;
						book.LastUpdatedUtc = dd;
						callback(book);
					}
					catch (Exception e)
					{ }
				}
				if (token["update"] != null && token["update"] is JArray)
				{
					foreach (var t in token["update"])
					{
						book = new ExchangeOrderBook();
						book.MarketSymbol = t["symbol"].ToString();
						if (t["side"].ToString() == "Buy")
						{
							var d = t["price"].ConvertInvariant<decimal>();
							book.Bids.Add(d, new ExchangeOrderPrice() { Price = d, Amount = t["size"].ConvertInvariant<decimal>() });
						}
						else
						{
							var d = t["price"].ConvertInvariant<decimal>();
							book.Asks.Add(d, new ExchangeOrderPrice() { Price = d, Amount = t["size"].ConvertInvariant<decimal>() });
						}
					}
					var dd = token.Root["timestamp_e6"].ConvertInvariant<long>().UnixTimeStampToDateTimeMicroseconds();
					book.Timestamp = dd;
					book.LastUpdatedUtc = dd;
					callback(book);
				}

			});
		}

		public override async Task<IWebSocket> GetTickersWebSocketAsync(Action<IReadOnlyCollection<KeyValuePair<string, ExchangeTicker>>> callback, params string[] symbols)
		{
			//string[] symbs = new string[] { "BTCUSD", "ETHUSD"};
			return await OnGetTickersWebSocketAsync(callback, symbols);
		}
		protected override async Task<IWebSocket> OnGetTickersWebSocketAsync(Action<IReadOnlyCollection<KeyValuePair<string, ExchangeTicker>>> callback, params string[] marketSymbols)
		{
			return await DoConnectWebSocketAsync(async (_socket) =>
			{
				await AddMarketSymbolsToChannel(_socket, "instrument_info.100ms.", marketSymbols);
			}, async (_socket, token) =>
			{
				ExchangeTicker ticker;
				if (token["update"] != null)
				{
					var list = new List<KeyValuePair<string, ExchangeTicker>>();
					foreach (var t in token["update"])
					{
						if (t["last_price"] is null)
							continue;
						ticker = new ExchangeTicker()
						{
							MarketSymbol = t["symbol"].ToString(),
							Last = t["last_price_e4"].ToObject<decimal>() / 10000,
							Volume = new ExchangeVolume()
							{
								BaseCurrency = t["symbol"].ToString().Substring(0, 3),
								QuoteCurrency = t["symbol"].ToString().Substring(3, 3),
								Timestamp = t["updated_at"].ConvertInvariant<DateTime>(),
								BaseCurrencyVolume = t["volume_24h"].ToObject<decimal>()
							},
							Id = t["id"].ToString()
						};
						var kv = new KeyValuePair<string, ExchangeTicker>(ticker.MarketSymbol, ticker);
						list.Add(kv);
					}
					if (list.Count > 0) callback(list);
				}
				else
				{
					ticker = new ExchangeTicker()
					{
						Bid = token["bid1_price_e4"].ToObject<decimal>() / 10000,
						Ask = token["ask1_price_e4"].ToObject<decimal>() / 10000,
						Last = token["last_price_e4"].ToObject<decimal>() / 10000,
						MarketSymbol = token["symbol"].ToString(),
						Volume = new ExchangeVolume()
						{
							BaseCurrency = token["symbol"].ToString().Substring(0, 3),
							QuoteCurrency = token["symbol"].ToString().Substring(3, 3),
							Timestamp = token["updated_at"].ConvertInvariant<DateTime>(),
							BaseCurrencyVolume = token["volume_24h"].ToObject<decimal>()
						},
						Id = token["id"].ToString()
					};
					var kv = new KeyValuePair<string, ExchangeTicker>(ticker.MarketSymbol, ticker);
					var list = new List<KeyValuePair<string, ExchangeTicker>>();
					list.Add(kv);
					callback(list);
				}
			});
		}

		protected override async Task<IWebSocket> OnGetTradesWebSocketAsync(Func<KeyValuePair<string, ExchangeTrade>, Task> callback, params string[] marketSymbols)
		{
			/*
            request:
            {"op":"subscribe","args":["trade.BTCUSD|XRPUSD"]}
			*/
			/*
			    response:
                {
                    "topic": "trade.BTCUSD",
                    "data": [
                        {
                            "timestamp": "2020-01-12T16:59:59.000Z",
                            "trade_time_ms": 1582793344685, // trade time in millisecond
                            "symbol": "BTCUSD",
                            "side": "Sell",
                            "size": 328,
                            "price": 8098,
                            "tick_direction": "MinusTick",
                            "trade_id": "00c706e1-ba52-5bb0-98d0-bf694bdc69f7",
                            "cross_seq": 1052816407
                        }
                    ]
                }
			 */
			return await DoConnectWebSocketAsync(async (_socket) =>
			{
				await AddMarketSymbolsToChannel(_socket, "trade.", marketSymbols);
			}, async (_socket, token) =>
			{
				foreach (var dataRow in token)
				{
					ExchangeTrade trade = dataRow.ParseTrade(
						amountKey: "size",
						priceKey: "price",
						typeKey: "side",
						timestampKey: "timestamp",
						timestampType: TimestampType.Iso8601,
						idKey: "trade_id");
					await callback(new KeyValuePair<string, ExchangeTrade>(dataRow["symbol"].ToStringInvariant(), trade));
				}
			});
		}

		public async Task<IWebSocket> GetExecutionWebSocketAsync(Action<IEnumerable<ExecutionRecord>> displayexecution)
		{
			return await DoConnectWebSocketAsync(async (_socket) =>
			{
				await SendWebsocketAuth(_socket);
				await _socket.SendMessageAsync(new { op = "subscribe", args = new[] { "execution" } });
			}, async (_socket, token) =>
			{
				List<ExecutionRecord> le = new List<ExecutionRecord>();
				foreach (var b in token)
				{
					var f = new ExecutionRecord()
					{
						TradedQty = b["exec_qty"].ConvertInvariant<decimal>(),
						TradePrice = b["price"].ConvertInvariant<decimal>(),
						LongOrShort = b["side"].ToStringInvariant() == "Buy" ? true : false,
						symbol = b["symbol"].ToStringInvariant(),
						Fee = b["exec_fee"].ConvertInvariant<decimal>(),
						TradeID = b["exec_id"].ToStringInvariant(),
						OrderID = b["order_id"].ToStringInvariant(),
						TradeTime = b["trade_time"].ToDateTimeInvariant(),
						IsMaker = b["is_maker"].ConvertInvariant<bool>(),
						UnTraded = b["leaves_qty"].ConvertInvariant<decimal>(),
						TradeType = b["exec_type"].ToStringInvariant(),
						OrderQty = b["order_qty"].ConvertInvariant<decimal>()
					};
					le.Add(f);
				}
				displayexecution(le);
				await Task.CompletedTask;
			});
		}
		public async Task<IWebSocket> GetOrderWebSocketAsync(Action<ExchangeFutureOrder> callback)
		{
			return await DoConnectWebSocketAsync(async (_socket) =>
			{
				await SendWebsocketAuth(_socket);
				await _socket.SendMessageAsync(new { op = "subscribe", args = new[] { "order" } });
			}, async (_socket, token) =>
			{
				foreach (var dataRow in token)
				{
					callback(ParseExchangeFutureOrder(dataRow));
				}
				await Task.CompletedTask;
			});
		}

		private ExchangeFutureOrder ParseExchangeFutureOrder(JToken dataRow)
		{/*{{
  "order_id": "712a24f8-969e-4e29-b8bf-0c648abd7846",
  "order_link_id": "",
  "symbol": "BTCUSDZ21",
  "side": "Sell",
  "order_type": "Limit",
  "price": "50987",
  "qty": 10,
  "time_in_force": "GoodTillCancel",
  "create_type": "CreateByUser",
  "cancel_type": "",
  "order_status": "New",
  "leaves_qty": 10,
  "cum_exec_qty": 0,
  "cum_exec_value": "0",
  "cum_exec_fee": "0",
  "timestamp": "2021-08-25T05:22:21.409Z",
  "take_profit": "0",
  "stop_loss": "0",
  "trailing_stop": "0",
  "last_exec_price": "0",
  "reduce_only": false,
  "close_on_trigger": false
}}*/
			ExchangeFutureOrder order = new ExchangeFutureOrder()
			{
				ExchangeID = "Bybit",
				Symbol = dataRow["symbol"].ToStringInvariant(),
				Side = dataRow["side"].ToStringInvariant(),
				OrderType = dataRow["order_type"].ToStringInvariant(),
				OrigPrice = dataRow["price"].ConvertInvariant<decimal>(),
				OrigQty = dataRow["qty"].ConvertInvariant<decimal>(),
				AvgPrice = dataRow["last_exec_price"].ConvertInvariant<decimal>(),
				TimeInForce = dataRow["time_in_force"].ToStringInvariant(),
				OrderStatus = dataRow["order_status"].ToStringInvariant(),
				OrderId = dataRow["order_id"].ToString(),
				FilledQty = dataRow["cum_exec_qty"].ConvertInvariant<decimal>(),
				LastFilledQty = dataRow["cum_exec_qty"].ConvertInvariant<decimal>(),
				UpdateTime = CryptoUtility.ParseTimestamp(dataRow["timestamp"], TimestampType.Iso8601),
				OrderTime = CryptoUtility.ParseTimestamp(dataRow["timestamp"], TimestampType.Iso8601),
				LastFilledPrice = dataRow["last_exec_price"].ConvertInvariant<decimal>(),
				IsReduceOnly = dataRow["reduce_only"].ConvertInvariant<bool>(),
				Commission = dataRow["cum_exec_fee"].ConvertInvariant<decimal>()
			};
			return order;
		}
		public async Task<IEnumerable<GeneralPosition>> GetAllOpenPositionsAsync(IEnumerable<string>? SymbolList)
		{
			List<GeneralPosition> listposi = new List<GeneralPosition>();
			var s = await DoMakeJsonRequestAsync<JToken>("/futures/private/position/list", BaseUrl, null, "GET");
			return listposi;
			if (SymbolList is null || SymbolList.Count() == 0)
				SymbolList = await GetMarketSymbolsAsync();

			foreach (var symbol in SymbolList)
			{
				var a = await GetOpenPositionAsync(symbol);
				if (a is null)
					continue;
				else
				{
					listposi.Add(new GeneralPosition()
					{
						Symbol = a.MarketSymbol,
						Position = a.Total,
						ExchangeID = "Bybit",
						RealizedPnl = a.ProfitLoss,
						UpdateTime = DateTime.UtcNow,
						LiquidationPrice = a.LiquidationPrice,
						//IsolatedOrCross = a.Type
					});
				}
			}
			return listposi;
		}
		public async Task<IWebSocket> GetPositionWebSocketAsync(Action<GeneralPosition> callbackposition, Action<GeneralAccount> callbackaccount)
		{
			/*
			    response:
                {
                "topic": "position",
                "action": "update",
                "data": [
                    {
                        "user_id":  1,                            // user ID
                        "symbol": "BTCUSD",                       // the contract for this position
                        "size": 11,                               // the current position amount
                        "side": "Sell",                           // side
                        "position_value": "0.00159252",           // positional value
                        "entry_price": "6907.291588174717",       // entry price
                        "liq_price": "7100.234",                  // liquidation price
                        "bust_price": "7088.1234",                // bankruptcy price
                        "leverage": "1",                           // leverage
                        "order_margin":  "1",                      // order margin
                        "position_margin":  "1",                   // position margin
                        "available_balance":  "2",                 // available balance
                        "take_profit": "0",                        // take profit price           
                        "tp_trigger_by":  "LastPrice",             // take profit trigger price, eg: LastPrice, IndexPrice. Conditional order only
                        "stop_loss": "0",                          // stop loss price
                        "sl_trigger_by":  "",                     // stop loss trigger price, eg: LastPrice, IndexPrice. Conditional order only
                        "realised_pnl":  "0.10",               // realised PNL
                        "trailing_stop": "0",                  // trailing stop points
                        "trailing_active": "0",                // trailing stop trigger price
                        "wallet_balance":  "4.12",             // wallet balance
                        "risk_id":  1,                       
                        "occ_closing_fee":  "0.1",             // position closing
                        "occ_funding_fee":  "0.1",             // funding fee
                        "auto_add_margin": 0,                  // auto margin replenishment switch
                        "cum_realised_pnl":  "0.12",           // Total realized profit and loss
                        "position_status": "Normal",           // status of position (Normal: normal Liq: in the process of liquidation Adl: in the process of Auto-Deleveraging)
                                        // Auto margin replenishment enabled (0: no 1: yes)
                        "position_seq": 14                     // position version number
                    }
                ]
                }
			 */
			return await DoConnectWebSocketAsync(async (_socket) =>
			{
				await SendWebsocketAuth(_socket);
				await _socket.SendMessageAsync(new { op = "subscribe", args = new[] { "position" } });
			}, async (_socket, token) =>
			{
				foreach (var dataRow in token)
				{
					callbackposition(ParsePosition(dataRow));
				}
				callbackaccount(ParseAccount(token));
				await Task.CompletedTask;
			});
		}
		protected override async Task<ExchangeMarginPositionResult> OnGetOpenPositionAsync(string marketSymbol)
		{
			var s = await DoMakeJsonRequestAsync<JToken>("/futures/private/position/list", BaseUrl, null, "GET");
			//var result = ParsePosition(s);
			ExchangeMarginPositionResult result = new ExchangeMarginPositionResult();
			return result;
		}
		private GeneralAccount ParseAccount(JToken data)
		{
			GeneralAccount ga = new GeneralAccount();
			foreach (var Row in data)
			{
				ga.RealizedPNL += Row["cum_realised_pnl"].ConvertInvariant<decimal>();
			}
			var dataRow = data.Last();
			ga.UserID = dataRow["user_id"].ToStringInvariant();
			ga.AccountType = "Future";
			ga.ExchangeID = "Bybit";
			ga.Available = dataRow["available_balance"].ConvertInvariant<decimal>();
			ga.Equity = dataRow["wallet_balance"].ConvertInvariant<decimal>();
			ga.OrderMargin = dataRow["order_margin"].ConvertInvariant<decimal>();
			ga.PositionMargin = dataRow["position_margin"].ConvertInvariant<decimal>();
			ga.Symbol = dataRow["symbol"].ToStringInvariant().Substring(0, 3);
			ga.UpdateTime = DateTime.UtcNow;
			return ga;
		}

		protected override async Task<IEnumerable<KeyValuePair<string, ExchangeTicker>>> OnGetTickersAsync()
		{
			var s = await DoMakeJsonRequestAsync<JToken>("/v2/public/tickers", BaseUrl, null, "GET");
			return ParseTickers(s);
		}

		private IEnumerable<KeyValuePair<string, ExchangeTicker>> ParseTickers(JToken s)
		{
			List<KeyValuePair<string, ExchangeTicker>> v = new List<KeyValuePair<string, ExchangeTicker>>();
			/*Root = {{
  "ret_code": 0,
  "ret_msg": "OK",
  "ext_code": "",
  "ext_info": "",
  "result": [    
  "symbol": "BTCUSD",
  "bid_price": "36797",
  "ask_price": "36797.5",
  "last_price": "36797.00",
  "last_tick_direction": "ZeroMinusTick",
  "prev_price_24h": "36374.50",
  "price_24h_pcnt": "0.011615",
  "high_price_24h": "38477.00",
  "low_...
			 */
			foreach (var t in s["result"].ConvertInvariant<JArray>())
			{
				var r = new ExchangeTicker();
				r.MarketSymbol = t["symbol"].ToString();
				r.Bid = t["bid_price"].ConvertInvariant<decimal>();
				r.Ask = t["ask_price"].ConvertInvariant<decimal>();
				r.Last = t["last_price"].ConvertInvariant<decimal>();
				r.Volume = new ExchangeVolume() { BaseCurrencyVolume = t["total_volume"].ConvertInvariant<decimal>() };
				v.Add(new KeyValuePair<string, ExchangeTicker>(r.MarketSymbol, r));
			}
			return v;
		}

		protected override async Task<IEnumerable<string>> OnGetMarketSymbolsAsync()
		{
			var m = await GetMarketSymbolsMetadataAsync();
			var n = m.Select(x => x.MarketSymbol);
			return n;
		}
		public override Task<IWebSocket> GetDeltaOrderBookWebSocketAsync(Action<ExchangeOrderBook> callback, int maxCount = 20, params string[] marketSymbols)
		{
			return OnGetDeltaOrderBookWebSocketAsync(callback, 20, marketSymbols);
		}
		protected override async Task<ExchangeOrderBook> OnGetOrderBookAsync(string symbol, int num)
		{
			var v = await DoMakeJsonRequestAsync<JToken>("/v2/public/orderBook/L2?symbol=" + symbol, BaseUrl, null, "GET");
			return ParseOrderBook(v);
		}

		private ExchangeOrderBook ParseOrderBook(JToken v)
		{
			/*			{				{					"ret_code": 0,
  "ret_msg": "OK",
  "ext_code": "",
  "ext_info": "",
  "result": [	    {
	    		"symbol": "BTCUSD",
               "price": "37041",
               "size": 2124,
               "side": "Buy"
	          },   {
				"symbol": "BTCUSD",
                "price": "37040.5",
                "size": 488,
                "side": "Buy"	}
  ],
  "time_now": "1623293393.187896"
	  }
			}*/
			if (v["ret_msg"].ToString() == "OK")
			{
				ExchangeOrderBook book = new ExchangeOrderBook();
				book.Timestamp = CryptoUtility.UnixTimeStampToDateTimeSeconds(v["time_now"].ConvertInvariant<double>());
				JArray arr = v["result"].ConvertInvariant<JArray>();
				book.MarketSymbol = arr[0]["symbol"].ToStringInvariant();
				book.SequenceId = 0;
				foreach (var token in arr)
				{
					decimal p = token["price"].ConvertInvariant<decimal>();
					decimal s = token["size"].ConvertInvariant<decimal>();
					if (token["side"].ToStringInvariant() == "Buy")
						book.Bids.Add(p, new ExchangeOrderPrice() { Price = p, Amount = s });
					else
						book.Asks.Add(p, new ExchangeOrderPrice { Price = p, Amount = s });

				}
				return book;
			}
			return new ExchangeOrderBook();
		}

		protected internal override async Task<IEnumerable<ExchangeMarket>> OnGetMarketSymbolsMetadataAsync()
		{
			/*
            {
            "ret_code": 0,
            "ret_msg": "OK",
            "ext_code": "",
            "ext_info": "",
            "result": [
                {
                "name": "BTCUSD",
                "base_currency": "BTC",
                "quote_currency": "USD",
                "price_scale": 2,
                "taker_fee": "0.00075",
                "maker_fee": "-0.00025",
                "leverage_filter": {
                    "min_leverage": 1,
                    "max_leverage": 100,
                    "leverage_step": "0.01"
                },
                "price_filter": {
                    "min_price": "0.5",
                    "max_price": "999999.5",
                    "tick_size": "0.5"
                },
                "lot_size_filter": {
                    "max_trading_qty": 1000000,
                    "min_trading_qty": 1,
                    "qty_step": 1
                }
                },
                {
                "name": "ETHUSD",
                "base_currency": "ETH",
                "quote_currency": "USD",
                "price_scale": 2,
                "taker_fee": "0.00075",
                "maker_fee": "-0.00025",
                "leverage_filter": {
                    "min_leverage": 1,
                    "max_leverage": 50,
                    "leverage_step": "0.01"
                },
                "price_filter": {
                    "min_price": "0.05",
                    "max_price": "99999.95",
                    "tick_size": "0.05"
                },
                "lot_size_filter": {
                    "max_trading_qty": 1000000,
                    "min_trading_qty": 1,
                    "qty_step": 1
                }
                },
                {
                "name": "EOSUSD",
                "base_currency": "EOS",
                "quote_currency": "USD",
                "price_scale": 3,
                "taker_fee": "0.00075",
                "maker_fee": "-0.00025",
                "leverage_filter": {
                    "min_leverage": 1,
                    "max_leverage": 50,
                    "leverage_step": "0.01"
                },
                "price_filter": {
                    "min_price": "0.001",
                    "max_price": "1999.999",
                    "tick_size": "0.001"
                },
                "lot_size_filter": {
                    "max_trading_qty": 1000000,
                    "min_trading_qty": 1,
                    "qty_step": 1
                }
                },
                {
                "name": "XRPUSD",
                "base_currency": "XRP",
                "quote_currency": "USD",
                "price_scale": 4,
                "taker_fee": "0.00075",
                "maker_fee": "-0.00025",
                "leverage_filter": {
                    "min_leverage": 1,
                    "max_leverage": 50,
                    "leverage_step": "0.01"
                },
                "price_filter": {
                    "min_price": "0.0001",
                    "max_price": "199.9999",
                    "tick_size": "0.0001"
                },
                "lot_size_filter": {
                    "max_trading_qty": 1000000,
                    "min_trading_qty": 1,
                    "qty_step": 1
                }
                }
            ],
            "time_now": "1581411225.414179"
            }}
             */

			List<ExchangeMarket> markets = new List<ExchangeMarket>();
			JToken allSymbols = CheckRetCode(await DoMakeJsonRequestAsync<JToken>("/v2/public/symbols"));
			foreach (JToken marketSymbolToken in allSymbols)
			{
				var market = new ExchangeMarket
				{
					MarketSymbol = marketSymbolToken["name"].ToStringUpperInvariant(),
					IsActive = true,
					QuoteCurrency = marketSymbolToken["quote_currency"].ToStringUpperInvariant(),
					BaseCurrency = marketSymbolToken["base_currency"].ToStringUpperInvariant(),
				};

				try
				{
					JToken priceFilter = marketSymbolToken["price_filter"];
					market.MinPrice = priceFilter["min_price"].ConvertInvariant<decimal>();
					market.MaxPrice = priceFilter["max_price"].ConvertInvariant<decimal>();
					market.PriceStepSize = priceFilter["tick_size"].ConvertInvariant<decimal>();

					JToken lotSizeFilter = marketSymbolToken["lot_size_filter"];
					market.MinTradeSize = lotSizeFilter["min_trading_qty"].ConvertInvariant<decimal>();
					market.MaxTradeSize = lotSizeFilter["max_trading_qty"].ConvertInvariant<decimal>();
					market.QuantityStepSize = lotSizeFilter["qty_step"].ConvertInvariant<decimal>();
				}
				catch
				{

				}
				markets.Add(market);
			}
			return markets;
		}

		protected override async Task<IEnumerable<KeyValuePair<string, GeneralAccount>>> OnGetGeneralAccountsAsync()
		{
			/*{
				{
					"equity": 0.02005273,
  "available_balance": 0.01971645,
  "used_margin": 0.00035909,
  "order_margin": 0.00022188,
  "position_margin": 0.00013721,
  "occ_closing_fee": 2.234E-05,
  "occ_funding_fee": 0,
  "wallet_balance": 0.02007554,
  "realised_pnl": 1.226E-05,
  "unrealised_pnl": -6.2E-07,
  "cum_realised_pnl": 0.0086916,
  "given_cash": 0,
  "service_cash": 0
	  }
			}*/
			Dictionary<string, GeneralAccount> balances = new Dictionary<string, GeneralAccount>();
			var queryString = await GetAuthenticatedQueryString();
			JToken Currencies = CheckRetCode(await DoMakeJsonRequestAsync<JToken>($"/v2/private/wallet/balance?" + queryString, BaseUrl, null, "GET"));
			foreach (JProperty currency in Currencies)
			{
				var c = currency.First;
				var n = currency.Name;
				balances.Add(n, new GeneralAccount()
				{
					Equity = c["equity"].ConvertInvariant<decimal>(),
					PositionMargin = c["used_margin"].ConvertInvariant<decimal>(),
					UnrealizedPNL = c["unrealised_pnl"].ConvertInvariant<decimal>(),
					RealizedPNL = c["cum_realised_pnl"].ConvertInvariant<decimal>(),
					OrderMargin = c["order_margin"].ConvertInvariant<decimal>(),
					Available = c["available_balance"].ConvertInvariant<decimal>(),
					Symbol = n,
					ExchangeID = "Bybit",
					AccountType = "Future",
					UpdateTime = DateTime.UtcNow
				});
			}
			return balances;
		}
		private async Task<Dictionary<string, decimal>> DoGetAmountsAsync(string field)
		{
			/*
            {
                "ret_code": 0,
                "ret_msg": "OK",
                "ext_code": "",
                "ext_info": "",
                "result": {
                    "BTC": {
                        "equity": 1002,                         //equity = wallet_balance + unrealised_pnl
                        "available_balance": 999.99987471,      //available_balance
                        //In Isolated Margin Mode:
                        // available_balance = wallet_balance - (position_margin + occ_closing_fee + occ_funding_fee + order_margin)
                        //In Cross Margin Mode:
                        //if unrealised_pnl > 0:
                        //available_balance = wallet_balance - (position_margin + occ_closing_fee + occ_funding_fee + order_margin)；
                        //if unrealised_pnl < 0:
                        //available_balance = wallet_balance - (position_margin + occ_closing_fee + occ_funding_fee + order_margin) + unrealised_pnl
                        "used_margin": 0.00012529,              //used_margin = wallet_balance - available_balance
                        "order_margin": 0.00012529,             //Used margin by order
                        "position_margin": 0,                   //position margin
                        "occ_closing_fee": 0,                   //position closing fee
                        "occ_funding_fee": 0,                   //funding fee
                        "wallet_balance": 1000,                 //wallet balance. When in Cross Margin mod, the number minus your unclosed loss is your real wallet balance.
                        "realised_pnl": 0,                      //daily realized profit and loss
                        "unrealised_pnl": 2,                    //unrealised profit and loss
                            //when side is sell:
                            // unrealised_pnl = size * (1.0 / mark_price -  1.0 / entry_price）
                            //when side is buy:
                            // unrealised_pnl = size * (1.0 / entry_price -  1.0 / mark_price）
                        "cum_realised_pnl": 0,                  //total relised profit and loss
                        "given_cash": 0,                        //given_cash
                        "service_cash": 0                       //service_cash
                    }
                },
                "time_now": "1578284274.816029",
                "rate_limit_status": 98,
                "rate_limit_reset_ms": 1580885703683,
                "rate_limit": 100
            }
            */
			//Dictionary<string, GeneralAccount> balances = new Dictionary<string, GeneralAccount>();
			Dictionary<string, decimal> amounts = new Dictionary<string, decimal>();
			var queryString = await GetAuthenticatedQueryString();
			JToken currencies = CheckRetCode(await DoMakeJsonRequestAsync<JToken>($"/v2/private/wallet/balance?" + queryString, BaseUrl, null, "GET"));
			foreach (JProperty currency in currencies.Children<JProperty>())
			{

				var balance = currency.Value[field].ConvertInvariant<decimal>();
				if (amounts.ContainsKey(currency.Name))
				{
					amounts[currency.Name] += balance;
				}
				else
				{
					amounts[currency.Name] = balance;
				}
			}
			return amounts;
		}

		protected override async Task<Dictionary<string, decimal>> OnGetAmountsAsync()
		{
			return await DoGetAmountsAsync("equity");
		}

		protected override async Task<Dictionary<string, decimal>> OnGetAmountsAvailableToTradeAsync()
		{
			return await DoGetAmountsAsync("available_balance");
		}

		public async Task<IEnumerable<GeneralPosition>> GetCurrentPositionsAsync()
		{
			/*
            {
                "ret_code": 0,
                "ret_msg": "OK",
                "ext_code": "",
                "ext_info": "",
                "result": {
                    "id": 27913,
                    "user_id": 1,
                    "risk_id": 1,
                    "symbol": "BTCUSD",
                    "side": "Buy",
                    "size": 5,
                    "position_value": "0.0006947",
                    "entry_price": "7197.35137469",
                    "is_isolated":true,
                    "auto_add_margin": 0,
                    "leverage": "1",  //In Isolated Margin mode, the value is set by user. In Cross Margin mode, the value is the max leverage at current risk level
                    "effective_leverage": "1", // Effective Leverage. In Isolated Margin mode, its value equals `leverage`; In Cross Margin mode, The formula to calculate:
                        effective_leverage = position size / mark_price / (wallet_balance + unrealised_pnl)
                    "position_margin": "0.0006947",
                    "liq_price": "3608",
                    "bust_price": "3599",
                    "occ_closing_fee": "0.00000105",
                    "occ_funding_fee": "0",
                    "take_profit": "0",
                    "stop_loss": "0",
                    "trailing_stop": "0",
                    "position_status": "Normal",
                    "deleverage_indicator": 4,
                    "oc_calc_data": "{\"blq\":2,\"blv\":\"0.0002941\",\"slq\":0,\"bmp\":6800.408,\"smp\":0,\"fq\":-5,\"fc\":-0.00029477,\"bv2c\":1.00225,\"sv2c\":1.0007575}",
                    "order_margin": "0.00029477",
                    "wallet_balance": "0.03000227",
                    "realised_pnl": "-0.00000126",
                    "unrealised_pnl": 0,
                    "cum_realised_pnl": "-0.00001306",
                    "cross_seq": 444081383,
                    "position_seq": 287141589,
                    "created_at": "2019-10-19T17:04:55Z",
                    "updated_at": "2019-12-27T20:25:45.158767Z"
                },
                "time_now": "1577480599.097287",
                "rate_limit_status": 119,
                "rate_limit_reset_ms": 1580885703683,
                "rate_limit": 120
            }
            */
			var queryString = await GetAuthenticatedQueryString();
			JToken token = CheckRetCode(await DoMakeJsonRequestAsync<JToken>($"/v2/private/position/list?" + queryString, BaseUrl, null, "GET"));
			List<GeneralPosition> positions = new List<GeneralPosition>();
			foreach (var item in token)
			{
				positions.Add(ParsePosition(item["data"]));
			}
			token = CheckRetCode(await DoMakeJsonRequestAsync<JToken>($"futures/private/position/list?" + queryString, BaseUrl, null, "GET"));
			foreach (var item in token)
			{
				positions.Add(ParsePosition(item["data"]));
			}
			token = CheckRetCode(await DoMakeJsonRequestAsync<JToken>($"private/linear/position/list?" + queryString, BaseUrl, null, "GET"));
			foreach (var item in token)
			{
				positions.Add(ParsePosition(item["data"]));
			}
			return positions;
		}

		private async Task<IEnumerable<ExchangeOrderResult>> DoGetOrderDetailsAsync(string orderId, string marketSymbol = null)
		{
			var extraParams = new Dictionary<string, object>();

			if (orderId != null)
			{
				extraParams["order_id"] = orderId;
			}

			if (!string.IsNullOrWhiteSpace(marketSymbol))
			{
				extraParams["symbol"] = marketSymbol;
			}
			else
			{
				throw new Exception("marketSymbol is required");
			}

			var queryString = await GetAuthenticatedQueryString(extraParams);
			JToken token = GetResult(await DoMakeJsonRequestAsync<JToken>($"/v2/private/order?" + queryString, BaseUrl, null, "GET"), out var retCode, out var retMessage);

			List<ExchangeOrderResult> orders = new List<ExchangeOrderResult>();
			if (orderId == null)
			{
				foreach (JToken order in token)
				{
					orders.Add(ParseOrder(order, retCode, retMessage));
				}
			}
			else
			{
				orders.Add(ParseOrder(token, retCode, retMessage));
			}

			return orders;
		}

		//Note, Bybit is not recommending the use of "/v2/private/order/list" now that "/v2/private/order" is capable of returning multiple results
		protected override async Task<IEnumerable<ExchangeOrderResult>> OnGetOpenOrderDetailsAsync(string marketSymbol = null)
		{
			if (!string.IsNullOrEmpty(marketSymbol))
			{
				var orders = await DoGetOrderDetailsAsync(null, marketSymbol);
				return orders;
			}
			else
			{
				List<ExchangeOrderResult> orders = new List<ExchangeOrderResult>();
				var symbollist = await GetMarketSymbolsAsync();
				foreach (var s in symbollist)
				{
					var orderss = await DoGetOrderDetailsAsync(null, s);
					if (orderss.Count() > 0)
						orders.AddRange(orderss);
				}
				return orders;
			}
		}
		public async Task<IEnumerable<ExchangeFutureOrder>> GetFullOpenOrdersAsync()
		{
			var extraParams = new Dictionary<string, object>();
			var symbollist = await GetMarketSymbolsAsync();
			var orderlist = new List<ExchangeFutureOrder>();
			foreach (var symbol in symbollist)
			{
				extraParams["symbol"] = symbol;
				var queryString = await GetAuthenticatedQueryString(extraParams);
				JToken token;
				string retCode, retMessage;
				var orders = new List<ExchangeFutureOrder>();
				try
				{
					if (symbol.Contains("USDT"))
					{
						token = GetResult(await DoMakeJsonRequestAsync<JToken>($"/private/linear/order/search?" + queryString, BaseUrl, null, "GET"), out retCode, out retMessage);
					}
					else
					if (symbol.Length > 8)
					{
						token = GetResult(await DoMakeJsonRequestAsync<JToken>($"/futures/private/order?" + queryString, BaseUrl, null, "GET"), out retCode, out retMessage);
					}
					else
						token = GetResult(await DoMakeJsonRequestAsync<JToken>($"/v2/private/order?" + queryString, BaseUrl, null, "GET"), out retCode, out retMessage);

					foreach (JToken order in token)
					{
						orderlist.Add(ParseFutureOrder(order, retCode, retMessage));
					}
				}
				catch (Exception ex)
				{ }
			}
			return orderlist;
		}

		protected override async Task<ExchangeOrderResult> OnGetOrderDetailsAsync(string orderId, string marketSymbol = null, bool isClientOrderId = false)
		{
			var orders = await DoGetOrderDetailsAsync(orderId, marketSymbol);
			if (orders.Count() > 0)
			{
				return orders.First();
			}
			else
			{
				return null;
			}
		}

		protected override async Task OnCancelOrderAsync(string orderId, string marketSymbol = null)
		{
			var extraParams = new Dictionary<string, object>();
			extraParams["order_id"] = orderId;
			if (!string.IsNullOrWhiteSpace(marketSymbol))
			{
				extraParams["symbol"] = marketSymbol;
			}
			else
			{
				throw new Exception("marketSymbol is required");
			}

			var payload = await GetAuthenticatedPayload(extraParams);
			CheckRetCode(await DoMakeJsonRequestAsync<JToken>($"/v2/private/order/cancel", BaseUrl, payload, "POST"));
			// new string[] {"0", "30032"});
			//30032: order has been finished or canceled
		}

		public async Task CancelAllOrdersAsync(string marketSymbol)
		{
			var extraParams = new Dictionary<string, object>();
			extraParams["symbol"] = marketSymbol;
			var payload = await GetAuthenticatedPayload(extraParams);
			CheckRetCode(await DoMakeJsonRequestAsync<JToken>($"/v2/private/order/cancelAll", BaseUrl, payload, "POST"));
		}

		protected override async Task<ExchangeOrderResult> OnPlaceOrderAsync(ExchangeOrderRequest order)
		{
			var payload = new Dictionary<string, object>();
			await AddOrderToPayload(order, payload);
			payload = await GetAuthenticatedPayload(payload);
			JToken token = GetResult(await DoMakeJsonRequestAsync<JToken>("/v2/private/order/create", BaseUrl, payload, "POST"), out var retCode, out var retMessage);
			return ParseOrder(token, retCode, retMessage);
		}

		public async Task<ExchangeOrderResult> OnAmendOrderAsync(ExchangeOrderRequest order)
		{
			var payload = new Dictionary<string, object>();
			payload["symbol"] = order.MarketSymbol;
			if (order.OrderId != null)
				payload["order_id"] = order.OrderId;
			else if (order.ClientOrderId != null)
				payload["order_link_id"] = order.ClientOrderId;
			else
				throw new Exception("Need either OrderId or ClientOrderId");

			payload["p_r_qty"] = (long)await ClampOrderQuantity(order.MarketSymbol, order.Amount);
			if (order.OrderType != OrderType.Market)
				payload["p_r_price"] = order.Price;

			payload = await GetAuthenticatedPayload(payload);
			JToken token = GetResult(await DoMakeJsonRequestAsync<JToken>("/v2/private/order/replace", BaseUrl, payload, "POST"), out var retCode, out var retMessage);

			var result = new ExchangeOrderResult();
			result.ResultCode = retCode;
			result.Message = retMessage;
			if (retCode == "0")
				result.OrderId = token["order_id"].ToStringInvariant();
			return result;
		}

		private async Task AddOrderToPayload(ExchangeOrderRequest order, Dictionary<string, object> payload)
		{
			/*
            side	true	string	Side
            symbol	true	string	Symbol
            order_type	true	string	Active order type
            qty	true	integer	Order quantity in USD
            price	false	number	Order price
            time_in_force	true	string	Time in force
            take_profit	false	number	Take profit price, only take effect upon opening the position
            stop_loss	false	number	Stop loss price, only take effect upon opening the position
            reduce_only	false	bool	What is a reduce-only order? True means your position can only reduce in size if this order is triggered
            close_on_trigger	false	bool	What is a close on trigger order? For a closing order. It can only reduce your position, not increase it. If the account has insufficient available balance when the closing order is triggered, then other active orders of similar contracts will be cancelled or reduced. It can be used to ensure your stop loss reduces your position regardless of current available margin.
            order_link_id	false	string	Customised order ID, maximum length at 36 characters, and order ID under the same agency has to be unique.
            */

			payload["side"] = order.IsBuy ? "Buy" : "Sell";
			payload["symbol"] = order.MarketSymbol;
			payload["order_type"] = order.OrderType.ToStringInvariant();
			payload["qty"] = await ClampOrderQuantity(order.MarketSymbol, order.Amount);

			if (order.OrderType != OrderType.Market)
				payload["price"] = order.Price;

			if (order.ClientOrderId != null)
				payload["order_link_id"] = order.ClientOrderId;

			if (order.ExtraParameters.TryGetValue("reduce_only", out var reduceOnly))
			{
				payload["reduce_only"] = reduceOnly;
			}

			if (order.ExtraParameters.TryGetValue("time_in_force", out var timeInForce))
			{
				payload["time_in_force"] = timeInForce;
			}
			else
			{
				payload["time_in_force"] = "GoodTillCancel";
			}
		}

		private GeneralPosition ParsePosition(JToken token)
		{
			/*
            "id": 27913,
            "user_id": 1,
            "risk_id": 1,
            "symbol": "BTCUSD",
            "side": "Buy",
            "size": 5,
            "position_value": "0.0006947",
            "entry_price": "7197.35137469",
            "is_isolated":true,
            "auto_add_margin": 0,
            "leverage": "1",  //In Isolated Margin mode, the value is set by user. In Cross Margin mode, the value is the max leverage at current risk level
            "effective_leverage": "1", // Effective Leverage. In Isolated Margin mode, its value equals `leverage`; In Cross Margin mode, The formula to calculate:
                effective_leverage = position size / mark_price / (wallet_balance + unrealised_pnl)
            "position_margin": "0.0006947",
            "liq_price": "3608",
            "bust_price": "3599",
            "occ_closing_fee": "0.00000105",
            "occ_funding_fee": "0",
            "take_profit": "0",
            "stop_loss": "0",
            "trailing_stop": "0",
            "position_status": "Normal",
            "deleverage_indicator": 4,
            "oc_calc_data": "{\"blq\":2,\"blv\":\"0.0002941\",\"slq\":0,\"bmp\":6800.408,\"smp\":0,\"fq\":-5,\"fc\":-0.00029477,\"bv2c\":1.00225,\"sv2c\":1.0007575}",
            "order_margin": "0.00029477",
            "wallet_balance": "0.03000227",
            "realised_pnl": "-0.00000126",
            "unrealised_pnl": 0,
            "cum_realised_pnl": "-0.00001306",
            "cross_seq": 444081383,
            "position_seq": 287141589,
            "created_at": "2019-10-19T17:04:55Z",
            "updated_at": "2019-12-27T20:25:45.158767Z
            */
			GeneralPosition result = new GeneralPosition
			{
				Symbol = token["symbol"].ToStringUpperInvariant(),
				Position = token["size"].ConvertInvariant<decimal>(),
				AvgCost = token["entry_price"].ConvertInvariant<decimal>(),
				PositionMargin = token["position_margin"].ConvertInvariant<decimal>(),
				OrderMargin = token["order_margin"].ConvertInvariant<decimal>(),
				RealizedPnl = token["realised_pnl"].ConvertInvariant<decimal>(),
				//UnrealizedPnl = token["unrealise_pnl"].ConvertInvariant<decimal>(),
				LiquidationPrice = token["liq_price"].ConvertInvariant<decimal>(),
				Leverage = token["leverage"].ConvertInvariant<int>(),
				IsolatedOrCross = token["Isolated"].ConvertInvariant<bool>(),
				UpdateTime = CryptoUtility.ParseTimestamp(token["updated_at"], TimestampType.Iso8601),
				UserID = token["user_id"].ToStringInvariant(),
			};
			if (token["side"].ToStringInvariant() == "Sell")
				result.Position *= -1;
			return result;
		}
		private IEnumerable<GeneralPosition> ParsePositions(JToken token)
		{
			List<GeneralPosition> positions = new List<GeneralPosition>();
			foreach (var t in token.Children())
			{

			}
			return positions;
		}

		public async Task<IEnumerable<ExecutionRecord>> GetTrades(string marketSymbol)
		{
			List<ExecutionRecord> trades = new List<ExecutionRecord>();
			var extraParams = new Dictionary<string, object>();
			if (!string.IsNullOrWhiteSpace(marketSymbol))
			{
				extraParams["symbol"] = marketSymbol;

				extraParams["start_time"] = (int)CryptoUtility.UnixTimestampFromDateTimeSeconds(DateTime.Now.AddDays(-7));
				extraParams["end_time"] = (int)CryptoUtility.UnixTimestampFromDateTimeSeconds(DateTime.Now);
				extraParams["limit"] = 200;
			}
			else
			{
				throw new Exception("marketSymbol is required");
			}
			var queryString = await GetAuthenticatedQueryString(extraParams);
			JToken token = GetResult(await DoMakeJsonRequestAsync<JToken>($"/v2/private/execution/list?" + queryString, BaseUrl, null, "GET"), out var retCode, out var retMessage);
			var list = token["trade_list"];
			foreach (var b in list)
			{     //ExchangeTrade struct is too simple for this case
				trades.Add(ParseExecution(b));
			}
			return trades;
		}
		private ExecutionRecord ParseExecution(JToken b)
		{
			return new ExecutionRecord()
			{
				TradedQty = b["exec_value"].ConvertInvariant<decimal>(),
				TradePrice = b["exec_price"].ConvertInvariant<decimal>(),
				LongOrShort = b["side"].ToStringInvariant() == "Buy" ? true : false,
				symbol = b["symbol"].ToStringInvariant(),
				Fee = b["exec_fee"].ConvertInvariant<decimal>(),
				TradeID = b["exec_id"].ToStringInvariant(),
				OrderID = b["order_id"].ToStringInvariant(),
				TradeTime = CryptoUtility.UnixTimeStampToDateTimeMilliseconds(b["trade_time_ms"].ConvertInvariant<long>()),
				IsMaker = b["is_maker"].ConvertInvariant<bool>(),
				UnTraded = b["leaves_qty"].ConvertInvariant<decimal>(),
				TradeType = b["exec_type"].ToStringInvariant(),
				OrderQty = b["order_qty"].ConvertInvariant<decimal>()
			};
		}
		private ExchangeOrderResult ParseOrder(JToken token, string resultCode, string resultMessage)
		{
			/*
            Active Order:
            {
            "ret_code": 0,
            "ret_msg": "OK",
            "ext_code": "",
            "ext_info": "",
            "result": {
                "user_id": 106958,
                "symbol": "BTCUSD",
                "side": "Buy",
                "order_type": "Limit",
                "price": "11756.5",
                "qty": 1,
                "time_in_force": "PostOnly",
                "order_status": "Filled",
                "ext_fields": {
                    "o_req_num": -68948112492,
                    "xreq_type": "x_create"
                },
                "last_exec_time": "1596304897.847944",
                "last_exec_price": "11756.5",
                "leaves_qty": 0,
                "leaves_value": "0",
                "cum_exec_qty": 1,
                "cum_exec_value": "0.00008505",
                "cum_exec_fee": "-0.00000002",
                "reject_reason": "",
                "cancel_type": "",
                "order_link_id": "",
                "created_at": "2020-08-01T18:00:26Z",
                "updated_at": "2020-08-01T18:01:37Z",
                "order_id": "e66b101a-ef3f-4647-83b5-28e0f38dcae0"
            },
            "time_now": "1597171013.867068",
            "rate_limit_status": 599,
            "rate_limit_reset_ms": 1597171013861,
            "rate_limit": 600
            }

            Active Order List:
            {
                "ret_code": 0,
                "ret_msg": "OK",
                "ext_code": "",
                "ext_info": "",
                "result": {
                    "data": [ 
                        {
                            "user_id": 160861,
                            "order_status": "Cancelled",
                            "symbol": "BTCUSD",
                            "side": "Buy",
                            "order_type": "Market",
                            "price": "9800",
                            "qty": "16737",
                            "time_in_force": "ImmediateOrCancel",
                            "order_link_id": "",
                            "order_id": "fead08d7-47c0-4d6a-b9e7-5c71d5df8ba1",
                            "created_at": "2020-07-24T08:22:30Z",
                            "updated_at": "2020-07-24T08:22:30Z",
                            "leaves_qty": "0",
                            "leaves_value": "0",
                            "cum_exec_qty": "0",
                            "cum_exec_value": "0",
                            "cum_exec_fee": "0",
                            "reject_reason": "EC_NoImmediateQtyToFill"
                        }
                    ],
                    "cursor": "w01XFyyZc8lhtCLl6NgAaYBRfsN9Qtpp1f2AUy3AS4+fFDzNSlVKa0od8DKCqgAn"
                },
                "time_now": "1604653633.173848",
                "rate_limit_status": 599,
                "rate_limit_reset_ms": 1604653633171,
                "rate_limit": 600
            }
            */
			ExchangeOrderResult result = new ExchangeOrderResult();
			if (token.Count() > 0)
			{
				result.Amount = token["qty"].ConvertInvariant<Decimal>();
				result.AmountFilled = token["cum_exec_qty"].ConvertInvariant<decimal>();
				result.Price = token["price"].ConvertInvariant<decimal>();
				result.IsBuy = token["side"].ToStringInvariant().EqualsWithOption("Buy");
				result.OrderDate = token["created_at"].ConvertInvariant<DateTime>();
				result.OrderId = token["order_id"].ToStringInvariant();
				result.ClientOrderId = token["order_link_id"].ToStringInvariant();
				result.MarketSymbol = token["symbol"].ToStringInvariant();

				switch (token["order_status"].ToStringInvariant())
				{
					case "Created":
					case "New":
						result.Result = ExchangeAPIOrderResult.PendingOpen;
						break;
					case "PartiallyFilled":
						result.Result = ExchangeAPIOrderResult.FilledPartially;
						break;
					case "Filled":
						result.Result = ExchangeAPIOrderResult.Filled;
						break;
					case "Cancelled":
						result.Result = ExchangeAPIOrderResult.Canceled;
						break;

					default:
						result.Result = ExchangeAPIOrderResult.Rejected;
						break;
				}
			}
			result.ResultCode = resultCode;
			result.Message = resultMessage;

			return result;
		}

		private ExchangeFutureOrder ParseFutureOrder(JToken token, string resultCode, string resultMessage)
		{           /*{
  "user_id": 2105258,
  "position_idx": 0,
  "symbol": "BTCUSD",
  "side": "Sell",
  "order_type": "Limit",
  "price": "52254",
  "qty": 50,
  "time_in_force": "GoodTillCancel",
  "order_status": "New",
  "ext_fields": {
    "o_req_num": 51991716
  },
  "last_exec_time": "1629860790.599773",
  "leaves_qty": 50,
  "leaves_value": "0.00095686",
  "cum_exec_qty": 0,
  "cum_exec_value": null,
  "cum_exec_fee": null,
  "reject_reason": "EC_NoError",
  "cancel_type": "UNKNOWN",
  "order_link_id": "",
  "created_at": "2021-08-25T03:06:30.599644955Z",
  "updated_at": "2021-08-25T03:06:30.599773Z",
  "order_id": "42502ce0-460a-4420-a20b-39cdcacbed7b",
  "take_profit": "0.00",
  "stop_loss": "0.00",
  "tp_trigger_by": "UNKNOWN",
  "sl_trigger_by": "UNKNOWN"
}}*/
			ExchangeFutureOrder result = new ExchangeFutureOrder();
			if (token.Count() > 0)
			{
				result.OrigQty = token["qty"].ConvertInvariant<Decimal>();
				result.FilledQty = token["cum_exec_qty"].ConvertInvariant<decimal>();
				result.OrigPrice = token["price"].ConvertInvariant<decimal>();
				result.Side = token["side"].ToStringInvariant().EqualsWithOption("Buy") ? "BUY" : "SELL";
				result.OrderId = token["order_id"].ToStringInvariant();
				result.ClientOrderId = token["ext_fields"]["o_req_num"].ConvertInvariant<decimal>().ToStringInvariant();
				result.Symbol = token["symbol"].ToStringInvariant();
				result.OrderStatus = token["order_status"].ToStringInvariant();
				result.OrderType = token["order_type"].ToStringInvariant();
				result.TimeInForce = token["time_in_force"].ToStringInvariant();
				result.AccountType = "Future";
				result.Commission = token["cum_exec_fee"].ConvertInvariant<decimal>();
				result.ExchangeID = "Bybit";
				result.OrderTime = token["created_at"].ConvertInvariant<DateTime>().ToUniversalTime();
				result.UpdateTime = token["updated_at"].ConvertInvariant<DateTime>();
			}

			return result;
		}

	}
	public partial class ExchangeName { public const string Bybit = "Bybit"; }
}
