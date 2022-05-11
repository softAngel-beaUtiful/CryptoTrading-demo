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
using System.Xml;

namespace CryptoTrading
{
    /// <summary>
    /// Interaction logic for CommonDisplayWindow.xaml
    /// </summary>
    public partial class CommonDisplayWindow 
    {
        public CommonDisplayWindow()
        {
            InitializeComponent();
        }
        private void CommonDisplayWin_Loaded(object sender, RoutedEventArgs e)
        {
            chkBoxUsePreSettlementPrice.IsChecked = Trader.Configuration.UsePreSettlementPrice;
        }

        private void CommonDisplayWin_Closed(object sender, EventArgs e)
        {

        }
        public override bool Save()
        {
            Trader.Configuration.UsePreSettlementPrice = chkBoxUsePreSettlementPrice.IsChecked.Value;
            return true;
        }
        public override bool Cancel()
        {
            return base.Cancel();
        }
    }
}
