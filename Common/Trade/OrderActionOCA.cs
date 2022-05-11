namespace TickQuant.Common
{
    public class OrderActionOCA : OrderAction
    {
        /// <summary>
        /// 对于限价单，限价转市价等待间隔时间
        /// </summary>
        public int LTMWaitTimeSeconds { get; set; } = 60;

        /// <summary>
        /// 止盈取值模式
        /// </summary>
        public EValueMode StopProfitValueMode { get; set; } = EValueMode.Percentage;

        /// <summary>
        /// 止盈阀值
        /// </summary>
        public decimal StopProfitValue { get; set; } = 0.02m;

        /// <summary>
        /// 止损取值模式
        /// </summary>
        public EValueMode StopLossValueMode { get; set; } = EValueMode.Percentage;

        /// <summary>
        /// 止损阀值
        /// </summary>
        public decimal StopLossValue { get; set; } = 0.02m;

        public OrderActionOCA()
        {
            this.ExecuteMode = EExecuteMode.OCA;
        }
    }
}
