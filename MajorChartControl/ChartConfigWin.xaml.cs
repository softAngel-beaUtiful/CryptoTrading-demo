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
using TickQuant.Common;

namespace MajorControl
{
    /// <summary>
    /// ChangePeriod.xaml 的交互逻辑
    /// </summary>
    public partial class ChartConfigWin : Window
    {
        List<int> vs = new List<int>();
        MajorChartControl majorcontrol;
        //ChartConfig chartConfig;
        public ChartConfigWin(MajorChartControl control)
        {
            majorcontrol = control;
            InitializeComponent();
            period.Text = "100";
            ratio.Text = "2.5";
            vs.Add(1);
            vs.Add(5);
            vs.Add(15);
            vs.Add(30);
            vs.Add(60);

            CandlePeriod.ItemsSource = vs;
            CandlePeriod.SelectedItem = vs[1];            
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            //lock (majorcontrol.)
            if (CandlePeriod.SelectedIndex != -1)
                majorcontrol.SetCandlePeriod((int)CandlePeriod.SelectedItem);
            if (period.Text != null && ratio.Text != null) majorcontrol.SettingBollingerBand(period.Text, ratio.Text);
            Close();            
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
