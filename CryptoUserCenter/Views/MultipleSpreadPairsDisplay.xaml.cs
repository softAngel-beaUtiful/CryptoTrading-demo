using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using MajorControl;
using TickQuant.Common;

namespace CryptoUserCenter.Views
{
    /// <summary>
    /// SingleSymbol.xaml 的交互逻辑
    /// </summary>
    public partial class MultipleSpreadPairsDisplay : Window
    {
        public List<string> mylist = new List<string>();
        
        public MultipleSpreadPairsDisplay(List<string> symbollist) 
        {           
            InitializeComponent();
            WindowState = WindowState.Maximized;
            mylist = symbollist;
            Title = "Crypto Spread Pairs Display";            
            var interopHelper = new WindowInteropHelper(this);
            var activeScreen = Screen.FromHandle(interopHelper.Handle);
            Width = activeScreen.WorkingArea.Width;
            Height = activeScreen.WorkingArea.Height;
            double width, height;

            if (mylist.Count == 1)
            {
                width = Width - 8;
                height = Height - 13;
            }
            else if (mylist.Count == 2)
            {
                width = Width - 8;
                height = (Height - 23) / 2;
            }
            else
               if (mylist.Count < 5)
            {
                width = (Width - 16) / 2;
                height = (Height - 23) / 2;
            }
            else
               if (mylist.Count < 7)
            {
                width = (Width - 16) / 3;
                height = (Height - 23) / 2;
            }
            else
            {
                width = (Width - 16) / 3;
                height = (Height - 23) / 3;
            }            
            foreach (string sym in symbollist)
            {
                var chartconfig = new ChartConfig();
                var d = new Dictionary<EIndicatorType, object>();
                d[EIndicatorType.BOLLBAND] = new Tuple<int, double>(100, 2);
                chartconfig.ComplexInstrument = new KeyValuePair<string, Dictionary<EIndicatorType, object>>(sym, d);
                var con = new MajorChartControl(chartconfig, sym, width, height);                
                majorpanel.Children.Add(con);
            }                                                               
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
                             
        }        

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            foreach (MajorChartControl c in majorpanel.Children)
            {
                if (mylist.Count == 1)
                {
                    c.Width = (ActualWidth - 8);
                    c.Height = Height - 14;
                }
                else
                if (mylist.Count < 3)
                {
                    c.Width = (ActualWidth - 8);
                    c.Height = (Height - 23) / 2;
                }
                else
                if (mylist.Count < 5)
                {
                    c.Width = (ActualWidth - 16) / 2;
                    c.Height = (Height - 23) / 2;
                }
                else
                if (mylist.Count<7)
                {
                    c.Width = (ActualWidth - 16) / 3;
                    c.Height = (Height - 23) / 2;
                }
                else
                {
                    c.Width = (ActualWidth - 16) / 3;
                    c.Height = (Height - 23) / 3;
                }
            }          
        }
        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState   == WindowState.Maximized)
            {
                var interopHelper = new WindowInteropHelper(this);
                var activeScreen = Screen.FromHandle(interopHelper.Handle);            
                Width = activeScreen.WorkingArea.Width;
                Height = activeScreen.WorkingArea.Height;               
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {            
                foreach (MajorChartControl i in majorpanel.Children)
                {
                    i.chartVM.StopReceiving();
                    /*if (i.chartVM.myTask != null)
                        while (i.chartVM.myTask.Status != System.Threading.Tasks.TaskStatus.RanToCompletion)
                            Thread.Sleep(12);
                    */
                    if (i.chartVM.myTask1 != null)
                        while (i.chartVM.myTask1.Status != TaskStatus.RanToCompletion)
                            Thread.Sleep(12);
                    //if (i.chartVM.myTask2 != null)
                    //    while (i.chartVM.myTask2.Status != System.Threading.Tasks.TaskStatus.RanToCompletion)
                    //        Thread.Sleep(12);
                    i.chartVM.myTask.Dispose();
                    i.chartVM.myTask1.Dispose();
                    i.chartVM.myTask2.Dispose();
                }
           
        }
    }    
}
