namespace TickQuant.Common
{
    public enum EStrategyState
    {
        Created,            // 已创建（初始状态
        Initializing,       // 初始化过程中
        InitializingStep2,  // 初始化第二阶段
        Initialized,        // 初始化完成
        DataLoaded,         // 历史数据已经载入
        Working,            // 策略运算中
        Suspended,          // 策略已挂起，不接受行情数据
        Closed,             // 策略已关闭
        Ordering,           // 处理报单中
        PositionOpened,     // 仓位已开
        PositionClosed,     // 仓位已平
    }
}