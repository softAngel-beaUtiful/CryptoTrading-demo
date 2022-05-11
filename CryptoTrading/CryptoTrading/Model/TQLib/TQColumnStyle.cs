using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace CryptoTrading.TQLib
{
    /// <summary>
    /// 列 Style 的各个属性，如背景色，对齐等
    /// </summary>
    public class TQStyle
    {
        public SolidColorBrush ColumnForegroundBindingProperty { get; set; }
        public SolidColorBrush ColumnBackroundBindingProperty { get; set; }

        public System.Windows.HorizontalAlignment ColumnHorizontalAlignment { get; set; }

        public string RowVisibilityBindingProperty { get; set; }
    }
    public enum EnumColor
    {
        Black,
        Blue,
        Gray,
        Green,
        Brown,
    }


}
