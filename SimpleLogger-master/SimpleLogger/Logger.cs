using Newtonsoft.Json;
using SimpleLogger.Logging;
using SimpleLogger.Logging.Handlers;
using SimpleLogger.Logging.Module;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace SimpleLogger
{
    public enum LogCategory
    {
        Debug=0,
        Info,
        Trade,
        Error,
        Fatal
    }
    public static class Logger
    {
        private static readonly LogPublisher LogPublisher;
        private static readonly ModuleManager ModuleManager;
        
        private static readonly object Sync = new object();
        
        static Logger()
        {
            lock (Sync)
            {
                LogPublisher = new LogPublisher();
                ModuleManager = new ModuleManager();               
            }
        }
        public static void DefaultInitialization()
        {
            LoggerHandlerManager
                .AddHandler(new ConsoleLoggerHandler())
                .AddHandler(new FileLoggerHandler());
        }

        public static ILoggerHandlerManager LoggerHandlerManager
        {
            get { return LogPublisher; }
        }         
        
        public static void Log<TClass>(TClass tclass, LogCategory category)
        {
            if (tclass is LogMessage)
            {
                Log(tclass as LogMessage, false, false);
                return;
            }
            var classtype = typeof(TClass).Name;
            var logmessage = new LogMessage()
            {
                CallingClass = "",
                CallingMethod = "",
                Data = (tclass is string) ? tclass as string : JsonConvert.SerializeObject(tclass),
                DataCategory = category,
                DateTime = DateTime.UtcNow
            };            
            Log(logmessage, false, false);
        }
        public static void Log<TClass>(TClass tclass, LogCategory category, string callingmethod= null, string callingclass = null, bool OutputToRedis=false, bool OutputToMySql=false ) where TClass:class
        {                                
            if (tclass is LogMessage)
            {
                Log(tclass as LogMessage, OutputToRedis, OutputToMySql);
                return;
            }
            var classtype = typeof(TClass).Name;
            var logmessage = new LogMessage()
            {
                CallingClass= callingclass,
                CallingMethod = callingmethod,
                Data = (tclass is string)? tclass as string: JsonConvert.SerializeObject(tclass),
                DataCategory = category,
                DateTime=DateTime.UtcNow
            };
           
            Log(logmessage, OutputToRedis, OutputToMySql);
        }

        public static void Log(LogMessage logmessage, bool OutputToRedis = false, bool OutputToMySQL=false)
        {
            lock (Sync)
            {
                ModuleManager.BeforeLog();
                LogPublisher.Publish(logmessage);
                ModuleManager.AfterLog(logmessage);
            }
        }

        private static MethodBase GetCallingMethodBase(StackFrame stackFrame)
        {
            return stackFrame == null
                ? MethodBase.GetCurrentMethod() : stackFrame.GetMethod();
        }

        private static StackFrame FindStackFrame()
        {
            var stackTrace = new StackTrace();
            for (var i = 0; i < stackTrace.GetFrames().Count(); i++)
            {
                var methodBase = stackTrace.GetFrame(i).GetMethod();
                var name = MethodBase.GetCurrentMethod().Name;
                if (!methodBase.Name.Equals("Log") && !methodBase.Name.Equals(name))
                    return new StackFrame(i, true);
            }
            return null;
        }   
        
        public static IEnumerable<LogMessage> Messages
        {
            get { return LogPublisher.Messages; }
        }       

        public static ModuleManager Modules
        {
            get { return ModuleManager; }
        }

        public static bool StoreLogMessages
        { 
            get { return LogPublisher.StoreLogMessages; }
            set { LogPublisher.StoreLogMessages = value; }
        }      
    }
}
