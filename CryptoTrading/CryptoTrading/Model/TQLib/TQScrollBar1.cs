using System;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace CryptoTrading.TQLib
{
    public class TQScrollBar1 : ScrollBar
    {
        public static readonly DependencyProperty ValueTextProperty =
            DependencyProperty.Register("ValueText", typeof(string), typeof(TQScrollBar1));
       
        public string ValueText
        {
            get
            {
                try
                {
                    return ((string)(GetValue(ValueTextProperty)));                    
                }
                catch (Exception e)
                {
                    Utility.WriteMemLog("error GetValue" + ValueTextProperty.ToString() + " \n " +
                         e.ToString());
                    return null;
                }
            }
            set
            {
                try
                {
                    SetValue(ValueProperty, value);
                    //double vv;
                    //if (double.TryParse(value, out vv))
                    //    Value = vv;
                    //else Value = 0;
                }
                catch (Exception e)
                {
                    Utility.WriteMemLog("error setValue" + ValueTextProperty.ToString() + " \n " +
                         e.ToString());
                }
            }
        }
        // 鼠标移入时自动全选。
        public static readonly DependencyProperty AutoSelectAllOnMouseMoveProperty =
            DependencyProperty.RegisterAttached("AutoSelectAllOnMouseMove", typeof(bool), typeof(TQScrollBar1),
                new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnAutoSelectAllOnMouseMoveChanged)));

        public static bool GetAutoSelectAllOnMouseMove(TQScrollBar1 d)
        {
            return (bool)d.GetValue(AutoSelectAllOnMouseMoveProperty);
        }
        public static void SetAutoSelectAllOnMouseMove(TQScrollBar1 d, bool value)
        {
            d.SetValue(AutoSelectAllOnMouseMoveProperty, value);
        }
        private static void OnAutoSelectAllOnMouseMoveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TQScrollBar1 myScrollBar = d as TQScrollBar1;
            if (myScrollBar != null)
            {
                bool flag = (bool)e.NewValue;
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
            TQScrollBar1 myScrollBar = sender as TQScrollBar1;
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
