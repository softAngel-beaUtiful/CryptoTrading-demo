using System;
using System.Collections.Generic;
using System.Text;

namespace TickQuant.Common
{
    public static class OkexInstrumentConverter
    {
        public static string ToOkexFuturesInstrumentid(this string instrumentid)
        {
            string returnvalue;
            if (instrumentid.Contains("this"))
                returnvalue = instrumentid.Replace("_this_week", $"-USD-{WeekQuarterConverter.GetCurrWeek()}").ToUpper();
            else if (instrumentid.Contains("next"))
                returnvalue = instrumentid.Replace("_next_week", $"-USD-{WeekQuarterConverter.GetNextWeek()}").ToUpper();
            else
                returnvalue = instrumentid.Replace("_quarter", $"-USD-{WeekQuarterConverter.GetCurrentQuarterWeek()}").ToUpper();
            return returnvalue;
        }

        public static string ToOkexSwapInstrumentid(this string instrumentid)
        {
            return instrumentid.Replace("_", "-").ToUpper();
        }
    }
    public class WeekQuarterConverter
    {
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

