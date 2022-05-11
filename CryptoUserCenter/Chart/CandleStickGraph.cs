using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CryptoUserCenter.Views
{
    public class CandleStickGraph : LinearGraph
    {
        public const int RightSpaceLeft = 100;
        public CandleStickGraph()
        {
        }
        
        private List<Path> _CandleStickGraphs = new List<Path>();

        /// <summary>
        /// Applies control template.
        /// </summary>
        public override void OnCanvasInit(Canvas canvas)
        {
            Canvas = canvas;
        }

        /// <summary>
        /// Renders graph.
        /// </summary>
        public override void Render(double lowest, double highest)
        {
            if (Items is null) return;

            var cc = Canvas.ActualWidth - RightSpaceLeft;
            var availablesticks = (int)(cc / IntervalWidth);
            int Count = Math.Min(Items.Count, availablesticks);
            Startpoint = Items.Count - Count;
            UpdateAllCandleSticks(Startpoint, Count, lowest, highest);
            MakeChartGrid(lowest, highest);
            MakeIndicatorData(lowest, highest);
        }

        private void MakeChartGrid(double lowest, double highest)
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
            Point LeftCenter = new Point(0, Canvas.ActualHeight / 2);
            Point LeftUpper = new Point(0, Canvas.ActualHeight / 4);
            Point LeftLower = new Point(0, Canvas.ActualHeight * 3 / 4);
            Point RightCenter = new Point(Canvas.ActualWidth, Canvas.ActualHeight / 2);
            Point RightUpper = new Point(Canvas.ActualWidth, Canvas.ActualHeight / 4);
            Point RightLower = new Point(Canvas.ActualWidth, Canvas.ActualHeight * 3 / 4);
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
            Canvas.Children.Add(CenterLine);
            Canvas.Children.Add(UpperLine);
            Canvas.Children.Add(LowerLine);
            TextBlock centertextBlock = new TextBlock();
            centertextBlock.Text = Math.Round(((highest + lowest) / 2), 2).ToString();
            
            TextBlock toptextblock = new TextBlock();
            toptextblock.Text = Math.Round(highest, 2).ToString();
            TextBlock bottomblock = new TextBlock();
            bottomblock.Text = Math.Round(lowest, 2).ToString();            
           

            Canvas.SetLeft(centertextBlock, 0);
            Canvas.SetTop(centertextBlock, Canvas.ActualHeight / 2 - 15);
        
            Canvas.Children.Add(centertextBlock);
            Canvas.SetLeft(toptextblock, 0);
            Canvas.SetTop(toptextblock, 0);
            Canvas.Children.Add(toptextblock);
            Canvas.SetLeft(bottomblock, 0);
            Canvas.SetTop(bottomblock, Canvas.ActualHeight-15);
            Canvas.Children.Add(bottomblock);
        }

        private void UpdateAllCandleSticks(int startpoint, int Count, double lowest, double highest)
        {
            Canvas.Children.Clear();
            if (_CandleStickGraphs.Count > 0) _CandleStickGraphs.Clear();
            for (int i = 0; i < Count; i++)
            {               
                CandleStick candle = Items[startpoint+i] as CandleStick;
                SetSingleCandleStick(i, candle, lowest, highest);
            }                        
        }

        private void MakeIndicatorData(double lowest, double highest)
        {
            double indivalue;
            List<double> listdouble= new List<double>();
            List<Point> listofpoint = new List<Point>();
            List<Line> listofline = new List<Line>();
            double width = IntervalWidth - 3;
            for (int i = Startpoint;i<Items.Count;i++)
            {
                if (i < 4)
                    continue;
                indivalue = ((CandleStick)Items[i]).Close + ((CandleStick)Items[i - 1]).Close + ((CandleStick)Items[i - 2]).Close + ((CandleStick)Items[i - 3]).Close + ((CandleStick)Items[i - 4]).Close;
                indivalue /= 5;                              
                listofpoint.Add(new Point(IntervalWidth * (i - Startpoint + 1) - (IntervalWidth - 5) / 2, CONVERTDOUBLETOY(indivalue, lowest, highest)));
            }

           
            for (int t = 1; t < listofpoint.Count; t++)
            {
                Line line = new Line();
                line.StrokeThickness = 3;
                line.Stroke = Brushes.Red;
                line.X1 = listofpoint[t - 1].X;
                line.Y1 = listofpoint[t - 1].Y;
                line.X2 = listofpoint[t].X;
                line.Y2 = listofpoint[t].Y;
                listofline.Add(line);
            }
            foreach (var g in listofline)
                Canvas.Children.Add(g);
        }

        private double CONVERTDOUBLETOY(double indivalue, double LOWEST, double HIGHEST)
        {
            return (Canvas.ActualHeight *(HIGHEST- indivalue) / (HIGHEST - LOWEST));
        }

        public override string OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var p = e.GetPosition(Canvas);
            if (p.X > Canvas.ActualWidth - RightSpaceLeft)
                return null;
            var index = (int)(p.X / IntervalWidth) + Startpoint;

            if(index >= 0 && index < Items.Count)
            {
                var stick = "DateTime:" +((CandleStick)Items[index]).Datetime.ToString("yyyy-MM-dd HH:mm:ss")
                + " Open: " + ((CandleStick)Items[index]).Open + " High: " + ((CandleStick)Items[index]).High + " Low: " + ((CandleStick)Items[index]).Low
                + " Close: " + ((CandleStick)Items[index]).Close;

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
        private void SetSingleCandleStick(int index, CandleStick Candle, double lowest, double highest)
        {
            var item = Candle;
           double width = IntervalWidth - 3;     
            double left = IntervalWidth * (index + 1) - width/2;
            double right = left + width;
            Path path = new Path();
            path.Stroke = new SolidColorBrush(Colors.Black);
            path.Fill = new SolidColorBrush(Colors.Black);
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
           
            double low = TranslateY(item.Low, lowest, highest, Canvas.ActualHeight);
            double open = TranslateY(item.Open, lowest, highest, Canvas.ActualHeight);
            double close = TranslateY(item.Close, lowest, highest, Canvas.ActualHeight);
            double high = TranslateY(item.High, lowest, highest, Canvas.ActualHeight);
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
            Canvas.Children.Add(_CandleStickGraphs[index]);
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
