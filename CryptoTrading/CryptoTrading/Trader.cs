using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryptoTrading.Model;
using CryptoTrading.TQLib;

namespace CryptoTrading
{
    public static class Trader
    {
        #region static public fields
        public static UserConfiguration Configuration;

        public static ExtensionConfiguration ExtConfig;
        public static Dictionary<string, DefaultQuantSet> DefaultInstrumentQuant;
        /// <summary>
        /// 当前使用自选合约
        /// </summary>

        public static  Dictionary<string, InstrumentData> CurrInstrumentDict = new Dictionary<string, InstrumentData>();
        public static string defaultOrderOption;
        #endregion

        ///// <summary>
        ///// 交易账号
        ///// </summary>
        //public static string InvestorID { get; set; }

        /// <summary>
        /// 用户配置文件
        /// </summary>
        public static string CfgFile
        {
            get; set;
        }

        /// <summary>
        /// 用户扩展配置文件
        /// </summary>
        public static string ExtCfgFile
        {
            get; set;
        }
        /// <summary>
        /// 历史持仓记录路径
        /// </summary>
        public static string HistoryPosiPath { get; set; }     

        /// <summary>
        /// 默认期商代码
        /// </summary>
        public static string DefaultBrokerID { get; set; }

        /// <summary>
        /// 默认网络线路
        /// </summary>
        public static string DefaultServer { get; set; }

        /// <summary>
        /// 行情服务器列表
        /// </summary>
        public static List<string> MarketServer { get; set; }

        /// <summary>
        /// 交易服务器列表
        /// </summary>
        public static List<string> TradingServer { get; set; }

        /// <summary>
        /// 默认期商
        /// </summary>
        public static string DefaultBroker { get; set; }

        private static int orderNo;
        public static int OrderNo { get { return orderNo++; } set { orderNo = value; } }

        /// <summary>
        /// 自定义合约报单号（前4位）
        /// </summary>
        private static int custProductOrderNo = 1;
        public static int CustProductOrderNo { get { return custProductOrderNo++; } set { custProductOrderNo = value; } }

        /// <summary>
        /// 是否是仿真帐号
        /// </summary>
        public static bool IsSimAccount { get; set; }

        /// <summary>
        /// 默认自选合约组
        /// </summary>
        //public static string DefaultInstrumentIDGroup { get; set; }
        public static string ActionDay { get; set; }
        public static string TradingDay { get; set; }
        private static LoginMode _LoginMode;

        public static LoginMode LoginMode
        {
            get
            {
                try
                {
                    switch (ConfigurationManager.AppSettings["LoginMode"].ToLower())
                    {
                        case "debugoffline":
                            _LoginMode = LoginMode.DebugOffline;
                            break;
                        case "predebugoffline":
                            _LoginMode = LoginMode.PreDebugOffline;
                            break;
                        default:
                            _LoginMode = LoginMode.Normal;
                            break;
                    }
                }
                catch
                {
                    _LoginMode = LoginMode.Normal;
                }
                return _LoginMode;
            }
            set { _LoginMode = value; }
        }

        public static UserConfiguration Load(string userID)
        {
            string cfgFile= Environment.CurrentDirectory +"\\Configurations\\Configuration.xml";
            if (File.Exists(cfgFile))
            {
                CfgFile = cfgFile;
                ExtCfgFile = Environment.CurrentDirectory + "\\Configurations\\Extensions.xml";
                InstrumentInfoCfgFile =  Environment.CurrentDirectory+ "\\Configurations\\InstrumentInfo.xml";
                HistoryPosiPath = Environment.CurrentDirectory + "\\"+userID+"\\History/";
                UserConfiguration v;
                try
                {
                    v = TQXmlHelper.XmlDeserializeFromFile<UserConfiguration>(CfgFile, Encoding.UTF8);                    
                }
                catch (Exception e)
                {
                    Utility.WriteMemFile(e.Message);
                    v = new UserConfiguration();
                }
                return v;
            }
            else
            {
                return new UserConfiguration();
            }
        }

        /// <summary>
        /// 加载用户扩展配置
        /// </summary>
        public static bool LoadExtConfig()
        {
            if (File.Exists(ExtCfgFile))
            {
                ExtConfig = TQXmlHelper.XmlDeserializeFromFile<ExtensionConfiguration>(ExtCfgFile, Encoding.UTF8);
                if (ExtConfig.Combos == null) return false;
                else
                {
                    CustProductOrderNo = ExtConfig.ConditionalOrderNo;
                    return true;
                }
            }
            else
            {
                ExtConfig = new ExtensionConfiguration();
                return true;
            }
        }
        public static void InitUserDirectoryAndConfiguration(string investorId)
        {
            string dirStr = investorId;
            if (!Directory.Exists(dirStr))
            {
                dirStr = investorId + "/MarketData";//行情流
                Directory.CreateDirectory(dirStr);
                dirStr = investorId + "/Trade";//交易流
                Directory.CreateDirectory(dirStr);
                dirStr = investorId + "/TradeLog";//交易记录
                Directory.CreateDirectory(dirStr);
                dirStr = investorId + "/Configs";//账户配置
                Directory.CreateDirectory(dirStr);
                //移初始配置文件
                File.Copy("include/InitConfiguration.xml", dirStr + "/" + "Configuration.xml");
                //配置初始策略模板文件
                File.Copy("include/InitStrategyLib.xml", dirStr + "/" + "StrategyLib.xml");
                dirStr = investorId + "/History";//历史数据
                Directory.CreateDirectory(dirStr);
            }
            else
            {
                dirStr = investorId + "/MarketData";//行情流
                if (!Directory.Exists(dirStr))
                {
                    Directory.CreateDirectory(dirStr);
                }
                dirStr = investorId + "/Trade";//交易流
                if (!Directory.Exists(dirStr))
                {
                    Directory.CreateDirectory(dirStr);
                }
                dirStr = investorId + "/TradeLog";//交易记录
                if (!Directory.Exists(dirStr))
                {
                    Directory.CreateDirectory(dirStr);
                }
                dirStr = investorId + "/Settlements";//结算单
                if (!Directory.Exists(dirStr))
                {
                    Directory.CreateDirectory(dirStr);
                }
                dirStr = investorId + "/Configs";//账户配置
                if (!Directory.Exists(dirStr))
                {
                    Directory.CreateDirectory(dirStr);
                }
                //移初始配置文件
                if (File.Exists("include/InitConfiguration.xml"))
                {
                    if (!File.Exists(dirStr + "/" + "Configuration.xml"))
                    {
                        File.Copy("include/InitConfiguration.xml", dirStr + "/" + "Configuration.xml");
                    }
                }
                //配置初始策略模板文件
                if (File.Exists("include/InitStrategyLib.xml"))
                {
                    if (!File.Exists(dirStr + "/" + "StrategyLib.xml"))
                    {
                        File.Copy("include/InitStrategyLib.xml", dirStr + "/" + "StrategyLib.xml");
                    }
                }
                dirStr = investorId + "/History";//历史数据
                if (!Directory.Exists(dirStr))
                {
                    Directory.CreateDirectory(dirStr);
                }
            }
        }
        /// <summary>
        /// 用户合约配置文件
        /// </summary>
        public static string InstrumentInfoCfgFile { get; set; }

    }
}
