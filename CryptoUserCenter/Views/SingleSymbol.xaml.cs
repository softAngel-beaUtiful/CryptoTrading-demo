using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Interop;
using MajorControl;
using TickQuant.Common;

namespace CryptoUserCenter.Views
{

    /// <summary>
    /// SingleSymbol.xaml 的交互逻辑
    /// </summary>
    public partial class SingleSymbol : Window
    {
        public List<string> mylist = new List<string>();
        //MajorChartControl
        List<MajorChartControl> majorChartControls = new List<MajorChartControl>();
       
        public SingleSymbol(List<string> symbollist) 
        {           
            InitializeComponent();
            WindowState = WindowState.Maximized;
            mylist = symbollist;
            Title = "Crypto Market Price Trend";            
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
            WrapPanel stack = new WrapPanel();
            foreach (string sym in symbollist)
            {
                var chartconfig = new ChartConfig();
                var d = new Dictionary<EIndicatorType, object>();
                d[EIndicatorType.BOLLBAND] = new Tuple<int, double>(100, 2);
                chartconfig.ComplexInstrument = new KeyValuePair<string, Dictionary<EIndicatorType, object>>(sym, d);
                var con = new MajorChartControl(chartconfig, sym, width, height);
                majorChartControls.Add(con);
                stack.Children.Add(con);
            }            
            majorgrid.Children.Add(stack);                                           
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                foreach (var i in majorChartControls)
                {
                    i.chartVM.StopReceiving();
                }
            }
            catch (Exception ex)
            { }                       
        }
        

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            foreach (var c in majorChartControls)
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

        private void Window_StateChanged(object sender, System.EventArgs e)
        {
            if (this.WindowState   == WindowState.Maximized)
            {
                var interopHelper = new WindowInteropHelper(this);
                var activeScreen = Screen.FromHandle(interopHelper.Handle);            
                Width = activeScreen.WorkingArea.Width;
                Height = activeScreen.WorkingArea.Height;
               
            }
        }
    }
    
}
