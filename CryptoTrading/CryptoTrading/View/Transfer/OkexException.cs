using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoTradingMaster.View.Transfer
{
    public class OkexException : Exception
    {
        private string errorcode;
        private string message;

        public OkexException(string errorcode, string message)
        {
            this.errorcode = errorcode;
            this.message = message;
        }
        public override string Message
        {
            get
            {
                return $"error code:{errorcode}  message:{message}";

            }
        }
        public override string ToString()
        {
            //            string s = null;
            //#if Debug
            //           s= this.StackTrace;
            //#endif
            return base.ToString(); //$"error code:{errorcode}  message:{message} \r\n{s}";
        }
    }
}
