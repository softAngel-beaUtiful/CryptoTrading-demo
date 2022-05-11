using Newtonsoft.Json;
using System.Collections.Generic;
namespace TickQuant.Common
{
    public class ClientRequest
    {
        [JsonProperty("op")]
        public string OP { get; set; }
        [JsonProperty("token")]
        public string Token { get; set; }
        [JsonProperty("userid")]
        public string UserID;
        [JsonProperty("strategyid")]
        public string StrategyID;
        [JsonProperty("data")]
        public StrategyParams Data { get; set; }
    }
}
