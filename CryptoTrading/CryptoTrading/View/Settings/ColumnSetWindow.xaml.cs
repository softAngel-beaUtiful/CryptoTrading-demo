using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace CryptoTrading
{
    /// <summary>
    /// Interaction logic for ColumnSetWindow.xaml
    /// </summary>
    public partial class ColumnSetWindow
    {
        //list is the collection for ListViewColumn to store ListView Data
        public ObservableCollection<ColumnSettingItem> ViewColumsList = new ObservableCollection<ColumnSettingItem>();

        private List<ColumnSettingItem> _ColumnSettingList = new List<ColumnSettingItem>();

        private DataGridType _DataGridType;
        //internal DataGrid dataGrid;

        //public ColumnSetWindow(DataGrid dg)
        //{
        //    InitializeComponent();
        //    dataGrid = dg;
        //    if (dataGrid.Name == null)
        //        NodeName = "MarketDataGridColumns";
        //    else
        //        NodeName = dataGrid.Name + "Columns";
        //    LoadConfiguration();
        //}

        //public ColumnSetWindow(List<GridListViewItem> lst)
        //{
        //    InitializeComponent();
        //    if (lst == null || lst.Count < 1)
        //    {
        //        return;
        //    }
        //    listViewColums.Clear();

        //    lst.Sort();

        //    foreach (var s in lst)
        //    {
        //        listViewColums.Add(s);
        //    }
        //    EditListview.ItemsSource = listViewColums;
        //}

        public ColumnSetWindow(DataGridType dataGridColumnsSetType)
        {
            InitializeComponent();

            _DataGridType = dataGridColumnsSetType;

            try {
                _ColumnSettingList = TQMain.dicMainDataGridMapping[_DataGridType].ColumnSettingList;
                LoadConfiguration();
            }
            catch { }
        }


        //MainWindow.xaml.cs的"loadConfiguration(DataGrid dg, string nodeName)"的一部分
        private void LoadConfiguration()
        {

            //switch (_DataGridType)
            //{
            //    case DataGridType.Account:
            //        _GridListViewItemList = Trader.Configuration.AccountDataGrid;
            //        break;
            //    case DataGridType.MarketData:
            //        _GridListViewItemList = Trader.Configuration.MarketDataGrid;
            //        break;
            //    case DataGridType.Instrument:
            //        _GridListViewItemList = Trader.Configuration.InstrumentDataGrid;
            //        break;
            //    case DataGridType.UnsettledOrders://
            //        _GridListViewItemList = Trader.Configuration.UnsettledOrderGrid;
            //        break;
            //    case DataGridType.TodayOrders:
            //        _GridListViewItemList = Trader.Configuration.TodayOrderGrid;
            //        break;
            //    case DataGridType.ComplexOrders:
            //        _GridListViewItemList = Trader.Configuration.ComplexOrderGrid;
            //        break;
            //    case DataGridType.SettledOrders:
            //        _GridListViewItemList = Trader.Configuration.SettledOrderGrid;
            //        break;
            //    case DataGridType.PositionDetails:
            //        _GridListViewItemList = Trader.Configuration.PositionDetailsGrid;
            //        break;
            //    case DataGridType.PositionSummary:
            //        _GridListViewItemList = Trader.Configuration.PositionSummaryGrid;
            //        break;
            //    case DataGridType.ComboPosition:
            //        _GridListViewItemList = Trader.Configuration.ComboPositionGrid;
            //        break;
            //    case DataGridType.TradeDetails:
            //        _GridListViewItemList = Trader.Configuration.TradeDetailsGrid;
            //        break;
            //    case DataGridType.TradeSummary:
            //        _GridListViewItemList = Trader.Configuration.TradeSummaryGrid;
            //        break;
            //    default:
            //        return;
            //}

            if (_ColumnSettingList == null || _ColumnSettingList.Count < 1)
            {
                return;
            }
            ViewColumsList.Clear();

            _ColumnSettingList.Sort();

            foreach (var s in _ColumnSettingList)
            {
                ViewColumsList.Add(s);
            }
            EditListview.ItemsSource = ViewColumsList;
        }

        //List<string> nodeList = Utility.GetConfigurationByNode(Trader.CfgFile, NodeName);
        //if (nodeList == null || nodeList.Count < 1)
        //{
        //    return;
        //}
        //listViewColums.Clear();
        //foreach (string s in nodeList)
        //{
        //    string[] split = s.Split(',');
        //    listViewColums.Add(new GridListViewItem { Title = split[0], Display = split[1], Name = split[2] });
        //}
        //EditListview.ItemsSource = listViewColums;

        #region 上调位置
        /// <summary>
        /// 上调方法
        /// </summary>
        /// <param name="listView"></param>
        private void ListViewUpMove(ListView myListView)
        {

            if (myListView.SelectedItem == null || myListView.SelectedIndex <= 0)
                return;
            int i = myListView.SelectedIndex;
            var temp = ViewColumsList[i];
            ViewColumsList.Remove(temp);
            ViewColumsList.Insert(i - 1, temp);
            myListView.SelectedIndex = i - 1;

        }
        #endregion
        #region 下调位置
        /// <summary>
        /// 下调方法
        /// </summary>
        /// <param name="listView"></param>
        private void ListViewDownMove(ListView myListView)
        {
            if (myListView.SelectedItem == null || myListView.SelectedIndex > myListView.Items.Count - 2)
                return;
            int i = myListView.SelectedIndex;
            var temp = ViewColumsList[i];
            ViewColumsList.Remove(temp);
            ViewColumsList.Insert(i + 1, temp);
            myListView.SelectedIndex = i + 1;

        }
        #endregion

        private void btnUp_Click(object sender, RoutedEventArgs e)
        {
            ListViewUpMove(EditListview);
        }

        private void btnDown_Click(object sender, RoutedEventArgs e)
        {
            ListViewDownMove(EditListview);
        }

        private void EditListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        //private void btnSetFontColor_Click(object sender, RoutedEventArgs e)
        //{

        //}

        //private void btnSave_Click(object sender, RoutedEventArgs e)
        //{

        //}

        public override bool Save()
        {
            _ColumnSettingList.Clear();

            short iSort = 1;
            foreach (var s in ViewColumsList)
            {
                s.Sort = iSort++;
                _ColumnSettingList.Add(s);
            }

            Trader.Configuration.Save();

            try
            {
                DataGrid dg = TQMain.dicMainDataGridMapping[_DataGridType].TQMainDataGrid;
                int SelectedIndex = dg.SelectedIndex;
                Utility.LoadConfiguration(dg, _DataGridType);
                dg.SelectedIndex = SelectedIndex;
                return true;
            }
            catch {
                return false;
            }

        }
    }
}
