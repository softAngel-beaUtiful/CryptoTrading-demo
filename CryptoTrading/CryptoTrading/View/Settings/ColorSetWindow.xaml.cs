using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;
using CryptoTrading.Model;
using CryptoTrading.ViewModel;

namespace CryptoTrading
{
    /// <summary>
    /// Interaction logic for CommonDisplayWindow.xaml
    /// </summary>
    public partial class ColorSetWindow
    {
        public ColorSetWindow()
        {
            InitializeComponent();
        }

        private ColorSetVM VM = new ColorSetVM();

        private ColorSet ColorSetBackup;// = Utility.Deserialize<ColorSet>(Utility.Serialize<ColorSet>(VM.ColorSetModelObj));

        private void CommonDisplayWin_Loaded(object sender, RoutedEventArgs e)
        {
            ColorSetBackup = Utility.Deserialize<ColorSet>(Utility.Serialize<ColorSet>(VM.ColorSetModelObj));

            this.DataContext = VM;


        //LoadColorSetConfig();

        //cpWindowBackground.BorderBrush = new SolidColorBrush(VM.ColorSetObj.WindowBackground);
        //cpChangeUpForeground.BorderBrush = new SolidColorBrush(VM.ColorSetObj.ChangeUpForeground);
        //cpChangeDownForeground.BorderBrush = new SolidColorBrush(VM.ColorSetObj.ChangeDownForeground);

        //ChangeButtonBackground(cpChangeDownForeground);
        //ChangeButtonBackground(cpChangeUpForeground);
        //ChangeButtonBackground2(cpWindowBackground);

        //Binding bindingShowIsOpenText = new Binding("CPColorPanelIsOpen");
        //bindingShowIsOpenText.Converter = new ColorSetVM.BoolToStringConverter();
        //tbShowIsOpen.SetBinding(TextBox.TextProperty, bindingShowIsOpenText);

    }

        private void CommonDisplayWin_Closed(object sender, EventArgs e)
        {
            //Save();
        }

        public override bool Save()
        {
            Trader.Configuration.ColorSetModelObj = this.VM.ColorSetModelObj;

            return true;
        }

        public override bool Cancel()
        {
            Trader.Configuration.ColorSetModelObj = this.ColorSetBackup;
            return true;
        }

        public override bool Default()
        {
            Trader.Configuration.ColorSetModelObj = this.ColorSetBackup;
            return true;
        }

        private void SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            //e.
        }

        //private void tbWindowBackground_GotFocus(object sender, RoutedEventArgs e)
        //{
        //    VM.CPColorPanelIsOpen = true;
        //}

        //public void ChangeButtonBackground(Xceed.Wpf.Toolkit.ColorPicker cpColorPicker)
        //{
        //    //将当前点击按钮背景设置为点击下面的样式
        //    LinearGradientBrush brushNow = new LinearGradientBrush();
        //    brushNow.StartPoint = new Point(0, 0);
        //    brushNow.EndPoint = new Point(0, 1);

        //    brushNow.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0xEF, 0xEF, 0xEF), 0));
        //    brushNow.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0xF8, 0xF7, 0xE9), 1));

        //    DependencyObject dep = VisualTreeHelper.GetChild(cpColorPicker, 0);
        //    if ((dep != null) && (dep is Border))
        //    {
        //        (dep as Border).Background = brushNow;
        //    }
        //}

        //public void ChangeButtonBackground2(Xceed.Wpf.Toolkit.ColorPicker cpColorPicker)
        //{
        //    //将当前点击按钮背景设置为点击下面的样式
        //    LinearGradientBrush brushNow = new LinearGradientBrush();
        //    brushNow.StartPoint = new Point(0, 0);
        //    brushNow.EndPoint = new Point(0, 1);

        //    brushNow.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0xEF, 0xEF, 0xEF), 0));
        //    brushNow.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0xC3, 0xCB, 0xCB), 1));

        //    DependencyObject dep = VisualTreeHelper.GetChild(cpColorPicker, 0);
        //    if ((dep != null) && (dep is Border))
        //    {
        //        (dep as Border).Background = brushNow;
        //    }
        //}
    }
}
