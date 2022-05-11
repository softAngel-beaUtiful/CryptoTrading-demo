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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CryptoTrading
{
    /// <summary>
    /// Interaction logic for BareTextBox.xaml
    /// 屏蔽了粘贴、复制、全选、全选、剪切、撤销等默认命令的TextBox
    /// </summary>
    public partial class BareTextBox : TextBox
    {
        public BareTextBox()
        {
            InitializeComponent();
            //屏蔽文本框中的Ctrl+A,Ctrl+X,Ctrl+V,Ctrl+A,Ctrl+Z
            KeyBinding keyBidCtrlA = new KeyBinding(ApplicationCommands.NotACommand, Key.A, ModifierKeys.Control);
            this.InputBindings.Add(keyBidCtrlA);

            KeyBinding keyBidCtrlX = new KeyBinding(ApplicationCommands.NotACommand, Key.A, ModifierKeys.Control);
            this.InputBindings.Add(keyBidCtrlX);

            KeyBinding keyBidCtrlV = new KeyBinding(ApplicationCommands.NotACommand, Key.V, ModifierKeys.Control);
            this.InputBindings.Add(keyBidCtrlV);

            KeyBinding keyBidCtrlC = new KeyBinding(ApplicationCommands.NotACommand, Key.C, ModifierKeys.Control);
            this.InputBindings.Add(keyBidCtrlC);

            KeyBinding keyBidCtrlZ = new KeyBinding(ApplicationCommands.NotACommand, Key.Z, ModifierKeys.Control);
            this.InputBindings.Add(keyBidCtrlZ);


            this.IsReadOnly = true;
            this.KeyDown += BareTextBox_KeyDown;
        }

        void BareTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            StringBuilder strCommand = new StringBuilder();
            if(e.Key==Key.Tab || e.Key== Key.Return || e.Key==Key.Enter)
            { return; }
            if ((e.KeyboardDevice.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                strCommand.AppendFormat("{0}+", ModifierKeys.Control.ToString());
            }
            if ((e.KeyboardDevice.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
            {
                strCommand.AppendFormat("{0}+", ModifierKeys.Alt.ToString());
            }
            if ((e.KeyboardDevice.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
            {
                strCommand.AppendFormat("{0}+", ModifierKeys.Shift.ToString());
            }
            strCommand.Append(e.Key.ToString());
            e.Handled = true;
            this.Text = strCommand.ToString();
        }
    }
}
