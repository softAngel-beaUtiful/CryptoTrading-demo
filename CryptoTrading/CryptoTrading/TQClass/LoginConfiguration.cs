using System.Xml.Serialization;
using System.Collections.Generic;
using CryptoTrading.TQLib;
using CryptoTrading.Model;

namespace CryptoTrading
{
    
    [XmlRoot(ElementName = "Users")]
    public class Users
    {
        [XmlElement(ElementName = "User")]
        public List<User> User { get; set; }
    }
    [XmlRoot(ElementName = "User")]
    public class User
    {
        [XmlElement(ElementName = "Exchange")]
        public List<Exchange> Exchange { get; set; }
        [XmlAttribute(AttributeName = "UserID")]
        public string UserID { get; set; }
    }

    [XmlRoot(ElementName = "Exchange")]
    public class Exchange
    {
        [XmlAttribute(AttributeName = "ExchangeID")]
        public string ExchangeID { get; set; }
        [XmlAttribute(AttributeName = "PublicKey")]
        public string PublicKey { get; set; }
        [XmlAttribute(AttributeName = "PrivateKey")]
        public string PrivateKey { get; set; }
        [XmlAttribute(AttributeName = "PassPhrase")]
        public string PassPhrase { get; set; }
    }
    [XmlRoot("Config")]
    public class LoginConfiguration
    {
        #region 属性
        [XmlElement(Order = 1)]
        public InvestorInfo Investor { get; set; }

        [XmlArrayItem("item")]
        [XmlArray("RecentInvestors", Order = 2)]
        public List<InvestorInfo> RecentInvestors { get; set; }
        [XmlElement(Order = 3)]
        public bool IsSaveLoginRecord { get; set; }
        #endregion

        public LoginConfiguration()
        {
            RecentInvestors = new List<InvestorInfo>();
            IsSaveLoginRecord = true;
        }

        public void AddOrUpdateRecentInvestor(InvestorInfo investor)
        {
            int itemIndex = RecentInvestors.FindIndex(x => x.ID == investor.ID);
            if (itemIndex < 0)
            {
                RecentInvestors.Add(investor);
            }
            else
            {
                RecentInvestors[itemIndex] = investor;
            }
        }

    }
    /// <summary>
    /// 投资者信息
    /// </summary>
    public class InvestorInfo
    {
        /// <summary>
        /// 资金账号
        /// </summary>
        [XmlAttribute]
        public string ID { get; set; }

        /// <summary>
        /// 资金账号名
        /// </summary>
        [XmlAttribute]
        public string BrokerName { get; set; }
        /// <summary>
        /// 期商配置文件
        /// </summary>
        [XmlAttribute]
        public string BrokerConfig { get; set; }

        /// <summary>
        /// 期商网络
        /// </summary>
        [XmlAttribute]
        public string BrokerServer { get; set; }
        /// <summary>
        /// 最近一次登录时间，格式yyyyMMddHHmmss,如20160610123323
        /// </summary>
        [XmlAttribute]
        public string LastLoginTime { get; set; }
    }
}
