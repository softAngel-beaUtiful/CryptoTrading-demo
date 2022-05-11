/*
 * 对接ThostModel中的类
 */
using CryptoTrading.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CryptoTrading.Model
{
    #region CTP接口相关结构对应的C#存储结构
    /// <summary>
    /// 结算单信息
    /// </summary>
    public class SettlementInfo
    {
        /// <summary>
        /// 交易日
        /// </summary>
        public string TradingDay { get; set; }

        /// <summary>
        /// 结算编号
        /// </summary>
        public int SettlementID;

        /// <summary>
        /// 经纪商代码
        /// </summary>
        public string BrokerID { get; set; }
        /// <summary>
        /// 投资者代码
        /// </summary>
        public string InvestorID { get; set; }
        /// <summary>
        /// 序号
        /// </summary>
        public int SequenceNo { get; set; }
        /// <summary>
        /// 消息正文
        /// </summary>
        public string Content { get; set; }

        public override string ToString()
        {
            return Utility.GetObjectPropertyInfo(this);
        }
    }

    /// <summary>
    /// 投资者持仓明细
    /// </summary>
    public class InvestorPositionDetail
    {
        /// <summary>
        /// 合约代码
        /// </summary>
        public string InstrumentID{get;set;}
        /// <summary>
        /// 经纪公司代码
        /// </summary>
        public string BrokerID{get;set;}
        /// <summary>
        /// 投资者代码
        /// </summary>
        public string InvestorID{get;set;}
        /// <summary>
        /// 投机套保标志('1'/投机，'2'/套利，'3'/套保，)
        /// </summary>
        public char HedgeFlag{get;set;}
        /// <summary>
        /// 买卖('0'/买，'1'/卖)
        /// </summary>
        public char Direction{get;set;}
        /// <summary>
        /// 开仓日期
        /// </summary>
        public string OpenDate{get;set;}
        /// <summary>
        /// 成交编号
        /// </summary>
        public string TradeID{get;set;}
        /// <summary>
        /// 数量
        /// </summary>
        public int Volume{get;set;}
        /// <summary>
        /// 开仓价
        /// </summary>
        public double OpenPrice{get;set;}
        /// <summary>
        /// 交易日
        /// </summary>
        public string TradingDay{get;set;}
        /// <summary>
        /// 结算编号
        /// </summary>
        public int SettlementID{get;set;}
        /// <summary>
        /// 成交类型
        /// '#'组合持仓拆分为单一持仓,初始化不应包含该类型的持仓
        ///'0'普通成交
        ///'1'期权执行
        ///'2'OTC成交
        ///'3'期转现衍生成交
        ///'4'组合衍生成交
        /// </summary>
        public char TradeType{get;set;}
        /// <summary>
        /// 组合合约代码
        /// </summary>
        public string CombInstrumentID{get;set;}
        /// <summary>
        /// 交易所代码
        /// </summary>
        public string ExchangeID{get;set;}
        /// <summary>
        /// 逐日盯市平仓盈亏
        /// </summary>
        public double CloseProfitByDate{get;set;}
        /// <summary>
        /// 逐笔对冲平仓盈亏
        /// </summary>
        public double CloseProfitByTrade{get;set;}
        /// <summary>
        /// 逐日盯市持仓盈亏
        /// </summary>
        public double PositionProfitByDate{get;set;}
        /// <summary>
        /// 逐笔对冲持仓盈亏
        /// </summary>
        public double PositionProfitByTrade{get;set;}
        /// <summary>
        /// 投资者保证金
        /// </summary>
        public double Margin { get; set; }
        /// <summary>
        /// 交易所保证金
        /// </summary>
        public double ExchMargin { get; set; }
        /// <summary>
        /// 保证金率
        /// </summary>
        public double MarginRateByMoney { get; set; }
        /// <summary>
        /// 保证金率(按手数)
        /// </summary>
        public double MarginRateByVolume { get; set; }
        /// <summary>
        /// 昨结算价
        /// </summary>
        public double LastSettlementPrice { get; set; }
        /// <summary>
        /// 结算价
        /// </summary>
        public double SettlementPrice { get; set; }
        /// <summary>
        /// 平仓量
        /// </summary>
        public int CloseVolume { get; set; }
        /// <summary>
        /// 平仓金额
        /// </summary>
        public double CloseAmount{get;set;}

        public override string ToString()
        {
            return Utility.GetObjectPropertyInfo(this);
        }
    }

    /// <summary>
    /// 资金账户口令变更
    /// </summary>
    public class TradingAccountPasswordUpdate
    {
        /// <summary>
        /// 经纪公司代码
        /// </summary>
        public string BrokerID{get; set;}

        /// <summary>
        /// 投资者帐号
        /// </summary>
        public string AccountID{get; set;}

        /// <summary>
        /// 原来的口令
        /// </summary>
        public string OldPassword{get; set;}

        /// <summary>
        /// 新的口令
        /// </summary>
        public string NewPassword{get; set;}

        /// <summary>
        /// 币种代码
        /// </summary>
        public string CurrencyID{get; set;}

        public override string ToString()
        {
            return Utility.GetObjectPropertyInfo(this);
        }
    }

    /// <summary>
    /// 银期转账交易流水表
    /// </summary>
    public class TransferSerial
    {
        /// <summary>
        /// 平台流水号
        /// </summary>
        public int PlateSerial{get; set;}

        /// <summary>
        /// 交易发起方日期
        /// </summary>
        public string TradeDate{get; set;}

        /// <summary>
        /// 交易日期
        /// </summary>
        public string TradingDay{get; set;}

        /// <summary>
        /// 交易时间
        /// </summary>
        public string TradeTime{get; set;}

        /// <summary>
        /// 交易描述
        /// </summary>
        public string TradeDesc{get; set;}
        /// <summary>
        /// 会话编号
        /// </summary>
        public int SessionID{get; set;}

        /// <summary>
        /// 银行编码
        /// </summary>
        public string BankID{get; set;}

        /// <summary>
        /// 银行分支机构编码
        /// </summary>
        public string BankBranchID{get; set;}

        /// <summary>
        /// 银行帐号类型
        /// </summary>
        public char BankAccType{get; set;}

        /// <summary>
        /// 银行帐号
        /// </summary>
        public string BankAccount{get; set;}

        /// <summary>
        /// 银行流水号
        /// </summary>
        public string BankSerial{get; set;}

        /// <summary>
        /// 期货公司编码
        /// </summary>
        public string BrokerID{get; set;}

        /// <summary>
        /// 期商分支机构代码
        /// </summary>
        public string BrokerBranchID{get; set;}
        /// <summary>
        /// 期货公司帐号类型
        /// </summary>
        public char FutureAccType{get; set;}

        /// <summary>
        /// 投资者帐号
        /// </summary>
        public string AccountID{get; set;}

        /// <summary>
        /// 投资者代码
        /// </summary>
        public string InvestorID{get; set;}
        /// <summary>
        /// 期货公司流水号
        /// </summary>
        public int FutureSerial{get; set;}
        /// <summary>
        /// 证件类型
        /// </summary>
        public char IdCardType{get; set;}

        /// <summary>
        /// 证件号码
        /// </summary>
        public string IdentifiedCardNo{get; set;}

        /// <summary>
        /// 币种代码
        /// </summary>
        public string CurrencyID{get; set;}
        /// <summary>
        /// 交易金额
        /// </summary>
        public double TradeAmount{get; set;}
        /// <summary>
        /// 应收客户费用
        /// </summary>
        public double CustFee{get; set;}
        /// <summary>
        /// 应收期货公司费用
        /// </summary>
        public double BrokerFee{get; set;}
        /// <summary>
        /// 有效标志
        /// </summary>
        public string AvailabilityFlag{get; set;}

        /// <summary>
        /// 操作员
        /// </summary>
        public string OperatorCode{get; set;}

        /// <summary>
        /// 新银行帐号
        /// </summary>
        public string BankNewAccount{get; set;}
        /// <summary>
        /// 错误代码
        /// </summary>
        public int ErrorID{get; set;}

        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorMsg{get; set;}

        /// <summary>
        /// 格式化输出所有公开属性的信息
        /// </summary>
        /// <returns>所有的公开属性信息(属性名：属性值),以",\t"分割</returns>
        public override string ToString()
        {
            return Utility.GetObjectPropertyInfo(this);
        }
    }
    /// <summary>
    ///客户开销户信息表
    /// </summary>
    public class Accountregister
    {
        /// <summary>
        /// 交易日期
        /// </summary>
        public string TradeDay{get; set;}

        /// <summary>
        /// 银行编码
        /// </summary>
        public string BankID{get; set;}

        /// <summary>
        /// 银行简称
        /// </summary>
        public string BankName { get; set; }
        /// <summary>
        /// 银行分支机构编码
        /// </summary>
        public string BankBranchID{get; set;}

        /// <summary>
        /// 银行帐号
        /// </summary>
        public string BankAccount{get; set;}

        /// <summary>
        /// 期货公司编码
        /// </summary>
        public string BrokerID{get; set;}

        /// <summary>
        /// 期货公司分支机构编码
        /// </summary>
        public string BrokerBranchID{get; set;}

        /// <summary>
        /// 投资者帐号
        /// </summary>
        public string AccountID{get; set;}
        /// <summary>
        /// 证件类型
        /// </summary>
        public string IdCardType{get; set;}

        /// <summary>
        /// 证件号码
        /// </summary>
        public string IdentifiedCardNo{get; set;}

        /// <summary>
        /// 客户姓名
        /// </summary>
        public string CustomerName{get; set;}

        /// <summary>
        /// 币种名称
        /// </summary>
        public string CurrencyName{get; set;}
        /// <summary>
        /// 开销户类别
        /// (开户Open '1';销户Destroy '0')
        /// </summary>
        public string OpenOrDestroy{get; set;}

        /// <summary>
        /// 签约日期
        /// </summary>
        public string RegDate{get; set;}

        /// <summary>
        /// 解约日期
        /// </summary>
        public string OutDate{get; set;}
        /// <summary>
        /// 交易ID
        /// </summary>
        public int TID{get; set;}
        /// <summary>
        /// 客户类型
        /// (自然人Person '0';机构户Institution '1')
        /// </summary>
        public string CustType{get; set;}
        /// <summary>
        /// 银行帐号类型
        /// </summary>
        public string BankAccType{get; set;}

        /// <summary>
        /// 格式化输出所有公开属性的信息
        /// </summary>
        /// <returns>所有的公开属性信息(属性名：属性值),以",\t"分割</returns>
        public override string ToString()
        {
            return Utility.GetObjectPropertyInfo(this);
        }
    }

    /// <summary>
    /// 银行发起银行资金转期货响应
    /// </summary>
    public class RspTransfer
    {
        /// <summary>
        /// 业务功能码
        /// </summary>
        public string TradeCode{get; set;}

        /// <summary>
        /// 银行代码
        /// </summary>
        public string BankID{get; set;}

        /// <summary>
        /// 银行分中心代码
        /// </summary>
        public string BankBrchID{get; set;}
        /// <summary>
        /// 经纪公司代码
        /// </summary>
        public string BrokerID{get; set;}

        /// <summary>
        /// 期商分支机构代码
        /// </summary>
        public string BrokerBranchID{get; set;}

        /// <summary>
        /// 交易日期
        /// </summary>
        public string TradeDate{get; set;}

        /// <summary>
        /// 交易时间
        /// </summary>
        public string TradeTime{get; set;}

        /// <summary>
        /// 银行流水号
        /// </summary>
        public string BankSerial{get; set;}

        /// <summary>
        /// 交易系统日期
        /// </summary>
        public string TradingDay{get; set;}
        /// <summary>
        /// 银期平台消息流水号
        /// </summary>
        public int PlateSerial{get; set;}
        /// <summary>
        /// 最后分片标志(是最后分片Yes '0'，不是最后分片No '1')
        /// </summary>
        public char LastFragment{get; set;}
        /// <summary>
        /// 会话号
        /// </summary>
        public int SessionID{get; set;}
        /// <summary>
        /// 客户姓名
        /// </summary>
        public string CustomerName{get; set;}

        /// <summary>
        /// 证件类型
        /// </summary>
        public char IdCardType{get; set;}

        /// <summary>
        /// 证件号码
        /// </summary>
        public string IdentifiedCardNo{get; set;}
        /// <summary>
        /// 客户类型
        /// </summary>
        public char CustType{get; set;}

        /// <summary>
        /// 银行帐号
        /// </summary>
        public string BankAccount{get; set;}

        /// <summary>
        ///银行密码
        /// </summary>
        public string BankPassWord{get; set;}

        /// <summary>
        /// 投资者帐号
        /// </summary>
        public string AccountID{get; set;}

        /// <summary>
        /// 期货密码
        /// </summary>
        public string Password{get; set;}
        /// <summary>
        /// 安装编号
        /// </summary>
        public int InstallID{get; set;}
        /// <summary>
        /// 期货公司流水号
        /// </summary>
        public int FutureSerial{get; set;}

        /// <summary>
        /// 用户标识
        /// </summary>
        public string UserID{get; set;}
        /// <summary>
        /// 验证客户证件号码标志('0'是，'1'否)
        /// </summary>
        public char VerifyCertNoFlag{get; set;}

        /// <summary>
        /// 币种代码
        /// </summary>
        public string CurrencyID{get; set;}
        /// <summary>
        /// 转帐金额
        /// </summary>
        public double TradeAmount{get; set;}
        /// <summary>
        /// 期货可取金额
        /// </summary>
        public double FutureFetchAmount{get; set;}
        /// <summary>
        /// 费用支付标志
        ///(由受益方支付费用BEN '0';由发送方支付费用OUR '1';由发送方支付发起的费用，受益方支付接受的费用SHA '2')
        /// </summary>
        public char FeePayFlag{get; set;}
        /// <summary>
        /// 应收客户费用
        /// </summary>
        public double CustFee{get; set;}
        /// <summary>
        /// 应收期货公司费用
        /// </summary>
        public double BrokerFee{get; set;}

        /// <summary>
        /// 发送方给接收方的消息
        /// </summary>
        public string Message{get; set;}

        /// <summary>
        /// 摘要
        /// </summary>
        public string Digest{get; set;}
        /// <summary>
        /// 银行帐号类型
        /// (银行存折BankBook '1';储蓄卡SavingCard '2';信用卡CreditCard '3')
        /// </summary>
        public char BankAccType{get; set;}

        /// <summary>
        /// 渠道标志
        /// </summary>
        public string DeviceID{get; set;}
        /// <summary>
        /// 期货单位帐号类型
        /// (银行存折BankBook '1';储蓄卡SavingCard '2';信用卡CreditCard '3')
        /// </summary>
        public char BankSecuAccType{get; set;}

        /// <summary>
        /// 期货公司银行编码
        /// </summary>
        public string BrokerIDByBank{get; set;}

        /// <summary>
        /// 期货单位帐号
        /// </summary>
        public string BankSecuAcc{get; set;}
        /// <summary>
        /// 银行密码标志
        /// (不核对NoCheck '0';明文核对BlankCheck '1';密文核对EncryptCheck '2')
        /// </summary>
        public char BankPwdFlag{get; set;}
        /// <summary>
        /// 期货资金密码核对标志
        /// (不核对NoCheck '0';明文核对BlankCheck '1';密文核对EncryptCheck '2')
        /// </summary>
        public char SecuPwdFlag{get; set;}

        /// <summary>
        /// 交易柜员
        /// </summary>
        public string OperNo{get; set;}
        /// <summary>
        /// 请求编号
        /// </summary>
        public int RequestID{get; set;}
        /// <summary>
        /// 交易ID
        /// </summary>
        public int TID{get; set;}
        /// <summary>
        /// 转账交易状态(正常Normal '0';被冲正Repealed '1')
        /// </summary>
        public char TransferStatus{get; set;}
        /// <summary>
        /// 错误代码
        /// </summary>
        public int ErrorID{get; set;}
        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorMsg{get; set;}

        /// <summary>
        /// 格式化输出所有公开属性的信息
        /// </summary>
        /// <returns>所有的公开属性信息(属性名：属性值),以",\t"分割</returns>
        public override string ToString()
        {
            return Utility.GetObjectPropertyInfo(this);
        }
    }

    /// <summary>
    /// 查询账户信息通知
    /// </summary>
    public class NotifyQueryAccount
    {
        /// <summary>
        /// 业务功能码
        /// </summary>
        public string TradeCode { get; set; }
        /// <summary>
        /// 银行代码
        /// </summary>
        public string BankID { get; set; }
        /// <summary>
        /// 银行分支机构代码
        /// </summary>
        public string BankBranchID { get; set; }
        /// <summary>
        /// 期商代码
        /// </summary>
        public string BrokerID { get; set; }
        /// <summary>
        /// 期商分支机构代码
        /// </summary>
        public string BrokerBranchID { get; set; }
        /// <summary>
        /// 交易日期
        /// </summary>
        public string TradeDate { get; set; }
        /// <summary>
        /// 交易时间
        /// </summary>
        public string TradeTime { get; set; }
        /// <summary>
        /// 银行流水号
        /// </summary>
        public string BankSerial { get; set; }
        /// <summary>
        /// 交易系统日期
        /// </summary>
        public string TradingDay { get; set; }
        /// <summary>
        /// 银期平台消息流水号
        /// </summary>
        public int PlateSerial { get; set; }
        /// <summary>
        /// 是否是最后分片标志(是最后分片Yes '0';不是最后分片No '1')
        /// </summary>
        public bool LastFragment { get; set; }
        /// <summary>
        /// 会话号
        /// </summary>
        public int SessionID { get; set; }
        /// <summary>
        /// 客户姓名
        /// </summary>
        public string CustomerName { get; set; }
        /// <summary>
        /// 证件类型
        /// </summary>
        public string IdCardType { get; set; }
        /// <summary>
        /// 证件号码
        /// </summary>
        public string IdentifiedCardNo { get; set; }
        /// <summary>
        /// 客户类型
        /// </summary>
        public string CustType { get; set; }
        /// <summary>
        /// 银行帐号
        /// </summary>
        public string BankAccount { get; set; }
        /// <summary>
        /// 银行密码
        /// </summary>
        public string BankPassWord { get; set; }
        /// <summary>
        /// 投资者帐号
        /// </summary>
        public string AccountID { get; set; }

        /// <summary>
        /// 期货密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 期货公司流水号
        /// </summary>
        public int FutureSerial { get; set; }
        /// <summary>
        /// 安装编号
        /// </summary>
        public int InstallID { get; set; }
        /// <summary>
        /// 用户标识
        /// </summary>
        public string UserID { get; set; }
        /// <summary>
        /// 是否验证客户证件号码(0是，1否)
        /// </summary>
        public bool VerifyCertNoFlag { get; set; }

        /// <summary>
        /// 币种代码
        /// </summary>
        public string CurrencyID { get; set; }

        /// <summary>
        /// 摘要
        /// </summary>
        public string Digest { get; set; }
        /// <summary>
        /// 银行帐号类型
        /// </summary>
        public string BankAccType { get; set; }
        /// <summary>
        /// 渠道标志
        /// </summary>
        public string DeviceID { get; set; }
        /// <summary>
        /// 期货单位帐号类型
        /// </summary>
        public string BankSecuAccType { get; set; }

        /// <summary>
        /// 期货公司银行编码
        /// </summary>
        public string BrokerIDByBank { get; set; }
        /// <summary>
        /// 期货单位帐号
        /// </summary>
        public string BankSecuAcc { get; set; }
        /// <summary>
        /// 银行密码标志
        /// </summary>
        public string BankPwdFlag { get; set; }
        /// <summary>
        /// 期货资金密码核对标志
        /// </summary>
        public string SecuPwdFlag { get; set; }

        /// <summary>
        /// 交易柜员
        /// </summary>
        public string OperNo { get; set; }
        /// <summary>
        /// 请求编号
        /// </summary>
        public int RequestID { get; set; }
        /// <summary>
        /// 交易ID
        /// </summary>
        public int TID { get; set; }
        ///// <summary>
        ///// 转账交易状态(正常Normal '0';被冲正Repealed '1')
        ///// V6.3.6已去除
        ///// </summary>
        //public string TransferStatus { get; set; }
        /// <summary>
        /// 银行可用金额
        /// </summary>
        public double BankUseAmount { get; set; }
        /// <summary>
        /// 银行可取金额
        /// </summary>
        public double BankFetchAmount { get; set; }
        /// <summary>
        /// 错误代码
        /// </summary>
        public int ErrorID { get; set; }
        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorMsg { get; set; }

        /// <summary>
        /// 格式化输出所有公有属性的信息
        /// </summary>
        /// <returns>所有的公有属性信息(属性名：属性值),以",\t"分割</returns>
        public override string ToString()
        {
            return Utility.GetObjectPropertyInfo(this);
        }
    }

    /// <summary>
    /// 产品
    /// </summary>
    public class Product
    {
        /// <summary>
        ///产品代码
        /// </summary>
        [XmlAttribute]
        public string ProductID{get;set;}

        /// <summary>
        ///产品名称
        /// </summary>
        [XmlAttribute]
        public string ProductName{get;set;}

        /// <summary>
        ///交易所代码
        /// </summary>
        [XmlIgnore]
        public string ExchangeID{get;set;}
        /// <summary>
        /// 产品类型
        /// </summary>
        [XmlIgnore]
        public char ProductClass{get;set;}
        /// <summary>
        /// 合约数量乘数
        /// </summary>
        [XmlIgnore]
        public int VolumeMultiple{get;set;}
        /// <summary>
        /// 最小变动价位
        /// </summary>
        [XmlIgnore]
        public double PriceTick{get;set;}

        /// <summary>
        ///市价单最大下单量
        /// </summary>
        [XmlIgnore]
        public int MaxMarketOrderVolume{get;set;}

        /// <summary>
        ///市价单最小下单量
        /// </summary>
        [XmlIgnore]
        public int MinMarketOrderVolume{get;set;}

        /// <summary>
        ///限价单最大下单量
        /// </summary>
        [XmlIgnore]
        public int MaxLimitOrderVolume{get;set;}
        /// <summary>
        /// 限价单最小下单量
        /// </summary>
        [XmlIgnore]
        public int MinLimitOrderVolume{get;set;}
        /// <summary>
        /// 持仓类型
        /// </summary>
        [XmlIgnore]
        public char PositionType{get;set;}
        /// <summary>
        /// 持仓日期类型
        /// </summary>
        [XmlIgnore]
        public char PositionDateType{get;set;}
        /// <summary>
        /// 平仓处理类型
        /// </summary>
        [XmlIgnore]
        public char CloseDealType{get;set;}

        /// <summary>
        /// 交易币种类型
        /// </summary>
        [XmlIgnore]
        public string TradeCurrencyID{get;set;}
        /// <summary>
        /// 质押资金可用范围
        /// </summary>
        [XmlIgnore]
        public char MortgageFundUseRange{get;set;}
        /// <summary>
        /// 交易所产品代码
        /// </summary>
        [XmlIgnore]
        public string ExchangeProductID{get;set;}

        /// <summary>
        /// 合约基础商品乘数
        /// </summary>
        [XmlIgnore]
        public double UnderlyingMultiple{get;set;}
    }
    /// <summary>
    /// 平仓明细
    /// </summary>
    public class ClosedPosition
    {
        /// <summary>
        /// 交易所名称
        /// </summary>
        public string Exchange { get; set; }

        /// <summary>
        /// 合约代码
        /// </summary>
        public string InstrumentID { get; set; }


        /// <summary>
        /// 多空方向
        /// </summary>
        public TradeDirection Direction { get; set; }

        /// <summary>
        /// 手数
        /// </summary>
        public int Volume { get; set; }

        /// <summary>
        /// 开仓日期
        /// </summary>
        public string OpenDate { get;set; }

        /// <summary>
        /// 开仓价
        /// </summary>
        public double OpenPrice { get; set; }

        /// <summary>
        /// 平仓日期
        /// </summary>
        public string CloseDate { get; set; }

        /// <summary>
        /// 平仓价
        /// </summary>
        public double ClosePrice { get; set; }

        /// <summary>
        /// 平仓盈亏
        /// </summary>
        public double CloseProfit { get; set; }

        //结算单中还可提取的数据有：品种/Production、昨结价/PrevSttl、权利金收支/PremiumInOut
    }

    /// <summary>
    /// 交易统计类
    /// </summary>
    public class TradeStat
    {
        /// <summary>
        /// 合约ID
        /// </summary>
        public string InstrumentID { get; set; }
        /// <summary>
        /// 多空方向
        /// </summary>
        public TradeDirection Direction { get; set; }
        /// <summary>
        /// 盈亏情况
        /// </summary>
        public string ProfitLoss { get; set; }
        /// <summary>
        /// 交易次数
        /// </summary>
        public int TradeCount { get;set; }
        /// <summary>
        /// 手数
        /// </summary>
        public int Volume{get;set;}
        /// <summary>
        /// 净利润
        /// </summary>
        public double NetProfit { get; set; }
        /// <summary>
        /// 平均每次利润
        /// </summary>
        public double AvgProfitByTime { get; set; }
        /// <summary>
        /// 平均每手利润
        /// </summary>
        public double AvgProfitByHand { get; set; }

        public static TradeStat DeepCopy(TradeStat t)
        {
            return new TradeStat()
            {
                AvgProfitByHand = t.AvgProfitByHand,
                AvgProfitByTime = t.AvgProfitByTime,
                Direction = t.Direction,
                InstrumentID = t.InstrumentID,
                NetProfit = t.NetProfit,
                ProfitLoss = t.ProfitLoss,
                TradeCount = t.TradeCount,
                Volume = t.Volume
            };
        }
    }
    #endregion

    #region 项目所需类
    public class DefaultQuantSettings
    {
        /// <summary>
        /// 合约(组)/自定义合约ID
        /// </summary>
        public string InstrumentID { get; set; }

        /// <summary>
        /// 合约(组)/自定义合约简称
        /// </summary>
        public string InstrumentName { get; set; }

        /// <summary>
        /// 默认手数
        /// </summary>
        public int DefaultQuant { get; set; }

        /// <summary>
        /// 快捷键
        /// </summary>
        public string CommandKey { get; set; }
    }
    #endregion
    /// <summary>
    /// 合约手续费率
    /// </summary>
    public class InstrumentCommissionRate
    {
        /// <summary>
        /// 合约代码
        /// </summary>
        [XmlAttribute]
        public string InstrumentID { get; set; }
        /// <summary>
        /// 投资者范围
        /// </summary>
        [XmlAttribute]
        public InvestorRangeType InvestorRange { get; set; }
        /// <summary>
        /// 经纪公司代码
        /// </summary>
        [XmlAttribute]
        public string BrokerID { get; set; }
        /// <summary>
        /// 投资者代码
        /// </summary>
        [XmlAttribute]
        public string InvestorID { get; set; }
        /// <summary>
        /// 开仓手续费率
        /// </summary>
        [XmlAttribute]
        public double OpenRatioByMoney { get; set; }

        /// <summary>
        /// 开仓手续费
        /// </summary>
        [XmlAttribute]
        public double OpenRatioByVolume { get; set; }

        /// <summary>
        /// 平仓手续费率
        /// </summary>
        [XmlAttribute]
        public double CloseRatioByMoney { get; set; }

        /// <summary>
        /// 平仓手续费
        /// </summary>
        [XmlAttribute]
        public double CloseRatioByVolume { get; set; }

        /// <summary>
        /// 平今手续费率
        /// </summary>
        [XmlAttribute]
        public double CloseTodayRatioByMoney { get; set; }

        /// <summary>
        /// 平今手续费
        /// </summary>
        [XmlAttribute]
        public double CloseTodayRatioByVolume { get; set; }
    }
    /// <summary>
    /// 合约保证金率
    /// </summary>
    public class InstrumentMarginRate
    {
        /// <summary>
        /// 合约代码
        /// </summary>
        [XmlAttribute]
        public string InstrumentID { get; set; }
        ///投资者范围
        [XmlAttribute]
        public InvestorRangeType InvestorRange { get; set; }
        /// <summary>
        /// 经纪公司代码
        /// </summary>
        [XmlAttribute]
        public string BrokerID { get; set; }
        /// <summary>
        /// 投资者代码
        /// </summary>
        [XmlAttribute]
        public string InvestorID { get; set; }
        /// <summary>
        /// 投机套保标志
        /// </summary>
        [XmlAttribute]
        public HedgeType HedgeFlag { get; set; }
        /// <summary>
        /// 多头保证金率
        /// </summary>
        [XmlAttribute]
        public double LongMarginRatioByMoney { get; set; }

        /// <summary>
        /// 多头保证金费
        /// </summary>
        [XmlAttribute]
        public double LongMarginRatioByVolume { get; set; }

        /// <summary>
        /// 空头保证金率
        /// </summary>
        [XmlAttribute]
        public double ShortMarginRatioByMoney { get; set; }

        /// <summary>
        /// 空头保证金费
        /// </summary>
        [XmlAttribute]
        public double ShortMarginRatioByVolume { get; set; }
        /// <summary>
        /// 是否相对交易所收取
        /// </summary>
        [XmlAttribute]
        public bool IsRelative { get; set; }
    }
}
