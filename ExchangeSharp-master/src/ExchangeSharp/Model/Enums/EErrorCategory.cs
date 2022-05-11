using System;
using System.Collections.Generic;
using System.Text;

namespace ExchangeSharp
{
    public enum EErrorCategory
    {
        // 正常（无错误
        Normal,
        // 不支持（需要考虑其他实现方式，或者不使用）
        NotSupport,
        // 校验错误（需要检查接口字段的传参是否符合规则）
        Invalid,
        // 禁止（当前操作在当前时间被禁止）
        Forbidden,
        // 临时错误（考虑延后重复处理一定的次数）
        Temporary,
        // 可用不足（考虑业务逻辑中的可用逻辑是否正常）
        NoSufficientFund,
        // 需进一步确认（当前操作可能已成功，也可能未成功）
        WaitConfirm,
        // 连接被断开(需要重连，然后确认当前操作是否成功，再考虑是否重新处理当前操作）
        Disconnected,
        // 时间不同步（告警提示，人工处理服务器时间同步问题）
        TimeAsynchrony,
        // 复合错误（需要进一步分析错误信息字段才能区分出具体的错误类型）
        Complex,
        // 其他未知的错误类型
        Other
    }
}
