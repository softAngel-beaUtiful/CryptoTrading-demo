using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Threading;
using System.Xml;
using System.IO;
using CryptoTrading;
using CryptoTrading.TQLib;
using System.Linq;
using System.ComponentModel;
using Newtonsoft.Json;
using CryptoTrading.Model;

namespace CryptoTrading
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow
    {       
        public string Password { get; set; }
        public string CurrentUserID { get; set; }

        private TQMain main;
        /// <summary>
        /// 用户字典(Key：用户名称)
        /// </summary>
        public Dictionary<string, List<Exchange>> UserDict;       
        List<Exchange> ExchangeList;
        Users thisusers;
        const string path = "configurations\\usersconfiguration.xml";
        public LoginWindow(TQMain mainwin)
        {
            InitializeComponent();
            main = mainwin ?? throw new ArgumentNullException("mainwin is null");

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            saveConfigChkBox.IsChecked = true;
            try
            {
                //获取排序后的用户列表
                UserDict = GetUserConfig();
                var userkeys = UserDict.Keys;
                UserListCB.ItemsSource = userkeys;
                UserListCB.Text = userkeys.ToList()[0];               
            }
            catch (Exception ex)
            {
                SimpleLogger.Logger.Log(ex.Message, SimpleLogger.LogCategory.Error);
            }                       
        }
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            if (saveConfigChkBox.IsChecked != null && saveConfigChkBox.IsChecked.Value)
            {
                SaveDefaultConfiguration();
            }
            DialogResult = false;
        }

        private void ExitLoginWindow(object sender, EventArgs e)
        {
            if (saveConfigChkBox.IsChecked != null && saveConfigChkBox.IsChecked.Value)
            {
                SaveDefaultConfiguration();
            }
        }
        private void loginbutton_Click(object sender, RoutedEventArgs e)
        {
            loginbutton.IsEnabled = false;
            //Password = PwBox.Password;
            ShowStatus("开始登录", 0);
            try

            {
                if (UserListCB.SelectedItem == null)
                {
                    SetDialogresult(false);
                    return;
                }
                //Trader.DefaultBroker = UserListCB.SelectedItem.ToString();
                //Trader.DefaultServer = ExchangeListCB.SelectedItem.ToString();
                /*List<ExchangeAccountInfo> brokerConfig;
                if (UserDict.TryGetValue(Trader.DefaultBroker, out brokerConfig))
                {
                    Trader.IsSimAccount = brokerConfig.FindAll((a)=>a.ExchangeID=="BitMex").Count>0 ? false : true;
                }
                Trader.ActionDay = DateTime.Now.ToString("yyyy-MM-dd");
                
                //为第一次使用软件的账户初始化文件目录结构
                if (txtPublicKey.Text.Trim() != Trader.Configuration.Investor.ID)
                {
                    Trader.Configuration.Investor.ID = txtPublicKey.Text.Trim();
                    InitUserDirectoryAndConfiguration(Trader.Configuration.Investor.ID);
                    Trader.Configuration.Investor.BrokerName = UserListCB.SelectedItem.ToString();
                }
                //
                //List<ExchangeAccountInfo> brokerconfigFile;
                //UserDict.TryGetValue(Trader.Configuration.Investor.BrokerName, out brokerconfigFile);

                //Trader.Configuration.Investor.BrokerConfig = brokerconfigFile;
                Trader.Configuration.Investor.BrokerServer = Trader.DefaultServer;
                //main.UpdateTraderServers(brokerconfigFile,Trader.DefaultServer);
                
                if (Trader.LoginMode == LoginMode.DebugOffline)
                {
                    SaveDefaultConfiguration();
                    SetDialogresult(true);
                }
                else
                    TQMain.T.OnRspUserLogin += trade_OnRspUserLogin;
                */
                
                InitUserDirectoryAndConfiguration(UserListCB.Text);
                SaveDefaultConfiguration();
                main.MyExchanges= ExchangeList;
                main.CurrentUserID = CurrentUserID;
                //main.InitTraderByConfig();
                SetDialogresult(true);
            }            
            catch (Exception ex)
            {
                SimpleLogger.Logger.Log(ex.Message, SimpleLogger.LogCategory.Error);
                SaveDefaultConfiguration();
                SetDialogresult(false);               
            }
        }
        
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    loginbutton_Click(this, new RoutedEventArgs());
                    break;
                case Key.Escape:
                    ExitButton_Click(this, new RoutedEventArgs());
                    break;
            }
        }
        private void UserListCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           
            CurrentUserID = UserListCB.SelectedItem.ToString();
            
            if (UserDict.TryGetValue(CurrentUserID, out ExchangeList))
            {
                var exchangelist = ExchangeList.Select(x => x.ExchangeID);
                ExchangeListCB.ItemsSource = null;
                ExchangeListCB.ItemsSource = exchangelist;
                ExchangeListCB.Text = exchangelist.ToList()[0];
            }                                                
        }
        private void ExchangeListCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ExchangeListCB.Items.Count == 0) return;
            var exchangeid = ExchangeListCB.SelectedItem.ToString();
            var info = UserDict[CurrentUserID].Find(x => x.ExchangeID == exchangeid);
            txtPublicKey.Text = info.PublicKey;
            txtPrivateKey.Text = info.PrivateKey;
            txtPassPhrase.Text = info.PassPhrase;
        }

        private void txtPublicKey_TextChanged(object sender, TextChangedEventArgs e)
        {
            
            //int itemIndex = TQMain.LoginConfig.RecentInvestors.FindIndex(x => x.ID == txtPublicKey.Text);
            //if (itemIndex >= 0)
            {
              //  UserListCB.SelectedItem = TQMain.LoginConfig.RecentInvestors[itemIndex].BrokerName;
              //  ExchangeListCB.SelectedItem = TQMain.LoginConfig.RecentInvestors[itemIndex].BrokerServer;
            }
        }   

        public void trade_OnRspUserLogin(object sender, ErrorEventArgs e)
        {
            //progressbar value =30
            //UpdateProgressBar(string.Format("登录交易服务器{0}", e.Value == 0 ? "成功" : "失败"));
            if (e == null) throw new ArgumentNullException("e");
            if (e.ErrorID == 0)
            {
                TQMain.LoginConfig.Investor.LastLoginTime = DateTime.Now.ToString("yyyyMMddHHmmss");
                ShowStatus("交易服务器登录", 20);

                ShowMsg("交易服务器登录");
                Thread.Sleep(50);
                //交易登录成功后,登录行情
               // if (TQMain.Q == null)
               //     LoginSuccess();
            }
            else
            {
                ShowStatus("交易服务器登录失败：" + e.ErrorMsg);
                /*Dispatcher.BeginInvoke(new Action(() =>
                    {
                        loginStatusTxtBlock.Text = e.ErrorMsg;
                    }));*/
                ShowMsg("交易服务器登录失败："+e.ErrorMsg);
                if (TQMain.T!=null && TQMain.T.IsLogin)
                    TQMain.T.ReqUserLogout();
                //MainWindow.T = null;
                //MainWindow.Q = null;
            }
        }
                          

        #region 窗体私有函数

        private void SetDialogresult(bool b)
        {
            DialogResult = b;
        }

        private void SaveDefaultConfiguration()
        {            
            TQXmlHelper.XmlSerializeToFile(thisusers, path,  System.Text.Encoding.UTF8);
        }

       

        private void ShowMsg(string pMsg)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                main.ShowMsg(pMsg);

            }));
        }
        public void ShowStatus(string msg, int process = 0)
        {
            /*if (this.lblStatus.CheckAccess())
            {
                //lblStatus.Content = msg;
                pbLogin.Value = process == 0 ? pbLogin.Value : process;
            }
            else
                Dispatcher.Invoke(new Action(() => { lblStatus.Content = msg; pbLogin.Value = process == 0 ? pbLogin.Value : process; }));
                */
        }

        /// <summary>
        /// 获取期商配置字典
        /// </summary>
        /// <param name="path">期商配置文件所在路径</param>
        /// <param name="serverDic">期商名称-服务器列表 形式的字典</param>
        /// <returns>期商名称-对应的配置文件名 形式的字典</returns>
        private Dictionary<string, List<Exchange>> GetUserConfig()
        {
            //Dictionary<string, string> result = new Dictionary<string, string>();
            var result = new Dictionary<string, List<Exchange>>();
            if (!File.Exists(path))
            {
                throw new Exception("no this file, please create this");
            }
            try {
                //DirectoryInfo folder = new DirectoryInfo(path);
                //if (!File.Exists(path + "UserConfiguration.xml")) 
                var futureconfigFiles = File.ReadAllText(path);
                //XmlDocument document = 
                thisusers = TQXmlHelper.XmlDeserializeFromFile<Users>(path, System.Text.Encoding.UTF8);
                foreach (var u in thisusers.User)
                    result.Add(u.UserID, u.Exchange);// usersapis.UserID[CurrentUserID];
                
            }
            catch (Exception ex)
            {
                Utility.WriteMemFile(ex.ToString()); 
            }
            return result;
        }
        
        /// <summary>
        /// 为第一次使用软件的账户初始化文件目录结构并且初始化其配置信息
        /// </summary>
        private void InitUserDirectoryAndConfiguration(string investorId)
        {
            //Trader.InitUserDirectoryAndConfiguration("Okex");
            if(Trader.Configuration==null || Trader.Configuration.Investor.ID != investorId)
            {
                Trader.Configuration = Trader.Load(investorId);   
                
                
            }
            Trader.Configuration.RequestID = 0;

        }
        #endregion
               
      
        private void saveConfigChkBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        
    }   
}
