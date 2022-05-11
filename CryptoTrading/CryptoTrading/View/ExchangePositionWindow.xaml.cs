using System.Collections.Generic;
using System.Windows;
using System.Linq;
using CryptoTrading.ViewModel;
using CryptoTrading.Model;

namespace CryptoTrading
{
    /// <summary>
    /// Interaction logic for TransferPositionWindow.xaml
    /// </summary>
    public partial class ExchangePositionWindow : Window
    {
        private string InstrumentID;
        public string DesInstrument;
        public ExchangePositionWindow(string Instrument)
        {
            InitializeComponent();
            lblSrcInstrument.Content = string.Format("将{0}移仓至", Instrument);
            InstrumentID = Instrument;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string productID = Utility.GetProductID(InstrumentID);

            List<string> InstrumentList = new List<string>();
            InstrumentList = TQMainModel.dicInstrumentData.Values.Where(x => x.InstrumentID == InstrumentID)
                .Select(x => x.InstrumentID).ToList();


            InstrumentList.Remove(InstrumentID);
            InstrumentList.Sort();
            cmbBoxInstruments.ItemsSource = InstrumentList;
            cmbBoxInstruments.SelectedIndex = 0;
        }
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            DesInstrument = cmbBoxInstruments.SelectedItem.ToString();
            this.Close();
        }

        private void btnCancle_Click(object sender, RoutedEventArgs e)
        {
            DesInstrument = string.Empty;
            this.Close();
        }

    }
}
