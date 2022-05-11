using ExchangeSharp;
using System;

namespace TickQuant.Common
{
    public class OrderAction
    {
        public EExecuteMode ExecuteMode { get; internal set; }
        public EOrderActionState ActionState { get; set; } = EOrderActionState.Pending;
        public ActionResult Result { get; } = new ActionResult();

        public class ActionResult
        {
            public int ProcessedStage { get; set; } = 0;
            public int ProcessedTimes { get; set; } = 0;
            public DateTime LastProcessTime { get; set; } = DateTime.UtcNow;
            public string LastOrderID { get; set; } = string.Empty;
            public decimal LastFilledQuant { get; set; } = 0m;
            public EOrderStatus LastOrderStatus { get; set; } = EOrderStatus.PendingNew;
            public IExchangeAPI RelatedExchangeAPI { get; set; }
        }
    }
}
