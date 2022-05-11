using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using TickQuant.Common;

namespace MajorControl
{
    public class LineGraph : LinearGraph
    {
        public ChartConfig Config;
        public LineGraph(ChartConfig config)
        {
            this.DefaultStyleKey = typeof(LineGraph);
            _lineGraph = new Polyline();
            Config = config;
            BindBrush();
            BindStrokeThickness();
        }

        private void BindBrush()
        {
            Binding brushBinding = new Binding("Brush");
            brushBinding.Source = this;
            _lineGraph.SetBinding(Polyline.StrokeProperty, brushBinding);
        }

        private void BindStrokeThickness()
        {
            Binding thicknessBinding = new Binding("StrokeThickness");
            thicknessBinding.Source = this;
            _lineGraph.SetBinding(Polyline.StrokeThicknessProperty, thicknessBinding);
        }
        
        private Polyline _lineGraph;
        
        public override void OnCanvasInit(Canvas canvas)
        {
            MyCanvas = canvas;
            MyCanvas.Children.Add(_lineGraph);
        }

        /// <summary>
        /// Renders line graph.
        /// </summary>
        public override void Render(decimal lowest, decimal highest)
        {
            var points = new PointCollection();
            var cc = MyCanvas.ActualWidth - Config.RightWidth;
            var availablesticks = (int)(cc / IntervalWidth);
            int Count = Math.Min(Items.Count, availablesticks);
            int Startpoint = Items.Count - Count;
            for (int i = Startpoint; i < Items.Count; ++i)
            {
                var item = Items[i];
                if (lowest == 0 || highest == 0) continue;
                var x = IntervalWidth * (i - Startpoint + 1) - (IntervalWidth - 8) / 2; //MyCanvas.ActualWidth - (i * IntervalWidth)  - (IntervalWidth / 2);IntervalWidth * (i - Startpoint + 1) - (IntervalWidth-8) / 2
                var y = TranslateY(((ArbitrageCandle)item).SClose, lowest, highest, MyCanvas.ActualHeight);

                if (y <= MyCanvas.ActualHeight)
                {
                    points.Add(new Point() { X = x, Y = y });
                }
            }
            _lineGraph.Points = points;
        }

        /// <summary>
        /// Identifies <see cref="StrokeThickness"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register(
            "StrokeThickness", typeof(double), typeof(LineGraph),
            new PropertyMetadata(2.0)
            );

        /// <summary>
        /// Gets or sets stroke thickness for a line graph line.
        /// This is a dependency property.
        /// The default is 2.
        /// </summary>
        public double StrokeThickness
        {
            get { return (double)GetValue(StrokeThicknessProperty); }
            set { SetValue(StrokeThicknessProperty, value); }
        }

        private List<dynamic> Values { get; set; } = new List<dynamic>();

        public override void OnItemsChanged()
        {            
            for (int i = 0; i < Items.Count; ++i)
            {
                var item = Items[i];

                decimal value = ((ArbitrageCandle)item).SClose; //    Convert.ToDouble(item.GetType().GetProperty(ValueMemberPath).GetValue(item));
             
                Values.Add(new { Value = value });
            }
        }
    }
}
