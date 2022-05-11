using System;

namespace ExchangeSharp
{
	/// <summary>Exchange funding information</summary>
	public sealed class ExchangeFunding
	{
		/// <summary>
		/// The market symbol.
		/// </summary>
		public string MarketSymbol { get; set; }

		/// <summary>
		/// Funding Rate
		/// </summary>
		public decimal Rate { get; set; }

		/// <summary>
		/// TimeStamp
		/// </summary>
		public DateTime TimeStamp { get; set; }
	}
}
