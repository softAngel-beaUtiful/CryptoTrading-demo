using System;
using System.Collections.Generic;
using System.Text;

using System.Runtime.Serialization;

namespace ExchangeSharp.Coinbase
{
	internal enum OrderSide
	{
		[EnumMember(Value = "buy")]
		Buy,
		[EnumMember(Value = "sell")]
		Sell
	}

	internal enum StopType
	{
		[EnumMember(Value = "Unknown")]
		Unknown,
		[EnumMember(Value = "loss")]
		Loss,
		[EnumMember(Value = "entry")]
		Entry,
	}

	internal enum DoneReasonType
	{
		Canceled,
		Filled
	}
}
