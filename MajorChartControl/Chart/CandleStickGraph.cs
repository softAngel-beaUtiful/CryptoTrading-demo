using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using TickQuant.Common;

namespace MajorControl 
{
    public class CandleStickGraph : LinearGraph
    {
        public int RightSpaceLeft; // = 100;
        public string Title { get; set; }
        public CandleStickGraph(ChartConfig chartConfig, string instru)
        {
            Title = instru;
            RightSpaceLeft = chartConfig.RightWidth;
        }        
        private List<Path> _CandleStickGraphs = new List<Path>();
        /// <summary>
        /// Applies control template.
        /// </summary>
        public override void OnCanvasInit(Canvas canvas)
        {
            MyCanvas = canvas;
        }
        /// <summary>
        /// Renders graph.
        /// </summary>
        public override void Render(decimal lowest, decimal highest)
        {            
            if (Items is null) return;
            var cc = MyCanvas.ActualWidth - RightSpaceLeft;
            var availablesticks = (int)(cc / IntervalWidth);
            int Count = Math.Min(Items.Count, availablesticks);
            Startpoint = Items.Count - Count;
            //clear all current UI elements, and redraw
            MyCanvas.Children.Clear();
            UpdateAllCandleSticks(Startpoint, Count, lowest, highest);
            MakeChartGrid(lowest, highest, availablesticks, Count);
            DrawIndicatorsOnMainChart(Startpoint, lowest, highest);
        }

        private void MakeChartGrid(decimal lowest, decimal highest, int availablesticks, int Count)
        {
            Line CenterLine = new Line();
            CenterLine.StrokeThickness = 1;
            CenterLine.Stroke = Brushes.Gray;

            Line UpperLine = new Line();
            UpperLine.StrokeThickness = 1;
            UpperLine.Stroke = Brushes.Gray;

            Line LowerLine = new Line();
            LowerLine.Stroke = Brushes.Gray;
            LowerLine.StrokeThickness = 1;
            Point LeftCenter = new Point(0, MyCanvas.ActualHeight / 2);
            Point LeftUpper = new Point(0, MyCanvas.ActualHeight / 4);
            Point LeftLower = new Point(0, MyCanvas.ActualHeight * 3 / 4);
            Point RightCenter = new Point(MyCanvas.ActualWidth, MyCanvas.ActualHeight / 2);
            Point RightUpper = new Point(MyCanvas.ActualWidth, MyCanvas.ActualHeight / 4);
            Point RightLower = new Point(MyCanvas.ActualWidth, MyCanvas.ActualHeight * 3 / 4);
            CenterLine.X1 = LeftCenter.X;
            CenterLine.Y1 = LeftCenter.Y;
            CenterLine.X2 = RightCenter.X;
            CenterLine.Y2 = RightCenter.Y;
            UpperLine.X1 = LeftUpper.X;
            UpperLine.Y1 = LeftUpper.Y;
            UpperLine.X2 = RightUpper.X;
            UpperLine.Y2 = RightUpper.Y;
            LowerLine.X1 = LeftLower.X;
            LowerLine.Y1 = LeftLower.Y;
            LowerLine.X2 = RightLower.X;
            LowerLine.Y2 = RightLower.Y;
            MyCanvas.Children.Add(CenterLine);
            MyCanvas.Children.Add(UpperLine);
            MyCanvas.Children.Add(LowerLine);
            TextBlock centertextBlock = new TextBlock();
            centertextBlock.Text = Get6RowValue((highest + lowest) / 2);
            TextBlock toptextblock = new TextBlock();
            toptextblock.Text = Get6RowValue(highest); // Math.Round(highest, 2).ToString();
            TextBlock bottomblock = new TextBlock();
            bottomblock.Text = Get6RowValue(lowest);//Math.Round(lowest, 2).ToString();            
            Canvas.SetLeft(centertextBlock, 0);
            Canvas.SetTop(centertextBlock, MyCanvas.ActualHeight / 2 - 15);
            MyCanvas.Children.Add(centertextBlock);
            Canvas.SetLeft(toptextblock, 0);
            Canvas.SetTop(toptextblock, 0);
            MyCanvas.Children.Add(toptextblock);
            Canvas.SetLeft(bottomblock, 0);
            Canvas.SetTop(bottomblock, MyCanvas.ActualHeight - 15);
            MyCanvas.Children.Add(bottomblock);
            TextBlock title = new TextBlock();
            title.Text = Title;
            title.Foreground = Brushes.Gray;
            title.FontSize = 24;
            Canvas.SetLeft(title, (MyCanvas.ActualWidth / 2 - 250 - title.FontSize * 2 - title.Text.Length * 3) < 0 ? 20 : (MyCanvas.ActualWidth / 2 - 250 - title.FontSize * 2 - title.Text.Length * 3));
            Canvas.SetTop(title, MyCanvas.ActualHeight / 2);
            MyCanvas.Children.Add(title);
            TextBlock time1, time2, time3;
            if (Items.Count > Startpoint + (int)(availablesticks / 4))
            {
                time1 = new TextBlock();
                time1.Text = ((ArbitrageCandle)Items[Startpoint + (int)(availablesticks / 4)]).Datetime.ToString("MM-dd HH:mm");
                time1.Foreground = Brushes.Gray;
                time1.FontSize = 14;
                Canvas.SetLeft(time1, (MyCanvas.ActualWidth - RightSpaceLeft) / 4);
                Canvas.SetTop(time1, MyCanvas.ActualHeight - 15);
                MyCanvas.Children.Add(time1);
                if (Items.Count > Startpoint + (int)(availablesticks / 2))
                {
                    time2 = new TextBlock();
                    time2.Text = ((ArbitrageCandle)Items[Startpoint + (int)(availablesticks / 2)]).Datetime.ToString("MM-dd HH:mm");
                    time2.Foreground = Brushes.Gray;
                    time2.FontSize = 14;
                    Canvas.SetLeft(time2, (MyCanvas.ActualWidth-RightSpaceLeft) / 2);
                    Canvas.SetTop(time2, MyCanvas.ActualHeight - 15);
                    MyCanvas.Children.Add(time2);
                    if (Items.Count > Startpoint + (int)(availablesticks / 4 * 3))
                    {
                        time3 = new TextBlock();
                        time3.Text = ((ArbitrageCandle)Items[Startpoint + (int)(availablesticks / 4 * 3)]).Datetime.ToString("MM-dd HH:mm");
                        time3.Foreground = Brushes.Gray;
                        time3.FontSize = 14;
                        Canvas.SetLeft(time3, (MyCanvas.ActualWidth-RightSpaceLeft) / 4 * 3);
                        Canvas.SetTop(time3, MyCanvas.ActualHeight - 15);
                        MyCanvas.Children.Add(time3);
                    }
                }
            }                       

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
        private void UpdateAllCandleSticks(int startpoint, int Count, decimal lowest, decimal highest)
        {
            //MyCanvas.Children.Clear();
            if (_CandleStickGraphs.Count > 0) _CandleStickGraphs.Clear();
            for (int i = 0; i < Count; i++)
            {               
                ArbitrageCandle candle = Items[startpoint+i] as ArbitrageCandle;
                SetSingleCandleStick(i, candle, lowest, highest);
            }                        
        }

        private void DrawIndicatorsOnMainChart(int startpoint, decimal lowest, decimal highest)
        {            
            if (Items.Count < 10) return;  //if there is no enough data, then don't draw
            List<Point> listofpointtop = new List<Point>();
            List<Point> listofpointmid = new List<Point>();
            List<Point> listofpointbottom = new List<Point>();
            List<Line> listoflinetop = new List<Line>();
            List<Line> listoflinemid = new List<Line>();
            List<Line> listoflinebottom = new List<Line>();
            
            for (int i = startpoint;i<Items.Count;i++)
            {
                if (((ArbitrageCandle)Items[i]).IndicatorValues  is null || 
                    ((ArbitrageCandle)Items[i]).IndicatorValues.Length == 0 || ((ArbitrageCandle)Items[i]).IndicatorValues[0].Tick.Item1 is null)
                    continue;
                var v = ((ArbitrageCandle)Items[i]).IndicatorValues[0].Tick;
                                                             
                listofpointbottom.Add(new Point(IntervalWidth * (i - Startpoint + 1) - (IntervalWidth-8) / 2,
                    CONVERTDOUBLETOY(v.Item1.Value, lowest, highest)));
                listofpointmid.Add(new Point(IntervalWidth * (i - Startpoint + 1) - (IntervalWidth-8) / 2,
                    CONVERTDOUBLETOY(v.Item2.Value, lowest, highest)));
                listofpointtop.Add(new Point(IntervalWidth * (i - Startpoint + 1) - (IntervalWidth-8) / 2,
                    CONVERTDOUBLETOY(v.Item3.Value, lowest, highest)));
            }
            if (listofpointbottom.Count < 2)
                return;

            for (int t = 1; t < listofpointtop.Count; t++)
            {
                Line line = new Line();
                line.StrokeThickness = 2;
                line.Stroke = Brushes.Red;
                line.X1 = listofpointtop[t - 1].X;
                line.Y1 = listofpointtop[t - 1].Y;
                line.X2 = listofpointtop[t].X;
                line.Y2 = listofpointtop[t].Y;
                listoflinetop.Add(line);
            }
            for (int t = 1; t < listofpointmid.Count; t++)
            {
                Line line1 = new Line();
                line1.StrokeThickness = 1;
                line1.Stroke = Brushes.Blue;
                line1.X1 = listofpointmid[t - 1].X;
                line1.Y1 = listofpointmid[t - 1].Y;
                line1.X2 = listofpointmid[t].X;
                line1.Y2 = listofpointmid[t].Y;
                listoflinemid.Add(line1);
            }
            for (int t = 1; t < listofpointbottom.Count; t++)
            {
                Line line2 = new Line();
                line2.StrokeThickness = 2;
                line2.Stroke = Brushes.Green;
                line2.X1 = listofpointbottom[t - 1].X;
                line2.Y1 = listofpointbottom[t - 1].Y;
                line2.X2 = listofpointbottom[t].X;
                line2.Y2 = listofpointbottom[t].Y;
                listoflinebottom.Add(line2);
            }
            foreach (var g in listoflinetop)
                MyCanvas.Children.Add(g);
            foreach (var g1 in listoflinemid)
                MyCanvas.Children.Add(g1);
            foreach (var g2 in listoflinebottom)
                MyCanvas.Children.Add(g2);
        }

        private double CONVERTDOUBLETOY(decimal indivalue, decimal LOWEST, decimal HIGHEST)
        {
            return MyCanvas.ActualHeight *((double)(HIGHEST- indivalue) / (double)(HIGHEST - LOWEST));
        }

        public override string OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (Items is null) return string.Empty;
            var p = e.GetPosition(MyCanvas);
            if (p.X > MyCanvas.ActualWidth - RightSpaceLeft)
                return null;
            var index = (int)(p.X / IntervalWidth) + Startpoint;

            if(index >= 0 && index < Items.Count)
            {
                var stick = "DateTime: " + ((ArbitrageCandle)Items[index]).Datetime.ToString("yyyy-MM-dd HH:mm:ss")
                + " Open: " + Math.Round(((ArbitrageCandle)Items[index]).Open, 6) + " High: " 
                + Math.Round(((ArbitrageCandle)Items[index]).High, 6) + " Low: " + Math.Round(((ArbitrageCandle)Items[index]).Low, 6)
                + " Close: " + Math.Round(((ArbitrageCandle)Items[index]).Close, 6);

                return $"{stick}";
            }
            return string.Empty;
        }

        public override void OnItemsChanged()
        {
            if(Items != null)
            {
                Values.Clear();
                foreach(var item in Items)
                {                    
                    Values.Add(item); 
                }
            }
        }
        private void SetSingleCandleStick(int index, ArbitrageCandle Candle, decimal lowest, decimal highest)
        {
            var item = Candle;
           double width = IntervalWidth - 3;     
            double left = IntervalWidth * (index + 1) - width/2;
            double right = left + width;
            Path path = new Path();
            path.Stroke = new SolidColorBrush(Colors.LightGray);
            path.Fill = new SolidColorBrush(Colors.LightGray);
            var geo = new PathGeometry();
            PathFigure candle = new PathFigure();                                
            
            for (int si = 0; si < 12; si++)
            {
                candle.Segments.Add(new LineSegment());              
            }
           
            geo.Figures.Add(candle);
            path.Data = geo;
           
            if (_CandleStickGraphs.Count <= index)
                _CandleStickGraphs.Add(path);
            else
                _CandleStickGraphs[index] = path;

            if (right <= 10)
            {                
                _CandleStickGraphs[index].Visibility = Visibility.Hidden;
                return;
            }
            _CandleStickGraphs[index].Visibility = Visibility.Hidden;
           
            double low = TranslateY(item.Low, (decimal)lowest, (decimal)highest, MyCanvas.ActualHeight);
            double open = TranslateY(item.Open, (decimal)lowest,(decimal)highest, MyCanvas.ActualHeight);
            double close = TranslateY(item.Close, (decimal)lowest,(decimal)highest, MyCanvas.ActualHeight);
            double high = TranslateY(item.High, (decimal)lowest, (decimal)highest, MyCanvas.ActualHeight);
            var up = Math.Min(open, close);
            var down = Math.Max(open, close);
            var lw = width / 100;
            var time = left + width / 2;
            
            _CandleStickGraphs[index].Visibility = Visibility.Visible;

            if (low >= down && high <= up)
            {
                if (open > close)
                {
                    _CandleStickGraphs[index].Fill = Brushes.LawnGreen;
                }
                else
                    if (open < close)
                    _CandleStickGraphs[index].Fill = Brushes.Red;
                else
                    _CandleStickGraphs[index].Fill = Brushes.Yellow;
               
                if (geo != null)
                {
                    geo.Figures[0].StartPoint = new Point(left, down);                             
                    (geo.Figures[0].Segments[0] as LineSegment).Point = new Point(left, up);
                    (geo.Figures[0].Segments[1] as LineSegment).Point = new Point(time - lw, up);
                    (geo.Figures[0].Segments[2] as LineSegment).Point = new Point(time - lw, high);
                    (geo.Figures[0].Segments[3] as LineSegment).Point = new Point(time + lw, high);
                    (geo.Figures[0].Segments[4] as LineSegment).Point = new Point(time + lw, up);                    
                    (geo.Figures[0].Segments[5] as LineSegment).Point = new Point(right, up);
                    (geo.Figures[0].Segments[6] as LineSegment).Point = new Point(right, down);
                    (geo.Figures[0].Segments[7] as LineSegment).Point = new Point(time + lw, down);
                    (geo.Figures[0].Segments[8] as LineSegment).Point = new Point(time + lw, low);
                    (geo.Figures[0].Segments[9] as LineSegment).Point = new Point(time - lw, low);
                    (geo.Figures[0].Segments[10] as LineSegment).Point = new Point(time - lw, down);
                    (geo.Figures[0].Segments[11] as LineSegment).Point = new Point(left, down);
                }              
            }
            MyCanvas.Children.Add(_CandleStickGraphs[index]);
        }

        public static readonly DependencyProperty HighMemberPathProperty = DependencyProperty.Register(
           "HighMemberPath", typeof(string), typeof(CandleStickGraph), null);
        public string HighMemberPath
        {
            get { return (string)GetValue(HighMemberPathProperty); }
            set { SetValue(HighMemberPathProperty, value); }
        }

        public static readonly DependencyProperty LowMemberPathProperty = DependencyProperty.Register(
           "LowMemberPath", typeof(string), typeof(CandleStickGraph), null);
        public string LowMemberPath
        {
            get { return (string)GetValue(LowMemberPathProperty); }
            set { SetValue(LowMemberPathProperty, value); }
        }

        public static readonly DependencyProperty OpenMemberPathProperty = DependencyProperty.Register(
           "OpenMemberPath", typeof(string), typeof(CandleStickGraph), null);
        public string OpenMemberPath
        {
            get { return (string)GetValue(OpenMemberPathProperty); }
            set { SetValue(OpenMemberPathProperty, value); }
        }

        public static readonly DependencyProperty CloseMemberPathProperty = DependencyProperty.Register(
           "CloseMemberPath", typeof(string), typeof(CandleStickGraph), null);
        public string CloseMemberPath
        {
            get { return (string)GetValue(CloseMemberPathProperty); }
            set { SetValue(CloseMemberPathProperty, value); }
        }

        public int Startpoint { get; private set; }
        public List<dynamic> Values { get; private set; } = new List<dynamic>();
    }
}
