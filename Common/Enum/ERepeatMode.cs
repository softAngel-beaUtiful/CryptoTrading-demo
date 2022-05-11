namespace TickQuant.Common
{
    public enum ERepeatMode
    {
        /// <summary>
        /// 一次性策略
        /// </summary>
        OneTime,          
        /// <summary>
        /// 重复执行策略
        /// </summary>
        Repeatable,      
        /// <summary>
        /// 建仓策略
        /// </summary>
        OpenPosition,    
        /// <summary>
        /// 平仓策略
        /// </summary>
        ClosePosition     
    }
}
