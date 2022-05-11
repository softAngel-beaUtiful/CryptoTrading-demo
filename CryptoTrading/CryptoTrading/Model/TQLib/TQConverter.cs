using CryptoTrading.Model;
using CryptoTrading.ViewModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CryptoTrading.TQLib
{
    /// <summary>
    /// Bool 到 String 转换 Converter
    /// </summary>
    [ValueConversion(typeof(bool), typeof(string))]
    public class BoolToStringConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool used = (bool)value;
            return used ? "True" : "False";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value.ToString().ToLower())
            {
                case "true":
                    return true;
                default:
                    return false;
            }
        }
    }
    /// <summary>
    /// Bool 到 String( Collapsed / Visible ) 转换 Converter
    /// false -> Visible
    /// true -> Collapsed
    /// </summary>
    [ValueConversion(typeof(bool), typeof(string))]
    public class BoolFalseToVisibilityStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool used = (bool)value;
            return used ? "Collapsed" : "Visible";
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value.ToString().ToLower())
            {
                case "Collapsed":
                    return true;
                default:
                    return false;
            }
        }
    }
    /// <summary>
    /// FakFok 枚举转换到 bool?
    /// FOK
    /// FAK
    /// </summary>
    [ValueConversion(typeof(FakFok), typeof(bool))]
    public class OrderBoardFakFokConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString() == parameter.ToString();
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                return (FakFok)Enum.Parse(typeof(FakFok), parameter.ToString());
            }
            return null;
        }
    }
    /// <summary>
    /// PricingMode 枚举转换到 bool?
    /// 限价（设定价，对手价，排队价）
    /// 市价
    /// </summary>
    [ValueConversion(typeof(PricingMode), typeof(bool))]
    public class OrderBoardPricingModeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString() == parameter.ToString();                    
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                return Enum.Parse(typeof(PricingMode), parameter.ToString());                
            }
            return null;
        }
    }
    /// <summary>
    /// HedgeRatio 枚举转换到 bool?
    /// 10\20倍杠杆
    /// 
    /// </summary>
    [ValueConversion(typeof(HedgeRatio), typeof(bool))]
    public class OrderBoardHedgeRatioConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString() == parameter.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                return (HedgeRatio)Enum.Parse(typeof(HedgeRatio), parameter.ToString());                
            }
            return null;
        }
    }
    /*
    /// <summary>
    /// OrderMode 枚举转换到 bool?
    /// </summary>
    [ValueConversion(typeof(OrderMode), typeof(bool))]
    public class OrderBoardOrderModeConverter : IValueConverter
    {
        /*
        自动
        平仓
        开仓
        
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {            
            return value.ToString() == parameter.ToString();           
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {           
            if ((bool)value)
            {
                return Enum.Parse(typeof(OrderMode), parameter.ToString());               
            }
            return null;
        }
    }*/
    /*
    /// <summary>
    /// OrderMode 枚举转换到 string
    /// </summary>
    [ValueConversion(typeof(OrderMode), typeof(string))]
    public class OrderBoardOrderModeTextConverter : IValueConverter
    {
        /*
        自动
        平仓
        开仓
        
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value.ToString())
            {
                case "Auto":
                    return "自动开平";
                default:// 开仓
                    return "指定开仓";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    */
    /// <summary>
    /// QuantMode 枚举转换到 bool?
    /// </summary>
    [ValueConversion(typeof(QuantMode), typeof(bool?))]
    public class OrderBoardQuantModeConverter : IValueConverter
    {       
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            QuantMode s = (QuantMode)value;

            if (s == QuantMode.Default) return null;

            return s == QuantMode.AllAvailable ? false : true;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return QuantMode.Default;
            else if (value.ToString()=="True")
                return QuantMode.Preset;
            else return QuantMode.AllAvailable;            
        }
    }

    ///// <summary>
    ///// PriceMode 枚举转换到 string
    ///// </summary>
    //[ValueConversion(typeof(PriceMode), typeof(string))]
    //public class OrderBoardPriceModeConverter : IValueConverter
    //{
    //    /* new
    //    <Trigger Property="IsChecked" Value="{x:Null}">
    //        <Setter Property="Content" Value="限定价"></Setter>
    //        <Setter Property="Foreground" Value="Red"></Setter>
    //    </Trigger>
    //    <Trigger Property="IsChecked" Value="false">
    //        <Setter Property="Content" Value="对手价"></Setter>
    //        <Setter Property="Foreground" Value="BlueViolet"></Setter>
    //    </Trigger>
    //    <Trigger Property="IsChecked" Value="true">
    //        <Setter Property="Content" Value="排队价"></Setter>
    //        <Setter Property="Foreground" Value="Green"></Setter>
    //    </Trigger>
    //    */
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        PriceMode pm = (PriceMode)value;

    //        return pm == PriceMode.PreSet ? "{x:Null}" : pm == PriceMode.Opposite ? "False": "True";
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        if(value == null) return PriceMode.Opposite;

    //        switch (value.ToString().ToLower())
    //        {
    //            case "true":
    //                return PriceMode.Ownside;
    //            //case "false":
    //            default:
    //                return PriceMode.PreSet;

    //        }
    //    }
    //}

    ///// <summary>
    ///// PriceMode 到 描述（string） 转换 Converter
    ///// </summary>
    //[ValueConversion(typeof(PriceMode), typeof(string))]
    //public class OrderBoardPriceModeDescriptionConverter : IValueConverter
    //{

    //    /* new
    //    <Trigger Property="IsChecked" Value="{x:Null}">
    //        <Setter Property="Content" Value="限定价"></Setter>
    //        <Setter Property="Foreground" Value="Red"></Setter>
    //    </Trigger>
    //    <Trigger Property="IsChecked" Value="false">
    //        <Setter Property="Content" Value="对手价"></Setter>
    //        <Setter Property="Foreground" Value="BlueViolet"></Setter>
    //    </Trigger>
    //    <Trigger Property="IsChecked" Value="true">
    //        <Setter Property="Content" Value="排队价"></Setter>
    //        <Setter Property="Foreground" Value="Green"></Setter>
    //    </Trigger>
    //    */
    //    /* old
    //    <Trigger Property="IsChecked" Value="{x:Null}">
    //        <Setter Property="Content" Value="对手价"></Setter>
    //        <Setter Property="Foreground" Value="Red"></Setter>
    //    </Trigger>
    //    <Trigger Property="IsChecked" Value="false">
    //        <Setter Property="Content" Value="限定价"></Setter>
    //        <Setter Property="Foreground" Value="BlueViolet"></Setter>
    //    </Trigger>
    //    <Trigger Property="IsChecked" Value="true">
    //        <Setter Property="Content" Value="排队价"></Setter>
    //        <Setter Property="Foreground" Value="Green"></Setter>
    //    </Trigger>
    //    */
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        PriceMode pm = (PriceMode)value;

    //        return pm == PriceMode.PreSet ? "限定价" : pm == PriceMode.Opposite ? "对手价" : "排队价";
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}


    //public class TQColorToColorConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        TQColor source = value as TQColor;
    //        return source.Color;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        System.Windows.Media.Color target = (System.Windows.Media.Color)value;

    //        return new TQColor() { Color = target };
    //    }
    //}
}
