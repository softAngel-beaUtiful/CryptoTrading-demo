using System;
using System.Collections.Concurrent;

namespace TickQuant.Common
{
    public class TQCollection<T> : BlockingCollection<T>, ICollectionOp<T>
    {
        public TQCollection() : base()
        {
        }

        public TQCollection(ConcurrentQueue<T> bars) : base(bars)
        {
        }

        public T this[int index]
        {
            get
            {
                if (Count >= 0 && index >= 0 && index < Count)
                    return ToArray()[index];
                else
                    throw new IndexOutOfRangeException($"TQCollection index out of range, index: {index}/count:{this.Count}");
            }
        }

        public T GetAgo(int reverseIndex)
        {
            if (reverseIndex < Count && reverseIndex >= 0)
                return ToArray()[Count - reverseIndex - 1];
            else
                throw new IndexOutOfRangeException($"TQCollection index out of range, index: {reverseIndex}/count:{this.Count}");
        }

        public object GetAgo()
        {
            throw new NotImplementedException();
        }

        public T First
        {
            get
            {
                if (Count > 0)
                    return ToArray()[0];
                else
                    throw new IndexOutOfRangeException("TQCollection has no datas");
            }
        }

        public T Last
        {
            get
            {
                if (Count > 0)
                    return ToArray()[Count - 1];
                else
                    throw new IndexOutOfRangeException("TQCollection has no datas");
            }
        }
    }
}