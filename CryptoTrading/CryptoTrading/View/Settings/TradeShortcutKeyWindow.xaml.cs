using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CryptoTrading.TQLib;
using System.Collections.ObjectModel;

namespace CryptoTrading
{
    /// <summary>
    /// Interaction logic for TradeShortcutKeyWindow.xaml
    /// </summary>
    public partial class TradeShortcutKeyWindow
    {
        public TradeShortcutKeyWindow()
        {
            InitializeComponent();
        }
        ObservableCollection<TradeCmdAction> cmdActions = new ObservableCollection<TradeCmdAction>();
        private void TradeShortcutKeyWin_Loaded(object sender, RoutedEventArgs e)
        {
            if (Trader.Configuration.TradeCmdList.Count > 0)
            {
                foreach (var item in Trader.Configuration.TradeCmdList)
                {
                    cmdActions.Add(new TradeCmdAction() { ID = item.ID, CmdKey = item.CmdKey, Action = item.Action });
                }
            }
            dgTradeShortKeys.ItemsSource = cmdActions;
        }

        private void TradeShortcutKeyWin_Closed(object sender, EventArgs e)
        {
        }

        public override bool Cancel()
        {
            return true;
        }
        public override bool Save()
        {
            int itemIndex = -1;
            int originCount = Trader.Configuration.TradeCmdList.Count;
            var cmdLst = cmdActions.ToList();
            for (int i = 0; i < originCount; i++)
            {
                itemIndex = cmdLst.FindIndex(x => x.ID == Trader.Configuration.TradeCmdList[i].ID);
                if (itemIndex < 0)
                {
                    Trader.Configuration.RemoveTradeCommand(Trader.Configuration.TradeCmdList[i]);
                    originCount--;
                }
            }

            //新增或删除的
            foreach (var dfltQuant in cmdActions)
            {
                Trader.Configuration.AddOrUpdateTradeCmd(dfltQuant);
            }

            return true;
        }
    }

    public enum CommandActions
    {
        撤单,
        全撤,
        对价平仓,
        市价平仓,
        市价反手,
        清仓,
        买,
        卖
    }
}
