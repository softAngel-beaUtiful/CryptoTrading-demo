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
    /// Interaction logic for NoticeWindow.xaml
    /// </summary>
    public partial class NoticeWindow : Window
    {
        private System.Windows.Forms.Integration.WindowsFormsHost host = new System.Windows.Forms.Integration.WindowsFormsHost();

        public System.Windows.Forms.WebBrowser noticeBrowser = new System.Windows.Forms.WebBrowser();


        public NoticeWindow(string noticeType)
        {
            InitializeComponent();
            if (noticeType == "期商通知")
            {
                Title = "期商通知信息";
            }
            else if (noticeType == "交易通知")
            {
                Title = "交易通知信息";
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TQMain.T.noticeWin = this;
            noticeBrowser.DocumentText = string.Empty;
            host.Child = noticeBrowser;
            //add the browser to the grid.
            host.SetValue(Grid.RowProperty, 0);
            this.winGrid.Children.Add(host);
            //string notice = string.Empty;//需要通过CTP查
            //System.IO.StreamReader sr = new System.IO.StreamReader(notice);
            //System.Xml.XmlReader xmlReader = System.Xml.XmlReader.Create(sr);
            //richTxtNotice.Document = (FlowDocument)System.Windows.Markup.XamlReader.Load(xmlReader);

            //richTxtNotice.Document = new FlowDocument(new Paragraph(new Run(notice))); 

        }

        private void Window_Closed(object sender, EventArgs e)
        {
            noticeBrowser.Dispose();
            host.Dispose();
        }
    }
}
