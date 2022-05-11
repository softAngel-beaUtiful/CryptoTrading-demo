using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TickQuant.Common;

namespace MajorControl
{
    /// <summary>
    /// UserControl1.xaml 的交互逻辑
    /// </summary>
    public partial class MajorChartControl : UserControl
    {
        
        //InstrumentID, screensize, interval, start, end, HIGHEST, LOWEST, 
        int Startpoint = 0;
        string InstrumentID;
        decimal HIGHEST;
        decimal LOWEST;
        public ChartViewModel chartVM;        
        
        //public List<ArrayList> IndicatorCollection { get; set; }
        public ChartConfig myChartConfig;
       
        public MajorChartControl(ChartConfig con, string sym, double width, double height)
        {
            InitializeComponent();
            //MYSQLHELPER = new MYSQLHELPER();
            this.BorderThickness = new Thickness(1);
            BorderBrush = Brushes.DarkGray;
            InstrumentID = con.ComplexInstrument.Key;
            Width = width;
            Height = height;
            myChartConfig = con;
            chartVM = new ChartViewModel(this, con);            
            DataSource = chartVM.CurrentKLines;                     
            ChartGraphs.Add(new CandleStickGraph(myChartConfig, con.ComplexInstrument.Key));
            var spreadgraph = new LineGraph(myChartConfig);
            spreadgraph.ValueMemberPath = "Spread";
            spreadgraph.Brush = Brushes.DarkBlue;
            spreadgraph.Items = chartVM.CurrentKLines;
            IndicatorGraphs.Add(spreadgraph);
            LowMemberPath = "Low";
            HighMemberPath = "High";
            //VolumeMemberPath = "Volume";
            chartCanvas.SizeChanged += (d, arg) =>
            {
                if (DataSource != null && DataSource.Count > 0)
                {
                    if (arg.WidthChanged)
                    {
                        var avail = (int)(arg.NewSize.Width - myChartConfig.RightWidth) / IntervalWidth;
                        int num = Math.Min(avail, DataSource.Count);
                        Startpoint = DataSource.Count - num;
                        width = ActualWidth - 16;
                    }
                    UpdateChart(arg.NewSize, true);
                }
            };
            spreadCanvas.SizeChanged += (d, arg) =>
            {
                //update Startpoint                
                if (DataSource != null && DataSource.Count > 0)
                {
                    if (arg.WidthChanged)
                    {
                        var avail = (int)(arg.NewSize.Width - myChartConfig.RightWidth) / IntervalWidth;
                        int num = Math.Min(avail, DataSource.Count);
                        Startpoint = DataSource.Count - num;
                        width = ActualWidth - 16;
                    }
                    UpdateIndicator(arg.NewSize, false);
                }
            };
        }

        internal void SettingBollingerBand(string text1, string text2)
        {
            lock (this.chartVM.CurrentKLines)
            {
                myChartConfig.ComplexInstrument.Value[EIndicatorType.BOLLBAND] = new Tuple<int, double>(int.Parse(text1), double.Parse(text2));
            }
        }

        internal void SetCandlePeriod(int selectedItem)
        {
            lock (chartVM.CurrentKLines)
            {
                myChartConfig.CandlePeriod = selectedItem;
                chartVM.ConvertKLinesFromBaseKLines();
            }
        }

        public IList ChartGraphs
        {
            get { return _chartgraphs; }
            set { throw new NotSupportedException("Setting Graphs collection is not supported"); }
        }
        private ObservableCollection<LinearGraph> _chartgraphs = new ObservableCollection<LinearGraph>();

        public IList IndicatorGraphs
        {
            get { return _spreadgraphs; }
            set { throw new NotSupportedException("Setting Graphs collection is not supported"); }
        }
        private ObservableCollection<LineGraph> _spreadgraphs = new ObservableCollection<LineGraph>();

        public static readonly DependencyProperty DataSourceProperty = DependencyProperty.Register(
            "DataSource", typeof(IList), typeof(MajorChartControl),
            new PropertyMetadata(null, new PropertyChangedCallback((d, arg) => {
                Trace.TraceInformation("xxxxxx render11");
                MajorChartControl majorChartControl = d as MajorChartControl;
                if (majorChartControl.DataSource == null)
                    return;

                //Do something
                majorChartControl.UpdateIndicator(majorChartControl.spreadCanvas.RenderSize, false);
                majorChartControl.UpdateChart(majorChartControl.chartCanvas.RenderSize, false);
                Trace.TraceInformation("start to render");
            })));
        //store chart info
        public IList DataSource
        {
            get { return (IList)GetValue(DataSourceProperty); }
            set { SetValue(DataSourceProperty, value); }
        }

        /*public static readonly DependencyProperty UpdateByProperty = DependencyProperty.Register(
            "UpdateBy", typeof(object), typeof(MajorChartControl),
            new PropertyMetadata(null, new PropertyChangedCallback((d, arg) => {
                var me = d as MajorChartControl;
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
                }
                //Do something
                me.UpdateIndicator(me.spreadCanvas.RenderSize, true);
                me.UpdateChart(me.chartCanvas.RenderSize, true);
                Trace.TraceInformation("xxxxxx render UpdateBy");
            })));
        public object UpdateBy
        {
            get { return GetValue(UpdateByProperty); }
            set { SetValue(UpdateByProperty, value); }
        }*/
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
        private int _intervalWidth = 10;
        public static readonly DependencyProperty HighMemberPathProperty = DependencyProperty.Register(
          "HighMemberPath", typeof(string), typeof(MajorChartControl), null);
        public string HighMemberPath
        {
            get { return (string)GetValue(HighMemberPathProperty); }
            set { SetValue(HighMemberPathProperty, value); }
        }
        public static readonly DependencyProperty LowMemberPathProperty = DependencyProperty.Register(
           "LowMemberPath", typeof(string), typeof(MajorChartControl), null);
        public string LowMemberPath
        {
            get { return (string)GetValue(LowMemberPathProperty); }
            set { SetValue(LowMemberPathProperty, value); }
        }        
        public bool Inited
        {
            get;
            set;
        }
        private void InitCanvas()
        {
            foreach (var graph in _chartgraphs)
            {
                if (graph != null && !chartCanvas.Children.Contains(graph))
                {
                    chartCanvas.Children.Add(graph);
                    graph.OnCanvasInit(chartCanvas);
                    graph.IntervalWidth = IntervalWidth;
                }
            }
            foreach (var graph in _spreadgraphs)
            {
                if (graph != null && !spreadCanvas.Children.Contains(graph))
                {
                    spreadCanvas.Children.Add(graph);
                    graph.OnCanvasInit(spreadCanvas);
                    graph.IntervalWidth = IntervalWidth;
                }
            }
            Inited = true;
        }
       
        public void UpdateChart(Size newSize, bool force)
        {
            //if (!Inited) return;
            if (DataSource.Count == 0) return;
            var avail = ((int)(chartCanvas.ActualWidth - 100) / IntervalWidth);
            if (avail < 0) avail = 1;
            int num = Math.Min(avail, DataSource.Count);
            Startpoint = DataSource.Count - num;
            HIGHEST = ((ArbitrageCandle)DataSource[Startpoint]).High;//Convert.ToDouble(DataSource[Startpoint].GetType().GetProperty(HighMemberPath).GetValue(DataSource[Startpoint]));
            LOWEST = ((ArbitrageCandle)DataSource[Startpoint]).Low;
            int i;
            try
            {
                for (i = Startpoint; i < DataSource.Count; i++)   //get highest and lowest
                {
                    var high = (DataSource[i] as ArbitrageCandle).High;
                    var low = (DataSource[i] as ArbitrageCandle).Low;
                    if (high > HIGHEST)
                        HIGHEST = high;
                    if (low < LOWEST)
                        LOWEST = low;
                }
            }
            catch (Exception ex)
            { return; }
            if (LOWEST == HIGHEST)
            {
                LOWEST -= 0.0001m;
                HIGHEST += 0.0001m;
            }
            candlestickdata.Text ="DateTime: "+ ((ArbitrageCandle)DataSource[DataSource.Count - 1]).Datetime.ToString("yyyy-MM-dd HH:mm:ss")//JsonConvert.SerializeObject(this.DataSource[DataSource.Count - 1]);
                + " Open: " + Get6RowValue(Math.Round(((ArbitrageCandle)DataSource[DataSource.Count - 1]).Open, 6)) + " High: " + 
                Get6RowValue(Math.Round(((ArbitrageCandle)DataSource[DataSource.Count - 1]).High, 6))
                + " Low: " + Get6RowValue(Math.Round(((ArbitrageCandle)DataSource[DataSource.Count - 1]).Low, 6)) + " Close: " +
                Get6RowValue(Math.Round(((ArbitrageCandle)DataSource[DataSource.Count - 1]).Close, 6));
            volume.Text = "BidAskSpread: " + Get6RowValue(Math.Round((DataSource[DataSource.Count - 1] as ArbitrageCandle).SClose, 6));

            foreach (var graph in _chartgraphs)
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
                graph.Render(LOWEST, HIGHEST);
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            InitCanvas();
        }
        public static string Get6RowValue(decimal Price)
        {
            string Value = "";

            if (Price >= 1000)
                Value = Price.ToString("0.##");
            else if (Price >= 100)
                Value = Price.ToString("0.###");

            else if (Price >= 10)
                Value = Price.ToString("0.####");

            else if (Price >= 1)
                Value = Price.ToString("0.#####");
            else
                Value = Price.ToString("0.######");

            return Value;
        }
        public void chartCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            foreach (var graph in _chartgraphs)
            {
                if (graph != null)
                {
                    //var p = e.GetPosition(chartCanvas);
                    var str = graph.OnMouseMove(sender, e);

                    if (!string.IsNullOrEmpty(str))
                    {
                        candlestickdata.Text = str;
                        break;
                    }
                }
            }

            foreach (var graph in _spreadgraphs)
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

        internal void UpdateIndicator(Size size, bool v)
        {            
            if (!Inited) return;
            if (DataSource.Count == 0) return;
            var avail = ((int)(chartCanvas.ActualWidth - 100) / IntervalWidth);
            if (avail < 0) avail = 1;
            int num = Math.Min(avail, DataSource.Count);
            Startpoint = DataSource.Count - num;
            HIGHEST = ((ArbitrageCandle)DataSource[Startpoint]).SHigh;//Convert.ToDouble(DataSource[Startpoint].GetType().GetProperty(HighMemberPath).GetValue(DataSource[Startpoint]));
            LOWEST = ((ArbitrageCandle)DataSource[Startpoint]).SLow;//Convert.ToDouble(DataSource[Startpoint].GetType().GetProperty(LowMemberPath).GetValue(DataSource[Startpoint]));

            int i;
            for (i = Startpoint; i < DataSource.Count; i++)   //get highest and lowest
            {
                var high =((ArbitrageCandle)DataSource[i]).SClose ;//Convert.ToDecimal(DataSource[i].GetType().GetProperty(HighMemberPath).GetValue(DataSource[i]));
                var low = ((ArbitrageCandle)DataSource[i]).SClose;//Convert.ToDecimal(DataSource[i].GetType().GetProperty(LowMemberPath).GetValue(DataSource[i]));
                if (high > HIGHEST)
                    HIGHEST = high;
                if (low < LOWEST)
                    LOWEST = low;
            }
            if (HIGHEST == LOWEST) return;
            /*candlestickdata.Text = "DateTime: " + ((ArbitrageCandle)DataSource[DataSource.Count - 1]).Datetime.ToString("yyyy-MM-dd HH:mm:ss")//JsonConvert.SerializeObject(this.DataSource[DataSource.Count - 1]);
                + " Open: " + ((ArbitrageCandle)DataSource[DataSource.Count - 1]).Open + " High: " + ((ArbitrageCandle)DataSource[DataSource.Count - 1]).High + " Low: " + ((ArbitrageCandle)DataSource[DataSource.Count - 1]).Low
                + " Close: " + ((ArbitrageCandle)DataSource[DataSource.Count - 1]).Close;*/
            volume.Text = "BidAskSpread: " + Math.Round((DataSource[DataSource.Count - 1] as ArbitrageCandle).SClose, 6).ToString();

            foreach (var graph in _spreadgraphs)
            {
                if (graph.Items == DataSource)
                {
                    //if (force)
                    {
                        graph.OnItemsChanged();
                    }
                }
                else
                {
                    graph.Items = DataSource;
                }
                graph.Render(LOWEST, HIGHEST);
            }
        }              
        private void ChangeCandlePeriod_Click(object sender, RoutedEventArgs e)
        {
            var period = new ChartConfigWin(this);
            
            period.Top = PointToScreen(Mouse.GetPosition(this)).Y;
            period.Left = PointToScreen(Mouse.GetPosition(this)).X;
            period.ShowDialog();
        }        

        private void chartCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ContextMenu cm = FindResource("MajorContextMenu") as ContextMenu;
            cm.PlacementTarget = sender as Canvas;            
            cm.IsOpen = true;           
        }             
    }
}
