using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;


namespace CryptoTrading
{
    enum BankFuturesOperateType
    {
        BankToFutures,
        FuturesToBank,
        QryFetchAmout
    }
    /// <summary>
    /// Interaction logic for BankFuturesTransferWindow.xaml
    /// </summary>
    public partial class BankFuturesTransferWindow : System.Windows.Window
    {
        #region private fieldes
        private string bankID = string.Empty;
        private string bankAccount = string.Empty;
        private string fundAccount = string.Empty;//资金账号
        private string bankPwd = string.Empty;

        private string brokerID = string.Empty;
        private string accountID = string.Empty;
        private string password = string.Empty;
        private string currencyID = string.Empty;
        private double tradeAmount = 0;
        private TradingAccountData tradingAccountData;
        //      pri
        #endregion

        #region public fieldes
        /// <summary>
        /// 转账流水视图
        /// </summary>
        public readonly ObservableCollection<TransferSerial> transferSerialView = new ObservableCollection<TransferSerial>();
        /// <summary>
        /// 银期账户视图
        /// </summary>
        public readonly ObservableCollection<NotifyQueryAccount> notifyQueryAccountView = new ObservableCollection<NotifyQueryAccount>();

        /// <summary>
        /// 签约银行视图
        /// </summary>
        public readonly ObservableCollection<Accountregister> accountregisterView = new ObservableCollection<Accountregister>();

        /// <summary>
        /// 签约银行字典(Key：期商代码+银行代码)
        /// </summary>
        public static readonly ConcurrentDictionary<string, ContractBankField> dicContractBankData = new ConcurrentDictionary<string, ContractBankField>();
        /// <summary>
        /// 客户开销户信息字典(Key:交易账号+银行账号)
        /// </summary>
        public static readonly ConcurrentDictionary<string, Accountregister> dicAccountregisterData = new ConcurrentDictionary<string, Accountregister>();
        /// <summary>
        /// 转账流水字典
        /// </summary>
        public static readonly ConcurrentDictionary<string, TransferSerial> dicTransferSerialData = new ConcurrentDictionary<string, TransferSerial>();
        /// <summary>
        /// 银期账户字典（Key:银行账号）
        /// </summary>
        public static readonly ConcurrentDictionary<string, NotifyQueryAccount> dicNotifyQueryAccountData = new ConcurrentDictionary<string, NotifyQueryAccount>();

        public static bool IsQryContractBankSuccess = false;
        #endregion

        #region private function


        /// <summary>
        /// 检查窗体的输入
        /// </summary>
        /// <param name="bankPwd"></param>
        /// <param name="password"></param>
        /// <param name="isQryFetchAmout"></param>
        /// <param name="tradeAmount"></param>
        private bool CheckInput(string bankPwd, string password, BankFuturesOperateType operateType, double tradeAmount, string bankName)
        {
            if (String.IsNullOrEmpty(password))
            {
                MessageBox.Show("请输入资金密码", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            //switch (operateType)
            //{
            //    case BankFuturesOperateType.BankToFutures:
            //    case BankFuturesOperateType.QryFetchAmout:
            //        {
            //            if (String.IsNullOrEmpty(bankPwd))
            //            {
            //                MessageBox.Show("请输入银行密码", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            //                return false;
            //            }
            //        }
            //        break;
            //    case BankFuturesOperateType.FuturesToBank:
            //        {
            //            if (bankName == "建设银行" || bankName == "工商银行" || bankName == "交通银行")
            //            {
            //                BankPwdTxt.Clear();
            //            }
            //            else
            //            {
            //                if (String.IsNullOrEmpty(bankPwd))
            //                {
            //                    MessageBox.Show("请输入银行密码", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            //                    return false;
            //                }
            //            }
            //        }
            //        break;
            //}

            if (operateType != BankFuturesOperateType.QryFetchAmout)
            {
                if (tradeAmount <= 0)
                {
                    MessageBox.Show("请输入正确的转账金额", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false;
                }
            }
            return true;
        }

        #endregion
        public BankFuturesTransferWindow(TQMain main)
        {
            InitializeComponent();

            Owner = main;

            //init the private fieldes
            brokerID = Trader.DefaultBrokerID;
            accountID = Trader.Configuration.Investor.ID;

            TQMain.T.transferWin = (BankFuturesTransferWindow)this;

            var accountList = main.TradingAccountDataView.Where(t => t.InvestorID == accountID && t.ExchangeID == Trader.DefaultBroker).ToList();
            if (accountList != null && accountList.Count > 0)
            {
                tradingAccountData = accountList[0];
                FuturesAvaiableAssertTxt.Text = tradingAccountData.WithdrawQuota.ToString();
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 从Configuration.xml中加载各种DataGrid列
            if (!TQMain.dicMainDataGridMapping.ContainsKey(DataGridType.Accountregister))
            {
                TQMain.dicMainDataGridMapping.Add(DataGridType.Accountregister, new TQClass.DataGridMapping()
                {
                    TQMainDataGrid = AccountDataGrid,
                    DataGridType = DataGridType.Accountregister,
                    ColumnSettingList = Trader.Configuration.AccountregisterDataGrid
                });
            }
            if (!TQMain.dicMainDataGridMapping.ContainsKey(DataGridType.TransferSerial))
            {
                TQMain.dicMainDataGridMapping.Add(DataGridType.TransferSerial, new TQClass.DataGridMapping()
                {
                    TQMainDataGrid = TransferSerialDataGrid,
                    DataGridType = DataGridType.TransferSerial,
                    ColumnSettingList = Trader.Configuration.TransferSerialDataGrid
                });
            }
            Utility.LoadConfiguration(AccountDataGrid, DataGridType.Accountregister);
            Utility.LoadConfiguration(TransferSerialDataGrid, DataGridType.TransferSerial);

            if (Trader.IsSimAccount)    //如果是仿真交易，除了显示期货可取资金，其余功能禁用。
            {
                QueryFetchAmountBtn.IsEnabled = TransferSerialBtn.IsEnabled
                    = BankToFuturesBtn.IsEnabled = FuturesToBankBtn.IsEnabled = false;
            }
            else
            {
                Task task = Task.Run(() =>
                {
                    QryAccountregisterField qryAccountregister = new QryAccountregisterField()
                    {
                        AccountID = Trader.Configuration.Investor.ID,
                        BrokerID = Trader.DefaultBrokerID,
                        CurrencyID = "CNY"
                    };
                    int timeout = 0;
                    while (true)
                    {
                        if (BankFuturesTransferWindow.IsQryContractBankSuccess)
                        {
                            TQMain.T.ReqQryAccountregister(qryAccountregister);
                            Thread.Sleep(1000);
                            break;
                        }
                        Thread.Sleep(1000);
                        timeout++;
                        if (timeout > 5)
                        {
                            break;
                        }
                    }
                    if (timeout > 5)
                    {
                        MessageBox.Show("查询签约银行响应超时");
                        return;
                    }
                    timeout = 0;
                    while (true)
                    {
                        if (dicAccountregisterData.Count > 0)
                        {
                            break;
                        }
                        Thread.Sleep(1000);
                        timeout++;
                        if (timeout > 5)
                        {
                            break;
                        }
                    }

                    if (timeout > 5)
                    {
                        MessageBox.Show("查询银期签约关系响应超时");
                        return;
                    }
                });

            }
            
            AccountDataGrid.ItemsSource = accountregisterView;
            TransferSerialDataGrid.ItemsSource = transferSerialView;

            AccountDataGrid.SelectedIndex = 0;

        }

        private void AccountDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ErrorInfoTxtBlock.Text = string.Empty;
            Accountregister account = AccountDataGrid.SelectedItem as Accountregister;
            if (account != null)
            {
                string key = account.BankAccount;
                NotifyQueryAccount notifyQryAccount;
                if (dicNotifyQueryAccountData.TryGetValue(key, out notifyQryAccount))
                {
                    BankAvaiableAssertTxt.Text = notifyQryAccount.BankFetchAmount.ToString();
                }
            }
        }

        private void QueryFetchAmountBtn_Click(object sender, RoutedEventArgs e)
        {
            ErrorInfoTxtBlock.Text = string.Empty;
            try
            {
                bankPwd = BankPwdTxt.Password;
                password = AssertPwdTxt.Password;
                if (!CheckInput(bankPwd, password, BankFuturesOperateType.QryFetchAmout, 0, string.Empty))
                {
                    BankPwdTxt.Clear();
                    AssertPwdTxt.Clear();
                    return;
                }

                Accountregister accountregister;
                if (dicAccountregisterData.Count == 1)
                {
                    accountregister = dicAccountregisterData.Values.ToList()[0];
                }
                else
                {
                    accountregister = AccountDataGrid.SelectedItem as Accountregister;
                }
                if (accountregister != null)
                {
                    ReqQueryAccountField reqQryAccount = new ReqQueryAccountField()
                    {
                        BankID = accountregister.BankID,
                        BankAccount = accountregister.BankAccount,
                        BankPassWord = bankPwd,
                        Password = password,
                        UserID = accountID,
                        BrokerID = brokerID,
                        CurrencyID = Utility.GetCurrencyID(accountregister.CurrencyName),
                        AccountID = accountID
                    };
                    QueryFetchAmountBtn.IsEnabled = false;
                    TQMain.T.ReqQueryBankAccountMoneyByFuture(reqQryAccount);
                    Thread.Sleep(1000);
                }
                else
                {
                    MessageBox.Show("请选择查询的银行账号", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            { Utility.WriteMemFile(ex.ToString()); }
            finally
            {
                BankPwdTxt.Clear();
                AssertPwdTxt.Clear();
            }
        }

        private void TransferSerialBtn_Click(object sender, RoutedEventArgs e)
        {
            ErrorInfoTxtBlock.Text = string.Empty;
            Accountregister accountregister;
            if (dicAccountregisterData.Count == 1)
            {
                accountregister = dicAccountregisterData.Values.ToList()[0];
            }
            else
            {
                accountregister = AccountDataGrid.SelectedItem as Accountregister;
            }
            if (accountregister != null)
            {
                bankID = accountregister.BankID;
                currencyID = Utility.GetCurrencyID(accountregister.CurrencyName);
                TransferSerialBtn.IsEnabled = false;
                TQMain.T.ReqQryTransferSerial(brokerID, accountID, bankID, currencyID);
                Thread.Sleep(1000);
            }
        }

        private void BankToFuturesBtn_Click(object sender, RoutedEventArgs e)
        {
            ErrorInfoTxtBlock.Text = string.Empty;
            try
            {
                bankPwd = BankPwdTxt.Password;
                password = AssertPwdTxt.Password;
                double.TryParse(TransferAmountTxt.Text, out tradeAmount);
                if (!CheckInput(bankPwd, password, BankFuturesOperateType.BankToFutures, tradeAmount, string.Empty))
                {
                    BankPwdTxt.Clear();
                    AssertPwdTxt.Clear();
                    TransferAmountTxt.Clear();
                    return;
                }
                Accountregister accountregister;
                if (dicAccountregisterData.Count == 1)
                {
                    accountregister = dicAccountregisterData.Values.ToList()[0];
                }
                else
                {
                    accountregister = AccountDataGrid.SelectedItem as Accountregister;
                }
                if (accountregister != null)
                {
                    currencyID = Utility.GetCurrencyID(accountregister.CurrencyName);
                    bankID = accountregister.BankID;
                    bankAccount = accountregister.BankAccount;
                    BankToFuturesBtn.IsEnabled = false;
                    TQMain.T.ReqFromBankToFutureByFuture(bankID, brokerID, string.Empty, accountID, password, currencyID, tradeAmount);
                    Thread.Sleep(1000);
                }
                else
                {
                    MessageBox.Show("请选择转账的银行账号", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                Utility.WriteMemFile(ex.ToString());
            }
            finally
            {
                BankPwdTxt.Clear();
                AssertPwdTxt.Clear();
                TransferAmountTxt.Clear();
            }
        }

        private void FuturesToBankBtn_Click(object sender, RoutedEventArgs e)
        {
            ErrorInfoTxtBlock.Text = string.Empty;
            try
            {
                bankPwd = BankPwdTxt.Password;
                password = AssertPwdTxt.Password;
                double.TryParse(TransferAmountTxt.Text, out tradeAmount);

                Accountregister accountregister;
                if (dicAccountregisterData.Count == 1)
                {
                    accountregister = dicAccountregisterData.Values.ToList()[0];
                }
                else
                {
                    accountregister = AccountDataGrid.SelectedItem as Accountregister;
                }
                if (accountregister != null)
                {
                    if (!CheckInput(bankPwd, password, BankFuturesOperateType.FuturesToBank, tradeAmount, accountregister.BankName))
                    {
                        BankPwdTxt.Clear();
                        AssertPwdTxt.Clear();
                        TransferAmountTxt.Clear();
                        return;
                    }

                    bankID = accountregister.BankID;
                    bankAccount = accountregister.BankAccount;
                    currencyID = Utility.GetCurrencyID(accountregister.CurrencyName);
                    FuturesToBankBtn.IsEnabled = false;
                    TQMain.T.ReqFromFutureToBankByFuture(bankID, bankAccount, brokerID, accountID, password, currencyID, tradeAmount);
                    Thread.Sleep(1000);
                }
                else
                {
                    MessageBox.Show("请选择转账的银行账号", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                Utility.WriteMemFile(ex.ToString());
            }
            finally
            {

                BankPwdTxt.Clear();
                AssertPwdTxt.Clear();
                TransferAmountTxt.Clear();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {

        }
    }
}
