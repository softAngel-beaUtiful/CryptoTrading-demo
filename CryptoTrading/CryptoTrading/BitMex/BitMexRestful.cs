using CryptoTrading;
using CryptoTrading.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BitMex
{
    public partial class BitMexBase
    {
        public string GetPosition()
        {
            var param = new Dictionary<string, string>();
            param["symbol"] = "XBTUSD";
            return Query("GET", "/position", param, true);
        }
        public string GetQuote(string instrument)
        {
            var param = new Dictionary<string, string>();
            param["symbol"] = instrument;
            return Query("GET", "/trade", param, false);
        }
        public string GetQuoteBucketed(KLineRequest klinerequest)
        {
            var param = new Dictionary<string, string>();
            param["binSize"] = "1m";
            param["partial"] = "true";
            param["symbol"] = klinerequest.symbol;
            param["count"] = "100";
            //param["startTime"] = "2018-11-10 0:01";
            //param["endTime"] = "2018-11-10 0:04";
            
            var r =  Query("GET", "/quote/bucketed", param, false);
            return r;
        }
        public string SendOrderRest(OrderData order)
        {
            return PostOrders(order);
        }
        public string PostOrders(OrderData order)
        {
            var param = new Dictionary<string, string>();
            param["symbol"] = order.InstrumentID;
            param["side"] = order.Direction == TradeDirection.Long ? "Buy" : "Sell";
            param["orderQty"] = order.OrderSize.ToString();
            param["ordType"] = order.OrderPriceType == OrderPriceType.市价 ? "Market" : "Limit";
            param["price"] = order.OrderPrice.ToString();
            CryptoTrading.Utility.WriteMemLog("price " + param["price"]);
            return Query("POST", "/order", param, true);
        }

        public async Task<string> GetAuthRest()
        {
            var param = new Dictionary<string, string>();
            param["symbol"] = "XBTUSD";
            return Query("GET", "", param, true);
        }
        private int GetExpires()
        {
            return (int)DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeSeconds();
        }
        private string Query(string method, string function, Dictionary<string, string> param = null, bool auth = false, bool json = false)
        {
            string paramData = json ? BuildJSON(param) : BuildQueryData(param);
            string url = "/api/v1" + function + ((method == "GET" && paramData != "") ? "?" + paramData : "");
            string postData = (method != "GET") ? paramData : "";

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(domain + url);
            webRequest.Method = method;
            if (auth)
            {
                int expires = GetExpires();//(Int32)(DateTime.UtcNow.AddDays(7).Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                string message = method + url + expires.ToString() + postData;
                byte[] signatureBytes = BitUtility.hmacsha256(Encoding.UTF8.GetBytes(marketaccess.secret_key), Encoding.UTF8.GetBytes(message));
                string signatureString = BitUtility.ByteArrayToString(signatureBytes);
                webRequest.Headers.Add("api-expires", expires.ToString());
                webRequest.Headers.Add("api-key", marketaccess.api_key);
                webRequest.Headers.Add("api-signature", signatureString);
            }
            try
            {
                if (postData != "")
                {
                    webRequest.ContentType = json ? "application/json" : "application/x-www-form-urlencoded";
                    var data = Encoding.UTF8.GetBytes(postData);
                    using (var stream = webRequest.GetRequestStream())
                    {
                        stream.Write(data, 0, data.Length);
                    }
                }
                using (WebResponse webResponse = webRequest.GetResponse())
                using (Stream str = webResponse.GetResponseStream())
                using (StreamReader sr = new StreamReader(str))
                {
                    return sr.ReadToEnd();
                }
            }
            catch (WebException wex)
            {
                using (HttpWebResponse response = (HttpWebResponse)wex.Response)
                {
                    if (response == null)
                        throw;

                    using (Stream str = response.GetResponseStream())
                    {
                        using (StreamReader sr = new StreamReader(str))
                        {
                            return sr.ReadToEnd();
                        }
                    }
                }
            }
        }
        public string GetOrders(string InstrumentID)
        {
            var param = new Dictionary<string, string>();
            param["symbol"] = InstrumentID;
            //param["filter"] = "{\"open\":true}";
            //param["columns"] = "";
            //param["count"] = 100.ToString();
            //param["start"] = 0.ToString();
            //param["reverse"] = false.ToString();
            //param["startTime"] = "";
            //param["endTime"] = "";
            return Query("GET", "/order", param, true);
        }
        public string GetPosition(string InstrumentID)
        {
            var param = new Dictionary<string, string>();
            param["symbol"] = InstrumentID;
            return Query("Get", "/position", param, true);
        }
        public string DeleteOrders(string orderid)
        {
            var param = new Dictionary<string, string>();
            param["orderID"] = orderid;
            param["text"] = "cancel order by ID";
            return Query("DELETE", "/order", param, true, true);
        }
        private string BuildQueryData(Dictionary<string, string> param)
        {
            if (param == null)
                return "";

            StringBuilder b = new StringBuilder();
            foreach (var item in param)
                b.Append(string.Format("&{0}={1}", item.Key, WebUtility.UrlEncode(item.Value)));

            try { return b.ToString().Substring(1); }
            catch (Exception) { return ""; }
        }

        private string BuildJSON(Dictionary<string, string> param)
        {
            if (param == null)
                return "";

            var entries = new List<string>();
            foreach (var item in param)
                entries.Add(string.Format("\"{0}\":\"{1}\"", item.Key, item.Value));

            return "{" + string.Join(",", entries) + "}";
        }

        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        private long GetNonce()
        {
            DateTime yearBegin = new DateTime(1990, 1, 1);
            return DateTime.UtcNow.Ticks - yearBegin.Ticks;
        }      
    }
}
