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

namespace CryptoTrading
{
    /// <summary>
    /// Interaction logic for ErrorReportWindow.xaml
    /// </summary>
    public partial class ErrorReportWindow : Window
    {
        public ErrorReportWindow()
        {
            InitializeComponent();
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtSubject.Text))
                {
                    SimpleLogger.Logger.Log("请填写标题", SimpleLogger.LogCategory.Info);
                    return;
                }
                if (string.IsNullOrEmpty(txtContent.Text))
                {
                    SimpleLogger.Logger.Log("内容不能为空，请描述具体内容",SimpleLogger.LogCategory.Info);
                    return;
                }
                string mailFrom = string.Empty;
                string pwd = string.Empty;
                if (chkBoxUseCustMail.IsChecked.Value)
                {
                    //使用正则表达式检查邮箱格式。
                    if (IsEmail(txtMailFrom.Text))
                    {
                        mailFrom = txtMailFrom.Text;
                        pwd = txtPwd.Text;
                    }
                    else
                    {
                        SimpleLogger.Logger.Log("邮箱格式不正确，请输入正确的邮箱地址", SimpleLogger.LogCategory.Info);
                        txtMailFrom.Clear();
                        txtMailFrom.Focus();
                        return;
                    }
                }
                else
                {
                    //使用默认的发送邮件的邮箱
                    mailFrom = "TickQuantSupport@163.com";
                    pwd = "Tao2708";
                }
                string mailTo = "TickQuantService@163.com";
                string content = string.Format("{0}\r\n\t来自{1}的{2}", txtContent.Text, Trader.DefaultBroker, Trader.Configuration.Investor.ID);
                Utility.SendMailByPlainFormat(mailFrom,pwd,mailTo, txtSubject.Text,content);
                Close();
            }
            catch (Exception ex)
            {
                SimpleLogger.Logger.Log(ex.Message, SimpleLogger.LogCategory.Info);
            }
        }

        private void btnCancle_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// 判断输入的字符串是否是一个合法的Email地址
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public  bool IsEmail(string input)
        {
            return true;
            //todo:huangrongyu
            //string pattern = @"^\\s*([A-Za-z0-9_-]+(\\.\\w+)*@(\\w+\\.)+\\w{2,5})\\s*$";
                
            //return System.Text.RegularExpressions.Regex.IsMatch(pattern, input);
            
        }
    }
}
