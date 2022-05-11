using System;
using System.Collections.Generic;
using TickQuant.Common;

namespace TickQuant.Strategy
{
    public class TQSignalS<T>
    {
        private readonly List<SignalSerie> SeriesTypes = new List<SignalSerie>();
        private readonly Dictionary<string, T> DictListBar = new Dictionary<string, T>();

        public int Count { get { return this.SeriesTypes.Count; } }

        public TQSignalS(List<SignalSerie> barInfoList)
        {
            if (barInfoList == null)
            {
                throw new Exception("empty args");
            }
            this.SeriesTypes.AddRange(barInfoList);
            foreach (var d in barInfoList)
            {
                this.DictListBar.Add(d.ExchangeID + "_" + d.Symbol + "_" + d.Period, Activator.CreateInstance<T>());
            }
        }
        public void AddSerieType(SignalSerie bst)
        {
            this.SeriesTypes.Add(bst);
            this.DictListBar.Add(bst.ExchangeID + "_" + bst.Symbol + "_" + bst.Period, Activator.CreateInstance<T>());
        }

        public T this[int i]
        {
            get
            {
                if (Count >= 0 && i >= 0 && i < Count)
                {
                    try
                    {
                        return this.DictListBar[$"{this.SeriesTypes[i].ExchangeID}_{this.SeriesTypes[i].Symbol}_{this.SeriesTypes[i].Period}"];
                    }
                    catch (KeyNotFoundException ex)
                    {
                        throw new Exception($"TQSeries has no key=>{i} ({ex.Message})");
                    }
                }
                else
                    throw new IndexOutOfRangeException($"TQSeries Key:{i}/Count:{Count}");
            }

        }
        public T this[SerieType bst]
        {
            get
            {
                try
                {
                    return DictListBar[$"{bst.ExchangeID}_{bst.Symbol}_{bst.Period}"];
                }
                catch (KeyNotFoundException ex)
                {
                    throw new Exception($"TQSeries has no key=>{bst.ExchangeID}_{bst.Symbol}_{bst.Period} ({ex.Message})");
                }
            }
            set
            {
                DictListBar[$"{bst.ExchangeID}_{bst.Symbol}_{bst.Period}"] = value;
            }
        }
    }
   
    public class TQSeries<T>
    {
        private readonly List<SerieType> SeriesTypes = new List<SerieType>();
        private readonly Dictionary<string, T> DictListBar = new Dictionary<string, T>();

        public int Count { get { return this.SeriesTypes.Count; } }

        public TQSeries(List<SerieType> barInfoList)
        {
            if (barInfoList == null)
            {
                throw new Exception("empty args");
            }
            this.SeriesTypes.AddRange(barInfoList);
            foreach (var d in barInfoList)
            {
                this.DictListBar.Add(d.ExchangeID + "_" + d.Symbol + "_" + d.Period, Activator.CreateInstance<T>());
            }
        }        
        public void AddSerieType(SerieType bst)
        {
            this.SeriesTypes.Add(bst);
            this.DictListBar.Add(bst.ExchangeID + "_" + bst.Symbol + "_" + bst.Period, Activator.CreateInstance<T>());
        }

        public T this[int i]
        {
            get
            {
                if (Count >= 0 && i >= 0 && i < Count)
                {
                    try
                    {
                        return this.DictListBar[$"{this.SeriesTypes[i].ExchangeID}_{this.SeriesTypes[i].Symbol}_{this.SeriesTypes[i].Period}"];
                    }
                    catch (KeyNotFoundException ex)
                    {
                        throw new Exception($"TQSeries has no key=>{i} ({ex.Message})");
                    }
                }
                else
                    throw new IndexOutOfRangeException($"TQSeries Key:{i}/Count:{Count}");
            }

        }
        public T this[SerieType bst]
        {
            get
            {
                try
                {
                    return DictListBar[$"{bst.ExchangeID}_{bst.Symbol}_{bst.Period}"];
                }
                catch (KeyNotFoundException ex)
                {
                    throw new Exception($"TQSeries has no key=>{bst.ExchangeID}_{bst.Symbol}_{bst.Period} ({ex.Message})");
                }
            }
            set
            {
                DictListBar[$"{bst.ExchangeID}_{bst.Symbol}_{bst.Period}"] = value;
            }
        }
    }
}
