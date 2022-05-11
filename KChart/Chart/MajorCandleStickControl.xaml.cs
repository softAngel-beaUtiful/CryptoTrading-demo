using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TickQuant.Common;

namespace KChart.Chart
{
    /// <summary>
    /// KChart.xaml 的交互逻辑
    /// </summary>
    public partial class MajorGeneralChartControl : UserControl
    {
        //InstrumentID, screensize, interval, start, end, HIGHEST, LOWEST, 
        int Startpoint = 0;
        string InstrumentID;
        double HIGHEST;
        double LOWEST;
        public ChartViewModel chartVM; 
        public List<CandleStick> DataSeries { get; set; }
        public ObservableCollection<CandleStick> KLines { get; set; }
        public ObservableCollection<ArrayList> IndicatorCollection { get; set; }
        public ChartConfig myChartConfig;
        
        public MajorGeneralChartControl(object con, string instru, double width, double height)
        {
           //new ChartConfig();
            InitializeComponent();           
            this.BorderThickness = new Thickness(2);
            BorderBrush = Brushes.Black;
            Width = width;
            Height = height;
            myChartConfig = (ChartConfig)con;
            chartVM = new ChartViewModel(this, instru);
            InstrumentID = instru;
            KLines = chartVM.MajorKLines;
            DataSource = KLines;
            IndicatorCollection = new ObservableCollection<ArrayList>();
            IndicatorCollection.Add(new ArrayList());
            IndicatorCollection[0].Add(new double[4] { 10.2, 13, 10, 12.25});
            //IndicatorCollection[0][0] = 
            Graphs.Add(new CandleStickGraph(myChartConfig, instru));
            var volumegraph = new ColumnGraph();
            volumegraph.ValueMemberPath = "Volume";
            volumegraph.Brush = Brushes.DarkBlue;
           
            OtherGraphs.Add(volumegraph);
            LowMemberPath = "Low";
            HighMemberPath = "High";
            VolumeMemberPath = "Volume";
            chartCanvas.SizeChanged += (d, arg) =>
            {                               
                if (DataSource != null && DataSource.Count>0)
                {
                    if (arg.WidthChanged)
                    {
                        var avail = ((int)(arg.NewSize.Width -100)/ IntervalWidth);
                        int num = Math.Min(avail, DataSource.Count);
                        Startpoint = DataSource.Count - num;
                        width = ActualWidth - 16;
                    }
                    UpdateChart(arg.NewSize, true);
                }
            };
            volumeCanvas.SizeChanged += (d, arg) =>
            {
                //update Startpoint                
                if (DataSource != null && DataSource.Count>0)
                {
                    /*if (arg.WidthChanged)
                    {
                        var avail = (int)((((Canvas)d).ActualWidth -100) / IntervalWidth);
                        int num = Math.Min(avail, DataSource.Count);
                        Startpoint = DataSource.Count - num;
                    }*/
                    UpdateAmount(arg.NewSize, false);
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
            "DataSource", typeof(IList), typeof(MajorGeneralChartControl),
            new PropertyMetadata(null, new PropertyChangedCallback((d, arg) => {
                Trace.TraceInformation("xxxxxx render11");
                var me = d as MajorGeneralChartControl;
                if (me.DataSource == null)
                    return;

                //Do something
                me.UpdateAmount(me.volumeCanvas.RenderSize, false);
                me.UpdateChart(me.chartCanvas.RenderSize, false);
                Trace.TraceInformation("start to render");
            })));
        //store chart info
        public IList DataSource
        {
            get { return (IList)GetValue(DataSourceProperty); }
            set { SetValue(DataSourceProperty, value); }
        }

        public static readonly DependencyProperty UpdateByProperty = DependencyProperty.Register(
            "UpdateBy", typeof(object), typeof(MajorGeneralChartControl),
            new PropertyMetadata(null, new PropertyChangedCallback((d, arg) => {
                var me = d as MajorGeneralChartControl;
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
                me.UpdateAmount(me.volumeCanvas.RenderSize, true);
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
        private int _intervalWidth = 10;
        public static readonly DependencyProperty HighMemberPathProperty = DependencyProperty.Register(
          "HighMemberPath", typeof(string), typeof(MajorGeneralChartControl), null);
        public string HighMemberPath
        {
            get { return (string)GetValue(HighMemberPathProperty); }
            set { SetValue(HighMemberPathProperty, value); }
        }

        public static readonly DependencyProperty LowMemberPathProperty = DependencyProperty.Register(
           "LowMemberPath", typeof(string), typeof(MajorGeneralChartControl), null);
        public string LowMemberPath
        {
            get { return (string)GetValue(LowMemberPathProperty); }
            set { SetValue(LowMemberPathProperty, value); }
        }

        public static readonly DependencyProperty VolumeMemberPathProperty = DependencyProperty.Register(
           "VolumeMemberPath", typeof(string), typeof(MajorGeneralChartControl), null);
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

        public void UpdateAmount(Size newSize, bool force)
        {
            if (!Inited) return;
            
            double highest = double.MinValue;
            double lowest = double.MaxValue;
            int i;
            for (i = Startpoint; i < DataSource.Count; i++)
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
                //var v = graph.ActualWidth - graph.ri;
                graph.Render(lowest, highest);
            }
        }

        public void UpdateChart(Size newSize, bool force)
        {
            if (!Inited) return;
            if (DataSource.Count == 0) return;
            var avail = ((int)(chartCanvas.ActualWidth- 100) / IntervalWidth);
            if (avail < 0) avail = 1;
            int num = Math.Min(avail, DataSource.Count);
            Startpoint = DataSource.Count - num;
            HIGHEST = ((CandleStick)DataSource[Startpoint]).High;//Convert.ToDouble(DataSource[Startpoint].GetType().GetProperty(HighMemberPath).GetValue(DataSource[Startpoint]));
            LOWEST = ((CandleStick)DataSource[Startpoint]).Low;//Convert.ToDouble(DataSource[Startpoint].GetType().GetProperty(LowMemberPath).GetValue(DataSource[Startpoint]));

            int i;
            for (i = Startpoint; i<DataSource.Count;i++)   //get highest and lowest
            {
                var high = Convert.ToDouble(DataSource[i].GetType().GetProperty(HighMemberPath).GetValue(DataSource[i]));                   
                var low = Convert.ToDouble(DataSource[i].GetType().GetProperty(LowMemberPath).GetValue(DataSource[i]));                               
                if (high > HIGHEST)
                    HIGHEST = high;
                if (low < LOWEST)
                    LOWEST = low;
            }
            candlestickdata.Text = "DateTime:" + ((CandleStick)DataSource[DataSource.Count - 1]).Datetime.ToString("yyyy-MM-dd HH:mm:ss")//JsonConvert.SerializeObject(this.DataSource[DataSource.Count - 1]);
                + " Open: " + ((CandleStick)DataSource[DataSource.Count - 1]).Open + " High: " + ((CandleStick)DataSource[DataSource.Count - 1]).High + " Low: " + ((CandleStick)DataSource[DataSource.Count - 1]).Low
                + " Close: " + ((CandleStick)DataSource[DataSource.Count - 1]).Close;
            volume.Text = "Volume: " + (DataSource[DataSource.Count - 1] as CandleStick).Volume.ToString();

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
                        volume.Text = "Volume: "+str;
                        break;
                    }
                }
            }
        }       
    }
}
