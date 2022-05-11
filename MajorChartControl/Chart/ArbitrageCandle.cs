using System;
using System.ComponentModel;
using System.Collections.Generic;
using Trady.Core.Infrastructure;
using Trady.Analysis;

namespace MajorControl
{
    public class ArbitrageCandle: IOhlcv
    {
        public DateTime Datetime { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Open { get; set; }
        public decimal Close { get; set; }
        public decimal SHigh { get; set; }
        public decimal SLow { get; set; }
        public decimal SOpen { get; set; }
        public decimal SClose { get; set; }
        public decimal Volume { get; set; }
        public AnalyzableTick<(decimal?, decimal?, decimal?)>[] IndicatorValues { get; set; } = new AnalyzableTick<(decimal?, decimal?, decimal?)>[] { };
        DateTimeOffset ITick.DateTime { get { return Datetime; } }        
    }
}