using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoUserCenter.Models
{
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
}
