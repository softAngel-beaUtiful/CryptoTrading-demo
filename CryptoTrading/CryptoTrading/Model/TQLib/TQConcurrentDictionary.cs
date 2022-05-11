using System;
using System.Collections.Concurrent;

namespace CryptoTrading.TQLib
{
    public class TQConcurrentDictionary<TKey, TValue> : ConcurrentDictionary<TKey, TValue>
    {
        public delegate void UpdateData(TValue data);
        public delegate void RemoveData(TKey data);
        public event UpdateData OnDataUpdated;
        public event RemoveData OnDataRemoved;
        public TValue TQAddOrUpdate(TKey key, TValue addValue)
        {
            if (OnDataUpdated != null)
            {
                OnDataUpdated(addValue);
            }
            var vv = AddOrUpdate(key, addValue,(k, v)=>addValue);
            return vv;
        }
        public TValue TQAddOrUpdate(TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory)
        {
            //if (key == null)
            //    return default(TValue);

            if (OnDataUpdated != null)
            {
                OnDataUpdated(addValue);
            }
            return AddOrUpdate(key, addValue, updateValueFactory);
        }

        public bool TQTryRemove(TKey key, out TValue value)
        {
            if (OnDataRemoved != null)
            {
                OnDataRemoved(key);
            }
            return TryRemove(key, out value);
        }
    }
}
