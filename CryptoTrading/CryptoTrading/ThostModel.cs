/*
 * 对接ThostFtdcUserApiStruct.h中的类
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CryptoTrading
{
    #region 查询相关的数据类

    /// <summary>
    /// 查询投资者结算结果
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class QrySettlementInfoField
    {
        /// <summary>
        /// 经纪商代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 11)]
        public string BrokerID;
        /// <summary>
        /// 投资者代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string InvestorID;
        /// <summary>
        /// 交易日
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string TradingDay;

        public override string ToString()
        {
            return Utility.GetObjectFieldInfo(this);
        }

    }

    /// <summary>
    /// 查询转帐银行
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class QryTransferBankField
    {
        /// <summary>
        /// 银行代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
        public string BankID;

        /// <summary>
        /// 银行分中心代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 5)]
        public string BankBrchID;

        /// <summary>
        /// 格式化输出所有公开字段的信息
        /// </summary>
        /// <returns>所有的公开字段信息(字段名：字段值),以",\t"分割</returns>
        public override string ToString()
        {
            return Utility.GetObjectFieldInfo(this);
        }
    }

    /// <summary>
    /// 请求查询转帐流水
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class QryTransferSerialField
    {
        /// <summary>
        /// 经纪公司代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 11)]
        public string BrokerID;

        /// <summary>
        /// 投资者帐号
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string AccountID;
        /// <summary>
        /// 银行代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
        public string BankID;

        /// <summary>
        /// 币种代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
        public string CurrencyID;

        /// <summary>
        /// 格式化输出所有公开字段的信息
        /// </summary>
        /// <returns>所有的公开字段信息(字段名：字段值),以",\t"分割</returns>
        public override string ToString()
        {
            return Utility.GetObjectFieldInfo(this);
        }
    }

    /// <summary>
    ///请求查询银期签约关系
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class QryAccountregisterField
    {
        /// <summary>
        /// 经纪公司代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 11)]
        public string BrokerID;

        /// <summary>
        /// 投资者帐号
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string AccountID;
        /// <summary>
        /// 银行代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
        public string BankID;

        /// <summary>
        /// 银行分中心代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 5)]
        public string BankBrchID;

        /// <summary>
        /// 币种代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
        public string CurrencyID;

        /// <summary>
        /// 格式化输出所有公开字段的信息
        /// </summary>
        /// <returns>所有的公开字段信息(字段名：字段值),以",\t"分割</returns>
        public override string ToString()
        {
            return Utility.GetObjectFieldInfo(this);
        }
    }

    /// <summary>
    /// 查询签约银行请求
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class QryContractBankField
    {
        /// <summary>
        /// 经纪公司代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 11)]
        public string BrokerID;

        /// <summary>
        /// 银行代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
        public string BankID;

        /// <summary>
        /// 银行分中心代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 5)]
        public string BankBrchID;

        /// <summary>
        /// 格式化输出所有公开字段的信息
        /// </summary>
        /// <returns>所有的公开字段信息(字段名：字段值),以",\t"分割</returns>
        public override string ToString()
        {
            return Utility.GetObjectFieldInfo(this);
        }
    }

    /// <summary>
    /// 查询投资者持仓明细
    /// </summary>
    [StructLayout( LayoutKind.Sequential)]
    public class QryInvestorPositionDetailField
    {
        /// <summary>
        /// 经纪商代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 11)]
        public string BrokerID;
        /// <summary>
        /// 投资者代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string InvestorID;
        /// <summary>
        /// 合约代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 31)]
        public string InstrumentID;

        public override string ToString()
        {
            return Utility.GetObjectFieldInfo(this);
        }
    }


    /// <summary>
    /// 查询合约保证金率
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class QryInstrumentMarginRateField
    {
        /// <summary>
        /// 经纪商代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 11)]
        public string BrokerID;
        /// <summary>
        /// 投资者代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string InvestorID;
        /// <summary>
        /// 合约代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 31)]
        public string InstrumentID;
        /// <summary>
        /// 投机套保标志
        /// </summary>
        public char HedgeFlag;
        public override string ToString()
        {
            return Utility.GetObjectFieldInfo(this);
        }
        
    }
    /// <summary>
    /// 查询手续费率
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class QryInstrumentCommissionRateField
    {
        /// <summary>
        /// 经纪商代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 11)]
        public string BrokerID;
        /// <summary>
        /// 投资者代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string InvestorID;
        /// <summary>
        /// 合约代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 31)]
        public string InstrumentID;

        public override string ToString()
        {
            return Utility.GetObjectFieldInfo(this);
        }
    }


    /// <summary>
    /// 查询经纪公司交易参数
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class QryBrokerTradingParamsField
    {
        /// <summary>
        /// 经纪商代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 11)]
        public string BrokerID;
        /// <summary>
        /// 投资者代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string InvestorID;
        /// <summary>
        /// 币种代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
        public string CurrencyID;
    }

    /// <summary>
    /// 查询经纪公司交易算法
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class QryBrokerTradingAlgosField
    {

        /// <summary>
        /// 经纪商代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 11)]
        public string BrokerID;
        /// <summary>
        /// 投资者代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string ExchangeID;
        /// <summary>
        /// 合约代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 31)]
        public string InstrumentID;
    }

    #endregion

    #region 请求/查询、回报/通知通用的数据类

    /// <summary>
    /// 资金账户口令变更域
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class TradingAccountPasswordUpdateField
    {
        /// <summary>
        /// 经纪公司代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 11)]
        public string BrokerID;

        /// <summary>
        /// 投资者帐号
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string AccountID;

        /// <summary>
        /// 原来的口令
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 41)]
        public string OldPassword;

        /// <summary>
        /// 新的口令
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 41)]
        public string NewPassword;

        /// <summary>
        /// 币种代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
        public string CurrencyID;

        /// <summary>
        /// 格式化输出所有公开字段的信息
        /// </summary>
        /// <returns>所有的公开字段信息(字段名：字段值),以",\t"分割</returns>
        public override string ToString()
        {
            return Utility.GetObjectFieldInfo(this);
        }
    }


    /// <summary>
    ///查询账户信息请求
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class ReqQueryAccountField
    {
        /// <summary>
        /// 业务功能码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 7)]
        public string TradeCode;

        /// <summary>
        /// 银行代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
        public string BankID;

        /// <summary>
        /// 银行分中心代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 5)]
        public string BankBrchID;
        /// <summary>
        /// 经纪公司代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 11)]
        public string BrokerID;

        /// <summary>
        /// 期商分支机构代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 31)]
        public string BrokerBranchID;

        /// <summary>
        /// 交易日期
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string TradeDate;

        /// <summary>
        /// 交易时间
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string TradeTime;

        /// <summary>
        /// 银行流水号
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string BankSerial;

        /// <summary>
        /// 交易系统日期
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string TradingDay;
        /// <summary>
        /// 银期平台消息流水号
        /// </summary>
        public int PlateSerial;
        /// <summary>
        /// 最后分片标志(是最后分片Yes '0'，不是最后分片No '1')
        /// </summary>
        public char LastFragment;
        /// <summary>
        /// 会话号
        /// </summary>
        public int SessionID;
        /// <summary>
        /// 客户姓名
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 51)]
        public string CustomerName;

        /// <summary>
        /// 证件类型
        /// </summary>
        public char IdCardType;

        /// <summary>
        /// 证件号码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 51)]
        public string IdentifiedCardNo;
        /// <summary>
        /// 客户类型
        /// </summary>
        public char CustType;

        /// <summary>
        /// 银行帐号
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 41)]
        public string BankAccount;

        /// <summary>
        ///银行密码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 41)]
        public string BankPassWord;

        /// <summary>
        /// 投资者帐号
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string AccountID;

        /// <summary>
        /// 期货密码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 41)]
        public string Password;

        /// <summary>
        /// 期货公司流水号
        /// </summary>
        public int FutureSerial;

        /// <summary>
        /// 安装编号
        /// </summary>
        public int InstallID;

        /// <summary>
        /// 用户标识
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string UserID;
        /// <summary>
        /// 验证客户证件号码标志('0'是，'1'否)
        /// </summary>
        public char VerifyCertNoFlag;

        /// <summary>
        /// 币种代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
        public string CurrencyID;

        /// <summary>
        /// 摘要
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 36)]
        public string Digest;
        /// <summary>
        /// 银行帐号类型
        /// (银行存折BankBook '1';储蓄卡SavingCard '2';信用卡CreditCard '3')
        /// </summary>
        public char BankAccType;

        /// <summary>
        /// 渠道标志
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 3)]
        public string DeviceID;
        /// <summary>
        /// 期货单位帐号类型
        /// (银行存折BankBook '1';储蓄卡SavingCard '2';信用卡CreditCard '3')
        /// </summary>
        public char BankSecuAccType;

        /// <summary>
        /// 期货公司银行编码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
        public string BrokerIDByBank;

        /// <summary>
        /// 期货单位帐号
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 41)]
        public string BankSecuAcc;
        /// <summary>
        /// 银行密码标志
        /// (不核对NoCheck '0';明文核对BlankCheck '1';密文核对EncryptCheck '2')
        /// </summary>
        public char BankPwdFlag;
        /// <summary>
        /// 期货资金密码核对标志
        /// (不核对NoCheck '0';明文核对BlankCheck '1';密文核对EncryptCheck '2')
        /// </summary>
        public char SecuPwdFlag;

        /// <summary>
        /// 交易柜员
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 17)]
        public string OperNo;
        /// <summary>
        /// 请求编号
        /// </summary>
        public int RequestID;
        /// <summary>
        /// 交易ID
        /// </summary>
        public int TID;

        /// <summary>
        /// 格式化输出所有公开字段的信息
        /// </summary>
        /// <returns>所有的公开字段信息(字段名：字段值),以",\t"分割</returns>
        public override string ToString()
        {
            return Utility.GetObjectFieldInfo(this);
        }
    }


    /// <summary>
    /// 预埋单
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class ParkedOrderField
    {
        /// <summary>
        /// 经纪公司代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 11)]
        public string BrokerID;

        /// <summary>
        /// 投资者代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string InvestorID;

        /// <summary>
        /// 合约代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 31)]
        public string InstrumentID;

        /// <summary>
        /// 报单引用
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string OrderRef;

        /// <summary>
        /// 用户代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string UserID;
        /// <summary>
        /// 报单价格条件/OrderPriceType
        /// </summary>
        public char OrderPriceType;
        /// <summary>
        /// 买卖方向
        /// </summary>
        public char Direction;

        /// <summary>
        /// 组合开平标志
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 5)]
        public string CombOffsetFlag;

        /// <summary>
        /// 组合投机套保标志
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 5)]
        public string CombHedgeFlag;
        /// <summary>
        /// 价格
        /// </summary>
        public double LimitPrice;
        /// <summary>
        /// 数量
        /// </summary>
        public int VolumeTotalOriginal;
        /// <summary>
        /// 有效期类型/TimeConditionType
        /// </summary>
        public char TimeCondition;

        /// <summary>
        /// GTD日期
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string GTDDate;
        /// <summary>
        /// 成交量类型/VolumeConditionType
        /// </summary>       
        public char VolumeCondition;
        /// <summary>
        /// 最小成交量
        /// </summary>
        public int MinVolume;
        /// <summary>
        /// 触发条件/ContingentConditionType
        /// </summary>
        public char ContingentCondition;
        /// <summary>
        /// 止损价
        /// </summary>
        public double StopPrice;
        /// <summary>
        /// 强平原因/ForceCloseReasonType
        /// </summary>       
        public char ForceCloseReason;
        /// <summary>
        /// 自动挂起标志
        /// </summary>
        public int IsAutoSuspend;

        /// <summary>
        /// 业务单元
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 21)]
        public string BusinessUnit;
        /// <summary>
        /// 请求编号
        /// </summary>
        public int RequestID;
        /// <summary>
        /// 用户强评标志
        /// </summary>
        public int UserForceClose;

        /// <summary>
        /// 交易所代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string ExchangeID;

        /// <summary>
        /// 预埋报单编号
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string ParkedOrderID;
        /// <summary>
        /// 用户类型
        /// </summary>
        public char UserType;
        /// <summary>
        /// 预埋单状态/ParkedOrderStatusType
        /// </summary>
        public char Status;
        /// <summary>
        /// 错误代码
        /// </summary>
        public int ErrorID;
        /// <summary>
        /// 错误信息
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 81)]
        public string ErrorMsg;
        /// <summary>
        /// 互换单标志
        /// </summary>
        public int IsSwapOrder;

        public override string ToString()
        {
            return Utility.GetObjectFieldInfo(this);
        }
    }
    /// <summary>
    /// 输入报单
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class InputOrderField
    {
        /// <summary>
        /// 经纪公司代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 11)]
        public string BrokerID;

        /// <summary>
        /// 投资者代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string InvestorID;

        /// <summary>
        /// 合约代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 31)]
        public string InstrumentID;

        /// <summary>
        /// 报单引用
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string OrderRef;

        /// <summary>
        /// 用户代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string UserID;
        /// <summary>
        /// 报单价格条件/OrderPriceType
        /// </summary>
        public char OrderPriceType;
        /// <summary>
        /// 买卖方向
        /// </summary>
        public char TradeDirection;

        /// <summary>
        /// 组合开平标志
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 5)]
        public string CombOffsetFlag;

        /// <summary>
        /// 组合投机套保标志
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 5)]
        public string CombHedgeFlag;
        /// <summary>
        /// 价格
        /// </summary>
        public double LimitPrice;
        /// <summary>
        /// 数量
        /// </summary>
        public int VolumeTotalOriginal;
        /// <summary>
        /// 有效期类型/TimeConditionType
        /// </summary>
        public char TimeCondition;

        /// <summary>
        /// GTD日期
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string GTDDate;
        /// <summary>
        /// 成交量类型/VolumeConditionType
        /// </summary>       
        public char VolumeCondition;
        /// <summary>
        /// 最小成交量
        /// </summary>
        public int MinVolume;
        /// <summary>
        /// 触发条件/ContingentConditionType
        /// </summary>
        public char ContingentCondition;
        /// <summary>
        /// 止损价
        /// </summary>
        public double StopPrice;
        /// <summary>
        /// 强平原因/ForceCloseReasonType
        /// </summary>       
        public char ForceCloseReason;
        /// <summary>
        /// 自动挂起标志
        /// </summary>
        public int IsAutoSuspend;

        /// <summary>
        /// 业务单元
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 21)]
        public string BusinessUnit;
        /// <summary>
        /// 请求编号
        /// </summary>
        public int RequestID;
        /// <summary>
        /// 用户强评标志
        /// </summary>
        public int UserForceClose;
               
        /// <summary>
        /// 互换单标志
        /// </summary>
        public int IsSwapOrder;


        public override string ToString()
        {
            return Utility.GetObjectFieldInfo(this);
        }
    }
   
    #endregion

    #region 请求的相关类
    /// <summary>
    /// 转账请求
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class ReqTransferField
    {
        /// <summary>
        /// 业务功能码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 7)]
        public string TradeCode;

        /// <summary>
        /// 银行代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
        public string BankID;

        /// <summary>
        /// 银行分中心代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 5)]
        public string BankBrchID;
        /// <summary>
        /// 经纪公司代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 11)]
        public string BrokerID;

        /// <summary>
        /// 期商分支机构代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 31)]
        public string BrokerBrchID;

        /// <summary>
        /// 交易日期
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string TradeDate;

        /// <summary>
        /// 交易时间
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string TradeTime;

        /// <summary>
        /// 银行流水号
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string BankSerial;

        /// <summary>
        /// 交易系统日期
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string TradingDay;
        /// <summary>
        /// 银期平台消息流水号
        /// </summary>
        public int PlateSerial;
        /// <summary>
        /// 最后分片标志(是最后分片Yes '0'，不是最后分片No '1')
        /// </summary>
        public char LastFragment;
        /// <summary>
        /// 会话号
        /// </summary>
        public int SessionID;
        /// <summary>
        /// 客户姓名
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 51)]
        public string CustomerName;

        /// <summary>
        /// 证件类型
        /// </summary>
        public char IdCardType;

        /// <summary>
        /// 证件号码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 51)]
        public string IdentifiedCardNo;
        /// <summary>
        /// 客户类型
        /// </summary>
        public char CustType;

        /// <summary>
        /// 银行帐号
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 41)]
        public string BankAccount;

        /// <summary>
        ///银行密码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 41)]
        public string BankPassWord;

        /// <summary>
        /// 投资者帐号
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string AccountID;

        /// <summary>
        /// 期货密码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 41)]
        public string Password;
        /// <summary>
        /// 安装编号
        /// </summary>
        public int InstallID;
        /// <summary>
        /// 期货公司流水号
        /// </summary>
        public int FutureSerial;

        /// <summary>
        /// 用户标识
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string UserID;
        /// <summary>
        /// 验证客户证件号码标志('0'是，'1'否)
        /// </summary>
        public char VerifyCertNoFlag;

        /// <summary>
        /// 币种代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
        public string CurrencyID;
        /// <summary>
        /// 转帐金额
        /// </summary>
        public double TradeAmount;
        /// <summary>
        /// 期货可取金额
        /// </summary>
        public double FutureFetchAmount;
        /// <summary>
        /// 费用支付标志
        ///(由受益方支付费用BEN '0';由发送方支付费用OUR '1';由发送方支付发起的费用，受益方支付接受的费用SHA '2')
        /// </summary>
        public char FeePayFlag;
        /// <summary>
        /// 应收客户费用
        /// </summary>
        public double CustFee;
        /// <summary>
        /// 应收期货公司费用
        /// </summary>
        public double BrokerFee;

        /// <summary>
        /// 发送方给接收方的消息
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 129)]
        public string Message;

        /// <summary>
        /// 摘要
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 36)]
        public string Digest;
        /// <summary>
        /// 银行帐号类型
        /// (银行存折BankBook '1';储蓄卡SavingCard '2';信用卡CreditCard '3')
        /// </summary>
        public char BankAccType;

        /// <summary>
        /// 渠道标志
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 3)]
        public string DeviceID;
        /// <summary>
        /// 期货单位帐号类型
        /// (银行存折BankBook '1';储蓄卡SavingCard '2';信用卡CreditCard '3')
        /// </summary>
        public char BankSecuAccType;

        /// <summary>
        /// 期货公司银行编码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
        public string BrokerIDByBank;

        /// <summary>
        /// 期货单位帐号
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 41)]
        public string BankSecuAcc;
        /// <summary>
        /// 银行密码标志
        /// (不核对NoCheck '0';明文核对BlankCheck '1';密文核对EncryptCheck '2')
        /// </summary>
        public char BankPwdFlag;
        /// <summary>
        /// 期货资金密码核对标志
        /// (不核对NoCheck '0';明文核对BlankCheck '1';密文核对EncryptCheck '2')
        /// </summary>
        public char SecuPwdFlag;

        /// <summary>
        /// 交易柜员
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 17)]
        public string OperNo;
        /// <summary>
        /// 请求编号
        /// </summary>
        public int RequestID;
        /// <summary>
        /// 交易ID
        /// </summary>
        public int TID;
        /// <summary>
        /// 转账交易状态(正常Normal '0';被冲正Repealed '1')
        /// </summary>
        public char TransferStatus;

        /// <summary>
        /// 格式化输出所有公开字段的信息
        /// </summary>
        /// <returns>所有的公开字段信息(字段名：字段值),以",\t"分割</returns>
        public override string ToString()
        {
            return Utility.GetObjectFieldInfo(this);
        }
    }


    /// <summary>
    /// 冲正请求
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class ReqRepealField
    {
        /// <summary>
        /// 冲正时间间隔
        /// </summary>
        public int RepealTimeInterval;
        /// <summary>
        /// 已经冲正次数
        /// </summary>
        public int RepealedTimes;
        /// <summary>
        /// 银行冲正标志
        /// (银行无需自动冲正BankNotNeedRepeal '0';银行待自动冲正BankWaitingRepeal '1';银行已自动冲正BankBeenRepealed '2')
        /// </summary>
        public char BankRepealFlag;
        /// <summary>
        /// 期商冲正标志
        /// (期商无需自动冲正BrokerNotNeedRepeal '0';期商待自动冲正BrokerWaitingRepeal '1';期商已自动冲正BrokerBeenRepealed '2')
        /// </summary>
        public char BrokerRepealFlag;
        /// <summary>
        /// 被冲正平台流水号
        /// </summary>
        public int PlateRepealSerial;

        /// <summary>
        /// 被冲正银行流水号
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string BankRepealSerial;
        /// <summary>
        /// 被冲正期货流水号
        /// </summary>
        public int FutureRepealSerial;
        /// <summary>
        /// 业务功能码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 7)]
        public string TradeCode;
        /// <summary>
        /// 银行代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
        public string BankID;
        /// <summary>
        /// 银行分支机构代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 5)]
        public string BankBranchID;
        /// <summary>
        /// 期商代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 11)]
        public string BrokerID;
        /// <summary>
        /// 期商分支机构代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 31)]
        public string BrokerBranchID;
        /// <summary>
        /// 交易日期
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string TradeDate;
        /// <summary>
        /// 交易时间
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string TradeTime;
        /// <summary>
        /// 银行流水号
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string BankSerial;
        /// <summary>
        /// 交易系统日期
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string TradingDay;
        /// <summary>
        /// 银期平台消息流水号
        /// </summary>
        public int PlateSerial;
        /// <summary>
        /// 最后分片标志(是最后分片Yes '0';不是最后分片No '1')
        /// </summary>
        public char LastFragment;
        /// <summary>
        /// 会话号
        /// </summary>
        public int SessionID;
        /// <summary>
        /// 客户姓名
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 51)]
        public string CustomerName;
        /// <summary>
        /// 证件类型
        /// </summary>
        public char IdCardType;
        /// <summary>
        /// 证件号码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 51)]
        public string IdentifiedCardNo;
        /// <summary>
        /// 客户类型
        /// </summary>
        public char CustType;
        /// <summary>
        /// 银行帐号
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 41)]
        public string BankAccount;
        /// <summary>
        /// 银行密码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 41)]
        public string BankPassWord;
        /// <summary>
        /// 投资者帐号
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string AccountID;

        /// <summary>
        /// 期货密码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 41)]
        public string Password;
        /// <summary>
        /// 安装编号
        /// </summary>
        public int InstallID;
        /// <summary>
        /// 期货公司流水号
        /// </summary>
        public int FutureSerial;

        /// <summary>
        /// 用户标识
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string UserID;
        /// <summary>
        /// 验证客户证件号码标志(0是，1否)
        /// </summary>
        public char VerifyCertNoFlag;

        /// <summary>
        /// 币种代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
        public string CurrencyID;
        /// <summary>
        /// 转帐金额
        /// </summary>
        public double TradeAmount;
        /// <summary>
        /// 期货可取金额
        /// </summary>
        public double FutureFetchAmount;
        /// <summary>
        /// 费用支付标志
        /// (由受益方支付费用BEN '0';由发送方支付费用OUR '1';由发送方支付发起的费用，受益方支付接受的费用SHA '2')
        /// </summary>
        public char FeePayFlag;
        /// <summary>
        /// 应收客户费用
        /// </summary>
        public double CustFee;
        /// <summary>
        /// 应收期货公司费用
        /// </summary>
        public double BrokerFee;
        /// <summary>
        /// 发送方给接收方的消息
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 129)]
        public string Message;

        /// <summary>
        /// 摘要
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 36)]
        public string Digest;
        /// <summary>
        /// 银行帐号类型
        /// </summary>
        public char BankAccType;
        /// <summary>
        /// 渠道标志
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 3)]
        public string DeviceID;
        /// <summary>
        /// 期货单位帐号类型
        /// </summary>
        public char BankSecuAccType;

        /// <summary>
        /// 期货公司银行编码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
        public string BrokerIDByBank;
        /// <summary>
        /// 期货单位帐号
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 41)]
        public string BankSecuAcc;
        /// <summary>
        /// 银行密码标志
        /// </summary>
        public char BankPwdFlag;
        /// <summary>
        /// 期货资金密码核对标志
        /// </summary>
        public char SecuPwdFlag;

        /// <summary>
        /// 交易柜员
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 17)]
        public string OperNo;
        /// <summary>
        /// 请求编号
        /// </summary>
        public int RequestID;
        /// <summary>
        /// 交易ID
        /// </summary>
        public int TID;
        /// <summary>
        /// 转账交易状态(正常Normal '0';被冲正Repealed '1')
        /// </summary>
        public char TransferStatus;

        /// <summary>
        /// 格式化输出所有公开字段的信息
        /// </summary>
        /// <returns>所有的公开字段信息(字段名：字段值),以",\t"分割</returns>
        public override string ToString()
        {
            return Utility.GetObjectFieldInfo(this);
        }
    };

    /// <summary>
    /// 查询预埋单
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class QryParkedOrderField
    {
        /// <summary>
        /// 经纪公司代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 11)]
        public string BrokerID;

        /// <summary>
        /// 投资者代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string InvestorID;
        /// <summary>
        /// 合约代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 31)]
        public string InstrumentID;
        /// <summary>
        /// 交易所代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 6)]
        public string ExchangeID;
    }

    /// <summary>
    /// 查询预埋撤单
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class QryParkedOrderActionField
    {
        /// <summary>
        /// 经纪公司代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 11)]
        public string BrokerID;

        /// <summary>
        /// 投资者代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string InvestorID;
        /// <summary>
        /// 合约代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 31)]
        public string InstrumentID;
        /// <summary>
        /// 交易所代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 6)]
        public string ExchangeID;
        /// <summary>
        /// 格式化输出所有公开字段的信息
        /// </summary>
        /// <returns>所有的公开字段信息(字段名：字段值),以",\t"分割</returns>
        public override string ToString()
        {
            return Utility.GetObjectFieldInfo(this);
        }
    }
    
    /// <summary>
    /// 查询交易事件通知
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class QryTradingNoticeField
    {
        /// <summary>
        /// 经纪公司代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 11)]
        public string BrokerID;

        /// <summary>
        /// 投资者代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string InvestorID;
        /// <summary>
        /// 格式化输出所有公开字段的信息
        /// </summary>
        /// <returns>所有的公开字段信息(字段名：字段值),以",\t"分割</returns>
        public override string ToString()
        {
            return Utility.GetObjectFieldInfo(this);
        }
    }
    #endregion

    #region 回报或通知相关数据类

    /// <summary>
    /// 投资者结算结果
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class SettlementInfoField
    {
        ///交易日
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string TradingDay;

        /// <summary>
        /// 结算编号
        /// </summary>
        public int SettlementID;

        /// <summary>
        /// 经纪商代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 11)]
        public string BrokerID;
        /// <summary>
        /// 投资者代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string InvestorID;
        /// <summary>
        /// 序号
        /// </summary>
        public int SequenceNo;
        /// <summary>
        /// 消息正文
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 501)]
        public string Content;

        /// <summary>
        /// 格式化输出所有公开字段的信息
        /// </summary>
        /// <returns>所有的公开字段信息(字段名：字段值),以",\t"分割</returns>
        public override string ToString()
        {
            return Utility.GetObjectFieldInfo(this);
        }
    }

    /// <summary>
    /// 投资者持仓明细
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class InvestorPositionDetailField
    {
        /// <summary>
        /// 合约代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 31)]
        public string InstrumentID;
        /// <summary>
        /// 经纪公司代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 11)]
        public string BrokerID;
        /// <summary>
        /// 投资者代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string InvestorID;
        /// <summary>
        /// 投机套保标志('1'/投机，'2'/套利，'3'/套保，)
        /// </summary>
        public char HedgeFlag;
        /// <summary>
        /// 买卖('0'/买，'1'/卖)
        /// </summary>
        public char Direction;
        /// <summary>
        /// 开仓日期
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string OpenDate;
        /// <summary>
        /// 成交编号
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 21)]
        public string TradeID;
        /// <summary>
        /// 数量
        /// </summary>
        public int Volume;
        /// <summary>
        /// 开仓价
        /// </summary>
        public double OpenPrice;
        /// <summary>
        /// 交易日
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string TradingDay;
        /// <summary>
        /// 结算编号
        /// </summary>
        public int SettlementID;
        /// <summary>
        /// 成交类型
        /// '#'组合持仓拆分为单一持仓,初始化不应包含该类型的持仓
        ///'0'普通成交
        ///'1'期权执行
        ///'2'OTC成交
        ///'3'期转现衍生成交
        ///'4'组合衍生成交
        /// </summary>
        public char TradeType;
        /// <summary>
        /// 组合合约代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 31)]
        public string CombInstrumentID;
        /// <summary>
        /// 交易所代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string ExchangeID;
        /// <summary>
        /// 逐日盯市平仓盈亏
        /// </summary>
        public double CloseProfitByDate;
        /// <summary>
        /// 逐笔对冲平仓盈亏
        /// </summary>
        public double CloseProfitByTrade;
        /// <summary>
        /// 逐日盯市持仓盈亏
        /// </summary>
        public double PositionProfitByDate;
        /// <summary>
        /// 逐笔对冲持仓盈亏
        /// </summary>
        public double PositionProfitByTrade;
        /// <summary>
        /// 投资者保证金
        /// </summary>
        public double Margin;
        /// <summary>
        /// 交易所保证金
        /// </summary>
        public double ExchMargin;
        /// <summary>
        /// 保证金率
        /// </summary>
        public double MarginRateByMoney;
        /// <summary>
        /// 保证金率(按手数)
        /// </summary>
        public double MarginRateByVolume;
        /// <summary>
        /// 昨结算价
        /// </summary>
        public double LastSettlementPrice;
        /// <summary>
        /// 结算价
        /// </summary>
        public double SettlementPrice;
        /// <summary>
        /// 平仓量
        /// </summary>
        public int CloseVolume;
        /// <summary>
        /// 平仓金额
        /// </summary>
        public double CloseAmount;

        /// <summary>
        /// 格式化输出所有公开字段的信息
        /// </summary>
        /// <returns>所有的公开字段信息(字段名：字段值),以",\t"分割</returns>
        public override string ToString()
        {
            return Utility.GetObjectFieldInfo(this);
        }
    }

    /// <summary>
    /// 转帐银行
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class TransferBankField
    {
        /// <summary>
        /// 银行代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
        public string BankID;

        /// <summary>
        /// 银行分中心代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 5)]
        public string BankBrchID;

        /// <summary>
        /// 银行名称
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 101)]
        public string BankName;

        /// <summary>
        /// 是否活跃
        /// </summary>
        public int IsActive;

        /// <summary>
        /// 格式化输出所有公开字段的信息
        /// </summary>
        /// <returns>所有的公开字段信息(字段名：字段值),以",\t"分割</returns>
        public override string ToString()
        {
            return Utility.GetObjectFieldInfo(this);
        }
    }


    /// <summary>
    /// 银期转账交易流水表
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class TransferSerialField
    {
        /// <summary>
        /// 平台流水号
        /// </summary>
        public int PlateSerial;

        /// <summary>
        /// 交易发起方日期
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string TradeDate;

        /// <summary>
        /// 交易日期
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string TradingDay;

        /// <summary>
        /// 交易时间
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string TradeTime;

        /// <summary>
        /// 交易代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 7)]
        public string TradeCode;
        /// <summary>
        /// 会话编号
        /// </summary>
        public int SessionID;

        /// <summary>
        /// 银行编码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
        public string BankID;

        /// <summary>
        /// 银行分支机构编码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 5)]
        public string BankBranchID;

        /// <summary>
        /// 银行帐号类型
        /// </summary>
        public char BankAccType;

        /// <summary>
        /// 银行帐号
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 41)]
        public string BankAccount;

        /// <summary>
        /// 银行流水号
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string BankSerial;

        /// <summary>
        /// 期货公司编码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 11)]
        public string BrokerID;

        /// <summary>
        /// 期商分支机构代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 31)]
        public string BrokerBranchID;
        /// <summary>
        /// 期货公司帐号类型
        /// </summary>
        public char FutureAccType;

        /// <summary>
        /// 投资者帐号
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string AccountID;

        /// <summary>
        /// 投资者代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string InvestorID;
        /// <summary>
        /// 期货公司流水号
        /// </summary>
        public int FutureSerial;
        /// <summary>
        /// 证件类型
        /// </summary>
        public char IdCardType;

        /// <summary>
        /// 证件号码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 51)]
        public string IdentifiedCardNo;

        /// <summary>
        /// 币种代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
        public string CurrencyID;
        /// <summary>
        /// 交易金额
        /// </summary>
        public double TradeAmount;
        /// <summary>
        /// 应收客户费用
        /// </summary>
        public double CustFee;
        /// <summary>
        /// 应收期货公司费用
        /// </summary>
        public double BrokerFee;
        /// <summary>
        /// 有效标志
        /// </summary>
        public char AvailabilityFlag;

        /// <summary>
        /// 操作员
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 17)]
        public string OperatorCode;

        /// <summary>
        /// 新银行帐号
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 41)]
        public string BankNewAccount;
        /// <summary>
        /// 错误代码
        /// </summary>
        public int ErrorID;

        /// <summary>
        /// 错误信息
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 81)]
        public string ErrorMsg;

        /// <summary>
        /// 格式化输出所有公开字段的信息
        /// </summary>
        /// <returns>所有的公开字段信息(字段名：字段值),以",\t"分割</returns>
        public override string ToString()
        {
            return Utility.GetObjectFieldInfo(this);
        }
    }

    /// <summary>
    /// 银行发起银行资金转期货响应
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class RspTransferField
    {
        /// <summary>
        /// 业务功能码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 7)]
        public string TradeCode;

        /// <summary>
        /// 银行代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
        public string BankID;

        /// <summary>
        /// 银行分中心代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 5)]
        public string BankBrchID;
        /// <summary>
        /// 经纪公司代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 11)]
        public string BrokerID;

        /// <summary>
        /// 期商分支机构代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 31)]
        public string BrokerBranchID;

        /// <summary>
        /// 交易日期
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string TradeDate;

        /// <summary>
        /// 交易时间
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string TradeTime;

        /// <summary>
        /// 银行流水号
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string BankSerial;

        /// <summary>
        /// 交易系统日期
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string TradingDay;
        /// <summary>
        /// 银期平台消息流水号
        /// </summary>
        public int PlateSerial;
        /// <summary>
        /// 最后分片标志(是最后分片Yes '0'，不是最后分片No '1')
        /// </summary>
        public char LastFragment;
        /// <summary>
        /// 会话号
        /// </summary>
        public int SessionID;
        /// <summary>
        /// 客户姓名
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 51)]
        public string CustomerName;

        /// <summary>
        /// 证件类型
        /// </summary>
        public char IdCardType;

        /// <summary>
        /// 证件号码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 51)]
        public string IdentifiedCardNo;
        /// <summary>
        /// 客户类型
        /// </summary>
        public char CustType;

        /// <summary>
        /// 银行帐号
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 41)]
        public string BankAccount;

        /// <summary>
        ///银行密码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 41)]
        public string BankPassWord;

        /// <summary>
        /// 投资者帐号
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string AccountID;

        /// <summary>
        /// 期货密码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 41)]
        public string Password;
        /// <summary>
        /// 安装编号
        /// </summary>
        public int InstallID;
        /// <summary>
        /// 期货公司流水号
        /// </summary>
        public int FutureSerial;

        /// <summary>
        /// 用户标识
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string UserID;
        /// <summary>
        /// 验证客户证件号码标志('0'是，'1'否)
        /// </summary>
        public char VerifyCertNoFlag;

        /// <summary>
        /// 币种代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
        public string CurrencyID;
        /// <summary>
        /// 转帐金额
        /// </summary>
        public double TradeAmount;
        /// <summary>
        /// 期货可取金额
        /// </summary>
        public double FutureFetchAmount;
        /// <summary>
        /// 费用支付标志
        ///(由受益方支付费用BEN '0';由发送方支付费用OUR '1';由发送方支付发起的费用，受益方支付接受的费用SHA '2')
        /// </summary>
        public char FeePayFlag;
        /// <summary>
        /// 应收客户费用
        /// </summary>
        public double CustFee;
        /// <summary>
        /// 应收期货公司费用
        /// </summary>
        public double BrokerFee;

        /// <summary>
        /// 发送方给接收方的消息
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 129)]
        public string Message;

        /// <summary>
        /// 摘要
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 36)]
        public string Digest;
        /// <summary>
        /// 银行帐号类型
        /// (银行存折BankBook '1';储蓄卡SavingCard '2';信用卡CreditCard '3')
        /// </summary>
        public char BankAccType;

        /// <summary>
        /// 渠道标志
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 3)]
        public string DeviceID;
        /// <summary>
        /// 期货单位帐号类型
        /// (银行存折BankBook '1';储蓄卡SavingCard '2';信用卡CreditCard '3')
        /// </summary>
        public char BankSecuAccType;

        /// <summary>
        /// 期货公司银行编码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
        public string BrokerIDByBank;

        /// <summary>
        /// 期货单位帐号
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 41)]
        public string BankSecuAcc;
        /// <summary>
        /// 银行密码标志
        /// (不核对NoCheck '0';明文核对BlankCheck '1';密文核对EncryptCheck '2')
        /// </summary>
        public char BankPwdFlag;
        /// <summary>
        /// 期货资金密码核对标志
        /// (不核对NoCheck '0';明文核对BlankCheck '1';密文核对EncryptCheck '2')
        /// </summary>
        public char SecuPwdFlag;

        /// <summary>
        /// 交易柜员
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 17)]
        public string OperNo;
        /// <summary>
        /// 请求编号
        /// </summary>
        public int RequestID;
        /// <summary>
        /// 交易ID
        /// </summary>
        public int TID;
        /// <summary>
        /// 转账交易状态(正常Normal '0';被冲正Repealed '1')
        /// </summary>
        public char TransferStatus;
        /// <summary>
        /// 错误代码
        /// </summary>
        public int ErrorID;
        /// <summary>
        /// 错误信息
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 81)]
        public string ErrorMsg;

        /// <summary>
        /// 格式化输出所有公开字段的信息
        /// </summary>
        /// <returns>所有的公开字段信息(字段名：字段值),以",\t"分割</returns>
        public override string ToString()
        {
            return Utility.GetObjectFieldInfo(this);
        }
    }
    /// <summary>
    ///客户开销户信息表
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class AccountregisterField
    {
        /// <summary>
        /// 交易日期
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string TradeDay;

        /// <summary>
        /// 银行编码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
        public string BankID;

        /// <summary>
        /// 银行分支机构编码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 5)]
        public string BankBranchID;

        /// <summary>
        /// 银行帐号
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 41)]
        public string BankAccount;

        /// <summary>
        /// 期货公司编码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 11)]
        public string BrokerID;

        /// <summary>
        /// 期货公司分支机构编码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 31)]
        public string BrokerBranchID;

        /// <summary>
        /// 投资者帐号
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string AccountID;
        /// <summary>
        /// 证件类型
        /// </summary>
        public char IdCardType;

        /// <summary>
        /// 证件号码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 51)]
        public string IdentifiedCardNo;

        /// <summary>
        /// 客户姓名
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 51)]
        public string CustomerName;

        /// <summary>
        /// 币种代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
        public string CurrencyID;
        /// <summary>
        /// 开销户类别
        /// (开户Open '1';销户Destroy '0')
        /// </summary>
        public char OpenOrDestroy;

        /// <summary>
        /// 签约日期
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string RegDate;

        /// <summary>
        /// 解约日期
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string OutDate;
        /// <summary>
        /// 交易ID
        /// </summary>
        public int TID;
        /// <summary>
        /// 客户类型
        /// (自然人Person '0';机构户Institution '1')
        /// </summary>
        public char CustType;
        /// <summary>
        /// 银行帐号类型
        /// </summary>
        public char BankAccType;

        /// <summary>
        /// 格式化输出所有公开字段的信息
        /// </summary>
        /// <returns>所有的公开字段信息(字段名：字段值),以",\t"分割</returns>
        public override string ToString()
        {
            return Utility.GetObjectFieldInfo(this);
        }
    }

    /// <summary>
    /// 查询签约银行响应
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class ContractBankField
    {
        /// <summary>
        /// 经纪公司代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 11)]
        public string BrokerID;

        /// <summary>
        /// 银行代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
        public string BankID;

        /// <summary>
        /// 银行分中心代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 5)]
        public string BankBrchID;

        /// <summary>
        /// 银行名称
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 101)]
        public string BankName;

        /// <summary>
        /// 格式化输出所有公开字段的信息
        /// </summary>
        /// <returns>所有的公开字段信息(字段名：字段值),以",\t"分割</returns>
        public override string ToString()
        {
            return Utility.GetObjectFieldInfo(this);
        }
    }

    /// <summary>
    /// 冲正响应
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class RspRepealField
    {
        /// <summary>
        /// 冲正时间间隔
        /// </summary>
        public int RepealTimeInterval;
        /// <summary>
        /// 已经冲正次数
        /// </summary>
        public int RepealedTimes;
        /// <summary>
        /// 银行冲正标志
        /// (银行无需自动冲正BankNotNeedRepeal '0';银行待自动冲正BankWaitingRepeal '1';银行已自动冲正BankBeenRepealed '2')
        /// </summary>
        public char BankRepealFlag;
        /// <summary>
        /// 期商冲正标志
        /// (期商无需自动冲正BrokerNotNeedRepeal '0';期商待自动冲正BrokerWaitingRepeal '1';期商已自动冲正BrokerBeenRepealed '2')
        /// </summary>
        public char BrokerRepealFlag;
        /// <summary>
        /// 被冲正平台流水号
        /// </summary>
        public int PlateRepealSerial;

        /// <summary>
        /// 被冲正银行流水号
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string BankRepealSerial;
        /// <summary>
        /// 被冲正期货流水号
        /// </summary>
        public int FutureRepealSerial;
        /// <summary>
        /// 业务功能码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 7)]
        public string TradeCode;
        /// <summary>
        /// 银行代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
        public string BankID;
        /// <summary>
        /// 银行分支机构代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 5)]
        public string BankBranchID;
        /// <summary>
        /// 期商代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 11)]
        public string BrokerID;
        /// <summary>
        /// 期商分支机构代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 31)]
        public string BrokerBranchID;
        /// <summary>
        /// 交易日期
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string TradeDate;
        /// <summary>
        /// 交易时间
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string TradeTime;
        /// <summary>
        /// 银行流水号
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string BankSerial;
        /// <summary>
        /// 交易系统日期
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string TradingDay;
        /// <summary>
        /// 银期平台消息流水号
        /// </summary>
        public int PlateSerial;
        /// <summary>
        /// 最后分片标志(是最后分片Yes '0';不是最后分片No '1')
        /// </summary>
        public char LastFragment;
        /// <summary>
        /// 会话号
        /// </summary>
        public int SessionID;
        /// <summary>
        /// 客户姓名
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 51)]
        public string CustomerName;
        /// <summary>
        /// 证件类型
        /// </summary>
        public char IdCardType;
        /// <summary>
        /// 证件号码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 51)]
        public string IdentifiedCardNo;
        /// <summary>
        /// 客户类型
        /// </summary>
        public char CustType;
        /// <summary>
        /// 银行帐号
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 41)]
        public string BankAccount;
        /// <summary>
        /// 银行密码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 41)]
        public string BankPassWord;
        /// <summary>
        /// 投资者帐号
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string AccountID;

        /// <summary>
        /// 期货密码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 41)]
        public string Password;
        /// <summary>
        /// 安装编号
        /// </summary>
        public int InstallID;
        /// <summary>
        /// 期货公司流水号
        /// </summary>
        public int FutureSerial;

        /// <summary>
        /// 用户标识
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string UserID;
        /// <summary>
        /// 验证客户证件号码标志(0是，1否)
        /// </summary>
        public char VerifyCertNoFlag;

        /// <summary>
        /// 币种代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
        public string CurrencyID;
        /// <summary>
        /// 转帐金额
        /// </summary>
        public double TradeAmount;
        /// <summary>
        /// 期货可取金额
        /// </summary>
        public double FutureFetchAmount;
        /// <summary>
        /// 费用支付标志
        /// (由受益方支付费用BEN '0';由发送方支付费用OUR '1';由发送方支付发起的费用，受益方支付接受的费用SHA '2')
        /// </summary>
        public char FeePayFlag;
        /// <summary>
        /// 应收客户费用
        /// </summary>
        public double CustFee;
        /// <summary>
        /// 应收期货公司费用
        /// </summary>
        public double BrokerFee;
        /// <summary>
        /// 发送方给接收方的消息
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 129)]
        public string Message;

        /// <summary>
        /// 摘要
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 36)]
        public string Digest;
        /// <summary>
        /// 银行帐号类型
        /// </summary>
        public char BankAccType;
        /// <summary>
        /// 渠道标志
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 3)]
        public string DeviceID;
        /// <summary>
        /// 期货单位帐号类型
        /// </summary>
        public char BankSecuAccType;

        /// <summary>
        /// 期货公司银行编码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
        public string BrokerIDByBank;
        /// <summary>
        /// 期货单位帐号
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 41)]
        public string BankSecuAcc;
        /// <summary>
        /// 银行密码标志
        /// </summary>
        public char BankPwdFlag;
        /// <summary>
        /// 期货资金密码核对标志
        /// </summary>
        public char SecuPwdFlag;

        /// <summary>
        /// 交易柜员
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 17)]
        public string OperNo;
        /// <summary>
        /// 请求编号
        /// </summary>
        public int RequestID;
        /// <summary>
        /// 交易ID
        /// </summary>
        public int TID;
        /// <summary>
        /// 转账交易状态(正常Normal '0';被冲正Repealed '1')
        /// </summary>
        public char TransferStatus;
        /// <summary>
        /// 错误代码
        /// </summary>
        public int ErrorID;
        /// <summary>
        /// 错误信息
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 81)]
        public string ErrorMsg;

        /// <summary>
        /// 格式化输出所有公开字段的信息
        /// </summary>
        /// <returns>所有的公开字段信息(字段名：字段值),以",\t"分割</returns>
        public override string ToString()
        {
            return Utility.GetObjectFieldInfo(this);
        }
    }

    /// <summary>
    /// 查询账户信息通知
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class NotifyQueryAccountField
    {
        /// <summary>
        /// 业务功能码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 7)]
        public string TradeCode;
        /// <summary>
        /// 银行代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
        public string BankID;
        /// <summary>
        /// 银行分支机构代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 5)]
        public string BankBranchID;
        /// <summary>
        /// 期商代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 11)]
        public string BrokerID;
        /// <summary>
        /// 期商分支机构代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 31)]
        public string BrokerBranchID;
        /// <summary>
        /// 交易日期
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string TradeDate;
        /// <summary>
        /// 交易时间
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string TradeTime;
        /// <summary>
        /// 银行流水号
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string BankSerial;
        /// <summary>
        /// 交易系统日期
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string TradingDay;
        /// <summary>
        /// 银期平台消息流水号
        /// </summary>
        public int PlateSerial;
        /// <summary>
        /// 最后分片标志(是最后分片Yes '0';不是最后分片No '1')
        /// </summary>
        public char LastFragment;
        /// <summary>
        /// 会话号
        /// </summary>
        public int SessionID;
        /// <summary>
        /// 客户姓名
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 51)]
        public string CustomerName;
        /// <summary>
        /// 证件类型
        /// </summary>
        public char IdCardType;
        /// <summary>
        /// 证件号码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 51)]
        public string IdentifiedCardNo;
        /// <summary>
        /// 客户类型
        /// </summary>
        public char CustType;
        /// <summary>
        /// 银行帐号
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 41)]
        public string BankAccount;
        /// <summary>
        /// 银行密码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 41)]
        public string BankPassWord;
        /// <summary>
        /// 投资者帐号
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string AccountID;

        /// <summary>
        /// 期货密码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 41)]
        public string Password;

        /// <summary>
        /// 期货公司流水号
        /// </summary>
        public int FutureSerial;
        /// <summary>
        /// 安装编号
        /// </summary>
        public int InstallID;
        /// <summary>
        /// 用户标识
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string UserID;
        /// <summary>
        /// 验证客户证件号码标志(0是，1否)
        /// </summary>
        public char VerifyCertNoFlag;

        /// <summary>
        /// 币种代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
        public string CurrencyID;

        /// <summary>
        /// 摘要
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 36)]
        public string Digest;
        /// <summary>
        /// 银行帐号类型
        /// </summary>
        public char BankAccType;
        /// <summary>
        /// 渠道标志
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 3)]
        public string DeviceID;
        /// <summary>
        /// 期货单位帐号类型
        /// </summary>
        public char BankSecuAccType;

        /// <summary>
        /// 期货公司银行编码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
        public string BrokerIDByBank;
        /// <summary>
        /// 期货单位帐号
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 41)]
        public string BankSecuAcc;
        /// <summary>
        /// 银行密码标志
        /// </summary>
        public char BankPwdFlag;
        /// <summary>
        /// 期货资金密码核对标志
        /// </summary>
        public char SecuPwdFlag;

        /// <summary>
        /// 交易柜员
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 17)]
        public string OperNo;
        /// <summary>
        /// 请求编号
        /// </summary>
        public int RequestID;
        /// <summary>
        /// 交易ID
        /// </summary>
        public int TID;
        /// <summary>
        /// 转账交易状态(正常Normal '0';被冲正Repealed '1')
        /// </summary>
        public char TransferStatus;
        /// <summary>
        /// 银行可用金额
        /// </summary>
        public double BankUseAmount;
        /// <summary>
        /// 银行可取金额
        /// </summary>
        public double BankFetchAmount;
        /// <summary>
        /// 错误代码
        /// </summary>
        public int ErrorID;
        /// <summary>
        /// 错误信息
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 81)]
        public string ErrorMsg;

        /// <summary>
        /// 格式化输出所有公开字段的信息
        /// </summary>
        /// <returns>所有的公开字段信息(字段名：字段值),以",\t"分割</returns>
        public override string ToString()
        {
            return Utility.GetObjectFieldInfo(this);
        }
    }
    /// <summary>
    /// 输入预埋单操作
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class ParkedOrderActionField
    {
        /// <summary>
        /// 经纪公司代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 11)]
        public string BrokerID;
        /// <summary>
        /// 投资者代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string InvestorID;
        /// <summary>
        /// 报单操作引用
        /// </summary>
        public int OrderActionRef;
        /// <summary>
        /// 报单引用
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string OrderRef;
        /// <summary>
        /// 请求编号
        /// </summary>
        public int RequestID;
        /// <summary>
        /// 前置编号
        /// </summary>
        public int FrontID;

        /// <summary>
        /// 会话编号
        /// </summary>
        public int SessionID;
        /// <summary>
        /// 交易所代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string ExchangeID;
        /// <summary>
        /// 报单编号
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 21)]
        public string OrderSysID;
        /// <summary>
        /// 操作标志
        /// </summary>  
        public char ActionFlag;
        /// <summary>
        /// 价格
        /// </summary>
        public double LimitPrice;
        /// <summary>
        /// 数量变化
        /// </summary>
        public int VolumeChange;
        /// <summary>
        /// 用户代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string UserID;

        /// <summary>
        /// 合约代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 31)]
        public string InstrumentID;
        /// <summary>
        /// 预埋撤单单编号
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string ParkedOrderActionID;
        ///用户类型
        //////TFtdcUserTypeType是一个用户类型类型
        /////////////////////////////////////////////////////////////////////////
        ///投资者Investor '0'
        ///操作员Operator '1'
        ///管理员SuperUser '2'
        public char UserType;
        ///预埋撤单状态
        ///TFtdcParkedOrderStatusType是一个预埋单状态类型
        ///未发送NotSend '1'
        ///已发送Send '2'
        ///已删除Deleted '3'
        public char Status;

        ///错误代码
        public int ErrorID;
        /// <summary>
        /// 错误信息
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 81)]
        public string ErrorMsg;

        /// <summary>
        /// 格式化输出所有公开字段的信息
        /// </summary>
        /// <returns>所有的公开字段信息(字段名：字段值),以",\t"分割</returns>
        public override string ToString()
        {
            return Utility.GetObjectFieldInfo(this);
        }
    }

    /// <summary>
    /// 删除预埋单
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class RemoveParkedOrderField
    {
        /// <summary>
        /// 经纪公司代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 11)]
        public string BrokerID;
        /// <summary>
        /// 投资者代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string InvestorID;

        /// <summary>
        /// 预埋报单编号
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string ParkedOrderID;

        /// <summary>
        /// 格式化输出所有公开字段的信息
        /// </summary>
        /// <returns>所有的公开字段信息(字段名：字段值),以",\t"分割</returns>
        public override string ToString()
        {
            return Utility.GetObjectFieldInfo(this);
        }
    }

    /// <summary>
    /// 删除预埋撤单
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class RemoveParkedOrderActionField
    {
        /// <summary>
        /// 经纪公司代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 11)]
        public string BrokerID;
        /// <summary>
        /// 投资者代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string InvestorID;

        /// <summary>
        /// 预埋撤单编号
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string ParkedOrderActionID;

        /// <summary>
        /// 格式化输出所有公开字段的信息
        /// </summary>
        /// <returns>所有的公开字段信息(字段名：字段值),以",\t"分割</returns>
        public override string ToString()
        {
            return Utility.GetObjectFieldInfo(this);
        }
    }

    /// <summary>
    ///用户事件通知信息
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class TradingNoticeInfoField
    {
        /// <summary>
        /// 经纪公司代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 11)]
        public string BrokerID;

        /// <summary>
        /// 投资者代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string InvestorID;

        /// <summary>
        /// 发送时间
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string SendTime;

        /// <summary>
        /// 消息正文
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 501)]
        public string FieldContent;
        /// <summary>
        /// 序列系列号
        /// </summary>
        public short SequenceSeries;
        /// <summary>
        /// 序列号
        /// </summary>
        public int SequenceNo;

        /// <summary>
        /// 格式化输出所有公开字段的信息
        /// </summary>
        /// <returns>所有的公开字段信息(字段名：字段值),以",\t"分割</returns>
        public override string ToString()
        {
            return Utility.GetObjectFieldInfo(this);
        }
    }
    /// <summary>
    /// 客户通知
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class NoticeField
    {
        /// <summary>
        /// 经纪公司代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 11)]
        public string BrokerID;

        /// <summary>
        /// 消息正文
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 501)]
        public string Content;

        /// <summary>
        /// 经纪公司通知内容序列号
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 2)]
        public string SequenceLabel;
        /// <summary>
        /// 格式化输出所有公开字段的信息
        /// </summary>
        /// <returns>所有的公开字段信息(字段名：字段值),以",\t"分割</returns>
        public override string ToString()
        {
            return Utility.GetObjectFieldInfo(this);
        }
    }

    /// <summary>
    /// 合约保证金率
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class InstrumentMarginRateField
    {
        /// <summary>
        /// 合约代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 31)]
        public string InstrumentID;
        ///投资者范围
        public char InvestorRange;
        /// <summary>
        /// 经纪公司代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 11)]
        public string BrokerID;
        /// <summary>
        /// 投资者代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string InvestorID;
        /// <summary>
        /// 投机套保标志
        /// </summary>
        public char HedgeFlag;
        /// <summary>
        /// 多头保证金率
        /// </summary>
        public double LongMarginRatioByMoney;

        /// <summary>
        /// 多头保证金费
        /// </summary>
        public double LongMarginRatioByVolume;

        /// <summary>
        /// 空头保证金率
        /// </summary>
        public double ShortMarginRatioByMoney;

        /// <summary>
        /// 空头保证金费
        /// </summary>
        public double ShortMarginRatioByVolume;
        /// <summary>
        /// 是否相对交易所收取
        /// </summary>
        public int IsRelative;
    }
    
    /// <summary>
    /// 合约手续费率
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class InstrumentCommissionRateField
    {
        /// <summary>
        /// 合约代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 31)]
        public string InstrumentID;
        ///投资者范围
        public char InvestorRange;
        /// <summary>
        /// 经纪公司代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 11)]
        public string BrokerID;
        /// <summary>
        /// 投资者代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string InvestorID;
        /// <summary>
        /// 开仓手续费率
        /// </summary>
        public double OpenRatioByMoney;

        /// <summary>
        /// 开仓手续费
        /// </summary>
        public double OpenRatioByVolume;

        /// <summary>
        /// 平仓手续费率
        /// </summary>
        public double CloseRatioByMoney;

        /// <summary>
        /// 平仓手续费
        /// </summary>
        public double CloseRatioByVolume;

        /// <summary>
        /// 平今手续费率
        /// </summary>
        public double CloseTodayRatioByMoney;

        /// <summary>
        /// 平今手续费
        /// </summary>
        public double CloseTodayRatioByVolume;
    }

    /// <summary>
    /// 产品
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class ProductField
    {
        /// <summary>
        ///产品代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 31)]
        public string ProductID;

        /// <summary>
        ///产品名称
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 21)]
        public string ProductName;

        /// <summary>
        ///交易所代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string ExchangeID;
        /// <summary>
        /// 产品类型
        /// </summary>
        public char ProductClass;
        /// <summary>
        /// 合约数量乘数
        /// </summary>
        public int VolumeMultiple;
        /// <summary>
        /// 最小变动价位
        /// </summary>
        public double PriceTick;

        /// <summary>
        ///市价单最大下单量
        /// </summary>
        public int MaxMarketOrderVolume;

        /// <summary>
        ///市价单最小下单量
        /// </summary>
        public int MinMarketOrderVolume;

        /// <summary>
        ///限价单最大下单量
        /// </summary>
        public int MaxLimitOrderVolume;
        /// <summary>
        /// 限价单最小下单量
        /// </summary>
        public int MinLimitOrderVolume;
        /// <summary>
        /// 持仓类型
        /// </summary>
        public char PositionType;
        /// <summary>
        /// 持仓日期类型
        /// </summary>
        public char PositionDateType;
        /// <summary>
        /// 平仓处理类型
        /// </summary>
        public char CloseDealType;
       
        /// <summary>
        /// 交易币种类型
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
        public string TradeCurrencyID;
        /// <summary>
        /// 质押资金可用范围
        /// </summary>
        public char MortgageFundUseRange;
        /// <summary>
        /// 交易所产品代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 31)]
        public string ExchangeProductID;

        /// <summary>
        /// 合约基础商品乘数
        /// </summary>
        public double UnderlyingMultiple;
    }

    /// <summary>
    /// 经纪公司交易参数
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class BrokerTradingParamsField
    {
        ///经纪公司代码
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 11)]
        public string BrokerID;
        ///投资者代码
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string InvestorID;
        ///保证金价格类型
        public char MarginPriceType;
        ///盈亏算法
        public char Algorithm;
        ///可用是否包含平仓盈利
        public char AvailIncludeCloseProfit;
        ///币种代码
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
        public string CurrencyID;
        ///期权权利金价格类型
        public char OptionRoyaltyPriceType;
    }

    /// <summary>
    /// 经纪公司交易算法
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class BrokerTradingAlgosField
    {
        ///经纪公司代码
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 11)]
        public string BrokerID;
        ///交易所代码
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string ExchangeID;
        ///合约代码
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 31)]
        public string InstrumentID;
        ///持仓处理算法编号
        public char HandlePositionAlgoID;
        ///寻找保证金率算法编号
        public char FindMarginRateAlgoID;
        ///资金处理算法编号
        public char HandleTradingAccountAlgoID;
    };
    #endregion


    /// <summary>
    /// 用户口令变更
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class UserPasswordUpdateField
    {
        /// <summary>
        /// 经纪公司代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 11)]
        public string BrokerID;

        /// <summary>
        /// 用户代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string UserID;

        /// <summary>
        /// 原来的口令
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 41)]
        public string OldPassword;

        /// <summary>
        /// 新的口令
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 41)]
        public string NewPassword;

        /// <summary>
        /// 格式化输出所有公开字段的信息
        /// </summary>
        /// <returns>所有的公开字段信息(字段名：字段值),以",\t"分割</returns>
        public override string ToString()
        {
            return Utility.GetObjectFieldInfo(this);
        }
    }

    /// <summary>
    /// 订阅合约的字段
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class SubMarketDataField
    {
        /// <summary>
        /// 订阅的合约组（多个合约使用,分割）
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public string Insturments;

        /// <summary>
        /// 订阅合约的数量
        /// </summary>
        public int Count;
    }

    /// <summary>
    /// 投资者
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class InvestorField
    {
        /// <summary>
        /// 投资者代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string InvestorID;
        /// <summary>
        /// 经纪公司代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 11)]
        public string BrokerID;

        /// <summary>
        /// 投资者分组代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string InvestorGroupID;

        /// <summary>
        /// 投资者名称
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 81)]
        public string InvestorName;
        /// <summary>
        /// 证件类型
        /// </summary>
        public char IdentifiedCardType;
        /// <summary>
        /// 证件号码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 51)]
        public string IdentifiedCardNo;
        /// <summary>
        /// 是否活跃
        /// </summary>
        public int IsActive;

        /// <summary>
        /// 联系电话
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 41)]
        public string Telephone;

        /// <summary>
        /// 通讯地址
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 101)]
        public string Address;

        /// <summary>
        /// 开户日期
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string OpenDate;

        /// <summary>
        /// 手机
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 41)]
        public string Mobile;

        /// <summary>
        /// 手续费率模板代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string CommModelID;

        /// <summary>
        /// 保证金率模板代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string MarginModelID;

        public override string ToString()
        {
            return Utility.GetObjectFieldInfo(this);
        }
    }

    /// <summary>
    /// 用户登出请求
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class UserLogoutField
    {

        /// <summary>
        /// 经纪公司代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 11)]
        public string BrokerID;
        /// <summary>
        /// 投资者代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string InvestorID;

        public override string ToString()
        {
            return Utility.GetObjectFieldInfo(this);
        }
    }
    
}
