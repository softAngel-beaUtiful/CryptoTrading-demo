using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using System.IO;
using CryptoTrading.Model;
using CryptoTrading.ViewModel;

namespace CryptoTrading
{
    /// <summary>
    /// Interaction logic for OptionalInstrumentWindow.xaml
    /// </summary>
    public partial class SettingInstrumentGroupWindow
    {
        public SettingInstrumentGroupWindow()
        {
            InitializeComponent();
        }
        ObservableCollection<InstrumentData> instrumentList = new ObservableCollection<InstrumentData>();
        private List<InstrumentData> originInstruments = new List<InstrumentData>();
        private List<InstrumentData> custInstruments = new List<InstrumentData>();
        private string groupName=string.Empty;
        private void SettingInstrumentIDsGroupWin_Loaded(object sender, RoutedEventArgs e)
        {
            //加载自定义合约
            if (Trader.ExtConfig.Combos.Count > 0)
            {
                foreach (var prod in Trader.ExtConfig.Combos)
                {
                    custInstruments.Add(new InstrumentData() { InstrumentID = prod.InstrumentID, Name = prod.InstrumentName, PriceTick = prod.PriceTick });
                }
            }
            //设置期货交易所的类型
            List<string> exchanges = new List<string>()
            {
                "上海期货",
                "大连期货",
                "郑州期货",
                "大连组合",
                "郑州组合",
                "中金所",
                "自定义合约",
                "全部合约",
            };            
            cmbBoxExchangeID.ItemsSource = exchanges;
            cmbBoxExchangeID.SelectedItem = "全部合约";
            //对应合约类别下的合约
            //gridUnselectedInstruments.ItemsSource = TQMainVM.dicInstrumentData.Values;
            //设置自选合约组下拉框
            
            List<string> list = Trader.Configuration.GetInstrumentGroupNames();            
            cmbBoxInstrumentGroup.ItemsSource = list;
            cmbBoxInstrumentGroup.SelectedIndex = list.FindIndex(x => x == Trader.Configuration.DefaultInstrumentIDGroup);
            //加载当前合约组
            //加载对应InstrumentGroup下的自选合约
            instrumentList.Clear();                  
            groupName = cmbBoxInstrumentGroup.SelectedItem.ToString();
            List<InstrumentData> instruments = Trader.Configuration.GetInstrumentsByGroup(groupName);
            if (instruments != null && instruments.Count > 0)
            {
                foreach (var inst in instruments)
                {
                    instrumentList.Add(inst);
                }
                originInstruments.AddRange(instruments);
            }
            gridSelectedInstrument.ItemsSource = instrumentList;
        }

        private void cmbBoxExchangeID_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string exchange = cmbBoxExchangeID.SelectedItem.ToString();
            List<InstrumentData> instruments = new List<InstrumentData>();
            switch (exchange)
            {                
                case "自定义合约":
                    custInstruments.Clear();
                    if (Trader.ExtConfig.Combos.Count > 0)
                    {
                        foreach (var prod in Trader.ExtConfig.Combos)
                        {
                            custInstruments.Add(new InstrumentData() { InstrumentID = prod.InstrumentID, Name = prod.InstrumentName, PriceTick = prod.PriceTick });
                        }
                    }
                    instruments = custInstruments;                 
                    break;
                case "全部合约":
                    instruments = TQMainModel.dicInstrumentData.Values.ToList();
                    instruments.AddRange(custInstruments);
                    break;
            }         
            gridUnselectedInstruments.ItemsSource = instruments;
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtInputInstrument.Text) && gridUnselectedInstruments.SelectedIndex < 0)
            {
                txtInputInstrument.Text = string.Empty;
                MessageBox.Show("请选择需要增加的合约");
                return;
            }
            InstrumentData instrument;
            if (!string.IsNullOrEmpty(txtInputInstrument.Text))
            {
                int itemIndex = -1;
                if (TQMainModel.dicInstrumentData.TryGetValue(txtInputInstrument.Text, out instrument))
                {
                    instrumentList.Add(instrument);
                }
                else if ((itemIndex =custInstruments.FindIndex(x=>x.InstrumentID==txtInputInstrument.Text))>=0)
                {
                    instrumentList.Add(custInstruments[itemIndex]);
                }
                else
                {
                    MessageBox.Show("不存在输入的合约代码，请重新输入");
                }
                txtInputInstrument.Text = string.Empty;
            }
            else
            {
                instrument = gridUnselectedInstruments.SelectedItem as InstrumentData;
                instrumentList.Add(instrument);
            }
        }

        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtInputInstrument.Text) && gridSelectedInstrument.SelectedIndex < 0)
            {
                txtInputInstrument.Text = string.Empty;
                System.Windows.MessageBox.Show("请选择需要删除的合约");
                return;
            }
            if (!string.IsNullOrEmpty(txtInputInstrument.Text))
            {
                int itemIndex = instrumentList.ToList().FindIndex(x => x.InstrumentID == txtInputInstrument.Text);
                if (itemIndex >= 0)
                {
                    instrumentList.RemoveAt(itemIndex);
                }
                else
                {
                    System.Windows.MessageBox.Show("输入的合约不在合约组中,请重新输入");
                }
                txtInputInstrument.Text = string.Empty;
            }
            else
            {
                InstrumentData instrument = gridSelectedInstrument.SelectedItem as InstrumentData;
                instrumentList.Remove(instrument);
            }
        }

        private void cmbBoxInstrumentGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //先保存之前的改动
            if (string.IsNullOrEmpty(groupName)) return;
            groupName = cmbBoxInstrumentGroup.SelectedItem.ToString();
            Save();
            instrumentList.Clear();
            
            if (Trader.Configuration.InstrumentGroupList != null && Trader.Configuration.InstrumentGroupList.Count > 0)
            {
                foreach (var item in Trader.Configuration.InstrumentGroupList)
                {
                    if (item.Name == groupName)
                    {
                        foreach (var inst in item.InstrumentDataList)
                        {
                            instrumentList.Add(inst);
                        }
                        originInstruments.AddRange(item.InstrumentDataList);
                    }
                }
            }
        }

        //private void btnSetInstrumentGroup_Click(object sender, RoutedEventArgs e)
        //{
        //    OptionalInstrumentGroupWindow optCntrGrpWin = new OptionalInstrumentGroupWindow();
        //    settingsWindow.childContainer.Children.Add(optCntrGrpWin);
        //    this.Close();

        //    //OptionalInstrumentGroupWindow optCntrGrpWin = new OptionalInstrumentGroupWindow();
        //    //SettingsWindow settingsWin = new SettingsWindow(SettingsType.OptionalInstrumentGroup);
        //    //settingsWin.ShowDialog();
        //}

        private void btnMoveUp_Click(object sender, RoutedEventArgs e)
        {
            if (gridSelectedInstrument.SelectedItem == null || gridSelectedInstrument.SelectedIndex <= 0)
                return;
            int i = gridSelectedInstrument.SelectedIndex;
            var temp = gridSelectedInstrument.Items[i] as InstrumentData;
            instrumentList.Remove(temp);
            instrumentList.Insert(i - 1, temp);
            gridSelectedInstrument.SelectedIndex = i - 1;
        }

        private void btnMoveDown_Click(object sender, RoutedEventArgs e)
        {
            if (gridSelectedInstrument.SelectedItem == null || gridSelectedInstrument.SelectedIndex > gridSelectedInstrument.Items.Count - 2)
                return;
            int i = gridSelectedInstrument.SelectedIndex;
            var temp = gridSelectedInstrument.Items[i] as InstrumentData;
            instrumentList.Remove(temp);
            instrumentList.Insert(i + 1, temp);
            gridSelectedInstrument.SelectedIndex = i + 1;
        }
        public override bool Save()
        {
            //如果修改的自选合约属于当前账户使用的自选合约组，需要对主界面的行情做退订和订阅处理
            List<string> slist = new List<string>(); // Trader.InstrumentIDList;
            ComboMarketData cust;
            //维护合约组，包含自定义品种
            int ind = Trader.Configuration.InstrumentGroupList.FindIndex(x => x.Name == groupName);
            Trader.Configuration.InstrumentGroupList[ind].InstrumentDataList = instrumentList.ToList();
            foreach (var v in instrumentList)
            {
                if (v.InstrumentID.Length >= 17)
                {
                    cust = new ComboMarketData(v.InstrumentID, v.Name);
                    slist.Remove(v.InstrumentID);
                    foreach (var i in cust.ItemList)
                        if (!slist.Contains(i.InstrumentID))
                            slist.Add(i.InstrumentID);
                }
                else
                    if (!slist.Contains(v.InstrumentID)) slist.Add(v.InstrumentID);
            }
            bool A = slist.All(a => TQMainModel.SubscribedInstrumentIDs.Keys.Contains(a));
            bool B = TQMainModel.SubscribedInstrumentIDs.Keys.All(b => slist.Contains(b));
            if (A && B) //目标订阅合约没有变化
            {
                //Trader.Configuration.Save();
                //TQMain.T.main.SwitchOrUpdateInstrumentIDsGroup(groupName);
                return true;
            }
            else
            {
                InstrumentGroup instgroup = Trader.Configuration.InstrumentGroupList.Find(x => x.Name == groupName);
                instgroup.InstrumentDataList.Clear();
                //重新塑造合约组列表（含自定义品种）
                foreach (var v in instrumentList)
                {
                    if (v.InstrumentID.Length < 18)      //普通合约 
                        instgroup.InstrumentDataList.Add(TQMainModel.dicInstrumentData[v.InstrumentID]);
                    else   //自定义合约
                        instgroup.InstrumentDataList.Add(new InstrumentData() { InstrumentID = v.InstrumentID, Name = v.Name });
                }
                Trader.Configuration.Save();               
                return true;
                //List<string> instrumentIDsInCustom = new List<string>(); //保存自定义合约包含的子合约，并且该子合约不在当前合约组内
                //if (groupName == Trader.Configuration.DefaultInstrumentIDGroup)
                //{
                //    List<string> originSubscribedInstrumentIDs = new List<string>();//保存原来需要订阅行情的合约代码
                //    originSubscribedInstrumentIDs.AddRange(TQMain.Q.SubscribedInstrumentIDs);

                //    List<string> instrumentIDlist = instrumentList.Select(c => c.InstrumentID).ToList();
                //    var removedInstrumentIDs =Trader.InstrumentIDList.Except(instrumentIDlist).ToList();
                //    var addedInstrumentIDs = instrumentIDlist.Except(Trader.InstrumentIDList).ToList();

                //    CustomProduct custProduct;
                //    #region 1.处理MarketView中的MarketData 和dicCustomProduct
                //    foreach (var item in removedInstrumentIDs)
                //    {
                //        //remove element from TQMain's  MarketDataView according removedInstruments
                //        int itemIndex = TQMain.Q.main.MarketDataView.ToList().FindIndex(x => x.InstrumentID == item);
                //        TQMain.Q.main.MarketDataView.RemoveAt(itemIndex);
                //        MarketData md;
                //       // TQMain.dicMarketData.TryRemove(item, out md);
                //        Trader.InstrumentIDList.Remove(item);
                //        if(TQMainVM.dicCustomProduct.ContainsKey(item))
                //        {
                //            TQMainVM.dicCustomProduct.TryRemove(item, out custProduct);
                //        }
                //    }
                //    if (removedInstrumentIDs.Count > 0)
                //    {
                //        //string removedInstrumentIDs = string.Join(",", removedInstruments);
                //        TQMain.Q.ReqUnSubscribeMarketData(removedInstrumentIDs);
                //    }

                //    XmlCustomProduct xmlCustProd;
                //    foreach (var item in addedInstrumentIDs)
                //    {
                //        //add element to TQMain's  MarketDataView according addedInstruments

                //        Trader.AddInstrumentID(item);
                //        xmlCustProd = Trader.ExtConfig.GetXmlCustomProduct(item);
                //        if(xmlCustProd==null)
                //        {
                //            //   TQMain.dicMarketData.TryAdd(item, new MarketData(item));
                //            TQMain.Q.main.MarketDataView.Add(new MarketData(item));
                //        }
                //        else
                //        {
                //            custProduct = new CustomProduct(xmlCustProd.InstrumentID, TQMain.T.main, xmlCustProd.InstrumentName);
                //            if (custProduct.instrumentID != null)
                //            {
                //                TQMainVM.dicCustomProduct.TryAdd(custProduct.InstrumentID, custProduct);
                //                TQMain.Q.main.MarketDataView.Add(custProduct);
                //                //TQMain.dicMarketData.TryAdd(item, new MarketData(custProduct));
                //                foreach (var it in custProduct.ItemList)
                //                {
                //                    if (!Trader.InstrumentIDList.Contains(it.InstrumentID))
                //                    {
                //                        instrumentIDsInCustom.Add(it.InstrumentID);
                //                        //Trader.AddInstrumentID(it.InstrumentID);
                //                        //TQMain.T.main.MarketDataView.Add(new MarketData(it.InstrumentID));
                //                        //TQMain.dicMarketData.TryAdd(it.InstrumentID, new MarketData(it.InstrumentID));
                //                    }
                //                }
                //            }
                //        }
                //    }
                //    #endregion
                //    #region 2. 处理dicMarketData
                //    TQMain.Q.SubscribedInstrumentIDs.Clear();
                //    if (addedInstrumentIDs.Count > 0)
                //    {
                //        TQMain.Q.SubscribedInstrumentIDs.AddRange(addedInstrumentIDs);
                //    }
                //    if (instrumentIDsInCustom.Count > 0)
                //    {
                //        TQMain.Q.SubscribedInstrumentIDs.AddRange(instrumentIDsInCustom);
                //    }
                //    foreach (var item in TQMain.dicPositionSummary.Values)
                //    {
                //        if (item!=null && !string.IsNullOrEmpty(item.InstrumentID) && !TQMain.Q.SubscribedInstrumentIDs.Contains(item.InstrumentID))
                //        {
                //            TQMain.Q.SubscribedInstrumentIDs.Add(item.InstrumentID);
                //        }
                //    }

                //    //TQMain.dicMarketData.Clear();
                //    //先处理自定义合约中的子合约，再处理自定义合约的dicMarketData
                //    List<XmlCustomProduct> custProducts = new List<XmlCustomProduct>();
                //    foreach (var inst in TQMain.Q.SubscribedInstrumentIDs)
                //    {
                //        if (string.IsNullOrEmpty(inst))
                //        { continue; }
                //        MarketData md;
                //        xmlCustProd = Trader.ExtConfig.GetXmlCustomProduct(inst);
                //        if (xmlCustProd == null)
                //        {
                //            md = new MarketData(inst);
                //            TQMain.dicMarketData.TQAddOrUpdate(inst, md, (k, v) => md);
                //        }
                //        else
                //        {
                //            custProducts.Add(xmlCustProd);
                //            //md = new DepthMarketDataField(custProduct);
                //        }
                //    }
                //    foreach(var prod in custProducts)
                //    {
                //        custProduct = new CustomProduct(prod.InstrumentID, TQMain.T.main, prod.InstrumentName);
                //        TQMain.dicMarketData.TQAddOrUpdate(custProduct.InstrumentID, custProduct, (k, v) => custProduct);
                //    }
                //    #endregion
                //    #region 2. 处理需要订阅行情的合约

                //    TQMain.Q.ReqSubscribeMarketData(TQMain.Q.SubscribedInstrumentIDs);
                //    #endregion
                //   var addedQryInstrumentIds = TQMain.Q.SubscribedInstrumentIDs.Except(originSubscribedInstrumentIDs).ToList();
                //    TQMain.T.main.QryInstrumentCommissionRate(addedQryInstrumentIds);
                //    TQMain.T.main.QryInstrumentMarginRate(addedQryInstrumentIds);
                //}
                //if (Trader.Configuration.InstrumentGroupList != null && Trader.Configuration.InstrumentGroupList.Count > 0)
                //{
                //    foreach (var item in Trader.Configuration.InstrumentGroupList)
                //    {
                //        if (item.Name == groupName)
                //        {
                //            Trader.Configuration.UpdateInstrumentsByGroup(groupName, this.instrumentList.ToList());
                //            //处理自定义合约包含的子合约，并且该子合约不在当前合约组内
                //            foreach (var inst in instrumentIDsInCustom)
                //            {
                //                InstrumentData instrumentData;
                //                if (TQMainVM.dicInstrumentData.TryGetValue(inst, out instrumentData))
                //                {
                //                    item.InstrumentDataList.Add(instrumentData);
                //                }
                //            }
                //            break;
                //        }
                //    }
                //}


            }
        }

        public override bool Cancel()
        {
            return base.Cancel();
        }
    }
}
