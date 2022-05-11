using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using ExchangeSharp;

namespace ExchangeSharpWinForms
{
    public partial class MainForm : Form
    {
		List<IExchangeAPI> apis = new List<IExchangeAPI>();
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());

        }

        private async void FetchTickers()
        {
            if (!Created || string.IsNullOrWhiteSpace(cmbExchange.SelectedItem as string))
            {
                return;
            }

            this.UseWaitCursor = true;
            try
            {
				var api = apis.FindLast(x => x.Name == cmbExchange.SelectedItem as string);
                var tickers = await api.GetTickersAsync();
                StringBuilder b = new StringBuilder();
                foreach (var ticker in tickers)
                {
                    b.AppendFormat("{0,-12}{1}\r\n", ticker.Key, ticker.Value);
                }
                textTickersResult.Text = b.ToString();
            }
            catch (Exception ex)
            {
                textTickersResult.Text = ex.ToString();
            }
            finally
            {
                Invoke(new Action(() => this.UseWaitCursor = false));
            }
        }

        public MainForm()
        {
            InitializeComponent();
			//List<IExchangeAPI> apiList = new List<IExchangeAPI>();
			//Task.Run(async() =>
			InitializeAPIs();			
		}

		private async void InitializeAPIs()
		{
			var type = typeof(IExchangeAPI);
			var exchangeTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).Where(x => x.IsClass
			&& !x.IsAbstract && type.IsAssignableFrom(x)).OrderBy(x => x.Name).ToList();
			var types = typeof(ExchangeAPI).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(ExchangeAPI)) && !t.IsAbstract).ToList();
			try
			{

				foreach (Type type1 in exchangeTypes)
				{
					if (!type1.FullName.Contains("Kraken"))
					{
						var api = ExchangeAPI.GetExchangeAPI(type1);
						apis.Add(api);
					}
				}
			}
			catch (Exception e)
			{ }
		}

		protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);						
            foreach (var exchange in this.apis)
            {
                cmbExchange.Items.Add(exchange.Name);
            }
            //cmbExchange.SelectedIndex = 0;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            FetchTickers();
        }

        private void cmbExchange_SelectedIndexChanged(object sender, EventArgs e)
        {
            FetchTickers();
        }
    }
}
