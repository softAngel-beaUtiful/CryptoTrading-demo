using CryptoTrading.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;


namespace CryptoTrading.ViewModel
{
    public class ComboOrder
    { 
        public string InstrumentID;
        public Dictionary<string, OrderData> dicComboOrder;
    }
    public class ComboMarketData : MajorMarketData
    {
        public List<ItemInstrument> ItemList = new List<ItemInstrument>();
        public int ItemIndex = -1;  //CustomInstrument's index in Trader.InstrumentList
        decimal price;

        MajorMarketData md;
        public class ItemInstrument
        {
            public string InstrumentID;
            public int Weight;

            public MajorMarketData MajorMarketData { get; set; }
            public int ContractValue { get; internal set; }
        }
        public ComboMarketData(string InstrumentIDin, string instrumentName = "", double priceTick = 0): base(InstrumentIDin)    //cust eg. "au1606-5*ag1606"
        {
            if (string.IsNullOrEmpty(InstrumentIDin))
            {
                Utility.WriteMemLog("missing Instrument " + InstrumentIDin);
                return;
            }
            
            InstrumentName = instrumentName == "" ? "�Զ���Ʒ��" : instrumentName;
            InstrumentID = "";
            //OrderBoardOrderMode = OrderMode.Open;
            string[] temp;
            string cust = InstrumentIDin;
            IsCombo = true;
            //ItemIndex = Trader.CurrInstrumentDict.//(x => x == InstrumentIDin);
            int weight = 1;
            var cl = cust.ToList();
            if (cl[0] != '-') cust = "+" + cust;
            for (int i = 1; i < cl.Count; i++)
                if ((cl[i] == '-' || cl[i] == '+') && cl[i - 1] != ' ') cl.Insert(i, ' ');

            for (int i = 1; i < cl.Count; i++)
            {
                if ((cl[i] == '-' || cl[i] == '+') && cl[i - 1] != ' ') cl.Insert(i, ' ');
            }

            string[] strarray = new string(cl.ToArray()).Split(' ');

            string strInstrumentID;
            for (int i = 0; i < strarray.Length; i++)
            {
                temp = strarray[i].Split('*');
                strInstrumentID = string.Empty;
                if (temp == null)
                {
                    Utility.WriteMemLog("missing Instrument " + InstrumentID);                   
                    return;
                }
                if (temp.Length == 2)
                {
                    strInstrumentID = temp[1];
                    weight = int.Parse(temp[0]);
                }
                else
                {
                    char headChar = temp[0][0];
                    switch (headChar)
                    {
                        case '+':
                            weight = 1;
                            strInstrumentID = temp[0].Substring(1);
                            break;
                        case '-':
                            weight = -1;
                            strInstrumentID = temp[0].Substring(1);
                            break;
                        default:
                            strInstrumentID = temp[0];
                            weight = 1;
                            break;
                    }
                }
                
                InstrumentID += (weight > 0 ? "+" : "") + weight.ToString() + "*" + strInstrumentID;
                var it = new ItemInstrument()
                {
                    InstrumentID = strInstrumentID,
                    Weight = weight,
                    ContractValue = TQMainModel.dicInstrumentData[strInstrumentID].ContractValue,
                    MajorMarketData = new MajorMarketData() { ExchangeID = TQMainModel.dicInstrumentData[strInstrumentID].ExchangeID }
                };
                ItemList.Add(it);
                InstrumentID = InstrumentID.TrimStart('+');
            }
            //����WeightByValue
            WeightByValue = 0;
            foreach(var i in ItemList)
            {
                WeightByValue = WeightByValue < Math.Abs(i.Weight) * i.ContractValue ? Math.Abs(i.Weight) * i.ContractValue : WeightByValue;
            }
            if (priceTick == 0)
            {
                //����PriceTick
                InstrumentData instru;
                double price;
                if (TQMainModel.dicInstrumentData.Count > 0 && ItemList.Count > 0)
                {
                    if (TQMainModel.dicInstrumentData.TryGetValue(ItemList[0].InstrumentID, out instru))
                    {
                        priceTick = Math.Abs((int)(instru.ContractValue * instru.PriceTick * ItemList[0].Weight));
                    }
                    for (int i = 1; i < ItemList.Count; i++)
                    {
                        if (TQMainModel.dicInstrumentData.TryGetValue(ItemList[i].InstrumentID, out instru))
                        {
                            price = instru.ContractValue * instru.PriceTick * Math.Abs(ItemList[i].Weight);

                            priceTick = priceTick < price ? priceTick : price;                           
                        }
                        else
                        {
                            priceTick = 1;
                            break;
                        }
                    }
                }
                else priceTick = 1;
                PriceTick = priceTick;                
            }
            else
            {
                PriceTick = priceTick;
                //OrderBoard.PriceTick = priceTick;
            }
        }
        public double WeightByValue { get; set; }
        private decimal _LastPrice;
        public override decimal LastPrice
        {
            set
            {
                _LastPrice = 0;
                for (int i = 0; i < ItemList.Count; i++)
                {
                    price = TQMainModel.dicMajorMarketData[ItemList[i].InstrumentID].LastPrice==0? TQMainModel.dicMajorMarketData[ItemList[i].InstrumentID].AskPrice1:
                        TQMainModel.dicMajorMarketData[ItemList[i].InstrumentID].LastPrice;
                    _LastPrice += ItemList[i].Weight * price * ItemList[i].ContractValue;
                }
                _LastPrice /= (decimal)WeightByValue;
                if ( last!= _LastPrice)
                {
                    Change = Math.Round(value - last, 2);
                    last = Math.Round(_LastPrice,2);
                    NotifyPropertyChanged("LastPrice");                 
                    NotifyPropertyChanged("ChangeForegroundBrush");
                    if (OrderBoardPriceCanBeRefresh && IsCurrent)
                    {
                        RefreshOrderBoardPrice();
                    }                                     
                }
            }
            get
            {
                return Math.Round(_LastPrice,3);
            }
        }
        private decimal bidPrice1=0;
        public override decimal BidPrice1
        {
            set
            {
                bidPrice1 = 0;
                for (int i = 0; i < ItemList.Count; i++)
                {
                    md = TQMainModel.dicMajorMarketData[ItemList[i].InstrumentID];
                    bidPrice1 += ItemList[i].Weight * (ItemList[i].Weight > 0 ? md.BidPrice1 : md.AskPrice1)*ItemList[i].ContractValue;                   
                }
                bidPrice1 /= (decimal)WeightByValue;
                NotifyPropertyChanged("BidPrice1");                
            }
            get
            {
                return Math.Round(bidPrice1,3);
            }
        }

        private decimal askPrice1;
        public override decimal AskPrice1
        {
            set
            {
                askPrice1 = 0;
                for (int i = 0; i < ItemList.Count; i++)
                {
                    if (!TQMainModel.dicMajorMarketData.TryGetValue(ItemList[i].InstrumentID, out md))
                    {
                        Utility.WriteMemLog("�����Ҳ���" + ItemList[i].InstrumentID + "��������");
                        askPrice1 = 0;
                        break;
                    }
                    askPrice1 += ItemList[i].Weight * (ItemList[i].Weight > 0 ? md.AskPrice1 : md.BidPrice1)*ItemList[i].ContractValue ;                    
                }
                askPrice1 /= (decimal)WeightByValue;
                NotifyPropertyChanged("AskPrice1");
            }
            get
            {
                return Math.Round(askPrice1,3);
            }
        }
        private decimal bidSize1;
        public override decimal BidSize1
        {
            set
            {
                decimal r=0;
                List<decimal> ii = new List<decimal>();
                for (int i = 0; i < ItemList.Count; i++)
                {                                        
                    foreach (var va in ItemList)
                    {
                        if (!TQMainModel.dicMajorMarketData.TryGetValue(ItemList[i].InstrumentID, out md))
                        {
                            break;
                        }
                        if (ItemList[i].Weight > 0)
                        {
                            r =md.BidSize1 / Math.Abs(ItemList[i].Weight*ItemList[i].ContractValue);
                        }
                        else
                            if (ItemList[i].Weight < 0) r =  md.AskSize1 / Math.Abs(ItemList[i].Weight*ItemList[i].ContractValue);
                        ii.Add(r);
                    }                   
                }
                bidSize1 = (ii.Count()==0? 0: ii.Min())*(decimal)WeightByValue;
                NotifyPropertyChanged("BidSize1");
            }
            get
            {
                return (int)bidSize1;
            }
        }
        private decimal askSize1;
        public override decimal AskSize1
        {
            get
            {
                return (int)askSize1;
            }
            set
            {
                decimal r=0;
                List<decimal> ii = new List<decimal>();
                for (int i = 0; i < ItemList.Count; i++)
                {
                    if (!TQMainModel.dicMajorMarketData.TryGetValue(ItemList[i].InstrumentID, out md))
                    {
                        break;
                    }
                    foreach (var va in ItemList)
                    {
                        if (ItemList[i].Weight > 0)
                        {
                            r = (md.AskSize1 / Math.Abs(ItemList[i].Weight*ItemList[i].ContractValue));
                        }
                        else
                            if (ItemList[i].Weight < 0) r = (md.BidSize1 / Math.Abs(ItemList[i].Weight*ItemList[i].ContractValue));
                        ii.Add(r);
                        
                    }
                    askSize1 = (ii.Count ()==0? 0: ii.Min())*(decimal)WeightByValue;
                }                
                NotifyPropertyChanged("AskSize1");
            }
        }                     
        /// <summary>
        /// ��Լ���
        /// </summary>
        [XmlAttribute]
        public string InstrumentName { get; set; }

        /// <summary>
        ///��С�䶯��λ
        /// </summary>
        //private double _PriceTick;
        [XmlAttribute]
        override public double PriceTick
        {
            get
            {
                return _PriceTick;
                
            }
            set
            {
                _PriceTick = value;

               

                NotifyPropertyChanged("PriceTick");
            }
        }

        public void CustomProductOrderActionDelete()
        { }

        public void CustomProductOrderAction()
        { }

        /// <summary>
        /// �Զ����ԼID�Ƿ���Ч
        /// </summary>
        /// <param name="custid">�Զ����Լ</param>
        /// <param name="msg">�����Ч���򷵻ظ�ʽ������Զ����ԼID����Ч�򷵻ش�����Ϣ</param>
        /// <returns></returns>
        public static bool InstrumentIDIsValid(string custid, out string msg)
        {
            if (string.IsNullOrEmpty(custid))
            {
                MessageBox.Show("ȱʧ��Լ��" + custid);
                msg = "ȱʧ��Լ��" + custid;
                return false;
            }
            msg = "";
            string cust = custid, inst = string.Empty;
            int weight = 0;
            var cl = cust.ToList();
            if (cl[0] != '-') cust = "+" + cust;
            for (int i = 1; i < cl.Count; i++)
                if ((cl[i] == '-' || cl[i] == '+') && cl[i - 1] != ' ') cl.Insert(i, ' ');

            for (int i = 1; i < cl.Count; i++)
            {
                if ((cl[i] == '-' || cl[i] == '+') && cl[i - 1] != ' ') cl.Insert(i, ' ');
            }
            string[] strarray = new string(cl.ToArray()).Split(' ');
            string[] temp;
            for (int i = 0; i < strarray.Length; i++)
            {
                temp = strarray[i].Split('*');
                if (temp == null)
                {
                    msg = "error Instrument " + custid;
                    MessageBox.Show("����ĺ�Լ���� " + custid);
                    //InstrumentID = "����ĺ�Լ���� " + custid;
                    return false;
                }
                InstrumentData instru = new InstrumentData();
                if (temp.Length == 2)
                {
                    inst = temp[1];
                    if (!int.TryParse(temp[0], out weight) || weight == 0)
                    {
                        msg = inst + "error get weight " + weight;
                        //InstrumentID = "Ȩ�������� " + weight;
                        MessageBox.Show("Ȩ�������� " + weight);
                        return false;
                    }
                }
                else
                {
                    char headChar = temp[0][0];
                    switch (headChar)
                    {
                        case '+':
                            weight = 1;
                            inst = temp[0].Substring(1);
                            break;
                        case '-':
                            weight = -1;
                            inst = temp[0].Substring(1);
                            break;
                        default:
                            inst = temp[0];
                            weight = 1;
                            break;
                    }

                }
                if (!TQMainModel.dicInstrumentData.TryGetValue(inst, out instru))
                {
                    msg = "error get InstrumentID " + inst;
                    MessageBox.Show("δ�ҵ���Լ�� " + inst);
                    return false;
                }
                //InstrumentID += weight+"*"+inst;
                msg += (weight > 0 ? "+" : "") + weight.ToString() + "*" + inst;
            }
            msg = msg.TrimStart('+');
            return true;
        }

        //public List<int> CustomProductOrderInsert(TradeDirection pDirection,MarketData orderboard)
        //{
        //    //translate into CTP orderinsert, set InstrumentIDs, direction, price, quant.
        //    //no matter if ordertype is market or limit , direct sends out orders as market
        //    //translate pInstrument into a List:  List<ItemInstrument>;
        //    //�Զ����Լ�µ�ʱ��ǰ4λ��ʾ�Զ����Լ������ÿһ���Զ����Լ�µ�Ϊһ�飬��8λ�������С�

        //    List<int> resultlist =new List<int>();
        //    PricingMode pType;
        //    TradeDirection Direction;
        //    double pPrice;
        //    string custCntrOrderNo = (Trader.CustProductOrderNo).ToString().PadLeft(4, '0');
        //    string orderRef = string.Empty;
        //    foreach (var item in ItemList)
        //    {
        //        orderRef = custCntrOrderNo + (Trader.OrderNo).ToString().PadLeft(8, '0');
        //        if (pDirection==TradeDirection.��)
        //            Direction = item.Weight > 0 ? TradeDirection.�� : TradeDirection.��;
        //        else
        //            Direction = item.Weight > 0? TradeDirection.��: TradeDirection.��;

        //        InstrumentData inst;
        //        TQMainVM.dicInstrumentData.TryGetValue(item.InstrumentID, out inst);
        //        if (inst.ExchangeID == "SHFE")
        //        {
        //            pType = PricingMode.Preset;
        //            MarketData md = TQMain.dicMarketData[inst.InstrumentID];
        //            pPrice = (Direction == TradeDirection.��) ? md.UpperLimitPrice : md.LowerLimitPrice;
        //        }
        //        else
        //        {
        //            pType = PricingMode.Market;
        //            pPrice = 0;
        //        }
        //        //DirectionType direction = (pDirection == TradeDirection.��) ? DirectionType.�� : DirectionType.��;
        //        InputOrderField inputOrder = new InputOrderField()
        //        {
        //            InstrumentID = InstrumentID,
        //            BrokerID = TQMain.T.Broker,
        //            InvestorID = TQMain.T.Investor,
        //            CombHedgeFlag = ((char)orderboard.OrderBoardHedgeType).ToString(),
        //            TradeDirection = (char)pDirection,
        //            CombOffsetFlag = ((char)orderboard.OrderBoardHedgeType).ToString(),
        //            VolumeTotalOriginal = orderboard.OrderBoardQuant * Math.Abs(item.Weight),
        //            LimitPrice = pPrice,
        //            IsAutoSuspend = 0,
        //            ContingentCondition = (char)ContingentConditionType.����,// THOST_FTDC_CC_Immediately,
        //            ForceCloseReason = (char)ForceCloseReasonType.��ǿƽ,
        //            IsSwapOrder = 0,
        //            UserForceClose = 0,
        //            VolumeCondition = (char)VolumeConditionType.�κ�����,  //THOST_FTDC_VC_AV;
        //            MinVolume = 1,
        //            UserID = TQMain.T.Investor,
        //            RequestID = ++TQMain.T.RequestID,
        //            OrderRef = orderRef,
        //        };
        //        switch (pType)
        //        {
        //            case PricingMode.Preset:
        //                inputOrder.TimeCondition = (char)TimeConditionType.GFD;
        //                inputOrder.OrderPriceType = (char)OrderPriceType.�޼�;
        //                break;
        //            case PricingMode.Market:
        //                inputOrder.OrderPriceType = (char)OrderPriceType.�����;
        //                inputOrder.LimitPrice = 0;
        //                inputOrder.TimeCondition = (char)TimeConditionType.IOC;
        //                break;
        //            case PricingMode.FAK:
        //                inputOrder.OrderPriceType = (char)OrderPriceType.�޼�;
        //                inputOrder.VolumeCondition = (char)VolumeConditionType.�κ�����;  //��������
        //                inputOrder.TimeCondition = (char)TimeConditionType.IOC;
        //                break;
        //            case PricingMode.FOK:
        //                inputOrder.OrderPriceType = (char)OrderPriceType.�޼�;
        //                inputOrder.VolumeCondition = (char)VolumeConditionType.ȫ������;
        //                inputOrder.TimeCondition = (char)TimeConditionType.IOC;
        //                break;
        //        }
        //        resultlist.Add(TQMain.T.ReqOrderInsert(inputOrder));
        //    }
        //    return resultlist;
        //}

        //public List<int> CustomProductOrderInsert(TradeDirection pDirection,HedgeType pHedge, OffsetType pOffset, double pPrice, int pVolume)
        //{
        //    List<int> resultlist = new List<int>();
        //    PricingMode orderType;
        //    TradeDirection Direction;
        //    double Price;
        //    string custCntrOrderNo = (Trader.CustProductOrderNo).ToString().PadLeft(4, '0');
        //    string orderRef = string.Empty;
        //    foreach (var item in ItemList)
        //    {
        //        orderRef = custCntrOrderNo + (Trader.OrderNo).ToString().PadLeft(8, '0');
        //        if (pDirection == TradeDirection.��)
        //            Direction = item.Weight > 0 ? TradeDirection.�� : TradeDirection.��;
        //        else
        //            Direction = item.Weight > 0 ? TradeDirection.�� : TradeDirection.��;

        //        InstrumentData inst;
        //        TQMainVM.dicInstrumentData.TryGetValue(item.InstrumentID, out inst);
        //        if (inst.ExchangeID == "SHFE")
        //        {
        //            orderType = PricingMode.Preset;
        //            MarketData md = TQMain.dicMarketData[inst.InstrumentID];
        //            pPrice = (Direction == TradeDirection.��) ? md.UpperLimitPrice : md.LowerLimitPrice;
        //        }
        //        else
        //        {
        //            orderType = PricingMode.Market;
        //            pPrice = 0;
        //        }
        //        //DirectionType direction = pDirection;
        //        InputOrderField inputOrder = new InputOrderField()
        //        {
        //            TradeDirection = (char)Direction,
        //            InstrumentID = InstrumentID,
        //            BrokerID = TQMain.T.Broker,
        //            InvestorID = TQMain.T.Investor,
        //            CombHedgeFlag = ((char)pHedge).ToString(),
        //            CombOffsetFlag = ((char)pOffset).ToString(),
        //            VolumeTotalOriginal = pVolume * Math.Abs(item.Weight),
        //            LimitPrice = pPrice,
        //            IsAutoSuspend = 0,
        //            ContingentCondition = (char)ContingentConditionType.����,// THOST_FTDC_CC_Immediately,
        //            ForceCloseReason = (char)ForceCloseReasonType.��ǿƽ,
        //            IsSwapOrder = 0,
        //            UserForceClose = 0,
        //            VolumeCondition = (char)VolumeConditionType.�κ�����,  //THOST_FTDC_VC_AV;
        //            MinVolume = 1,
        //            UserID = TQMain.T.Investor,
        //            RequestID = ++TQMain.T.RequestID,
        //            OrderRef = orderRef,
        //        };
        //        switch (orderType)
        //        {
        //            case PricingMode.Preset:
        //                inputOrder.TimeCondition = (char)TimeConditionType.GFD;
        //                inputOrder.OrderPriceType = (char)OrderPriceType.�޼�;
        //                break;
        //            case PricingMode.Market:
        //                inputOrder.OrderPriceType = (char)OrderPriceType.�����;
        //                inputOrder.LimitPrice = 0;
        //                inputOrder.TimeCondition = (char)TimeConditionType.IOC;
        //                break;
        //            case PricingMode.FAK:
        //                inputOrder.OrderPriceType = (char)OrderPriceType.�޼�;
        //                inputOrder.VolumeCondition = (char)VolumeConditionType.�κ�����;  //��������
        //                inputOrder.TimeCondition = (char)TimeConditionType.IOC;
        //                break;
        //            case PricingMode.FOK:
        //                inputOrder.OrderPriceType = (char)OrderPriceType.�޼�;
        //                inputOrder.VolumeCondition = (char)VolumeConditionType.ȫ������;
        //                inputOrder.TimeCondition = (char)TimeConditionType.IOC;
        //                break;
        //        }
        //        resultlist.Add(TQMain.T.ReqOrderInsert(inputOrder));
        //    }
        //    return resultlist;
        //}
    }
}