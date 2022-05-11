using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml;

namespace CryptoTrading
{
    /// <summary>
    /// 用户自定义命令
    /// </summary>
    public class CustomCommands
    {
        private static List<string> sysCmdKeys = new List<string>()
        {
            "Windows+Break",
            "Windows+D",
            "Windows+M",
            "Windows+E",
            "Windows+F",
            "Windows+F1",
            "Windows+R",
            "Windows+U",
            "Windows+L",
            "Control+S",
            "Control+W",
            "Control+N",
            "Control+O",
            "Control+Z",
            "Control+F",
            "Control+X",
            "Control+C",
            "Control+V",
            "Control+A",
            "Control+B",
            "Control+I",
            "Control+U",
            "Alt+W"
        };
        static CustomCommands()
        {
            //初始化菜单选择命令

        }
        /// <summary>
        /// 初始化配置文件中的命令
        /// </summary>
        public static void InitCmd()
        {
            if (TQMain.dicInstrumentIDsGroup.Count > 0)
            {
                foreach (var item in TQMain.dicInstrumentIDsGroup.Values)
                {
                    if (!string.IsNullOrEmpty(item.CmdKey))
                    {
                        RoutedUICommand cmd = AddCommand(item.CmdKey, item.Name, "ChangeGroup");
                        CommandBinding cb = new CommandBinding();
                        cb.Command = cmd;
                        cb.CanExecute += TQMain.T.main.ChangeGroup_CanExecute;
                        cb.Executed += TQMain.T.main.ChangeGroup_Executed;
                        TQMain.T.main.CommandBindings.Add(cb);
                    }
                }
            }
            if (Trader.DefaultInstrumentQuant.Count > 0)
            {
                foreach (var item in Trader.DefaultInstrumentQuant)
                {
                    if (!string.IsNullOrEmpty(item.Value.CmdKey))
                    {
                        RoutedUICommand cmd =AddCommand(item.Value.CmdKey, item.Value.InstrumentID, SettingsType.DefaultQuant.ToString());
                        CommandBinding cb = new CommandBinding();
                        cb.Command = cmd;
                        cb.CanExecute += TQMain.T.main.DefaultQuant_CanExecute;
                        cb.Executed += TQMain.T.main.DefaultQuant_Executed;
                        TQMain.T.main.CommandBindings.Add(cb);
                    }
                }
            }

            if (Trader.Configuration.TradeCmdList.Count > 0)
            {
                foreach (var item in Trader.Configuration.TradeCmdList)
                {
                    if (!string.IsNullOrEmpty(item.CmdKey))
                    {
                        RoutedUICommand cmd = AddCommand(item.CmdKey, item.Action, SettingsType.DefaultQuant.ToString());
                        CommandBinding cb = new CommandBinding();
                        cb.Command = cmd;
                        cb.CanExecute += TQMain.T.main.TradeCommand_CanExecute;
                        cb.Executed += TQMain.T.main.TradeCommand_Executed;
                        TQMain.T.main.CommandBindings.Add(cb);
                    }
                }
            }
        }
        public static List<RoutedUICommand> CommandList = new List<RoutedUICommand>();
        public static List<string> CommandStrList = new List<string>();

        public static List<string> CommandNameList = new List<string>();
        public static RoutedUICommand AddCommand(string cmdKeys, string cmdName)
        {
            RoutedUICommand command;
            string[] keys = cmdKeys.Split('+');
            Key k = Key.None;
            ModifierKeys modifierKeys, mk;
            modifierKeys = ModifierKeys.None;
            switch (keys.Count())
            {
                case 1:
                    Enum.TryParse(keys[0], out k);
                    break;
                case 2:
                    Enum.TryParse(keys[0], out modifierKeys);
                    Enum.TryParse(keys[1], out k);
                    break;
                case 3:
                    Enum.TryParse(keys[0], out mk);
                    modifierKeys = mk;
                    Enum.TryParse(keys[1], out mk);
                    modifierKeys = modifierKeys | mk;
                    Enum.TryParse(keys[2], out k);
                    break;
                case 4:
                    Enum.TryParse(keys[0], out mk);
                    modifierKeys = mk;
                    Enum.TryParse(keys[1], out mk);
                    modifierKeys = modifierKeys | mk;
                    Enum.TryParse(keys[2], out mk);
                    modifierKeys = modifierKeys | mk;
                    Enum.TryParse(keys[3], out k);
                    break;
            }
            KeyGesture keyGesture = (modifierKeys== ModifierKeys.None)? new KeyGesture(k): new KeyGesture(k, modifierKeys);
            command = new RoutedUICommand(cmdName, cmdName, typeof(CustomCommands), new InputGestureCollection() { keyGesture });
            CommandList.Add(command);
            CommandStrList.Add(cmdKeys);
            CommandNameList.Add(cmdName);
            return command;
        }
        public static RoutedUICommand AddCommand(string cmdKeys, string cmdName,string displayText)
        {
            RoutedUICommand command;
            string[] keys = cmdKeys.Split('+');
            Key k = Key.None;
            ModifierKeys modifierKeys, mk;
            modifierKeys = ModifierKeys.None;
            switch (keys.Count())
            {
                case 1:
                    Enum.TryParse(keys[0], out k);
                    break;
                case 2:
                    Enum.TryParse(keys[0], out modifierKeys);
                    Enum.TryParse(keys[1], out k);
                    break;
                case 3:
                    Enum.TryParse(keys[0], out mk);
                    modifierKeys = mk;
                    Enum.TryParse(keys[1], out mk);
                    modifierKeys = modifierKeys | mk;
                    Enum.TryParse(keys[2], out k);
                    break;
                case 4:
                    Enum.TryParse(keys[0], out mk);
                    modifierKeys = mk;
                    Enum.TryParse(keys[1], out mk);
                    modifierKeys = modifierKeys | mk;
                    Enum.TryParse(keys[2], out mk);
                    modifierKeys = modifierKeys | mk;
                    Enum.TryParse(keys[3], out k);
                    break;
            }
            KeyGesture keyGesture = (modifierKeys == ModifierKeys.None) ? new KeyGesture(k) : new KeyGesture(k, modifierKeys);
            command = new RoutedUICommand(displayText, cmdName, typeof(CustomCommands), new InputGestureCollection() { keyGesture });
            
            CommandList.Add(command);
            CommandStrList.Add(cmdKeys);
            CommandNameList.Add(cmdName);
            return command;
        }
        public static bool Exist(string cmdKeys)
        {
            return CommandStrList.Contains(cmdKeys);
        }

        public static bool Exist(RoutedUICommand cmd)
        {
            return CommandList.Contains(cmd);
        }
        /// <summary>
        /// 检查快捷键是否已被其它软件注册
        /// </summary>
        /// <param name="cmdKeys"></param>
        /// <returns></returns>
        public static bool ExistInAnotherSoftware(string cmdKeys)
        {
            return sysCmdKeys.Contains(cmdKeys);
        }

        /// <summary>
        /// 注销命令
        /// </summary>
        /// <param name="cmdName"></param>
        /// <returns></returns>
        public static bool UnregisterCommand(string cmdName)
        {
            var cm = CustomCommands.CommandList.Where(x => x.Name == cmdName);
            if (cm != null && cm.Count() > 0)
            {
                RoutedUICommand cmd = cm.ToList()[0];
                int ncount = TQMain.T.main.CommandBindings.Count;
                for (int i = 0; i < ncount; i++)
                {
                    if (TQMain.T.main.CommandBindings[i].Command == cmd)
                    {
                        TQMain.T.main.CommandBindings.RemoveAt(i);
                        CustomCommands.CommandList.Remove(cmd);
                        CommandNameList.Remove(cmdName);
                        string cmdStr = ((KeyGesture)cmd.InputGestures[0]).Modifiers.ToString() + "+" + ((KeyGesture)cmd.InputGestures[0]).Key.ToString();
                        CommandStrList.Remove(cmdStr);
                        break;
                    }
                }
                return true;
            }
            return false;
        }
    }
}
