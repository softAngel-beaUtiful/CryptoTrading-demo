using System;
using System.Collections.Generic;
using System.Text;

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ExchangeSharp.Coinbase
{
	
	internal class Activate : BaseMessage
	{
		public Guid OrderId { get; set; }
		public StopType OrderType { get; set; }
		public decimal Size { get; set; }
		public decimal Funds { get; set; }
		public decimal TakerFeeRate { get; set; }
		public bool Private { get; set; }
		public decimal StopPrice { get; set; }
		public string UserId { get; set; }
		public Guid ProfileId { get; set; }
		public OrderSide Side { get; set; }
		public string ProductId { get; set; }
		public DateTimeOffset TimeStamp { get; set; }

		public ExchangeOrderResult ExchangeOrderResult => new ExchangeOrderResult()
		{
			OrderId = OrderId.ToString(),
			ClientOrderId = null, // not provided here
			Result = ExchangeAPIOrderResult.PendingOpen, // order has just been activated (so it starts in PendingOpen)
			Message = null, // can use for something in the future if needed
			Amount = Size,
			AmountFilled = 0, // just activated, so none filled
			Price = null, // not specified here (only StopPrice is)
			AveragePrice = null, // not specified here (only StopPrice is)
			OrderDate = TimeStamp.UtcDateTime, // order activated event
			CompletedDate = null, // order is active
			MarketSymbol = ProductId,
			IsBuy = Side == OrderSide.Buy,
			Fees = null, // only TakerFeeRate is specified - no fees have been charged yet
			TradeId = null, // no trades have been made
			UpdateSequence = null, // unfortunately, the Activate event doesn't provide a sequence number
		};
	}

	internal class Change : BaseMessage
	{
		public Guid OrderId { get; set; }
		public decimal NewSize { get; set; }
		public decimal OldSize { get; set; }
		public decimal OldFunds { get; set; }
		public decimal NewFunds { get; set; }
		public decimal Price { get; set; }
		public OrderSide Side { get; set; }
		public string ProductId { get; set; }
		public long Sequence { get; set; }
		public DateTime Time { get; set; }

		public ExchangeOrderResult ExchangeOrderResult => new ExchangeOrderResult()
		{
			OrderId = OrderId.ToString(),
			ClientOrderId = null, // not provided here
			Result = ExchangeAPIOrderResult.Unknown, // change messages are sent anytime an order changes in size; this includes resting orders (open) as well as received but not yet open
			Message = null, // can use for something in the future if needed
			Amount = NewSize,
			AmountFilled = null, // not specified here
			Price = Price,
			AveragePrice = null, // not specified here
								 // OrderDate - unclear if the Time in the Change msg is the new OrderDate or whether that is unchanged
			CompletedDate = null, // order is active
			MarketSymbol = ProductId,
			IsBuy = Side == OrderSide.Buy,
			Fees = null, // only TakerFeeRate is specified - no fees have been charged yet
			TradeId = null, // not a trade msg
			UpdateSequence = Sequence,
		};
	}

	internal class Done : BaseMessage
	{
		public OrderSide Side { get; set; }
		public Guid OrderId { get; set; }
		public DoneReasonType Reason { get; set; }
		public string ProductId { get; set; }
		public decimal Price { get; set; }
		public decimal RemainingSize { get; set; }
		public long Sequence { get; set; }
		public DateTimeOffset Time { get; set; }

		public ExchangeOrderResult ExchangeOrderResult => new ExchangeOrderResult()
		{
			OrderId = OrderId.ToString(),
			ClientOrderId = null, // not provided here
			Result = Reason == DoneReasonType.Filled ? ExchangeAPIOrderResult.Filled
													 : ExchangeAPIOrderResult.Canceled, // no way to tell it it is FilledPartiallyAndCenceled here 
			Message = null, // can use for something in the future if needed
			Amount = 0, // ideally, this would be null, but ExchangeOrderResult.Amount is not nullable
			AmountFilled = RemainingSize,
			IsAmountFilledReversed = true, // since only RemainingSize is provided, not Size or FilledSize
			Price = Price,
			AveragePrice = null, // not specified here
								 // OrderDate - not provided here. ideally would be null but ExchangeOrderResult.OrderDate
			CompletedDate = Time.UtcDateTime,
			MarketSymbol = ProductId,
			IsBuy = Side == OrderSide.Buy,
			Fees = null, // not specified here
			TradeId = null, // not a trade msg
			UpdateSequence = Sequence,
		};
	}

	internal class Error : BaseMessage
	{
		public string Message { get; set; }
		public string Reason { get; set; }
	}

	internal class Heartbeat : BaseMessage
	{
		public long LastTradeId { get; set; }
		public string ProductId { get; set; }
		public long Sequence { get; set; }
		public System.DateTimeOffset Time { get; set; }
		public override string ToString()
		{
			return $"Heartbeat: Last TID {LastTradeId}, Product Id {ProductId}, Sequence {Sequence}, Time {Time}";
		}
	}

	internal class LastMatch : BaseMessage
	{
		public long TradeId { get; set; }
		public Guid MakerOrderId { get; set; }
		public Guid TakerOrderId { get; set; }
		public OrderSide Side { get; set; }
		public decimal Size { get; set; }
		public decimal Price { get; set; }
		public string ProductId { get; set; }
		public long Sequence { get; set; }
		public DateTimeOffset Time { get; set; }
	}

	internal class Match : BaseMessage
	{
		public long TradeId { get; set; }
		public Guid MakerOrderId { get; set; }
		public Guid TakerOrderId { get; set; }
		public string TakerUserId { get; set; }
		public string UserId { get; set; }
		public Guid? TakerProfileId { get; set; }
		public Guid ProfileId { get; set; }
		public OrderSide Side { get; set; }
		public decimal Size { get; set; }
		public decimal Price { get; set; }
		public string ProductId { get; set; }
		public long Sequence { get; set; }
		public DateTimeOffset Time { get; set; }
		public string MakerUserId { get; set; }
		public Guid? MakerProfileId { get; set; }
		public decimal? MakerFeeRate { get; set; }
		public decimal? TakerFeeRate { get; set; }

		public ExchangeOrderResult ExchangeOrderResult => new ExchangeOrderResult()
		{
			OrderId = MakerProfileId != null ? MakerOrderId.ToString() : TakerOrderId.ToString(),
			ClientOrderId = null, // not provided here
			Result = ExchangeAPIOrderResult.FilledPartially, // could also be completely filled, but unable to determine that here
			Message = null, // can use for something in the future if needed
			Amount = 0, // ideally, this would be null, but ExchangeOrderResult.Amount is not nullable
			AmountFilled = Size,
			IsAmountFilledReversed = false, // the size here appears to be amount filled, no no need to reverse
			Price = Price,
			AveragePrice = Price, // not specified here
								  // OrderDate - not provided here. ideally would be null but ExchangeOrderResult.OrderDate is not nullable
			CompletedDate = null, // order not necessarily fullly filled at this point
			TradeDate = Time.ToDateTimeInvariant(),
			MarketSymbol = ProductId,
			IsBuy = Side == OrderSide.Buy,
			Fees = (MakerFeeRate ?? TakerFeeRate) * Price * Size,
			TradeId = TradeId.ToString(),
			UpdateSequence = Sequence,
		};
	}

	internal class Open : BaseMessage
	{
		public OrderSide Side { get; set; }
		public decimal Price { get; set; }
		public Guid OrderId { get; set; }
		public decimal RemainingSize { get; set; }
		public string ProductId { get; set; }
		public long Sequence { get; set; }
		public DateTimeOffset Time { get; set; }

		public ExchangeOrderResult ExchangeOrderResult => new ExchangeOrderResult()
		{
			OrderId = OrderId.ToString(),
			ClientOrderId = null, // not provided here
			Result = ExchangeAPIOrderResult.Open, // order is now Open
			Message = null, // can use for something in the future if needed
			Amount = 0, // ideally, this would be null, but ExchangeOrderResult.Amount is not nullable
			AmountFilled = RemainingSize,
			IsAmountFilledReversed = true, // since only RemainingSize is provided, not Size or FilledSize
			Price = Price,
			AveragePrice = null, // not specified here
			OrderDate = Time.UtcDateTime, // order open event
			CompletedDate = null, // order is active
			MarketSymbol = ProductId,
			IsBuy = Side == OrderSide.Buy,
			Fees = null, // not specified here
			TradeId = null, // not a trade msg
			UpdateSequence = Sequence,
		};
	}

	internal class Received : BaseMessage
	{
		public Guid OrderId { get; set; }
		public OrderType OrderType { get; set; }
		public decimal Size { get; set; }
		public decimal Price { get; set; }
		public OrderSide Side { get; set; }
		public Guid? ClientOid { get; set; }
		public string ProductId { get; set; }
		public long Sequence { get; set; }
		public DateTimeOffset Time { get; set; }

		public ExchangeOrderResult ExchangeOrderResult => new ExchangeOrderResult()
		{
			OrderId = OrderId.ToString(),
			ClientOrderId = ClientOid.ToString(),
			Result = ExchangeAPIOrderResult.PendingOpen, // order is now Pending
			Message = null, // can use for something in the future if needed
			Amount = Size,
			AmountFilled = 0, // order received but not yet open, so none filled
			IsAmountFilledReversed = false,
			Price = Price,
			AveragePrice = null, // not specified here
			OrderDate = Time.UtcDateTime, // order received event
			CompletedDate = null, // order is active
			MarketSymbol = ProductId,
			IsBuy = Side == OrderSide.Buy,
			Fees = null, // not specified here
			TradeId = null, // not a trade msg
			UpdateSequence = Sequence,
		};
	}

	internal class Subscription : BaseMessage
	{
		public List<Channel> Channels { get; set; }
	}
}
