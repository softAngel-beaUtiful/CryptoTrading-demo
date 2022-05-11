using System.Collections.Generic;

namespace TickQuant.Common
{
    public class SerieType
    {
        public string ExchangeID;
        public string Symbol;
        public string Period;  //K线的秒数
        public ESymbolStatus SymbolStatus;
        public RobotSymbolParam SymbolParam;
        public Dictionary<EIndicatorType, List<decimal[]>> SymbolIndicatorParams;
    }
    public class SignalSerie
    {
        public string ExchangeID;
        public string Symbol;
        public string Period;  //K线的秒数
        public Dictionary<EIndicatorType, List<decimal[]>> SymbolIndicatorsParams;
    }
}
