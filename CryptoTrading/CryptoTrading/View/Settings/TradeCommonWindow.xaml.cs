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
    /// Interaction logic for TradeCommonWindow.xaml
    /// </summary>
    public partial class TradeCommonWindow
    {
        public TradeCommonWindow()
        {
            InitializeComponent();
        }

        private void TradeCommonWin_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void TradeCommonWin_Closed(object sender, EventArgs e)
        {

        }

        public override bool Cancel()
        {
            return base.Cancel();
        }

        public override bool Save()
        {
            return base.Save();
        }
    }
}
