using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace CryptoTrading
{
    /// <summary>
    /// Interaction logic for UserPwdUpdateWindow.xaml
    /// </summary>
    public partial class UserPwdUpdateWindow : System.Windows.Window
    {
        private string brokerID = string.Empty;
        string userID = string.Empty;
        public UserPwdUpdateWindow(TQMain main)
        {
            InitializeComponent();
            
            Owner = main;
        }

        private void UpdateBtn_Click(object sender, RoutedEventArgs e)
        {
            string oldPwd = oldPwdPb.Password;
            string newPwd = newPwdPb.Password;
            string confirmPwd = confirmPwdPb.Password;
            if (string.IsNullOrEmpty(oldPwd) || string.IsNullOrEmpty(newPwd) || string.IsNullOrEmpty(confirmPwd))
            {
                MessageBox.Show("输入的旧密码或新密码不能为空，请重新输入。", "修改密码", MessageBoxButton.OK, MessageBoxImage.Error);
                oldPwdPb.Focus();
                return;
            }
            if (oldPwd==newPwd)
            {
                MessageBox.Show("输入的新密码和旧密码不能一致，请重新输入。", "修改密码", MessageBoxButton.OK, MessageBoxImage.Error);
                newPwdPb.Focus();
                return;
            }
            if (confirmPwd != newPwd)
            {
                MessageBox.Show("两次输入的新密码不一致，请重新输入。", "修改密码", MessageBoxButton.OK, MessageBoxImage.Error);
                newPwdPb.Focus();
                return;
            }
            if (pwdTypeCB.SelectedIndex == 0)
            {
                //调用修改用户交易密码的请求。
                TQMain.T.ReqUserPasswordUpdate(brokerID, userID, oldPwd, newPwd);
            }
            else
            {
                TQMain.T.ReqTradingAccountPasswordUpdate(brokerID, userID, oldPwd, newPwd);
            }
            Thread.Sleep(2000);
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            TQMain.T.OnRspTradingAccountPasswordUpdate -= _trade_OnRspTradingAccountPasswordUpdate;
             TQMain.T.OnRspUserPwdUpdate -= trade_OnRspUserPasswordUpdate;
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TQMain.T.userPwdUpdateWin = (UserPwdUpdateWindow)this;
            TQMain.T.OnRspTradingAccountPasswordUpdate += _trade_OnRspTradingAccountPasswordUpdate;
            TQMain.T.OnRspUserPwdUpdate += trade_OnRspUserPasswordUpdate;
            userID = TQMain.T.Investor;
            brokerID = TQMain.T.Broker;

            pwdTypeCB.SelectedIndex = 0;
        }

        public void trade_OnRspUserPasswordUpdate(object sender, CryptoTrading.PasswordUpdateEventArgs e)
        {
            if (e.ErrorID == 0)
            {
                Task.Run(() =>
                    {
                        var mr= MessageBox.Show("修改期货交易密码成功", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                        if (mr == MessageBoxResult.OK)
                        {
                            ShowMsg("修改期货交易密码成功");
                            this.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                this.Close();
                            }));
                        } 
                    });
            }
            else
            {
                Task.Run(() =>
                {
                    var mr= MessageBox.Show(string.Format("修改期货交易密码失败:{0}", e.ErrorMsg), "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    if (mr == MessageBoxResult.OK)
                    {
                        ShowMsg(string.Format("修改期货交易密码失败:{0}", e.ErrorMsg));
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            this.oldPwdPb.Password = string.Empty;
                            this.newPwdPb.Password = string.Empty;
                            this.confirmPwdPb.Password = string.Empty;
                            this.oldPwdPb.Focus();
                        }));
                    }
                });
            }
        }

        private void _trade_OnRspTradingAccountPasswordUpdate(object sender, CryptoTrading.TradingAccountPwdUpdateEventArgs e)
        {
            if (e.ErrorID == 0)
            {
                Task.Run(() =>
                    {
                        var mr = MessageBox.Show("修改期货资金密码成功", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                        if (mr == MessageBoxResult.OK)
                        {
                            ShowMsg("修改期货资金密码成功");
                            this.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                this.Close();
                            }));
                        }
                    });
            }
            else
            {
                Task.Run(() =>
                {
                    var mr = MessageBox.Show(string.Format("修改期货资金密码失败:{0}", e.ErrorMsg), "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    if (mr == MessageBoxResult.OK)
                    {
                        ShowMsg(string.Format("修改期货资金密码失败:{0}", e.ErrorMsg));
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            this.oldPwdPb.Password = string.Empty;
                            this.newPwdPb.Password = string.Empty;
                            this.confirmPwdPb.Password = string.Empty;
                            this.oldPwdPb.Focus();
                        }));
                    }
                });
            }
        }
        void ShowMsg(string pMsg)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                ((TQMain)Owner).ShowMsg(pMsg);

            }));
        }
    }
}
