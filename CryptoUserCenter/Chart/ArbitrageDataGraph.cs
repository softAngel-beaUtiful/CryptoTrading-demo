using CryptoUserCenter.Models;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;


namespace CryptoUserCenter.Views
{
    public class ArbitrageDataGraph : LinearGraph
    {
        public const int RightSpaceLeft = 100;
        public enum GraphType
        {
            Dotted,
            Lined,
            Stick
        }
        public GraphType graphType { get; set; }
        public ArbitrageDataGraph(GraphType GraphType = GraphType.Lined)
        {
            graphType = GraphType;
        }
        
        private List<Line> _ArbitrageDataGraphs = new List<Line>();

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
            var availables = (int)(cc / IntervalWidth);
            int Count = Math.Min(Items.Count, availables);
            Startpoint = Items.Count - Count;
            
            double Lowest = (double)((OrderBook)Items[Startpoint]).AskPrice1;
            double Highest = (double)((OrderBook)Items[Startpoint]).AskPrice1;
            for (int ii = Startpoint; ii < Items.Count; ii++)
            {
                if (Highest < (double)((OrderBook)Items[ii]).AskPrice1)
                    Highest = (double)((OrderBook)Items[ii]).AskPrice1;
                if (Lowest > (double)((OrderBook)Items[ii]).BidPrice1)
                    Lowest = (double)((OrderBook)Items[ii]).BidPrice1;
            }
            if (Lowest == 0 || Highest == 0 || Highest <= Lowest)
                return;
            UpdateAllArbitrageDataGraph(Startpoint, Count, Lowest, Highest);
            MakeChartGrid(Lowest, Highest);            
        }

        private void MakeChartGrid(double lowest, double highest)
        {           
            double Gap = highest - lowest;
            Line CenterLine = new Line();
            CenterLine.StrokeThickness = 1;
            CenterLine.Stroke = Brushes.Gray;
            CenterLine.StrokeDashArray = new DoubleCollection { 3, 3 };
             Line UpperLine = new Line();
            UpperLine.StrokeThickness = 1;
            UpperLine.Stroke = Brushes.Gray;
            UpperLine.StrokeDashArray = new DoubleCollection { 3, 3 };
            Line LowerLine = new Line();
            LowerLine.Stroke = Brushes.Gray;
            LowerLine.StrokeThickness = 1;
            LowerLine.StrokeDashArray = new DoubleCollection { 3, 3 };
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
            centertextBlock.Text = Math.Round(((highest + lowest) / 2), 8).ToString();
            
            TextBlock toptextblock = new TextBlock();
            toptextblock.Text = Math.Round(highest, 8).ToString();
            TextBlock bottomblock = new TextBlock();
            bottomblock.Text = Math.Round(lowest, 8).ToString();
            
            Canvas.SetLeft(centertextBlock, 0);
            Canvas.SetTop(centertextBlock, Canvas.ActualHeight / 2 - 15);
        
            Canvas.Children.Add(centertextBlock);
            Canvas.SetLeft(toptextblock, 0);
            Canvas.SetTop(toptextblock, 0);
            Canvas.Children.Add(toptextblock);
            Canvas.SetLeft(bottomblock, 0);
            Canvas.SetTop(bottomblock, Canvas.ActualHeight-15);
            Canvas.Children.Add(bottomblock);
            TextBlock upblock = new TextBlock();
            upblock.Text = Math.Round(highest - Gap * 0.25, 8).ToString();
            Canvas.SetLeft(upblock, 0);
            Canvas.SetTop(upblock, Canvas.ActualHeight * 0.25 - 15);
            Canvas.Children.Add(upblock);
            TextBlock lowerblock = new TextBlock();
            lowerblock.Text= Math.Round(highest - Gap * 0.75, 8).ToString();
            Canvas.SetLeft(lowerblock, 0);
            Canvas.SetTop(lowerblock, Canvas.ActualHeight * 0.75 - 15);
            Canvas.Children.Add(lowerblock);


        }

        private void UpdateAllArbitrageDataGraph(int startpoint, int Count, double Lowest, double Highest)
        {
            Canvas.Children.Clear();
            if (_ArbitrageDataGraphs.Count > 0) _ArbitrageDataGraphs.Clear();
            List<(double, double, double, double, DateTime)> pointpairs = new List<(double, double, double, double, DateTime)>();

            lock (Items)
            {
                for (int i = 0; i < Count; i++)
                {
                    OrderBook candle = Items[startpoint + i] as OrderBook;
                    var a = SetPoints(i, candle, Lowest, Highest);
                    pointpairs.Add(a);
                }
            }
            switch (graphType)
            {
                case (GraphType.Lined):
                    for (int i = 1; i < Count; i++)
                    {
                        var line1 = new Line();
                        var line2 = new Line();
                        line1.StrokeThickness = 1;
                        line1.Stroke = Brushes.Red;
                        line2.Stroke = Brushes.Blue;
                        line2.StrokeThickness = 2;
                        line1.X1 = pointpairs[i - 1].Item1;
                        line1.Y1 = pointpairs[i - 1].Item2;
                        line1.X2 = pointpairs[i].Item1;
                        line1.Y2 = pointpairs[i].Item2;
                        line2.X1 = pointpairs[i - 1].Item3;
                        line2.Y1 = pointpairs[i - 1].Item4;
                        line2.X2 = pointpairs[i].Item3;
                        line2.Y2 = pointpairs[i].Item4;
                        _ArbitrageDataGraphs.Add(line1);
                        _ArbitrageDataGraphs.Add(line2);
                        Canvas.Children.Add(line1);
                        Canvas.Children.Add(line2);

                    }
                    break;
                case (GraphType.Dotted):
                    foreach (var v in pointpairs)
                    {
                        var line1 = new Line();
                        line1.X1 = v.Item1;
                        line1.Y1 = v.Item2;
                        line1.X2 = v.Item1;
                        line1.Y2 = v.Item2 + 2;
                        line1.Stroke = Brushes.LawnGreen;
                        line1.StrokeThickness = 2;
                        var line2 = new Line();
                        line2.X1 = v.Item3;
                        line2.Y1 = v.Item4;
                        line2.X2 = v.Item3;
                        line2.Y2 = v.Item4 - 2;
                        line2.Stroke = Brushes.Red;
                        line2.StrokeThickness = 2;
                        Canvas.Children.Add(line1);
                        Canvas.Children.Add(line2);
                    }
                    break;
                case (GraphType.Stick):
                    Line line; // = new Line();

                    foreach (var v in pointpairs)
                    {
                        line = new Line();
                        line.X1 = v.Item1;
                        line.Y1 = v.Item2;
                        line.X2 = v.Item3;
                        line.Y2 = v.Item4;
                        line.Stroke = Brushes.Gray;
                        line.StrokeThickness = 1;
                        Canvas.Children.Add(line);
                    }
                    break;
            }
            int minute = -1;
            for (int g = 0; g < Count; g++)
            {                
                if (pointpairs[g].Item5.Second <5 && minute !=pointpairs[g].Item5.Minute && g>50)
                {
                    minute = pointpairs[g].Item5.Minute;
                    var bottom = new TextBlock();
                    bottom.Text = pointpairs[g].Item5.ToString("HH:mm")+":00";
                    Canvas.SetLeft(bottom, g+5);
                    Canvas.SetTop(bottom, Canvas.ActualHeight - 15);
                    Canvas.Children.Add(bottom);
                }

            
            }
            /*for (int i = 1; i < points.Count; i++)
            {
                //var line1 = new Line();
                var line2 = new Line();
                //line1.StrokeThickness = 1;
                //line1.Stroke = Brushes.Red;
                line2.Stroke = Brushes.Blue;
                line2.StrokeThickness = 2;
                //line1.X1 = points[i - 1].Item1;
                //line1.Y1 = points[i - 1].Item2;
                //line1.X2 = points[i].Item1;
                //line1.Y2 = points[i].Item2;
                line2.X1 = points[i - 1].Item3;
                line2.Y1 = points[i - 1].Item4;
                line2.X2 = points[i].Item3;
                line2.Y2 = points[i].Item4;
                _ArbitrageDataGraphs.Add(line2);
                //_ArbitrageDataGraphs.Add(line2);
                Canvas.Children.Add(line2);
                //Canvas.Children.Add(line2);
            }*/
            //Canvas.Children.Add(_ArbitrageDataGraphs);

        }

        private (double, double,double, double, DateTime) SetPoints(int i, OrderBook book, double lowest, double highest)
        {
            double UPX1 = (5 + IntervalWidth * i);
            double LOX1 = UPX1;
            double UPY1 = CONVERTDOUBLETOY((double)book.AskPrice1, lowest, highest);
            double LOY1 = CONVERTDOUBLETOY((double)book.BidPrice1, lowest, highest);

            return (UPX1, UPY1, LOX1, LOY1, book.timestamp);
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
                var stick =  "DateTime:" +((OrderBook)Items[index]).timestamp.ToString("yyyy-MM-dd HH:mm:ss")
                + " Bid: " + ((OrderBook)Items[index]).BidPrice1 + " BidVol: " + ((OrderBook)Items[index]).BidAmount1 
                + " Ask: " + ((OrderBook)Items[index]).AskPrice1 + " AskVol: " + ((OrderBook)Items[index]).AskAmount1;

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
        /*private void SetSingleCandleStick(int index, OrderBook Candle, double lowest, double highest)
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
        */
        public static readonly DependencyProperty HighMemberPathProperty = DependencyProperty.Register(
           "HighMemberPath", typeof(string), typeof(ArbitrageDataGraph), null);
        public string HighMemberPath
        {
            get { return (string)GetValue(HighMemberPathProperty); }
            set { SetValue(HighMemberPathProperty, value); }
        }

        public static readonly DependencyProperty LowMemberPathProperty = DependencyProperty.Register(
           "LowMemberPath", typeof(string), typeof(ArbitrageDataGraph), null);
        public string LowMemberPath
        {
            get { return (string)GetValue(LowMemberPathProperty); }
            set { SetValue(LowMemberPathProperty, value); }
        }

        public static readonly DependencyProperty OpenMemberPathProperty = DependencyProperty.Register(
           "OpenMemberPath", typeof(string), typeof(ArbitrageDataGraph), null);
        public string OpenMemberPath
        {
            get { return (string)GetValue(OpenMemberPathProperty); }
            set { SetValue(OpenMemberPathProperty, value); }
        }

        public static readonly DependencyProperty CloseMemberPathProperty = DependencyProperty.Register(
           "CloseMemberPath", typeof(string), typeof(ArbitrageDataGraph), null);
        public string CloseMemberPath
        {
            get { return (string)GetValue(CloseMemberPathProperty); }
            set { SetValue(CloseMemberPathProperty, value); }
        }

        public int Startpoint { get; private set; }
        public List<dynamic> Values { get; private set; } = new List<dynamic>();
    }
}
