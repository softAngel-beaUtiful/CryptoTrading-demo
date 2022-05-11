using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;
using CryptoTrading.Model;

namespace CryptoTrading.TQClass
{
    /// <summary>
    /// 交易信息配置文件
    /// （包含合约、交易费、保证金率的信息）
    /// </summary>
    [XmlRoot("Config")]
    public class InstrumentInfoConfiguration
    {
        public InstrumentInfoConfiguration()
        {
            ListInstrument = new List<InstrumentData>();
            CommissionRates = new List<InstrumentCommissionRate>();
            MarginRates = new List<InstrumentMarginRate>();
        }
        /// <summary>
        /// 合约列表
        /// </summary>
        [XmlArrayItem("item")]
        [XmlArray("Instruments", Order = 1)]
        public List<InstrumentData> ListInstrument { get; set; }

        /// <summary>
        /// 交易费列表
        /// </summary>
        [XmlArrayItem("item")]
        [XmlArray("CommissionRates",Order =2)]
        public List<InstrumentCommissionRate> CommissionRates { get; set; }

        /// <summary>
        /// 保证金率列表
        /// </summary>
        [XmlArrayItem("item")]
        [XmlArray("MarginRates", Order = 3)]
        public List<InstrumentMarginRate> MarginRates { get; set; }
    }
}
