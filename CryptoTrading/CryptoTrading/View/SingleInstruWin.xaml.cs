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
using System.Threading;
using CryptoTrading.ViewModel;
using CryptoTrading.Model;

namespace CryptoTrading
{
    /// <summary>
    /// Interaction logic for SingleInstruWin.xaml
    /// </summary>
    public partial class SingleInstruWin : System.Windows.Window
    {
        public string InstrumentID { get; set; }
        public TQMain mdv;
        public DepthMarketDataField marketdata;

        public SingleInstruWin(TQMain mainwindow)
        {
            mdv = mainwindow;
            InitializeComponent();
            ;
            InstrumentID = Trader.CurrInstrumentDict.ToList()[mainwindow.MarketDataGrid.SelectedIndex].Key;
            marketdata = new DepthMarketDataField(InstrumentID);
            this.DataContext = new { ThreadId = Thread.CurrentThread.ManagedThreadId };
            thistxtbx.Text = DateTime.Now.ToLongTimeString();
            dgbx1.ItemsSource = mainwindow.Viewmodel.MajorMarketDataView;            
        }
        public void DisplayInfo()
        {

        }

    }
}
