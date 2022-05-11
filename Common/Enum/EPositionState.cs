namespace TickQuant.Common
{
    public enum EPositionState
    {
        None,       // 无持仓
        Placing,    // 报单中
        Long,       // 持有多仓
        Short,      // 持有空仓
        Mixed       // 既持有多仓又持有空仓
    }
}
