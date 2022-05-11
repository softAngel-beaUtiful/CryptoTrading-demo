﻿using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace KChart.Chart
{
    public abstract class LinearGraph : Control
    {
        public LinearGraph()
        {            
        }

        public abstract void OnCanvasInit(Canvas canvas);

        public abstract void Render(double lowest, double highest);

        public abstract void OnItemsChanged();

        public double TranslateY(double value, double lowest, double highest, double canvasheight)
        {           
            var range = highest-lowest;
            var current = highest - value;
            return canvasheight * current / range;
        }

        public int IntervalWidth
        {
            get
            { return _intervalWidth; }
            set
            {
                if(_intervalWidth != value)
                {
                    _intervalWidth = value;
                }
            }
        }
        private int _intervalWidth = 10;

        public IList Items
        {
            get
            {  return _items; }
            set
            {
                if(_items != value)
                {
                    _items = value;
                    OnItemsChanged();
                }
            }
        }
        private IList _items = null;

        /// <summary>
        /// For temp parsed value storage
        /// </summary>
        /*protected List<dynamic> Values
        {
            get;
            set;
        }*/

        /// <summary>
        /// Canvas to draw shapes
        /// </summary>
        protected Canvas Canvas
        {
            get;
            set;
        }

        /// <summary>
        /// Identifies <see cref="Brush"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty BrushProperty = DependencyProperty.Register(
            "Brush", typeof(Brush), typeof(LinearGraph),
            new PropertyMetadata(null)
            );

        /// <summary>
        /// Gets or sets brush for the graph.
        /// This is a dependency property.
        /// </summary>
        public Brush Brush
        {
            get { return (Brush)GetValue(BrushProperty); }
            set { SetValue(BrushProperty, value); }
        }

        public static readonly DependencyProperty ValueMemberPathProperty = DependencyProperty.Register(
           "ValueMemberPath", typeof(string), typeof(LinearGraph), null);
        public string ValueMemberPath
        {
            get { return (string)GetValue(ValueMemberPathProperty); }
            set { SetValue(ValueMemberPathProperty, value); }
        }

        public virtual string OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var p = e.GetPosition(Canvas);

            var index = (int)(p.X / IntervalWidth);
            
            if (index >= 0 && index < Items.Count)
            {
                var data = Items[index];
                CandleStick dd = (CandleStick)data;
                string stick = dd.Volume.ToString();               

                return $"{stick}";
            }
            return string.Empty;            
        }
    }
}
