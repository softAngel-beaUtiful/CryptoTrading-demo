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
    /// Interaction logic for AccountInfoWindow.xaml
    /// </summary>
    public partial class AccountInfoWindow : Window
    {
        public AccountInfoWindow()
        {
            InitializeComponent();
            //TQMain.T.accountInfoWin = this;

            //TQMain.T.ReqQryInvestor();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
