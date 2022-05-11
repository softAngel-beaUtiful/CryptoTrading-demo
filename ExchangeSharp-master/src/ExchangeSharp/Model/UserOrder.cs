/*
MIT LICENSE

Copyright 2017 Digital Ruby, LLC - http://www.digitalruby.com

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

namespace ExchangeSharp
{
    using System;

    /// <summary>Result of an exchange order</summary>
    public sealed class UserOrder
    {
        /// <summary>Order id</summary>
        public string OrderId { get; set; }

        /// <summary>Client order id</summary>
        public string ClOrderId { get; set; }

        /// <summary>Exchange market ID</summary>
        public string ExchangeID { get; set; }

        /// <summary>Symbol. E.g. ADA/ETH</summary>
        public string Symbol { get; set; }

        /// <summary>Whether the order is a buy or sell</summary>
        public bool IsBuy { get; set; }

        /// <summary>Whether the order is a open or close</summary>
        public bool IsOpen { get; set; }

        /// <summary>Whether the order is a margin or not</summary>
        public bool IsMargin { get; set; }

        /// <summary>The limit price on the order in the ratio of base/market currency.
        /// E.g. 0.000342 ADA/ETH</summary>
        public decimal Price { get; set; }

        /// <summary>Original order amount in the market currency. 
        /// E.g. ADA/BTC would be ADA</summary>
        public decimal Amount { get; set; }

        /// <summary>Price per unit in the ratio of base/market currency.
        /// E.g. 0.000342 ADA/ETH</summary>
        public decimal AveragePrice { get; set; }

        /// <summary>Amount filled in the market currency.</summary>
        public decimal FilledAmount { get; set; }

        /// <summary>The fees on the order (not a percent).
        /// E.g. 0.0025 ETH</summary>
        public decimal Fees { get; set; }

        public decimal LeaveAmount { get; set; }

        public string Account { get; set; }
        public string Currency { get; set; }
        public string SettlCurrency { get; set; }
        public string TimeInForce { get; set; }
        /// <summary>Message if any</summary>
        public string Message { get; set; }

        public DateTime TransactionTime { get; set; } = DateTime.UtcNow;

        /// <summary>Result of the order</summary>
        public EOrderState OrderState { get; set; }

        public EOrderPriceType PriceType { get; set; }

        public EOrderConditionType ConditionType { get; set; }


        /// <summary>Append another order to this order - order id and type must match</summary>
        /// <param name="other">Order to append</param>
        public void AppendOrderWithOrder(UserOrder other)
        {
            if ((OrderId != null) && (Symbol != null) && ((OrderId != other.OrderId) || (IsBuy != other.IsBuy) || (Symbol != other.Symbol)))
            {
                throw new InvalidOperationException("Appending orders requires order id, symbol and is buy to match");
            }

            decimal tradeSum = Amount + other.Amount;
            decimal baseAmount = Amount;
            Amount += other.Amount;
            FilledAmount += other.FilledAmount;
            Fees += other.Fees;
            SettlCurrency = other.SettlCurrency;
            AveragePrice = (AveragePrice * (baseAmount / tradeSum)) + (other.AveragePrice * (other.Amount / tradeSum));
            OrderId = other.OrderId;
            TransactionTime = TransactionTime == default(DateTime) ? other.TransactionTime : TransactionTime;
            Symbol = other.Symbol;
            IsBuy = other.IsBuy;
        }

        /// <summary>Returns a string that represents this instance.</summary>
        /// <returns>A string that represents this instance.</returns>
        public override string ToString()
        {
            return $"[{TransactionTime}], {(IsBuy ? "Buy" : "Sell")} {FilledAmount} of {Amount} {Symbol} {OrderState} at {AveragePrice}, fees paid {Fees} {SettlCurrency}";
        }
    }
}