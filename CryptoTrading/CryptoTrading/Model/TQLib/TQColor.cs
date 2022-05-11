using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace CryptoTrading.TQLib
{
    /// <summary>
    /// 颜色及对应的画刷，建立这个类有2个目的
    /// 1，颜色序列化
    /// 2，颜色需要和界面双向绑定，不过画刷保留初始值（单向绑定）
    /// </summary>
    [Serializable]
    public class TQColor: ObservableObject, IXmlSerializable
    {
        //public System.Windows.Media.Color Color = new System.Windows.Media.Color();

        private System.Windows.Media.Color _Color;

        [XmlIgnore]
        public System.Windows.Media.Color Color
        {
            get { return _Color; }
            set
            {
                _Color = value;
                NotifyPropertyChanged("Color");
                NotifyPropertyChanged("ColorBrush");
            }
        }

        public SolidColorBrush ColorBrush
        {
            get { return new SolidColorBrush(this.Color); }
        }


        public TQColor() { }

        //[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        //public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        //{
        //    string str = string.Format("{0}:{1}:{2}:{3}", Color.A, Color.R, Color.B, Color.G);
        //    info.AddValue("ARGBColor", str);
        //}

        //protected TQColor(SerializationInfo info, StreamingContext context)
        //{
        //    byte a, r, g, b;
        //    string ARGBColor = info.GetString("ARGBColor");
        //    string[] pieces = ARGBColor.Split(new char[] { ':' });

        //    a = byte.Parse(pieces[1]);
        //    r = byte.Parse(pieces[2]);
        //    g = byte.Parse(pieces[3]);
        //    b = byte.Parse(pieces[4]);

        //    this.Color = System.Windows.Media.Color.FromArgb(a, r, g, b);
        //}

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            byte a, r, g, b;
            //string ARGBColor = reader.ReadContentAsString();
            //string[] pieces = ARGBColor.Split(new char[] { ':' });
            //a = byte.Parse(pieces[1]);
            //r = byte.Parse(pieces[2]);
            //g = byte.Parse(pieces[3]);
            //b = byte.Parse(pieces[4]);

            a = byte.Parse(reader.GetAttribute("A"));
            r = byte.Parse(reader.GetAttribute("R"));
            g = byte.Parse(reader.GetAttribute("G"));
            b = byte.Parse(reader.GetAttribute("B"));

            this.Color = System.Windows.Media.Color.FromArgb(a, r, g, b);
        }

        public void WriteXml(XmlWriter writer)
        {
            //string str = string.Format("{0}:{1}:{2}:{3}", Color.A, Color.R, Color.B, Color.G);

            writer.WriteAttributeString("A", Color.A.ToString());
            writer.WriteAttributeString("R", Color.R.ToString());
            writer.WriteAttributeString("G", Color.G.ToString());
            writer.WriteAttributeString("B", Color.B.ToString());
        }
    }
}
