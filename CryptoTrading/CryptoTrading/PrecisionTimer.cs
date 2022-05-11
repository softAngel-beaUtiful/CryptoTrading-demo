using CTP;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickQuant
{
    public class ExchangeTimeStru
    {
        public ExchangeType ExchangeID;
        public string ExchangTime;
    }
    public enum ExchangeTimeEvent
    {
        MorningAuctionStart,
        MorningAuctionClose,
        MorningOpen,
        MorningCFFEXOpen,
        MorningBreak,
        MorningBreakOff,
        MorningClose,
        AfternoonOpen,
        AfternoonCFFEXOpen,
        AfternoonSHFEBreak,
        AfternoonSHFEBreakOff,
        AfternoonClose,
        NightOpen,
        NightClose2300,
        NightClose2330,
        NightClose100,
        NightClose230,
        NoTrading
    }
    public class PrecisionTimer
    {
        Stopwatch stopWatch;
        double BaseSWTotalMilliseconds;
        DateTime EstimatedBaseSHFETime;        
        DateTime[] ExchangeTime = new DateTime[5]; // SHFETime,DCETime,CZCETime,CFFEXTime,INETime;
        bool EventTriggeredFirstTime = false;
             
        private double DefaultExchangeDelayMilliseconds= 30.0;  //系统默认的交易所延迟毫秒数
        private DateTime Today, Morning500,Morning910, Morning950, Morning1025, Morning1050, Morning1115, 
            Afternoon1230, Afternoon1315, Afternoon1355, Afternoon1415,Afternoon1450,Afternoon1510,
            Night2115, Night2315, Night2355, Night115, Night235;
        string dateAction;
        int i=0;
        public void SetUpMileStones()
        {
            Today = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
            Morning500 = Today.AddHours(5);
            Morning910 = Today.AddMinutes(550);
            Morning950 = Today.AddMinutes(590);
            Morning1025 = Today.AddMinutes(625);
            Morning1050 = Today.AddMinutes(650);            
            Afternoon1230 = Today.AddMinutes(750);
            Afternoon1315 = Today.AddMinutes(795);
            Afternoon1355 = Today.AddMinutes(835);
            Afternoon1415 = Today.AddMinutes(855);
            Afternoon1450 = Today.AddMinutes(890);
            Afternoon1510 = Today.AddMinutes(910);
            Night2115 = Today.AddMinutes(1275);
            Night2315 = Today.AddMinutes(1395);
            Night2355 = Today.AddMinutes(1435);
            Night115 = Today.AddDays(1);
            Night115 = Night115.AddMinutes(75);
            Night235 = Night115.AddMinutes(80);
        }
        public ExchangeTimeEvent EstTimeEvent()
        {
            DateTime curr = DateTime.Now;
            if (curr > Morning500 && curr < Morning910) return ExchangeTimeEvent.MorningOpen;
            else if (curr >= Morning910 && curr < Morning950) return ExchangeTimeEvent.MorningCFFEXOpen;
            else if (curr >= Morning950 && curr < Morning1025) return ExchangeTimeEvent.MorningBreak;
            else if (curr >= Morning1025 && curr < Morning1050) return ExchangeTimeEvent.MorningBreakOff;
            else if (curr >= Morning1050 && curr < Afternoon1230) return ExchangeTimeEvent.MorningClose;
            else if (curr >= Afternoon1230 && curr < Afternoon1315) return ExchangeTimeEvent.AfternoonCFFEXOpen;
            else if (curr >= Afternoon1315 && curr < Afternoon1355) return ExchangeTimeEvent.AfternoonOpen;
            else if (curr >= Afternoon1355 && curr < Afternoon1415) return ExchangeTimeEvent.AfternoonSHFEBreak;
            else if (curr >= Afternoon1415 && curr < Afternoon1450) return ExchangeTimeEvent.AfternoonSHFEBreakOff;
            else if (curr >= Afternoon1450 && curr < Afternoon1510) return ExchangeTimeEvent.AfternoonClose;
            else if (curr >= Afternoon1510 && curr < Night2115) return ExchangeTimeEvent.NightOpen;
            else if (curr >= Night2115 && curr < Night2315) return ExchangeTimeEvent.NightClose2300;
            else if (curr >= Night2315 && curr < Night2355) return ExchangeTimeEvent.NightClose2330;
            else if (curr >= Night2355 && curr < Night115) return ExchangeTimeEvent.NightClose100;
            else if (curr >= Night115 && curr < Night235) return ExchangeTimeEvent.NightClose230;
            else return ExchangeTimeEvent.NoTrading;
        }
        public PrecisionTimer()
        {
            stopWatch = new Stopwatch();
            stopWatch.Start();
            SetUpMileStones();
            dateAction = DateTime.Now.ToString("yyyy-MM-dd") + " ";
        }
        public void SetDefaultExchangeDelay(double totalms)
        {
            DefaultExchangeDelayMilliseconds = totalms;
        }
        /// <summary>
        /// 通过CTP传回的交易所时间的数组设置精准时钟和各个交易所相对上期所的时间偏移值
        /// 交易所的顺序为:SHFE，DCE，CZCE，CFFEX，INE
        /// 如果使用各个交易所的系统，情况又有不同，如易盛、
        /// </summary>
        /// <param name="exchangeTime"></param>
        public void LoginSetPrecisionTime(ExchangeTimeStru[] exchangeTime)   //根据精确到秒的交易所来校时
        {
            BaseSWTotalMilliseconds = stopWatch.Elapsed.TotalMilliseconds;
            dateAction = DateTime.Now.ToString("yyyy-MM-dd")+" ";
            for (int i = 1; i < 5; i++)
            {
                ExchangeTime[i] = DateTime.Parse(dateAction + exchangeTime[i].ExchangTime).AddMilliseconds(DefaultExchangeDelayMilliseconds);                 
            }           
        }
        /// <summary>
        /// 根据行情所带的精确到毫秒的行情时间来校时
        /// </summary>
        /// <param name="md"></param>
        public void MarketDateSetPreciseTimer(ThostFtdcDepthMarketDataField md)
        {
            BaseSWTotalMilliseconds = stopWatch.Elapsed.TotalMilliseconds;      
            switch (md.ExchangeID)
            {
                case "SHFE":
                    i = 0;
                    break;
                case "DCE":
                    i = 1;
                    break;
                case "CZCE":
                    i = 2;
                    break;
                case "CFFEX":
                    i = 3;
                    break;
                case "INE":
                    i = 4;
                    break;
            }
            ExchangeTime[i] = DateTime.Parse(md.ActionDay + " " + md.UpdateTime).AddMilliseconds(md.UpdateMillisec + DefaultExchangeDelayMilliseconds);
                       
        }
        /// <summary>
        ///  利用事件回调函数的事件发生时间来跟交易所时间校时
        ///  包括交易所合约的状态改变时间（如9：00：00开市）
        /// </summary>
        /// <param name="exchangetime"></param>
        public void EventSetPreciseTimer(ExchangeTimeStru exchangetime)
        {           
            double current = stopWatch.Elapsed.TotalMilliseconds- BaseSWTotalMilliseconds;
            DateTime currentTime = DateTime.Now;
            //BaseSWTotalMilliseconds = stopWatch.Elapsed.TotalMilliseconds;
            ExchangeTimeEvent timeevent = EstTimeEvent();
            switch (exchangetime.ExchangeID)
            {
                case ExchangeType.SHFE:
                    break;
                case ExchangeType.DCE:
                    break;
                case ExchangeType.CZSE:
                    break;
                case ExchangeType.CFFEX:
                    break;
                case ExchangeType.INE:
                    break;
            }
            switch (timeevent)
            {
                case ExchangeTimeEvent.MorningAuctionClose:
                    EstimatedBaseSHFETime = DateTime.Parse(Trader.ActionDay + " 8:59:00").AddMilliseconds(DefaultExchangeDelayMilliseconds);
                    break;
                case ExchangeTimeEvent.MorningOpen:
                    EstimatedBaseSHFETime = DateTime.Parse(Trader.ActionDay + " 09:00:00").AddMilliseconds(DefaultExchangeDelayMilliseconds);
                    break;
                case ExchangeTimeEvent.MorningBreak:
                    EstimatedBaseSHFETime = DateTime.Parse(Trader.ActionDay + " 10:15:00").AddMilliseconds(DefaultExchangeDelayMilliseconds);
                    break;
                case ExchangeTimeEvent.MorningClose:
                    EstimatedBaseSHFETime = DateTime.Parse(Trader.ActionDay + " 11:30:00").AddMilliseconds(DefaultExchangeDelayMilliseconds);
                    break;
                case ExchangeTimeEvent.AfternoonOpen:
                    EstimatedBaseSHFETime = DateTime.Parse(Trader.ActionDay + " 13:00:00").AddMilliseconds(DefaultExchangeDelayMilliseconds);
                    break;
            }
           
           
        }
        public DateTime GetPrecisionTime()
        {
            return (EstimatedBaseSHFETime.AddMilliseconds(stopWatch.Elapsed.TotalMilliseconds-BaseSWTotalMilliseconds));
        }
        public int CalcTimeDiff(DateTime SHFEdate, DateTime date)
        {
            TimeSpan ts1 = new TimeSpan(SHFEdate.Ticks);
            TimeSpan ts2 = new TimeSpan(date.Ticks);
            TimeSpan ts3 = ts1.Subtract(ts2).Duration();
            //你想转的格式
            return ts3.Seconds;
        }
        ~PrecisionTimer()
        {
            stopWatch.Stop();            
        }
    }
}
