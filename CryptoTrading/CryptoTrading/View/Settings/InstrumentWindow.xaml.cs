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
using CryptoTrading.Model;
using CryptoTrading.ViewModel;

namespace CryptoTrading
{
    /// <summary>
    /// Interaction logic for InstCommissionWindow.xaml
    /// </summary>
    public partial class InstrumentWindow : Window
    {
        public InstrumentWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            cmbExchanges.ItemsSource = new List<string>()
            {
                EnuExchangeID.OkexFutures.ToString(),
                EnuExchangeID.OkexSwap.ToString(),
                EnuExchangeID.OkexSpot.ToString(),
                EnuExchangeID.BitMex.ToString(),
                EnuExchangeID.Binance.ToString(),
                EnuExchangeID.Bitfinex.ToString(),
                EnuExchangeID.Huobi.ToString(),
                EnuExchangeID.Bittrex.ToString(),
                EnuExchangeID.GDAX.ToString(),
                EnuExchangeID.Gemini.ToString(),
                EnuExchangeID.Poloniex.ToString(),
                EnuExchangeID.Kraken.ToString()
            };           

            Utility.LoadConfiguration(dgInst, DataGridType.Instrument);
            dgInst.ItemsSource = TQMainModel.dicInstrumentData.Values;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
        }
        

    }
}
