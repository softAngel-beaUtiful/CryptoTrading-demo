using CryptoTrading.OkexSpace;
using System;
using System.Collections.Generic;

namespace CryptoTrading
{
    public partial class Okex
    {
        //public static Dictionary<string, string> dict;
        private const string url = "wss://real.okex.com:10440/websocket/okexapi";
        //WebSocketService websocketservice;
        public WebSocketBase websocketbase;
        
        
        public Okex()
        {            
            //websocketservice = new BusinessServiceImpl();
            websocketbase = new WebSocketBase();           
        }

        private void Wb_OnRtnPositions(object sender, ClassRspPosition e)
        {
            //foreach( var v in e.positions)
            //Console.WriteLine("OnRtnPositionInfo " +v.contract_id+ " "+ v.contractposition);
        }

        private void Wb_OnRspLogin(object sender, ResponseLogin e)
        {
            Console.WriteLine("Login Response: " + e.data);
        }

        private void Wb_OnRspForecastPrice(object sender, ForecastPrice e)
        {
            Console.WriteLine("ForecastPrice for " + e.channel + ": " + e.data);
        }
        
        #region Response Event Handling Methods
       
        
        
        public string channeltocurr(string str)
        {
            int s = str.LastIndexOf('_');
            
            string temp = str.Substring(++s,3);
            return temp;
        }
        
        private void Wb_OnRspMessage(object sender, string e)
        {
            Console.WriteLine(DateTime.Now.ToString()+ "  "+e);
            //Console.WriteLine("----------------------------");
        }
        private void Wb_HeartBeat(object sender, string e)
        {
            //foreach (var v in e) Console.Write(v + " ");
            //Console.WriteLine(e+"   -------------");            
        }        
        private void Wb_OnError(object sender, WebSocketSharp.ErrorEventArgs e)
        {
            //Console.WriteLine("On Error, " + e.Message);
            BitMexUtility.WriteMemFile(e.Message);
        }
        #endregion
                                
        public string InstrumentIDToContractType(string instrumentID)
        {
            DateTime dt;
            string sss = "20" + instrumentID.Substring(3);
            DateTime.TryParse(sss.Substring(0, 4) + "-" + sss.Substring(4, 2) + "-" + sss.Substring(6, 2), out dt);
            DateTime now = DateTime.Now;
            string[] da = new string[] { "2018-03-30", "2018-06-29", "2018-09-30", "2018-12-31" };
            List<DateTime> ld = new List<DateTime>();
            DateTime dd;
            foreach (var vs in da)
            {
                DateTime.TryParse(vs, out dd);
                ld.Add(dd);
            }
            if (ld.Contains(dt))
                return "quarter";
            dt = dt.AddHours(16);
            if (dt > now.AddDays(7))
                return "next_week";
            return "this_week";
        }
        
        
        private string ContractTypeToDate(string contracttype)
        {
            DateTime now = DateTime.Now;
            int ii = (int)now.DayOfWeek;
            string thisweek = String.Empty, nextweek = String.Empty, quarter = String.Empty;
            switch (now.DayOfWeek)
            {
                case (DayOfWeek.Friday):
                    {
                        if (now.Hour > 16)
                        {
                            thisweek = now.AddDays(7).ToString("yyMMdd");
                            nextweek = now.AddDays(14).ToString("yyMMdd");
                        }
                        else
                        {
                            thisweek = now.ToString("yyMMdd");
                            nextweek = now.AddDays(7).ToString("yyMMdd");

                        }
                    }
                    break;
                case (DayOfWeek.Monday):
                case (DayOfWeek.Sunday):
                case (DayOfWeek.Tuesday):
                case (DayOfWeek.Wednesday):
                case (DayOfWeek.Thursday):
                    {
                        ii = 5 - ii;
                        thisweek = now.AddDays(ii).ToString("yyMMdd");
                        nextweek = now.AddDays(7 + ii).ToString("yyMMdd");
                    }
                    break;
                default:
                case (DayOfWeek.Saturday):
                    {
                        thisweek = now.AddDays(6).ToString("yyMMdd");
                        nextweek = now.AddDays(13).ToString("yyMMdd");
                    }
                    break;
            }
            string[] da = new string[] { "2018-03-30", "2018-06-30", "2018-09-30", "2018-12-31" };
            List<DateTime> ld = new List<DateTime>();
            DateTime dd;
            foreach (var vs in da)
            {
                DateTime.TryParse(vs, out dd);
                dd = dd.AddHours(16);
                ld.Add(dd);
            }
            for (int j = 0; j < 4; j++)
            {
                if (now < ld[j])
                {
                    quarter = ld[j].ToString("yyMMdd");
                    break;
                }
            }
            switch (contracttype.Substring(3))
            {
                default:
                case ("this_week"): return thisweek;
                case ("next_week"): return nextweek;
                case ("quarter"): return quarter;
            }
        }                   
        
        
        
        
    }   
}
