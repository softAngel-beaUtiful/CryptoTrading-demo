using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoTrading
{
    public enum TradeDirection
    {
        Long = '0',
        Short = '1',
        Unknown= '2'
    }

    public class UnfilledOrder
    {
        public int iCloseNumber;
        public Action ActionCloseOrder;
    }
    public enum EnuExchangeID
    {
        OkexFutures,
        OkexSwap,
        OkexSpot,
        OkexETT,
        OkexMargin,
        OkexAccount,
        BitMex,        
        Binance,
        Huobi,
        Bitfinex,
        Poloniex,
        GDAX,
        Kraken,
        Bittrex,
        Gemini
    }
    #region enum
    /// <summary>
    /// 定价方式枚举
    /// </summary>
    public enum PricingMode
    {        
        Preset,       
        Market,        
        OppositePlus,       
        Ownside,      
        MiddlePrice        
    };
   
    public enum FakFok
    {
        Default,       
        FAK,        
        FOK,
    };


   

    /// <summary>
    /// 
    /// </summary>
    public enum HedgeType
    {
        /// <summary>
        /// 投机
        /// </summary>
        投机 = '1',
        /// <summary>
        /// 套利
        /// </summary>
        套利 = '2',
        /// <summary>
        /// 套保
        /// </summary>
        套保 = '3',
    }

    #endregion enum

    /// <summary>
    /// 系统枚举值
    ///Author:rongy.huang@gmail.com
    /// Create Date:2015-12-17
    /// </summary>
    public partial class SysEnum
    {
        /// <summary>
        /// 投资者范围类型
        /// </summary>
        [EnumDescription("投资者范围类型")]
        public enum DepartmentRangeType
        {
            /// <summary>
            /// 所有
            /// </summary>
            [EnumDescription("所有")]
            All = '1',
            /// <summary>
            /// 组织架构
            /// </summary>
            [EnumDescription("组织架构")]
            Group = '2',
            /// <summary>
            /// 单一投资者
            /// </summary>
            [EnumDescription("单一投资者")]
            Single = '3'
        }

        /// <summary>
        /// 数据同步状态类型
        /// </summary>
        [EnumDescription("数据同步状态类型")]
        public enum DataSyncStatusType
        {
            /// <summary>
             /// 未同步
             /// </summary>
            [EnumDescription("未同步")]
            Asynchronous = '1',
            /// <summary>
            /// 同步中
            /// </summary>
            [EnumDescription("同步中")]
            Synchronizing = '2',
            /// <summary>
            /// 已同步
            /// </summary>
            [EnumDescription("已同步")]
            Synchronized = '3'

        }

        /// <summary>
        /// 经纪公司数据同步状态类型
        /// </summary>
        [EnumDescription("经纪公司数据同步状态类型")]
        public enum BrokerDataSyncStatusType
        {

            /// <summary>
            /// 同步中
            /// </summary>
            [EnumDescription("同步中")]
            Synchronizing = '2',
            /// <summary>
            /// 已同步
            /// </summary>
            [EnumDescription("已同步")]
            Synchronized = '1'

        }

        public enum OrderMode { Close, Open, Auto };
        //Current, Assigned, Opposite
        public enum PriceMode { PreSet, Opposite, Ownside }
        public enum QuantMode { Default, AllAvailable, Preset }


        /// <summary>
        /// 投机套保标志类型
        /// </summary>
        [EnumDescription("投机套保标志类型")]
        public enum HedgeFlagType
        {
            /// <summary>
            /// 投机
            /// </summary>
            [EnumDescription("")]
            Speculation ='1',
            /// <summary>
            /// 套利
            /// </summary>
            [EnumDescription("套利")]
            Arbitrage ='2',
            /// <summary>
            /// 套保
            /// </summary>
            [EnumDescription("套保")]
            Hedge='3'
        }

        /*/// <summary>
        /// 产品类型枚举
        /// </summary>
        [EnumDescription("产品类型枚举")]
        public enum ProductClassType
        {
            ///
            /// <summary>
            /// 期货
            /// </summary>
            [EnumDescription("期货")]
            Futures = '1',
            /// <summary>
            /// 期货期权
            /// </summary>
            [EnumDescription("期货期权")]
            Options = '2',
            /// <summary>
            /// 组合
            /// </summary>
            [EnumDescription("组合")]
            Combination = '3',
            /// <summary>
            /// 即期
            /// </summary>
            [EnumDescription("即期")]
            Spot = '4',
            /// <summary>
            /// 期转现
            /// </summary>
            [EnumDescription("期转现")]
            EFP = '5',
            /// <summary>
            /// 现货期权
            /// </summary>
            [EnumDescription("现货期权")]
            SpotOption = '6'
        }*/

    }
    /// <summary>
    /// 交易所类型
    /// </summary>
    [EnumDescription("交易所类型")]
    public enum ExchangeType
    {
        /// <summary>
        /// 上期所
        /// </summary>
        [EnumDescription("上期所")]
        SHFE,
        /// <summary>
        /// 大商所
        /// </summary>
        [EnumDescription("大商所")]
        DCE,
        /// <summary>
        /// 郑商所
        /// </summary>
        [EnumDescription("郑商所")]
        CZCE,
        /// <summary>
        /// 中金所
        /// </summary>
        [EnumDescription("中金所")]
        CFFEX,
        /// <summary>
        /// 能源中心
        /// </summary>
        [EnumDescription("能源中心")]
        INE
    }
    /// <summary>
    /// 报单状态类型
    /// </summary>
    [EnumDescription("报单状态类型")]
    public enum OrderStatusType
    {
        /// <summary>
        /// 全部成交
        /// </summary>
        [EnumDescription("全部成交")]
        全部成交 = '0',
        /// <summary>
        /// 部分成交还在队列中
        /// </summary>
        [EnumDescription("部分成交")]
        部分成交 = '1',
       

        /// <summary>
        /// 未成交还在队列中
        /// </summary>
        [EnumDescription("未成交")]
        未成交 = '3',       

        /// <summary>
        /// 撤单
        /// </summary>
        [EnumDescription("已撤单")]
        已撤单 = '5',
        /// <summary>
        /// 撤单中
        /// </summary>
        [EnumDescription("撤单中")]
        撤单中 = '6',
        /// <summary>
        /// 未知
        /// </summary>
        [EnumDescription("未知")]
        未知 = 'a',

        /// <summary>
        /// 尚未触发
        /// </summary>
        [EnumDescription("尚未触发")]
        尚未触发 = 'b',

        /// <summary>
        /// 已触发
        /// </summary>
        [EnumDescription("下单中")]
        下单中 = 'c',

        [EnumDescription("已完成")]
           已完成 = 'd' ,
        [EnumDescription("失败")]
        失败= 'e',
    }

    /// <summary>
    /// 报单提交状态
    /// </summary>
    [EnumDescription("报单提交状态")]
    public enum OrderSubmitStatusType
    {
        /// <summary>
        /// 已经提交
        /// </summary>
        [EnumDescription("已经提交")]
        已经提交 = '0',
        /// <summary>
        /// 撤单已经提交
        /// </summary>
        撤单已经提交 = '1',
        /// <summary>
        /// 修改已经提交
        /// </summary>
        修改已经提交 = '2',
        /// <summary>
        /// 已经接受
        /// </summary>
        已经接受 = '3',
        /// <summary>
        /// 报单已经被拒绝
        /// </summary>
        报单已经被拒绝 = '4',
        /// <summary>
        /// 撤单已经被拒绝
        /// </summary>
        撤单已经被拒绝 = '5',
        /// <summary>
        /// 改单已经被拒绝
        /// </summary>
        改单已经被拒绝 = '6',
    }

    /// <summary>
    /// 开平标志类型
    /// </summary>
    [EnumDescription("开平标志类型")]
    public enum OffsetFlagType
    {

        /// <summary>
        /// 开仓
        /// </summary>
        [EnumDescription("开仓")]
        Open = '0',
        /// <summary>
        /// 平仓
        /// </summary>
        [EnumDescription("平仓")]
        Close = '1',
        /// <summary>
        /// 强平
        /// </summary>
        [EnumDescription("强平")]
        ForceClose = '2',
        /// <summary>
        /// 平今
        /// </summary>
        [EnumDescription("平今")]
        CloseToday = '3',
        /// <summary>
        /// 平昨
        /// </summary>
        [EnumDescription("平昨")]
        CloseYesterday = '4',
        /// <summary>
        /// 强减
        /// </summary>
        [EnumDescription("强减")]
        ForceOff = '5',
        /// <summary>
        /// 本地强平
        /// </summary>
        [EnumDescription("本地强平")]
        LocalForceClose = '6'
    }
    /// <summary>
    /// 查询范围类型
    /// </summary>
    public enum QueryRangeType
    {
        /// <summary>
        /// 用户指定
        /// </summary>
        [EnumDescription("用户指定")]
        UserDefined=0,
        /// <summary>
        /// 今日
        /// </summary>
        [EnumDescription("今日")]
        Today = 1,
        /// <summary>
        /// 最近一周
        /// </summary>
        [EnumDescription("最近一周")]
        NearWeek = 2,
        /// <summary>
        /// 最近一月
        /// </summary>
        [EnumDescription("最近一月")]
        NearMonth = 3
    }

    /// <summary>
    /// 证件类型类型
    /// </summary>
    [EnumDescription("证件类型类型")]
    public enum IdCardType
    {
        /// <summary>
        /// 组织机构代码
        /// </summary>
        [EnumDescription("组织机构代码")]
        EID = '0',

        /// <summary>
        /// 身份证
        /// </summary>
        [EnumDescription("身份证")]
        IDCard = '1',

        /// <summary>
        /// 军官证
        /// </summary>
        [EnumDescription("军官证")]
        OfficerIDCard = '2',

        /// <summary>
        /// 警官证
        /// </summary>
        [EnumDescription("警官证")]
        PoliceIDCard = '3',

        /// <summary>
        /// 士兵证
        /// </summary>
        [EnumDescription("士兵证")]
        SoldierIDCard = '4',

        /// <summary>
        /// 户口簿
        /// </summary>
        [EnumDescription("户口簿")]
        HouseholdRegister = '5',

        /// <summary>
        /// 护照
        /// </summary>
        [EnumDescription("护照")]
        Passport = '6',

        /// <summary>
        /// 台胞证
        /// </summary>
        [EnumDescription("台胞证")]
        TaiwanCompatriotIDCard = '7',

        /// <summary>
        /// 回乡证
        /// </summary>
        [EnumDescription("回乡证")]
        HomeComingCard = '8',

        /// <summary>
        /// 营业执照号
        /// </summary>
        [EnumDescription("营业执照号")]
        LicenseNo = '9',

        /// <summary>
        /// 税务登记号
        /// </summary>
        [EnumDescription("税务登记号")]
        TaxNo = 'A',

        /// <summary>
        /// 其他证件
        /// </summary>
        [EnumDescription("其他证件")]
        OtherCard = 'x'

    }
    /// <summary>
    /// 银行帐户类型类型
    /// </summary>
    [EnumDescription("银行帐户类型类型")]
    public enum BankAccType
    {
        /// <summary>
        /// 存折
        /// </summary>
        [EnumDescription("存折")]
        BankBook = '1',

        /// <summary>
        /// 储蓄卡
        /// </summary>
        [EnumDescription("储蓄卡")]
        BankCard = '2',
        /// <summary>
        /// 信用卡
        /// </summary>
        [EnumDescription("信用卡")]
        CreditCard = '3'
    }

    /// <summary>
    /// 客户类型类型
    /// </summary>
    [EnumDescription("客户类型类型")]
    public enum CustType
    {
        /// <summary>
        /// 自然人
        /// </summary>
        [EnumDescription("自然人")]
        Person = '0',

        /// <summary>
        /// 机构户
        /// </summary>
        [EnumDescription("机构户")]
        Institution = '1',
    }

    /// <summary>
    /// 期货公司帐号类型类型
    /// </summary>
    [EnumDescription("期货公司帐号类型类型")]
    public enum FutureAccType
    {
        /// <summary>
        /// 存折
        /// </summary>
        [EnumDescription("存折")]
        BankBook = '1',

        /// <summary>
        /// 储蓄卡
        /// </summary>
        [EnumDescription("储蓄卡")]
        BankCard = '2',
        /// <summary>
        /// 信用卡
        /// </summary>
        [EnumDescription("信用卡")]
        CreditCard = '3'
    }

    /// <summary>
    /// 密码核对标志类型
    /// </summary>
    [EnumDescription("密码核对标志类型")]
    public enum PwdFlagType
    {
        /// <summary>
        /// 不核对
        /// </summary>
        [EnumDescription("不核对")]
        NoCheck = '0',
        /// <summary>
        /// 明文核对
        /// </summary>
        [EnumDescription("明文核对")]
        BlankCheck = '1',
        /// <summary>
        /// 密文核对
        /// </summary>
        [EnumDescription("密文核对")]
        EncryptCheck = '2'
    }

    /// <summary>
    /// 期商冲正标志
    /// </summary>
    [EnumDescription("期商冲正标志")]
    public enum BrokerRepealFlagType
    {
        /// <summary>
        /// 无需自动冲正
        /// </summary>
        [EnumDescription("期商无需自动冲正")]
        NotNeedRepeal = '0',

        /// <summary>
        /// 待自动冲正
        /// </summary>
        [EnumDescription("期商待自动冲正")]
        WaitingRepeal = '1',

        /// <summary>
        /// 已自动冲正
        /// </summary>
        [EnumDescription("期商已自动冲正")]
        BeenRepealed = '2'
    }

    /// <summary>
    /// 银行冲正标志
    /// </summary>
    [EnumDescription("银行冲正标志")]
    public enum BankRepealFlagType
    {
        /// <summary>
        /// 无需自动冲正
        /// </summary>
        [EnumDescription("银行无需自动冲正")]
        NotNeedRepeal = '0',

        /// <summary>
        /// 待自动冲正
        /// </summary>
        [EnumDescription("银行待自动冲正")]
        WaitingRepeal = '1',

        /// <summary>
        /// 已自动冲正
        /// </summary>
        [EnumDescription("银行已自动冲正")]
        BeenRepealed = '2'
    }
    /// <summary>
    /// 费用支付标志类型
    /// </summary>
    [EnumDescription("费用支付标志类型")]
    public enum FeePayFlagType
    {
        /// <summary>
        /// 由受益方支付费用
        /// </summary>
        [EnumDescription("由受益方支付费用")]
        BEN = '0',
        /// <summary>
        /// 由发送方支付费用
        /// </summary>
        [EnumDescription("由发送方支付费用")]

        OUR = '1',
        /// <summary>
        /// 由发送方支付发起的费用，受益方支付接受的费用
        /// </summary>
        [EnumDescription("由发送方支付发起的费用，受益方支付接受的费用")]
        SHA = '2',
    }

    /// <summary>
    /// 有效标志类型
    /// </summary>
    [EnumDescription("有效标志类型")]
    public enum AvailabilityFlagType
    {
        /// <summary>
        /// 未确认
        /// </summary>
        [EnumDescription("未确认")]
        Invalid = '0',
        /// <summary>
        /// 有效
        /// </summary>
        [EnumDescription("有效")]
        Valid = '1',
        /// <summary>
        /// 冲正
        /// </summary>
        [EnumDescription("冲正")]
        Repeal = '2'
    }

    /// <summary>
    /// 报单价格条件类型
    /// </summary>
    [EnumDescription("报单价格条件类型")]
    public enum OrderPriceType
    {
        /// <summary>
        /// 任意价
        /// </summary>
        [EnumDescription("市价")]
        市价 = '1',
        /// <summary>
        /// 限价
        /// </summary>
        [EnumDescription("限价")]
        限价 = '2',
        [EnumDescription("停损价")]
        停损价 = '3',
        [EnumDescription("停损限价")]
        停损限价 = '4',        
    }

    /// <summary>
    /// 触发条件类型
    /// </summary>
    [EnumDescription("触发条件类型")]
    public enum ContingentConditionType
    {
        /// <summary>
        /// 立即
        /// </summary>
        [EnumDescription("立即")]
        立即 = '1',
        /// <summary>
        /// 止损
        /// </summary>
        [EnumDescription("止损")]
        止损 = '2',
        /// <summary>
        /// 止赢
        /// </summary>
        [EnumDescription("止赢")]
        止赢 = '3',
        /// <summary>
        /// 预埋单
        /// </summary>
        [EnumDescription("预埋单")]
        预埋单 = '4',
        /// <summary>
        /// 最新价大于条件价
        /// </summary>
        [EnumDescription("最新价大于条件价")]
        最新价大于条件价 = '5',
        /// <summary>
        /// 最新价大于等于条件价
        /// </summary>
        [EnumDescription("最新价大于等于条件价")]
        最新价大于等于条件价 = '6',
        /// <summary>
        /// 最新价小于条件价
        /// </summary>
        [EnumDescription("最新价小于条件价")]
        最新价小于条件价 = '7',
        /// <summary>
        /// 最新价小于等于条件价
        /// </summary>
        [EnumDescription("最新价小于等于条件价")]
        最新价小于等于条件价 = '8',
        /// <summary>
        /// 卖一价大于条件价
        /// </summary>
        [EnumDescription("卖一价大于条件价")]
        卖一价大于条件价 = '9',
        /// <summary>
        /// 卖一价大于等于条件价
        /// </summary>
        [EnumDescription("卖一价大于等于条件价")]
        卖一价大于等于条件价 = 'A',
        /// <summary>
        /// 卖一价小于条件价
        /// </summary>
        [EnumDescription("卖一价小于条件价")]
        卖一价小于条件价 = 'B',
        /// <summary>
        /// 卖一价小于等于条件价
        /// </summary>
        [EnumDescription("卖一价小于等于条件价")]
        卖一价小于等于条件价 = 'C',
        /// <summary>
        /// 买一价大于条件价
        /// </summary>
        [EnumDescription("买一价大于条件价")]
        买一价大于条件价 = 'D',
        /// <summary>
        /// 买一价大于等于条件价
        /// </summary>
        [EnumDescription("买一价大于等于条件价")]
        买一价大于等于条件价 = 'E',
        /// <summary>
        /// 买一价小于条件价
        /// </summary>
        [EnumDescription("买一价小于条件价")]
        买一价小于条件价 = 'F',
        /// <summary>
        /// 买一价小于等于条件价
        /// </summary>
        [EnumDescription("买一价小于等于条件价")]
        买一价小于等于条件价 = 'H'
    }

    /// <summary>
    /// 强平原因类型
    /// </summary>
    [EnumDescription("强平原因类型")]
    public enum ForceCloseReasonType
    {
        /// <summary>
        /// 非强平
        /// </summary>
        [EnumDescription("非强平")]
        非强平 = '0',
        /// <summary>
        /// 资金不足
        /// </summary>
        [EnumDescription("资金不足")]
        资金不足 = '1',
        /// <summary>
        /// 客户超仓
        /// </summary>
        [EnumDescription("客户超仓")]
        客户超仓 = '2',
        /// <summary>
        /// 会员超仓
        /// </summary>
        [EnumDescription("会员超仓")]
        会员超仓 = '3',
        /// <summary>
        /// 持仓非整数倍
        /// </summary>
        [EnumDescription("持仓非整数倍")]
        持仓非整数倍 = '4',
        /// <summary>
        /// 违规
        /// </summary>
        [EnumDescription("违规")]
        违规 = '5',
        /// <summary>
        /// 其它
        /// </summary>
        [EnumDescription("其它")]
        其它 = '6',
        /// <summary>
        /// 自然人临近交割
        /// </summary>
        [EnumDescription("自然人临近交割")]
        自然人临近交割 = '7'
    }

    /// <summary>
    /// 预埋单状态类型
    /// </summary>
    [EnumDescription("预埋单状态类型")]
    public enum ParkedOrderStatusType
    {
        /// <summary>
        /// 未发送
        /// </summary>
        [EnumDescription("未发送")]
        未发送 = '1',
        /// <summary>
        /// 已发送
        /// </summary>
        [EnumDescription("已发送")]
        已发送 = '2',
        /// <summary>
        /// 已删除
        /// </summary>
        [EnumDescription("已删除")]
        已删除 = '3'
    }

    /// <summary>
    /// 有效期类型枚举
    /// </summary>
    [EnumDescription("有效期类型")]
    public enum TimeConditionType
    {
        /// <summary>
        /// 立即完成，否则撤销
        /// </summary>
        [EnumDescription("立即完成，否则撤销")]
        IOC = '1',
        /// <summary>
        /// 本节有效
        /// </summary>
        [EnumDescription("本节有效")]
        GFS = '2',
        /// <summary>
        /// 当日有效
        /// </summary>
        [EnumDescription("当日有效")]
        GFD = '3',
        /// <summary>
        /// 指定日期前有效
        /// </summary>
        [EnumDescription("指定日期前有效")]
        GTD = '4',
        /// <summary>
        /// 撤销前有效
        /// </summary>
        [EnumDescription("撤销前有效")]
        GTC = '5',
        /// <summary>
        /// 集合竞价有效
        /// </summary>
        [EnumDescription("集合竞价有效")]
        GFA = '6'
    }

    /// <summary>
    /// 成交量类型枚举
    /// </summary>
    [EnumDescription("成交量类型枚举")]
    public enum VolumeConditionType
    {
        /// <summary>
        /// 任何数量
        /// </summary>
        [EnumDescription("任何数量")]
        任何数量 = '1',
        /// <summary>
        /// 最小数量
        /// </summary>
        [EnumDescription("最小数量")]
        最小数量 = '2',
        /// <summary>
        /// 全部数量
        /// </summary>
        [EnumDescription("全部数量")]
        全部数量 = '3'
    }


    /// <summary>
    /// 操作标志类型
    /// </summary>
    [EnumDescription("操作标志类型")]
    public enum ActionFlagType
    {
        /// <summary>
        /// 删除
        /// </summary>
        [EnumDescription("删除")]
        Delete = '0',

        /// <summary>
        /// 修改
        /// </summary>
        [EnumDescription("修改")]
        Modify = '3'
    }

    /// <summary>
    /// 用户类型枚举
    /// </summary>
    [EnumDescription("用户类型枚举")]
    public enum UserType
    {
        /// <summary>
        /// 投资者
        /// </summary>
        [EnumDescription("投资者")]
        Investor = '0',
        /// <summary>
        /// 操作员
        /// </summary>
        [EnumDescription("操作员")]
        Operator = '1',
        /// <summary>
        /// 管理员
        /// </summary>
        [EnumDescription("管理员")]
        SuperUser = '2'
    }

    /// <summary>
    /// 报单类型类型
    /// </summary>
    [EnumDescription("报单类型类型")]
    public enum OrderTypeType
    {
        /// <summary>
        /// 正常
        /// </summary>
        [EnumDescription("正常")]
        Normal = '0',
        /// <summary>
        /// 报价衍生
        /// </summary>
        [EnumDescription("报价衍生")]
        DeriveFromQuote = '1',
        /// <summary>
        /// 组合衍生
        /// </summary>
        [EnumDescription("组合衍生")]
        DeriveFromCombination = '2',
        /// <summary>
        /// 组合报单
        /// </summary>
        [EnumDescription("组合报单")]
        Combination = '3',
        /// <summary>
        /// 条件单
        /// </summary>
        [EnumDescription("条件单")]
        ConditionalOrder = '4',
        /// <summary>
        /// 互换单
        /// </summary>
        [EnumDescription("互换单")]
        Swap = '5'
    }

    /// <summary>
    /// 报单来源类型
    /// </summary>
    [EnumDescription("报单来源类型")]
    public enum OrderSourceType
    {
        /// <summary>
        /// 来自参与者
        /// </summary>
        [EnumDescription("来自参与者")]
        Participant ='0',
        /// <summary>
        /// 来自管理员
        /// </summary>
        [EnumDescription("来自管理员")]
        Administrator ='1'
    }



    public enum ExchaneID
    {
        Okex,
        BitMex,
        Polonix,
        Biance,
        Bitfinex,
        CEX,
        HuoBi,
        CoinBase,    
    }

    /// <summary>
    /// 选项配置类型
    /// </summary>
    public enum SettingsType
    {
        EditingInstrumentData,
        /// <summary>
        /// 设置合约组
        /// </summary>
        SettingInstrumentGroup,
        /// <summary>
        /// 自定义组合
        /// </summary>
        SettingCustomInstrument,
        /// <summary>
        /// 管理合约组
        /// </summary>
        ManageInstrumentGroup,

        /// <summary>
        /// 默认手数
        /// </summary>
        DefaultQuant,

        /// <summary>
        /// 下单板
        /// </summary>
        OrderBoard,
        /// <summary>
        /// 常规显示
        /// </summary>
        CommonDisplay,
        /// <summary>
        /// 颜色
        /// </summary>
        Color,

        /// <summary>
        /// 通知及反馈
        /// </summary>
        NoticesSet,

        /// <summary>
        /// 本地数据维护设置
        /// </summary>
        LocalDataMaintain,
        /// <summary>
        /// 交易快捷键常规
        /// </summary>
        TradeShortcutKeyCommon,

        /// <summary>
        /// 交易快捷键设置
        /// </summary>
        TradeShortcutKeySet,
        /// <summary>
        /// 资金账号
        /// </summary>
        TradingAccountColumn,
        /// <summary>
        /// 行情
        /// </summary>
        MarketColumn,

        /// <summary>
        /// 合约
        /// </summary>
        InstrumentColumn,

        /// <summary>
        /// 未成交委托单
        /// </summary>
        UnsettledOrderColumn,

        /// <summary>
        /// 撤单/错单
        /// </summary>
        CanceledOrdersColumn,

        /// <summary>
        /// 所有委托单
        /// </summary>
        OrderColumn,

        /// <summary>
        /// 预埋条件单
        /// </summary>
        ParkedOrderColumn,

        /// <summary>
        /// 已成交单
        /// </summary>
        SettledOrderColumn,
        /// <summary>
        /// 通用持仓
        /// </summary>
        GeneralPositionColumn,

        /// <summary>
        /// 策略持仓
        /// </summary>
        StrategyPositionColumn,

        /// <summary>
        /// 持仓明细
        /// </summary>
        PositionColumn,

        /// <summary>
        /// 持仓汇总
        /// </summary>
        PositionSummaryColumn,

        /// <summary>
        /// 组合持仓
        /// </summary>
        ComboPositionColumn,
        /// <summary>
        /// 成交明细
        /// </summary>
        TradeColumn,

        /// <summary>
        /// 成交汇总
        /// </summary>
        TradeSummaryColumn
    }


    /// <summary>
    /// 组合类型类型
    /// </summary>
    [EnumDescription("组合类型类型")]
    public enum CombinationType
    {
        /// <summary>
        /// 期货组合
        /// </summary>
        [EnumDescription("期货组合")]
        Future = '0',

        /// <summary>
        /// 垂直价差BUL
        /// </summary>
        [EnumDescription("垂直价差BUL")]
        BUL = '1',

        /// <summary>
        /// 垂直价差BER
        /// </summary>
        [EnumDescription("垂直价差BER")]
        BER = '2',

        /// <summary>
        /// 跨式组合
        /// </summary>
        [EnumDescription("跨式组合")]
        STD = '3',

        /// <summary>
        /// 宽跨式组合
        /// </summary>
        [EnumDescription("宽跨式组合")]
        STG = '4',

        /// <summary>
        /// 备兑组合
        /// </summary>
        [EnumDescription("备兑组合")]
        PRT = '5'
    }
    /// <summary>
    /// 投资者范围类型
    /// </summary>
    public enum InvestorRangeType
    {
        /// <summary>
        /// 所有
        /// </summary>
        所有 = '1',
        /// <summary>
        /// 投资者组
        /// </summary>
        投资者组 = '2',
        /// <summary>
        /// 单一投资者
        /// </summary>
        单一投资者 = '3'
    }

    /// <summary>
    /// DataGrid列名设置类别
    /// </summary>
    public enum DataGridType
    {
        /// <summary>
        /// 资金账号DataGrid
        /// </summary>
        Account,

        /// <summary>
        /// 行情DataGrid
        /// </summary>
        MarketData,
        /// <summary>
        /// 合约DataGrid
        /// </summary>
        Instrument,
        /// <summary>
        /// 未成交单DataGrid
        /// </summary>
        UnsettledOrders,
        /// <summary>
        /// 所有委托DataGrid
        /// </summary>
        TodayOrders,
        /// <summary>
        /// 撤单错单
        /// </summary>
        CanceledOrders,
        /// <summary>
        /// 预埋条件单DataGrid
        /// </summary>
        ComplexOrders,
        /// <summary>
        /// 已成交单DataGrid
        /// </summary>
        SettledOrders,
        /// <summary>
        /// 人工交易持仓DataGrid
        /// </summary>
        ManualPosition,
        /// <summary>
        /// 策略持仓DataGrid
        /// </summary>
        StrategyPosition,
        /// <summary>
        /// 持仓汇总DataGrid
        /// </summary>
        PositionSummary,
        /// <summary>
        /// 持仓明细DataGrid
        /// </summary>
        PositionDetails,
        /// <summary>
        /// 成交明细DataGrid
        /// </summary>
        TradeDetails,
        /// <summary>
        /// 成交汇总DataGrid
        /// </summary>
        TradeSummary,
        /// <summary>
        /// 银期签约账号DataGrid
        /// </summary>
        Accountregister,
        /// <summary>
        /// 转账流水DataGrid
        /// </summary>
        TransferSerial
    }
    /// <summary>
    /// 保证金价格类型类型
    /// </summary>
    public enum MarginPriceTypeType
    {
        /// <summary>
        /// 昨结算价
        /// </summary>
        PreSettlementPrice = '1',
        /// <summary>
        /// 最新价
        /// </summary>
        SettlementPrice = '2',
        /// <summary>
        /// 成交均价
        /// </summary>
        AveragePrice = '3',
        /// <summary>
        /// 开仓价
        /// </summary>
        OpenPrice = '4'
    }

    /// <summary>
    /// 盈亏算法类型
    /// </summary>
    public enum AlgorithmType
    {
        /// <summary>
        /// 浮盈浮亏都计算
        /// </summary>
        All = '1',
        /// <summary>
        /// 浮盈不计，浮亏计
        /// </summary>
        OnlyLost = '2',
        /// <summary>
        /// 浮盈计，浮亏不计
        /// </summary>
        OnlyGain = '3',
        /// <summary>
        /// 浮盈浮亏都不计算
        /// </summary>
        None = '4'
    }
    /// <summary>
    /// 是否包含平仓盈利类型
    /// </summary>
    public enum IncludeCloseProfitType
    {
        /// <summary>
        /// 包含平仓盈利
        /// </summary>
        Include = '0',
        /// <summary>
        /// 不包含平仓盈利
        /// </summary>
        NotInclude = '2'

    }
    /// <summary>
    /// 期权权利金价格类型类型
    /// </summary>
    public enum OptionRoyaltyPriceTypeType
    {
        /// <summary>
        /// 昨结算价
        /// </summary>
        PreSettlementPrice = '1',
        /// <summary>
        /// 开仓价
        /// </summary>
        OpenPrice = '4'
    }


    public enum LimitMarket
    {
        限价,
        市价,
    }
    public enum AverageOrEvery
    {
        均价,
        分笔, 
    }
    public enum RepeatOrOnce
    {
        重复,
        单次,
    }
}

