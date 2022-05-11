using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ExchangeSharp.BinanceGroup
{
	public class ExecutionReport
	{
		[JsonProperty("e")]
		public string EventType { get; set; }
		[JsonProperty("E")]
		public long EventTime { get; set; }
		[JsonProperty("s")]
		public string Symbol { get; set; }
		[JsonProperty("c")]
		public string ClientOrderId { get; set; }
		[JsonProperty("S")]
		public string Side { get; set; }
		[JsonProperty("o")]
		public string OrderType { get; set; }
		[JsonProperty("f")]
		public string TimeInForce { get; set; }
		[JsonProperty("q")]
		public decimal OrderQuantity { get; set; }
		[JsonProperty("p")]
		public decimal OrderPrice { get; set; }
		[JsonProperty("P")]
		public decimal StopPrice { get; set; }
		[JsonProperty("F")]
		public decimal IcebergQuantity { get; set; }
		[JsonProperty("g")]
		public int OrderListId { get; set; }
		[JsonProperty("C")]
		public string OriginalClientOrderId { get; set; }
		[JsonProperty("x")]
		public string CurrentExecutionType { get; set; }
		[JsonProperty("X")]
		public string CurrentOrderStatus { get; set; }
		[JsonProperty("r")]
		public string OrderRejectReason { get; set; }
		[JsonProperty("i")]
		public int OrderId { get; set; }
		[JsonProperty("l")]
		public decimal LastExecutedQuantity { get; set; }
		[JsonProperty("z")]
		public decimal CumulativeFilledQuantity { get; set; }
		[JsonProperty("L")]
		public decimal LastExecutedPrice { get; set; }
		[JsonProperty("n")]
		public string CommissionAmount { get; set; }
		[JsonProperty("N")]
		public string CommissionAsset { get; set; }
		[JsonProperty("T")]
		public string TransactionTime { get; set; }
		[JsonProperty("t")]
		public string TradeId { get; set; }
		[JsonProperty("w")]
		public string IsTheOrderWorking { get; set; }
		[JsonProperty("m")]
		public string IsThisTradeTheMakerSide { get; set; }
		[JsonProperty("O")]
		public string OrderCreationTime { get; set; }
		[JsonProperty("Z")]
		public decimal CumulativeQuoteAssetTransactedQuantity { get; set; }
		[JsonProperty("Y")]
		public decimal LastQuoteAssetTransactedQuantity { get; set; }
		[JsonProperty("Q")]
		public decimal QuoteOrderQty { get; set; }

		public override string ToString()
		{
			return $"{nameof(Symbol)}: {Symbol}, {nameof(OrderType)}: {OrderType}, {nameof(OrderQuantity)}: {OrderQuantity}, {nameof(OrderPrice)}: {OrderPrice}, {nameof(CurrentOrderStatus)}: {CurrentOrderStatus}, {nameof(OrderId)}: {OrderId}, {nameof(QuoteOrderQty)}: {QuoteOrderQty}";
		}

	}

	public class Order
	{
		[JsonProperty("s")]
		public string Symbol { get; set; }
		[JsonProperty("i")]
		public int OrderId { get; set; }
		[JsonProperty("c")]
		public string ClientOrderId { get; set; }

		public override string ToString()
		{
			return $"{nameof(Symbol)}: {Symbol}, {nameof(OrderId)}: {OrderId}, {nameof(ClientOrderId)}: {ClientOrderId}";
		}
	}

	public class ListStatus
	{
		[JsonProperty("e")]
		public string EventType { get; set; }
		[JsonProperty("E")]
		public long EventTime { get; set; }
		[JsonProperty("s")]
		public string Symbol { get; set; }
		[JsonProperty("g")]
		public int OrderListId { get; set; }
		[JsonProperty("c")]
		public string ContingencyType { get; set; }
		[JsonProperty("l")]
		public string ListStatusType { get; set; }
		[JsonProperty("L")]
		public string ListOrderStatus { get; set; }
		[JsonProperty("r")]
		public string ListRejectReason { get; set; }
		[JsonProperty("C")]
		public string ListClientOrderId { get; set; }
		[JsonProperty("T")]
		public long TransactionTime { get; set; }
		[JsonProperty("O")]
		public List<Order> Orders { get; set; }

		public override string ToString()
		{
			return $"{nameof(EventType)}: {EventType}, {nameof(EventTime)}: {EventTime}, {nameof(Symbol)}: {Symbol}, {nameof(OrderListId)}: {OrderListId}, {nameof(ContingencyType)}: {ContingencyType}, {nameof(ListStatusType)}: {ListStatusType}, {nameof(ListOrderStatus)}: {ListOrderStatus}, {nameof(ListRejectReason)}: {ListRejectReason}, {nameof(ListClientOrderId)}: {ListClientOrderId}, {nameof(TransactionTime)}: {TransactionTime}, {nameof(Orders)}: {Orders}";
		}
	}

	public class Balance
	{
		[JsonProperty("a")]
		public string Asset { get; set; }
		[JsonProperty("f")]
		public decimal Free { get; set; }
		[JsonProperty("l")]
		public decimal Locked { get; set; }

		public override string ToString()
		{
			return $"{nameof(Asset)}: {Asset}, {nameof(Free)}: {Free}, {nameof(Locked)}: {Locked}";
		}
	}

	public class OutboundAccount
	{
		[JsonProperty("e")]
		public string EventType { get; set; }
		[JsonProperty("E")]
		public long EventTime { get; set; }
		[JsonProperty("u")]
		public long LastAccountUpdate { get; set; }
		[JsonProperty("B")]
		public List<Balance> Balances { get; set; }
	}
	public class FutureOrder
	{
		[JsonProperty("s")]
		public string Symbol { get; set; }
		[JsonProperty("c")]
		public string ClientOrderId { get; set; }
		[JsonProperty("S")]
		public string Side { get; set; }
		[JsonProperty("o")]
		public string OrderType { get; set; }
		[JsonProperty("f")]
		public string TimeInForce { get; set; }
		[JsonProperty("q")]
		public decimal OriginalQuantity { get; set; }
		[JsonProperty("p")]
		public decimal OriginalPrice { get; set; }
		[JsonProperty("ap")]
		public decimal AveragePrice { get; set; }
		[JsonProperty("sp")]
		public decimal StopPrice { get; set; }
		[JsonProperty("x")]
		public string ExecutionType { get; set; }
		[JsonProperty("X")]
		public string OrderStatus { get; set; }
		[JsonProperty("i")]
		public string OrderId { get; set; }
		[JsonProperty("l")]
		public decimal OrderLastFilledQuantity { get; set; }
		[JsonProperty("z")]
		public decimal OrderFilledAcumulatedQuantity { get; set; }
		[JsonProperty("L")]
		public decimal LastFilledPrice { get; set; }
		[JsonProperty("ma")]
		public string MarginAsset { get; set; }
		[JsonProperty("N")]
		public string CommissionAsset { get; set; }
		[JsonProperty("n")]
		public decimal CommissionOfTrade { get; set; }
		[JsonProperty("T")]
		public long OrderTradeTime { get; set; }
		[JsonProperty("t")]
		public long OrderTradeId { get; set; }
		[JsonProperty("rp")]
		public decimal RealizedProfitOfTheTrade { get; set; }
		[JsonProperty("b")]
		public string BidQuantityOfBaseAsset { get; set; }
		[JsonProperty("a")]
		public string AskQuantityofBaseAsset { get; set; }
		[JsonProperty("m")]
		public bool IsMaker { get; set; }
		[JsonProperty("R")]
		public bool IsReduceOnly { get; set; }
		[JsonProperty("wt")]
		public string StopPriceType { get; set; }
		[JsonProperty("ot")]
		public string OriginalOrderType { get; set; }
		[JsonProperty("ps")]
		public string PositionSide { get; set; }
		[JsonProperty("cp")]
		public bool IFCLOSEALL { get; set; }
		[JsonProperty("AP")]
		public decimal ACTIVATIONPRICE { get; set; }
		[JsonProperty("cr")]
		public decimal CallBackRate { get; set; }
		[JsonProperty("pP")]
		public bool IfConditionOrderProtected { get; set; }
		[JsonProperty("si")]
		public decimal si { get; set; }
		[JsonProperty("ss")]
		public decimal ss { get; set; }
		public override string ToString()
		{
			return $"{nameof(Symbol)}: {Symbol}, {nameof(OrderType)}: {OrderType}, {nameof(Side)}: {Side}, {nameof(OriginalPrice)}:{OriginalPrice}";
		}
	}
	public class OrderUpdate
	{
		[JsonProperty("e")]
		public string EventType { get; set; }
		[JsonProperty("E")]
		public long EventTime { get; set; }
		[JsonProperty("T")]
		public long TransactionTime { get; set; }
		[JsonProperty("i")]
		public string AccountAlias { get; set; }
		[JsonProperty("o")]
		public FutureOrder order { get; set; }
	}
	public class AccountUpdate
	{
		[JsonProperty("e")]
		public string EventType { get; set; }
		[JsonProperty("E")]
		public long EventTime { get; set; }
		[JsonProperty("T")]
		public long TransactionTime { get; set; }
		[JsonProperty("i")]
		public string AccountAlias { get; set; }
		[JsonProperty("a")]
		public UpdateData updateData { get; set; }
	}
	public class FutureBalance
	{
		[JsonProperty("a")]
		public string Asset { get; set; }
		[JsonProperty("wb")]
		public decimal WalletBalance { get; set; }
		[JsonProperty("cw")]
		public decimal CrossWalletBalance { get; set; }
		[JsonProperty("bc")]
		public decimal BalanceChange { get; set; }    //Balance Change except PnL and Commission
	}
	public class UpdateData
	{
		[JsonProperty("m")]
		public string EventReasonType { get; set; }
		[JsonProperty("B")]
		public IEnumerable<FutureBalance> Balances { get; set; }
		[JsonProperty("P")]
		public IEnumerable<FuturePosition> Positions { get; set; }
	}

	public class FuturePosition
	{
		[JsonProperty("s")]
		public string Symbol { get; set; }
		[JsonProperty("pa")]
		public string PositionAmount { get; set; }
		[JsonProperty("ep")]
		public string EntryPrice { get; set; }
		[JsonProperty("cr")]
		public string AccumulatedRealized { get; set; }
		[JsonProperty("up")]
		public string UnrealizedPnL { get; set; }
		[JsonProperty("mt")]
		public string MarginType { get; set; }
		[JsonProperty("iw")]
		public string IsolatedWallet { get; set; }
		[JsonProperty("ps")]
		public string PositionSide { get; set; }
	}
	public class MarginCallUpdate
	{
		[JsonProperty("e")]
		public string EventType { get; set; }
		[JsonProperty("E")]
		public long EventTime { get; set; }
		[JsonProperty("i")]
		public string AccountAlias { get; set; }
		[JsonProperty("cw")]
		public string CrossWalletBalance { get; set; }
		[JsonProperty("p")]
		public IEnumerable<PositionMarginCall> positionMarginCalls { get; set; }
	}

	public class PositionMarginCall
	{
		[JsonProperty("s")]
		public string Symbol { get; set; }
		[JsonProperty("ps")]
		public string PositionSide { get; set; }
		[JsonProperty("pa")]
		public string PositionAmount { get; set; }
		[JsonProperty("mt")]
		public string MarginType { get; set; }
		[JsonProperty("iw")]
		public string IsolatedWallet { get; set; }
		[JsonProperty("mp")]
		public string MarkPrice { get; set; }
		[JsonProperty("up")]
		public string UnrealizedPnL { get; set; }
		[JsonProperty("mm")]
		public string MaintenanceMarginRequired { get; set; }
	}
	public class ACCOUNT_CONFIG_UPDATE
	{
		[JsonProperty("e")]
		public string Event_Type { get; set; }
		[JsonProperty("E")]
		public long EventTime { get; set; }
		[JsonProperty("ac")]
		public SymbolLeverage AC { get; set; }
	}
	public class SymbolLeverage
	{
		[JsonProperty("s")]
		public string Symbol { get; set; }
		[JsonProperty("l")]
		public int Leverage { get; set; }
	}
}
