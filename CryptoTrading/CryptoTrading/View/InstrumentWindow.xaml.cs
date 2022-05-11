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
using TickQuant.ViewModel;

namespace TickQuant
{
    /// <summary>
    /// Interaction logic for InstCommissionWindow.xaml
    /// </summary>
    public partial class InstrumentWindow : Window
    {
        public InstrumentWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            cmbExchanges.ItemsSource = new List<string> { "上期所", "中金所", "大商所", "郑商所", "CUSTOM" };// TQMain.dicProductIDName.Values;
            
            Utility.LoadConfiguration(dgInst, DataGridType.Instrument);
            dgInst.ItemsSource = TQMain.dicInstrumentData.Values;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
        }

        private void btnQuery_Click(object sender, RoutedEventArgs e)
        {
            //根据所选交易所，从合约字典中取出合约信息，并重新刷新DataGrid的数据
            string exchangeName = cmbExchanges.SelectedItem.ToString();
            if (exchangeName == "CUSTOM")
            {
                List<InstrumentData> instruments = new List<InstrumentData>();
                Trader.ExtConfig.CustomProducts.ForEach(item =>
                    {
                        instruments.Add(new InstrumentData() { CombinationType = CombinationType.期货组合, ExchangeID = exchangeName, ExchangeInstID = item.InstrumentID, InstrumentName = item.InstrumentName, IsTrading = "是", PriceTick = item.PriceTick });
                    });
                dgInst.ItemsSource = instruments;
            }
            else
            {
                dgInst.ItemsSource = TQMain.dicInstrumentData.Values.Where(x => x.ExchangeID == exchangeName);
            }
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            //查询指定交易所的合约，并将查询的合约信息更新到本地文件。
            string exchangeID = string.Empty;
            if (cmbExchanges.SelectedItem != null && !string.IsNullOrEmpty(cmbExchanges.SelectedItem.ToString()))
            {
                exchangeID = Utility.GetExchangeID(cmbExchanges.SelectedItem.ToString());
            }
            TQMain.T.ReqQryInstrument(new QryInstrumentField() { ExchangeID= exchangeID });
        }

        private void btnUpdateAll_Click(object sender, RoutedEventArgs e)
        {
            //TODO：查询所有的合约信息，并更新到本地文件。
            TQMain.T.ReqQryInstrument(new QryInstrumentField());
        }
    }
}
