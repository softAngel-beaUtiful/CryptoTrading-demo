using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using CryptoTrading.ViewModel;
using System.Linq;

namespace CryptoTrading
{
    /// <summary>
    /// Interaction logic for CustomInstrumentWindow.xaml
    /// </summary>
    public partial class ComboInstrumentIDWindow
    {
        private ObservableCollection<ComboMarketData> comboList = new ObservableCollection<ComboMarketData>();
        private TQMain Owner;
        public ComboInstrumentIDWindow(TQMain owner)
        {
            InitializeComponent();
            Owner = owner;
        }        
        private void ComboInstrumentWin_Loaded(object sender, RoutedEventArgs e)
        {
            if (Trader.ExtConfig.Combos.Count > 0)
            {
                foreach (var item in Trader.ExtConfig.Combos)
                {
                    comboList.Add(new ComboMarketData(item.InstrumentID, item.InstrumentName,item.PriceTick));                   
                }
            }
            gridCustInstrument.ItemsSource = comboList;
        }

        private void ComboInstrumentWin_Closed(object sender, EventArgs e)
        {
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            //1.先检查自定义合约ID格式（是否为空，是否符合标准weight*InstrumentID+weight*InstrumentID）
            string instrumentID = txtNewInstrument.Text;
            if (string.IsNullOrEmpty(instrumentID))
            {
                MessageBox.Show("自定义合约ID不能为空,请重新输入");
                txtNewInstrument.Focus();
                return;
            }
            string msg = string.Empty;
            if (!ComboMarketData.InstrumentIDIsValid(instrumentID, out msg))
            {
                MessageBox.Show(msg);
                txtNewInstrument.Clear();
                txtNewInstrument.Focus();
                txtNewInstrumentName.Clear();
                return;
            }
            instrumentID = string.IsNullOrEmpty(msg)? instrumentID:msg;
            string instrumentName = string.Empty;
            if (string.IsNullOrEmpty(txtNewInstrumentName.Text))
            {
                instrumentName = instrumentID;
            }
            else
            {
                instrumentName = txtNewInstrumentName.Text;
            }
            ComboMarketData custProd = new ComboMarketData(instrumentID, instrumentName);           
            comboList.Add(custProd);
            XmlCombo xcombo = new XmlCombo() { InstrumentID = custProd.InstrumentID, InstrumentName = custProd.InstrumentName, PriceTick = custProd.PriceTick };
            Trader.ExtConfig.Combos.Add(xcombo);
            Trader.ExtConfig.Save();

            //txtNewInstrument.Clear();
            //txtNewInstrumentName.Clear();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            //txtNewInstrument.Text = string.Empty;
            txtNewInstrument.Focus();
            //txtNewInstrumentName.Text = string.Empty;
        }

        private void btnModify_Click(object sender, RoutedEventArgs e)
        {
            if (gridCustInstrument.SelectedIndex < 0)
            {
                MessageBox.Show("请选择需修改的自定义合约");
                return;
            }
            XmlCombo custProduct = gridCustInstrument.SelectedItem as XmlCombo;

            txtNewInstrument.Text = custProduct.InstrumentID;
            txtNewInstrumentName.Text = custProduct.InstrumentName;
            Save();
        }
        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            if (gridCustInstrument.SelectedIndex < 0)
            {
                MessageBox.Show("请选择需要删除的自定义合约");
                return;
            }
            comboList.RemoveAt(gridCustInstrument.SelectedIndex);
            Save();
        }


        private void gridCustInstrument_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            string str = string.Empty;
            var custProd = e.Row;
        }
        public override bool Save()
        {
            //被删除的
            int itemIndex = -1;
            int originCount = Trader.ExtConfig.Combos.Count;
            var dfltQuantLst = comboList.ToList();
            for (int i = 0; i < originCount; i++)
            {
                itemIndex = dfltQuantLst.FindIndex(x => x.InstrumentID == Trader.ExtConfig.Combos[i].InstrumentID);
                if (itemIndex < 0)
                {
                    Trader.ExtConfig.Combos.RemoveAt(i);
                    originCount--;
                }
            }

            //新增或删除的
            foreach (var dfltQuant in comboList)
            {
                Trader.ExtConfig.AddOrUpdateCombo(new XmlCombo()
                {                      
                    InstrumentName = dfltQuant.InstrumentName,
                    InstrumentID = dfltQuant.InstrumentID,
                    PriceTick = dfltQuant.PriceTick
                });
            }
            Trader.ExtConfig.Save();            
            return true;
        }

        public override bool Cancel()
        {
            return base.Cancel();
        }
    }
}
