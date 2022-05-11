using System;
using System.Runtime.InteropServices;
using CryptoTrading.ViewModel;
namespace CryptoTrading.Model
{
    [StructLayout(LayoutKind.Sequential)]
    public class DepthMarketDataField
    {
        public DepthMarketDataField() { }
        public DepthMarketDataField(string InstrumentID)
        {
            instrumentID = InstrumentID;
        }
        /// <summary>
        /// 交易日
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string tradingDay;
        /// <summary>
        /// 合约代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 31)]
        public string instrumentID;
        /// <summary>
        /// 交易所代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string exchangeID;
        /// <summary>
        /// 合约在交易所的代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 31)]
        public String exchangeInstID;
        /// <summary>
        /// 最新价
        /// </summary>
        public double lastPrice;
        /// <summary>
        /// 上次结算价
        /// </summary>
        public double preSettlementPrice;
        /// <summary>
        /// 昨收盘
        /// </summary>
        public double preClosePrice;
        /// <summary>
        /// 昨持仓量
        /// </summary>
        public double preOpenInterest;
        /// <summary>
        /// 今开盘
        /// </summary>
        public double openPrice;
        /// <summary>
        /// 最高价
        /// </summary>
        public double highestPrice;
        /// <summary>
        /// 最低价
        /// </summary>
        public double lowestPrice;
        /// <summary>
        /// 数量
        /// </summary>
        public int volume;
        /// <summary>
        /// 成交金额
        /// </summary>
        public double turnover;
        /// <summary>
        /// 持仓量
        /// </summary>
        public double openInterest;
        /// <summary>
        /// 今收盘
        /// </summary>
        public double closePrice;
        /// <summary>
        /// 本次结算价
        /// </summary>
        public double settlementPrice;
        /// <summary>
        /// 涨停板价
        /// </summary>
        public double upperLimitPrice;
        /// <summary>
        /// 跌停板价
        /// </summary>
        public double lowerLimitPrice;
        /// <summary>
        /// 昨虚实度
        /// </summary>
        public double preDelta;
        /// <summary>
        /// 今虚实度
        /// </summary>
        public double currDelta;
        /// <summary>
        /// 最后修改时间
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string updateTime;
        /// <summary>
        /// 最后修改毫秒
        /// </summary>
        public int updateMillisec;
        /// <summary>
        /// 申买价一
        /// </summary>
        public double bidPrice1;
        /// <summary>
        /// 申买量一
        /// </summary>
        public int bidSize1;
        /// <summary>
        /// 申卖价一
        /// </summary>
        public double askPrice1;
        /// <summary>
        /// 申卖量一
        /// </summary>
        public int askSize1;
        /// <summary>
        /// 申买价二
        /// </summary>
        public double bidPrice2;
        /// <summary>
        /// 申买量二
        /// </summary>
        public int bidSize2;
        /// <summary>
        /// 申卖价二
        /// </summary>
        public double askPrice2;
        /// <summary>
        /// 申卖量二
        /// </summary>
        public int askSize2;
        /// <summary>
        /// 申买价三
        /// </summary>
        public double bidPrice3;
        /// <summary>
        /// 申买量三
        /// </summary>
        public int bidSize3;
        /// <summary>
        /// 申卖价三
        /// </summary>
        public double askPrice3;
        /// <summary>
        /// 申卖量三
        /// </summary>
        public int askSize3;
        /// <summary>
        /// 申买价四
        /// </summary>
        public double bidPrice4;
        /// <summary>
        /// 申买量四
        /// </summary>
        public int bidSize4;
        /// <summary>
        /// 申卖价四
        /// </summary>
        public double askPrice4;
        /// <summary>
        /// 申卖量四
        /// </summary>
        public int askSize4;
        /// <summary>
        /// 申买价五
        /// </summary>
        public double bidPrice5;
        /// <summary>
        /// 申买量五
        /// </summary>
        public int bidSize5;
        /// <summary>
        /// 申卖价五
        /// </summary>
        public double askPrice5;
        /// <summary>
        /// 申卖量五
        /// </summary>
        public int askSize5;
        /// <summary>
        /// 当日均价
        /// </summary>
        public double averagePrice;
        /// <summary>
        /// 业务日期
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string actionDay;
        private int volumeMultiple;
        public virtual int VolumeMultiple
        {
            get
            {
                if (volumeMultiple == 0)
                {
                    InstrumentData instru;
                    if (TQMainModel.dicInstrumentData.TryGetValue(instrumentID, out instru))
                    {
                        volumeMultiple = instru.ContractValue;
                    }
                }

                return volumeMultiple;
            }
            set { volumeMultiple = value; }
        }
    }
}