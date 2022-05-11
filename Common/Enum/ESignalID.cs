using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace TickQuant.Common
{
    public enum ESignalID
    {
        [JsonProperty(PropertyName = "arb_long")]
        arb_long,
        [JsonProperty(PropertyName = "arb_short")]
        arb_short,
        [JsonProperty(PropertyName = "trend_long")]
        trend_long,
        [JsonProperty(PropertyName = "trend_short")]
        trend_short,
        [JsonProperty(PropertyName = "arb_closelong")]
        arb_closelong,
        [JsonProperty(PropertyName = "arb_closeshort")]
        arb_closeshort,
        [JsonProperty(PropertyName = "trend_closelong")]
        trend_closelong,
        [JsonProperty(PropertyName = "trend_closeshort")]
        trend_closeshort,
    }
}
