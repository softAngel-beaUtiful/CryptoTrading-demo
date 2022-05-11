using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace CryptoTrading.TQLib
{
    public class TQOrderBoardButton : System.Windows.Controls.Button
    {
        public static readonly DependencyProperty LeftTextProperty =
            DependencyProperty.Register("LeftText", typeof(string), typeof(TQOrderBoardButton));

        public string LeftText
        {
            get
            {
                return ((string)(base.GetValue(TQOrderBoardButton.LeftTextProperty)));
            }
            set
            {
                base.SetValue(TQOrderBoardButton.LeftTextProperty, value);

            }
        }


        public static readonly DependencyProperty RightTextProperty =
            DependencyProperty.Register("RightText", typeof(string), typeof(TQOrderBoardButton));

        public string RightText
        {
            get
            {
                return ((string)(base.GetValue(TQOrderBoardButton.RightTextProperty)));
            }
            set
            {
                base.SetValue(TQOrderBoardButton.RightTextProperty, value);

            }
        }
        public static readonly DependencyProperty LeftgroundProperty =
            DependencyProperty.Register("Leftground", typeof(Brush), typeof(TQOrderBoardButton));

        public Brush Leftground
        {
            get
            {
                return ((Brush)(base.GetValue(TQOrderBoardButton.LeftgroundProperty)));
            }
            set
            {
                base.SetValue(TQOrderBoardButton.LeftgroundProperty, value);
            }

        }



        public static readonly DependencyProperty RightgroundProperty =
            DependencyProperty.Register("Rightground", typeof(Brush), typeof(TQOrderBoardButton));

        public Brush Rightground
        {
            get
            {
                return ((Brush)(base.GetValue(TQOrderBoardButton.RightgroundProperty)));
            }
            set
            {
                base.SetValue(TQOrderBoardButton.RightgroundProperty, value);

            }
        }


        public static readonly DependencyProperty MidgroundProperty =
            DependencyProperty.Register("Midground", typeof(Brush), typeof(TQOrderBoardButton));

        public Brush Midground
        {
            get
            {
                return ((Brush)(base.GetValue(TQOrderBoardButton.MidgroundProperty)));
            }
            set
            {
                base.SetValue(TQOrderBoardButton.MidgroundProperty, value);

            }
        }


    }
}
