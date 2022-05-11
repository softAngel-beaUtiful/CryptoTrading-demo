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
    /// Interaction logic for OrderBoardWindow.xaml
    /// </summary>
    public partial class OrderBoardWindow 
    {
        public OrderBoardWindow()
        {
            InitializeComponent();
        }

        private void OrderBoardWin_Loaded(object sender, RoutedEventArgs e)
        {
            //加载配置
            chkBoxOrderConfirm.IsChecked = Trader.Configuration.ConfirmOrder;
        }

        private void OrderBoardWin_Closed(object sender, EventArgs e)
        {
        }

        private void chkBoxOrderConfirm_Click(object sender, RoutedEventArgs e)
        {

        }

        private void chkBoxFillDfltQuant_Click(object sender, RoutedEventArgs e)
        {

        }

        private void chkBoxOrderPrice_Click(object sender, RoutedEventArgs e)
        {

        }

        public override bool Save()
        {
            //保存配置
            Trader.Configuration.ConfirmOrder = chkBoxOrderConfirm.IsChecked.Value;
            //保存配置到文件

            return true;
        }

        public override bool Cancel()
        {
            return base.Cancel();
        }
    }
}
