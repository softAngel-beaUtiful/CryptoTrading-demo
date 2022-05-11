using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using System.Xml.Serialization;
using CryptoTrading.TQLib;

namespace CryptoTrading.Model
{
    [Serializable]
    public class ColorSet: ObservableObject
    {

        //public List<TQColor> TQColorList = new List<TQColor>();


        //private TQColor _WindowBackground;

        //[XmlElement("WindowBackground")]
        //public TQColor WindowBackground
        //{
        //    get { return _WindowBackground; }
        //    set
        //    {
        //        _WindowBackground = value;
        //        NotifyPropertyChanged("WindowBackground");
        //    }
        //}

        private TQColor _GridBackground;

        [XmlElement("GridBackground")]
        public TQColor GridBackground
        {
            get { return _GridBackground; }
            set
            {
                _GridBackground = value;
                NotifyPropertyChanged("GridBackground");
            }
        }

        private TQColor _GridForeground;

        [XmlElement("GridForeground")]
        public TQColor GridForeground
        {
            get { return _GridForeground; }
            set
            {
                _GridForeground = value;
                NotifyPropertyChanged("GridForeground");
            }
        }


        private TQColor _ChangeUpForeground;

        [XmlElement("ChangeUpForeground")]
        public TQColor ChangeUpForeground
        {
            get { return _ChangeUpForeground; }
            set
            {
                _ChangeUpForeground = value;
                NotifyPropertyChanged("ChangeUpForeground");
            }
        }

        private TQColor _ChangeDownForeground;

        [XmlElement("ChangeDownForeground")]
        public TQColor ChangeDownForeground
        {
            get { return _ChangeDownForeground; }
            set
            {
                _ChangeDownForeground = value;
                NotifyPropertyChanged("ChangeDownForeground");
            }
        }

        private TQColor _ChangeStableForeground;

        [XmlElement("ChangeStableForeground")]
        public TQColor ChangeStableForeground
        {
            get { return _ChangeStableForeground; }
            set
            {
                _ChangeStableForeground = value;
                NotifyPropertyChanged("ChangeStableForeground");
            }
        }

    }
}
