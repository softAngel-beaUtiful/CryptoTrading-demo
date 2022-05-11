using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using CryptoTrading.Model;
using CryptoTrading.ViewModel;

namespace CryptoTrading.View
{
    /// <summary>
    /// Interaction logic for RiskManagementWin.xaml
    /// </summary>
    public partial class RiskManagementWin : Window
    {
        TQMain Main;
        public ObservableCollection<RiskManagement> ObRiskManagement = new ObservableCollection<RiskManagement>();
        public RiskManagementWin(TQMain main)
        {
            InitializeComponent();
            Main = main;
        }
    }
    public class RiskManagement
    {
        public bool State { get; set; }
        public Dictionary<string, PositionData> dicPositionData { get; set; }   //风控目标的持仓字典
        public RiskConditions RiskCondition { get; set; }
        public bool PercentOrAmount { get; set; }
        public double Value { get; set; }
        public DateTime TrigerTime { get; set; }
        public TQMain Main;   //获取行情数据等必要的数据
        public bool CheckCondition()   //时间类条件输入时间
        {
            switch (RiskCondition)
            {
                case RiskConditions.时间平仓离场:  //if (DateTime.Now> TrigerTime) 处理清仓离场
                    break;
                case RiskConditions.时间限制开仓:  // if (DateTime.Now< TrigerTime)  //不允许开仓信号设置
                    break;
            }
            return true;
        }
        public bool CheckCondition(MajorMarketData md)    //跟行情有关条件输入行情
        {
            switch (RiskCondition)
            {
                case RiskConditions.持仓条件:
                    {
                        if (PercentOrAmount)
                        {
                            //if (GetPositionByPercent()> Value) 处理减仓动作  计算仓位占比
                        }
                        else
                        {
                            //if (GetPositionByAmount()> value) //处理减仓动作 计算仓位占用保证金金额
                        }
                    }
                    break;
                case RiskConditions.盈亏:
                    {
                        //if (GetPositionPNL()) 根据浮动盈亏和实现盈亏，减仓清仓或者禁止开新仓
                    }
                    break;
                case RiskConditions.交易次数:
                    {
                        //if (GetTotalTrades()> Value) 禁止开新仓，允许开仓按钮置为假
                    }
                    break;
                case RiskConditions.手工单止损:
                    {
                        List<string> list = new List<string>(dicPositionData.Keys);
                        if (!list.Contains(md.InstrumentID)) break;
                        //if (dicPositionData[md.InstrumentID].PositionProfit< 0-Value)  State=true;
                    }
                    break;
                case RiskConditions.手工单止盈:
                    {
                        List<string> list = new List<string>(dicPositionData.Keys);
                        if (!list.Contains(md.InstrumentID)) break;
                        //if (dicPositionData[md.InstrumentID].PositionProfit>Value)  State=true;
                    }
                    break;
                case RiskConditions.手工单移动止损:
                    //case RiskConditions.时间限制开仓:
                    //case RiskConditions.时间平仓离场:
                    {
                        List<string> list = new List<string>(dicPositionData.Keys);
                        if (!list.Contains(md.InstrumentID)) break;
                        //处理移动高低点的维护和移动止损点的维护
                        //if (dicPositionData[md.InstrumentID].TradeDirection==TradeDirection.多)
                        // {if (md.LastPrice<Highest-Value) State=true;}
                        //else {if (md.LastPrice > Lowest + Value)
                        //   State = true;}
                    }
                    break;
            }
            if (State)
            {
                ExecuteTrade();
                return true;
            }
            else
                return false;
        }
        
        public void ExecuteTrade()
        { }
    }
    public enum RiskConditions
    {
        持仓条件,     //持仓达上限，不允许开仓
        盈亏,        //亏损达线，不允许开仓，或者盈利达线，不允许再做交易，平仓离场
        交易次数,    //通过交易次数来限制开仓，不允许发单：交易次数达上限，限制开仓
        手工单止盈,   //达到盈利目标，平仓离场
        手工单止损,   //浮动亏损达上限，平仓止损离场
        手工单移动止损,  //移动止损目标达到，平仓离场
        时间限制开仓,    //早于时间不允许开仓 
        时间平仓离场,    //到时间平仓离场
    }
}
