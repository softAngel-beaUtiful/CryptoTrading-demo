using System;
using System.ComponentModel;

namespace CryptoUserCenter.Views
{
    public class CandleStick:  INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private DateTime datetime;
        public DateTime Datetime { get { return datetime; } set { datetime = value; NotifyPropertyChanged("Datetime"); } }
        private double high;
        public double High { get { return high; } set { high = value; NotifyPropertyChanged("High"); } }
        private double low;
        public double Low { get { return low; } set { low = value; NotifyPropertyChanged("Low"); } }
        private double open;
        public double Open { get { return open; } set { open = value; NotifyPropertyChanged("Open"); } }
        private double close;
        public double Close { get { return close; } set { close = value; NotifyPropertyChanged("Close"); } }
        private double volume;
        public double Volume { get { return volume; } set { volume = value; NotifyPropertyChanged("Volume"); } }

    }
}