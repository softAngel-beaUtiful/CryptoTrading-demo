using System.Collections.Generic;
using System.Xml.Serialization;
using System.Xml;
using System.ComponentModel;
using System.Text;
using System.Linq;
using System.Windows.Input;
using System;
using CryptoTrading.TQLib;
using CryptoTrading.Model;

namespace CryptoTrading
{

    /// <summary>
    /// 用户配置
    /// 用于保存至用户配置的XML文件
    /// </summary>
    [XmlRoot("Config")]
    public class UserConfiguration: ObservableObject
    {
        public UserConfiguration()
        {
            AccountDataGrid = new List<ColumnSettingItem>();
            InstrumentDataGrid = new List<ColumnSettingItem>();
            MarketDataGrid = new List<ColumnSettingItem>();
            TodayOrderGrid = new List<ColumnSettingItem>();
            SettledOrdersGrid = new List<ColumnSettingItem>();
            ComplexOrderGrid = new List<ColumnSettingItem>();
            UnsettledOrdersGrid = new List<ColumnSettingItem>();
            PositionDetailsGrid = new List<ColumnSettingItem>();
            PositionSummaryGrid = new List<ColumnSettingItem>();
            ComboPositionGrid = new List<ColumnSettingItem>();
            TradeDetailsGrid = new List<ColumnSettingItem>();
            TradeSummaryGrid = new List<ColumnSettingItem>();
            TransferSerialDataGrid = new List<ColumnSettingItem>();
            AccountregisterDataGrid = new List<ColumnSettingItem>();
            CanceledOrdersGrid = new List<ColumnSettingItem>();
            ManualPositionGrid = new List<ColumnSettingItem>();
            StrategyPositionGrid = new List<ColumnSettingItem>();
            Trader.TradingDay = DateTime.Today.ToString("yyyyMMdd");
        }
        #region 属性
        /// <summary>
        /// 投资者信息
        /// </summary>
        [XmlElement( Order = 1)]
        public ExchangeAccountInfo Investor { get; set; }
        /// <summary>
        /// 邮箱配置
        /// </summary>
        [XmlElement(Order = 2)]
        public MailConfig Mail { get; set; }

        /// <summary>
        /// 默认手数（对于没有设置默认手数的合约）
        /// </summary>
        [XmlElement(Order = 3)]
        public int DefaultQuant { get; set; }
        /// <summary>
        /// 默认下单操作
        /// </summary>
        [XmlElement(Order = 4)]
        public int DefaultOrderOption { get; set; }
        /// <summary>
        /// 默认UI风格
        /// </summary>
        [XmlElement(Order = 5)]
        public string DefaultUILayout { get; set; }
        /// <summary>
        /// 默认合约组
        /// </summary>
        [XmlElement(Order = 6)]
        public string DefaultInstrumentIDGroup { get; set; }
        /// <summary>
        /// 下单是否需要确认
        /// </summary>
        [XmlElement(Order = 7)]
        public bool ConfirmOrder { get; set; }
        /// <summary>
        /// 交易日
        /// </summary>
        [XmlElement(Order = 8)]
        public string TradingDay
        {
            get { return Trader.TradingDay; }
            set { Trader.TradingDay = value; }
        }
        /// <summary>
        /// 请求编号
        /// </summary>
        [XmlElement(Order = 9)]
        public int RequestID { get; set; }
        /// <summary>
        /// 涨跌(幅)计算算法:是否使用昨结价计算
        /// </summary>
        [XmlElement(Order = 10)]
        public bool UsePreSettlementPrice { get; set; }

        /// <summary>
        /// 资金账号表格列设置
        /// </summary>
        //[XmlElement("AccountDataGridColumns", Order = 11)]
        [XmlArrayItem("item")]
        [XmlArray("AccountDataGridColumns", Order = 11)]
        public List<ColumnSettingItem> AccountDataGrid { get; set; }


        /// <summary>
        /// 合约信息表格列设置
        /// </summary>
        //[XmlElement("InstrumentDataGridColumns", Order = 12)]
        [XmlArrayItem("item")]
        [XmlArray("InstrumentDataGridColumns", Order = 12)]
        public List<ColumnSettingItem> InstrumentDataGrid { get; set; }


        /// <summary>
        /// 行情表格列设置
        /// </summary>
        [XmlArrayItem("item")]
        [XmlArray("MarketDataGridColumns", Order = 13)]
        //[XmlElement("", Order = 13)]
        public List<ColumnSettingItem> MarketDataGrid { get; set; }

        /// <summary>
        /// 当日委托表格列设置
        /// </summary>
        //[XmlElement("TodayOrdersGridColumns", Order = 14)]
        [XmlArrayItem("item")]
        [XmlArray("TodayOrdersGridColumns", Order = 14)]
        public List<ColumnSettingItem> TodayOrderGrid { get; set; }
        /// <summary>
        /// 成交单表格列设置
        /// </summary>
        //[XmlElement("SettledOrdersGridColumns", Order = 15)]
        [XmlArrayItem("item")]
        [XmlArray("SettledOrdersGridColumns", Order = 15)]
        public List<ColumnSettingItem> SettledOrdersGrid { get; set; }

        /// <summary>
        /// 预埋条件委托表格列设置
        /// </summary>
        //[XmlElement("ComplexOrderGridColumns",Order = 16)]
        [XmlArrayItem("item")]
        [XmlArray("ComplexOrdersGridColumns", Order = 16)]
        public List<ColumnSettingItem> ComplexOrderGrid { get; set; }
        /// <summary>
        /// 未成交单表格列设置
        /// </summary>
       // [XmlElement("UnsettledOrdersGridColumns", Order = 17)]
        [XmlArrayItem("item")]
        [XmlArray("UnsettledOrdersGridColumns", Order = 17)]
        public List<ColumnSettingItem> UnsettledOrdersGrid { get; set; }
        /// <summary>
        /// 撤销单表格列设置
        /// </summary>        
        [XmlArrayItem("item")]
        [XmlArray("CanceledOrdersGridColumns", Order = 18)]
        public List<ColumnSettingItem> CanceledOrdersGrid { get; set; }

        /// <summary>
        /// 手工交易持仓表格列设置
        /// </summary>
        [XmlArrayItem("item")]
        [XmlArray("ManualPositionGridColumns", Order = 19)]
        public List<ColumnSettingItem> ManualPositionGrid { get; set; }

        /// <summary>
        /// 策略持仓表格列设置
        /// </summary>
        [XmlArrayItem("item")]
        [XmlArray("StrategyPositionGridColumns", Order = 20)]
        public List<ColumnSettingItem> StrategyPositionGrid { get; set; }
        
        /// <summary>
        /// 持仓汇总表格列设置
        /// </summary>
        //[XmlElement("PositionSummaryGridColumns", Order = 18)]
        [XmlArrayItem("item")]
        [XmlArray("PositionSummaryGridColumns", Order = 21)]
        public List<ColumnSettingItem> PositionSummaryGrid { get; set; }
        /// <summary>
        /// 持仓明细表格列设置
        /// </summary>
        //[XmlElement("PositionDetailsGridColumns", Order = 19)]
        [XmlArrayItem("item")]
        [XmlArray("PositionDetailsGridColumns", Order = 22)]
        public List<ColumnSettingItem> PositionDetailsGrid { get; set; }
        /// <summary>
        /// 组合持仓表格列设置
        /// </summary>
        //[XmlElement("ComboPositionGridColumns", Order = 20)]
        [XmlArrayItem("item")]
        [XmlArray("ComboPositionGridColumns", Order = 23)]
        public List<ColumnSettingItem> ComboPositionGrid { get; set; }
        /// <summary>
        /// 成交明细表格列设置
        /// </summary>
        //[XmlElement("TradeDetailsGridColumns", Order = 21)]
        [XmlArrayItem("item")]
        [XmlArray("TradeDetailsGridColumns", Order = 24)]
        public List<ColumnSettingItem> TradeDetailsGrid { get; set; }
        /// <summary>
        /// 成交汇总表格列设置
        /// </summary>
        //[XmlElement("", Order = 22)]
        [XmlArrayItem("item")]
        [XmlArray("TradeSummaryGridColumns", Order = 25)]
        public List<ColumnSettingItem> TradeSummaryGrid { get; set; }

        /// <summary>
        /// 资金账号表格列设置
        /// </summary>
          //[XmlElement("", Order = 23)]
        [XmlArrayItem("item")]
        [XmlArray("AccountregisterDataGridColumns", Order = 26)]
        public List<ColumnSettingItem> AccountregisterDataGrid { get; set; }
        /// <summary>
        /// 转账流水表格列设置
        /// </summary>
        //[XmlElement("", Order = 24)]
        [XmlArrayItem("item")]
        [XmlArray("TransferSerialDataGridColumns", Order = 27)]
        public List<ColumnSettingItem> TransferSerialDataGrid { get; set; }

        /// <summary>
        /// 合约默认手数设置
        /// </summary>
        //[XmlElement("DefaultQuant")]
        [XmlArrayItem("item")]
        [XmlArray("DefaultQuant", Order = 28)]
        public List<DefaultQuantSet> DefaultQuantSettings { get; set; }

        /// <summary>
        /// 合约组列表
        /// </summary>

        //[XmlElement("", Order = 26)]
        [XmlArrayItem("Group")]
        [XmlArray("SelectedInstrumentIDGroup", Order = 29)]
        public List<InstrumentGroup> InstrumentGroupList { get; set; }
        /// <summary>
        /// 产品列表
        /// </summary>
        [XmlArrayItem("item")]
        [XmlArray("InstrumentNameDict", Order = 30)]
        public List<Product> ProductList { get; set; }

        private Model.ColorSet _ColorSetModelObj;
        [XmlElement("Color", Order = 31)]
        public Model.ColorSet ColorSetModelObj {
            get { return _ColorSetModelObj; }
            set
            {
                _ColorSetModelObj = value;
                //_ColorSetModelObj.NotifyAllPropertyNest(_ColorSetModelObj);
                NotifyPropertyChanged("ColorSetModelObj");
            }
        }

        [XmlArrayItem("item")]
        [XmlArray("TradeCmdActions", Order =32)]
        public List<TradeCmdAction> TradeCmdList { get; set; }


        /// <summary>
        /// 本地数据维护设置
        /// </summary>
        [XmlElement(Order =33)]
        public LocalDataMaintainSet LocalMaintainSet { get; set; }              
        #endregion
        public void Save()
        {
            TQXmlHelper.XmlSerializeToFile(this,Trader.CfgFile, Encoding.UTF8);
        }
        #region 品种列表的增删改
        /// <summary>
        ///
        /// </summary>
        /// <param name="product"></param>
        public void AddProduct(Product product)
        {
            if (!ProductList.Contains(product))
            {
                ProductList.Add(product);
            }
        }

        public void RemoveProduct(Product p)
        {
            if (ProductList.Contains(p))
            {
                ProductList.Remove(p);
            }
        }

        public void UpdateProduct(string productID, Product p)
        {
            int itemIndex = ProductList.FindIndex(x => x.ProductID == productID);
            if (itemIndex >= 0)
            {
                ProductList[itemIndex] = p;
            }
        }
        #endregion

        #region 合约默认手数的管理
        public bool InstrumentQuantSettingsContains(int id)
        {
            var lst = DefaultQuantSettings.Where(x => x.ID == id);
            return (lst == null || lst.ToList().Count < 1) ? false:true;
        }
        /// <summary>
        /// 新增或修改默认手数
        /// 如有快捷键，则新增或修改快捷键
        /// </summary>
        /// <param name="settings"></param>
        public void AddOrUpdateInstrumentQuantSet(DefaultQuantSet settings)
        {            
            int itemIndex = DefaultQuantSettings.FindIndex(x => x.InstrumentID == settings.InstrumentID);
            if (itemIndex>=0)
            {   if (DefaultQuantSettings[itemIndex].CmdKey == settings.CmdKey
                    && DefaultQuantSettings[itemIndex].InstrumentID == settings.InstrumentID
                    && DefaultQuantSettings[itemIndex].Quant == settings.Quant)
                { return; }
                if (DefaultQuantSettings[itemIndex].CmdKey != settings.CmdKey)
                {
                    if (!string.IsNullOrEmpty(settings.CmdKey))
                    {
                        CustomCommands.UnregisterCommand(DefaultQuantSettings[itemIndex].InstrumentID);
                    }
                    if (!string.IsNullOrEmpty(settings.CmdKey))
                    {
                        RoutedUICommand cmd = CustomCommands.AddCommand(settings.CmdKey, settings.InstrumentID, SettingsType.DefaultQuant.ToString());
                        CommandBinding cb = new CommandBinding();
                        cb.Command = cmd;
                        cb.CanExecute += TQMain.T.main.DefaultQuant_CanExecute;
                        cb.Executed += TQMain.T.main.DefaultQuant_Executed;
                        TQMain.T.main.CommandBindings.Add(cb);
                    }
                }
                DefaultQuantSettings[itemIndex] = settings;
            }
            else
            {
                if (!string.IsNullOrEmpty(settings.CmdKey))
                {
                    RoutedUICommand cmd = CustomCommands.AddCommand(settings.CmdKey, settings.InstrumentID, SettingsType.DefaultQuant.ToString());
                    CommandBinding cb = new CommandBinding();
                    cb.Command = cmd;

                    cb.CanExecute += TQMain.T.main.DefaultQuant_CanExecute;
                    cb.Executed += TQMain.T.main.DefaultQuant_Executed;
                    TQMain.T.main.CommandBindings.Add(cb);
                }
                DefaultQuantSettings.Add(settings);
            }
        }
        /// <summary>
        /// 移除合约默认手数设置，如有快捷键，则注销快捷键
        /// </summary>
        /// <param name="dfltQuant"></param>
        public void RemoveInstrumentQuantSet(DefaultQuantSet dfltQuant)
        {
            if (!string.IsNullOrEmpty(dfltQuant.CmdKey))
            {
                CustomCommands.UnregisterCommand(dfltQuant.InstrumentID);
            }
            DefaultQuantSettings.Remove(dfltQuant);
        }        
        /// <summary>
        /// 根据合约ID或者品种ID查询默认下单手数
        /// </summary>
        /// <param name="instrumetID">合约ID</param>
        /// <param name="productID">品种ID</param>
        /// <returns>默认下单手数</returns>
        public int GetDefaultQuant(string instrumetID,string productID)
        {
            int dfltQuant = DefaultQuant;
            int itemIndex = DefaultQuantSettings.FindIndex(x => x.InstrumentID == instrumetID);
            if (itemIndex < 0)
            {
                itemIndex = DefaultQuantSettings.FindIndex(x => productID==x.InstrumentID);
            }
            if (itemIndex >= 0)
            {
                dfltQuant = DefaultQuantSettings[itemIndex].Quant;
            }
            return dfltQuant;
        }
        #endregion

        #region 合约组管理
        /// <summary>
        /// 根据自选组名获取自选组的合约信息
        /// </summary>
        /// <param name="groupName">自选组名</param>
        /// <returns></returns>
        public List<InstrumentData> GetInstrumentsByGroup(string groupName)
        {
            List<InstrumentData> result = new List<InstrumentData>();
            if(InstrumentGroupList==null || InstrumentGroupList.Count<1)
            {
                return result;
            }
            var cntrGroupLst = InstrumentGroupList.FindLast(x => x.Name == groupName);
            if (cntrGroupLst == null)
            {
                return result;
            }
            result.AddRange(cntrGroupLst.InstrumentDataList);
            return result;
        }
        /// <summary>
        /// 获取所有自选组组名
        /// </summary>
        /// <returns></returns>
        public List<string> GetInstrumentGroupNames()
        {
            if (InstrumentGroupList == null || InstrumentGroupList.Count < 1)
            {
                return new List<string>();
            }
            return InstrumentGroupList.Select(x => x.Name).ToList();
        }

        /// <summary>
        /// 移除合约组
        /// 如有快捷键，则注销该快捷键
        /// </summary>
        /// <param name="cntrGroup"></param>
        public void RemoveInstrumentGroup(InstrumentGroup cntrGroup)
        {
            if (!string.IsNullOrEmpty(cntrGroup.CmdKey))
            {
                CustomCommands.UnregisterCommand(cntrGroup.Name);
            }
            InstrumentGroupList.Remove(cntrGroup);
        }
        /// <summary>
        /// 新增或修改合约组
        /// 如果快捷键，需要新增或修改
        /// </summary>
        /// <param name="cntrGroup"></param>
        public void AddOrUpdateInstrumentGroup(InstrumentGroup cntrGroup)
        {
            int itemIndex = InstrumentGroupList.FindIndex(x => x.ID == cntrGroup.ID);
            if (itemIndex < 0)//新增
            {
                if (!string.IsNullOrEmpty(cntrGroup.CmdKey))
                {
                    RoutedUICommand cmd = CustomCommands.AddCommand(cntrGroup.CmdKey, cntrGroup.Name, "ChangeGroup");
                    CommandBinding cb = new CommandBinding();
                    cb.Command = cmd;
                    cb.CanExecute += TQMain.T.main.ChangeGroup_CanExecute;
                    cb.Executed += TQMain.T.main.ChangeGroup_Executed;
                    TQMain.T.main.CommandBindings.Add(cb);
                }
                InstrumentGroupList.Add(cntrGroup);
            }
            else//修改
            {
                if(InstrumentGroupList[itemIndex].CmdKey == cntrGroup.CmdKey
                    && InstrumentGroupList[itemIndex].InstrumentDataList == cntrGroup.InstrumentDataList
                    && InstrumentGroupList[itemIndex].Name == cntrGroup.Name)
                { return; }
                if (InstrumentGroupList[itemIndex].CmdKey != cntrGroup.CmdKey)
                {
                    if (!string.IsNullOrEmpty(InstrumentGroupList[itemIndex].CmdKey))
                    {
                        CustomCommands.UnregisterCommand(InstrumentGroupList[itemIndex].Name);
                    }
                    if (!string.IsNullOrEmpty(cntrGroup.CmdKey))
                    {
                        RoutedUICommand cmd = CustomCommands.AddCommand(cntrGroup.CmdKey, cntrGroup.Name, "ChangeGroup");
                        CommandBinding cb = new CommandBinding();
                        cb.Command = cmd;
                        cb.CanExecute += TQMain.T.main.ChangeGroup_CanExecute;
                        cb.Executed += TQMain.T.main.ChangeGroup_Executed;
                        TQMain.T.main.CommandBindings.Add(cb);
                    }
                }
                InstrumentGroupList[itemIndex] = cntrGroup;
            }
        }
        /// <summary>
        /// 合约组增加合约
        /// </summary>
        /// <param name="groupName"></param>
        /// <param name="instruments"></param>
        public void UpdateInstrumentsByGroup(string groupName, List<InstrumentData> instruments)
        {
            if (string.IsNullOrEmpty(groupName) || instruments == null || instruments.Count < 1)
            { return; }
            foreach (var item in InstrumentGroupList)
            {              
                if (item.Name == groupName)
                {
                    item.InstrumentDataList.Clear();
                    foreach (var inst in instruments)
                    {
                        if (!item.InstrumentDataList.Contains(inst))
                        {
                            item.InstrumentDataList.Add(inst);
                        }
                    }
                }
            }
        }

        #endregion

        #region 交易命令管理
        /// <summary>
        /// 新增或修改交易命令
        /// 如有快捷键，则新增或修改快捷键
        /// </summary>
        /// <param name="tradeCmd"></param>
        public void AddOrUpdateTradeCmd(TradeCmdAction tradeCmd)
        {
            int itemIndex = TradeCmdList.FindIndex(x => x.ID == tradeCmd.ID);
            if (itemIndex >= 0)
            {
                if (TradeCmdList[itemIndex].CmdKey == tradeCmd.CmdKey
                    && TradeCmdList[itemIndex].Action == tradeCmd.Action)
                { return; }
                if (TradeCmdList[itemIndex].CmdKey != tradeCmd.CmdKey)
                {
                    if (!string.IsNullOrEmpty(TradeCmdList[itemIndex].CmdKey))
                    {
                        CustomCommands.UnregisterCommand(TradeCmdList[itemIndex].Action);
                    }
                    if (!string.IsNullOrEmpty(tradeCmd.CmdKey))
                    {
                        RoutedUICommand cmd = CustomCommands.AddCommand(tradeCmd.CmdKey, tradeCmd.Action, SettingsType.TradeShortcutKeySet.ToString());
                        CommandBinding cb = new CommandBinding();
                        cb.Command = cmd;
                        cb.CanExecute += TQMain.T.main.TradeCommand_CanExecute;
                        cb.Executed += TQMain.T.main.TradeCommand_Executed;
                        TQMain.T.main.CommandBindings.Add(cb);
                    }
                }
                TradeCmdList[itemIndex] = tradeCmd;
            }
            else
            {
                if (!string.IsNullOrEmpty(tradeCmd.CmdKey))
                {
                    RoutedUICommand cmd = CustomCommands.AddCommand(tradeCmd.CmdKey, tradeCmd.Action, SettingsType.TradeShortcutKeySet.ToString());
                    CommandBinding cb = new CommandBinding();
                    cb.Command = cmd;
                    cb.CanExecute += TQMain.T.main.TradeCommand_CanExecute;
                    cb.Executed += TQMain.T.main.TradeCommand_Executed;
                    TQMain.T.main.CommandBindings.Add(cb);
                }
                TradeCmdList.Add(tradeCmd);
            }
        }
        /// <summary>
        /// 移除交易命令
        /// </summary>
        /// <param name="tradeCmd"></param>
        public void RemoveTradeCommand(TradeCmdAction tradeCmd)
        {
            if (!string.IsNullOrEmpty(tradeCmd.CmdKey))
            {
                CustomCommands.UnregisterCommand(tradeCmd.Action);
            }
            TradeCmdList.Remove(tradeCmd);
        }
        #endregion
    }

    /// <summary>
    /// 默认期商配置
    /// </summary>
    [XmlType("DefaultBroker")]
    public class DefaultBroker
    {

        /// <summary>
        /// 期商代码
        /// </summary>
        [XmlIgnore]
        public string ID { get; set; }

        /// <summary>
        /// 期商名称
        /// </summary>
        [XmlAttribute]
        public string Name { get; set; }
        /// <summary>
        /// 期商的配置文件名称
        /// </summary>
        [XmlAttribute]
        public string Config { get; set; }

        /// <summary>
        /// 网络服务商
        /// </summary>
        [XmlAttribute]
        public string Server { get; set; }
    }


    /// <summary>
    /// 合约组信息
    /// </summary>
    public class InstrumentGroup
    {
        public InstrumentGroup()
        {
            InstrumentDataList = new List<InstrumentData>();
        }

        [XmlAttribute]
        public int ID { get; set; }
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string CmdKey { get; set; }

        [XmlElement("item")]
        public List<InstrumentData> InstrumentDataList { get; set; }
    }

    /// <summary>
    /// 默认手数列表设置
    /// </summary>

    public class DefaultQuantSet
    {
        /// <summary>
        /// 标志
        /// </summary>
        [XmlAttribute]
        public int ID;
        /// <summary>
        /// 品种/合约代码
        /// </summary>
        [XmlAttribute]
        public string InstrumentID { get; set; }
        /// <summary>
        /// 手数
        /// </summary>
        [XmlAttribute]
        public int Quant { get; set; }

        /// <summary>
        /// 快捷代码
        /// </summary>
        [XmlAttribute]
        public string CmdKey { get; set; }
    }

    /// <summary>
    /// DataGrid列表设置
    /// </summary>
    public class ColumnSettingItem : ObservableObject, IComparable
    {
        private string _title;
        private string _display;
        private string _name;
        private short _Sort;

        [XmlAttribute]
        public string Title
        { get { return _title; }
            set { _title = value; NotifyPropertyChanged("Title"); } }
        [XmlAttribute]
        public string Display { get { return _display; } set { _display = value; NotifyPropertyChanged("Display"); } }
        [XmlAttribute]
        public string Name { get { return _name; } set { _name = value; NotifyPropertyChanged("Name"); } }
        [XmlAttribute]
        public short Sort { get { return _Sort; } set { _Sort = value; NotifyPropertyChanged("Sort"); } }
        public int CompareTo(object obj) { ColumnSettingItem item = obj as ColumnSettingItem; return Sort.CompareTo(item.Sort); }
    }
    /// <summary>
    /// 邮箱配置
    /// </summary>
    public class MailConfig
    {
        /// <summary>
        /// 用户名
        /// </summary>
        [XmlAttribute("Name")]
        public string UserName { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        [XmlAttribute("Password")]
        public string Password { get; set; }
    }    
    /// <summary>
    /// 投资者信息
    /// </summary>
    public class ExchangeAccountInfo
    {
        /// <summary>
        /// 资金账号
        /// </summary>
        [XmlAttribute]
        public string ID { get; set; }

        [XmlAttribute]
        public string BrokerName { get; set; }
        /// <summary>
        /// 期商配置文件
        /// </summary>
        [XmlAttribute]
        public string BrokerConfig { get; set; }

        /// <summary>
        /// 期商网络
        /// </summary>
        [XmlAttribute]
        public string BrokerServer { get; set; }
        /// <summary>
        /// 最近一次登录时间，格式yyyyMMddHHmmss,如20160610123323
        /// </summary>
        [XmlAttribute]
        public string LastLoginTime { get; set; }
    }
    public class TradeCmdAction
    {
        [XmlAttribute]
        public int ID { get; set; }
        [XmlAttribute]
        public string CmdKey { get; set; }
        [XmlAttribute]
        public string Action { get; set; }
    }

    public class LocalDataMaintainSet
    {
        /// <summary>
        /// 是否在登录成功后查询品种
        /// </summary>
        [XmlAttribute]
        public bool IsInitQryProduct { get; set; }
        /// <summary>
        /// 是否在登录成功后查询合约
        /// </summary>
        [XmlAttribute]
        public bool IsInitQryInstrument { get; set; }
        /// <summary>
        /// 是否在登录成功后查询保证金率
        /// </summary>
        [XmlAttribute]
        public bool IsInitQryMargin { get; set; }
        /// <summary>
        /// 是否在登录成功后查询手续费
        /// </summary>
        [XmlAttribute]
        public bool IsInitQryCommission { get; set; }
    }
}
