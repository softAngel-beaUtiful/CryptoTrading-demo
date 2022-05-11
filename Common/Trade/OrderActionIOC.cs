namespace TickQuant.Common
{
    public class OrderActionIOC : OrderAction
    {
        /// <summary>
        /// 即成剩撤发起时间间隔
        /// </summary>
        public int WaitTimeSeconds { get; set; } = 0;

        public OrderActionIOC()
        {
            this.ExecuteMode = EExecuteMode.IOC;
        }
    }
}
