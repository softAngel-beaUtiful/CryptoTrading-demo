using System;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace CryptoTrading.TQLib
{
    public class TQScrollBar : ScrollBar
    {        
        public static readonly DependencyProperty ValueTextProperty = DependencyProperty.Register("ValueText", typeof(string), typeof(TQScrollBar));
        // 鼠标移入时自动全选。
        public static readonly DependencyProperty AutoSelectAllOnMouseMoveProperty =
            DependencyProperty.RegisterAttached("AutoSelectAllOnMouseMove", typeof(bool), typeof(TQScrollBar),
                new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnAutoSelectAllOnMouseMoveChanged)));
        public string ValueText   //get access to ValueTextProperty
        {
            get
            {
                return (string)(GetValue(ValueTextProperty));
            }
            set
            {
                SetValue(ValueTextProperty, value);                
            }
        }       
        //public new double Value
        //{
        //    get { if (ValueText != "市价" || ValueText != null) return double.Parse(ValueText);
        //        else return 0;
        //    }
        //    set { ValueText = value.ToString(); }
        //}
        public static bool GetAutoSelectAllOnMouseMove(TQScrollBar d)
        {
            return (bool)d.GetValue(AutoSelectAllOnMouseMoveProperty);
        }
        public static void SetAutoSelectAllOnMouseMove(TQScrollBar d, bool value)
        {
            d.SetValue(AutoSelectAllOnMouseMoveProperty, value);
        }
        private static void OnAutoSelectAllOnMouseMoveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var myScrollBar = d as TQScrollBar;
            if (myScrollBar != null)
            {
                var flag = (bool)e.NewValue;
                if (flag)
                {
                    myScrollBar.MouseMove += OnMouseMove;
                    //if (myScrollBar.TextBox != null)
                    //    myScrollBar.TextBox.GotFocus += TextBoxOnGotFocus;
                }
                else
                {
                    myScrollBar.MouseMove -= OnMouseMove;
                    //if (myScrollBar.TextBox != null)
                    //    myScrollBar.TextBox.GotFocus -= TextBoxOnGotFocus;
                }
            }
        }
        private static void OnMouseMove(object sender, RoutedEventArgs e)
        {
            TQScrollBar myScrollBar = sender as TQScrollBar;
            if (myScrollBar != null)
            {
                myScrollBar.SelectAll();
            }
        }
        private System.Windows.Controls.TextBox _TextBox;
        public System.Windows.Controls.TextBox TextBox
        {
            get
            {
                if (_TextBox == null)
                    _TextBox = GetTemplateChild("Text") as System.Windows.Controls.TextBox;

                return _TextBox;
            }
        }
        public void SelectAll()
        {
            if (TextBox != null)
            {
                TextBox.Focus();
                TextBox.SelectAll();
            }
        }
    }   
}
