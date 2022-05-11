using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace TickQuant.Common
{/*{
  "UserID":"MajorShi",
  "Password":"518052",
  "ExchangeKey":{
    "okex":{
      "PublicKey":"dec6fd7c-d70a-445b-810b-5c0838ec6bac",
      "SecretKey":"39B4DF0176697A7E912071657C9232E7",
      "PassPhrase":"518052"
    }
  }
}*/
    public class UserInfo
    {
        public string UserID;
        public string Password;
        public Dictionary<string,ExchangeKey> DictExchangeKey;
    }

    public class AuthUserInfo
    {
        [JsonProperty("uid")]
        public string UID;
        [JsonProperty("username")]
        public string UserName;
        [JsonProperty("email")]
        public string Email;
        [JsonProperty("mobile")]
        public string Mobile;
        [JsonProperty("nickname")]
        public string NickName;
        [JsonProperty("head_image")]
        public string HeadImage;
        [JsonProperty("desc")]
        public string Description;
        [JsonProperty("status")]
        public long Status;
        [JsonProperty("is_bind")]
        public long IsBind;
        [JsonProperty("mobile_prefix")]
        public string MobilePrefix;
        [JsonProperty("expire_at")]
        public DateTime ExpireTime;
        [JsonProperty("strategy_id")]
        public string StragteID;

    }
}
