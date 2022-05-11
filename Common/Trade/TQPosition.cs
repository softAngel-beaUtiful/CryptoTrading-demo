using TickQuant.Common;

namespace TickQuant.Strategy
{
    public class TQPosition
    {
        /// <summary>
        /// UserID
        /// </summary>		
        public string UserID { get; set; }
        /// <summary>
        /// RunMode
        /// </summary>		
        public ERunMode RunMode { get; set; }
        /// <summary>
        /// StrategyID
        /// </summary>		
        public string StrategyID { get; set; }
        /// <summary>
        /// BackTestID
        /// </summary>		
        public string BackTestID { get; set; }
        /// <summary>
        /// ExchangeID
        /// </summary>		
        public string ExchangeID { get; set; }
        /// <summary>
        /// Symbol
        /// </summary>		
        public string Symbol { get; set; }
        /// <summary>
        /// HoldingCurrency
        /// </summary>		
        public string HoldingCurrency { get; set; }
        /// <summary>
        /// QuoteCurrency
        /// </summary>		
        public string QuoteCurrency { get; set; }
        /// <summary>
        /// PositionOffset
        /// </summary>		
        public string PositionOffset { get; set; }
        /// <summary>
        /// Account
        /// </summary>		
        public string Account { get; set; }
        /// <summary>
        /// LastPrice
        /// </summary>		
        public decimal LastPrice { get; set; }
        /// <summary>
        /// StraPosition
        /// </summary>		
        public decimal StraPosition { get; set; }
        /// <summary>
        /// FrozenStraPosition
        /// </summary>		
        public decimal FrozenStraPosition { get; set; }
        /// <summary>
        /// StraAvgCost
        /// </summary>		
        public decimal StraAvgCost { get; set; }
        /// <summary>
        /// RealizedPnl
        /// </summary>		
        public decimal RealizedPnl { get; set; }
        /// <summary>
        /// UnrealizePnl
        /// </summary>		
        public decimal UnrealizePnl { get; set; }
    }
}
