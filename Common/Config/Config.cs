using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
namespace TickQuant.Common
{
    public class ChartConfig
    {
        public int CandlePeriod { get; set; } = 5;
        public int LeftWidth { get; set; } = 10;
        public int RightWidth { get; set; } = 100;
        public int UpGap { get; set; } = 2;
        public int DownGap { get; set; } = 2;
        public int MaxLookBackPeriod { get; set; } = 1500;
        public int CandleStickWidth { get; set; } = 1;
        public Color PositiveColor { get; set; } = Color.Green;
        public Color NegativeColor { get; set; } = Color.Red;
        public KeyValuePair<string, Dictionary<EIndicatorType, object>> ComplexInstrument { get; set; } //  = new KeyValuePair<string, Dictionary<string, object>>("Binance:BTCUSDT", new Dictionary<string, object>()); 
        //.Add（"BOLL", (100, 2.0) ） };
    }
    public class WebSocketConfig
    {
        [JsonProperty("ip")]
        public string Ip { get; set; }
        [JsonProperty("market_server_port")]
        public int MarketServerPort { get; set; }
        [JsonProperty("trade_server_port")]
        public int TradeServerPort { get; set; }
        [JsonProperty("depth_server_port")]
        public int DepthServerPort { get; set; }
        [JsonProperty("strategy_server_port")]
        public int StrategyServerPort { get; set; }
    }

    public class RedisConfig
    {
        [JsonProperty("ip")]
        public string Ip { get; set; }
        [JsonProperty("Port")]
        public int Port { get; set; }

        [JsonProperty("password")]
        public string PassWord { get; set; }

        [JsonProperty("kline_m_max")]
        public int KlineMMax { get; set; }

        [JsonProperty("kline_h_max")]

        public int KlineHMax { get; set; }

        public override string ToString()
        {
            return $"{Ip}:{Port},password = {PassWord}";
        }
    }

    public class ExchangeAPIConfig
    {
        [JsonProperty("exchange_name")]
        public string ExchangeName { get; set; }
        [JsonProperty("public_api_key")]
        public string PublicApiKey { get; set; }
        [JsonProperty("private_api_key")]
        public string PrivateApiKey { get; set; }

        [JsonProperty("pass_phrase")]
        public string Passphrase { get; set; }


    }

    public class MysqlConfig
    {
        [JsonProperty("ip")]
        public string Ip { get; set; }
        [JsonProperty("port")]
        public int Port { get; set; }
        [JsonProperty("user")]
        public string User { get; set; }
        [JsonProperty("password")]
        public string PassWord { get; set; }
        public bool FirstUse { get; set; }
        public int MySqlConnectionCacheSize { get; set; }
        public override string ToString()
        {
            return Ip + ":" + Port.ToString();
        }
    }

    public class ExchangesConfig
    {
        Dictionary<string, string[]> Pairs { get; set; }
    }

    public static class Config
    {
        public static JToken ReadJson()
        {
            if (!File.Exists("Config.json"))
            {
                throw new Exception("file config.json doesnot exist");
            }
            StreamReader streamReader = new StreamReader("Config.json");
            var jsonFile = streamReader.ReadToEnd();
            return JToken.Parse(jsonFile);

        }
        public static RedisConfig GetRedisConfig()
        {
            RedisConfig redisConfig = new RedisConfig();
            var jtoken = ReadJson();
            redisConfig = jtoken["redis"].ToObject<RedisConfig>();
            return redisConfig;
        }

        public static WebSocketConfig GetWebSocketConfig()
        {
            WebSocketConfig webSocketConfig = new WebSocketConfig();
            var jtoken = ReadJson();
            webSocketConfig = jtoken["websocket"].ToObject<WebSocketConfig>();
            return webSocketConfig;
        }

        public static MysqlConfig GetMysqlConfig()
        {
            MysqlConfig mysqlConfig = new MysqlConfig();
            var jtoken = ReadJson();
            mysqlConfig = jtoken["mysql"].ToObject<MysqlConfig>();
            if (mysqlConfig.MySqlConnectionCacheSize <= 0)
                mysqlConfig.MySqlConnectionCacheSize = 36;
            return mysqlConfig;
        }

        public static ExchangeAPIConfig GetExchangeApiConfig(string exchangeName)
        {
            ExchangeAPIConfig exchangeAPIConfig = null;
            var jtoken = ReadJson();                    // 读取JSON
            var apis = jtoken["exchange_apis"];   // 读取APIS
            //apis.SelectToken(x)

            // 循环遍历取值
            foreach (var item in apis)
            {
                string pName = ((JProperty)item.First).Name;
                if (exchangeName == pName)
                {
                    var pv = item.SelectToken(pName);
                    exchangeAPIConfig = pv.ToObject<ExchangeAPIConfig>();
                    exchangeAPIConfig.ExchangeName = pName;
                    return exchangeAPIConfig;
                }
            }
            return new ExchangeAPIConfig();
        }

        public static Dictionary<string, string[]> GetExchanges()
        {
            Dictionary<string, string[]> pairs = new Dictionary<string, string[]>();
            var jtoken = ReadJson();
            var token = jtoken["exchanges"];
            foreach (var item in token)
            {
                var name = ((JProperty)item.First).Name;
                var allSymbolList = new List<string>();
                var symbols = (JArray)item[name];
                foreach (var symbol in symbols)
                {
                    allSymbolList.Add(symbol.Value<string>());
                }
                pairs.Add(name, allSymbolList.ToArray());
            }

            return pairs;
        }
        public static Dictionary<string, SignalConfig> GetSignalConfig()
        {
            Dictionary<string, SignalConfig> signalconfig; // = new Dictionary<string, SignalConfig>();
            var jtoken = ReadJson();
            signalconfig = jtoken["SignalConfigs"].ToObject<Dictionary<string, SignalConfig>>();
            Dictionary<string, SignalConfig> newconfig = new Dictionary<string, SignalConfig>();
            foreach (var s in signalconfig)
                newconfig.Add(s.Key.Replace("this_week", "usd_"+GetCurrWeek()).Replace("next_week","usd_"+GetNextWeek()).Replace("quarter","usd_"+GetCurrentQuarterWeek()), 
                   s.Value);            
            return newconfig;
        }
        public class SignalConfig
        {
            public int bollperiod { get; set; }
            public decimal bollratio1 { get; set; }
            public decimal bollratio2 { get; set; }
            public decimal openprice { get; set; }
            public decimal? closeprice { get; set; }
            public decimal? stoploss { get; set; }
        }

        public static string GetCurrWeek()
        {
            DateTime now = DateTime.UtcNow;
            int weekday = (int)now.DayOfWeek;
            if (((int)weekday < 5) || (weekday == 5 && now.Hour < 8))
                return now.AddDays(5 - weekday).ToString("yyMMdd");
            else
                if (weekday == 5)
                return now.AddDays(7).ToString("yyMMdd");
            else
                return now.AddDays(6).ToString("yyMMdd");
        }
        public static string GetNextWeek()
        {
            DateTime now = DateTime.UtcNow;
            var weekday = now.DayOfWeek;
            if (((int)weekday < 5) || ((int)weekday == 5 && now.Hour < 8))
                return now.AddDays(12 - (int)weekday).ToString("yyMMdd");
            else if ((int)weekday == 5) return now.AddDays(14).ToString("yyMMdd");
            else return now.AddDays(13).ToString("yyMMdd");
        }
        public static string ConvertDateToWeekQuarter(string date) //"yyMMdd"
        {
            string thisweek = GetCurrWeek();
            string nextweek = GetNextWeek();
            string thisquarter = GetCurrentQuarterWeek();
            if (date == thisweek) return "this_week";
            else if (date == nextweek) return "next_week";
            else if (date == thisquarter) return "quarter";
            else return null;
        }
        public static string GetCurrentQuarterWeek()
        {
            string LastFridayOfThisQuarter;
            DateTime now = DateTime.UtcNow;
            if (now.Hour >= 8)
            {
                now.AddDays(1);
            }
            int Y = now.Year;
            int M = now.Month;
            int D = 1;
            int H = now.Hour;
            if (M <= 3)
            {
                M = 4;
            }
            else if (M <= 6)
            {
                M = 7;
            }
            else if (M <= 9)
            {
                M = 10;
            }
            else //if (M <= 12)
            {
                M = 1;
            }
            DateTime dateTime = new DateTime(Y, M, D, 0, 0, 0).AddMilliseconds(-1);
            if (M == 1)
                dateTime.AddYears(1);
            while (true)
            {
                if ((int)dateTime.DayOfWeek != 5)
                {
                    dateTime = dateTime.AddDays(-1);
                }
                else
                {
                    dateTime.AddHours(-16);
                    LastFridayOfThisQuarter = dateTime.ToString("yyMMdd");
                    if (LastFridayOfThisQuarter != GetCurrWeek() && LastFridayOfThisQuarter != GetNextWeek()
                        && dateTime > DateTime.UtcNow)
                        return LastFridayOfThisQuarter;
                    else
                    {
                        dateTime = DateTime.UtcNow.AddMonths(4);
                        Y = dateTime.Year;
                        M = dateTime.Month;
                        dateTime = new DateTime(Y, M, D, 0, 0, 0).AddMilliseconds(-1);
                        //M = dateTime.AddMonths(3).Month;
                        while (true)
                        {
                            if ((int)dateTime.DayOfWeek != 5)
                            {
                                dateTime = dateTime.AddDays(-1);
                            }
                            else
                                return dateTime.ToString("yyMMdd");
                        }
                    }
                }
            }
        }
    }
}
