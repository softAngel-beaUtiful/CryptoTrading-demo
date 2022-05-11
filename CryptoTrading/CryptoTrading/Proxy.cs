using System;
using System.IO;
using System.Runtime.InteropServices;
using CryptoTrading;
using CryptoTrading.Model;

namespace CryptoTrading
{

    // exchange[8], xxxtime[16], id[32], msg[128]


    #region structs
    /// <summary>
    /// 合约信息
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class InstrumentField
    {
        ///合约代码
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 31)]
        public string InstrumentID;
        ///交易所代码
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string ExchangeID;
        ///合约名称
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 21)]
        public string InstrumentName;
        ///合约在交易所的代码
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 31)]
        public string ExchangeInstID;
        ///产品代码
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 31)]
        public string ProductID;
        ///产品类型
        public char ProductClass;
        ///交割年份
        public int DeliveryYear;
        ///交割月
        public int DeliveryMonth;
        ///市价单最大下单量
        public int MaxMarketOrderVolume;
        ///市价单最小下单量
        public int MinMarketOrderVolume;
        ///限价单最大下单量
        public int MaxLimitOrderVolume;
        ///限价单最小下单量
        public int MinLimitOrderVolume;
        ///合约数量乘数
        public int VolumeMultiple;
        ///最小变动价位
        public double PriceTick;
        ///创建日
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string CreateDate;
        ///上市日
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string OpenDate;
        ///到期日
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string ExpireDate;
        ///开始交割日
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string StartDelivDate;
        ///结束交割日
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string EndDelivDate;
        ///合约生命周期状态
        public char InstLifePhase;
        ///当前是否交易
        public int IsTrading;
        ///持仓类型
        public char PositionType;
        ///持仓日期类型
        public char PositionDateType;
        ///多头保证金率
        public double LongMarginRatio;
        ///空头保证金率
        public double ShortMarginRatio;
        ///是否使用大额单边保证金算法
        public char MaxMarginSideAlgorithm;
        ///基础商品代码
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 31)]
        public string UnderlyingInstrID;
        ///执行价
        public double StrikePrice;
        ///期权类型
        public double OptionsType;
        ///合约基础商品乘数
        public double UnderlyingMultiple;
        ///组合类型
        public char CombinationType;
    }

    /// <summary>
    /// 查询产品
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class QryProductField
    {
        /// <summary>
        /// 产品代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 31)]
        public string ProductID;
        /// <summary>
        /// 产品类型
        /// </summary>
        public char ProductClass;
    };
    /// <summary>
    /// 查询合约结构体
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class QryInstrumentField
    {
        ///合约代码
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 31)]
        public string InstrumentID;

        ///交易所代码
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string ExchangeID;

        ///合约在交易所的代码
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 31)]
        public string ExchangeInstID;

        ///产品代码
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 31)]
        public string ProductID;
    };
    ///<summary>
    ///结算单确认信息
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class SettlementInfoConfirmField
    {
        ///经纪公司代码
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 11)]
        public string BrokerID;

        ///投资者代码
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string InvestorID;

        ///确认日期
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string ConfirmDate;

        ///确认时间
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string ConfirmTime;
    }

    [StructLayout(LayoutKind.Sequential)]
    public class RspInfoField
    {
        ///错误代码
        public int ErrorID;
        ///错误信息
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 81)]
        public string ErrorMsg;
    }
    #endregion structs

    public class Proxy
    {
        public IntPtr _handle;
        /// <summary>
        /// 导入C++ dll文件
        /// </summary>
        /// <param name="pFile">文件名(包含完整路径)</param>
        public Proxy(string pFile)
        {
            LoadDll(pFile);
        }

        ~Proxy()
        {
            FreeLibrary(_handle);
            //if (File.Exists(_file))
            //    File.Delete(_file);
        }

        /// <summary>
        ///     原型是 :HMODULE LoadLibrary(LPCTSTR lpFileName);
        /// </summary>
        /// <param name="lpFileName"> DLL 文件名 </param>
        /// <returns> 函数库模块的句柄 </returns>
        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string lpFileName);

        /// <summary>
        ///     原型是 : FARPROC GetProcAddress(HMODULE hModule, LPCWSTR lpProcName);
        /// </summary>
        /// <param name="hModule"> 包含需调用函数的函数库模块的句柄 </param>
        /// <param name="lpProcName"> 调用函数的名称 </param>
        /// <returns> 函数指针 </returns>
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        /// <summary>
        ///     原型是 : BOOL FreeLibrary(HMODULE hModule);
        /// </summary>
        /// <param name="hModule"> 需释放的函数库模块的句柄 </param>
        /// <returns> 是否已释放指定的 Dll </returns>
        [DllImport("kernel32", EntryPoint = "FreeLibrary", SetLastError = true)]
        protected static extern bool FreeLibrary(IntPtr hModule);

        /// <summary>
        ///
        /// </summary>
        /// <param name="pHModule"></param>
        /// <param name="lpProcName"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private static Delegate Invoke(IntPtr pHModule, string lpProcName, Type t)
        {
            // 若函数库模块的句柄为空，则抛出异常
            if (pHModule == IntPtr.Zero)
            {
                throw (new Exception(" 函数库模块的句柄为空 , 请确保已进行 LoadDll 操作 !"));
            }
            // 取得函数指针
            IntPtr farProc = GetProcAddress(pHModule, lpProcName);
            // 若函数指针，则抛出异常
            if (farProc == IntPtr.Zero)
            {
                throw (new Exception(" TradeDLL 没有找到 :" + lpProcName + " 这个函数的入口点 "));
            }
            return Marshal.GetDelegateForFunctionPointer(farProc, t); //由函数指针返回委托给C#托管代码使用
        }
        //private string _file;

        /// <summary>
        /// 加载C++文件,取得相应函数
        /// </summary>
        /// <param name="pFile">格式:proxyXXXQuote.dll或proxyXXXTrade.dll;XXX-平台名</param>
        /// <exception cref="Exception"></exception>
        protected void LoadDll(string pFile)
        {
            //Directory.CreateDirectory("log");
            //string fullpathfile = Environment.CurrentDirectory + pFile;
            if (File.Exists(pFile))
            {
                //_file = Environment.CurrentDirectory + "\\log\\" + DateTime.Now.Ticks + ".dll";
                //File.Copy(pFile, _file);
                _handle = LoadLibrary(pFile);
            }
            else
            {
                throw (new Exception(string.Format("没有找到 ctp_proxy_trade.dll  :{0}.", pFile)));
            }
            if (_handle == IntPtr.Zero)
            {                 
                throw (new Exception(string.Format("Current Directory is: "+Environment.CurrentDirectory+"   没有找到!!! thosttraderapi.dll !!!!  {0}.","thosttraderapi.dll")));
            }

        }
        #region 请求的代理

        private delegate int RQCreateApi(string flowPath);

        private delegate int RQReqConnect(string[] pFronts, int nCount);

        private delegate int RQReqUserLogin(string pInvestor, string pPwd, string pBroker);

        private delegate int RQReqQryOrder();

        private delegate int RQReqQryTrade();

        private delegate int RQReqQryPosition();

        private delegate int RQReqQryInvestorPositionDetail(QryInvestorPositionDetailField pQryInvestorPosiDetail);

        private delegate int RQReqQryAccount();

        private delegate int RQReqSettlementInfoConfirm();

        //private delegate void RQReqUserLogout(ThostFtdcUserLogoutField logout, int nRequestID);

        private delegate int RQReqUserPasswordUpdate(UserPasswordUpdateField field);

        private delegate IntPtr RQGetTradingDay();

       // private delegate int RQReqOrderInsert(string pInstrument, DirectionType pDirection, OffsetType pOffset, double pPrice, int pVolume, HedgeType pHedge, OrderType pType, string pCustom);

        //private delegate int RQReqOrderInsert(string pInstrument, DirectionType pDirection, OffsetType pOffset, double pPrice, int pVolume, HedgeType pHedge, OrderType pType, string pCustom);
        private delegate int RQReqOrderInsert(InputOrderField inputOrder);

        private delegate int RQReqOrderAction(InputOrderActionField inputOrderAction);

        ///请求查询合约保证金率
        private delegate int RQReqQryInstrumentMarginRate(QryInstrumentMarginRateField pQryInstrumentMarginRate);
        ///请求查询合约手续费率
        private delegate int RQReqQryInstrumentCommissionRate(QryInstrumentCommissionRateField pQryInstrumentCommissionRate);

        private delegate int RQReqQryProduct(QryProductField pInstrumentField);

        private delegate int RQReqQryInstrument(QryInstrumentField pInstrumentField);

        private delegate int RQReqQrySettlementInfo(QrySettlementInfoField field);

        ///资金账户口令更新请求
        private delegate int RQReqTradingAccountPasswordUpdate(TradingAccountPasswordUpdateField pTradingAccountPasswordUpdate);

        ///请求查询转帐银行
        private delegate int RQReqQryTransferBank(QryTransferBankField pQryTransferBank);

        ///请求查询转帐流水
        private delegate int RQReqQryTransferSerial(QryTransferSerialField pQryTransferSerial);

        ///请求查询银期签约关系
        private delegate int RQReqQryAccountregister(QryAccountregisterField pQryAccountregister);

        ///请求查询签约银行
        private delegate int RQReqQryContractBank(QryContractBankField pQryContractBank);

        ///期货发起银行资金转期货请求
        private delegate int RQReqFromBankToFutureByFuture(ReqTransferField pReqTransfer);

        ///期货发起期货资金转银行请求
        private delegate int RQReqFromFutureToBankByFuture(ReqTransferField pReqTransfer);

        ///期货发起查询银行余额请求
        private delegate int RQReqQueryBankAccountMoneyByFuture(ReqQueryAccountField pReqQueryAccount);

        private delegate int RQReqCondOrderInsert(InputOrderField pFiled, PricingMode orderType, string pCustom);
        ///预埋单录入请求
        private delegate int RQReqParkedOrderInsert(ParkedOrderField pParkedOrder, PricingMode pType, string pCustom);

        ///预埋撤单录入请求
        private delegate int RQReqParkedOrderAction(ParkedOrderActionField pParkedOrderAction);

        //请求删除预埋单
        private delegate int RQReqRemoveParkedOrder(RemoveParkedOrderField pRemoveParkedOrder);

        ///请求删除预埋撤单
        private delegate int RQReqRemoveParkedOrderAction(RemoveParkedOrderActionField pRemoveParkedOrderAction);

        ///请求查询预埋单
        private delegate int RQReqQryParkedOrder(QryParkedOrderField pQryParkedOrder);

        ///请求查询预埋撤单
        private delegate int RQReqQryParkedOrderAction(QryParkedOrderActionField pQryParkedOrderAction);

        ///请求查询交易通知
        private delegate int RQReqQryTradingNotice(QryTradingNoticeField pQryTradingNotice);
        /// <summary>
        /// 请求查询客户通知
        /// </summary>
        /// <returns></returns>
        private delegate int RQReqQryNotice();
        //请求查询投资者响应
        private delegate int RQReqQryInvestor();
        #region 注册响应事件和事件处理函数 Callback
        /// <summary>
        /// 请求查询经纪公司交易参数
        /// </summary>
        /// <param name="pQryBrokerTradingParams"></param>
        /// <returns></returns>
        private delegate int RQReqQryBrokerTradingParams(QryBrokerTradingParamsField pQryBrokerTradingParams);

        /// <summary>
        /// 请求查询经纪公司交易算法
        /// </summary>
        /// <param name="pQryBrokerTradingAlgos"></param>
        /// <returns></returns>
        private delegate int RQReqQryBrokerTradingAlgos(QryBrokerTradingAlgosField pQryBrokerTradingAlgos);

        #endregion
        private delegate void Reg(IntPtr pPtr);

        public delegate void FrontConnected();

        private FrontConnected _OnFrontConnected;

        public event FrontConnected OnFrontConnected
        {
            add
            {
                _OnFrontConnected += value;
                //get delegate to setup EventHandler for RegOnFrontConnected in Ctp-trade-proxy.dll
                Reg reg = (Reg)Invoke(_handle, "RegOnFrontConnected", typeof(Reg));
                //_OnFrontConnected is the private delegate of Proxy's
                IntPtr ii = Marshal.GetFunctionPointerForDelegate(_OnFrontConnected);
                reg(ii);
            }
            remove
            {
                _OnFrontConnected -= value;
                ((Reg)Invoke(this._handle, "RegOnFrontConnected", typeof(Reg)))(Marshal.GetFunctionPointerForDelegate(_OnFrontConnected));
            }
        }

        public delegate void FrontDisconnected(int nReason);

        private FrontDisconnected _OnFrontDisconnected;

        public event FrontDisconnected OnFrontDisconnected
        {
            add
            {
                _OnFrontDisconnected += value;
                Reg reg = (Reg)Invoke(this._handle, "RegOnFrontDisconnected", typeof(Reg));
                IntPtr ii = Marshal.GetFunctionPointerForDelegate(_OnFrontDisconnected);
                reg(ii);
            }
            remove
            {
                _OnFrontDisconnected -= value;
                ((Reg)Invoke(this._handle, "RegOnFrontDisconnected", typeof(Reg)))(Marshal.GetFunctionPointerForDelegate(_OnFrontDisconnected));
            }
        }
        public delegate void RspUserLogin(CThostFtdcRspUserLoginField pLogin, RspInfoField pRspInfo);

        private RspUserLogin _OnRspUserLogin;

        public event RspUserLogin OnRspUserLogin
        {
            add
            {
                _OnRspUserLogin += value;
                (Invoke(this._handle, "RegOnRspUserLogin", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRspUserLogin));
            }
            remove
            {
                _OnRspUserLogin -= value;
                (Invoke(this._handle, "RegOnRspUserLogin", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRspUserLogin));
            }
        }

        public delegate void RspUserLogout(UserLogoutField pUserLogout, RspInfoField pRspInfo, int nRequestID, bool bIsLast);

        private RspUserLogout _OnRspUserLogout;

        public event RspUserLogout OnRspUserLogout
        {
            add
            {
                _OnRspUserLogout += value;
                (Invoke(this._handle, "RegOnRspUserLogout", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRspUserLogout));
            }
            remove
            {
                _OnRspUserLogout -= value;
                (Invoke(this._handle, "RegOnRspUserLogout", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRspUserLogout));
            }
        }

        public delegate void RspUserPasswordUpdate(UserPasswordUpdateField pUserPasswordUpdate, RspInfoField pRspInfo, int nRequestID, bool bIsLast);

        private RspUserPasswordUpdate _OnRspUserPasswordUpdate;

        public event RspUserPasswordUpdate OnRspUserPasswordUpdate
        {
            add
            {
                _OnRspUserPasswordUpdate += value;
                (Invoke(this._handle, "RegOnRspUserPasswordUpdate", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRspUserPasswordUpdate));
            }
            remove
            {
                _OnRspUserPasswordUpdate -= value;
                (Invoke(this._handle, "RegOnRspUserPasswordUpdate", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRspUserPasswordUpdate));
            }
        }

        public delegate void RtnError(int pErrId, string pMsg);

        private RtnError _OnRtnError;

        public event RtnError OnRtnError
        {
            add
            {
                _OnRtnError += value;
                (Invoke(this._handle, "RegOnRtnError", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRtnError));
            }
            remove
            {
                _OnRtnError -= value;
                (Invoke(this._handle, "RegOnRtnError", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRtnError));
            }
        }

        public delegate void RtnInstrumentStatus(ThostFtdcInstrumentStatusField instrumentStatus);

        private RtnInstrumentStatus _OnRtnInstrumentStatus;

        public event RtnInstrumentStatus OnRtnInstrumentStatus
        {
            add
            {
                _OnRtnInstrumentStatus += value;
                (Invoke(_handle, "RegOnRtnInstrumentStatus", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRtnInstrumentStatus));
            }
            remove
            {
                _OnRtnInstrumentStatus -= value;
                (Invoke(_handle, "RegOnRtnInstrumentStatus", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRtnInstrumentStatus));
            }
        }

        public delegate void RspQryInstrumentMarginRate(InstrumentMarginRateField pInstrumentMarginRate, RspInfoField pRspInfo, int nRequestID, bool bIsLast);

        private RspQryInstrumentMarginRate _OnRspQryInstrumentMarginRate;

        public event RspQryInstrumentMarginRate OnRspQryInstrumentMarginRate
        {
            add
            {
                _OnRspQryInstrumentMarginRate += value;
                (Invoke(this._handle, "RegOnRspQryInstrumentMarginRate", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRspQryInstrumentMarginRate));
            }
            remove
            {
                _OnRspQryInstrumentMarginRate -= value;
                (Invoke(this._handle, "RegOnRspQryInstrumentMarginRate", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRspQryInstrumentMarginRate));
            }
        }

        public delegate void RspQryInstrumentCommissionRate(InstrumentCommissionRateField pInstrumentCommissionRate, RspInfoField pRspInfo, int nRequestID, bool bIsLast);

        private RspQryInstrumentCommissionRate _OnRspQryInstrumentCommissionRate;

        public event RspQryInstrumentCommissionRate OnRspQryInstrumentCommissionRate
        {
            add
            {
                _OnRspQryInstrumentCommissionRate += value;
                (Invoke(this._handle, "RegOnRspQryInstrumentCommissionRate", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRspQryInstrumentCommissionRate));
            }
            remove
            {
                _OnRspQryInstrumentCommissionRate -= value;
                (Invoke(this._handle, "RegOnRspQryInstrumentCommissionRate", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRspQryInstrumentCommissionRate));
            }
        }


        public delegate void RspQryProduct(ProductField pProduct, RspInfoField pRspInfo, int nRequestID, bool bIsLast);

        private RspQryProduct _OnRspQryProduct;

        public event RspQryProduct OnRspQryProduct
        {
            add
            {
                _OnRspQryProduct += value;
                (Invoke(this._handle, "RegOnRspQryProduct", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRspQryProduct));
            }
            remove
            {
                _OnRspQryProduct -= value;
                (Invoke(this._handle, "RegOnRspQryProduct", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRspQryProduct));
            }
        }


        public delegate void RspQryInstrument(InstrumentField pInstrument, bool pLast);

        private RspQryInstrument _OnRspQryInstrument;

        public event RspQryInstrument OnRspQryInstrument
        {
            add
            {
                _OnRspQryInstrument += value;
                (Invoke(this._handle, "RegOnRspQryInstrument", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRspQryInstrument));
            }
            remove
            {
                _OnRspQryInstrument -= value;
                (Invoke(this._handle, "RegOnRspQryInstrument", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRspQryInstrument));
            }
        }

        public delegate void RspQryOrder(OrderField pField, bool pLast);

        private RspQryOrder _OnRspQryOrder;

        public event RspQryOrder OnRspQryOrder
        {
            add
            {
                _OnRspQryOrder += value;
                (Invoke(this._handle, "RegOnRspQryOrder", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRspQryOrder));
            }
            remove
            {
                _OnRspQryOrder -= value;
                (Invoke(this._handle, "RegOnRspQryOrder", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRspQryOrder));
            }
        }

        public delegate void RspQryTrade(TradeData pField, bool pLast);

        private RspQryTrade _OnRspQryTrade;

        public event RspQryTrade OnRspQryTrade
        {
            add
            {
                _OnRspQryTrade += value;
                (Invoke(this._handle, "RegOnRspQryTrade", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRspQryTrade));
            }
            remove
            {
                _OnRspQryTrade -= value;
                (Invoke(this._handle, "RegOnRspQryTrade", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRspQryTrade));
            }
        }

        public delegate void RspQryInvestorPositionDetail(InvestorPositionDetailField pInvestorPositionDetail, RspInfoField pRspInfo, int nRequestID, bool bIsLast);

        private RspQryInvestorPositionDetail _OnRspQryInvestorPositionDetail;

        public event RspQryInvestorPositionDetail OnRspQryInvestorPositionDetail
        {
            add
            {
                _OnRspQryInvestorPositionDetail += value;
                (Invoke(this._handle, "RegOnRspQryInvestorPositionDetail", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRspQryInvestorPositionDetail));
            }
            remove
            {
                _OnRspQryInvestorPositionDetail -= value;
                (Invoke(this._handle, "RegOnRspQryInvestorPositionDetail", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRspQryInvestorPositionDetail));
            }
        }


        public delegate void RspQryPosition(PositionField pPosition, bool pLast);        
       
        public delegate void RspQryTradingAccount(CThostFtdcTradingAccountField pAccount,RspInfoField pRspInfo,bool pIsLast);

        private RspQryTradingAccount _OnRspQryTradingAccount;

        public event RspQryTradingAccount OnRspQryTradingAccount
        {
            add
            {
                _OnRspQryTradingAccount += value;
                (Invoke(this._handle, "RegOnRspQryTradingAccount", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRspQryTradingAccount));
            }
            remove
            {
                _OnRspQryTradingAccount -= value;
                (Invoke(this._handle, "RegOnRspQryTradingAccount", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRspQryTradingAccount));
            }
        }

        public delegate void RspSettlementInfoConfirm(SettlementInfoConfirmField pSettlementInfoConfirm, RspInfoField pRspInfo,
               int nRequestID, bool bIsLast);

        private RspSettlementInfoConfirm _OnRspSettlementInfoConfirm;

        public event RspSettlementInfoConfirm OnRspSettlementInfoConfirm
        {
            add
            {
                _OnRspSettlementInfoConfirm += value;
                (Invoke(this._handle, "RegOnRspSettlementInfoConfirm", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRspSettlementInfoConfirm));
            }
            remove
            {
                _OnRspSettlementInfoConfirm -= value;
                (Invoke(this._handle, "RegOnRspSettlementInfoConfirm", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRspSettlementInfoConfirm));
            }
        }


        public delegate void RtnOrder(OrderField pOrder);

        private RtnOrder _OnRtnOrder;

        public event RtnOrder OnRtnOrder
        {
            add
            {
                _OnRtnOrder += value;
                (Invoke(this._handle, "RegOnRtnOrder", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRtnOrder));
            }
            remove
            {
                _OnRtnOrder -= value;
                (Invoke(this._handle, "RegOnRtnOrder", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRtnOrder));
            }
        }

        private RtnOrder _OnRtnCancel;

        public event RtnOrder OnRtnCancel
        {
            add
            {
                _OnRtnCancel += value;
                (Invoke(this._handle, "RegOnRtnCancel", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRtnCancel));
            }
            remove
            {
                _OnRtnCancel -= value;
                (Invoke(this._handle, "RegOnRtnCancel", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRtnCancel));
            }
        }

        public delegate void RtnTrade(TradeData pTrade);

        private RtnTrade _OnRtnTrade;

        public event RtnTrade OnRtnTrade
        {
            add
            {
                _OnRtnTrade += value;
                (Invoke(this._handle, "RegOnRtnTrade", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRtnTrade));
            }
            remove
            {
                _OnRtnTrade -= value;
                (Invoke(this._handle, "RegOnRtnTrade", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRtnTrade));
            }
        }

        /// <summary>
        /// 撤单回调函数
        /// </summary>
        /// <param name="pInputOrderAction"></param>
        /// <param name="pRspInfo"></param>
        /// <param name="nRequestID"></param>
        /// <param name="bIsLast"></param>
        public delegate void RspOrderAction(InputOrderActionField pInputOrderAction, RspInfoField pRspInfo, int nRequestID, bool bIsLast);

        private RspOrderAction _OnRspOrderAction;

        public event RspOrderAction OnRspOrderAction
        {
            add
            {
                _OnRspOrderAction += value;
                (Invoke(this._handle, "RegOnRspOrderAction", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRtnTrade));
            }
            remove
            {
                _OnRspOrderAction -= value;
                (Invoke(this._handle, "RegOnRspOrderAction", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRtnTrade));
            }
        }

        public delegate void RspQrySettlementInfo(SettlementInfoField pSettlementInfo, RspInfoField pRspInfo,
               int nRequestID, bool bIsLast);

        private RspQrySettlementInfo _OnRspQrySettlementInfo;

        public event RspQrySettlementInfo OnRspQrySettlementInfo
        {
            add
            {
                _OnRspQrySettlementInfo += value;
                Reg reg = (Reg)Invoke(this._handle, "RegOnRspQrySettlementInfo", typeof(Reg));
                IntPtr ii = Marshal.GetFunctionPointerForDelegate(_OnRspQrySettlementInfo);
                reg(ii);
          }
            remove
            {
                _OnRspQrySettlementInfo -= value;
                (Invoke(this._handle, "RegOnRspQrySettlementInfo", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRspQrySettlementInfo));
            }
        }

        #region 银期业务相关 注册响应事件和事件处理函数 Callback
        ///资金账户口令更新请求响应
        public delegate void RspTradingAccountPasswordUpdate(TradingAccountPasswordUpdateField pTradingAccountPasswordUpdate, RspInfoField pRspInfo, int nRequestID, bool bIsLast);
        private RspTradingAccountPasswordUpdate _OnRspTradingAccountPasswordUpdate;

        public event RspTradingAccountPasswordUpdate OnRspTradingAccountPasswordUpdate
        {
            add
            {
                _OnRspTradingAccountPasswordUpdate += value;
                (Invoke(this._handle, "RegOnRspTradingAccountPasswordUpdate", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRspTradingAccountPasswordUpdate));
            }
            remove
            {
                _OnRspTradingAccountPasswordUpdate -= value;
                (Invoke(this._handle, "RegOnRspTradingAccountPasswordUpdate", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRspTradingAccountPasswordUpdate));
            }
        }

        ///请求查询转帐银行响应
        public delegate void RspQryTransferBank(TransferBankField pTransferBank, RspInfoField pRspInfo, int nRequestID, bool bIsLast);

        private RspQryTransferBank _OnRspQryTransferBank;

        public event RspQryTransferBank OnRspQryTransferBank
        {
            add
            {
                _OnRspQryTransferBank += value;
                (Invoke(this._handle, "RegOnRspQryTransferBank", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRspQryTransferBank));
            }
            remove
            {
                _OnRspQryTransferBank -= value;
                (Invoke(this._handle, "RegOnRspQryTransferBank", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRspQryTransferBank));
            }
        }

        ///请求查询转帐流水响应
        public delegate void RspQryTransferSerial(TransferSerialField pTransferSerial, RspInfoField pRspInfo, int nRequestID, bool bIsLast);
        private RspQryTransferSerial _OnRspQryTransferSerial;

        public event RspQryTransferSerial OnRspQryTransferSerial
        {
            add
            {
                _OnRspQryTransferSerial += value;
                (Invoke(this._handle, "RegOnRspQryTransferSerial", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRspQryTransferSerial));
            }
            remove
            {
                _OnRspQryTransferSerial -= value;
                (Invoke(this._handle, "RegOnRspQryTransferSerial", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRspQryTransferSerial));
            }
        }

        ///请求查询银期签约关系响应
        public delegate void RspQryAccountregister(AccountregisterField pAccountregister, RspInfoField pRspInfo, int nRequestID, bool bIsLast);
        private RspQryAccountregister _OnRspQryAccountregister;

        public event RspQryAccountregister OnRspQryAccountregister
        {
            add
            {
                _OnRspQryAccountregister += value;
                (Invoke(this._handle, "RegOnRspQryAccountregister", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRspQryAccountregister));
            }
            remove
            {
                _OnRspQryAccountregister -= value;
                (Invoke(this._handle, "RegOnRspQryAccountregister", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRspQryAccountregister));
            }
        }

        ///请求查询签约银行响应
        public delegate void RspQryContractBank(ContractBankField pContractBank, RspInfoField pRspInfo, int nRequestID, bool bIsLast);
        private RspQryContractBank _OnRspQryContractBank;

        public event RspQryContractBank OnRspQryContractBank
        {
            add
            {
                _OnRspQryContractBank += value;
                (Invoke(this._handle, "RegOnRspQryContractBank", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRspQryContractBank));
            }
            remove
            {
                _OnRspQryContractBank -= value;
                (Invoke(this._handle, "RegOnRspQryContractBank", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRspQryContractBank));
            }
        }
        ///期货发起银行资金转期货通知
        public delegate void RtnFromBankToFutureByFuture(RspTransferField pRspTransfer);
        private RtnFromBankToFutureByFuture _OnRtnFromBankToFutureByFuture;

        public event RtnFromBankToFutureByFuture OnRtnFromBankToFutureByFuture
        {
            add
            {
                _OnRtnFromBankToFutureByFuture += value;
                (Invoke(this._handle, "RegOnRtnFromBankToFutureByFuture", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRtnFromBankToFutureByFuture));
            }
            remove
            {
                _OnRtnFromBankToFutureByFuture -= value;
                (Invoke(this._handle, "RegOnRtnFromBankToFutureByFuture", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRtnFromBankToFutureByFuture));
            }
        }
        ///期货发起期货资金转银行通知
        public delegate void RtnFromFutureToBankByFuture(RspTransferField pRspTransfer);
        private RtnFromFutureToBankByFuture _OnRtnFromFutureToBankByFuture;

        public event RtnFromFutureToBankByFuture OnRtnFromFutureToBankByFuture
        {
            add
            {
                _OnRtnFromFutureToBankByFuture += value;
                (Invoke(this._handle, "RegOnRtnFromFutureToBankByFuture", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRtnFromFutureToBankByFuture));
            }
            remove
            {
                _OnRtnFromFutureToBankByFuture -= value;
                (Invoke(this._handle, "RegOnRtnFromFutureToBankByFuture", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRtnFromFutureToBankByFuture));
            }
        }
        ///期货发起查询银行余额应答
        public delegate void RspQueryBankAccountMoneyByFuture(ReqQueryAccountField pReqQueryAccount, RspInfoField pRspInfo, int nRequestID, bool bIsLast) ;
        private RspQueryBankAccountMoneyByFuture _OnRspQueryBankAccountMoneyByFuture;

        public event RspQueryBankAccountMoneyByFuture OnRspQueryBankAccountMoneyByFuture
        {
            add
            {
                _OnRspQueryBankAccountMoneyByFuture += value;
                (Invoke(this._handle, "RegOnRspQueryBankAccountMoneyByFuture", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRspQueryBankAccountMoneyByFuture));
            }
            remove
            {
                _OnRspQueryBankAccountMoneyByFuture -= value;
                (Invoke(this._handle, "RegOnRspQueryBankAccountMoneyByFuture", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRspQueryBankAccountMoneyByFuture));
            }
        }

        ///银行发起银行资金转期货通知
        public delegate void RtnFromBankToFutureByBank(RspTransferField pRspTransfer);
        private RtnFromBankToFutureByBank _OnRtnFromBankToFutureByBank;

        public event RtnFromBankToFutureByBank OnRtnFromBankToFutureByBank
        {
            add
            {
                _OnRtnFromBankToFutureByBank += value;
                (Invoke(this._handle, "RegOnRtnFromBankToFutureByBank", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRtnFromBankToFutureByBank));
            }
            remove
            {
                _OnRtnFromBankToFutureByBank -= value;
                (Invoke(this._handle, "RegOnRtnFromBankToFutureByBank", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRtnFromBankToFutureByBank));
            }
        }

        ///银行发起期货资金转银行通知
        public delegate void RtnFromFutureToBankByBank(RspTransferField pRspTransfer);
        private RtnFromFutureToBankByBank _OnRtnFromFutureToBankByBank;

        public event RtnFromFutureToBankByBank OnRtnFromFutureToBankByBank
        {
            add
            {
                _OnRtnFromFutureToBankByBank += value;
                (Invoke(this._handle, "RegOnRtnFromFutureToBankByBank", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRtnFromFutureToBankByBank));
            }
            remove
            {
                _OnRtnFromFutureToBankByBank -= value;
                (Invoke(this._handle, "RegOnRtnFromFutureToBankByBank", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRtnFromFutureToBankByBank));
            }
        }

        ///银行发起冲正银行转期货通知
        public delegate void RtnRepealFromBankToFutureByBank(RspRepealField pRspRepeal);
        private RtnRepealFromBankToFutureByBank _OnRtnRepealFromBankToFutureByBank;

        public event RtnRepealFromBankToFutureByBank OnRtnRepealFromBankToFutureByBank
        {
            add
            {
                _OnRtnRepealFromBankToFutureByBank += value;
                (Invoke(this._handle, "RegOnRtnRepealFromBankToFutureByBank", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRtnRepealFromBankToFutureByBank));
            }
            remove
            {
                _OnRtnRepealFromBankToFutureByBank -= value;
                (Invoke(this._handle, "RegOnRtnRepealFromBankToFutureByBank", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRtnRepealFromBankToFutureByBank));
            }
        }

        ///银行发起冲正期货转银行通知
        public delegate void RtnRepealFromFutureToBankByBank(RspRepealField pRspRepeal);
        private RtnRepealFromFutureToBankByBank _OnRtnRepealFromFutureToBankByBank;

        public event RtnRepealFromFutureToBankByBank OnRtnRepealFromFutureToBankByBank
        {
            add
            {
                _OnRtnRepealFromFutureToBankByBank += value;
                (Invoke(this._handle, "RegOnRtnRepealFromFutureToBankByBank", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRtnRepealFromFutureToBankByBank));
            }
            remove
            {
                _OnRtnRepealFromFutureToBankByBank -= value;
                (Invoke(this._handle, "RegOnRtnRepealFromFutureToBankByBank", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRtnRepealFromFutureToBankByBank));
            }
        }

        //系统运行时期货端手工发起冲正银行转期货请求，银行处理完毕后报盘发回的通知
        public delegate void RtnRepealFromBankToFutureByFutureManual(RspRepealField pRspRepeal);
        private RtnRepealFromBankToFutureByFutureManual _OnRtnRepealFromBankToFutureByFutureManual;

        public event RtnRepealFromBankToFutureByFutureManual OnRtnRepealFromBankToFutureByFutureManual
        {
            add
            {
                _OnRtnRepealFromBankToFutureByFutureManual += value;
                (Invoke(this._handle, "RegOnRtnRepealFromBankToFutureByFutureManual", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRtnRepealFromBankToFutureByFutureManual));
            }
            remove
            {
                _OnRtnRepealFromBankToFutureByFutureManual -= value;
                (Invoke(this._handle, "RegOnRtnRepealFromBankToFutureByFutureManual", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRtnRepealFromBankToFutureByFutureManual));
            }
        }

        ///系统运行时期货端手工发起冲正期货转银行请求，银行处理完毕后报盘发回的通知
        public delegate void RtnRepealFromFutureToBankByFutureManual(RspRepealField pRspRepeal);
        private RtnRepealFromFutureToBankByFutureManual _OnRtnRepealFromFutureToBankByFutureManual;

        public event RtnRepealFromFutureToBankByFutureManual OnRtnRepealFromFutureToBankByFutureManual
        {
            add
            {
                _OnRtnRepealFromFutureToBankByFutureManual += value;
                (Invoke(this._handle, "RegOnRtnRepealFromFutureToBankByFutureManual", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRtnRepealFromFutureToBankByFutureManual));
            }
            remove
            {
                _OnRtnRepealFromFutureToBankByFutureManual -= value;
                (Invoke(this._handle, "RegOnRtnRepealFromFutureToBankByFutureManual", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRtnRepealFromFutureToBankByFutureManual));
            }
        }

        ///期货发起查询银行余额通知
        public delegate void RtnQueryBankBalanceByFuture(NotifyQueryAccountField pNotifyQueryAccount);
        private RtnQueryBankBalanceByFuture _OnRtnQueryBankBalanceByFuture;

        public event RtnQueryBankBalanceByFuture OnRtnQueryBankBalanceByFuture
        {
            add
            {
                _OnRtnQueryBankBalanceByFuture += value;
                (Invoke(this._handle, "RegOnRtnQueryBankBalanceByFuture", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRtnQueryBankBalanceByFuture));
            }
            remove
            {
                _OnRtnQueryBankBalanceByFuture -= value;
                (Invoke(this._handle, "RegOnRtnQueryBankBalanceByFuture", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRtnQueryBankBalanceByFuture));
            }
        }

        ///期货发起冲正银行转期货请求，银行处理完毕后报盘发回的通知
        public delegate void RtnRepealFromBankToFutureByFuture(RspRepealField pRspRepeal);
        private RtnRepealFromBankToFutureByFuture _OnRtnRepealFromBankToFutureByFuture;

        public event RtnRepealFromBankToFutureByFuture OnRtnRepealFromBankToFutureByFuture
        {
            add
            {
                _OnRtnRepealFromBankToFutureByFuture += value;
                (Invoke(this._handle, "RegOnRtnRepealFromBankToFutureByFuture", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRtnRepealFromBankToFutureByFuture));
            }
            remove
            {
                _OnRtnRepealFromBankToFutureByFuture -= value;
                (Invoke(this._handle, "RegOnRtnRepealFromBankToFutureByFuture", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRtnRepealFromBankToFutureByFuture));
            }
        }

        ///期货发起冲正期货转银行请求，银行处理完毕后报盘发回的通知
        public delegate void RtnRepealFromFutureToBankByFuture(RspRepealField pRspRepeal);
        private RtnRepealFromFutureToBankByFuture _OnRtnRepealFromFutureToBankByFuture;

        public event RtnRepealFromFutureToBankByFuture OnRtnRepealFromFutureToBankByFuture
        {
            add
            {
                _OnRtnRepealFromFutureToBankByFuture += value;
                (Invoke(this._handle, "RegOnRtnRepealFromFutureToBankByFuture", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRtnRepealFromFutureToBankByFuture));
            }
            remove
            {
                _OnRtnRepealFromFutureToBankByFuture -= value;
                (Invoke(this._handle, "RegOnRtnRepealFromFutureToBankByFuture", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRtnRepealFromFutureToBankByFuture));
            }
        }

        ///期货发起银行资金转期货应答
        public delegate void RspFromBankToFutureByFuture(ReqTransferField pReqTransfer, RspInfoField pRspInfo, int nRequestID, bool bIsLast);
        private RspFromBankToFutureByFuture _OnRspFromBankToFutureByFuture;

        public event RspFromBankToFutureByFuture OnRspFromBankToFutureByFuture
        {
            add
            {
                _OnRspFromBankToFutureByFuture += value;
                (Invoke(this._handle, "RegOnRspFromBankToFutureByFuture", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRspFromBankToFutureByFuture));
            }
            remove
            {
                _OnRspFromBankToFutureByFuture -= value;
                (Invoke(this._handle, "RegOnRspFromBankToFutureByFuture", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRspFromBankToFutureByFuture));
            }
        }

        ///期货发起期货资金转银行应答
        public delegate void RspFromFutureToBankByFuture(ReqTransferField pReqTransfer, RspInfoField pRspInfo, int nRequestID, bool bIsLast);
        private RspFromFutureToBankByFuture _OnRspFromFutureToBankByFuture;

        public event RspFromFutureToBankByFuture OnRspFromFutureToBankByFuture
        {
            add
            {
                _OnRspFromFutureToBankByFuture += value;
                (Invoke(this._handle, "RegOnRspFromFutureToBankByFuture", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRspFromFutureToBankByFuture));
            }
            remove
            {
                _OnRspFromFutureToBankByFuture -= value;
                (Invoke(this._handle, "RegOnRspFromFutureToBankByFuture", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRspFromFutureToBankByFuture));
            }
        }
        ///期货发起银行资金转期货错误回报
        public delegate void ErrRtnBankToFutureByFuture(ReqTransferField pReqTransfer, RspInfoField pRspInfo);
        private ErrRtnBankToFutureByFuture _OnErrRtnBankToFutureByFuture;

        public event ErrRtnBankToFutureByFuture OnErrRtnBankToFutureByFuture
        {
            add
            {
                _OnErrRtnBankToFutureByFuture += value;
                (Invoke(this._handle, "RegOnErrRtnBankToFutureByFuture", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnErrRtnBankToFutureByFuture));
            }
            remove
            {
                _OnErrRtnBankToFutureByFuture -= value;
                (Invoke(this._handle, "RegOnErrRtnBankToFutureByFuture", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnErrRtnBankToFutureByFuture));
            }
        }

        ///期货发起期货资金转银行错误回报
        public delegate void ErrRtnFutureToBankByFuture(ReqTransferField pReqTransfer, RspInfoField pRspInfo);
        private ErrRtnFutureToBankByFuture _OnErrRtnFutureToBankByFuture;

        public event ErrRtnFutureToBankByFuture OnErrRtnFutureToBankByFuture
        {
            add
            {
                _OnErrRtnFutureToBankByFuture += value;
                (Invoke(this._handle, "RegOnErrRtnFutureToBankByFuture", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnErrRtnFutureToBankByFuture));
            }
            remove
            {
                _OnErrRtnFutureToBankByFuture -= value;
                (Invoke(this._handle, "RegOnErrRtnFutureToBankByFuture", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnErrRtnFutureToBankByFuture));
            }
        }

        ///系统运行时期货端手工发起冲正银行转期货错误回报
        public delegate void ErrRtnRepealBankToFutureByFutureManual(ReqRepealField pReqRepeal, RspInfoField pRspInfo);
        private ErrRtnRepealBankToFutureByFutureManual _OnErrRtnRepealBankToFutureByFutureManual;

        public event ErrRtnRepealBankToFutureByFutureManual OnErrRtnRepealBankToFutureByFutureManual
        {
            add
            {
                _OnErrRtnRepealBankToFutureByFutureManual += value;
                (Invoke(this._handle, "RegOnErrRtnRepealBankToFutureByFutureManual", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnErrRtnRepealBankToFutureByFutureManual));
            }
            remove
            {
                _OnErrRtnRepealBankToFutureByFutureManual -= value;
                (Invoke(this._handle, "RegOnErrRtnRepealBankToFutureByFutureManual", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnErrRtnRepealBankToFutureByFutureManual));
            }
        }

        ///系统运行时期货端手工发起冲正期货转银行错误回报
        public delegate void ErrRtnRepealFutureToBankByFutureManual(ReqRepealField pReqRepeal, RspInfoField pRspInfo);
        private ErrRtnRepealFutureToBankByFutureManual _OnErrRtnRepealFutureToBankByFutureManual;

        public event ErrRtnRepealFutureToBankByFutureManual OnErrRtnRepealFutureToBankByFutureManual
        {
            add
            {
                _OnErrRtnRepealFutureToBankByFutureManual += value;
                (Invoke(this._handle, "RegOnErrRtnRepealFutureToBankByFutureManual", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnErrRtnRepealFutureToBankByFutureManual));
            }
            remove
            {
                _OnErrRtnRepealFutureToBankByFutureManual -= value;
                (Invoke(this._handle, "RegOnErrRtnRepealFutureToBankByFutureManual", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnErrRtnRepealFutureToBankByFutureManual));
            }
        }

        ///期货发起查询银行余额错误回报
        public delegate void ErrRtnQueryBankBalanceByFuture(ReqQueryAccountField pReqQueryAccount, RspInfoField pRspInfo);
        private ErrRtnQueryBankBalanceByFuture _OnErrRtnQueryBankBalanceByFuture;

        public event ErrRtnQueryBankBalanceByFuture OnErrRtnQueryBankBalanceByFuture
        {
            add
            {
                _OnErrRtnQueryBankBalanceByFuture += value;
                (Invoke(this._handle, "RegOnErrRtnQueryBankBalanceByFuture", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnErrRtnQueryBankBalanceByFuture));
            }
            remove
            {
                _OnErrRtnQueryBankBalanceByFuture -= value;
                (Invoke(this._handle, "RegOnErrRtnQueryBankBalanceByFuture", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnErrRtnQueryBankBalanceByFuture));
            }
        }

        #endregion

        #region 预埋/预埋单 注册响应事件和事件处理函数 Callback
        ///预埋单录入请求响应
        public delegate void RspParkedOrderInsert(ParkedOrderField pParkedOrder, RspInfoField pRspInfo, int nRequestID, bool bIsLast);
        private RspParkedOrderInsert _OnRspParkedOrderInsert;

        public event RspParkedOrderInsert OnRspParkedOrderInsert
        {
            add
            {
                _OnRspParkedOrderInsert += value;
                (Invoke(this._handle, "RegOnRspParkedOrderInsert", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRspParkedOrderInsert));
            }
            remove
            {
                _OnRspParkedOrderInsert -= value;
                (Invoke(this._handle, "RegOnRspParkedOrderInsert", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRspParkedOrderInsert));
            }
        }

        ///预埋撤单录入请求响应
         public delegate void RspParkedOrderAction(ParkedOrderActionField pParkedOrderAction, RspInfoField pRspInfo, int nRequestID, bool bIsLast);
        private RspParkedOrderAction _OnRspParkedOrderAction;

        public event RspParkedOrderAction OnRspParkedOrderAction
        {
            add
            {
                _OnRspParkedOrderAction += value;
                (Invoke(this._handle, "RegOnRspParkedOrderAction", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRspParkedOrderAction));
            }
            remove
            {
                _OnRspParkedOrderAction -= value;
                (Invoke(this._handle, "RegOnRspParkedOrderAction", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRspParkedOrderAction));
            }
        }

        ///删除预埋单响应
        public delegate void RspRemoveParkedOrder(RemoveParkedOrderField pRemoveParkedOrder, RspInfoField pRspInfo, int nRequestID, bool bIsLast);
        private RspRemoveParkedOrder _OnRspRemoveParkedOrder;

        public event RspRemoveParkedOrder OnRspRemoveParkedOrder
        {
            add
            {
                _OnRspRemoveParkedOrder += value;
                (Invoke(this._handle, "RegOnRspRemoveParkedOrder", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRspRemoveParkedOrder));
            }
            remove
            {
                _OnRspRemoveParkedOrder -= value;
                (Invoke(this._handle, "RegOnRspRemoveParkedOrder", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRspRemoveParkedOrder));
            }
        }


        ///删除预埋撤单响应
        public delegate void RspRemoveParkedOrderAction(RemoveParkedOrderActionField pRemoveParkedOrderAction, RspInfoField pRspInfo, int nRequestID, bool bIsLast);
        private RspRemoveParkedOrderAction _OnRspRemoveParkedOrderAction;

        public event RspRemoveParkedOrderAction OnRspRemoveParkedOrderAction
        {
            add
            {
                _OnRspRemoveParkedOrderAction += value;
                (Invoke(this._handle, "RegOnRspRemoveParkedOrderAction", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRspRemoveParkedOrderAction));
            }
            remove
            {
                _OnRspRemoveParkedOrderAction -= value;
                (Invoke(this._handle, "RegOnRspRemoveParkedOrderAction", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRspRemoveParkedOrderAction));
            }
        }

        ///请求查询预埋单响应
        public delegate void RspQryParkedOrder(ParkedOrderField pParkedOrder, RspInfoField pRspInfo, int nRequestID, bool bIsLast);
        private RspQryParkedOrder _OnRspQryParkedOrder;

        public event RspQryParkedOrder OnRspQryParkedOrder
        {
            add
            {
                _OnRspQryParkedOrder += value;
                (Invoke(this._handle, "RegOnRspQryParkedOrder", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRspQryParkedOrder));
            }
            remove
            {
                _OnRspQryParkedOrder -= value;
                (Invoke(this._handle, "RegOnRspQryParkedOrder", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRspQryParkedOrder));
            }
        }

        ///请求查询预埋撤单响应
        public delegate void RspQryParkedOrderAction(ReqTransferField pReqTransfer, RspInfoField pRspInfo, int nRequestID, bool bIsLast);
        private RspQryParkedOrderAction _OnRspQryParkedOrderAction;

        public event RspQryParkedOrderAction OnRspQryParkedOrderAction
        {
            add
            {
                _OnRspQryParkedOrderAction += value;
                (Invoke(this._handle, "RegOnRspQryParkedOrderAction", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRspQryParkedOrderAction));
            }
            remove
            {
                _OnRspQryParkedOrderAction -= value;
                (Invoke(this._handle, "RegOnRspQryParkedOrderAction", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRspQryParkedOrderAction));
            }
        }
        #endregion

        #region 查询通知（期商通知、交易通知） 注册响应事件和事件处理函数

        ///请求查询交易通知响应
        public delegate void RtnNotice(TradingNoticeInfoField pTradingNoticeInfo);

        private RtnNotice _OnRtnNotice;

        public event RtnNotice OnRtnNotice
        {
            add
            {
                _OnRtnNotice += value;
                (Invoke(this._handle, "RegOnRtnTradingNotice", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRtnNotice));
            }
            remove
            {
                _OnRtnNotice -= value;
                (Invoke(this._handle, "RegOnRtnTradingNotice", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRtnNotice));
            }
        }

        ///请求查询客户通知响应
        public delegate void RspQryNotice(NoticeField pNotice, RspInfoField pRspInfo,int nRequestID, bool bIsLast);
        private RspQryNotice _OnRspQryNotice;

        public event RspQryNotice OnRspQryNotice
        {
            add
            {
                _OnRspQryNotice += value;
                (Invoke(this._handle, "RegOnRspQryNotice", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRspQryNotice));
            }
            remove
            {
                _OnRspQryNotice -= value;
                (Invoke(this._handle, "RegOnRspQryNotice", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRspQryNotice));
            }
        }

        #endregion

        public delegate void RspQryInvestor(InvestorField pInvestor, RspInfoField pRspInfo,int nRequestID, bool bIsLast);

        private RspQryInvestor _OnRspQryInvestor;

        public event RspQryInvestor OnRspQryInvestor
        {
            add
            {
                _OnRspQryInvestor += value;
                Reg reg = (Reg)Invoke(this._handle, "RegOnRspQryInvestor", typeof(Reg));
                IntPtr ii = Marshal.GetFunctionPointerForDelegate(_OnRspQryInvestor);
                reg(ii);
            }
            remove
            {
                _OnRspQryInvestor -= value;
                (Invoke(this._handle, "RegOnRspQryInvestor", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRspQryInvestor));
            }
        }
        public delegate void RspQryBrokerTradingParams(BrokerTradingParamsField pBrokerTradingParams, RspInfoField pRspInfo, int nRequestID, bool bIsLast);

        private RspQryBrokerTradingParams _OnRspQryBrokerTradingParams;

        public event RspQryBrokerTradingParams OnRspQryBrokerTradingParams
        {
            add
            {
                _OnRspQryBrokerTradingParams += value;
                Reg reg = (Reg)Invoke(this._handle, "RegOnRspQryBrokerTradingParams", typeof(Reg));
                IntPtr ii = Marshal.GetFunctionPointerForDelegate(_OnRspQryBrokerTradingParams);
                reg(ii);
            }
            remove
            {
                _OnRspQryBrokerTradingParams -= value;
                (Invoke(this._handle, "RegOnRspQryBrokerTradingParams", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRspQryBrokerTradingParams));
            }
        }

        public delegate void RspQryBrokerTradingAlgos(BrokerTradingAlgosField pBrokerTradingAlgos, RspInfoField pRspInfo, int nRequestID, bool bIsLast);

        private RspQryBrokerTradingAlgos _OnRspQryBrokerTradingAlgos;

        public event RspQryBrokerTradingAlgos OnRspQryBrokerTradingAlgos
        {
            add
            {
                _OnRspQryBrokerTradingAlgos += value;
                Reg reg = (Reg)Invoke(this._handle, "RegOnRspQryBrokerTradingAlgos", typeof(Reg));
                IntPtr ii = Marshal.GetFunctionPointerForDelegate(_OnRspQryBrokerTradingAlgos);
                reg(ii);
            }
            remove
            {
                _OnRspQryBrokerTradingAlgos -= value;
                (Invoke(this._handle, "RegOnRspQryBrokerTradingAlgos", typeof(Reg)) as Reg)(Marshal.GetFunctionPointerForDelegate(_OnRspQryBrokerTradingAlgos));
            }
        }

        #endregion

        public int ReqConnect(string flowPath,string[] pFronts, int nCount)
        {
            ((RQCreateApi)Invoke(this._handle, "CreateApi", typeof(RQCreateApi)))(flowPath);
            return ((RQReqConnect)Invoke(this._handle, "ReqConnect", typeof(RQReqConnect)))(pFronts, nCount);
        }

        public int ReqUserLogin(string pInvestor, string pPwd, string pBroker)
        {
            return ((RQReqUserLogin)Invoke(this._handle, "ReqUserLogin", typeof(RQReqUserLogin)))(pInvestor, pPwd, pBroker);
        }


        public int ReqUserPasswordUpdate(UserPasswordUpdateField pUserPasswordUpdate)
        {
            return ((RQReqUserPasswordUpdate)Invoke(this._handle, "ReqUserPasswordUpdate", typeof(RQReqUserPasswordUpdate)))(pUserPasswordUpdate);
        }

        public IntPtr GetTradingDay()
        {
            return ((RQGetTradingDay)Invoke(this._handle, "GetTradingDay", typeof(RQGetTradingDay)))();
        }

        public int ReqQryOrder()
        {
            return ((RQReqQryOrder)Invoke(this._handle, "ReqQryOrder", typeof(RQReqQryOrder)))();
        }
        /// <summary>
        /// 请求查询合约保证金率
        /// </summary>
        /// <param name="pQryInstrumentMarginRate"></param>
        /// <returns></returns>
        public int ReqQryInstrumentMarginRate(QryInstrumentMarginRateField pQryInstrumentMarginRate)
        {
            return ((RQReqQryInstrumentMarginRate)Invoke(this._handle, "ReqQryInstrumentMarginRate", typeof(RQReqQryInstrumentMarginRate)))(pQryInstrumentMarginRate);
        }

        /// <summary>
        /// 请求查询合约手续费率
        /// </summary>
        /// <param name="pQryInstrumentCommissionRate"></param>
        /// <returns></returns>
        public int ReqQryInstrumentCommissionRate(QryInstrumentCommissionRateField pQryInstrumentCommissionRate)
        {
            return ((RQReqQryInstrumentCommissionRate)Invoke(this._handle, "ReqQryInstrumentCommissionRate", typeof(RQReqQryInstrumentCommissionRate)))(pQryInstrumentCommissionRate);
        }

        /// <summary>
        /// 查询品种信息
        /// </summary>
        /// <param name="pInstrumentField"></param>
        /// <returns></returns>
        public int ReqQryProduct(QryProductField pInstrumentField)
        {
            return ((RQReqQryProduct)Invoke(this._handle, "ReqQryProduct", typeof(RQReqQryProduct)))(pInstrumentField);
        }
        public int ReqQryInstrument(QryInstrumentField pInstrumentField)
        {
            return ((RQReqQryInstrument)Invoke(this._handle, "ReqQryInstrument", typeof(RQReqQryInstrument)))(pInstrumentField);
        }
        public int ReqQryTrade()
        {
            return ((RQReqQryTrade)Invoke(this._handle, "ReqQryTrade", typeof(RQReqQryTrade)))();
        }

        public int ReqQryPosition()
        {
            return ((RQReqQryPosition)Invoke(this._handle, "ReqQryPosition", typeof(RQReqQryPosition)))();
        }

        public int ReqQryInvestorPositionDetail(QryInvestorPositionDetailField pQryInvestorPosiDetail)
        {
            return ((RQReqQryInvestorPositionDetail)Invoke(_handle, "ReqQryInvestorPositionDetail", typeof(RQReqQryInvestorPositionDetail)))(pQryInvestorPosiDetail);
        }
        public int ReqQryAccount()
        {
            return ((RQReqQryAccount)Invoke(this._handle, "ReqQryAccount", typeof(RQReqQryAccount)))();
        }

        public int ReqSettlementInfoConfirm()
        {
            int i = ((RQReqSettlementInfoConfirm)Invoke(this._handle, "ReqSettlementInfoConfirm", typeof(RQReqSettlementInfoConfirm)))();
            return i;
        }
        public int ReqOrderInsert(InputOrderField inputOrder)
        {
            ((RQReqOrderInsert)Invoke(_handle, "ReqOrderInsert", typeof(RQReqOrderInsert)))(inputOrder);
            return 1;
        }

        public int ReqOrderAction(InputOrderActionField inputOrderAction)
        {
            return ((RQReqOrderAction)Invoke(this._handle, "ReqOrderAction", typeof(RQReqOrderAction)))(inputOrderAction);
        }

        public int ReqQrySettlementInfo(QrySettlementInfoField field)
        {
            return ((RQReqQrySettlementInfo)Invoke(this._handle, "ReqQrySettlementInfo",
                typeof(RQReqQrySettlementInfo)))(field);
        }

        /// <summary>
        /// 资金账户口令更新请求
        /// </summary>
        /// <param name="pTradingAccountPasswordUpdate"></param>
        /// <returns></returns>
        public int ReqTradingAccountPasswordUpdate(TradingAccountPasswordUpdateField pTradingAccountPasswordUpdate)
        {
            return ((RQReqTradingAccountPasswordUpdate)Invoke(this._handle, "ReqTradingAccountPasswordUpdate",
               typeof(RQReqTradingAccountPasswordUpdate)))(pTradingAccountPasswordUpdate);
        }

        /// <summary>
        /// 请求查询转帐银行
        /// </summary>
        /// <param name="pQryTransferBank"></param>
        /// <returns></returns>
        public int ReqQryTransferBank(QryTransferBankField pQryTransferBank)
        {
            return ((RQReqQryTransferBank)Invoke(this._handle, "ReqQryTransferBank",
               typeof(RQReqQryTransferBank)))(pQryTransferBank);
        }

        /// <summary>
        /// 请求查询转帐流水
        /// </summary>
        /// <param name="pQryTransferSerial"></param>
        /// <returns></returns>
        public int ReqQryTransferSerial(QryTransferSerialField pQryTransferSerial)
        {
            return ((RQReqQryTransferSerial)Invoke(this._handle, "ReqQryTransferSerial",
               typeof(RQReqQryTransferSerial)))(pQryTransferSerial);
        }

        /// <summary>
        /// 请求查询银期签约关系
        /// </summary>
        /// <param name="pQryAccountregister"></param>
        /// <returns></returns>
        public int ReqQryAccountregister(QryAccountregisterField pQryAccountregister)
        {
            return ((RQReqQryAccountregister)Invoke(this._handle, "ReqQryAccountregister",
               typeof(RQReqQryAccountregister)))(pQryAccountregister);
        }

        /// <summary>
        /// 请求查询签约银行
        /// </summary>
        /// <param name="pQryContractBank"></param>
        /// <returns></returns>
        public int ReqQryContractBank(QryContractBankField pQryContractBank)
        {
            return ((RQReqQryContractBank)Invoke(this._handle, "ReqQryContractBank",
               typeof(RQReqQryContractBank)))(pQryContractBank);
        }

        /// <summary>
        /// 期货发起银行资金转期货请求
        /// </summary>
        /// <param name="pReqTransfer"></param>
        /// <returns></returns>
        public int ReqFromBankToFutureByFuture(ReqTransferField pReqTransfer)
        {
            return ((RQReqFromBankToFutureByFuture)Invoke(this._handle, "ReqFromBankToFutureByFuture",
               typeof(RQReqFromBankToFutureByFuture)))(pReqTransfer);
        }

        /// <summary>
        /// 期货发起期货资金转银行请求
        /// </summary>
        /// <param name="pReqTransfer"></param>
        /// <returns></returns>
        public int ReqFromFutureToBankByFuture(ReqTransferField pReqTransfer)
        {
            return ((RQReqFromFutureToBankByFuture)Invoke(this._handle, "ReqFromFutureToBankByFuture",
               typeof(RQReqFromFutureToBankByFuture)))(pReqTransfer);
        }

        /// <summary>
        /// 期货发起查询银行余额请求
        /// </summary>
        /// <param name="pReqQueryAccount"></param>
        /// <returns></returns>
        public int ReqQueryBankAccountMoneyByFuture(ReqQueryAccountField pReqQueryAccount)
        {
            return ((RQReqQueryBankAccountMoneyByFuture)Invoke(this._handle, "ReqQueryBankAccountMoneyByFuture",
               typeof(RQReqQueryBankAccountMoneyByFuture)))(pReqQueryAccount);
        }

        /// <summary>
        /// 条件单下单请求
        /// </summary>
        /// <param name="pFiled"></param>
        /// <returns></returns>
        public int ReqCondOrderInsert(InputOrderField pFiled, PricingMode orderType, string pOrderRef)
        {
            return ((RQReqCondOrderInsert)Invoke(this._handle, "ReqCondOrderInsert", typeof(RQReqCondOrderInsert)))(pFiled,orderType,  pOrderRef);
        }

        ///预埋单录入请求
        public int ReqParkedOrderInsert(ParkedOrderField pParkedOrder, PricingMode pType, string pOrderRef)
        {
            return ((RQReqParkedOrderInsert)Invoke(this._handle, "ReqParkedOrderInsert",
                         typeof(RQReqParkedOrderInsert)))(pParkedOrder,pType,pOrderRef);
        }

        ///预埋撤单录入请求
        public int ReqParkedOrderAction(ParkedOrderActionField pParkedOrderAction)
        {
            return ((RQReqParkedOrderAction)Invoke(this._handle, "ReqParkedOrderAction",
                typeof(RQReqParkedOrderAction)))(pParkedOrderAction);
        }

        //请求删除预埋单
        public int ReqRemoveParkedOrder(RemoveParkedOrderField pRemoveParkedOrder)
        {
            return ((RQReqRemoveParkedOrder)Invoke(this._handle, "ReqRemoveParkedOrder",
                typeof(RQReqRemoveParkedOrder)))(pRemoveParkedOrder);
        }

        ///请求删除预埋撤单
        public int ReqRemoveParkedOrderAction(RemoveParkedOrderActionField pRemoveParkedOrderAction)
        {
            return ((RQReqRemoveParkedOrderAction)Invoke(this._handle, "ReqRemoveParkedOrderAction",
                typeof(RQReqRemoveParkedOrderAction)))(pRemoveParkedOrderAction);
        }

        ///请求查询预埋单
        public int ReqQryParkedOrder(QryParkedOrderField pQryParkedOrder)
        {
            return ((RQReqQryParkedOrder)Invoke(this._handle, "ReqQryParkedOrder",
                typeof(RQReqQryParkedOrder)))(pQryParkedOrder);
        }

        ///请求查询预埋撤单
        public int ReqQryParkedOrderAction(QryParkedOrderActionField pQryParkedOrderAction)
        {
            return ((RQReqQryParkedOrderAction)Invoke(this._handle, "ReqQryParkedOrderAction",
                typeof(RQReqQryParkedOrderAction)))(pQryParkedOrderAction);
        }
        #region 查询通知（期商通知、交易通知）
        //请求查询交易通知
        public int ReqQryTradingNotice(QryTradingNoticeField pQryTradingNotice)
        {
            return ((RQReqQryTradingNotice)Invoke(this._handle, "ReqQryTradingNotice",
                        typeof(RQReqQryTradingNotice)))(pQryTradingNotice);
        }

        ///请求查询客户通知
        public int ReqQryNotice()
        {
            return ((RQReqQryNotice)Invoke(this._handle, "ReqQryNotice",
                           typeof(RQReqQryNotice)))();
        }

        ///请求查询投资者响应
        public int ReqQryInvestor()
        {
            return ((RQReqQryInvestor)Invoke(this._handle, "ReqQryInvestor",
                        typeof(RQReqQryInvestor)))();
        }
        ///请求查询经纪公司交易参数
        public int ReqQryBrokerTradingParams(QryBrokerTradingParamsField pQryBrokerTradingParams)
        {
            return ((RQReqQryBrokerTradingParams)Invoke(this._handle, "ReqQryBrokerTradingParams",
                  typeof(RQReqQryBrokerTradingParams)))(pQryBrokerTradingParams);
        }

        //请求查询经纪公司交易参数
        public int ReqQryBrokerTradingAlgos(QryBrokerTradingAlgosField pQryBrokerTradingAlgos)
        {
            return ((RQReqQryBrokerTradingAlgos)Invoke(this._handle, "ReqQryBrokerTradingAlgos",
                 typeof(RQReqQryBrokerTradingAlgos)))(pQryBrokerTradingAlgos);
        }
        #endregion
    }
}
