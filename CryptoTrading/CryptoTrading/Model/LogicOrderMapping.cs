using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using CryptoTrading.Model;
using CryptoTrading.ViewModel;

namespace CryptoTrading.Model
{
    /// <summary>
    /// 逻辑报单对应关系
    /// 包含策略或者组合
    /// 不包含普通报单
    /// </summary>
    [Serializable]
    public class LogicOrderMapping
    {
        [XmlElement]
        public OrderParams OrderParams;
        [XmlIgnore]
        public List<TradeData> TradeList;
        public LogicOrderMapping()
        {
            TradeList = new List<TradeData>();
        }
        /// <summary>
        /// 接受成交回报，存入 TradeList
        /// 计算 已成交数量 写入 VolumeTraded，同时将增加部分计入持仓
        /// 根据方向计算 OpenVolume 、CloseVolume
        /// 设置 position = ydposition + OpenVolume - CloseVolume
        /// </summary>
        /// <param name="trade"></param>
        public void AddTrade(TradeData trade, out int tradeVolumeAdded, out double tradeCostAdded)//, out double tradeCommissionAdded)
        {

            InstrumentData instrument;

            tradeVolumeAdded = 0;
            tradeCostAdded = 0;
            //tradeCommissionAdded = 0;

            TradeList.Add(trade);

            // 尝试合成仓位

            // 如果是组合
            if (OrderParams.IsCombo)
            {

                ComboMarketData combo;

                TQMainModel.dicCustomProduct.TryGetValue(OrderParams.InstrumentID, out combo);

                if (combo == null)
                {
                    throw new Exception(string.Format("合约列表中找不到该自定义合约：{0}！", OrderParams.InstrumentID));
                }

                int position = int.MaxValue, positionNew = 0;
                foreach (var item in combo.ItemList)
                {
                    var tradeInList = TradeList.FindAll(t => t.InstrumentID == item.InstrumentID);

                    if (tradeInList == null)
                    {
                        position = 0;
                        break;
                    }
                    else
                    {
                        positionNew = Math.Abs(tradeInList.Sum(t => (int)t.Quant) / item.Weight);

                        if (position > positionNew) position = positionNew;
                    }
                }


                tradeVolumeAdded = position - OrderParams.VolumeTraded;

                if (tradeVolumeAdded > 0)
                {
                    Dictionary<TradeData, int> TradeJustTradedList = new Dictionary<TradeData, int>();
                    List<double> tradeCostAddedList = new List<double>();
                    List<double> tradeCommissionAddedList = new List<double>();

                    foreach (var item in combo.ItemList)
                    {
                        if (!TQMainModel.dicInstrumentData.TryGetValue(item.InstrumentID, out instrument) || instrument == null || string.IsNullOrEmpty(instrument.InstrumentID))
                        {
                            throw new Exception(string.Format("获取合约信息失败\r\n合约编号：{0}！", item.InstrumentID));
                        }


                        int PreTotalVolume, PreVolume = 0, PreShortVolume, PreLeftVolume, JustTotalVolume, JustVolume = 0, JustShortVolume;

                        PreTotalVolume = Math.Abs(item.Weight * OrderParams.VolumeTraded);
                        JustTotalVolume = Math.Abs(item.Weight * tradeVolumeAdded);

                        var tradeInList = TradeList.FindAll(t => t.InstrumentID == item.InstrumentID);

                        tradeInList.Sort((k, v) =>
                        {
                            if (GetInt(k.OrderTime) < GetInt(v.OrderTime)) return 1;
                            return -1;
                        });

                        for (int m = 0; m < tradeInList.Count; m++)
                        {
                            PreShortVolume = PreTotalVolume - PreVolume;
                            if (PreShortVolume > 0)
                            {
                                if (tradeInList[m].Quant > PreShortVolume)
                                {
                                    PreLeftVolume = (int)tradeInList[m].Quant - PreShortVolume;

                                    PreVolume += PreShortVolume;
                                }
                                else
                                {
                                    PreVolume += (int)tradeInList[m].Quant;
                                    continue;
                                }
                            }
                            else
                            {
                                PreLeftVolume = (int)tradeInList[m].Quant;
                            }

                            JustShortVolume = JustTotalVolume - JustVolume;
                            if (JustShortVolume > 0)
                            {
                                if (PreLeftVolume >= JustShortVolume)
                                {
                                    TradeJustTradedList.Add(tradeInList[m], JustShortVolume);
                                    break;
                                }
                                else
                                {
                                    JustVolume += PreLeftVolume;
                                    TradeJustTradedList.Add(tradeInList[m], PreLeftVolume);
                                }
                            }
                            else
                            {
                                break;
                            }

                        }

                        tradeCostAdded = 0;
                        //tradeCommissionAdded = 0;

                        foreach (var tradeJustTraded in TradeJustTradedList)
                        {
                            if (item.Weight > 0)
                                tradeCostAdded += tradeJustTraded.Key.OrderPrice * tradeJustTraded.Value;
                            else
                                tradeCostAdded -= tradeJustTraded.Key.OrderPrice * tradeJustTraded.Value;
                            //tradeCommissionAdded += tradeJustTraded.Key.Commission * tradeJustTraded.Value / tradeJustTraded.Key.volume;
                        }

                        tradeCostAddedList.Add(tradeCostAdded);
                        //tradeCommissionAddedList.Add(tradeCommissionAdded);

                        TradeJustTradedList.Clear();
                    }

                    tradeCostAdded = 0;
                    //tradeCommissionAdded = 0;
                    foreach (var tradeAmount in tradeCostAddedList)
                    {
                        tradeCostAdded += tradeAmount;
                    }
                    //foreach (var tradeCommission in tradeCommissionAddedList)
                    //{
                    //    tradeCommissionAdded += tradeCommission;
                    //}

                    OrderParams.VolumeTraded = position;
                }

            }
            else
            {
                MajorMarketData md;
                tradeVolumeAdded = (int)trade.Quant;
                TQMainModel.dicMajorMarketData.TryGetValue(OrderParams.InstrumentID, out md);
                tradeCostAdded = tradeVolumeAdded * trade.OrderPrice;
                //tradeCommissionAdded += trade.Commission;
            }



            //// 修改或新增到 OrderList
            //TradeField old = TradeList.Find(o =>
            //    o.OrderRef == trade.OrderRef &&
            //    o.SessionID == trade.SessionID &&
            //    o.FrontID == trade.FrontID);

            //if (old != null)
            //{
            //    if (trade.OrderStatus != old.OrderStatus || trade.OrderSubmitStatus != old.OrderSubmitStatus)
            //    {
            //        old.CancelTime = trade.CancelTime;
            //        old.OrderStatus = trade.OrderStatus;

            //        old.OrderSysID = trade.OrderSysID;
            //        old.OrderSubmitStatus = trade.OrderSubmitStatus;
            //        old.StatusMsg = trade.StatusMsg;
            //        old.QuantUnfilled = trade.QuantUnfilled;
            //        old.VolumeTraded = trade.VolumeTraded;
            //        old.ZCETotalTradedVolume = trade.ZCETotalTradedVolume;
            //    }
            //}

            //else
            //{
            //    TradeList.Add(trade);
            //}

            //public XmlSchema GetSchema()
            //{
            //    return null;
            //}

            //public void ReadXml(XmlReader reader)
            //{
            //    byte a, r, g, b;
            //    //string ARGBColor = reader.ReadContentAsString();
            //    //string[] pieces = ARGBColor.Split(new char[] { ':' });
            //    //a = byte.Parse(pieces[1]);
            //    //r = byte.Parse(pieces[2]);
            //    //g = byte.Parse(pieces[3]);
            //    //b = byte.Parse(pieces[4]);

            //    a = byte.Parse(reader.GetAttribute("A"));
            //    r = byte.Parse(reader.GetAttribute("R"));
            //    g = byte.Parse(reader.GetAttribute("G"));
            //    b = byte.Parse(reader.GetAttribute("B"));

            //    this.Color = System.Windows.Media.Color.FromArgb(a, r, g, b);
            //}

            //public void WriteXml(XmlWriter writer)
            //{
            //    //string str = string.Format("{0}:{1}:{2}:{3}", Color.A, Color.R, Color.B, Color.G);

            //    writer.WriteAttributeString("A", Color.A.ToString());
            //    writer.WriteAttributeString("R", Color.R.ToString());
            //    writer.WriteAttributeString("G", Color.G.ToString());
            //    writer.WriteAttributeString("B", Color.B.ToString());
            //}

            /// TQMain 日结，每交易日第一次启动时做日结
            /// 日结 的时候将 position 写入 ydposition
            /// 日结 时将 OpenVolume 、CloseVolume 清零

            ///// 获取 position = ydposition + OpenVolume - CloseVolume
            //public int GetPostion()
            //{
            //    return Model.Position.YdPosition + Model.Position.OpenVolume - Model.Position.CloseVolume;
            //}
        }

        private int GetInt(string tradeTime)
        {
            return int.Parse(tradeTime.Trim().Replace(":", ""));
        }
    }
}