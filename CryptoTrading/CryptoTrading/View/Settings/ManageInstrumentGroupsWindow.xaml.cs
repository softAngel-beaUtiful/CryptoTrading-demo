using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using CryptoTrading.TQLib;

namespace CryptoTrading
{
    /// <summary>
    /// Interaction logic for OptionalInstrumentGroupWindow.xaml
    /// </summary>
    public partial class InstrumentGroupWindow
    {
        public InstrumentGroupWindow()
        {
            InitializeComponent();
        }
        ObservableCollection<InstrumentGroup> groupList = new ObservableCollection<InstrumentGroup>();
        
        private void InstrumentGroupWin_Loaded(object sender, RoutedEventArgs e)
        {
            groupList.Clear();
            Trader.Configuration.InstrumentGroupList.ForEach(item =>
            {
                groupList.Add(new InstrumentGroup() {  ID=item.ID, Name=item.Name,CmdKey=item.CmdKey,InstrumentDataList=item.InstrumentDataList});
            });
            gridOptionalInstrumentGroup.ItemsSource = groupList;
        }

        private void InstrumentGroupWin_Closed(object sender, EventArgs e)
        {
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            judge = 1;//现在为添加状态
            gridOptionalInstrumentGroup.CanUserAddRows = true;
        }

        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            if (gridOptionalInstrumentGroup.SelectedIndex < 0)
            {
                System.Windows.MessageBox.Show("请选择需要删除的合约组");
                return;
            }
            InstrumentGroup optCntrGrp = gridOptionalInstrumentGroup.SelectedItem as InstrumentGroup;
            groupList.Remove(optCntrGrp);
        }
       
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }
        int judge = 0;   //0表示编辑状态，1为添加状态。因为后面的增加和编辑都在同一个事件中，所以建一个变量来区分操作  
        private void gridOptionalInstrumentGroup_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            InstrumentGroup group = new InstrumentGroup();
            group = e.Row.Item as InstrumentGroup;
            if (CustomCommands.ExistInAnotherSoftware(group.CmdKey))
            {
               Xceed.Wpf.Toolkit.MessageBox.Show("该快捷键与其它软件冲突，请重新输入");
                return;
            }
            if (CustomCommands.Exist(group.CmdKey))
            {
                Xceed.Wpf.Toolkit.MessageBox.Show(string.Format("该快捷键({0})已经被系统注册，请重新输入", group.CmdKey));
                return;
            }
            if (judge==1)//如果处于添加状态
            {
                var lst = from c in groupList
                          orderby c.ID descending
                          select c.ID;
                int maxID = lst.ToList()[0];
                group.ID = maxID + 1;
                //groupList.Add(group);
            }
            else
            {
                int itemIndex = groupList.ToList().FindIndex(x => x.ID == group.ID);
                groupList[itemIndex] = group;
            }
        }

        public override bool Save()
        {
            judge = 0;
            gridOptionalInstrumentGroup.CanUserAddRows = false;
            //删除部分
            int itemIndex = -1;
            int originCount = Trader.Configuration.InstrumentGroupList.Count;
            var cntrGroupLst = groupList.ToList();
            for (int i=0;i< originCount; i++)
            {
                itemIndex = cntrGroupLst.FindIndex(x => x.ID == Trader.Configuration.InstrumentGroupList[i].ID);
                if (itemIndex < 0)
                {
                    Trader.Configuration.RemoveInstrumentGroup(Trader.Configuration.InstrumentGroupList[i]);
                    originCount--;
                }
            }
            //新增或修改的
            foreach (var item in groupList)
            {
                Trader.Configuration.AddOrUpdateInstrumentGroup(item);
            }

            return true;          
        }
    }
}
