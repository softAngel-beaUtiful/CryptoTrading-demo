namespace TickQuant.Common
{
    public enum EOrderStatus
    {
        /// <summary>
        /// 未知状态
        /// </summary>
        Unknown,

        /// <summary>
        /// 下单中
        /// </summary>
        PendingNew,

        /// <summary>
        /// 已下单
        /// </summary>
        New,

        /// <summary>
        /// 预提交（埋单）
        /// </summary>
        PreSubmited,

        /// <summary>
        /// 撤单中
        /// </summary>
        PendingCancel,

        /// <summary>
        /// 部成
        /// </summary>
        PartiallyFilled,

        /// <summary>
        /// 全成
        /// </summary>
        Filled,

        /// <summary>
        /// 废单
        /// </summary>
        Rejected,

        /// <summary>
        /// 全撤
        /// </summary>
        Canceled,

        /// <summary>
        /// 部撤
        /// </summary>
        PartiallyCanceled,

        /// <summary>
        /// 假定已完成（查询时不明确时使用，作为终态）
        /// </summary>
        PendingFinished
    }
}
