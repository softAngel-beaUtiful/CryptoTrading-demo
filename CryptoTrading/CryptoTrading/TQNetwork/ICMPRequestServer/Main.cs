using System;
using System.Diagnostics;

namespace ICMPRequestServer
{
    //   public static class PreciseTimer
    //   {
    //       public static Stopwatch stopWatch = new Stopwatch();
    //       static readonly ExchangeState[] exchangeStateArray = new ExchangeState[]
    //       {new ExchangeState(), new ExchangeState(), new ExchangeState(), new ExchangeState(), new ExchangeState()};
    //       //static DateTime[] ExchangeTime = new DateTime[5]; // SHFETime,DCETime,CZCETime,CFFEXTime,INETime;
    //       static System.Timers.Timer internalTimer = new System.Timers.Timer(30000);
    //       private static long DefaultExchangeDelayTicks = 300000;  //默认的交易所延迟Ticks
    //       private const long internalSoftwareExecutiveDelay = 100000;   //10ms
    //       static string ActionDay;
    //       static int i = 0;
    //       const double internalExecutiveDelay = 5;

    //       public static bool GetTimeEvent(ThostFtdcInstrumentStatusField status, TimeSpan elapsed, out TimeSpan diff, out int exchangeIndex)
    //       {
    //           switch (status.ExchangeID)
    //           {
    //               case "SHFE": exchangeIndex = 0; break;
    //               case "DCE": exchangeIndex = 1; break;
    //               case "CZCE": exchangeIndex = 2; break;
    //               case "CFFEX": exchangeIndex = 3; break;
    //               case "INE": exchangeIndex = 4; break;
    //               default: exchangeIndex = 0; break;
    //           }
    //           DateTime dt = exchangeStateArray[exchangeIndex].ExchangeTime + (elapsed.Subtract(exchangeStateArray[exchangeIndex].StopWatchBase));
    //           DateTime entertime = (DateTime.Parse(ActionDay + status.EnterTime)).AddTicks(DefaultExchangeDelayTicks);   //count in the exchangedelayticks
    //           double dd = (diff = dt.Subtract(entertime)).TotalMilliseconds;

    //           if (dd >= -1100 && dd <= 1100)
    //           {
    //               //Utility.WriteMemLog(TQMain.MemLog, string.Format("比对交易所传回时间(考虑网络延时）与本地交易所时间（{0}）{1} VS {2}",
    //               //   exchangeIndex, entertime.TimeOfDay, dt.TimeOfDay));
    //               return true;
    //           }
    //           else
    //               return false;
    //       }
    //       //static PreciseTimer()
    //       //{
    //       //    DateTime now = DateTime.Now;
    //       //    for (int i = 0; i < 5; i++)
    //       //        exchangeStateArray[i].ExchangeTime = now;
    //       //    internalTimer.AutoReset = false;
    //       //    internalTimer.Interval = 30000;
    //       //    internalTimer.Elapsed += ResumeExchangeFirstTimeEventFlag;

    //       //}
    //       public static void StartTimer()
    //       {
    //           stopWatch.Start();
    //       }
    //       public static void SetDefaultExchangeDelay(double totalms)
    //       {
    //           DefaultExchangeDelayTicks = (long)(totalms * 10000) + internalSoftwareExecutiveDelay;
    //       }
    //       public static void SetStopWatchBase()
    //       {
    //           for (int i = 0; i < 5; i++)
    //               exchangeStateArray[i].StopWatchBase = stopWatch.Elapsed;
    //       }
    //       /// <summary>
    //       /// 通过CTP传回的交易所时间的数组设置各个交易所的精准时钟
    //       /// 交易所的顺序为:SHFE，DCE，CZCE，CFFEX，INE
    //       /// 如果使用各个交易所的系统，情况又有不同，如易盛
    //       /// </summary>
    //       /// <param name="exchangetime"></param>
    //       public static void LoginSetPreciseTimer(ExchangeTimeStru[] exchangetime)   //根据精确到秒的交易所时间数组来校时，考虑进来网络延时
    //       {
    //           SetStopWatchBase();
    //           //SetDefaultExchangeDelay(Utility.PingServerDelayMS(0));   //parameter: Index of TradingServer IPAddress Array
    //           ActionDay = DateTime.Now.ToString("yyyy-MM-dd") + " ";
    //           for (int i = 0; i < 5; i++)
    //           {
    //               if (exchangetime[i].ExchangTime != "--:--:--")
    //                   exchangeStateArray[i].ExchangeTime = DateTime.Parse(ActionDay + exchangetime[i].ExchangTime).AddTicks(500 * 10000 + DefaultExchangeDelayTicks);
    //               else
    //                   exchangeStateArray[i].ExchangeTime = DateTime.Now; ;
    //           }
    //       }
    //       /// <summary>
    //       /// 根据行情所带的精确到毫秒的行情时间来校时
    //       /// </summary>
    //       /// <param name="md"></param>
    //       //public static void MarketDataSetPreciseTimer(MarketData md)
    //       //{
    //       //    switch (md.ExchangeID)
    //       //    {
    //       //        case "SHFE":
    //       //            i = 0;
    //       //            break;
    //       //        case "DCE":
    //       //            i = 1;
    //       //            break;
    //       //        case "CZCE":
    //       //            i = 2;
    //       //            break;
    //       //        case "CFFEX":
    //       //            i = 3;
    //       //            break;
    //       //        case "INE":
    //       //            i = 4;
    //       //            break;
    //       //    }
    //       //    exchangeStateArray[i].StopWatchBase = stopWatch.Elapsed;
    //       //    exchangeStateArray[i].ExchangeTime = DateTime.Parse(ActionDay + md.updateTime).AddTicks((long)(md.UpdateMillisec * 10000 + DefaultExchangeDelayTicks));
    //       //}

    //       //public static void ResumeExchangeFirstTimeEventFlag(object sender, ElapsedEventArgs e)
    //       //{
    //       //    for (int i = 0; i < 5; i++)
    //       //        exchangeStateArray[i].ExchangeTriggered = false;
    //       //}
    //       /// <summary>
    //       ///  利用事件回调函数的事件发生时间来跟交易所时间校时
    //       ///  ThostFtdcInstrumentStatus中包含ExchangeID，InstrumentID，InstrumentStatus,EnterTime,
    //       ///  InstrumentStatus，0-6，7种状态值
    //       ///  包括交易所合约的状态改变时间（如9：00：00开市）关键在于处理一种事件多个回调调用，需要加开关来仅处理第一个回调
    //       /// </summary>
    //       /// <param name="ThostFtdcInstrumentStatusField"></param>
    //       //public static bool EventSetPreciseTimer(ThostFtdcInstrumentStatusField status, out DateTime dt)
    //       //{
    //       //    var baseElapsedTimeSpan = stopWatch.Elapsed;
    //       //    dt = new DateTime();
    //       //    int ExchangeIndex;
    //       //    TimeSpan ts;
    //       //    if (!GetTimeEvent(status, baseElapsedTimeSpan, out ts, out ExchangeIndex))
    //       //        return false;
    //       //    if (exchangeStateArray[ExchangeIndex].ExchangeTriggered)   //only if not triggered, continue to update ExchangeTime
    //       //        return false;                                 //otherwise, return false;

    //       //    exchangeStateArray[ExchangeIndex].ExchangeTriggered = true;  //next same event from this exchange, won't update time.
    //       //    exchangeStateArray[ExchangeIndex].StopWatchBase = baseElapsedTimeSpan;
    //       //    dt = (DateTime.Parse(ActionDay + status.EnterTime)).AddTicks(DefaultExchangeDelayTicks);
    //       //    exchangeStateArray[ExchangeIndex].ExchangeTime = dt;
    //       //    if (!internalTimer.Enabled) internalTimer.Start();
    //       //    //Utility.WriteMemLog(TQMain.MemLog, string.Format("ExchangeTime differs from Event Enter Time: {0}", ts));
    //       //    for (int i = 0; i < 5; i++)
    //       //    {
    //       //        //Utility.WriteMemLog(TQMain.MemLog, string.Format(" Event ExchangeTime[{0}] {1} {2} ", i, exchangeStateArray[i].ExchangeTime, exchangeStateArray[i].StopWatchBase));
    //       //    }
    //       //    return true;
    //       //}
    //       public static DateTime GetPreciseTime()
    //       {
    //           return GetPreciseTime("");
    //       }

    //       public static DateTime GetPreciseTime(string exchangeid)
    //       {
    //           int exchangeIndex;
    //           switch (exchangeid)
    //           {
    //               case "DCE": exchangeIndex = 1; break;
    //               case "CZCE": exchangeIndex = 2; break;
    //               case "CFFEX": exchangeIndex = 3; break;
    //               case "INE": exchangeIndex = 4; break;
    //               case "SHFE":
    //               default: exchangeIndex = 0; break;
    //           }
    //           return GetPreciseTime(exchangeIndex);
    //       }
    //       public static DateTime GetPreciseTime(ExchangeType exchangetype)
    //       {
    //           return GetPreciseTime((int)exchangetype);
    //       }
    //       public static DateTime GetPreciseTime(int exchangeIndex)
    //       {
    //           return exchangeStateArray[exchangeIndex].ExchangeTime + (stopWatch.Elapsed.Subtract(exchangeStateArray[exchangeIndex].StopWatchBase));
    //       }
    //       public static void StopTimer()
    //       {
    //           stopWatch.Stop();
    //       }
    //       public static double GetStopWatchElapsed()
    //       {
    //           return stopWatch.Elapsed.TotalMilliseconds;
    //       }
    //   }
    class ICMPRequestServer
    {
        /// <summary>
        /// Demo of the icmpRequest utility.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            //test ping
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            Console.WriteLine("**********************TEST PING**********************\r\n");
            IcmpRequest myRequest = new IcmpRequest(new ToolsCommandRequest("www.microsoft.com", "Hello",
                CommandType.ping, 10, 32, 1000, 1000, 128),stopWatch);
            //Console.WriteLine("\r\n\r\n**********************TEST TRACERT*******************\r\n");
            //myRequest = new IcmpRequest(new ToolsCommandRequest("www.yahoo.com","Hello",CommandType.tracert,4,32,1000,1000,128));			
            Console.ReadLine();
        }
    }
}
