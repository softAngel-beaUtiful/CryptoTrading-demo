namespace TickQuant.Common
{
    public class TQAccount
    {
        public string RunMode { get; set; }

        /// <summary>
        /// 量化系统用户ID
        /// </summary>
        public string UserID { get; set; }

        /// <summary>
        /// 回测批号
        /// </summary>
        public string BackTestBatchID { get; set; }

        /// <summary>
        /// 交易所账户名称
        /// </summary>
        public string Account { get; set; }

        /// <summary>
        /// 交易市场ID
        /// </summary>
        public string ExchangeID { get; set; }

        /// <summary>
        /// 持有的币种（或合约）
        /// </summary>
        public string InstrumentID { get; set; }

        /// <summary>
        /// 账户币对数量（或合约持仓数量）
        /// </summary>
        public decimal AccPosition { get; set; }

        /// <summary>
        /// 冻结的账户币对数量（或合约的持仓数量）
        /// </summary>
        public decimal FrozenAccPosition { get; set; }

        /// <summary>
        /// 冻结的账户币对数量（或合约的持仓数量）
        /// </summary>
        public decimal LockedAccPosition { get; set; }

        /// <summary>
        /// 可用的账户币对数量（或合约的持仓数量）
        /// </summary>
        public decimal Available { get; set; }
    }

}
