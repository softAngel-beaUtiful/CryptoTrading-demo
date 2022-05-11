using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CryptoTrading.TQLib;
using CryptoTrading.View.Settings;

namespace CryptoTrading
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class MultipleSettingsWindow
    {
        private TQMain MyOwner;
        private bool isFirstInit = true;
        public MultipleSettingsWindow(SettingsType settingsType, TQMain owner)
        {
            InitializeComponent();
            Tag = settingsType.ToString();
            SelectSettings(settingsType);
            MyOwner = owner;
            isFirstInit = false;
        }

        private void InstrumentGroup_Settingd(object sender, RoutedEventArgs e)
        {
            SelectSettings(SettingsType.SettingInstrumentGroup);
            // Tag = SettingsType.SelectedInstrumentID.ToString();
        }

        private void CustomInstrument_Setting(object sender, RoutedEventArgs e)
        {
            SelectSettings(SettingsType.SettingCustomInstrument);
        }

        private void ManageInstrumentGroup(object sender, RoutedEventArgs e)
        {
            SelectSettings(SettingsType.ManageInstrumentGroup);
            //Tag = SettingsType.SelectedInstrumentIDGroup.ToString();
        }
        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            Save();
            Close();            
        }

        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            this.Save();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Cancel();
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Save()
        {
            foreach (var item in childContainer.Children)
            {
                try
                {
                    var v = item as TQXceedChildWindow;
                    v.Save();
                }
                catch (Exception ex)
                {
                    //MessageBox.Show(ex.Message);
                }
            }
            Trader.Configuration.Save();
            Trader.ExtConfig.Save();
        }

        private void Cancel()
        {
            foreach (var item in childContainer.Children)
            {
                try
                {
                    ((TQXceedChildWindow)item).Cancel();
                    ((TQXceedChildWindow)item).Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void btnChooseAll_Click(object sender, RoutedEventArgs e)
        {

        }

        private void EditListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //this.listview.SelectedIndex = ((ListView)sender).SelectedIndex;
        }

        private void DefaultQuantConfig_Selected(object sender, RoutedEventArgs e)
        {
            SelectSettings(SettingsType.DefaultQuant);

        }
        #region 表格设置

        private void TradingAccountColumn_Selected(object sender, RoutedEventArgs e)
        {
            SelectSettings(SettingsType.TradingAccountColumn);
        }
        private void OfferPriceColumn_Selected(object sender, RoutedEventArgs e)
        {
            SelectSettings(SettingsType.MarketColumn);
        }

        private void TradeColumn_Selected(object sender, RoutedEventArgs e)
        {
            SelectSettings(SettingsType.TradeColumn);
        }

        private void TradeSummaryColumn_Selected(object sender, RoutedEventArgs e)
        {
            SelectSettings(SettingsType.TradeSummaryColumn);
        }
        private void ManualPositionColumn_Selected(object sender, RoutedEventArgs e)
        {
            SelectSettings(SettingsType.GeneralPositionColumn);
        }

        private void StrategyPositionColumn_Selected(object sender, RoutedEventArgs e)
        {
            SelectSettings(SettingsType.StrategyPositionColumn);
        }

        private void PositionColumn_Selected(object sender, RoutedEventArgs e)
        {
            SelectSettings(SettingsType.PositionColumn);
        }

        private void PositionSummary_Selected(object sender, RoutedEventArgs e)
        {
            SelectSettings(SettingsType.PositionSummaryColumn);
        }

        private void ComboPositionColumn_Selected(object sender, RoutedEventArgs e)
        {
            SelectSettings(SettingsType.ComboPositionColumn);
        }

        private void InstrumentColumn_Selected(object sender, RoutedEventArgs e)
        {
            SelectSettings(SettingsType.InstrumentColumn);
        }

        private void OrderColumn_Selected(object sender, RoutedEventArgs e)
        {
            SelectSettings(SettingsType.OrderColumn);
        }

        private void UnsettledOrderColumn_Selected(object sender, RoutedEventArgs e)
        {
            SelectSettings(SettingsType.UnsettledOrderColumn);
        }

        private void ParkedOrderColumn_Selected(object sender, RoutedEventArgs e)
        {
            SelectSettings(SettingsType.ParkedOrderColumn);
        }

        private void SettledOrderColumn_Selected(object sender, RoutedEventArgs e)
        {
            SelectSettings(SettingsType.SettledOrderColumn);
        }

        private void CanceledOrderColumn_Selected(object sender, RoutedEventArgs e)
        {
            SelectSettings(SettingsType.CanceledOrdersColumn);
        }
        #endregion


        #region 窗体私有函数
        /// <summary>
        /// 根据设置的选项初始化子窗体
        /// </summary>
        /// <param name="settingsType">设置的选项</param>
        private void SelectSettings(SettingsType settingsType)
        {
            if (!isFirstInit && Tag.ToString() == settingsType.ToString())
            { return; }
            ClearContainerChildern();
            Tag = settingsType.ToString();
            switch (settingsType)
            {
                case SettingsType.EditingInstrumentData:
                    this.OpenWindow<EditInstrumentDataDict>(settingsType);

                    break;
                case SettingsType.SettingInstrumentGroup://设置合约组
                    this.OpenWindow<SettingInstrumentGroupWindow>(settingsType);
                    SettingInstrumentGroupWindow setInstGrpWin = new SettingInstrumentGroupWindow();
                    setInstGrpWin.WindowState = Xceed.Wpf.Toolkit.WindowState.Open;
                    setInstGrpWin.WindowStyle = WindowStyle.None;
                    childContainer.Children.Add(setInstGrpWin);
                    isFirstInit = false;
                    tviOptionalInstrument.IsSelected = true;
                    break;
                case SettingsType.SettingCustomInstrument:     //自定义组合
                    this.OpenWindow<ComboInstrumentIDWindow>(settingsType, new object[] { MyOwner });
                    //CustomInstrumentIDWindow custCntrWin = new CustomInstrumentIDWindow(MyOwner);
                    //custCntrWin.WindowState = Xceed.Wpf.Toolkit.WindowState.Open;
                    //custCntrWin.WindowStyle = WindowStyle.None;
                    //childContainer.Children.Add(custCntrWin);
                    //isFirstInit = false;
                    //this.OpenWindow<CustomInstrumentIDWindow>(settingsType);
                    tviCustomInstrument.IsSelected = true;
                    break;
                case SettingsType.ManageInstrumentGroup:     //管理合约组
                    this.OpenWindow<InstrumentGroupWindow>(settingsType);
                    //InstrumentGroupWindow mngInstGrpWin = new InstrumentGroupWindow();
                    //mngInstGrpWin.WindowState = Xceed.Wpf.Toolkit.WindowState.Open;
                    //mngInstGrpWin.WindowStyle = WindowStyle.None;
                    //childContainer.Children.Add(mngInstGrpWin);
                    //isFirstInit = false;
                    tviOptionalInstrumentGroup.IsSelected = true;
                    break;
                case SettingsType.DefaultQuant:              //默认手数
                    this.OpenWindow<DefaultQuantWindow>(settingsType);
                    //DefaultQuantWindow DefaultQuantWin = new DefaultQuantWindow();
                    //DefaultQuantWin.WindowState = Xceed.Wpf.Toolkit.WindowState.Open;
                    //DefaultQuantWin.WindowStyle = WindowStyle.None;
                    //childContainer.Children.Add(DefaultQuantWin);
                    //isFirstInit = false;
                    tviDefaultQuant.IsSelected = true;
                    break;
                case SettingsType.OrderBoard:
                    this.OpenWindow<OrderBoardWindow>(settingsType);
                    //OrderBoardWindow orderBoardWin = new OrderBoardWindow();
                    //orderBoardWin.WindowState = Xceed.Wpf.Toolkit.WindowState.Open;
                    //orderBoardWin.WindowStyle = WindowStyle.None;
                    //childContainer.Children.Add(orderBoardWin);
                    //isFirstInit = false;
                    //tviOrderBoardWin.IsSelected = true;
                    break;
                case SettingsType.CommonDisplay:
                    this.OpenWindow<CommonDisplayWindow>(settingsType);
                    //CommonDisplayWindow commonDisplayWin = new CommonDisplayWindow();
                    //commonDisplayWin.WindowState = Xceed.Wpf.Toolkit.WindowState.Open;
                    //commonDisplayWin.WindowStyle = WindowStyle.None;
                    //childContainer.Children.Add(commonDisplayWin);
                    //isFirstInit = false;
                    tviCommonDispaly.IsSelected = true;
                    break;
                case SettingsType.Color:
                    this.OpenWindow<ColorSetWindow>(settingsType);
                    //ColorSetWindow colorSetWin = new ColorSetWindow();
                    //colorSetWin.WindowState = Xceed.Wpf.Toolkit.WindowState.Open;
                    //colorSetWin.WindowStyle = WindowStyle.None;
                    //childContainer.Children.Add(colorSetWin);
                    //isFirstInit = false;
                    tviColor.IsSelected = true;
                    break;
                case SettingsType.NoticesSet:
                    this.OpenWindow<NoticesSetWindow>(settingsType);
                    //NoticesSetWindow noticeSetWin = new NoticesSetWindow();
                    //noticeSetWin.WindowState = Xceed.Wpf.Toolkit.WindowState.Open;
                    //noticeSetWin.WindowStyle = WindowStyle.None;
                    //childContainer.Children.Add(noticeSetWin);
                    //isFirstInit = false;
                    tviNotices.IsSelected = true;
                    break;
                case SettingsType.LocalDataMaintain:
                    this.OpenWindow<LocalDataMaintainSetWindow>(settingsType);
                    //LocalDataMaintainSetWindow localMaintainSet = new LocalDataMaintainSetWindow();
                    //localMaintainSet.WindowState = Xceed.Wpf.Toolkit.WindowState.Open;
                    //localMaintainSet.WindowStyle = WindowStyle.None;
                    //childContainer.Children.Add(localMaintainSet);
                    //isFirstInit = false;
                    tviLocalDataMaintain.IsSelected = true;
                    break;
                case SettingsType.TradeShortcutKeyCommon:
                    this.OpenWindow<TradeCommonWindow>(settingsType);
                    //TradeCommonWindow tradeCommonWin = new TradeCommonWindow();
                    //tradeCommonWin.WindowState = Xceed.Wpf.Toolkit.WindowState.Open;
                    //tradeCommonWin.WindowStyle = WindowStyle.None;
                    //childContainer.Children.Add(tradeCommonWin);
                    //isFirstInit = false;
                    tviTradeCommon.IsSelected = true;
                    break;
                case SettingsType.TradeShortcutKeySet:
                    this.OpenWindow<TradeShortcutKeyWindow>(settingsType);
                    //TradeShortcutKeyWindow tradeShortcutKeySetWin = new TradeShortcutKeyWindow();
                    //tradeShortcutKeySetWin.WindowState = Xceed.Wpf.Toolkit.WindowState.Open;
                    //tradeShortcutKeySetWin.WindowStyle = WindowStyle.None;
                    //childContainer.Children.Add(tradeShortcutKeySetWin);
                    //isFirstInit = false;
                    tviTradeShortcutKey.IsSelected = true;
                    break;
                case SettingsType.TradingAccountColumn:
                    this.OpenWindow<ColumnSetWindow>(settingsType, new object[] { DataGridType.Account });
                    tviTradingAccount.IsSelected = true;
                    break;
                case SettingsType.MarketColumn:
                    this.OpenWindow<ColumnSetWindow>(settingsType, new object[] { DataGridType.MarketData });
                    tviMarketData.IsSelected = true;
                    break;
                case SettingsType.InstrumentColumn:
                    this.OpenWindow<ColumnSetWindow>(settingsType, new object[] { DataGridType.Instrument });
                    tviInstrument.IsSelected = true;
                    break;
                case SettingsType.UnsettledOrderColumn:
                    this.OpenWindow<ColumnSetWindow>(settingsType, new object[] { DataGridType.UnsettledOrders });
                    tviUnsettledOrder.IsSelected = true;
                    break;
                case SettingsType.OrderColumn:
                    this.OpenWindow<ColumnSetWindow>(settingsType, new object[] { DataGridType.TodayOrders });
                    tviOrder.IsSelected = true;
                    break;
                case SettingsType.ParkedOrderColumn:
                    this.OpenWindow<ColumnSetWindow>(settingsType, new object[] { DataGridType.ComplexOrders });
                    tviParkedOrder.IsSelected = true;
                    break;
                case SettingsType.SettledOrderColumn:
                    this.OpenWindow<ColumnSetWindow>(settingsType, new object[] { DataGridType.SettledOrders });
                    tviSettledOrder.IsSelected = true;
                    break;
                case SettingsType.CanceledOrdersColumn:
                    this.OpenWindow<ColumnSetWindow>(settingsType, new object[] { DataGridType.CanceledOrders });
                    tviCanceledOrder.IsSelected = true;
                    break;
                case SettingsType.GeneralPositionColumn:
                    this.OpenWindow<ColumnSetWindow>(settingsType, new object[] { DataGridType.ManualPosition });
                    tviManualPosition.IsSelected = true;
                    break;
                case SettingsType.StrategyPositionColumn:
                    this.OpenWindow<ColumnSetWindow>(settingsType, new object[] { DataGridType.StrategyPosition });
                    tviStrategyPosition.IsSelected = true;
                    break;
                case SettingsType.PositionColumn:
                    this.OpenWindow<ColumnSetWindow>(settingsType, new object[] { DataGridType.PositionDetails });
                    tviPosition.IsSelected = true;
                    break;
                case SettingsType.PositionSummaryColumn:
                    this.OpenWindow<ColumnSetWindow>(settingsType, new object[] { DataGridType.PositionSummary });
                    tviPositionSummary.IsSelected = true;
                    break;
                //case SettingsType.ComboPositionColumn:
                //    this.OpenWindow<ColumnSetWindow>(settingsType, new object[] { DataGridType.ComboPosition });
                //    tviComboPosition.IsSelected = true;
                //    break;
                case SettingsType.TradeColumn:
                    this.OpenWindow<ColumnSetWindow>(settingsType, new object[] { DataGridType.TradeDetails });
                    tviTrade.IsSelected = true;
                    break;
                case SettingsType.TradeSummaryColumn:
                    this.OpenWindow<ColumnSetWindow>(settingsType, new object[] { DataGridType.TradeSummary });
                    tviTradeSummary.IsSelected = true;
                    break;

            }
        }

        /// <summary>
        /// 根据设置选项初始化表格设置的子窗体
        /// </summary>
        /// <param name="settingsType"></param>
        private void SetGridColumn(SettingsType settingsType)
        {
            if (!isFirstInit && Tag.ToString() == settingsType.ToString())
            { return; }
            //ClearContainerChildern();
            Tag = settingsType.ToString();
            ColumnSetWindow columnSetWin = null;
            switch (settingsType)
            {
                case SettingsType.TradingAccountColumn:
                    columnSetWin = new ColumnSetWindow(/*TQMain.T.main.AccountDataGrid,*/ DataGridType.Account);
                    //columnSetWin = new ColumnSetWindow(Trader.Configuration.AccountDataGrid);
                    isFirstInit = false;
                    tviTradingAccount.IsSelected = true;
                    break;
                case SettingsType.MarketColumn:
                    columnSetWin = new ColumnSetWindow(/*TQMain.T.main.MarketDataGrid,*/  DataGridType.MarketData);
                    //columnSetWin = new ColumnSetWindow(Trader.Configuration.MarketDataGrid);
                    isFirstInit = false;
                    tviMarketData.IsSelected = true;
                    break;
                case SettingsType.InstrumentColumn:
                    columnSetWin = new ColumnSetWindow(/*TQMain.T.main.InstrumentDataGrid,*/ DataGridType.Instrument);
                    isFirstInit = false;
                    tviInstrument.IsSelected = true;
                    break;
                case SettingsType.UnsettledOrderColumn:
                    columnSetWin = new ColumnSetWindow(/*TQMain.T.main.UnsettledOrderGrid,*/  DataGridType.UnsettledOrders);
                    isFirstInit = false;
                    tviUnsettledOrder.IsSelected = true;
                    break;
                case SettingsType.OrderColumn:
                    columnSetWin = new ColumnSetWindow(/*TQMain.T.main.TodayOrdersGrid,*/  DataGridType.TodayOrders);
                    isFirstInit = false;
                    tviOrder.IsSelected = true;
                    break;
                case SettingsType.ParkedOrderColumn:
                    columnSetWin = new ColumnSetWindow(/*TQMain.T.main.ComplexOrderGrid,*/ DataGridType.ComplexOrders);
                    isFirstInit = false;
                    tviParkedOrder.IsSelected = true;
                    break;
                case SettingsType.SettledOrderColumn:
                    columnSetWin = new ColumnSetWindow(/*TQMain.T.main.SettledOrderGrid,*/  DataGridType.SettledOrders);
                    isFirstInit = false;
                    tviSettledOrder.IsSelected = true;
                    break;
                case SettingsType.CanceledOrdersColumn:
                    columnSetWin = new ColumnSetWindow(/*TQMain.T.main.SettledOrderGrid,*/  DataGridType.CanceledOrders);
                    isFirstInit = false;
                    tviSettledOrder.IsSelected = true;
                    break;
                case SettingsType.PositionColumn:
                    columnSetWin = new ColumnSetWindow(/*TQMain.T.main.PositionDetailsGrid,*/  DataGridType.PositionDetails);
                    isFirstInit = false;
                    tviPosition.IsSelected = true;
                    break;
                case SettingsType.PositionSummaryColumn:
                    columnSetWin = new ColumnSetWindow(/*TQMain.T.main.PositionSummaryGrid,*/ DataGridType.PositionSummary);
                    isFirstInit = false;
                    tviPositionSummary.IsSelected = true;
                    break;
                //case SettingsType.ComboPositionColumn:
                //    columnSetWin = new ColumnSetWindow(/*TQMain.T.main.ComboPositionGrid,*/  DataGridType.ComboPosition);
                //    isFirstInit = false;
                //    tviComboPosition.IsSelected = true;
                //    break;
                case SettingsType.TradeColumn:
                    columnSetWin = new ColumnSetWindow(/*TQMain.T.main.TradeRecordDetailsGrid, */ DataGridType.TradeDetails);
                    isFirstInit = false;
                    tviTrade.IsSelected = true;
                    break;
                case SettingsType.TradeSummaryColumn:
                    columnSetWin = new ColumnSetWindow(/*TQMain.T.main.TradeRecordSummaryGrid,*/ DataGridType.TradeSummary);
                    isFirstInit = false;
                    tviTradeSummary.IsSelected = true;
                    break;
            }
            columnSetWin.WindowState = Xceed.Wpf.Toolkit.WindowState.Open;
            columnSetWin.WindowStyle = WindowStyle.None;
            childContainer.Children.Add(columnSetWin);
        }

        /// <summary>
        /// 清除MDI容器的所有子窗体
        /// </summary>
        private void ClearContainerChildern()
        {
            foreach (TQXceedChildWindow item in childContainer.Children)
            {
                try
                {
                    item.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }


        private void OpenWindow<T>(SettingsType tag) where T : TQXceedChildWindow
        {
            this.OpenWindow<T>(tag, null);
        }
        private void OpenWindow<T>(SettingsType tag, params object[] args) where T : TQXceedChildWindow
        {
            try
            {
                int childCount = childContainer.Children.Count;
                //bool isExist = false;
                for (int i = 0; i < childCount; i++)
                {
                    TQXceedChildWindow item = childContainer.Children[i] as TQXceedChildWindow;
                    if (item.Tag != null && item.Tag.ToString() == tag.ToString())
                    {
                        for (int j = 0; j < childCount; j++)
                        {
                            if (j != i)
                            {
                                childContainer.Children[j].Visibility = Visibility.Collapsed;
                            }
                        }

                        item.Reload();
                        return;
                    }
                }

                T Win = (T)Activator.CreateInstance(typeof(T), args);
                Win.WindowState = Xceed.Wpf.Toolkit.WindowState.Open;
                Win.WindowStyle = WindowStyle.None;
                Win.Tag = tag;
                childContainer.Children.Add(Win);
                isFirstInit = false;
            }
            catch (Exception ex)
            {
                Utility.WriteMemFile(ex.ToString());
            }
        }
        #endregion

        private void tviCommonDispaly_Selected(object sender, RoutedEventArgs e)
        {
            SelectSettings(SettingsType.CommonDisplay);
        }

        private void tviOrderBoard_Selected(object sender, RoutedEventArgs e)
        {
            SelectSettings(SettingsType.OrderBoard);
        }

        private void tviColor_Selected(object sender, RoutedEventArgs e)
        {
            SelectSettings(SettingsType.Color);
        }

        private void tviNotices_Selected(object sender, RoutedEventArgs e)
        {
            try
            {
                SelectSettings(SettingsType.NoticesSet);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        private void tviLocalDataMaintain_Selected(object sender, RoutedEventArgs e)
        {
            try
            {
                SelectSettings(SettingsType.LocalDataMaintain);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void tviTradeCommon_Selected(object sender, RoutedEventArgs e)
        {
            try
            {
                SelectSettings(SettingsType.TradeShortcutKeyCommon);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void tviTradeShortcutKey_Selected(object sender, RoutedEventArgs e)
        {
            try
            {
                SelectSettings(SettingsType.TradeShortcutKeySet);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            foreach (var item in childContainer.Children)
            {
                try
                {
                    ((TQXceedChildWindow)item).Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);

                    e.Cancel = true;
                    break;
                }
            }
        }

        private void tviEditInstrumentData_Selected(object sender, RoutedEventArgs e)
        {
            SelectSettings(SettingsType.EditingInstrumentData);
        }
    }
}
