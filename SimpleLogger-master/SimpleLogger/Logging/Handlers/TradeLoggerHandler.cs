using Newtonsoft.Json;
using SimpleLogger.Logging.Formatters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleLogger.Logging.Handlers
{
    public class TradeLoggerHandler : ILoggerHandler
    {
        private readonly string _fileName;
        private readonly string _directory;
        private readonly ILoggerFormatter _loggerFormatter;
        private readonly EventHandler<string>  OutputToTradeLog;
                

        public TradeLoggerHandler(EventHandler<string> output)
        {
            OutputToTradeLog += output;
        }
               
        public TradeLoggerHandler(ILoggerFormatter loggerFormatter, string fileName, string directory)
        {
            _loggerFormatter = loggerFormatter;
            _fileName = fileName;
            _directory = directory;
        }

        public void Publish(LogMessage logMessage)
        {
            OutputToTradeLog?.Invoke(this, JsonConvert.SerializeObject(logMessage));
        }        
    }
}

