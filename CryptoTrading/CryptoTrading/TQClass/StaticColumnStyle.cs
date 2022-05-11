using System.Windows;
using System.Windows.Media;
using CryptoTrading.TQLib;
using Brushes = System.Drawing.Brushes;

namespace CryptoTrading
{
    public static class StaticGridStyle
    {
        /// <summary>
        /// 根据 Grid 名和列名获取该列的 Style 属性设置
        /// </summary>
        /// <param name="GridName">Grid 名</param>
        /// <param name="ColumnName">列名</param>
        /// <returns>返回 TQColumnStyle 实例</returns>
        public static TQStyle GetTQColumnStyle(DataGridType GridName, string ColumnName)
        {
            TQStyle TQColumnStyle = new TQStyle();

            // 颜色
            switch (GridName)
            {
                case DataGridType.PositionSummary:
                case DataGridType.ManualPosition:
                case DataGridType.StrategyPosition:
                case DataGridType.Account:
                default:
                    switch (ColumnName)
                    {
                        case "PositionProfit":
                            TQColumnStyle.ColumnForegroundBindingProperty = System.Windows.Media.Brushes.Chartreuse;
                            break;
                        default:
                            TQColumnStyle.ColumnForegroundBindingProperty = System.Windows.Media.Brushes.Chartreuse;
                            break;
                    }
                    break;
                case DataGridType.MarketData:
                    switch (ColumnName)
                    {
                        //case "LastPrice":
                        case "Change":
                            TQColumnStyle.ColumnForegroundBindingProperty = System.Windows.Media.Brushes.Chartreuse;
                            break;
                        case "UpdateTime":
                            TQColumnStyle.ColumnForegroundBindingProperty = System.Windows.Media.Brushes.CadetBlue;
                            TQColumnStyle.ColumnBackroundBindingProperty = System.Windows.Media.Brushes.Brown;
                            break;
                            //case ""
                        default:
                            TQColumnStyle.ColumnForegroundBindingProperty = System.Windows.Media.Brushes.Chartreuse;
                            break;
                    }
                    break;               
            }

            // 文字对齐
            switch (GridName)
            {
                case DataGridType.Account:
                    switch (ColumnName)
                    {
                        case "BrokerName":
                        case "InvestorID":
                        case "SettlementID":
                            TQColumnStyle.ColumnHorizontalAlignment = HorizontalAlignment.Center;
                            break;
                        default:
                            TQColumnStyle.ColumnHorizontalAlignment = HorizontalAlignment.Right;
                            break;
                    }
                    break;
                case DataGridType.Accountregister:
                    switch (ColumnName)
                    {
                        //case "InstrumentID":
                        //    TQCS.HorizontalAlignment = HorizontalAlignment.Right;
                        //    break;
                        default:
                            TQColumnStyle.ColumnHorizontalAlignment = HorizontalAlignment.Center;
                            break;
                    }
                    break;
                case DataGridType.Instrument:
                    switch (ColumnName)
                    {
                        case "PriceTick":
                        case "Commission":
                        case "VolumeMultiple":                       
                            TQColumnStyle.ColumnHorizontalAlignment = HorizontalAlignment.Right;
                            break;
                        default:
                            TQColumnStyle.ColumnHorizontalAlignment = HorizontalAlignment.Center;
                            break;
                    }
                    break;
                case DataGridType.MarketData:
                    switch (ColumnName)
                    {
                        case "InstrumentID":                                                             
                        case "IsCustom":
                            TQColumnStyle.ColumnHorizontalAlignment = HorizontalAlignment.Center;
                            break;
                        case "UpdateTime": //行情更新时间
                            TQColumnStyle.ColumnHorizontalAlignment = HorizontalAlignment.Left;
                            break;
                        default:
                            TQColumnStyle.ColumnHorizontalAlignment = HorizontalAlignment.Right;
                            break;
                    }
                    break;
                case DataGridType.TodayOrders:
                    switch (ColumnName)
                    {
                        case "LimitPrice":                       
                        case "StopPrice":                        
                        case "VolumeTraded":                       
                            TQColumnStyle.ColumnHorizontalAlignment = HorizontalAlignment.Right;
                            break;
                        default:
                            TQColumnStyle.ColumnHorizontalAlignment = HorizontalAlignment.Center;
                            break;
                    }
                    break;
                case DataGridType.SettledOrders:
                    switch (ColumnName)
                    {
                        case "LimitPrice":                                              
                        case "StopPrice":                       
                            TQColumnStyle.ColumnHorizontalAlignment = HorizontalAlignment.Right;
                            break;
                        default:
                            TQColumnStyle.ColumnHorizontalAlignment = HorizontalAlignment.Center;
                            break;
                    }
                    break;
                case DataGridType.ComplexOrders:
                    switch (ColumnName)
                    {
                        case "LimitPrice":                        
                            TQColumnStyle.ColumnHorizontalAlignment = HorizontalAlignment.Right;
                            break;
                        default:
                            TQColumnStyle.ColumnHorizontalAlignment = HorizontalAlignment.Center;
                            break;
                    }
                    break;
                case DataGridType.UnsettledOrders:
                    switch (ColumnName)
                    {
                        case "LimitPrice":
                        case "VolumeTotalOriginal":
                        case "MinVolume":
                        case "StopPrice":
                        case "SequenceNo":
                        case "VolumeTraded":
                        case "VolumeLeft":
                        case "BrokerOrderSeq":
                            TQColumnStyle.ColumnHorizontalAlignment = HorizontalAlignment.Right;
                            break;
                        default:
                            TQColumnStyle.ColumnHorizontalAlignment = HorizontalAlignment.Center;
                            break;
                    }
                    break;
                case DataGridType.PositionDetails:
                    switch (ColumnName)
                    {
                        case "InstrumentID":
                        case "BrokerID":
                        case "InvestorID":
                        case "Direction":
                        case "Hedge":
                        case "PositionType":
                            TQColumnStyle.ColumnHorizontalAlignment = HorizontalAlignment.Center;
                            break;
                        default:
                            TQColumnStyle.ColumnHorizontalAlignment = HorizontalAlignment.Right;
                            break;
                    }
                    break;
                case DataGridType.PositionSummary:
                case DataGridType.ManualPosition:
                case DataGridType.StrategyPosition:
                    switch (ColumnName)
                    {
                        case "AvgPrice":
                        case "CloseVolume":
                        case "OpenVolume":
                        case "PositionCost":
                        case "PositionProfit":
                        case "Position":
                        case "TodayPosition":
                        case "YdPosition":
                            TQColumnStyle.ColumnHorizontalAlignment = HorizontalAlignment.Right;
                            break;
                        default:
                            TQColumnStyle.ColumnHorizontalAlignment = HorizontalAlignment.Center;
                            break;
                    }
                    break;
                //case DataGridType.ComboPosition:
                //    switch (ColumnName)
                //    {
                //        //case "InstrumentID":
                //        //    TQCS.HorizontalAlignment = HorizontalAlignment.Right;
                //        //    break;
                //        default:
                //            TQCS.ColumnHorizontalAlignment = HorizontalAlignment.Center;
                //            break;
                //    }
                //    break;
                case DataGridType.TradeDetails:
                    switch (ColumnName)
                    {
                        case "Volume":
                        case "Commission":
                        case "SequenceNo":
                        case "VolumeTraded":
                        case "VolumeTotalOriginal":
                        case "VolumeLeft":
                        case "VolumeTotal":
                        case "Order":
                            TQColumnStyle.ColumnHorizontalAlignment = HorizontalAlignment.Right;
                            break;
                        default:
                            TQColumnStyle.ColumnHorizontalAlignment = HorizontalAlignment.Center;
                            break;
                    }
                    break;
                case DataGridType.TradeSummary:
                    switch (ColumnName)
                    {
                        case "Volume":
                        case "AvgPrice":
                        case "Commission":
                            TQColumnStyle.ColumnHorizontalAlignment = HorizontalAlignment.Right;
                            break;
                        default:
                            TQColumnStyle.ColumnHorizontalAlignment = HorizontalAlignment.Center;
                            break;
                    }
                    break;
                case DataGridType.TransferSerial:
                    switch (ColumnName)
                    {
                        case "TradeAmount":
                        case "CustFee":
                        case "BrokerFee":
                        case "ErrorID":
                            TQColumnStyle.ColumnHorizontalAlignment = HorizontalAlignment.Right;
                            break;
                        default:
                            TQColumnStyle.ColumnHorizontalAlignment = HorizontalAlignment.Center;
                            break;
                    }
                    break;
                default:
                    TQColumnStyle.ColumnHorizontalAlignment = HorizontalAlignment.Center;
                    break;
            }

            return TQColumnStyle;
        }

    }


}
