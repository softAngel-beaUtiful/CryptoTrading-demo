using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoTrading.TQLib
{
    public class TQObservableCollection<T> : ObservableCollection<T>
    {
        #region DoNotifyCollectionChangeRemove
        /// <summary>
        /// Use this method to force any views bound to this collection to look for a specific
        /// item as deleted..use after changing any property that would cause the object to be filtered.
        /// </summary>
        /// <param name=”obj”></param>
        public void TQNotifyCollectionChangeRemove(T obj, int index)
        {
            base.OnCollectionChanged(
            new System.Collections.Specialized.NotifyCollectionChangedEventArgs(
            System.Collections.Specialized.NotifyCollectionChangedAction.Remove, obj, index));
        }
        #endregion
        #region DoNotifyCollectionChangeAdd
        /// <summary>
        /// Use this method to force any views bound to this collection to look for a specific
        /// item as inserted, use after any operation that should cause new items to appear
        /// </summary>
        /// <param name=”obj”></param>
        public void TQNotifyCollectionChangeAdd(T obj, int index)
        {
            base.OnCollectionChanged(
            new System.Collections.Specialized.NotifyCollectionChangedEventArgs(
            System.Collections.Specialized.NotifyCollectionChangedAction.Add, obj, index));
        }
        #endregion
    }
}