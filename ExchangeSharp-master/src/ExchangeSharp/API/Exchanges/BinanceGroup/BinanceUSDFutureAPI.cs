
using ExchangeSharp.BinanceGroup;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExchangeSharp
{
	public sealed class ExchangeBinanceUSDFutureAPI : BinanceGroupCommon
	{
		public override string BaseUrl { get; set; } = "https://fapi.binance.com/fapi/v1";
		public override string BaseUrlPrivate { get; set; } = "https://fapi.binance.com/fapi/v1";
		public override string BaseUrlWebSocket { get; set; } = "wss://fstream.binance.com";

		public override string WithdrawalUrlPrivate { get; set; } = "https://api.binance.com/wapi/v3";
		public override string BaseWebUrl { get; set; } = "https://www.binance.com";
		protected override async Task<IEnumerable<KeyValuePair<string, GeneralAccount>>> OnGetGeneralAccountsAsync()
		{
			JToken token = await MakeJsonRequestAsync<JToken>("/account", BaseUrlPrivate, await GetNoncePayloadAsync());
			List<KeyValuePair<string, GeneralAccount>> balances = new List<KeyValuePair<string, GeneralAccount>>();

			foreach (JToken balance in token["assets"])
			{
				if (balance["walletBalance"].ConvertInvariant<decimal>() > 0m)
				{
					string sym = balance["asset"].ToStringInvariant();
					balances.Add(new KeyValuePair<string, GeneralAccount>(sym, new GeneralAccount()
					{
						Available = balance["availableBalance"].ConvertInvariant<decimal>(),
						ExchangeID = "Binance",
						Symbol = sym,
						PositionMargin = balance["maintMargin"].ConvertInvariant<decimal>(),
						Equity = balance["walletBalance"].ConvertInvariant<decimal>(),
						UnrealizedPNL = balance["unrealizedProfit"].ConvertInvariant<decimal>(),
						OrderMargin = balance["openOrderInitialMargin"].ConvertInvariant<decimal>(),
						AccountType = "USDTFuture",
						UpdateTime = CryptoUtility.UnixTimeStampToDateTimeMilliseconds(balance["updateTime"].ConvertInvariant<long>()),
					}));
				}
			}
			return balances;
		}
		/*
		public async Task<(IEnumerable<KeyValuePair<string, GeneralAccount>>, IEnumerable<KeyValuePair<string, GeneralPosition>>)> GetAccountAndPositionsAsync()
		{
			JToken token = await MakeJsonRequestAsync<JToken>("/account", BaseUrlPrivate, await GetNoncePayloadAsync());
			List<KeyValuePair<string, GeneralAccount>> balances = new List<KeyValuePair<string, GeneralAccount>>();
			List<KeyValuePair<string, GeneralPosition>> positions = new List<KeyValuePair<string, GeneralPosition>>();
			foreach (JToken balance in token["assets"])
			{
				if (balance["walletBalance"].ConvertInvariant<decimal>() > 0m)
				{
					string sym = balance["asset"].ToStringInvariant();
					balances.Add(new KeyValuePair<string, GeneralAccount>(sym, new GeneralAccount()
					{
						Available = balance["availableBalance"].ConvertInvariant<decimal>(),
						ExchangeID = "Binance",
						Symbol = sym,
						PositionMargin = balance["maintMargin"].ConvertInvariant<decimal>(),
						Equity = balance["walletBalance"].ConvertInvariant<decimal>(),
						UnrealizedPNL = balance["unrealizedProfit"].ConvertInvariant<decimal>(),
						OrderMargin = balance["openOrderInitialMargin"].ConvertInvariant<decimal>(),
						AccountType = "USDFuture"
					}));
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
		}*/
		protected override async Task<IWebSocket> OnUserDataWebSocketAsync(Action<object> callback, string listenKey)
		{
			return await ConnectWebSocketAsync($"{BaseUrlWebSocket}/ws/{listenKey}", (_socket, msg) =>
			{   /**/
				JToken token = JToken.Parse(msg.ToStringFromUTF8());

				var eventType = token["e"].ToStringInvariant();
				if (eventType == "ORDER_TRADE_UPDATE")
				{
					var update = JsonConvert.DeserializeObject<OrderUpdate>(token.ToStringInvariant());
					callback(update);
				}
				else if (eventType == "ACCOUNT_UPDATE")
				{
					var update = JsonConvert.DeserializeObject<AccountUpdate>(token.ToStringInvariant());
					callback(update);
				}
				else if (eventType == "MARGIN_CALL")
				{
					var update = JsonConvert.DeserializeObject<MarginCallUpdate>(token.ToStringInvariant());
					callback(update);
				}
				else if (eventType == "ACCOUNT_CONFIG_UPDATE")
				{
					var config = JsonConvert.DeserializeObject<ACCOUNT_CONFIG_UPDATE>(token.ToStringInvariant());
					callback(config);
				}
				else
				{ }
				return Task.CompletedTask;
			}, Connectcallback, disconnectcallback);
		}

		private Task disconnectcallback(IWebSocket socket)
		{
			throw new NotImplementedException();
		}

		private Task Connectcallback(IWebSocket socket)
		{
			return Task.CompletedTask;
			//throw new NotImplementedException();
		}

		public override async Task<string> GetListenKeyAsync()
		{
			var payload = await GetNoncePayloadAsync();
			JToken response = await MakeJsonRequestAsync<JToken>("/listenKey", BaseUrl, payload, "POST");
			var listenKey = response["listenKey"].ToStringInvariant();
			return listenKey;
		}
		public async Task<IEnumerable<FuturePosition>> GetFuturePositions()
		{
			JToken token = await MakeJsonRequestAsync<JToken>("/account", BaseUrlPrivate, await GetNoncePayloadAsync());
			List<FuturePosition> balances = new List<FuturePosition>();

			foreach (JToken p in token["positions"])
			{
				if (p["initialMargin"].ToStringInvariant() != "0")
					balances.Add(new FuturePosition()
					{
						Symbol = p["symbol"].ToStringInvariant(),
						PositionSide = p["positionSide"].ToStringInvariant(),
						UnrealizedPnL = p["unrealizedProfit"].ToStringInvariant(),
						PositionAmount = p["initialMargin"].ToStringInvariant(),
						EntryPrice = p["entryPrice"].ToStringInvariant(),
						IsolatedWallet = p["isolated"].ToStringInvariant()
						/*
						UnrealizedPnL = p["unrealizedProfit"].ToStringInvariant(),
						ExchangeID = "BinanceCOINFuture",
						InstrumentID = sym,
						UsedMargin = balance["initialMargin"].ConvertInvariant<decimal>(),
						Balance = balance["walletBalance"].ConvertInvariant<decimal>(),
						UnrealizedPNL = balance["unrealizedProfit"].ConvertInvariant<decimal>(),
						LockedMargin = balance["openOrderInitialMargin"].ConvertInvariant<decimal>(),
						AccountType = "FUT"*/
					});
			}
			return balances;
		}
		/*protected override async Task<IEnumerable<ExchangeOrderResult>> OnGetCompletedOrderDetailsAsync(string? marketSymbol = null, DateTime? afterDate = null)
		{
			//new way
			List<ExchangeOrderResult> orders = new List<ExchangeOrderResult>();
			try
			{
				if (string.IsNullOrWhiteSpace(marketSymbol))
				{
					//throw new Exception();
					orders.AddRange(GetCompletedOrdersForAllSymbolsAsync(afterDate));
				}
				else
				{
					Dictionary<string, object> payload = await GetNoncePayloadAsync();
					payload["symbol"] = marketSymbol!;
					if (afterDate != null)
					{
						payload["startTime"] = Math.Round(afterDate.Value.UnixTimestampFromDateTimeMilliseconds());
					}
					JToken token = await MakeJsonRequestAsync<JToken>("/allOrders", BaseUrlPrivate, payload);
					foreach (JToken order in token)
					{
						if (order["status"].ToStringInvariant() != "FILLED") continue;
						orders.Add(ParseOrderBinance(order, marketSymbol!));
						/*  order details info
						 * {
  "orderId": 19531170924,
  "symbol": "BTCUSD_PERP",
  "pair": "BTCUSD",
  "status": "FILLED",
  "clientOrderId": "electron_Sf08d8ihx9sZM6vej6SM",
  "price": "45803.3",
  "avgPrice": "45803.5",
  "origQty": "1",
  "executedQty": "1",
  "cumBase": "0.00218324",
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
  "time": 1628555954298,
  "updateTime": 1628556470531
}
						 * trade info details
						"symbol": "BTCUSD_PERP",
  "id": 214353443,
  "orderId": 19521895123,
  "pair": "BTCUSD",
  "side": "SELL",
  "price": "45798.7",
  "qty": "1",
  "realizedPnl": "0",
  "marginAsset": "BTC",
  "baseQty": "0.00218347",
  "commission": "0.00000021",
  "commissionAsset": "BTC",
  "time": 1628546011550,
  "positionSide": "BOTH",
  "maker": true,
  "buyer": false
}
					}
				}
				return orders;
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
		}*/

		public IEnumerable<ExchangeOrderResult> GetCompletedOrdersForAllSymbolsAsync(DateTime? afterDate)
		{
			List<ExchangeOrderResult> orders = new List<ExchangeOrderResult>();
			Exception? ex = null;
			string? failedSymbol = null;
			var syms = GetMarketSymbolsAsync().GetAwaiter().GetResult();

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

		private ExchangeOrderResult ParseOrderBinance(JToken trade, string v)
		{
			ExchangeOrderResult result = new ExchangeOrderResult()
			{
				Amount = trade["origQty"].ConvertInvariant<decimal>(),
				MarketSymbol = trade["symbol"].ToStringInvariant(),
				ClientOrderId = trade["clientOrderId"].ToStringInvariant(),
				AveragePrice = trade["avgPrice"].ConvertInvariant<decimal>(),
				OrderId = trade["orderId"].ToStringInvariant(),
				AmountFilled = trade["executedQty"].ConvertInvariant<decimal>(),
				Price = trade["price"].ConvertInvariant<decimal>(),
				//TradeDate
				OrderDate = CryptoUtility.UnixTimeStampToDateTimeMilliseconds(trade["time"].ConvertInvariant<long>()).ToUniversalTime(),
				TradeDate = CryptoUtility.UnixTimeStampToDateTimeMilliseconds(trade["updateTime"].ConvertInvariant<long>()),
				Message = trade["origType"].ToStringInvariant(),
				ResultCode = trade["status"].ToStringInvariant()
			};
			switch (trade["status"].ToStringInvariant())
			{
				case "FILLED":
					result.Result = ExchangeAPIOrderResult.Filled;
					break;
				case "CANCELED":
					result.Result = ExchangeAPIOrderResult.Canceled;
					break;

			}
			if (trade["side"].ToStringInvariant() == "BUY")
				result.IsBuy = true;
			else
				result.IsBuy = false;

			return result;
		}
	}

	public partial class ExchangeName { public const string BinanceUSDFUT = "BinanceUSDFUT"; }
}
