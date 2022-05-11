using Newtonsoft.Json;

namespace TickQuant.Common
{
    /// <summary>
    /// 机器指令动作 分为 Join,Start,Stop,Forget,Clear
    /// </summary>
    public enum ERobotAction
    {
        Join=0,   // 初次加入交易对
        Start=1,  // 启动交易对
        Pause=2,  // 暂停交易对
        Forget=3, // 忘记交易对
        Clear=4,  // 清仓交易对
        Manual=5, // 手工操作
        Param =6, // 交易参数
        Unknown=7 // 未知操作
    }

    public enum EQuantType
    {

    }

    public class TRobotUserOrder
    {
        public decimal Price { get;set; }
        public decimal Amount { get; set; }
        public bool IsBuy { get; set; }
        public bool IsMarket { get; set; }                        
    }

    public class RobotSymbolParam: SymbolParam
    {
        /// <summary>
        /// 机器人指令动作
        /// </summary>
        [JsonProperty("robot_action")]
        public ERobotAction RobotAction { get; set; }
        [JsonProperty("user_order")]
        public TRobotUserOrder RobotUserOrder { get; set; }
        /// <summary>
        /// 交易所ID
        /// </summary>
        [JsonProperty("exchange_code")]
        public string ExchangeID { get; set; }
        /// <summary>
        /// 首单买入
        /// </summary>
        [JsonProperty("first_buy")]
        public decimal FirstBuy { get; set; }
        /// <summary>
        /// 交易速度 档位
        /// </summary>
        [JsonProperty("level")]
        public int Level { get; set; }
        /// <summary>
        /// 最大金额用计价货币计算
        /// </summary>
        [JsonProperty("max_cost")]
        public decimal MaxCost { get; set; }
        /// <summary>
        /// 最大交易次数
        /// </summary>
        [JsonProperty("total_trade_num")]
        public string TotalTradeNum { get; set; }
        /// <summary>
        /// 基准货币
        /// </summary>
        [JsonProperty("base_currency")]
        public string BaseCurrency { get; set; }
        /// <summary>
        /// 交易对
        /// </summary>
        [JsonProperty("symbol")]
        public string Symbol { get; set; }
        /// <summary>
        /// 计价货币
        /// </summary>
        [JsonProperty("quote_currency")]
        public string QuoteCurrency { get; set; }
        /// <summary>
        /// 最大计价货币
        /// </summary>
        [JsonProperty("max_quote_currency")]
        public decimal MaxQuoteCurrency { get; set; }
        /// <summary>
        /// 追踪止盈回降比例
        /// </summary>
        [JsonProperty("profit_down_percent")]
        public decimal ProfitDownPercent { get; set; }
        /// <summary>
        /// 是否开启 追踪止盈
        /// </summary>
        [JsonProperty("is_check_profit")]
        public bool IsCheckProfit { get; set; }
        /// <summary>
        /// 止盈百分比
        /// </summary>
        [JsonProperty("stop_profit_percent")]
        public decimal StopProfitPercent { get; set; }
        /// <summary>
        /// 止损百分比
        /// </summary>
        [JsonProperty("stop_less_percent")]
        public decimal StopLossPercent { get; set; }
    }
}
