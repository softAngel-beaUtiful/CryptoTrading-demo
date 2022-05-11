using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CryptoTrading
{
    /// <summary>
    /// Interaction logic for NoticesSetWindow.xaml
    /// </summary>
    public partial class LocalDataMaintainSetWindow
    {
        public LocalDataMaintainSetWindow()
        {
            InitializeComponent();
        }


        private void LocalDataMaintainSetWind_Loaded(object sender, RoutedEventArgs e)
        {
            chkBoxInitCommissions.IsChecked = Trader.Configuration.LocalMaintainSet.IsInitQryCommission;
            chkBoxInitInstruments.IsChecked = Trader.Configuration.LocalMaintainSet.IsInitQryInstrument;
            chkBoxInitMargins.IsChecked = Trader.Configuration.LocalMaintainSet.IsInitQryMargin;
            chkBoxInitProducts.IsChecked = Trader.Configuration.LocalMaintainSet.IsInitQryProduct;
        }

        private void LocalDataMaintainSetWind_Closed(object sender, EventArgs e)
        {
        }

        public override bool Save()
        {

            Trader.Configuration.LocalMaintainSet.IsInitQryCommission = chkBoxInitCommissions.IsChecked.Value;
            Trader.Configuration.LocalMaintainSet.IsInitQryInstrument = chkBoxInitInstruments.IsChecked.Value;
            Trader.Configuration.LocalMaintainSet.IsInitQryMargin = chkBoxInitMargins.IsChecked.Value;
            Trader.Configuration.LocalMaintainSet.IsInitQryProduct = chkBoxInitProducts.IsChecked.Value;
            return true;
        }

        public override bool Cancel()
        {
            return base.Cancel();
        }
    }
}
