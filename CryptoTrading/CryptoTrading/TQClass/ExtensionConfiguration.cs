using System.Collections.Generic;
using System.Xml.Serialization;
using System.Xml;
using System.ComponentModel;
using System.Text;
using System.Linq;
using System.Windows.Input;
using System;
using CryptoTrading.TQLib;
using CryptoTrading.ViewModel;

namespace CryptoTrading
{
    /// <summary>
    /// 用户扩展配置
    /// </summary>
    [XmlRoot("Configuration")]
    public class ExtensionConfiguration
    {
        public ExtensionConfiguration()
        {
            ConditionalOrderNo = 0;
            Combos = new List<XmlCombo>();
        }
        [XmlElement]
        public int ConditionalOrderNo { get; set; }

        [XmlArray("CustomProducts")]
        [XmlArrayItem("item")]
        public List<XmlCombo> Combos { get; set; }

        public void AddOrUpdateCombo(XmlCombo product)
        {
            int itemIndex = Combos.FindIndex(x => x.InstrumentID == product.InstrumentID);
            if (itemIndex < 0)
            {
                Combos.Add(product);
            }
            else
            {
                Combos[itemIndex] = product;
            }
        }               
        public XmlCombo GetXmlCombo(string instrumentID)
        {
            var xmlProduct = Combos.Find(x => x.InstrumentID == instrumentID);
            if (xmlProduct == null || string.IsNullOrEmpty(xmlProduct.InstrumentID))
            { return null; }
            else
            {
                //CustomProduct cust = new CustomProduct(xmlProduct.InstrumentID, TQMain.T.main);
                return xmlProduct;
            }
        }
        public void Save()
        {
            TQXmlHelper.XmlSerializeToFile(this, Trader.ExtCfgFile, Encoding.UTF8);
        }
    }

    public class XmlCombo
    {
        [XmlAttribute]
        public string InstrumentID { get; set; }
        [XmlAttribute]
        public string InstrumentName { get; set; }
        [XmlAttribute]
        public double PriceTick { get; set; }
    }
}
