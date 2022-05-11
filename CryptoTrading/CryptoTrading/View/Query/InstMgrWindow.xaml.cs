using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using CryptoTrading.Model;
using CryptoTrading.ViewModel;
namespace CryptoTrading
{
    /// <summary>
    /// Interaction logic for InstMgrWindow.xaml
    /// </summary>
    public partial class InstMgrWindow : Window
    {
        public InstMgrWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
           // cmbInstruments.ItemsSource = TQMainVM.dicProductIDName.Keys;
            cmbHedgs.ItemsSource = new List<string> { "投机", "套保", "套利" };
            dgInstMgr.ItemsSource = TQMain.T.dicMarginRate.Values;
        }

        private void Window_Closed(object sender, EventArgs e)
        {

        }

        private void btnQuery_Click(object sender, RoutedEventArgs e)
        {
            //根据所选合约和投机套保标志，从保证金率字典中取出保证金率信息，并重新刷新DataGrid的数据
            //支持合约的模糊查询
            string instrumentID = txtInstrument.Text.Trim();
            if (cmbHedgs.SelectedItem == null)
            {
                dgInstMgr.ItemsSource = TQMain.T.dicMarginRate.Values.Where(x => x.InstrumentID.Contains(instrumentID));
                return;
            }
            HedgeType hedge = HedgeType.投机;
            switch (cmbHedgs.SelectedItem.ToString())
            {
                case "套利":
                    hedge = HedgeType.套利;
                    break;
                case "套保":
                    hedge = HedgeType.套保;
                    break;
            }
            dgInstMgr.ItemsSource = TQMain.T.dicMarginRate.Values.Where(x => x.InstrumentID.Contains(instrumentID) && x.HedgeFlag==hedge);
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            //根据所选合约和投机套保标志，查询保证金率信息，并更新到本地
            string instrumentID = txtInstrument.Text.Trim();
            if (!TQMainModel.dicInstrumentData.ContainsKey(instrumentID))
            {
                MessageBox.Show(string.Format("{0}不是交易所合约,请重新输入", instrumentID));
                return;
            }
            if (cmbHedgs.SelectedItem == null)
            {
                MessageBox.Show("请选择需要更新的合约的投机套保标志");
                return;
            }
            HedgeType hedge = HedgeType.投机;
            switch (cmbHedgs.SelectedItem.ToString())
            {
                case "套利":
                    hedge = HedgeType.套利;
                    break;
                case "套保":
                    hedge = HedgeType.套保;
                    break;
            }
            TQMain.T.ReqQryInstrumentMarginRate(new QryInstrumentMarginRateField() { InstrumentID = instrumentID, HedgeFlag = (char)hedge });
        }
       
    }
}
