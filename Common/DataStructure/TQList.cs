using System;
using System.Collections.Generic;

namespace TickQuant.Common
{
    /// <summary>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TList<T> : ICollectionOp<T>
    {
        private List<T> InterList;
        public int MaxLength { get; set; } = 100000;
        public int Count { get { return InterList.Count; } }

        public void Add(T t)
        {
            this.InterList.Add(t);
            if (Count > MaxLength)
                InterList.RemoveRange(0, Count - MaxLength);
        }

        public void Add(List<T> list)
        {
            this.InterList.AddRange(list);
            if (Count > MaxLength)
                InterList.RemoveRange(0, Count - MaxLength);
        }

        public TList(List<T> list, int maxLength = 100000)
        {
            this.InterList = list;
            this.MaxLength = maxLength;
        }

        public TList()
        {
            this.InterList = new List<T>();
        }

        public T this[int index]
        {
            get
            {
                if (this.InterList.Count >= 0 && index >= 0 && index < this.InterList.Count)
                    return this.InterList[index];
                else
                    throw new IndexOutOfRangeException($"TQList index out of range, index:{index}/count:{this.Count}");
            }
        }

        public List<T> GetRange(int index, int count)
        {
            if (this.InterList.Count > 0 && index >= 0 && index < this.InterList.Count && index + count <= this.InterList.Count)
                return new List<T>(this.InterList.GetRange(index, count));
            else
                throw new IndexOutOfRangeException($"TQList index out of range, start index: {index} and wanted:{count}/count: {this.Count}");
        }

        public T GetAgo(int reverseIndex)
        {
            if (reverseIndex < Count && reverseIndex >= 0)
                return this.InterList[Count - reverseIndex - 1];
            else
                throw new IndexOutOfRangeException($"TQList index out of range, index: {reverseIndex}/count:{this.Count}");
        }

        public T First
        {
            get
            {
                if (Count > 0)
                    return this.InterList[0];
                else
                    throw new IndexOutOfRangeException("TQList has no datas");
            }
        }

        public T Last
        {
            get
            {
                if (Count > 0)
                    return this.InterList[Count - 1];
                else
                    throw new IndexOutOfRangeException("TQList has no datas");
            }
        }
    }   
}