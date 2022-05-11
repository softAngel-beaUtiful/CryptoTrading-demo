using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;
using Newtonsoft.Json;
using CryptoTrading.ViewModel;
using CryptoTrading.Model;

namespace BitMex
{
    public class BitUtility
    {
        public static class BitmexAuthentication
        {

            public static long CreateAuthNonce(long? time = null)
            {

                return GetNonce();
               
            }

            public static string CreateAuthPayload(long nonce)
            {
                return "GET/realtime" + nonce;
            }

            public static string CreateSignature(string key, string message)
            {
                var keyBytes = Encoding.UTF8.GetBytes(key);
                var messageBytes = Encoding.UTF8.GetBytes(message);


                string ByteToString(byte[] buff)
                {
                    var builder = new StringBuilder();

                    for (var i = 0; i < buff.Length; i++)
                    {
                        builder.Append(buff[i].ToString("X2")); // hex format
                    }
                    return builder.ToString();
                }

                using (var hmacsha256 = new HMACSHA256(keyBytes))
                {
                    byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
                    return ByteToString(hashmessage).ToLower();
                }
            }
        }
        public static OrderData[] ConvertBMOrderOrderData(BMOrder[] bmarr)
        {
            //CryptoTrading.Utility.WriteMemLog(bmarr);
            OrderData[] orderdataarray = new OrderData[bmarr.Length];
            for (int i=0;i<bmarr.Length;i++)
            {
                orderdataarray[i] = new OrderData()
                {
                    AvgPrice = (decimal)(bmarr[i].AvgPx ?? 0),
                    OrderSize = bmarr[i].OrderQty ?? 0,
                    InstrumentID = bmarr[i].Symbol,
                    InvestorID = bmarr[i].Account.ToString(),
                    QuantUnfilled = bmarr[i].leavesQty ?? 0,
                    QuantFilled = bmarr[i].CumQty ?? 0,
                    CreateTime = (bmarr[i].Timestamp??DateTime.Now).ToString("yyyy-MM-dd HH:mm:ss.ff"),                    
                    OrderID = bmarr[i].OrderId,
                    OrderPrice = (decimal)(bmarr[i].Price??0),
                    ExchangeID = CryptoTrading.EnuExchangeID.BitMex,
                    Direction = bmarr[i].Side==BitmexSide.Buy? CryptoTrading.TradeDirection.Long:bmarr[i].Side==BitmexSide.Sell? CryptoTrading.TradeDirection.Short:CryptoTrading.TradeDirection.Unknown,
                    StatusMsg = bmarr[i].Text,                                      
                };  
                
                switch (bmarr[i].OrdStatus)
                {
                    case BMOrderStatus.Filled:
                        orderdataarray[i].OrderStatus = CryptoTrading.OrderStatusType.全部成交;
                        break;
                    case BMOrderStatus.Canceled:
                        orderdataarray[i].OrderStatus = CryptoTrading.OrderStatusType.已撤单;
                        break;
                    case BMOrderStatus.New:
                        orderdataarray[i].OrderStatus = CryptoTrading.OrderStatusType.未成交;
                        break;
                    case BMOrderStatus.PartiallyFilled:
                        orderdataarray[i].OrderStatus = CryptoTrading.OrderStatusType.部分成交;
                        break;
                    default:
                    case BMOrderStatus.Undefined:
                        orderdataarray[i].OrderStatus = CryptoTrading.OrderStatusType.未知;
                        break;
                }
                switch (bmarr[i].OrdType)
                {
                    case "Limit":
                        orderdataarray[i].OrderPriceType = CryptoTrading.OrderPriceType.限价;
                        break;
                    case "Market":
                        orderdataarray[i].OrderPriceType = CryptoTrading.OrderPriceType.市价;
                        break;
                    default:
                        break;
                }                
            }
            return orderdataarray;
        }
        public static string BuildQueryData(Dictionary<string, string> param)
        {
            if (param == null)
                return "";

            StringBuilder b = new StringBuilder();
            foreach (var item in param)
                b.Append(string.Format("&{0}={1}", item.Key, WebUtility.UrlEncode(item.Value)));

            try { return b.ToString().Substring(1); }
            catch (Exception) { return ""; }
        }
        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }
        public static long GetNonce()
        {
            try
            {
                string path = Environment.CurrentDirectory + "\\configurations\\bitmexconfigs\\BitMexGetnonce.txt";
                var getno = File.ReadAllText(path);
                NonceClass nonce = JsonConvert.DeserializeObject<NonceClass>(getno);
                nonce.Nonce += 1;
                var w = JsonConvert.SerializeObject(nonce);
                File.WriteAllText(path, w);
                return nonce.Nonce;
            }
            catch(Exception ee)
            {
                return 0;
            }

            //DateTime yearBegin = new DateTime(1990, 1, 13);
            //return 9007175606319811;// 
            //return DateTime.UtcNow.Ticks - yearBegin.Ticks;
        }
        public static string BuildJSON(Dictionary<string, string> param)
        {
            if (param == null)
                return "";

            var entries = new List<string>();
            foreach (var item in param)
                entries.Add(string.Format("\"{0}\":\"{1}\"", item.Key, item.Value));

            return "{" + string.Join(",", entries) + "}";
        }
        public static byte[] GetHashCode(string apiKey, string message)
        {            
            //string message = method + url + nonce + postData;
            byte[] signatureBytes = hmacsha256(Encoding.UTF8.GetBytes(apiKey), Encoding.UTF8.GetBytes(message));
            return signatureBytes;
        }
        public static byte[] hmacsha256(byte[] keyByte, byte[] messageBytes)
        {
            using (var hash = new HMACSHA256(keyByte))
            {
                return hash.ComputeHash(messageBytes);
            }
        }
    }
    internal static class BmxValidations
    {
        /// <summary>
        /// It throws <exception cref="BitmexBadInputException"></exception> if value is null or empty/white spaces
        /// </summary>
        /// <param name="value">The value to be validated</param>
        /// <param name="name">Input parameter name</param>
        public static void ValidateInput(string value, string name)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new BitmexBadInputException($"Input string parameter '{name}' is null or empty. Please correct it.");
            }
        }

        /// <summary>
        /// It throws <exception cref="BitmexBadInputException"></exception> if value is null
        /// </summary>
        /// <param name="value">The value to be validated</param>
        /// <param name="name">Input parameter name</param>
        public static void ValidateInput<T>(T value, string name)
        {
            if (Equals(value, default(T)))
            {
                throw new BitmexBadInputException($"Input parameter '{name}' is null. Please correct it.");
            }
        }

        /// <summary>
        /// It throws <exception cref="BitmexBadInputException"></exception> if collection is null or collection is empty
        /// </summary>
        /// <param name="collection">The collection to be validated</param>
        /// <param name="name">Input parameter name</param>
        public static void ValidateInputCollection<T>(IEnumerable<T> collection, string name)
        {
            // ReSharper disable once PossibleMultipleEnumeration
            ValidateInput(collection, name);

            // ReSharper disable once PossibleMultipleEnumeration
            if (!collection.Any())
            {
                throw new BitmexBadInputException($"Input collection '{name}' is empty. Please correct it.");
            }
        }

        /// <summary>
        /// It throws <exception cref="BitmexBadInputException"></exception> if value is not in specified range
        /// </summary>
        /// <param name="value">The value to be validated</param>
        /// <param name="name">Input parameter name</param>
        /// <param name="minValue">Minimal value of input</param>
        /// <param name="maxValue">Maximum value of input</param>
        public static void ValidateInput(int value, string name, int minValue = int.MinValue, int maxValue = int.MaxValue)
        {
            if (value < minValue)
            {
                throw new BitmexBadInputException($"Input parameter '{name}' is lower than {minValue}. Please correct it.");
            }
            if (value > maxValue)
            {
                throw new BitmexBadInputException($"Input parameter '{name}' is higher than {maxValue}. Please correct it.");
            }
        }

        /// <summary>
        /// It throws <exception cref="BitmexBadInputException"></exception> if value is not in specified range
        /// </summary>
        /// <param name="value">The value to be validated</param>
        /// <param name="name">Input parameter name</param>
        /// <param name="minValue">Minimal value of input</param>
        /// <param name="maxValue">Maximum value of input</param>
        public static void ValidateInput(long value, string name, long minValue = long.MinValue, long maxValue = long.MaxValue)
        {
            if (value < minValue)
            {
                throw new BitmexBadInputException($"Input parameter '{name}' is lower than {minValue}. Please correct it.");
            }
            if (value > maxValue)
            {
                throw new BitmexBadInputException($"Input parameter '{name}' is higher than {maxValue}. Please correct it.");
            }
        }

        /// <summary>
        /// It throws <exception cref="BitmexBadInputException"></exception> if value is not in specified range
        /// </summary>
        /// <param name="value">The value to be validated</param>
        /// <param name="name">Input parameter name</param>
        /// <param name="minValue">Minimal value of input</param>
        /// <param name="maxValue">Maximum value of input</param>
        public static void ValidateInput(double value, string name, double minValue = double.MinValue, double maxValue = double.MaxValue)
        {
            if (value < minValue)
            {
                throw new BitmexBadInputException($"Input parameter '{name}' is lower than {minValue}. Please correct it.");
            }
            if (value > maxValue)
            {
                throw new BitmexBadInputException($"Input parameter '{name}' is higher than {maxValue}. Please correct it.");
            }
        }
    }
    public class BitmexBadInputException : BitmexException
    {
        public BitmexBadInputException()
        {
        }

        public BitmexBadInputException(string message) : base(message)
        {
        }

        public BitmexBadInputException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
    public class BitmexException : Exception
    {
        public BitmexException()
        {
        }

        public BitmexException(string message)
            : base(message)
        {
        }

        public BitmexException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
    public static class BitmexTime
    {
        public static readonly DateTime UnixBase = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long NowMs()
        {
            var substracted = DateTime.UtcNow.Subtract(UnixBase);
            return (long)substracted.TotalMilliseconds;
        }

        public static long NowTicks()
        {
            return DateTime.UtcNow.Ticks - UnixBase.Ticks;
        }

        public static DateTime ConvertToTime(long timeInMs)
        {
            return UnixBase.AddMilliseconds(timeInMs);
        }
    }
}
