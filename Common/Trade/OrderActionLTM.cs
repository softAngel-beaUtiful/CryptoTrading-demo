namespace TickQuant.Common
{
    public class OrderActionLTM : OrderAction
    {
        /// <summary>
        /// 限价转市价等待间隔时间
        /// </summary>
        public int WaitTimeSeconds { get; set; } = 60;

        public OrderActionLTM()
        {
            this.ExecuteMode = EExecuteMode.LTM;
        }
    }
}
