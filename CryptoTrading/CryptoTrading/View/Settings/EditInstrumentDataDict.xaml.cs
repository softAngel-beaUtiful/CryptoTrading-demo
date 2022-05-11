using CryptoTrading;
using CryptoTrading.Model;
using CryptoTrading.ViewModel;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CryptoTrading.View.Settings
{
    public class BackgroundData : CryptoTrading.TQLib.ObservableObject
    {
        private string tinstrumentid;
        public string tInstrumentID
        {
            get { return tinstrumentid; }
            set
            {
                if (tinstrumentid != value)
                {
                    tinstrumentid = value;
                    NotifyPropertyChanged("tInstrumentID");
                }
            }
        }
        private string tinstrumentname;
        public string tInstrumentName
        {
            get { return tinstrumentname; }
            set { if (tinstrumentname != value) { tinstrumentname = value; NotifyPropertyChanged("tInstrumentName"); } }
        }
        private EnuExchangeID texchange;
        public EnuExchangeID tExchange
        {
            get { return texchange; }
            set { if (texchange != value) { texchange = value; NotifyPropertyChanged("tExchange"); } }
        }
        private double tpricetick;
        public double tPriceTick
        {
            get { return tpricetick; }
            set
            {
                if (tpricetick != value) { tpricetick = value; NotifyPropertyChanged("tPriceTick"); }
            }
        }
        private int tcontractvalue;
        public int tContractValue
        {
            get { return tcontractvalue; }
            set { if (tcontractvalue != value) { tcontractvalue = value; NotifyPropertyChanged("tContractValue"); } }
        }
    }
    /// <summary>
    /// EditInstrumentDataDict.xaml 的交互逻辑
    /// </summary>
    public partial class EditInstrumentDataDict
    {
        public BackgroundData gd;
        public ObservableCollection<InstrumentData> oInstrumentData;
        public EditInstrumentDataDict()
        {
            InitializeComponent();
            gd = new BackgroundData();
            InstrumentEditing.DataContext = gd;
            oInstrumentData = new ObservableCollection<InstrumentData>();
            foreach (var r in TQMainModel.dicInstrumentData)
            {
                oInstrumentData.Add(r.Value);
            }
            gridInstrumentDataDict.ItemsSource = oInstrumentData;                      
            if (gridInstrumentDataDict.HasItems)
                gridInstrumentDataDict.SelectedIndex = 0;           
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {

        }
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            InstrumentData id;
            if (TQMainModel.dicInstrumentData.TryGetValue(gd.tInstrumentID, out id))
            {
                TQMainModel.dicInstrumentData.Remove(gd.tInstrumentID);
                oInstrumentData.RemoveAt(oInstrumentData.ToList().FindIndex((x) => x.InstrumentID == id.InstrumentID));
            }
        }
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            InstrumentData id;
            if (!TQMainModel.dicInstrumentData.TryGetValue(gd.tInstrumentID, out id))
            {
                id = new InstrumentData()
                {
                    InstrumentID = gd.tInstrumentID,
                    Name = gd.tInstrumentName,
                    PriceTick = gd.tPriceTick,
                    ExchangeID = gd.tExchange,
                    ContractValue = gd.tContractValue
                };
                TQMainModel.dicInstrumentData.Add(gd.tInstrumentID, id);
                Dispatcher.Invoke(()=>oInstrumentData.Add(id));
            }
            else
            {                
                id = new InstrumentData() { InstrumentID = gd.tInstrumentID, Name = gd.tInstrumentName, PriceTick = gd.tPriceTick, ExchangeID = gd.tExchange, ContractValue= gd.tContractValue };
                Dispatcher.Invoke(() =>
                {
                    int ind = oInstrumentData.ToList().FindIndex((x) => x.InstrumentID == id.InstrumentID);
                    oInstrumentData.RemoveAt(ind);
                    oInstrumentData.Add(id);
                });
            }
        }
        private void gridInstrumentDataDict_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //ObservableCollection<InstrumentData> s = (ObservableCollection<InstrumentData>)(((DataGrid)sender).ItemsSource);// ((DataGrid)sender).ItemsSource;
             //DataGrid v = (DataGrid)e.Source;

            //InstrumentData id;
            if (e.AddedItems.Count > 0)
            {
                var id = (InstrumentData)e.AddedItems[0];// { InstrumentID= e.AddedItems[0].;
                gd.tInstrumentName = id.Name;
                gd.tExchange = id.ExchangeID;
                gd.tInstrumentID = id.InstrumentID;
                gd.tPriceTick = id.PriceTick;
                gd.tContractValue = id.ContractValue;
            }
            else
            {
                var s = (ObservableCollection<InstrumentData>)(((DataGrid)sender).ItemsSource);
                gd.tInstrumentID = s[0].InstrumentID;
                gd.tInstrumentName = s[0].Name;
                gd.tExchange = s[0].ExchangeID;
                gd.tPriceTick = s[0].PriceTick;
                gd.tContractValue = s[0].ContractValue;                
            }

        }
    }
}