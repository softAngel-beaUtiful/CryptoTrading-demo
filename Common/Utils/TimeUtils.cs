using System;

namespace TickQuant.Common
{
    public static class TimeUtils
    {
        public static long ToUnixTime(this DateTime dateTime)
        {
            TimeSpan timeSpan = (dateTime - new DateTime(1970, 1, 1));
            long timestamp = (long)timeSpan.TotalSeconds;

            return timestamp;
        }
        public static long ToUnixMillsecondsTime(this DateTime dateTime)
        {
            TimeSpan timeSpan = (dateTime - new DateTime(1970, 1, 1));
            long timestamp = (long)timeSpan.TotalMilliseconds;

            return timestamp;
        }

        public static string GetTimestamp(DateTime dateTime)
        {
            var timestamp = ToUnixTime(dateTime);
            return timestamp.ToString();
        }

        public static string GetTimestamp()
        {
            return GetTimestamp(DateTime.UtcNow);
        }

        public static long GetTimestampLong(DateTime dateTime, bool isMsSencond = true)
        {
            if (isMsSencond)
            {
                return (dateTime.ToUniversalTime().Ticks - 621355968000000000) / 10000;
            }
            else
            {
                return (dateTime.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
            }
        }
    }
}
