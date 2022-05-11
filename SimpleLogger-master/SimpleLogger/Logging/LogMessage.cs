using System;
using SimpleLogger.Logging.Formatters;

namespace SimpleLogger
{
    public class LogMessage
    {
        public DateTime DateTime { get; set; }   
        public LogCategory DataCategory { get; set; }
        public string Data { get; set; }
        public string CallingClass { get; set; }
        public string CallingMethod { get; set; }      
        public string UserID { get; set; }
        public string StrategyID { get; set; }
        public string BackTestID { get; set; }

        public LogMessage() { }
        public LogMessage( LogCategory dataType, string jsontext, DateTime dateTime, string callingClass, string callingMethod, string userId, string strategyId, string backtestId)
        {
            DataCategory = dataType;
            Data = jsontext;
            DateTime = dateTime;
            CallingClass = callingClass;
            CallingMethod = callingMethod;
            UserID = userId;
            StrategyID = strategyId;
            BackTestID = backtestId;
        }

        public override string ToString()
        {
            return new DefaultLoggerFormatter().ApplyFormat(this);
        }
    }
}
