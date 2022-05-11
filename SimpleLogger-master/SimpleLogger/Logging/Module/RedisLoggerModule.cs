using Newtonsoft.Json;
using StackExchange.Redis;
using System;
namespace SimpleLogger.Logging.Module
{
    public class RedisConfiguration
    {
        public string Password { get; private set; }
        public string Host { get; private set; }
        public int Port { get; private set; }
        public string UserID { get; set; }
        public string StrategyID { get; set; }
        public RedisConfiguration(string userid, string strategyid, string host, int port, string password)
        {
            StrategyID = strategyid;
            UserID = userid;
            Host = host;
            Port = port;
            Password = password;
        }
    }
    public class RedisLoggerModule : LoggerModule
    {
        private ConnectionMultiplexer ConnectionRedis;
        public override string Name { get { return "RedisLoggerModule"; } }
        public string UserID;
        public string StrategyID;
        private string RedisIP;
        private int Port;
        private string Password;
        private IDatabase dbredis;   

        public RedisLoggerModule( RedisConfiguration redisconfig)
        {
            UserID = redisconfig.UserID;
            StrategyID = redisconfig.StrategyID;
            RedisIP = redisconfig.Host;
            Port = redisconfig.Port;
            Password = redisconfig.Password;
        }

        public override void Initialize()
        {
            ConnectionRedis = ConnectionMultiplexer.Connect(RedisIP + ":" + Port + ", password=" + Password);
            dbredis = ConnectionRedis.GetDatabase();
        }

        public override void AfterLog(LogMessage logMessage)
        {
            dbredis.StringAppend("Logs" + ":" + DateTime.UtcNow.Month.ToString() + DateTime.UtcNow.Day.ToString() + ":" + UserID + ":" + StrategyID, JsonConvert.SerializeObject(logMessage) + "\n");
        }

        public override void ExceptionLog(Exception exception)
        {
            dbredis.StringAppend("Error", exception.Message);
        }
    }
}
