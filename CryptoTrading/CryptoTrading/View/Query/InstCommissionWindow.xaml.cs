using System;
using System.Linq;
using System.Windows;
using CryptoTrading.Model;
using CryptoTrading.ViewModel;
namespace CryptoTrading
{

    /// <summary>
    /// Interaction logic for InstCommissionWindow.xaml
    /// </summary>
    public partial class InstCommissionWindow : Window
    {
        public InstCommissionWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            cmbProducts.ItemsSource = TQMainModel.dicProductIDName.Values;
            dgInstCommission.ItemsSource = TQMain.T.dicCommissionRate.Values.ToList();
        }

        private void Window_Closed(object sender, EventArgs e)
        {

        }

        private void btnQuery_Click(object sender, RoutedEventArgs e)
        {
            //根据所选合约，从交易费字典中取出交易费信息，并重新刷新DataGrid的数据
            if (cmbProducts.SelectedItem == null)
            {
                dgInstCommission.ItemsSource = TQMain.T.dicCommissionRate.Values;
                return;
            }
            string productID =TQMainModel.dicProductIDName.FirstOrDefault(x => x.Value == cmbProducts.SelectedItem.ToString()).Key;  //get first key
            dgInstCommission.ItemsSource = TQMain.T.dicCommissionRate.Values.Where(x => x.InstrumentID == productID).ToList();
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            //查询指定品种的交易费信息，并将查询的交易费信息更新到本地文件。
            if (cmbProducts.SelectedItem == null)
            {
                SimpleLogger.Logger.Log("请选择需要更新交易费的品种", SimpleLogger.LogCategory.Info);
                return;
            }
            string productID = TQMainModel.dicProductIDName.FirstOrDefault(x => x.Value == cmbProducts.SelectedItem.ToString()).Key;  //get first key
            string instrumentID = TQMainModel.dicInstrumentData.FirstOrDefault(x => x.Value.Name == productID).Key;
            TQMain.T.ReqQryInstrumentCommissionRate(new QryInstrumentCommissionRateField() {  InstrumentID= instrumentID });
        }

        private void btnUpdateAll_Click(object sender, RoutedEventArgs e)
        {
            //TODO：将查询的交易费信息更新到本地文件。
            //TQMain.T.QryAllInstrumentsCommissionRate();
        }
    }
}
