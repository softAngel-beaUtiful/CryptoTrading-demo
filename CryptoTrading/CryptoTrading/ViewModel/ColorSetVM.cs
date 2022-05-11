using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media;
using CryptoTrading.TQLib;
using CryptoTrading.Model;

namespace CryptoTrading.ViewModel
{
    class ColorSetVM : INotifyPropertyChanged
    {

        public ColorSetVM()
        {
            ColorSetModelObj = Trader.Configuration.ColorSetModelObj;              
        }

        private ColorSet _ColorSetModelObj;

        public ColorSet ColorSetModelObj
        {
            get { return _ColorSetModelObj; }
            set
            {
                _ColorSetModelObj = value;
                //NotifyPropertyChanged("ColorSetObj");
            }
        }
        public SolidColorBrush GridForegroundBrush
        {
            get
            {
                return new SolidColorBrush(ColorSetModelObj.GridForeground.Color);
            }
        }

        public SolidColorBrush GridBackgroundBrush
        {
            get
            {
                return new SolidColorBrush(ColorSetModelObj.GridBackground.Color);
            }
        }
        public SolidColorBrush ChangeDownForegroundBrush
        {
            get
            {
                return new SolidColorBrush(ColorSetModelObj.ChangeDownForeground.Color);
            }
        }
        public SolidColorBrush ChangeUpForegroundBrush
        {
            get
            {
                return new SolidColorBrush(ColorSetModelObj.ChangeUpForeground.Color);
            }
        }

        public SolidColorBrush ChangeStableForegroundBrush
        {
            get
            {
                return new SolidColorBrush(ColorSetModelObj.ChangeStableForeground.Color);
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
