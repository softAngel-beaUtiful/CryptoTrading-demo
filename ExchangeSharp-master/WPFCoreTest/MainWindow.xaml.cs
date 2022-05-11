using Discord;
using ExchangeSharp;
using ExchangeSharp.BinanceGroup;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WPFCoreTest
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		Dictionary<string, IExchangeAPI> apis = new Dictionary<string, IExchangeAPI>();
		//ObservableCollection<string> apiNames = new ObservableCollection<string>();
		List<KeyValuePair<string, IWebSocket>> dictWebSocket = new List<KeyValuePair<string, IWebSocket>>();
		string BinanceListenKey { get; set; }
		string LOGFile;
		public MainWindow()
		{
			InitializeComponent();
			MyInitialization();
		}
		private void MyInitialization()
		{			
			Task.Run(() =>
			{
				
			});
		}
		private void Button_Click_1(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrWhiteSpace(MyCombo.Text) || string.IsNullOrWhiteSpace(symbol.Text))
			{
				return;
			}
			string sym = symbol.Text;
			string exch = MyCombo.Text;
			var api = apis.Values.Where(x => x.Name == MyCombo.SelectedItem.ToString()).Last();
			Task.Run(async () =>
			{
				try
				{
					var ticker = await api.GetOrderBookAsync(sym); //GetTickersAsync();
					StringBuilder b = new StringBuilder();
					b.AppendFormat("{0} \r\n", api.Name);
					//foreach (var ticker in tickers)
					{
						b.AppendFormat("{0,-12} {1} {2} {3}\r\n", ticker.LastUpdatedUtc, exch, ticker.MarketSymbol, ticker.Asks.First().Value.Price + "    " + ticker.Bids.First().Value.Price);
					}
					await Dispatcher.BeginInvoke(new Action(() => MyTextBox.Text = b.ToString()));
				}
				catch (Exception ex)
				{
					await Dispatcher.BeginInvoke(new Action(() => MyTextBox.Text = ex.ToString()));
				}
			});

		}

		private void SubTicker_Click(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrWhiteSpace(MyCombo.Text) || string.IsNullOrWhiteSpace(symbol.Text))
			{
				return;
			}
			string sym = symbol.Text;
			string exch = MyCombo.Text;
			var api = apis.Values.Where(x => x.Name == MyCombo.SelectedItem.ToString()).Last();
			Task.Run(async () =>
			{
				try
				{
					if (dictWebSocket.FindIndex(x => x.Key == exch) > -1)
					{
						//dictWebSocket.TryRemove(exch, out IWebSocket web1);
						//web1.Dispose();

					}
					dictWebSocket.Add(new KeyValuePair<string, IWebSocket>(exch, await api.GetTickersWebSocketAsync(
						new Action<IReadOnlyCollection<KeyValuePair<string, ExchangeTicker>>>(s => { DisplayTickerAsync(s, exch); }), new string[] { sym })));
				}
				catch (Exception ex)
				{
					await Dispatcher.BeginInvoke(new Action(() => MyTextBox.Text = ex.ToString()));
				}
			});
		}

		private async Task DisplayTickerAsync(IReadOnlyCollection<KeyValuePair<string, ExchangeTicker>> f, string exchangeid)
		{
			foreach (var l in f)
			{
				string i = exchangeid + "  " + l.Key + "    " + l.Value.ToString() + Environment.NewLine;
				await Dispatcher.BeginInvoke(new Action(() => MyTextBox.Text = i + MyTextBox.Text));
			}
		}

		private void Button_Click_CLEAR(object sender, RoutedEventArgs e)
		{
			Dispatcher.Invoke(new Action(() => MyTextBox.Text = null));
		}

		private void SubOrderBook_Click(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrWhiteSpace(MyCombo.Text) || string.IsNullOrWhiteSpace(symbol.Text))
			{
				return;
			}
			string sym = symbol.Text;
			string exch = MyCombo.Text;
			var api = apis.Values.ToList().FindLast(x => x.Name == MyCombo.SelectedItem.ToString());
			Task.Run(async () =>
			{
				try
				{
					if (dictWebSocket.FindIndex(x => x.Key == exch) > -1)
					{
						//dictWebSocket.TryRemove(exch, out IWebSocket web1);
						//web1.Dispose();

					}

					dictWebSocket.Add(new KeyValuePair<string, IWebSocket>(exch, await api.GetDeltaOrderBookWebSocketAsync(//GetFullOrderBookWebSocketAsync(
						new Action<ExchangeOrderBook>(s => { DisplayOrderBookAsync(s, exch); }), 20, new string[] { sym })));
				}
				catch (Exception ex)
				{
					await Dispatcher.BeginInvoke(new Action(() => MyTextBox.Text = ex.ToString()));
				}
				finally
				{
					//Invoke(new Action(() => this.UseWaitCursor = false));
				}
			});
		}
		private void DisplayOrderBookAsync(ExchangeOrderBook s, string exch)
		{
			try
			{
				string d = s.MarketSymbol + "  " + exch + "   Depth  " + "ASK0: " +
					   (s.Asks.Values.Count > 0 ? s.Asks.Keys.First().ToStringInvariant() + " " + s.Asks.Values.First().Amount : "")
					   + "    BIDS0: " + (s.Bids.Keys.Count > 0 ? s.Bids.Keys.First().ToString() + " " + s.Bids.Values.First().Amount : "") + Environment.NewLine;
				Dispatcher.BeginInvoke(new Action(() =>
				{
					MyTextBox.Text = d + MyTextBox.Text;
				}));
			}
#pragma warning disable CS0168 // 声明了变量“e”，但从未使用过
			catch (Exception e)
#pragma warning restore CS0168 // 声明了变量“e”，但从未使用过
			{ }
		}

		async void Click_Amounts(object sender, RoutedEventArgs e)
		{
			apis.TryGetValue(MyCombo.SelectedItem.ToString(), out IExchangeAPI exchangeAPI);
			if (exchangeAPI.Name == "OKEx")
			{
				var l = await exchangeAPI.GetAmountsAsync();
				foreach (var m in l)
				{
					Dispatcher.BeginInvoke(new Action(() =>
					{
						MyTextBox.Text +=m.Key.ToString()+ " "+ m.Value.ToString()+ Environment.NewLine;
					}));
				}

			}
		}

		private void click_auth(object sender, RoutedEventArgs e)
		{
			var file = System.IO.File.ReadAllText("../../UserAuth.json");
			var token = Newtonsoft.Json.Linq.JToken.Parse(file);
			var pub = token[MyCombo.SelectedItem.ToString()]["Public"].ToString();
			var priv = token[MyCombo.SelectedItem.ToString()]["Private"].ToString();
			var pass = token[MyCombo.SelectedItem.ToString()]["PassPhrase"].ToString();
			apis.TryGetValue(MyCombo.SelectedItem.ToString(), out IExchangeAPI exchangeAPI);
			exchangeAPI.LoadAPIKeysUnsecure(pub, priv, pass);
		}

		private void DisplayAccountInfo(IEnumerable<KeyValuePair<string, GeneralAccount>> a)
		{
			string c = String.Empty;
			foreach (var b in a)
			{
				c += b.Value.InstrumentID + " ";
				c += b.Value.UsedMargin + " ";
				//c += b.Value.PositionMargin + " ";
				c += b.Value.Available + " ";
				c += b.Value.Balance + " ";
				c += b.Value.ExchangeID + Environment.NewLine;
			}
			Dispatcher.BeginInvoke(new Action(() =>
			{
				MyTextBox.Text = c;
			}));
		}

		private void click_accounts(object sender, RoutedEventArgs e)
		{
			apis.TryGetValue(MyCombo.SelectedItem.ToString(), out IExchangeAPI exchangeAPI);
			Task.Run(async () =>
			{
				var a = await exchangeAPI.GetGeneralAccountsAsync();
				DisplayAccountInfo(a);
			});
		}

		private void click_userdataws(object sender, RoutedEventArgs e)
		{
			//BinanceListenKey
			string exch = MyCombo.Text;
			Task.Run(async () =>
			{
				try
				{
					int index = -1;
					if ((index = dictWebSocket.FindIndex(x => x.Key == exch)) > -1)
					{
						dictWebSocket.RemoveAt(index);
					}
					if (exch == "OKEx")
					{
						//apis.TryGetValue("OKEx", out IExchangeAPI api);
						//dictWebSocket.Add(new KeyValuePair<string, IWebSocket>(exch, await ((ExchangeOKExAPI)api).
					}
					else
					if (exch == "Binance")
					{
						apis.TryGetValue("Binance", out IExchangeAPI binbaseapi);
						BinanceListenKey = await ((ExchangeBinanceAPI)binbaseapi).GetListenKeyAsync();
					}
					else
					{
						apis.TryGetValue(exch, out IExchangeAPI binanceapi);
						BinanceListenKey = await ((BinanceGroupCommon)binanceapi).GetListenKeyAsync();
						dictWebSocket.Add(
							new KeyValuePair<string, IWebSocket>(exch, await ((BinanceGroupCommon)binanceapi).GetUserDataWebSocketAsync(async (x) => await DisplayUserDataAsync(x, exch))));
					}
				}
				catch (Exception ex)
				{
					await Dispatcher.BeginInvoke(new Action(() => MyTextBox.Text = ex.ToString()));
				}
			});
		}
		private async Task DisplayUserDataAsync(object s, string exchangeID)
		{
			string disp = string.Empty;
			if (exchangeID == "Binance")
			{
				if (s is ExchangeSharp.BinanceGroup.Balance balance)
				{
					disp = balance.ToString();
				}
				else
					if (s is ExchangeSharp.BinanceGroup.ExecutionReport executionreport)
				{
					disp = executionreport.ToString();
				}
				else
					if (s is ExchangeSharp.BinanceGroup.ListStatus liststatus)
				{
					disp = liststatus.ToString();
				}
				else
					if (s is ExchangeSharp.BinanceGroup.OutboundAccount outbaoundaccount)
				{
					disp = outbaoundaccount.ToString();
				}
			}
			else
				if (exchangeID == "BinanceCOINFuture" || exchangeID == "BinanceUSDFuture")
			{
				if (s is OrderUpdate orderupdate)
				{
					disp = orderupdate.EventType + " " + orderupdate.order.Symbol + "  " + orderupdate.order.OriginalPrice;
				}
				else
				{
					if (s is AccountUpdate accountupdate)
					{
						foreach (var b in accountupdate.updateData.Balances)
						{
							if (b.CrossWalletBalance > 0)
								disp += b.Asset + "  " + b.CrossWalletBalance + Environment.NewLine;//accountupdate.EventType + " " + accountupdate.updateData.Balances.ToList();
						}
						foreach (var c in accountupdate.updateData.Positions)
						{
							if (c.PositionAmount is null) continue;
							disp += c.PositionAmount + " " + c.PositionSide + " " + c.EntryPrice + " " + c.AccumulatedRealized + Environment.NewLine;
						}
					}
				}
			}
			await Dispatcher.BeginInvoke(new Action(() => MyTextBox.Text = disp + "\n" + MyTextBox.Text));
		}

		private void Click_CompleteOrders(object sender, RoutedEventArgs e)
		{
			apis.TryGetValue(MyCombo.SelectedItem.ToString(), out IExchangeAPI exchangeAPI);
			string sym = this.symbol.Text;
			Task.Run(async () =>
			{
				var orders = await exchangeAPI.GetCompletedOrderDetailsAsync(sym);
				string aa = string.Empty;
				foreach (var o in orders)
				{
					aa +=  " Amount " + o.Amount.ToString() + "  "+ (o.IsBuy==true? "BUY":"SELL")+  " " + o.TradeDate + " AvgPrice " + o.AveragePrice + Environment.NewLine;
				}
				Dispatcher.BeginInvoke(new Action(() => MyTextBox.Text =sym+ Environment.NewLine+aa));
			});
		}

		private void Click_OpenOrders(object sender, RoutedEventArgs e)
		{
			apis.TryGetValue(MyCombo.SelectedItem.ToString(), out IExchangeAPI exchangeAPI);
			string sym = this.symbol.Text;
			Task.Run(async () =>
			{
				var a = await exchangeAPI.GetOpenOrderDetailsAsync(sym);
				string aa = string.Empty;
				foreach (var b in a)
				{ aa = b.Amount.ToString() + "  " + aa; }
				Dispatcher.BeginInvoke(new Action(() => MyTextBox.Text = aa));
			});
		}

		private void Click_OpenPosition(object sender, RoutedEventArgs e)
		{
			apis.TryGetValue(MyCombo.SelectedItem.ToString(), out IExchangeAPI exchangeAPI);
			string sym = this.symbol.Text;
			Task.Run(async () =>
			{
				var a = await exchangeAPI.GetOpenPositionAsync(sym);
				string aa = a.MarketSymbol + "  " + a.BasePrice + "  " + a.ProfitLoss;
				Dispatcher.BeginInvoke(new Action(() => MyTextBox.Text = aa));
			});
		}

		private void TickersButton_Click(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrWhiteSpace(MyCombo.Text))
			{
				return;
			}
			var api = apis.Values.ToArray().Where(x => x.Name == MyCombo.SelectedItem.ToString()).Last();
			Task.Run(async () =>
			{				
				try
				{
					var tickers = await api.GetTickersAsync();
					StringBuilder b = new StringBuilder();
					b.AppendFormat("{0} \r\n", api.Name);
					foreach (var ticker in tickers)
					{
						b.AppendFormat("{0,-12}{1}\r\n", ticker.Key, ticker.Value);
					}
					Dispatcher.BeginInvoke(new Action(() => MyTextBox.Text = b.ToString()));
				}
				catch (Exception ex)
				{
					Dispatcher.BeginInvoke(new Action(() => MyTextBox.Text = ex.ToString()));

				}
				finally
				{
					//Invoke(new Action(() => this.UseWaitCursor = false));
				}
			});
		}		

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			var type = typeof(IExchangeAPI);
			//var exchangeTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).Where(x => x.IsClass
			//&& !x.IsAbstract && type.IsAssignableFrom(x)).OrderBy(x => x.Name).ToList();
			var types = typeof(ExchangeAPI).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(ExchangeAPI)) && !t.IsAbstract).OrderBy(x=>x.Name).ToList();
			try
			{
				foreach (Type type1 in types)
				{
					var api = ExchangeAPI.GetExchangeAPI(type1);
					apis[api.Name] = api;					
				}
				Dispatcher.BeginInvoke(new Action(() =>
				{
					MyCombo.ItemsSource = apis.Keys;
					MyCombo.SelectedItem = 0;
				}));
			}
			catch (Exception ex)
			{ }			
		}

		private void Getcompletedorders(object sender, RoutedEventArgs e)
		{

		}
	}
}
