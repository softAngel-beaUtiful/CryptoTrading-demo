using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.ObjectModel;
using CryptoTrading.TQLib;

namespace CryptoTrading
{
    /// <summary>
    /// Interaction logic for RecentUserWindow.xaml
    /// </summary>
    public partial class RecentUserWindow : Window
    {
        public RecentUserWindow()
        {
            InitializeComponent();
        }
        ObservableCollection<ExchangeAccountInfo> recentUsers = new ObservableCollection<ExchangeAccountInfo>();
        public ExchangeAccountInfo selectedInvestor;
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (dgRecentUsers.SelectedIndex >= 0)
            {
                selectedInvestor = dgRecentUsers.SelectedItem as ExchangeAccountInfo;
            }
            this.DialogResult = true;
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgRecentUsers.SelectedIndex < 0)
            {
                return;
            }
            var user = dgRecentUsers.SelectedItem as ExchangeAccountInfo;
            recentUsers.Remove(user);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (TQMain.LoginConfig.RecentInvestors.Count > 0)
            {
                TQMain.LoginConfig.RecentInvestors.ForEach((Action<InvestorInfo>)(item =>
                {
                    recentUsers.Add(new ExchangeAccountInfo() { ID = item.ID, BrokerName = item.BrokerName, BrokerServer = item.BrokerServer,BrokerConfig= item.BrokerConfig });
                }));
            }
            chkAutoSaveLoginRecord.IsChecked = TQMain.LoginConfig.IsSaveLoginRecord;
            dgRecentUsers.ItemsSource = recentUsers;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            int originCount = TQMain.LoginConfig.RecentInvestors.Count;
            int itemIndex = -1;
            var recentUserLst = recentUsers.ToList();
            for (int i = 0; i < originCount; i++)
            {
                itemIndex = recentUserLst.FindIndex(x => x.ID == TQMain.LoginConfig.RecentInvestors[i].ID);
                if (itemIndex < 0)
                {
                    TQMain.LoginConfig.RecentInvestors.RemoveAt(i);
                    originCount--;
                }
            }
            TQMain.LoginConfig.IsSaveLoginRecord = chkAutoSaveLoginRecord.IsChecked.Value;
            if (TQMain.LoginConfig.IsSaveLoginRecord == false)
            {
                TQMain.LoginConfig.RecentInvestors.Clear();
            }
        }
    }
}
