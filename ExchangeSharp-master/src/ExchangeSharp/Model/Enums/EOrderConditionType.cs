namespace ExchangeSharp
{
    public enum EOrderConditionType
    {
        Normal = 0,         //普通委托
        PostOnly = 1,       //只做Maker（Post only）
        FOK = 2,            //全部成交或立即取消（FOK）
        IOC = 3,            //立即成交并取消剩余（IOC）
        Other = -1
    }
}
