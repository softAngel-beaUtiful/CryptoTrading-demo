using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xceed.Wpf.Toolkit;

namespace CryptoTrading.TQLib
{
    public class TQXceedChildWindow: ChildWindow
    {
        public virtual bool Save()
        {
            return true;
        }
        public virtual bool Cancel()
        {
            return true;
        }
        public virtual bool Default()
        {
            return true;
        }
        public virtual bool Reload()
        {
            this.Visibility = System.Windows.Visibility.Visible;
            return true;
        }
    }
}
