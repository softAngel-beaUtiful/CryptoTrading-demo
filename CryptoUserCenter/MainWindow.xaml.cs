using CryptoUserCenter.Views;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CryptoUserCenter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {       
        Model model = new Model();
        private MultipleSpreadPairsDisplay singleSymbol;

        public MainWindow()
        {
            InitializeComponent();            
            DataContext = model;
            AccountsData.ItemsSource = model.ObservableAccounts;
            PositionsData.ItemsSource = model.ObservablePositions;
            OrdersData.ItemsSource = model.ObservableOrders;
            TradesData.ItemsSource = model.ObservableTrades;
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SpreadData_Click(object sender, RoutedEventArgs e)
        {
            List<string> mylist = new List<string>();
            EditArbiPairList editComplexInstrumentList = new EditArbiPairList(mylist);
            var v = editComplexInstrumentList.ShowDialog();
            if (v != true || mylist.Count == 0)
            {
                return;
            }
            singleSymbol = new MultipleSpreadPairsDisplay(mylist);
            singleSymbol.Closed += (s, f) =>
            {
                singleSymbol = null;
            };
            singleSymbol.Show();
        }

        private void OnWindowsClosed(object sender, EventArgs e)
        {
            for (int intCounter = App.Current.Windows.Count - 1; intCounter >= 0; intCounter--)
                App.Current.Windows[intCounter].Close();
            //Environment.Exit(0);
        }
    }
}
