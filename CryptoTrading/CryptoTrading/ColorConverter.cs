using System;
using System.Globalization;
using System.Windows.Data;

namespace CryptoTrading
{
    //数值-颜色值转换器
    [ValueConversion(typeof(string), typeof(string))]
    public class ColorConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 根据文字显示颜色
            string strValue = value.ToString();
            decimal decValue;
            decimal.TryParse(strValue, out decValue);
            if (decValue < 0)
            {
                return "Green";
            }
            else 
            if (decValue > 0)
            {
                return "Red";
            }
            else
            {
                return "Black";
            }           
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {

            return "";
        }
    }
}
