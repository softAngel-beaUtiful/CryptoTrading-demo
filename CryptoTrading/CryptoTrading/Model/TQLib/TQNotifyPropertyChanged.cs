using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CryptoTrading.TQLib
{
    /// <summary>
    /// 实现 INotifyPropertyChanged 接口
    /// 派生类可重写或直接使用 NotifyPropertyChanged 函数
    /// </summary>
    public class ObservableObject : INotifyPropertyChanged
    {        
        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }       
    }
}
