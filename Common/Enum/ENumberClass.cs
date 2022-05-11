using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace TickQuant.Common
{
    public enum ENumberClass
    {
        [JsonProperty(PropertyName = "openprice")]
        OpenPrice,
        [JsonProperty(PropertyName = "closeprice")]
        ClosePrice,
        [JsonProperty(PropertyName = "movingclose")]
        MovingClose,
        [JsonProperty(PropertyName = "stoploss")]
        StopLoss
    }
}
