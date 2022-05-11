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
    public partial class NoticesSetWindow 
    {
        public NoticesSetWindow()
        {
            InitializeComponent();
        }

        private void NoticesSetWin_Loaded(object sender, RoutedEventArgs e)
        {
            txtUserMail.Text = Trader.Configuration.Mail.UserName;
            pwdBoxMail.Password = Trader.Configuration.Mail.Password;
        }

        private void NoticesSetWin_Closed(object sender, EventArgs e)
        {

        }
        public override bool Save()
        {
            Trader.Configuration.Mail.UserName = txtUserMail.Text;
            Trader.Configuration.Mail.Password = pwdBoxMail.Password;
            return true;
        }
        public override bool Cancel()
        {
            return base.Cancel();
        }
    }
}
