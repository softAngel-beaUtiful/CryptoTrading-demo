namespace TickQuant.Common
{
    public class OrderActionLTC : OrderAction
    {
        /// <summary>
        /// 追单间隔时间
        /// </summary>
        public int WaitTimeSeconds { get; set; } = 60;

        /// <summary>
        /// 在追对手盘的基础上额外增加几跳
        /// </summary>
        public int AdditonalPriceStep { get; set; } = 1;

        /// <summary>
        /// 只追对手盘的标记（如果为True，将不会增加AdditonalPriceStep指定的跳数）
        /// </summary>
        public bool OnlyChasingOpponent { get; set; } = true;

        public OrderActionLTC()
        {
            this.ExecuteMode = EExecuteMode.LTC;
        }
    }
}
