namespace SimpleLogger.Logging.Formatters
{
    internal class DefaultLoggerFormatter : ILoggerFormatter
    {
        public string ApplyFormat(LogMessage logMessage)
        {
            return string.Format("{0:yyyy.MM.dd HH:mm:ss.fff}: {1} {2} {3} {4}",logMessage.DataCategory,
                            logMessage.DateTime,  logMessage.CallingClass,
                            logMessage.CallingMethod, logMessage.Data);
        }
    }
}