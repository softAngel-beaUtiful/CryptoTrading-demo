using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace KChart.Chart
{
    public class ColumnGraph : LinearGraph
    {
        public const int RightSpaceLeft = 100;
        public int Startpoint;
        public ColumnGraph()
        {
            _columnGraph = new Path();
            _columnGraphGeometry = new PathGeometry();
            _columnGraph.Data = _columnGraphGeometry;

            BindBrush();
        }

        private void BindBrush()
        {
            Binding brushBinding = new Binding("Brush");
            brushBinding.Source = this;
            _columnGraph.SetBinding(Shape.FillProperty, brushBinding);
        }

        private Path _columnGraph;
        private PathGeometry _columnGraphGeometry;

        public List<dynamic> Values { get; private set; } = new List<dynamic>();

        /// <summary>
        /// Renders graph.
        /// </summary>
        public override void Render(double lowest, double highest)
        {
            if (Items != null)
            {
                var cc = Canvas.ActualWidth - RightSpaceLeft;
                var availablesticks = (int)(cc / IntervalWidth);
                int Count = Math.Min(Items.Count, availablesticks);
                Startpoint = Items.Count - Count;
                UpdateColumns(Startpoint, Count, lowest, highest);
            }
        }
        public override string OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var p = e.GetPosition(Canvas);
            if (p.X > Canvas.ActualWidth - RightSpaceLeft)
                return null;
            var index = (int)(p.X / IntervalWidth) + Startpoint;

            if (index >= 0 && index < Items.Count)
            {
                var vol = (Items[index] as CandleStick).Volume;                
                return $"{vol}";
            }

            return string.Empty;
        }
        private void UpdateColumns(int startpoint, int Count, double lowest, double highest)
        {
            if (_columnGraphGeometry.Figures.Count != Count)
            {
                _columnGraphGeometry.Figures.Clear();
                for (int i = startpoint; i < startpoint+Count; i++)
                {
                    PathFigure column = new PathFigure();
                    _columnGraphGeometry.Figures.Add(column);
                    for (int si = 0; si < 4; si++)
                    {
                        column.Segments.Add(new LineSegment());
                    }
                }
            }
            for (int i = 0;i<Count;i++)
                SetColumnSegments(i+startpoint,Items[i+startpoint] as CandleStick, lowest, highest);
                       
        }
        private double TranslateY(double volume, double highest, double actualheight)
        {
            var range = highest;
            var current = highest - volume;
            return actualheight * current / range;
        }

        private void SetColumnSegments(int indexinItems, CandleStick item, double lowest, double highest)
        {
			if (indexinItems >= Items.Count )
                return;
           
            double width = IntervalWidth - 3;
            double left = (indexinItems-Startpoint + 1) * IntervalWidth - width /2;
            double right = left + width;
            
            double y1 = Canvas.ActualHeight;
            double vol = item.Volume;
            double y2 = TranslateY(vol, highest, Canvas.ActualHeight);

            if (right < 10)
                y1 = y2;
            
            try
            {
                int myindex = indexinItems - Startpoint;
                _columnGraphGeometry.Figures[myindex].StartPoint = new Point(left, y1);
                (_columnGraphGeometry.Figures[myindex].Segments[0] as LineSegment).Point = new Point(right, y1);
                (_columnGraphGeometry.Figures[myindex].Segments[1] as LineSegment).Point = new Point(right, y2);
                (_columnGraphGeometry.Figures[myindex].Segments[2] as LineSegment).Point = new Point(left, y2);
                (_columnGraphGeometry.Figures[myindex].Segments[3] as LineSegment).Point = new Point(left, y1);
            }
            catch (Exception ex)
            { }
        }

        public override void OnCanvasInit(Canvas canvas)
        {
            Canvas = canvas;
            Canvas.Children.Add(_columnGraph);
        }

        public override void OnItemsChanged()
        {
            if (Items != null)
            {
                Values.Clear();
                foreach (var item in Items)
                {
                    var value = Convert.ToDouble(item.GetType().GetProperty(ValueMemberPath).GetValue(item));
                    Values.Add(new
                    {
                        Value = value,
                    });
                }
            }
        }//end func
    }
}