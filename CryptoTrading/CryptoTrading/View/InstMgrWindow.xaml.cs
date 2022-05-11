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

namespace TickQuant
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
           // cmbInstruments.ItemsSource = TQMain.dicProductIDName.Keys;
            cmbHedgs.ItemsSource = new List<string> { "投机", "套保", "套利" };
            dgInstMgr.ItemsSource = TQMain.T.dicMarginRate.Values;
        }

        private void Window_Closed(object sender, EventArgs e)
        {

        }

        private void btnQuery_Click(object sender, RoutedEventArgs e)
        {
            //根据所选合约和投机套保标志，从保证金率字典中取出保证金率信息，并重新刷新DataGrid的数据
            string instrumentID = txtInstrument.Text.Trim();
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
            dgInstMgr.ItemsSource = TQMain.T.dicMarginRate.Values.Where(x => x.InstrumentID == instrumentID && x.HedgeFlag==hedge);
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        { 
            //根据所选合约和投机套保标志，查询保证金率信息，并更新到本地
            string instrumentID = txtInstrument.Text.Trim();
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

        private void btnUpdateAll_Click(object sender, RoutedEventArgs e)
        {
            //TODO：查询所有合约的保证金率信息，并更新到本地
            TQMain.T.QryAllInstrumentsMarginRate();
        }
    }
}
