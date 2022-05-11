using CryptoUserCenter.Models;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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


namespace CryptoUserCenter.Views
{   
    public enum SpreadOrRatio
    {
        Spread,
        Ratio
    }
    /// <summary>
    /// Interaction logic for EditComplexInstrumentList.xaml
    /// </summary>
    public partial class EditArbiPairList : Window
    {

        ObservableCollection<string> observablecomplexinst = new ObservableCollection<string>();
        string ip = "192.168.2.2";
        string user = "root";
        string passWord = "521052";
        public string[] ExchangeArr = new string[] { "Okex", "Okcoin", "Binance", "BitMEX", "Huobi", "Gate", "ZBcom" };       
      
        MysqlConfig mysqlConfig;
        List<string> temp;
        public EditArbiPairList(List<string> vs)
        {
            InitializeComponent();
            if (!File.Exists("MarketConfig.json"))
            {
                throw new Exception("file marketconfig.json doesnot exist");
            }
            EnteredInst.DataContext = observablecomplexinst;
            StreamReader streamReader = new StreamReader("MarketConfig.json");
            var jsonFile = streamReader.ReadToEnd();
            mysqlConfig = JToken.Parse(jsonFile)["mysql"].ToObject<MysqlConfig>();
            user = mysqlConfig.User;
            passWord = mysqlConfig.PassWord;
            temp = vs;
            EnteredInst.ItemsSource = observablecomplexinst;
            exch1.ItemsSource = ExchangeArr;
            exch2.ItemsSource = ExchangeArr;                       
            var diff = new SpreadOrRatio[] { Views.SpreadOrRatio.Spread, Views.SpreadOrRatio.Ratio };
            SpreadOrRatio.ItemsSource = diff;          
        }
        public List<string> QueryExchangeSymbols(string exchangeid)
        {
            string tableName = "exchangesymbol";
            List<string> exchangesymbols = new List<string>();
            string InitconnectionString = "Server={0};port=3306;database={1};UID={2};Password={3}";
            string connectionString = string.Format(InitconnectionString, ip, "setting", user, passWord);
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    string cmdStr = $"SELECT * FROM {tableName} where exchangeid= \"{exchangeid}\";"; // +exchangeid;
                    MySqlCommand candleCmd = new MySqlCommand(cmdStr, conn);
                    MySqlDataReader reader = candleCmd.ExecuteReader();
                    while (reader.Read())
                    {
                        
                        exchangesymbols.Add(Convert.ToString(reader["symbol"]));
                    }
                    reader.Close();
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return exchangesymbols;
        }
        private void exch1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string strs = e.AddedItems[0].ToString();
            var symbols = QueryExchangeSymbols(strs);
            this.inst1.ItemsSource = symbols;
            inst1.SelectedIndex = 0;           
        }
        private void exch2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string strs = e.AddedItems[0].ToString();
            var symbols = QueryExchangeSymbols(strs);
            this.inst2.ItemsSource = symbols;
            inst2.SelectedIndex = 0;           
        }
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            foreach (var o in observablecomplexinst)
            temp.Add(o); 
            this.Close();
            
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            string str = exch1.Text+":" + inst1.Text;
            if (this.SpreadOrRatio.SelectedIndex != -1)
            {
                str +=" "+ SpreadOrRatio.Text +" "+ exch2.Text + ":" + inst2.Text;
            }
            this.observablecomplexinst.Add(str);           
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (observablecomplexinst.Count>0 && EnteredInst.SelectedIndex>-1)
            {
                observablecomplexinst.RemoveAt(EnteredInst.SelectedIndex);                
            }
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists("InstrumentConfig.json"))
            {
                var file = File.OpenText("InstrumentConfig.JSON");
                var str = file.ReadToEnd();
                file.Close();
                var cc = JsonConvert.DeserializeObject<Dictionary<string, ObservableCollection<string>>>(str);
                foreach(var v in cc["Default"])
                    observablecomplexinst.Add(v);
            }
            else
            {   //create a new file InstrumentConfig.json
                var file = File.CreateText("InstrumentConfig.json");
                observablecomplexinst.Add("Okex:BTC-USD-220624");
                Dictionary<string, ObservableCollection<string>> dictPair = new Dictionary<string, ObservableCollection<string>>();
                dictPair["Default"] = observablecomplexinst;
                //List<KeyValuePair<string, ObservableCollection<string>>> list = new List<KeyValuePair<string, ObservableCollection<string>>>();
                //list.Add(keyValuePair);
                var ff =  JsonConvert.SerializeObject(dictPair);                
                file.Write(ff);
                file.Close();                
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var file = File.CreateText("InstrumentConfig.json");
            var d = new Dictionary<string, ObservableCollection<string>>();
            d["Default"]=observablecomplexinst;
            file.Write(JsonConvert.SerializeObject(d));
            file.Close();
        }
    }
}
