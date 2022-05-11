using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xceed.Wpf.Toolkit;
using System.Linq;
using System.Windows.Input;
using System.Xml;
using CryptoTrading.TQLib;

namespace CryptoTrading
{
    /// <summary>
    /// Interaction logic for DefaultQuantWindow.xaml
    /// </summary>
    public partial class DefaultQuantWindow
    {
        private ObservableCollection<DefaultQuantSet> defaultQuantList = new ObservableCollection<DefaultQuantSet>();
        public DefaultQuantWindow()
        {          
            InitializeComponent();
        }

        private bool isAddRow = false;
        private void btnAdd_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            isAddRow = true;
            gridDefaultQuant.CanUserAddRows = true;
        }

        private void btnDel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if(gridDefaultQuant.SelectedIndex<0)
            {
                return;
            }
            DefaultQuantSet de = gridDefaultQuant.SelectedItem as DefaultQuantSet;
            defaultQuantList.Remove(de);
        }

        private void DefaultQuant_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (Trader.Configuration.DefaultQuantSettings.Count > 0)
            {
                foreach (var item in Trader.Configuration.DefaultQuantSettings)
                {
                    defaultQuantList.Add(new DefaultQuantSet() {  ID=item.ID,InstrumentID=item.InstrumentID,CmdKey=item.CmdKey,Quant=item.Quant});
                }
            }
            gridDefaultQuant.ItemsSource = defaultQuantList;
            txtDefaultQuant.Text = Trader.Configuration.DefaultQuant.ToString();
        }

        private void DefaultQuant_Closed(object sender, System.EventArgs e)
        {
            //释放资源、保存设置数据
            Save();
        }

        private void btnMoveUp_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (gridDefaultQuant.SelectedItem == null || gridDefaultQuant.SelectedIndex <= 0)
                return;
            int i = gridDefaultQuant.SelectedIndex;
            var temp = gridDefaultQuant.Items[i] as DefaultQuantSet;
            defaultQuantList.Remove(temp);
            defaultQuantList.Insert(i - 1, temp);
            gridDefaultQuant.SelectedIndex = i - 1;
        }

        private void btnMoveDown_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (gridDefaultQuant.SelectedItem == null || gridDefaultQuant.SelectedIndex > gridDefaultQuant.Items.Count - 2)
                return;
            int i = gridDefaultQuant.SelectedIndex;
            var temp = gridDefaultQuant.Items[i] as DefaultQuantSet;
            defaultQuantList.Remove(temp);
            defaultQuantList.Insert(i + 1, temp);
            gridDefaultQuant.SelectedIndex = i + 1;
        }

        private void gridDefaultQuant_RowEditEnding(object sender, System.Windows.Controls.DataGridRowEditEndingEventArgs e)
        {
            DefaultQuantSet set = e.Row.Item as DefaultQuantSet;
            if (CustomCommands.ExistInAnotherSoftware(set.CmdKey))
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("该快捷键与其它软件冲突，请重新输入");
                return;
            }
            if (CustomCommands.Exist(set.CmdKey))
            {
                Xceed.Wpf.Toolkit.MessageBox.Show(string.Format("该快捷键({0})已经被系统注册，请重新输入", set.CmdKey));
                return;
            }
            if (isAddRow)
            {
                var lst = from c in defaultQuantList
                          orderby c.ID descending
                          select c.ID;
                int maxID = lst.ToList()[0];
                set.ID = maxID + 1;
            }
            else
            {
                int itemIndex = defaultQuantList.ToList().FindIndex(x => x.ID == set.ID);
                defaultQuantList[itemIndex] = set;
            }
        }
        

        private void txtDefaultQuant_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtDefaultQuant.Text))
                {
                    return;
                }
                int dfltQuant;
                if (!int.TryParse(txtDefaultQuant.Text, out dfltQuant))
                {
                    MessageBox.Show(string.Format("{0}不是整数，请重新输入", txtDefaultQuant.Text));
                    txtDefaultQuant.Clear();
                    return;
                }
                else
                {
                    if (dfltQuant < 0)
                    {
                        MessageBox.Show(string.Format("{0}不能小于零，请重新输入", txtDefaultQuant.Text));
                        txtDefaultQuant.Clear();
                        return;
                    }
                }
            }
            catch
            {
                MessageBox.Show(string.Format("{0}不是正整数，请重新输入", txtDefaultQuant.Text));
                txtDefaultQuant.Clear();
                return;
            }
        }


        public override bool Save()
        {
            isAddRow = false;
            gridDefaultQuant.CanUserAddRows = false;
            //被删除的
            int itemIndex = -1;
            int originCount = Trader.Configuration.DefaultQuantSettings.Count;
            var dfltQuantLst = defaultQuantList.ToList();
            for (int i = 0; i < originCount; i++)
            {
                itemIndex = dfltQuantLst.FindIndex(x => x.ID == Trader.Configuration.DefaultQuantSettings[i].ID);
                if (itemIndex < 0)
                {
                    Trader.Configuration.RemoveInstrumentQuantSet(Trader.Configuration.DefaultQuantSettings[i]);
                    originCount--;
                }
            }

            //新增或删除的
            foreach (var dfltQuant in defaultQuantList)
            {
                Trader.Configuration.AddOrUpdateInstrumentQuantSet(dfltQuant);
            }
            Trader.Configuration.DefaultQuant = int.Parse(txtDefaultQuant.Text);

            return true;
        }

        public override bool Cancel()
        {
            return base.Cancel();
        }

    }
}
