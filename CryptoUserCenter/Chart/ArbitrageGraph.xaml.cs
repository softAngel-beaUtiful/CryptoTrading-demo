using CryptoUserCenter.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace CryptoUserCenter.Views
{
    /// <summary>
    /// ArbitrageGraph.xaml 的交互逻辑
    /// </summary>
    public partial class ArbitrageGraph : UserControl
    {
        int Startpoint = 0;
        double HIGHEST;
        double LOWEST;
        public ChartViewModel chartVM;
        public List<OrderBook> ArbDataList { get; set; }
        public ObservableCollection<double[]> IndicatorCollection { get; set; }
        public (string, string) symbolspair = ("bitmex_xbtusd", "okex_btc_usd_200626");
        public ArbitrageGraph()
        {
            InitializeComponent();
            chartVM = new ChartViewModel(this, symbolspair);
            DataSource = chartVM.ComboOrderBook;
            /*IndicatorCollection = new ObservableCollection<double[]>
            {
                new double[4]
            };
            IndicatorCollection.Add(new double[44]);*/
            Graphs.Add(new ArbitrageDataGraph());
            //var volumegraph = new ColumnGraph();
            //volumegraph.ValueMemberPath = "Volume";
            //volumegraph.Brush = Brushes.DarkBlue;

            //OtherGraphs.Add(volumegraph);
            //LowMemberPath = "Low";
            //HighMemberPath = "High";
            //VolumeMemberPath = "Volume";
            chartCanvas.SizeChanged += (d, arg) =>
            {
                if (DataSource != null && DataSource.Count > 0)
                {
                    if (arg.WidthChanged)
                    {
                        var avail = ((int)(arg.NewSize.Width - 100) / IntervalWidth);
                        int num = Math.Min(avail, DataSource.Count);
                        Startpoint = DataSource.Count - num;
                    }
                    UpdateChart(arg.NewSize, true);
                }
            };
            volumeCanvas.SizeChanged += (d, arg) =>
            {
                //update Startpoint                
                if (DataSource != null && DataSource.Count > 0)
                {
                    /*if (arg.WidthChanged)
                    {
                        var avail = (int)((((Canvas)d).ActualWidth -100) / IntervalWidth);
                        int num = Math.Min(avail, DataSource.Count);
                        Startpoint = DataSource.Count - num;
                    }*/
                    //UpdateAmount(arg.NewSize, false);
                }
            };
        }
        #region Properties

        public IList Graphs
        {
            get { return _graphs; }
            set { throw new NotSupportedException("Setting Graphs collection is not supported"); }
        }
        private ObservableCollection<LinearGraph> _graphs = new ObservableCollection<LinearGraph>();

        public IList OtherGraphs
        {
            get { return _volumegraphs; }
            set { throw new NotSupportedException("Setting Graphs collection is not supported"); }
        }
        private ObservableCollection<LinearGraph> _volumegraphs = new ObservableCollection<LinearGraph>();

        public static readonly DependencyProperty DataSourceProperty = DependencyProperty.Register(
            "DataSource", typeof(IList), typeof(ArbitrageGraph),
            new PropertyMetadata(null, new PropertyChangedCallback((d, arg) => {
                Trace.TraceInformation("xxxxxx render11");
                var me = d as ArbitrageGraph;
                if (me.DataSource == null)
                    return;

                //Do something
                //me.UpdateAmount(me.volumeCanvas.RenderSize, false);
                me.UpdateChart(me.chartCanvas.RenderSize, false);
                Trace.TraceInformation("start to render");
            })));
        public IList DataSource
        {
            get { return (IList)GetValue(DataSourceProperty); }
            set { SetValue(DataSourceProperty, value); }
        }

        public static readonly DependencyProperty UpdateByProperty = DependencyProperty.Register(
            "UpdateBy", typeof(object), typeof(ArbitrageGraph),
            new PropertyMetadata(null, new PropertyChangedCallback((d, arg) => {
                var me = d as ArbitrageGraph;
                //Do something
                if (me.DataSource == null)
                    return;
                /*
                foreach (var graph in me.OtherGraphs)
                {
                    (graph as LinearGraph).OnItemsChanged();
                }

                foreach(var graph in me.Graphs)
                {
                    (graph as LinearGraph).OnItemsChanged();
                }*/

                //Do something
                //me.UpdateAmount(me.volumeCanvas.RenderSize, true);
                me.UpdateChart(me.chartCanvas.RenderSize, true);
                Trace.TraceInformation("xxxxxx render UpdateBy");
            })));
        public object UpdateBy
        {
            get { return GetValue(UpdateByProperty); }
            set { SetValue(UpdateByProperty, value); }
        }

        public int IntervalWidth
        {
            get { return _intervalWidth; }
            set
            {
                if (_intervalWidth != value)
                {
                    _intervalWidth = value;
                }
            }
        }
        private int _intervalWidth = 1;
        public static readonly DependencyProperty HighMemberPathProperty = DependencyProperty.Register(
          "HighMemberPath", typeof(string), typeof(ArbitrageGraph), null);
        public string HighMemberPath
        {
            get { return (string)GetValue(HighMemberPathProperty); }
            set { SetValue(HighMemberPathProperty, value); }
        }

        public static readonly DependencyProperty LowMemberPathProperty = DependencyProperty.Register(
           "LowMemberPath", typeof(string), typeof(ArbitrageGraph), null);
        public string LowMemberPath
        {
            get { return (string)GetValue(LowMemberPathProperty); }
            set { SetValue(LowMemberPathProperty, value); }
        }

        public static readonly DependencyProperty VolumeMemberPathProperty = DependencyProperty.Register(
           "VolumeMemberPath", typeof(string), typeof(ArbitrageGraph), null);
        public string VolumeMemberPath
        {
            get { return (string)GetValue(VolumeMemberPathProperty); }
            set { SetValue(VolumeMemberPathProperty, value); }
        }
        #endregion

        public bool Inited
        {
            get;
            set;
        }

        private void InitCanvas()
        {
            foreach (var graph in _graphs)
            {
                if (graph != null && !chartCanvas.Children.Contains(graph))
                {
                    chartCanvas.Children.Add(graph);
                    graph.OnCanvasInit(chartCanvas);
                    graph.IntervalWidth = IntervalWidth;
                }
            }

            foreach (var graph in _volumegraphs)
            {
                if (graph != null && !volumeCanvas.Children.Contains(graph))
                {
                    volumeCanvas.Children.Add(graph);
                    graph.OnCanvasInit(volumeCanvas);
                    graph.IntervalWidth = IntervalWidth;
                }
            }
            Inited = true;
        }

        /*public void UpdateAmount(Size newSize, bool force)
        {
            if (!Inited) return;

            double highest = double.MinValue;
            double lowest = double.MaxValue;
            int i;
            for (i = Startpoint; i < DataSource.Count; i++) // in DataSource)
            {
                var temp = Convert.ToDouble(DataSource[i].GetType().GetProperty(VolumeMemberPath).GetValue(DataSource[i]));

                if (temp > highest)
                    highest = temp;

                if (temp < lowest && temp >= 0)
                    lowest = temp;
            }
            foreach (var graph in _volumegraphs)
            {
                if (graph.Items == DataSource)
                {
                    if (force)
                    {
                        graph.OnItemsChanged();
                    }
                }
                else
                {
                    graph.Items = DataSource;
                }               
                graph.Render(lowest, highest);
            }
        }
        */
        public void UpdateChart(Size newSize, bool force)
        {           
            if (!Inited) return;
            if (DataSource.Count == 0) return;
            var avail = (int)(chartCanvas.ActualWidth - 100) / IntervalWidth;            
                int num = Math.Min(avail, DataSource.Count);
                Startpoint = DataSource.Count - num;
                HIGHEST = (double)((OrderBook)DataSource[Startpoint]).AskPrice1;
                LOWEST = (double)((OrderBook)DataSource[Startpoint]).BidPrice1;
           
            int i;
            
            candlestickdata.Text =symbolspair.Item1+"-"+symbolspair.Item2+" DateTime:" + ((OrderBook)DataSource[DataSource.Count - 1]).timestamp.ToString("yyyy-MM-dd HH:mm:ss")
                + " Bid: " + ((OrderBook)DataSource[DataSource.Count - 1]).BidPrice1 + " BidVol: " + ((OrderBook)DataSource[DataSource.Count - 1]).BidAmount1 + " Ask: " + ((OrderBook)DataSource[DataSource.Count - 1]).AskPrice1
                + " AskVol: " + ((OrderBook)DataSource[DataSource.Count - 1]).AskAmount1;            

            foreach (var graph in _graphs)
            {
                if (graph.Items == DataSource)
                {
                    if (force)
                    {
                        graph.OnItemsChanged();
                    }
                }
                else
                {
                    graph.Items = DataSource;
                }
                for (i = Startpoint; i < DataSource.Count; i++)   //get highest and lowest
                {
                    var high = (double)((OrderBook)DataSource[Startpoint]).AskPrice1;
                    var low = (double)((OrderBook)DataSource[Startpoint]).BidPrice1;
                    if (high > HIGHEST)
                        HIGHEST = high;
                    if (low < LOWEST)
                        LOWEST = low;
                }
                graph.Render(LOWEST, HIGHEST);
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            InitCanvas();
        }


        public void chartCanvas_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            foreach (var graph in _graphs)
            {
                if (graph != null)
                {
                    var p = e.GetPosition(chartCanvas);
                    var str = graph.OnMouseMove(sender, e);
                    str = symbolspair.Item1 + "-" + symbolspair.Item2 + " " + str;
                    if (!string.IsNullOrEmpty(str))
                    {
                        candlestickdata.Text = str;
                        break;
                    }
                }
            }

            foreach (var graph in _volumegraphs)
            {
                if (graph != null)
                {
                    var str = graph.OnMouseMove(sender, e);

                    if (!string.IsNullOrEmpty(str))
                    {
                        volume.Text = "Volume: " + str;
                        break;
                    }
                }
            }
        }
    }
}
