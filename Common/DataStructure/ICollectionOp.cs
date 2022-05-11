namespace TickQuant.Common
{
    public interface ICollectionOp<T>
    {
        T this[int index] { get; }
        T GetAgo(int reverseIndex);
        T First { get; }
        T Last { get; }
    }
}
