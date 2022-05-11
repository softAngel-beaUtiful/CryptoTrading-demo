using System;
using System.Windows;
using System.Windows.Controls;


namespace CryptoTrading
{
    /// <summary>
    /// Interaction logic for InvestorServiceWindow.xaml
    /// </summary>
    public partial class InvestorServiceWindow : Window
    {
        private System.Windows.Forms.Integration.WindowsFormsHost host = new System.Windows.Forms.Integration.WindowsFormsHost();

        private System.Windows.Forms.WebBrowser serviceBrowser = new System.Windows.Forms.WebBrowser();

        public InvestorServiceWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            serviceBrowser.Url = new Uri("https://investorservice.cfmmc.com/");  
            host.Child = serviceBrowser;
            //add the browser to the grid.
            host.SetValue(Grid.RowProperty, 0);
            this.winGrid.Children.Add(host);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            serviceBrowser.Dispose();
            host.Dispose();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
