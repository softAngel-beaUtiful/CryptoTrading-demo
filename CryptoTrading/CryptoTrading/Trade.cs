using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Threading;
using CryptoTrading.TQLib;
using CryptoTrading.Model;
using CryptoTrading.ViewModel;
using System.Text;
using CryptoTrading.TQClass;
using System.Windows;

namespace CryptoTrading
{
    #region 自定义事件参数类
    /// <summary>
	/// Int event args
	/// </summary>
	public class IntEventArgs : EventArgs
    {
        /// <summary>
        /// 错误代码
        /// </summary>
        public int Value = 0;
    }

    /// <summary>
    ///
    /// </summary>
    public class StringEventArgs : EventArgs
    {
        /// <summary>
        /// 错误代码
        /// </summary>
        public string Value = string.Empty;
    }

    /// <summary>
    ///
    /// </summary>
    public class ErrorEventArgs : EventArgs
    {
        /// <summary>
        /// 错误代码
        /// </summary>
        public int ErrorID = 0;
        /// <summary>
        /// 错误说明
        /// </summary>
        public string ErrorMsg = string.Empty;
    }

    /// <summary>
    ///
    /// </summary>
    public class StatusEventArgs : EventArgs
    {
        /// <summary>
        /// 交易所/品种/合约
        /// </summary>
        public string Exchange = string.Empty;
        /// <summary>
        /// 交易状态
        /// </summary>
        public InstrumentStatusType Status = InstrumentStatusType.开盘前;
    }

    /// <summary>
    /// 报单状态改变响应
    /// </summary>
    public class OrderArgs : EventArgs
    {
        /// <summary>
        /// 报单
        /// </summary>
        public OrderData Value;
    }
    /// <summary>
    /// 报单成交响应
    /// </summary>
    public class TradeArgs : EventArgs
    {
        /// <summary>
        /// 报单
        /// </summary>
        public TradeData Value;
    }

    /// <summary>
    ///
    /// </summary>
    public class PasswordUpdateEventArgs : EventArgs
    {
        /// <summary>
        /// 错误代码
        /// </summary>
        public int ErrorID = 0;
        /// <summary>
        /// 错误说明
        /// </summary>
        public string ErrorMsg = string.Empty;
    }

    public class TradingAccountPwdUpdateEventArgs : EventArgs
    {
        /// <summary>
        /// 错误代码
        /// </summary>
        public int ErrorID = 0;
        /// <summary>
        /// 错误说明
        /// </summary>
        public string ErrorMsg = string.Empty;
    }
    #endregion
  
    public class Trade : TradeInterface
    {
        public TQMain main;
        public SettlementWindow settlementWin;        
        public UserPwdUpdateWindow userPwdUpdateWin;      
        public NoticeWindow noticeWin;
        public AccountInfoWindow accountInfoWin;
        /// <summary>
        /// 用于发出/重置登录过程中的信号
        /// </summary>
        public ManualResetEventSlim loginEvent = new ManualResetEventSlim(false);
        /// <summary>
        /// Key: SessionID+OrderRef
        /// </summary>
        public TQConcurrentDictionary<string, InputOrderField> dicOrderRef = new TQConcurrentDictionary<string, InputOrderField>();
        [DllImport("Kernel32.dll")]
        public static extern bool SetLocalTime(ref SystemTime sysTime);

        /// <summary>
        /// 手续费字典(Key：ProductID)
        /// </summary>
        public ConcurrentDictionary<string, InstrumentCommissionRate> dicCommissionRate = new ConcurrentDictionary<string, InstrumentCommissionRate>();

        /// <summary>
        /// 保证金率字典（Key:InstrumentID+HedgeFlag）
        /// </summary>
        public ConcurrentDictionary<string, InstrumentMarginRate> dicMarginRate = new ConcurrentDictionary<string, InstrumentMarginRate>();
        /// <summary>
        /// 某一合约、方向上平仓未成交单的数量（Key:InstrumentID+Direction）
        /// </summary>
        public ConcurrentDictionary<string, UnfilledOrder> dicUnfilledCloseOrder = new ConcurrentDictionary<string, UnfilledOrder>();
        ///// <summary>
        ///// 原始订单列表
        ///// </summary>
        //public static List<OrderFieldExt> ExtOrdersList = new List<OrderFieldExt>();

        private Proxy _proxy;
        /// <summary>
        /// 是否完成登录的初始化查询
        /// </summary>
        private bool isStarted;
        /// <summary>
        /// 是否确认结算单
        /// </summary>
        private bool isConfirmed;
        public int RequestID = 0;
        private InstrumentInfoConfiguration instrumentCfg;

        public Trade(string tradeDll)
        {
            //TA = new CTPTraderAdapter("./log/Trader/");
            _proxy = new Proxy(tradeDll);
           

            _proxy.OnRspUserPasswordUpdate += _Trade_OnRspUserPasswordUpdate;


            //银期转账
          
            //尝试加载本地保存的合约信息
            //登录过程中，如果存在本地合约信息，则直接加载
            if (File.Exists(Trader.InstrumentInfoCfgFile))
            {
                instrumentCfg = TQXmlHelper.XmlDeserializeFromFile<InstrumentInfoConfiguration>(Trader.InstrumentInfoCfgFile, Encoding.UTF8);
                foreach (var v in instrumentCfg.ListInstrument)
                    TQMainModel.dicInstrumentData.Add(v.InstrumentID, v);
                foreach (var v in instrumentCfg.MarginRates)
                    dicMarginRate.AddOrUpdate(v.InstrumentID, v, (x, y) => y);
                foreach (var v in instrumentCfg.CommissionRates)
                    dicCommissionRate.AddOrUpdate(v.InstrumentID, v, (x, y) => y);
            }
            else
            {
                Utility.WriteMemFile("there is no InstrumentInfo.xml");
                return;
            }
        }

      
        /// <summary>
        /// 交易所状态
        /// </summary>
        public ConcurrentDictionary<string, ThostFtdcInstrumentStatusField> DicExchangeStatus = new ConcurrentDictionary<string, ThostFtdcInstrumentStatusField>();

        /// <summary>
        /// 交易所时间
        /// </summary>
        //protected ConcurrentDictionary<string, TimeSpan> DicExcLoginTime = new ConcurrentDictionary<string, TimeSpan>();

        public List<string> OrderRefList = new List<string>();
        public List<string> OrderLocalIDList = new List<string>();
        public List<string> OrderSysIDList = new List<string>();
        public List<string> OrderKeyList = new List<string>();

        public ConcurrentDictionary<string, string> DicSettlementContent = new ConcurrentDictionary<string, string>();
        #region 注册响应事件
        //前置连接
        public delegate void FrontConnected(object sender, EventArgs e);

        private FrontConnected _OnFrontConnected;

        public event FrontConnected OnFrontConnected
        {
            add
            {
                _OnFrontConnected += value;
            }
            remove
            {
                _OnFrontConnected -= value;
            }
        }

        //用户登录请求
        public delegate void RspUserLogin(object sender, ErrorEventArgs e);

        private RspUserLogin _OnRspUserLogin;

        public event RspUserLogin OnRspUserLogin
        {
            add
            {
                _OnRspUserLogin += value;
            }
            remove
            {
                _OnRspUserLogin -= value;
            }
        }

        //用户修改密码请求
        public delegate void RspUserPwdUpdate(object sender, PasswordUpdateEventArgs e);

        private RspUserPwdUpdate _OnRspUserPwdUpdate;

        public event RspUserPwdUpdate OnRspUserPwdUpdate
        {
            add
            {
                _OnRspUserPwdUpdate += value;
            }
            remove
            {
                _OnRspUserPwdUpdate -= value;
            }
        }


        //确认结算单信息
        public delegate void RspSettlementInfoConfirm(Object sender, EventArgs e);//SettlementInfoConfirmField pSettlementInfoConfirm, RspInfoField pRspInfo,
                                                                                  //int nRequestID, bool bIsLast);

        private RspSettlementInfoConfirm _OnRspSettlementInfoConfirm;

        public event RspSettlementInfoConfirm OnRspSettlementInfoConfirm
        {
            add
            {
                _OnRspSettlementInfoConfirm += value;
            }
            remove
            {
                _OnRspSettlementInfoConfirm -= value;
            }
        }

        //用户退出请求
        public delegate void RspUserLogout(object sender, IntEventArgs e);

        private RspUserLogout _OnRspUserLogout;

        public event RspUserLogout OnRspUserLogout
        {
            add
            {
                _OnRspUserLogout += value;
            }
            remove
            {
                _OnRspUserLogout -= value;
            }
        }

        //错误回报
        public delegate void RtnError(object sender, ErrorEventArgs e);

        private RtnError _OnRtnError;

        public event RtnError OnRtnError
        {
            add
            {
                _OnRtnError += value;
            }
            remove
            {
                _OnRtnError -= value;
            }
        }

        //消息回报
        public delegate void RtnNotice(object sender, StringEventArgs e);

        private RtnNotice _OnRtnNotice;

        public event RtnNotice OnRtnNotice
        {
            add
            {
                _OnRtnNotice += value;
            }
            remove
            {
                _OnRtnNotice -= value;
            }
        }

        //交易所状态回报
        //public delegate void RtnInstrumentStatus(object sender, StatusEventArgs e);

        //private RtnInstrumentStatus _OnRtnInstrumentStatus;

        //public event RtnInstrumentStatus OnRtnInstrumentStatus
        //{
        //	add
        //	{
        //		_OnRtnInstrumentStatus += value;
        //	}
        //	remove
        //	{
        //		_OnRtnInstrumentStatus -= value;
        //	}
        //}

        //委托回报
        public delegate void RtnOrder(object sender, OrderArgs e);

        private RtnOrder _OnRtnOrder;

        public event RtnOrder OnRtnOrder
        {
            add
            {
                _OnRtnOrder += value;
            }
            remove
            {
                _OnRtnOrder -= value;
            }
        }

        //撤单回报
        private RtnOrder _OnRtnCancel;

        public event RtnOrder OnRtnCancel
        {
            add
            {
                _OnRtnCancel += value;
            }
            remove
            {
                _OnRtnCancel -= value;
            }
        }

        //成交回报
        public delegate void RtnTrade(object sender, TradeArgs e);

        private RtnTrade _OnRtnTrade;

        public event RtnTrade OnRtnTrade
        {
            add
            {
                _OnRtnTrade += value;
            }
            remove
            {
                _OnRtnTrade -= value;
            }
        }

        /// <summary>
        /// 查询结算单响应
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void RspQrySettlementInfo(object sender, EventArgs e);

        private RspQrySettlementInfo _OnRspQrySettlementInfo;

        public event RspQrySettlementInfo OnRspQrySettlementInfo
        {
            add
            {
                _OnRspQrySettlementInfo += value;
            }
            remove
            {
                _OnRspQrySettlementInfo -= value;
            }
        }

        //修改资金密码通知
        public delegate void RspTradingAccountPasswordUpdate(object sender, TradingAccountPwdUpdateEventArgs e);

        private RspTradingAccountPasswordUpdate _OnRspTradingAccountPasswordUpdate;

        public event RspTradingAccountPasswordUpdate OnRspTradingAccountPasswordUpdate
        {
            add
            {
                _OnRspTradingAccountPasswordUpdate += value;
            }
            remove
            {
                _OnRspTradingAccountPasswordUpdate -= value;
            }
        }

        /// <summary>
        /// 查询结算单响应
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void RtnQueryBankBalanceByFuture(object sender, EventArgs e);

        private RtnQueryBankBalanceByFuture _OnRtnQueryBankBalanceByFuture;

        public event RtnQueryBankBalanceByFuture OnRtnQueryBankBalanceByFuture
        {
            add
            {
                _OnRtnQueryBankBalanceByFuture += value;
            }
            remove
            {
                _OnRtnQueryBankBalanceByFuture -= value;
            }
        }

        /// <summary>
        /// 查询合约响应
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void RspQryInstrument(object sender, EventArgs e);

        private RspQryInstrument _OnRspQryInstrument;

        public event RspQryInstrument OnRspQryInstrument
        {
            add
            {
                _OnRspQryInstrument += value;
            }
            remove
            {
                _OnRspQryInstrument -= value;
            }
        }

        /// <summary>
        /// 查询交易账户响应
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void RspQryTradingAccount(object sender, EventArgs e);

        private RspQryTradingAccount _OnRspQryTradingAccount;

        public event RspQryTradingAccount OnRspQryTradingAccount
        {
            add
            {
                _OnRspQryTradingAccount += value;
            }
            remove
            {
                _OnRspQryTradingAccount -= value;
            }
        }
        /// <summary>
        /// 查询投资者持仓明细响应
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void RspQryInvestorPositionDetail(object sender, EventArgs e);

        private RspQryInvestorPosition _OnRspQryInvestorPositionDetail;

        public event RspQryInvestorPosition OnRspQryInvestorPositionDetail
        {
            add
            {
                _OnRspQryInvestorPositionDetail += value;
            }
            remove
            {
                _OnRspQryInvestorPositionDetail -= value;
            }
        }

        /// <summary>
        /// 查询投资者持仓响应
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void RspQryInvestorPosition(object sender, EventArgs e);

        private RspQryInvestorPosition _OnRspQryInvestorPosition;

        public event RspQryInvestorPosition OnRspQryInvestorPosition
        {
            add
            {
                _OnRspQryInvestorPosition += value;
            }
            remove
            {
                _OnRspQryInvestorPosition -= value;
            }
        }


        /// <summary>
        /// 查询投资者报单响应
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void RspQryOrder(object sender, EventArgs e);

        private RspQryOrder _OnRspQryOrder;

        public event RspQryOrder OnRspQryOrder
        {
            add
            {
                _OnRspQryOrder += value;
            }
            remove
            {
                _OnRspQryOrder -= value;
            }
        }

        /// <summary>
        /// 查询投资者报单成交响应
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void RspQryTrade(object sender, EventArgs e);

        private RspQryTrade _OnRspQryTrade;

        public event RspQryTrade OnRspQryTrade
        {
            add
            {
                _OnRspQryTrade += value;
            }
            remove
            {
                _OnRspQryTrade -= value;
            }
        }

        #endregion

        #region 属性

        /// <summary>
        /// 服务器名称
        /// </summary>
        public string Server { get; set; }

        /// <summary>
        /// 经纪公司代码
        /// </summary>
        public string Broker { get; set; }

        /// <summary>
        /// 帐号
        /// </summary>
        public string Investor { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 交易日
        /// </summary>
        //public string TradingDay { get; protected set; }

        /// <summary>
        /// 登录成功
        /// </summary>
        public bool IsLogin { get; protected set; }
        #endregion

        #region 响应/回报函数
                                   
        private void _Trade_OnRspUserPasswordUpdate(UserPasswordUpdateField pUserPasswordUpdate, RspInfoField pRspInfo, int nRequestId, bool bIsLast)
        {
            //将反馈的信息显示到用户密码修改界面。

            if (_OnRspUserPwdUpdate != null)
            {
                _OnRspUserPwdUpdate(this, new PasswordUpdateEventArgs
                {
                    ErrorID = pRspInfo.ErrorID,
                    ErrorMsg = pRspInfo.ErrorMsg,
                });
            }
        }
        private List<string> _contents = new List<string>();
        private ConcurrentDictionary<string, List<string>> dicSettlement = new ConcurrentDictionary<string, List<string>>();
        //private List<string> settlements = new List<string>();
        private string tradingDay = string.Empty;
               
        #endregion

        #region 封装的代理请求
        public int ReqConnect()
        {
            if (isStarted)
            {
                isStarted = false;
            }
            string flowPath = Trader.Configuration.Investor.ID + "/Trade/";
            List<string> servers = new List<string>();
            Trader.TradingServer.ForEach(item =>
            {
                if (item.Contains("//"))
                {
                    servers.Add(item);
                }
                else
                {
                    servers.Add("tcp://" + item);
                }
            });
            return _proxy.ReqConnect(flowPath, servers.ToArray(), servers.Count);
        }

        public int ReqUserLogin()
        {
            return _proxy.ReqUserLogin(Investor, Password, Broker);
        }

        public void ReqUserLogout()
        {
           
        }

        /// <summary>
        /// 请求更新用户口令
        /// </summary>
        /// <param name="brokerID">期商代码</param>
        /// <param name="userID">用户代码</param>
        /// <param name="oldPwd">旧密码</param>
        /// <param name="newPwd">新密码</param>
        /// <returns></returns>
        public int ReqUserPasswordUpdate(string brokerID, string userID, string oldPwd, string newPwd)
        {
            UserPasswordUpdateField pUserPasswordUpdate = new UserPasswordUpdateField()
            {
                BrokerID = brokerID,
                NewPassword = newPwd,
                OldPassword = oldPwd,
                UserID = userID
            };
            return _proxy.ReqUserPasswordUpdate(pUserPasswordUpdate);
        }

        public int ReqSettlementInfoConfirm()
        {
            return _proxy.ReqSettlementInfoConfirm();
        }


        /// <summary>
        /// 撤单录入请求
        /// </summary>
        /// <param name="exchangeid">交易所代码</param>
        /// <param name="ordersysid">报单编号</param>
        /// <returns></returns>
        public int ReqOrderAction(string exchangeid, string ordersysid)
        {
            string subject = string.Format("资金账号({0})的撤单通知", Investor);
            string mailContent = string.Format("对交易所代码/ExchangeID:{0},报单编号/OrderSysID:{1}的委托单撤单：\r\n{2}\r\n", exchangeid, ordersysid, DateTime.Now.ToString("yyyyMMdd hh:mm:ss"));
            Task.Run(new Action(() => { Utility.SendNotice(subject, mailContent); })); ;

            InputOrderActionField inputOrderAction = new InputOrderActionField()
            {
                ActionFlag = (char)ActionFlagType.Delete,
                BrokerID = Broker,
                InvestorID = Investor,
                UserID = Investor,
                ExchangeID = exchangeid,
                OrderSysID = ordersysid
            };
            return _proxy.ReqOrderAction(inputOrderAction);
        }

        public int ReqQryOrder()
        {
            Thread.Sleep(1000);
            return _proxy.ReqQryOrder();
        }

        public int ReqQryTrade()
        {
            Thread.Sleep(1000);
            return _proxy.ReqQryTrade();
        }
        /// <summary>
        /// 请求查询合约保证金率
        /// </summary>
        /// <param name="pQryInstrumentMarginRate"></param>
        /// <returns></returns>
        public int ReqQryInstrumentMarginRate(QryInstrumentMarginRateField pQryInstrumentMarginRate)
        {
            if (!isStarted)
            {
                loginEvent.Reset();
                //登录过程中，如果存在本地合约的交易费信息，则直接加载
                if (instrumentCfg != null)
                {
                    string key = string.Empty;
                    instrumentCfg.MarginRates.ForEach(item =>
                    {
                        key = item.InstrumentID + (char)item.HedgeFlag;
                        if (!dicMarginRate.ContainsKey(key))
                        {
                            dicMarginRate.TryAdd(key, item);
                        }
                    });
                    if (dicMarginRate.Count > 0)
                    {
                        loginEvent.Set();
                        return 0;
                    }
                }
            }

            Thread.Sleep(1000);
            if (string.IsNullOrEmpty(pQryInstrumentMarginRate.BrokerID))
            {
                pQryInstrumentMarginRate.BrokerID = Broker;
            }
            if (string.IsNullOrEmpty(pQryInstrumentMarginRate.InvestorID))
            {
                pQryInstrumentMarginRate.InvestorID = Investor;
            }
            return 1;
            return _proxy.ReqQryInstrumentMarginRate(pQryInstrumentMarginRate);
        }
        /// <summary>
        /// 请求查询合约保证金率
        /// </summary>
        /// <param name="instrumentID"></param>
        /// <param name="hedgeFlag"></param>
        /// <returns></returns>
        public int ReqQryInstrumentMarginRate(string instrumentID, char hedgeFlag)
        {
            if (!isStarted)
            {
                loginEvent.Reset();
                //登录过程中，如果存在本地合约的交易费信息，则直接加载
                if (instrumentCfg != null)
                {
                    string key = string.Empty;
                    instrumentCfg.MarginRates.ForEach(item =>
                    {
                        key = item.InstrumentID + (char)item.HedgeFlag;
                        if (!dicMarginRate.ContainsKey(key))
                        {
                            dicMarginRate.TryAdd(key, item);
                        }
                    });
                    if (dicMarginRate.Count > 0)
                    {
                        loginEvent.Set();
                        return 0;
                    }
                }
            }
            return 1;
            QryInstrumentMarginRateField pQryInstrumentMarginRate = new QryInstrumentMarginRateField()
            {
                BrokerID = Broker,
                InvestorID = Investor,
                InstrumentID = instrumentID,
                HedgeFlag = hedgeFlag
            };
            Thread.Sleep(1000);
            return _proxy.ReqQryInstrumentMarginRate(pQryInstrumentMarginRate);
        }

        /// <summary>
        /// 请求查询合约手续费率
        /// </summary>
        /// <param name="pQryInstrumentCommissionRate"></param>
        /// <returns></returns>
        public int ReqQryInstrumentCommissionRate(QryInstrumentCommissionRateField pQryInstrumentCommissionRate)
        {
            if (!isStarted)
            {
                loginEvent.Reset();
                //登录过程中，如果存在本地合约的交易费信息，则直接加载
                if (instrumentCfg != null)
                {
                    instrumentCfg.CommissionRates.ForEach(item =>
                    {
                        if (!dicCommissionRate.ContainsKey(item.InstrumentID))
                        {
                            dicCommissionRate.TryAdd(item.InstrumentID, item);
                        }
                    });
                    if (dicCommissionRate.Count > 0)
                    {
                        loginEvent.Set();
                        return 0;
                    }
                }
            }
            return 1;
            if (string.IsNullOrEmpty(pQryInstrumentCommissionRate.BrokerID))
            {
                pQryInstrumentCommissionRate.BrokerID = Broker;
            }
            if (string.IsNullOrEmpty(pQryInstrumentCommissionRate.InvestorID))
            {
                pQryInstrumentCommissionRate.InvestorID = Investor;
            }
            Thread.Sleep(1000);
            return _proxy.ReqQryInstrumentCommissionRate(pQryInstrumentCommissionRate);
        }
        /// <summary>
        /// 请求查询合约手续费率
        /// </summary>
        /// <param name="instrumentID"></param>
        /// <returns></returns>
        public int ReqQryInstrumentCommissionRate(string instrumentID)
        {
            if (!isStarted)
            {
                loginEvent.Reset();
                //登录过程中，如果存在本地合约的交易费信息，则直接加载
                if (instrumentCfg != null && instrumentCfg.CommissionRates != null)
                {
                    instrumentCfg.CommissionRates.ForEach(item =>
                    {
                        if (!dicCommissionRate.ContainsKey(item.InstrumentID))
                        {
                            dicCommissionRate.TryAdd(item.InstrumentID, item);
                        }
                    });
                    if (dicCommissionRate.Count > 0)
                    {
                        loginEvent.Set();
                        return 0;
                    }
                }
            }
            return 1;
            QryInstrumentCommissionRateField pQryInstrumentCommissionRate = new QryInstrumentCommissionRateField()
            {
                BrokerID = Broker,
                InvestorID = Investor,
                InstrumentID = instrumentID
            };
            Thread.Sleep(1000);
            return _proxy.ReqQryInstrumentCommissionRate(pQryInstrumentCommissionRate);
        }

        /// <summary>
        /// 查询品种
        /// </summary>
        /// <param name="pProductField"></param>
        /// <returns></returns>
        public int ReqQryProduct(QryProductField pProductField)
        {
            Thread.Sleep(1000);
            return _proxy.ReqQryProduct(pProductField);
        }


        public int ReqQryInstrument(QryInstrumentField pInstrumentField)
        {
            if (!isStarted)
            {
                loginEvent.Reset();
                //登录过程中，如果存在本地合约信息，则直接加载
                if (instrumentCfg != null)
                {
                    instrumentCfg.ListInstrument.ForEach(item =>
                    {
                        if (!TQMainModel.dicInstrumentData.ContainsKey(item.InstrumentID))
                        {
                            TQMainModel.dicInstrumentData.Add(item.InstrumentID, item);
                        }
                    });
                }
                if (TQMainModel.dicInstrumentData.Count > 100)
                {
                    loginEvent.Set();
                    //ReqQryPosition();
                    return 0;
                }
            }
            return 1;
            Thread.Sleep(1000);
            return _proxy.ReqQryInstrument(pInstrumentField);

        }

        public int ReqQryPosition()
        {
            Thread.Sleep(1000);
            return _proxy.ReqQryPosition();
        }

        public int ReqQryInvestorPositionDetail()
        {
            Thread.Sleep(1000);
            return _proxy.ReqQryInvestorPositionDetail(new QryInvestorPositionDetailField()
            { InstrumentID = string.Empty, BrokerID = Broker, InvestorID = Investor });
        }
        public int ReqQryAccount()
        {
            Utility.WriteMemLog("ReqQryTradingAccount    ");
            Thread.Sleep(1000);
            return _proxy.ReqQryAccount();
        }

        /// <summary>
        /// 请求查询结算单
        /// </summary>
        /// <returns></returns>
        //public int ReqQrySettlementInfo()
        //{            
        //    return _proxy.ReqQrySettlementInfo();
        //}
        public int ReqQrySettlementInfo(QrySettlementInfoField  pSettlement)
        {
            return _proxy.ReqQrySettlementInfo(pSettlement);
        }
        #region 银期转账请求指令

        /// <summary>
        /// 资金账户口令更新请求
        /// </summary>
        /// <param name="pTradingAccountPasswordUpdate"></param>
        /// <returns></returns>
        public int ReqTradingAccountPasswordUpdate(TradingAccountPasswordUpdateField pTradingAccountPasswordUpdate)
        {
            return _proxy.ReqTradingAccountPasswordUpdate(pTradingAccountPasswordUpdate);
        }

        /// <summary>
        /// 资金账户口令更新请求
        /// </summary>
        /// <param name="brokerID">经纪公司代码</param>
        /// <param name="accountID">投资者账号</param>
        /// <param name="oldPwd">原来的口令</param>
        /// <param name="newPwd">新口令</param>
        /// <param name="currencyID">币种代码</param>
        /// <returns></returns>
        public int ReqTradingAccountPasswordUpdate(string brokerID, string accountID, string oldPwd, string newPwd, string currencyID = "CNY")
        {
            TradingAccountPasswordUpdateField pTradingAccountPasswordUpdate = new TradingAccountPasswordUpdateField()
            {
                AccountID = accountID,
                BrokerID = brokerID,
                CurrencyID = currencyID,
                NewPassword = newPwd,
                OldPassword = oldPwd
            };
            return _proxy.ReqTradingAccountPasswordUpdate(pTradingAccountPasswordUpdate);
        }
        /// <summary>
        /// 请求查询转帐银行
        /// </summary>
        /// <param name="pQryTransferBank"></param>
        /// <returns></returns>
        public int ReqQryTransferBank(QryTransferBankField pQryTransferBank)
        {
            return _proxy.ReqQryTransferBank(pQryTransferBank);
        }

        /// <summary>
        /// 请求查询转帐流水(仅包含必须传输的参数)
        /// </summary>
        /// <param name="brokerID"></param>
        /// <param name="accountID"></param>
        /// <param name="bankID"></param>
        /// <param name="currencyID"></param>
        /// <returns></returns>
        public int ReqQryTransferSerial(string brokerID, string accountID, string bankID, string currencyID = "CNY")
        {
            QryTransferSerialField pQryTransferSerial = new QryTransferSerialField()
            {
                BrokerID = brokerID,
                AccountID = accountID,
                BankID = bankID,
                CurrencyID = currencyID
            };
            return _proxy.ReqQryTransferSerial(pQryTransferSerial);
        }
        /// <summary>
        /// 请求查询转帐流水(仅包含必须传输的参数)
        /// </summary>
        /// <param name="pQryTransferSerial"></param>
        /// <returns></returns>
        public int ReqQryTransferSerial(QryTransferSerialField pQryTransferSerial)
        {
            return _proxy.ReqQryTransferSerial(pQryTransferSerial);
        }

        /// <summary>
        /// 请求查询银期签约关系
        /// </summary>
        /// <param name="brokerID">期商代码</param>
        /// <param name="bankID">银行代码(可以查询指定银行账号信息，不填就是查询所有签约银行帐号信息)</param>
        /// <param name="bankBranchId">银行分支代码</param>
        /// <param name="currencyID">币种("CNY","HKD","USD")</param>
        /// <returns></returns>
        public int ReqQryAccountregister(string brokerID, string bankID, string bankBranchId, string currencyID)
        {
            QryAccountregisterField pQryAccountregister = new QryAccountregisterField()
            {
                BrokerID = brokerID,
                BankID = bankID,
                BankBrchID = bankBranchId,
                CurrencyID = currencyID
            };
            return _proxy.ReqQryAccountregister(pQryAccountregister);
        }
        /// <summary>
        /// 请求查询银期签约关系
        /// </summary>
        /// <param name="pQryAccountregister"></param>
        /// <returns></returns>
        public int ReqQryAccountregister(QryAccountregisterField pQryAccountregister)
        {
            return _proxy.ReqQryAccountregister(pQryAccountregister);
        }

        /// <summary>
        /// 请求查询签约银行
        /// </summary>
        /// <param name="pQryContractBank"></param>
        /// <returns></returns>
        public int ReqQryContractBank(QryContractBankField pQryContractBank)
        {
            return _proxy.ReqQryContractBank(pQryContractBank);
        }

        /// <summary>
        /// 期货发起期货资金转银行请求
        /// </summary>
        /// <param name="bankID">银行代码</param>
        /// <param name="bankAccount">银行账号</param>
        /// <param name="brokerID">期货公司代码</param>
        /// <param name="bankPwd">银行帐户密码</param>
        /// <param name="accountID">投资者帐号</param>
        /// <param name="password">资金帐户密码</param>
        /// <param name="currencyID">币种</param>
        /// <param name="tradeAmount">转帐金额</param>
        /// <returns></returns>
   

       
       
        #endregion

        #region 预埋-条件单请求指令
        /// <summary>
        /// 预埋单录入请求
        /// </summary>
        /// <param name="pParkedOrder"></param>
        /// <returns></returns>
        public int ReqParkedOrderInsert(ParkedOrderField pParkedOrder, PricingMode pType = PricingMode.Preset)
        {
            Trader.OrderNo = Trader.OrderNo + 1;
            string orderRef = Trader.OrderNo.ToString();
            return _proxy.ReqParkedOrderInsert(pParkedOrder, pType, orderRef);
        }

        public int ReqParkedOrderInsert(string instrumentID, TradeDirection direction, OffsetFlagType offset, HedgeType hedge, double price, int volume, TimeConditionType timeCondition, VolumeConditionType volumeCondition,
            ContingentConditionType contingentCondition, PricingMode pType = PricingMode.Preset)
        {
            Trader.OrderNo += 1;
            string orderRef = Trader.OrderNo.ToString();
            /*必填：期商代码，
             */
            ParkedOrderField pParkedOrder = new ParkedOrderField()
            {
                BrokerID = Broker,
                InvestorID = Investor,
                ContingentCondition = (char)contingentCondition,
                Direction = (char)direction,
                InstrumentID = instrumentID,
                LimitPrice = price,
                TimeCondition = (char)timeCondition,
                UserID = Investor,
                VolumeCondition = (char)volumeCondition,
                VolumeTotalOriginal = volume
            };
            pParkedOrder.CombHedgeFlag = Convert.ToString((char)SysEnum.HedgeFlagType.Speculation);
            pParkedOrder.CombOffsetFlag = Convert.ToString((char)offset);
            pParkedOrder.MinVolume = 1;
            pParkedOrder.IsAutoSuspend = 0;//自动挂起标志:否
            pParkedOrder.UserForceClose = 0;//用户强平标识:否
            pParkedOrder.UserType = (char)UserType.Investor;
            return _proxy.ReqParkedOrderInsert(pParkedOrder, pType, orderRef);
        }
        /// <summary>
        /// 预埋撤单录入请求
        /// </summary>
        /// <param name="pParkedOrderAction"></param>
        /// <returns></returns>
        public int ReqParkedOrderAction(ParkedOrderActionField pParkedOrderAction)
        {
            return _proxy.ReqParkedOrderAction(pParkedOrderAction);
        }

        /// <summary>
        ///预埋撤单录入请求
        /// </summary>
        /// <param name="orderRef"></param>
        /// <param name="frontID"></param>
        /// <param name="sessionID"></param>
        /// <param name="exchangeID"></param>
        /// <param name="orderSysID"></param>
        /// <param name="instrumentID"></param>
        /// <returns></returns>
        public int ReqParkedOrderAction(string orderRef, int frontID, int sessionID, string exchangeID, string orderSysID, string instrumentID)
        {
            ParkedOrderActionField pParkedOrderAction = new ParkedOrderActionField()
            {
                BrokerID = Broker,
                InvestorID = Investor,
                OrderRef = orderRef,
                FrontID = frontID,
                SessionID = sessionID,
                ExchangeID = exchangeID,
                OrderSysID = orderSysID,
                UserID = Investor,
                InstrumentID = instrumentID
            };
            pParkedOrderAction.ActionFlag = (char)ActionFlagType.Delete;
            return _proxy.ReqParkedOrderAction(pParkedOrderAction);
        }
        /// <summary>
        /// 请求删除预埋单
        /// </summary>
        /// <param name="pRemoveParkedOrder"></param>
        /// <returns></returns>
        public int ReqRemoveParkedOrder(RemoveParkedOrderField pRemoveParkedOrder)
        {
            return _proxy.ReqRemoveParkedOrder(pRemoveParkedOrder);
        }

        /// <summary>
        /// 请求删除预埋单
        /// </summary>
        /// <param name="parkedOrderID">预埋单报单编号</param>
        /// <returns></returns>
        public int ReqRemoveParkedOrder(string parkedOrderID)
        {
            RemoveParkedOrderField pRemoveParkedOrder = new RemoveParkedOrderField()
            {
                BrokerID = Broker,
                InvestorID = Investor,
                ParkedOrderID = parkedOrderID
            };
            return _proxy.ReqRemoveParkedOrder(pRemoveParkedOrder);
        }
        /// <summary>
        /// 请求删除预埋撤单
        /// </summary>
        /// <param name="pRemoveParkedOrderAction"></param>
        /// <returns></returns>
        public int ReqRemoveParkedOrderAction(RemoveParkedOrderActionField pRemoveParkedOrderAction)
        {
            return _proxy.ReqRemoveParkedOrderAction(pRemoveParkedOrderAction);
        }

        /// <summary>
        /// 请求删除预埋撤单
        /// </summary>
        /// <param name="parkedOrderActionID">预埋单撤单编号（预埋撤单下单成功所得ID）</param>
        /// <returns></returns>
        public int ReqRemoveParkedOrderAction(string parkedOrderActionID)
        {
            RemoveParkedOrderActionField pRemoveParkedOrderAction = new RemoveParkedOrderActionField()
            {
                BrokerID = Broker,
                InvestorID = Investor,
                ParkedOrderActionID = parkedOrderActionID
            };
            return _proxy.ReqRemoveParkedOrderAction(pRemoveParkedOrderAction);
        }
        /// <summary>
        /// 请求查询预埋单
        /// </summary>
        /// <param name="pQryParkedOrder"></param>
        /// <returns></returns>
        public int ReqQryParkedOrder(QryParkedOrderField pQryParkedOrder)
        {
            return _proxy.ReqQryParkedOrder(pQryParkedOrder);
        }

        /// <summary>
        /// 请求查询预埋撤单
        /// </summary>
        /// <param name="pQryParkedOrderAction"></param>
        /// <returns></returns>
        public int ReqQryParkedOrderAction(QryParkedOrderActionField pQryParkedOrderAction)
        {
            return _proxy.ReqQryParkedOrderAction(pQryParkedOrderAction);
        }

        /// <summary>
        /// 条件单下单请求
        /// </summary>
        /// <param name="pFiled"></param>
        /// <returns></returns>
        public int ReqCondOrderInsert(InputOrderField pFiled, PricingMode LimitMarketFakFok = PricingMode.Preset)
        {
            Trader.OrderNo = Trader.OrderNo + 1;
            string orderRef = Trader.OrderNo.ToString();
            return _proxy.ReqCondOrderInsert(pFiled, LimitMarketFakFok, orderRef);
        }

        #endregion

       
      
        #endregion

        #region implement  tradeInterface

        public int ReqOrderInsert(string InstrumentID, TradeDirection pDirection, OffsetType pOffset, double pPrice, int pVolume, LimitMarket pType, 
            HedgeType pHedge = HedgeType.投机, double pStopPrice = 0, ForceCloseReasonType forceclose = ForceCloseReasonType.非强平, FakFok pFakFok = FakFok.Default)
        {
            string orderno = Trader.OrderNo.ToString();
            //string OrderRef = orderno.PadLeft(12 - orderno.Length);
            InputOrderField inputOrder = new InputOrderField()
            {
                InstrumentID = InstrumentID,
                BrokerID = Broker,
                InvestorID = Investor,
                CombHedgeFlag = ((char)pHedge).ToString(),
                TradeDirection = (char)pDirection,
                CombOffsetFlag = ((char)pOffset).ToString(),
                VolumeTotalOriginal = pVolume,
                LimitPrice = pPrice,
                StopPrice = pStopPrice,
                IsAutoSuspend = 0,
                ContingentCondition = (char)ContingentConditionType.立即,
                ForceCloseReason = (char)forceclose,
                IsSwapOrder = 0,
                UserForceClose = 0,
                VolumeCondition = (char)VolumeConditionType.任何数量,
                MinVolume = 1,
                UserID = Investor,
                RequestID = ++RequestID,
                //OrderRef = Utility.GetOrderRef(),
            };

            switch (pType)
            {
                case LimitMarket.限价:
                    inputOrder.TimeCondition = (char)TimeConditionType.GFD;
                    inputOrder.OrderPriceType = (char)OrderPriceType.限价;
                    break;
                case LimitMarket.市价:
                    if (Utility.GetExchangeID(InstrumentID) != EnuExchangeID.Binance)
                    {
                        inputOrder.OrderPriceType = (char)OrderPriceType.市价;
                        inputOrder.LimitPrice = 0;
                        inputOrder.TimeCondition = (char)TimeConditionType.IOC;
                    }
                    else
                    {
                        inputOrder.OrderPriceType = (char)OrderPriceType.限价;
                        inputOrder.LimitPrice = (double)(pDirection == TradeDirection.Long ? TQMainModel.dicMajorMarketData[InstrumentID].LimitHigh : TQMainModel.dicMajorMarketData[InstrumentID].LimitLow);
                        inputOrder.TimeCondition = (char)TimeConditionType.GFD;
                    }
                    break;
                //case PricingMode.Opposite:// 对手价
                //    inputOrder.TimeCondition = (char)TimeConditionType.GFD;
                //    inputOrder.OrderPriceType = (char)OrderPriceType.限价;

                //    inputOrder.LimitPrice = pDirection == TradeDirection.多 ? TQMain.dicMarketData[InstrumentID].AskPrice1 : TQMain.dicMarketData[InstrumentID].BidPrice1;
                //    break;
                //case PricingMode.Ownside:// 排队价
                //    inputOrder.TimeCondition = (char)TimeConditionType.GFD;
                //    inputOrder.OrderPriceType = (char)OrderPriceType.限价;

                //    inputOrder.LimitPrice = pDirection == TradeDirection.空 ? TQMain.dicMarketData[InstrumentID].AskPrice1 : TQMain.dicMarketData[InstrumentID].BidPrice1;
                //    break;
            }

            switch (pFakFok)
            {
                case FakFok.FAK:
                    inputOrder.VolumeCondition = (char)VolumeConditionType.任何数量;
                    inputOrder.TimeCondition = (char)TimeConditionType.IOC;
                    break;
                case FakFok.FOK:
                    inputOrder.VolumeCondition = (char)VolumeConditionType.全部数量;
                    inputOrder.TimeCondition = (char)TimeConditionType.IOC;
                    break;
            }

            int returnresult = _proxy.ReqOrderInsert(inputOrder);
            SendMail(inputOrder);
            return returnresult;
        }
        public int ReqOrderInsert(ComboMarketData cust, TradeDirection Direction, int Quant, string custorderRef)
        {
            return 1;
        }

        public int ReqOrderInsert(MajorMarketData md, TradeDirection Direction, int Quant)
        {
            string orderRef = "";
            return ReqOrderInsert(md, Direction, Quant, md.OrderBoardPricingMode, orderRef);
        }

        public int ReqOrderInsert(MajorMarketData md, TradeDirection Direction, int Quant,string orderRef)
        {
            return ReqOrderInsert(md, Direction, Quant, md.OrderBoardPricingMode,orderRef);
        }

        public int ReqOrderInsert(MajorMarketData md, TradeDirection Direction, int Quant, PricingMode PriceMode, string orderRef)   //Order from OrderBoard
        {
           
            return 0;
        }
        public int ReqOrderInsert(InputOrderField input)
        {
            return 0;//_proxy.ReqOrderInsert(input);
        }
        public void SendMail(InputOrderField input)
        {
            string subject = string.Format("资金账号({0})的{1}下单通知", input.InvestorID, input.InstrumentID);
            string mailContent = string.Format("按{0}价格{1} {2} {3} {4}张。\r\n{5}", input.LimitPrice, input.TradeDirection == '0' ? "买入" : "卖出", input.InstrumentID, input.CombOffsetFlag, input.VolumeTotalOriginal, DateTime.Now.ToString("yyyyMMdd hh:mm:ss"));
            Task.Run(new Action(() => { Utility.SendNotice(subject, mailContent); })); 
        }
                
        public int ReqCloseBiDirectionalPositions(MajorMarketData orderboard, PricingMode ordertype = PricingMode.Market)
        {
            PositionDataSummary PDS;
            //MarketData marketdata;
            PricingMode OriginalPricingMode = orderboard.OrderBoardPricingMode;
            orderboard.OrderBoardPricingMode = PricingMode.Market;
            if (TQMainModel.dicPositionSummary.TryGetValue(Investor + orderboard.InstrumentID + TradeDirection.Long, out PDS)) //有多仓
            {
                ReqClosePosition(orderboard, TradeDirection.Short);      //平多单
            }
            if (TQMainModel.dicPositionSummary.TryGetValue(Investor + orderboard.InstrumentID + TradeDirection.Short, out PDS))
            {
                ReqClosePosition(orderboard, TradeDirection.Long);    //平空单
            }
            orderboard.OrderBoardPricingMode = OriginalPricingMode;
            return 0;
        }
        /// <summary>
        /// For Close Position of directional
        /// </summary>
        /// <param name="orderboard"></param>
        /// <param name="Direction"></param>
        /// <param name="Q"></param>
        /// <returns></returns>
        public void ReqClosePosition(MajorMarketData orderboard, TradeDirection Direction, int Q = 0, string orderRef="", double PresetPrice=-1)
        {
             
        }
        
        /// <summary>
        /// 撤单录入请求
        /// </summary>
        /// <param name="frontID">前置机ID</param>
        /// <param name="sessionID">回话ID</param>
        /// <param name="orderRef">报单引用</param>
        /// <returns></returns>
        public int ReqOrderAction(int frontID, int sessionID, string orderRef, string instrumentID)
        {
            return 0;
        }
        /// <summary>
        /// 撤单录入请求
        /// </summary>
        /// <param name="inputOrderAction">输入报单操作</param>
        /// <returns></returns>
		public int ReqOrderAction(InputOrderActionField inputOrderAction)
        {
            string subject = string.Format("资金账号({0})的{1}撤单通知", Investor, inputOrderAction.InstrumentID);
            string mailContent = string.Format("对{0}撤单：\r\n前置编号/FrontID:{1}\t会话编号/SessionID:{2}\t报单引用：{3}。\r\n{4}", inputOrderAction.InstrumentID, inputOrderAction.FrontID, inputOrderAction.SessionID.ToString(), inputOrderAction.OrderRef, DateTime.Now.ToString("yyyyMMdd hh:mm:ss"));
            Task.Run(new Action(() => { Utility.SendNotice(subject, mailContent); })); ;

            inputOrderAction.BrokerID = Broker;
            inputOrderAction.InvestorID = Investor;
            inputOrderAction.UserID = Investor;
            return _proxy.ReqOrderAction(inputOrderAction);
        }
        public void ReqOrderActionCancel()
        { }
        public void ReqExchangePosition()
        { }
        public void ReqReversePosition(PositionDataSummary position)
        {
           /* //1.先对当前持仓汇总平仓,这里不要修改dicMarketData中的值为好
            int CurrentPosition = position.Position;
            TradeDirection dir = (position.Direction == TradeDirection.空) ? TradeDirection.多 : TradeDirection.空;

            var orderboard = Utility.GetNewMarketData(TQMainVM.dicMajorMarketData[position.InstrumentID]);

            orderboard.OrderBoardPricingMode = PricingMode.Market;
            //orderboard.upperLimitPrice = TQMain.dicMarketData[]
            orderboard.OrderBoardOrderMode = OrderMode.Close;
            orderboard.OrderBoardQuant = CurrentPosition;
            //orderboard.OrderBoardPriceMode = PriceMode.PreSet;
            //orderboard.OrderBoardHedgeType = HedgeType.投机;
            orderboard.Exchange = Utility.GetExchangeID(position.InstrumentID);
            ReqClosePosition(orderboard, dir, CurrentPosition);
            //2.再开反向仓
            orderboard.OrderBoardPricingMode = PricingMode.Market;
            orderboard.OrderBoardOrderMode = OrderMode.Open;
            //orderboard.OrderBoardPriceMode = PriceMode.PreSet;
            //orderboard.OrderBoardHedgeType = HedgeType.投机;
            orderboard.Exchange = Utility.GetExchangeID(position.InstrumentID);
            ReqOrderInsert(orderboard, dir, CurrentPosition);*/
        }

        public void ReqReversePosition()
        {
            
        }
        public void ReqForceClose(MajorMarketData orderboard)   //force to close all position ASAP
        {
            /*
            foreach (var v in TQMainVM.dicPositionSummary)
            {
                var ob = new MajorMarketData(v.Value.InstrumentID)
                {
                    //OrderBoardHedgeType = HedgeType.投机,
                    OrderBoardOffsetFlag = OffsetType.平仓,
                    OrderBoardPricingMode = PricingMode.Market,
                    //OrderBoardPrice = 0,
                    Exchange = Utility.GetExchangeID(v.Value.ExchangeName),
                    OrderBoardInvestorID = v.Value.InvestorID,
                    //InstrumentID = v.Value.InstrumentID,
                    OrderBoardQuant = v.Value.Position,
                    OrderBoardTradeDirection = v.Value.Direction == TradeDirection.多 ? TradeDirection.空 : TradeDirection.多,
                };
                ReqClosePosition(ob, ob.OrderBoardTradeDirection, ob.OrderBoardQuant);
            }*/
        }
        public int ReqQryBrokerTradingParams(QryBrokerTradingParamsField pFiled)
        {
            return _proxy.ReqQryBrokerTradingParams(pFiled);
        }
        public void trade_OnRspQryBrokerTradingParams(BrokerTradingParamsField pBrokerTradingParams, RspInfoField pRspInfo, int nRequestID, bool bIsLast)
        {

        }

        public int ReqQryBrokerTradingAlgos(QryBrokerTradingAlgosField pFiled)
        {
            return _proxy.ReqQryBrokerTradingAlgos(pFiled);
        }
        ///请求查询经纪公司交易算法响应
        public void trade_OnRspQryBrokerTradingAlgos(BrokerTradingAlgosField pBrokerTradingAlgos, RspInfoField pRspInfo, int nRequestID, bool bIsLast)
        {
        }
        #endregion
    }
  
}
