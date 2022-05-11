namespace TickQuant.Common
{
    public class OrderParams
    {
        public int InstrumentIndex;     //标的索引
        public string UserID;           //用户ID
        public string ExchangeID;       //交易所
        public string StrategyID;       //策略ID
        public string ClOrderID;        //客户端订单ID
        public string Symbol;           //交易对
        public string OrderTag;         //交易标签
        public decimal Quant;           //下单数量
        public decimal Price;           //下单价格
        public EDirection Direction;    //订单方向 long/short(对于现货而言long代表买入，short代表卖出）
        public EPriceType PriceType;    //订单类型 limit/market
        public bool OpenOrClose;        //开平仓标志（仅针对期货和永续期货有效）
        public bool MarginAccount;      //融资账户（仅针对融资有效）
        public OrderAction OrderAdvanced;//下单增强操作
    }
}
