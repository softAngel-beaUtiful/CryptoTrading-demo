using ExchangeSharp.BinanceGroup;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExchangeSharp
{
	public sealed class ExchangeBinanceCOINFutureAPI : BinanceGroupCommon
	{
		public override string BaseUrl { get; set; } = "https://dapi.binance.com/dapi/v1";
		public override string BaseUrlPrivate { get; set; } = "https://dapi.binance.com/dapi/v1";
		public override string BaseUrlWebSocket { get; set; } = "wss://dstream.binance.com";

		public override string WithdrawalUrlPrivate { get; set; } = "https://api.binance.com/wapi/v3";
		public override string BaseWebUrl { get; set; } = "https://www.binance.com";

		public override async Task<string> GetListenKeyAsync()
		{
			var payload = await GetNoncePayloadAsync();
			JToken response = await MakeJsonRequestAsync<JToken>("/listenKey", BaseUrl, payload, "POST");
			var listenKey = response["listenKey"].ToStringInvariant();
			return listenKey;
		}
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
		protected override async Task<IWebSocket> OnUserDataWebSocketAsync(Action<object> callback, string listenKey)
		{
			return await ConnectWebSocketAsync($"{BaseUrlWebSocket}/ws/{listenKey}", (_socket, msg) =>
			{/*{{
  "e": "ORDER_TRADE_UPDATE",
  "T": 1624462525214,
  "E": 1624462525219,
  "i": "FzoCsRSguXoCmY",
  "o": {
    "s": "ETHUSD_210924",
    "c": "electron_aZSXjDcXHJkGGJ0TC2Bb",
    "S": "BUY",
    "o": "LIMIT",
    "f": "GTC",
    "q": "1",
    "p": "1996",
    "ap": "0",
    "sp": "0",
    "x": "NEW",
    "X": "NEW",
    "i": 850551527,
    "l": "0",
    "z": "0",
    "L": "0",
    "T": 1624462525214,
    "t": 0,
    "b": "0.00501002",
    "a": "0.15157969",
    "m": false,
    "R": false,
    "wt": "CONTRACT_PRICE",
    "ot": "LIMIT",
    "ps": "BOTH",
    "cp": false,
    "ma": "ETH",
    "rp": "0",
    "pP": false,
    "si": 0,
    "ss": 0
  }}}*/
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
				else
				{

					if (eventType == "listenKeyExpired")
					{ callback("listenKeyExpired"); }
					else
					{

					}
				}
				return Task.CompletedTask;
			}, connectcallback, disconnectcallback);
		}

		private Task disconnectcallback(IWebSocket socket)
		{
			throw new NotImplementedException();
		}

		private Task connectcallback(IWebSocket socket)
		{
			return Task.CompletedTask;
			//throw new NotImplementedException();
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
	}
	public partial class ExchangeName { public const string BinanceCOINFUT = "BinanceCOINFUT"; }
}
