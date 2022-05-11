using CryptoTrading.Model;
using System;
using System.Collections.Generic;
using WebSocketSharp;

namespace CryptoTrading.OkexSpace
{
    public partial class OkexBase :IOkex
    {
        
        public object sendobject = new object();
        #region Public Events
        /// <summary>
        /// Occurs when the WebSocket connection has been closed.
        /// </summary>
        public event EventHandler<CloseEventArgs> OnClose;               
        /// <summary>
        /// Occurs when the <see cref="WebSocket"/> receives a message.
        /// </summary>
        public event EventHandler<string> OnHeartBeat;
        public event EventHandler<string> OnRspMessage;
        public event EventHandler<MDKLines> OnRspKline;
        public event EventHandler<ResponseDepth> OnRspDepthAll;
        public event EventHandler<ResponseDepth> OnRspDepthNew;
        public event EventHandler<FutureIndex> OnRspFutureIndex;
        public event EventHandler<ResponseLogin> OnRspLogin;
        public event EventHandler<Dictionary<string, OrderData>> OnRtnOrders;
        public event EventHandler<List<OrderResultV3>> OnRtnOrdersResultV3;
        public event EventHandler<Dictionary<string, userInfo>> OnRtnUserInfo;
        public event EventHandler<Dictionary<string, AccountCurrency>> OnRtnUserInfoV3;
        public event EventHandler<List<SwapInfo>> OnRtnAccountInfoSwap;
        //public event EventHandler<RspTrade> OnRspTrade;
        public event EventHandler<FutureMarketData> OnRspFutureTicker;
        public event EventHandler<List<FutureMarketDataV3>> OnRspFutureTickerV3;
        //public event EventHandler<MessageEventArgs> OnRspSpotTicker;
        public event EventHandler<ContractTradeInfo> OnRspContractTrade;
        public event EventHandler<ForecastPrice> OnRspForecastPrice;
        //public event EventHandler<MessageEventArgs> OnCancelTrade;
        public event EventHandler<FutureTrade> OnRtnOrderInfo;
        public event EventHandler<FutureTrade> OnRspTrade;          //成交信息
        public event EventHandler<ClassTradeResult> OnRtnTrade;     //成交简报      
        public event EventHandler<List<TradeResultV3>> OnRtnTradeV3;     //成交简报    
        public event EventHandler<AccountInfo> OnRtnAccInfo;
        public event EventHandler<ClassRspPosition> OnRtnPositions;    //更新持仓详情
        public event EventHandler<List<SummaryPositionSwapClass>> OnRtnSummaryPositionSwapV3;  //Swap更新持仓汇总
        public event EventHandler<List<SummaryPositionSwapV3>> OnRtnSummaryPositionSwapV3Q;
        public event EventHandler<List<SummaryPositionV3>> OnRtnSummaryPositionV3;  //更新持仓汇总
        
        public event EventHandler<WebSocketSharp.ErrorEventArgs> OnError;
        public event EventHandler<WebSocketSharp.ErrorEventArgs> OnFatal;
        public event EventHandler OnReqReconnect;
        public event EventHandler<string> OnRtnTextMessage;
        /// <summary>
        /// Occurs when the WebSocket connection has been re-established.
        /// </summary>
        public event EventHandler OnReconnected;
        bool IOkex.SendOrderRest(OrderData order)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
