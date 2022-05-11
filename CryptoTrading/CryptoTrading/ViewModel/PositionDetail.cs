using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryptoTrading.Model;
using CryptoTrading.TQLib;
namespace CryptoTrading.Model
{
    /// <summary>
    /// 投资者持仓明细
    /// </summary>
    public class PositionDetail : ObservableObject, IComparable<PositionDetail>
    {
        ///合约代码
        public string InstrumentID { get; set; }
        ///投资者代码
        public string InvestorID { get; set; }
        ///投机套保标志
        public HedgeType HedgeFlag { get; set; }
        ///持仓类别
        public TradeDirection Direction { get; set; }
        ///开仓日期
        public string OpenDate { get; set; }
        ///成交编号
        //public string TradeID { get; set; }
        private decimal positionsize;
        /// <summary>
        /// 持仓数量
        /// </summary>
        public decimal PositionSize { get { return positionsize; } set { positionsize = value; NotifyPropertyChanged("PositionSize"); } }
        private decimal positionavailable;
        public decimal PositionAvailable { get { return positionavailable; } set { positionavailable = value; NotifyPropertyChanged("PositionAvailable"); } }
        private decimal openPrice;
        ///开仓价
        public decimal OpenPrice { get { return openPrice; } set { openPrice = value; NotifyPropertyChanged("OpenPrice"); } }
        ///成交类型
        public TradeType TradeType { get; set; }
        ///交易所代码
        public string ExchangeID { get; set; }
        private decimal margin;
        ///投资者保证金
        public decimal Margin { get { return margin; } set { margin = value; NotifyPropertyChanged("Margin"); } }
        public string PositionID { get; set; }
        public int SortIndex;
        public int CompareTo(PositionDetail other)
        {
            if (other == null) return 1;
            SortIndex = int.Parse(other.OpenDate) - int.Parse(OpenDate);
            SortIndex = SortIndex == 0 ? int.Parse(other.PositionID) - int.Parse(PositionID) : SortIndex;
            return SortIndex;
        }
    }
}
