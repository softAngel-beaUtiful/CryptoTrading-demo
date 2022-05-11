#define Debug
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Net.Mail;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;
using CryptoTrading.TQLib;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Security.Cryptography;
using CryptoTrading.Model;
using Newtonsoft.Json.Linq;
using WebSocketSharp;

namespace CryptoTrading
{
    public static class Utility
    {
        static Utility()
        {
            pLog = new Logger(LogLevel.Trace, ".log", WriteToLogFile);
        }
        static object forwritememfile = new object();
        /// <summary>
        /// 加密签名生成
        /// </summary>
        /// <param name="parameters">所有字符串型的参数</param>
        /// <param name="secretKey">签名秘钥</param>
        /// <returns>签名</returns>
        public static string GenerateSign(IDictionary<string, string> parameters, string secretKey)
        {
            IDictionary<string, string> sortedParams = new SortedDictionary<string, string>(parameters);
            IEnumerator<KeyValuePair<string, string>> dem = sortedParams.GetEnumerator();
            StringBuilder query = new StringBuilder();
            while (dem.MoveNext())
            {
                string key = dem.Current.Key;
                string value = dem.Current.Value;
                if (!string.IsNullOrEmpty(key))
                {
                    query.Append(key).Append("=").Append(value).Append("&");
                }
            }
            query.Append("secret_key=");
            query.Append(secretKey);
            //使用MD5加密
            MD5 md5 = MD5.Create();
            byte[] bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(query.ToString()));
            //把二进制转化为大写的十六进制
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                result.Append(bytes[i].ToString("X2"));
            }
            return result.ToString();
        }
        static string GetMd5Hash(MD5 md5Hash, string input)
        {
            // Convert the input string to a byte array and compute the hash.
            byte[] dat = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data
            // and format each one as a hexadecimal string.
            for (int i = 0; i < dat.Length; i++)
            {
                sBuilder.Append(dat[i].ToString("X2"));
            }
            //string st = dat.ToString();
            // Return the hexadecimal string.
            return sBuilder.ToString();
        }
        static bool VerifyMd5Hash(MD5 md5Hash, string input, string hash)
        {
            // Hash the input.
            string hashOfInput = GetMd5Hash(md5Hash, input);

            // Create a StringComparer an compare the hashes.
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            if (0 == comparer.Compare(hashOfInput, hash))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #region 日志处理

        public static void WriteMemLogToLogFile(IEnumerable<string> txt)
        {
            lock (forwritememfile)
            {
                var path = string.Format("{0}/Log/{1}.log", Environment.CurrentDirectory, DateTime.Now.ToString("yyMMdd"));
                File.AppendAllLines(path, txt, Encoding.UTF8);
                while (TQMainModel.MemLog.Count > 0)
                    TQMainModel.MemLog.TryDequeue(out string re);
            }
        }
        public static void WriteMemFile(string txt)
        {
            var path = string.Format("{0}/Log/{1}.log", Environment.CurrentDirectory, DateTime.Now.ToString("yyMMdd"));
            lock (forwritememfile)
            {
                File.AppendAllLines(path, new List<string> { string.Format("{0:G} {1} ", DateTime.Now.TimeOfDay, txt) }, Encoding.UTF8);
            }
        }
        public static void WriteTickToFile(List<FutureMarketData> lfm)
        {
            //if (lfm.Count == 0) return;
            lock (forwritememfile)
            {
                if (lfm.Count == 0) return;
                var path = string.Format("{0}/okex/TickData/{1}", Environment.CurrentDirectory, lfm[0].InstrumentID);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                path += string.Format("/{0}.tck", DateTime.Now.ToString("yyMMdd"));
                List<string> ll = new List<string>();
                for (int i = 0; i < lfm.Count; i++)
                {
                    ll.Add(JsonConvert.SerializeObject(lfm[i], Newtonsoft.Json.Formatting.None));
                }
                File.AppendAllLines(path, ll, Encoding.UTF8);
            }
        }
        public static void WriteMemLog(string txt)
        {
            TimeSpan dt = DateTime.Now.TimeOfDay;
            TQMainModel.MemLog.Enqueue(string.Format("Time {0} {1}", dt, txt));
            if (TQMainModel.MemLog.Count>1000)
            {
                WriteMemLogToLogFile(TQMainModel.MemLog);               
            }
        }
        #endregion
        /// <summary>
        /// 获取对象的属性信息
        /// </summary>
        /// <param name="o">对象</param>
        /// <returns>对象所有的公开属性信息(属性名：属性值),以",\t"分割</returns>
        public static string GetObjectPropertyInfo(object o)
        {
            StringBuilder sbResult = new StringBuilder();
            Type t = o.GetType();
            foreach (PropertyInfo pi in t.GetProperties())
            {
                sbResult.AppendFormat("{0}:{1},\t", pi.Name, pi.GetValue(o, null));

            }
            return sbResult.ToString();
        }

        /// <summary>
        /// 获取对象的字段信息
        /// </summary>
        /// <param name="o">对象</param>
        /// <returns>对象所有的公开字段信息(字段名：字段值),以",\t"分割</returns>
        public static string GetObjectFieldInfo(object o)
        {
            var sbResult = new StringBuilder();
            var t = o.GetType();
            foreach (var pi in t.GetFields())
            {
                sbResult.AppendFormat("{0}:{1},\t", pi.Name, pi.GetValue(o));

            }
            return sbResult.ToString();
        }
        public static string GetBaseName(string Instru)
        {
            switch (Instru.Substring(0, 3))
            {
                case ("eth"):
                    return "以太币";
                case ("eos"):
                    return "柚子币";
                case ("ltc"):
                    return "莱特币";
                case ("bch"):
                    return "比特现金";
                case ("XBTUSD"):
                    return "XBTUSD";
                case ("XBTU18"):
                    return "XBTU18";
                case ("XBTZ18"):
                    return "XBTZ18";
                case ("btc"):
                default:
                    return "比特币";
            }
        }
        #region 获取枚举char值对应的文本描述
        /// <summary>
        /// 根据密码标志获取其文本描述
        /// </summary>
        /// <param name="pwdFlag"></param>
        /// <returns></returns>
        public static string GetPwdFlagText(char pwdFlag)
        {
            string pwdFlagText = String.Empty;
            switch (pwdFlag)
            {
                case (char)PwdFlagType.BlankCheck:
                    pwdFlagText = EnumDescription.GetFieldText(PwdFlagType.BlankCheck);
                    break;
                case (char)PwdFlagType.EncryptCheck:
                    pwdFlagText = EnumDescription.GetFieldText(PwdFlagType.EncryptCheck);
                    break;
                case (char)PwdFlagType.NoCheck:
                    pwdFlagText = EnumDescription.GetFieldText(PwdFlagType.NoCheck);
                    break;
            }
            return pwdFlagText;
        }

        /// <summary>
        /// 根据银行帐户类型获取其文本描述
        /// </summary>
        /// <param name="bankAccType">银行帐户类型</param>
        /// <returns></returns>
        public static string GetBankAccTypeText(char bankAccType)
        {
            string bankAccTypeText = String.Empty;
            switch (bankAccType)
            {
                case (char)BankAccType.BankBook:
                    bankAccTypeText = EnumDescription.GetFieldText(BankAccType.BankBook);
                    break;
                case (char)BankAccType.BankCard:
                    bankAccTypeText = EnumDescription.GetFieldText(BankAccType.BankCard);
                    break;
                case (char)BankAccType.CreditCard:
                    bankAccTypeText = EnumDescription.GetFieldText(BankAccType.CreditCard);
                    break;
            }
            return bankAccTypeText;
        }

        /// <summary>
        /// 根据客户类型获取其文本描述
        /// </summary>
        /// <param name="custType">客户类型</param>
        /// <returns></returns>
        public static string GetCustTypeText(char custType)
        {
            string custTypeText = String.Empty;
            switch (custType)
            {
                case (char)CustType.Institution:
                    custTypeText = EnumDescription.GetFieldText(CustType.Institution);
                    break;
                case (char)CustType.Person:
                    custTypeText = EnumDescription.GetFieldText(CustType.Person);
                    break;
            }
            return custTypeText;
        }

        /// <summary>
        /// 根据证件类型获取其文本描述
        /// </summary>
        /// <param name="idCardType">证件类型</param>
        /// <returns></returns>
        public static string GetIdCardTypeText(char idCardType)
        {
            string idCardTypeText = String.Empty;
            switch (idCardType)
            {
                case (char)IdCardType.EID:
                    idCardTypeText = EnumDescription.GetFieldText(IdCardType.EID);
                    break;
                case (char)IdCardType.HomeComingCard:
                    idCardTypeText = EnumDescription.GetFieldText(IdCardType.HomeComingCard);
                    break;
                case (char)IdCardType.HouseholdRegister:
                    idCardTypeText = EnumDescription.GetFieldText(IdCardType.HouseholdRegister);
                    break;
                case (char)IdCardType.IDCard:
                    idCardTypeText = EnumDescription.GetFieldText(IdCardType.IDCard);
                    break;
                case (char)IdCardType.LicenseNo:
                    idCardTypeText = EnumDescription.GetFieldText(IdCardType.LicenseNo);
                    break;
                case (char)IdCardType.OfficerIDCard:
                    idCardTypeText = EnumDescription.GetFieldText(IdCardType.OfficerIDCard);
                    break;
                case (char)IdCardType.OtherCard:
                    idCardTypeText = EnumDescription.GetFieldText(IdCardType.OtherCard);
                    break;
                case (char)IdCardType.Passport:
                    idCardTypeText = EnumDescription.GetFieldText(IdCardType.Passport);
                    break;
                case (char)IdCardType.PoliceIDCard:
                    idCardTypeText = EnumDescription.GetFieldText(IdCardType.PoliceIDCard);
                    break;
                case (char)IdCardType.SoldierIDCard:
                    idCardTypeText = EnumDescription.GetFieldText(IdCardType.SoldierIDCard);
                    break;
                case (char)IdCardType.TaiwanCompatriotIDCard:
                    idCardTypeText = EnumDescription.GetFieldText(IdCardType.TaiwanCompatriotIDCard);
                    break;
                case (char)IdCardType.TaxNo:
                    idCardTypeText = EnumDescription.GetFieldText(IdCardType.TaxNo);
                    break;
            }
            return idCardTypeText;
        }

        /// <summary>
        /// 获取有效标志的文本描述
        /// </summary>
        /// <param name="availabilityFlag"></param>
        /// <returns></returns>
        public static string GetAvailabilityFlagText(char availabilityFlag)
        {
            string availabilityFlagText = String.Empty;
            switch (availabilityFlag)
            {
                case (char)AvailabilityFlagType.Invalid:
                    availabilityFlagText = EnumDescription.GetFieldText(AvailabilityFlagType.Invalid);
                    break;
                case (char)AvailabilityFlagType.Valid:
                    availabilityFlagText = EnumDescription.GetFieldText(AvailabilityFlagType.Valid);
                    break;
                case (char)AvailabilityFlagType.Repeal:
                    availabilityFlagText = EnumDescription.GetFieldText(AvailabilityFlagType.Repeal);
                    break;
            }
            return availabilityFlagText;
        }

        #endregion

        /// <summary>
        /// 根据货币代码获取货币名称
        /// </summary>
        /// <param name="currencyID">货币代码</param>
        /// <returns>货币名称</returns>
        public static string GetCurrencyName(string currencyID)
        {
            string currencyName = "人民币";
            if (currencyID == "HKD")
            {
                currencyName = "港币";
            }
            else if (currencyID == "USD")
            {
                currencyName = "美元";
            }
            return currencyName;
        }

        /// <summary>
        /// 根据货币名称获取货币代码
        /// </summary>
        /// <param name="currencyID">货币名称</param>
        /// <returns>货币代码</returns>
        public static string GetCurrencyID(string currencyName)
        {
            string currencyID = "CNY";
            if (currencyName == "港币")
            {
                currencyID = "HKD";
            }
            else if (currencyName == "美元")
            {
                currencyID = "USD";
            }
            return currencyID;
        }

        /// <summary>
        /// 根据交易所名字获取交易所ID
        /// </summary>
        /// <param name="exchangeName"></param>
        /// <returns></returns>
        public static EnuExchangeID GetExchangeID(string exchangeName)
        {
            InstrumentData instru;
            switch (exchangeName)
            {
                case "OkexFutures":
                    return EnuExchangeID.OkexFutures;
                default:
                    if (TQMainModel.dicInstrumentData.TryGetValue(exchangeName, out instru)) return instru.ExchangeID;
                    else return EnuExchangeID.BitMex;
            }
        }

        /// <summary>
        /// 根据合约代码获取PriceTick
        /// </summary>
        /// <param name="InstrumentID"></param>
        /// <returns></returns>
        public static double GetPriceTick(string InstrumentID)
        {
            InstrumentData instru;
            if (TQMainModel.dicInstrumentData.TryGetValue(InstrumentID, out instru)) return instru.PriceTick;
            else return 1;
        }

        /// <summary>
        /// 从Configuration.xml文件中加载指定节点需要在DataGrid显示的列
        /// </summary>
        /// <param name="dg">目标DataGrid控件</param>
        /// <param name="dgType">节点名</param>
        public static void LoadConfiguration(DataGrid dg, DataGridType dgType)
        {
            var Black = Brushes.Black;
            var Blue = Brushes.Blue; //new SolidColorBrush(Color.FromArgb(255, 51, 153, 255));
            var Gray = Brushes.Gray;//new SolidColorBrush(Color.FromArgb());
            dg.Columns.Clear();
            var win1 = new ColumnSetWindow(dgType);

            foreach (var v in win1.ViewColumsList)
            {
                if (v.Display != "True") continue;

                DataGridTextColumn textColumnDynamic = new DataGridTextColumn() { Header = v.Title };

                TQStyle columnStyle = StaticGridStyle.GetTQColumnStyle(dgType, v.Name);
                if (columnStyle != null)
                {
                    Style styleColumn = new Style();           
                    styleColumn.Setters.Add(new Setter(Control.BackgroundProperty, columnStyle.ColumnBackroundBindingProperty?? Brushes.Black));
                    styleColumn.Setters.Add(new Setter(Control.ForegroundProperty, columnStyle.ColumnForegroundBindingProperty?? Brushes.CadetBlue));
                    Trigger triggerDataGridCellSelected = new Trigger();
                    triggerDataGridCellSelected.Property = DataGridCell.IsSelectedProperty;
                    triggerDataGridCellSelected.Value = true;
                    triggerDataGridCellSelected.Setters.Add(new Setter(Control.ForegroundProperty, Brushes.DarkBlue)); // new SolidColorBrush(Color.FromArgb(255, 55, 255, 255))));                    
                    triggerDataGridCellSelected.Setters.Add(new Setter(Control.BackgroundProperty, Brushes.DarkGray));
                    styleColumn.Triggers.Add(triggerDataGridCellSelected);
                    Style styleElement = new Style();
                    styleElement.Setters.Add(new Setter(FrameworkElement.HorizontalAlignmentProperty, columnStyle.ColumnHorizontalAlignment));
                    textColumnDynamic.CellStyle = styleColumn;
                    textColumnDynamic.ElementStyle = styleElement;
                }
                Binding bindingColumnText = new Binding(v.Name);
                textColumnDynamic.Binding = bindingColumnText;
                dg.Columns.Add(textColumnDynamic);
            }

            //SolidColorBrush BrushRow1 = new SolidColorBrush(Color.FromRgb(240, 255, 240)); // 蜜露橙,240,255,240,#F0FFF0
            SolidColorBrush BrushRow1 = new SolidColorBrush(Color.FromRgb(255, 255, 255)); // 白色，闪电王单行色 #FFFFFF

            //SolidColorBrush BrushRow2 = new SolidColorBrush(Color.FromRgb(245,245,245)); // 白烟，245，245，245，#F5F5F5
            SolidColorBrush BrushRow2 = new SolidColorBrush(Color.FromRgb(234, 234, 234)); // 闪电王双行色 #EAEAEA

            // new SolidColorBrush(Color.FromRgb(189, 252, 201)); // 薄荷色 189,252,201,#BDFCC9
            // new SolidColorBrush(Color.FromRgb(221, 160, 221)); // 梅红色 221 160 221 #DDA0DD
            // new SolidColorBrush(Color.FromRgb(127, 255, 212)); // 碧绿色 #7FFFD4
            // new SolidColorBrush(Color.FromRgb(176,224,230)); // 浅灰蓝色，176，224，230，#B0E0E6

            //Style theCellStyle = new Style();
            //theCellStyle.Setters.Add(new Setter(DataGridCell.VerticalAlignmentProperty, VerticalAlignment.Center));
            //theCellStyle.Setters.Add(new Setter(DataGridCell.HorizontalAlignmentProperty, HorizontalAlignment.Right));
            //dg.CellStyle = theCellStyle;

            //dg.RowStyle.Setters.Add(new Setter(DataGridRow.HorizontalContentAlignmentProperty, AlignmentY.Center));

            dg.RowBackground = BrushRow1;
            dg.AlternatingRowBackground = BrushRow2;

            //dg.Background

            //dg.Foreground = new SolidColorBrush(Color.FromRgb(49, 49, 49));//#313131 闪电王 正常 颜色
            //dg.Foreground = Trader.Configuration.ColorSetModelObj.GridForeground.ColorBrush;//#313131 闪电王 正常 颜色

            Binding bDataGridForeground = new Binding("ColorSetModelObj.GridForeground.ColorBrush")
            {
                Source = Trader.Configuration
            };
            dg.SetBinding(DataGrid.ForegroundProperty, bDataGridForeground);

            Binding bDataGridBackground = new Binding("ColorSetModelObj.GridBackground.ColorBrush")
            {
                Source = Trader.Configuration
            };
            dg.SetBinding(DataGrid.BackgroundProperty, bDataGridBackground);

            //dg.Foreground = Trader.Configuration.ColorSetModelObj.GridForeground.ColorBrush;// new SolidColorBrush(Color.FromRgb(49, 49, 49));//#313131 闪电王 正常 颜色
            //dg.Background = Trader.Configuration.ColorSetModelObj.GridBackground.ColorBrush;// new SolidColorBrush(Color.FromRgb(234, 234, 234));//#EAEAEA 闪电王 窗格背景色

            //BindingOperations.SetBinding(dg, DataGrid.ForegroundProperty, new Binding())

            dg.FontFamily = new FontFamily("NSimSun");
            dg.FontSize = 13;
            dg.RowHeight = 22.5;

            win1.Close();
        }

        /// <summary>
        /// 根据配置文件节点，获取配置信息
        /// </summary>
        /// <param name="nodeName">配置节点</param>
        /// <returns></returns>
        public static List<string> GetConfigurationByNode(string cfgFile, string nodeName)
        {
            List<string> nodeList = new List<string>();
            string columnpath = "descendant::" + nodeName;
            XmlDocument configfile = new XmlDocument();
            configfile.Load(cfgFile);
            XmlElement root = configfile.DocumentElement;
            if (root != null)
            {
                XmlNodeList columns = root.SelectSingleNode(columnpath).ChildNodes;
                string content;
                foreach (XmlNode x in columns)
                {
                    content = x.Attributes["Title"].Value + "," + x.Attributes["Display"].Value + "," + x.Attributes["Name"].Value;
                    nodeList.Add(content);
                }
            }
            return nodeList;
        }

        /// <summary>
        /// 根据节点保存配置信息
        /// </summary>
        /// <param name="nodeName">节点</param>
        /// <param name="nodeList">配置信息</param>
        public static void SaveConfigurationByNode(string cfgFile, string nodeName, List<string> nodeList)
        {
            if (string.IsNullOrEmpty(nodeName))
            {
                throw new ArgumentException("保存配置文件的节点不能为空");
            }

            string columnpath = "descendant::" + nodeName;
            XmlDocument configfile = new XmlDocument();
            configfile.Load(cfgFile);
            XmlElement root = configfile.DocumentElement;
            XmlNode desXmlNode = root.SelectSingleNode(columnpath);
            if (desXmlNode != null)
            {
                root.RemoveChild(desXmlNode);
            }
            XmlElement xmlNode = configfile.CreateElement(nodeName);
            int nCount = nodeList.Count;
            if (nCount > 0)
            {
                string[] contents;
                foreach (var item in nodeList)
                {
                    XmlElement xmlItem = configfile.CreateElement("item");
                    contents = item.Split(',');
                    //xmlItem.InnerText = item;
                    xmlItem.SetAttribute("Title", contents[0]);
                    xmlItem.SetAttribute("Display", contents[1]);
                    xmlItem.SetAttribute("Name", contents[2]);
                    xmlNode.AppendChild(xmlItem);
                }
            }
            root.AppendChild(xmlNode);
            configfile.Save(cfgFile);
        }

        /// <summary>
        /// 法定节假日（非周末的）
        /// </summary>
        public static string Holidays;
        private static Logger pLog;

        /// <summary>
        /// 是否为节假日
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static bool IsWorkDay(DateTime date)
        {
            if (Holidays.Contains(date.ToString("yyyyMMdd")))
            {
                return false;
            }
            //返回true表示属于工作日
            return (int)date.DayOfWeek > 0 && (int)date.DayOfWeek < 6;
        }

        /// <summary>
        /// 获取枚举的所有描述值
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static List<string> GetEnumAllDescription(Type type)
        {
            var enumDespStrList = new List<string>();
            var enumDesp = EnumDescription.GetFieldTexts(type);
            if (enumDesp == null || !enumDesp.Any())
            {
                return enumDespStrList;
            }
            enumDespStrList.AddRange(enumDesp.Select(item => item.EnumDisplayText));
            return enumDespStrList;
        }
        /// <summary>
        /// 发送纯文本电子邮件
        /// </summary>
        /// <param name="mailFrom">发件人</param>
        /// <param name="password">发件人密码</param>
        /// <param name="mailTo">收件人</param>
        /// <param name="subject">主题</param>
        /// <param name="content">正文</param>
        public static void SendMailByPlainFormat(string mailFrom, string password, string mailTo, string subject, string content)
        {
            MailMessage mailMsg = new MailMessage(mailFrom, mailTo, subject, content);
            mailMsg.BodyEncoding = Encoding.UTF8;
            mailMsg.BodyTransferEncoding = System.Net.Mime.TransferEncoding.SevenBit;
            mailMsg.IsBodyHtml = false;
            mailMsg.Priority = MailPriority.High;

            int nIndex = mailFrom.IndexOf("@");
            string host = mailFrom.Substring(nIndex + 1);
            string userName = mailFrom.Substring(0, nIndex);
            host = "smtp." + host;
            SmtpClient smtpClient = new SmtpClient();
            smtpClient.Host = host;
            smtpClient.Port = 25;
            smtpClient.EnableSsl = false;
            smtpClient.Credentials = new System.Net.NetworkCredential(userName, password);
            //smtpClient.Send(mailMsg);
            smtpClient.Dispose();
            mailMsg.Dispose();
        }
        /// <summary>
        /// 发送交易通知到邮箱
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="content"></param>
        public static void SendNotice(string subject, string content)
        {
            string mailFrom = "TickQuantSupport@163.com";
            string mailPwd = "Tao2708";
            SendMailByPlainFormat(mailFrom, mailPwd, Trader.Configuration.Mail.UserName, subject, content);
            //SendMailByPlainFormat(Trader.Configuration.Mail.UserName,Trader.Configuration.Mail.Password,mailFrom, subject, content);
        }
        public static string Serialize<T>(T obj)
        {
            try
            {
                MemoryStream Stream = new MemoryStream();
                XmlSerializer xml = new XmlSerializer(typeof(T));
                //System.Runtime.Serialization.Formatters.Binary.BinaryFormatter b = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                try
                {
                    xml.Serialize(Stream, obj);
                    //b.Serialize(Stream, obj);
                }
                catch (InvalidOperationException)
                {
                    throw;
                }
                Stream.Position = 0;
                StreamReader sr = new StreamReader(Stream);
                string str = sr.ReadToEnd();

                sr.Dispose();
                Stream.Dispose();

                return str;
            }
            catch (Exception e)
            {
                WriteMemFile(e.ToString());
            }
            return null;
        }
        public static T Deserialize<T>(string xml)
        {
            try
            {
                using (StringReader sr = new StringReader(xml))
                {
                    XmlSerializer xmldes = new XmlSerializer(typeof(T));
                    return (T)xmldes.Deserialize(sr);
                }
            }
            catch (Exception) { }

            return default(T);
        }

        public static string GetProductID(string instrumentID)
        {
            string productID = string.Empty;
            if (!string.IsNullOrEmpty(instrumentID))
            {
                Regex reg = new Regex(@"\d");
                int intIndex = reg.Match(instrumentID).Index;
                productID = instrumentID.Substring(0, intIndex);
            }
            return productID;
        }
        /// <summary>
        /// Get string of this_week, next_week, quarter from one of three dates
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string GetContractType(string date)
        {
            DateTime now = DateTime.Now;
            string sthis_week = "", snext_week = "";
            DayOfWeek week = now.DayOfWeek;
            int w;
            if ((w = (int)week) < 5)
            {
                sthis_week = now.AddDays(5 - w).ToString("MMdd");
                snext_week = now.AddDays(12 - w).ToString("MMdd");
            }
            else if ((w = (int)week) >= 5)
            {
                sthis_week = now.AddDays(12 - w).ToString("MMdd");
                snext_week = now.AddDays(19 - w).ToString("MMdd");
            }
            if (date == sthis_week) return "this_week";
            else if (date == snext_week) return "next_week";
            else return "quarter";
        }
        /*public static string GetDateFromContractType(string contracttype)
        {

        }*/
        public static string GetCurrWeek()
        {
            DateTime now = DateTime.UtcNow;
            int weekday = (int)now.DayOfWeek;
            if (((int)weekday < 5) || (weekday == 5 && now.Hour < 8))
                return now.AddDays(5 - weekday).ToString("yyMMdd");
            else
                if (weekday == 5)
                return now.AddDays(7).ToString("yyMMdd");
            else
                return now.AddDays(6).ToString("yyMMdd");
        }
        public static string GetNextWeek()
        {
            DateTime now = DateTime.UtcNow;
            var weekday = now.DayOfWeek;
            if (((int)weekday < 5) || ((int)weekday == 5 && now.Hour < 8))
                return now.AddDays(12 - (int)weekday).ToString("yyMMdd");
            else if ((int)weekday==5) return now.AddDays(14).ToString("yyMMdd");
            else return now.AddDays(13).ToString("yyMMdd");
        }      
        public static string GetCurrentQuarter()
        {
            string LastFridayOfThisQuarter;
            DateTime now = DateTime.UtcNow;
            if (now.Hour >= 8)
            {
                now.AddDays(1);
            }
            int Y = now.Year;
            int M = now.Month;
            int D = 1;
            int H = now.Hour;
            if (M <= 3)
            {
                M = 4;
            }
            else if (M <= 6)
            {
                M = 7;
            }
            else if (M <= 9)
            {
                M = 10;
            }
            else //if (M <= 12)
            {
                M = 1;
            }
            DateTime dateTime = new DateTime(Y, M, D, 0, 0, 0).AddMilliseconds(-1);
            if (M == 1)
                dateTime.AddYears(1);
            while (true)
            {
                if ((int)dateTime.DayOfWeek != 5)
                {
                    dateTime = dateTime.AddDays(-1);
                }
                else
                {
                    dateTime.AddHours(-16);
                    LastFridayOfThisQuarter = dateTime.ToString("yyMMdd");
                    if (LastFridayOfThisQuarter != GetCurrWeek() && LastFridayOfThisQuarter != GetNextWeek()
                        && dateTime > DateTime.UtcNow)
                        return LastFridayOfThisQuarter;
                    else
                    {
                        dateTime = DateTime.UtcNow.AddMonths(4);
                        Y = dateTime.Year;
                        M = dateTime.Month;
                        dateTime = new DateTime(Y, M, D, 0, 0, 0).AddMilliseconds(-1);
                       
                        while (true)
                        {
                            if ((int)dateTime.DayOfWeek != 5)
                            {
                                dateTime = dateTime.AddDays(-1);
                            }
                            else
                                return dateTime.ToString("yyMMdd");
                        }
                    }
                }
            }
        }
        public static string GetBiQuarter()
        {
            var currquarter = GetCurrentQuarter();
            string currentquarter = "20" + currquarter.Substring(0, 2) + "/" + currquarter.Substring(2, 2) + "/" + currquarter.Substring(4, 2);
            var DateofcurrQuarter = DateTime.Parse(currentquarter);
            DateTime biquarter = DateofcurrQuarter.AddMonths(4);
            var Y = biquarter.Year;
            var M = biquarter.Month;            
            var dateTime = new DateTime(Y, M, 1, 0, 0, 0).AddMilliseconds(-1);

            while (true)
            {
                if ((int)dateTime.DayOfWeek != 5)
                {
                    dateTime = dateTime.AddDays(-1);
                }
                else
                    return dateTime.ToString("yyMMdd");
            }          
        }

        public static long ConvertDataTimeLong(DateTime dt)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            TimeSpan toNow = dt.Subtract(dtStart);
            long timeStamp = toNow.Ticks;
            timeStamp = long.Parse(timeStamp.ToString().Substring(0, timeStamp.ToString().Length - 4));
            return timeStamp;
        }

        public static DateTime ConvertLongDateTime(long d)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = long.Parse(d + "0000");
            TimeSpan toNow = new TimeSpan(lTime);
            DateTime dtResult = dtStart.Add(toNow);
            return dtResult;
        }
        public static string ToJson(this object v)
        {
            try
            {
                return JsonConvert.SerializeObject(v);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return "";
            }
        }
       
        public static T FromOkexJson<T>(this JContainer v)
        {
            CheckOkexResult(v);
            try
            {
                if (v.Type == JTokenType.Object)
                {
                    var typeT = typeof(T);
                    while (true)
                    {
                        var typename = typeT.Name;
                        if (typename == "Object")
                        {
                            break;
                        }
                        else if (typename == "Array")
                        {
                            return default(T);
                        }
                        else
                        {
                            typeT = typeT.BaseType;
                        }
                    }
                }
                T t = v.ToObject<T>();
#if Debug
                string injson = v.ToString();
                string outjson = JsonConvert.SerializeObject(t);
                pLog.Info($"injson:{injson}&outjson:{outjson}");
#endif
                return t;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                pLog.Fatal(ex.ToString());
                return default(T);
            }
        }
        public static void CheckOkexResult(JContainer v)
        {
            Console.WriteLine(v.ToJson());
            if (v.Type == JTokenType.Object)
            {
                JToken codeJToken;
                var jv = ((JObject)v);
                if (jv.TryGetValue("code", out codeJToken))
                {                    
                    var message = jv["message"].ToString();
                    var code = jv["code"].ToString();
                    SimpleLogger.Logger.Log("error code: " + code + " " + jv.ToString(), SimpleLogger.LogCategory.Error);
                }
            }
        }
        private static object WriteToLogFileLocker = new object();
        public static void WriteToLogFile(LogData data, string file)
        {
            StreamWriter writer = null;
            string fi = string.Format("{0}/Log/{1}.log", Environment.CurrentDirectory, DateTime.Now.ToString("yyMMdd"));

            try
            {
                lock (WriteToLogFileLocker)
                {
                    using (writer = new StreamWriter(fi, true))
                    {
                        writer.WriteLine("{0}\r\n-------------------------------------\r\n", data.Message);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                if (writer != null) writer.Close();
            }
        }

        public static string ToOkexFuturesInstrumentid(this string instrumentid)
        {
            string returnvalue;
            if (instrumentid.Contains("this"))
                returnvalue = instrumentid.Replace("_this_week", $"-USD-{GetCurrWeek()}").ToUpper();
            else if (instrumentid.Contains("next"))
                returnvalue = instrumentid.Replace("_next_week", $"-USD-{GetNextWeek()}").ToUpper();
            else if (instrumentid.Contains("_quarter"))
                returnvalue = instrumentid.Replace("_quarter", $"-USD-{GetCurrentQuarter()}").ToUpper();
            else 
                returnvalue = instrumentid.Replace("_biquarter", $"-USD-{GetBiQuarter()}").ToUpper();

            return returnvalue;
        }

        public static string ToOkexSwapInstrumentid( this string instrumentid)
        {
            return instrumentid.Replace("_", "-").ToUpper();
        }

        public static void WriteToLog(this Exception ex)
        {
            string exstr = ex is AggregateException ? ((AggregateException)ex).ToString() : ex.ToString();
#if Debug
            Console.WriteLine(exstr);
#endif
            pLog.Fatal(exstr);
        }
        public static string GetClientId()
        {           
            string path = string.Format("{0}/Configurations/clientOId.txt", Environment.CurrentDirectory);
            FileInfo fi = new FileInfo(path);
            long l;
            try
            {
                if (fi.Exists)
                {
                    using (var sw = new StreamReader(path))
                    {
                        string r = sw.ReadToEnd();
                        l = long.Parse(r);
                        l++;
                    }
                    using (var sr = new StreamWriter(path))
                    {
                        sr.Write(l);

                    }
                    return l.ToString();
                }
                else
                {
                    
                    var filewriter = fi.Create();
                    var buff = Encoding.UTF8.GetBytes("1234567");
                    filewriter.Write(buff, 0, buff.Length); //.ToArray())
                    filewriter.Dispose();
                    return "1234567";
                }
            }
            catch 
            {
                var filewriter = fi.Create();
                var buff = Encoding.UTF8.GetBytes("1234567");
                filewriter.Write(buff, 0, buff.Length); //.ToArray())
                filewriter.Dispose();
                return "1234567";
            }
        }
    }
}