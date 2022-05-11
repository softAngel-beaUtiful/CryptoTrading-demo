namespace TickQuant.Common
{
    public enum EExecuteMode
    {
        /// <summary>
        /// 即成剩撤(Immediate or Cancel)
        /// </summary>
        IOC,
        /// <summary>
        /// 限价转市价(Limit to Market)
        /// </summary>
        LTM,
        /// <summary>
        /// 限价转追单(Limit to Chasing)
        /// </summary>
        LTC,
        /// <summary>
        /// 组合条件单(One-Cancels-All)
        /// </summary>
        OCA
    }
}
