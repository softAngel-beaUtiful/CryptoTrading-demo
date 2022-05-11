using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OKExSDK
{
    public class WebSocketor : IDisposable
    {
        string url = "wss://real.okex.com:8443/ws/v3";
        //string url = "wss://ws.okx.com:8443/ws/v5/public";
        ClientWebSocket ws = null;
        CancellationTokenSource cts = new CancellationTokenSource();
        public event WebSocketPushHandler WebSocketPush;
        public event EventHandler<string> OnError;
        public delegate void WebSocketPushHandler(string message);
        private bool isLogin = false;
        private string apiKey;
        private string secret;
        private string phrase;
        private decimal delaytime = 1;
        private List<string> channels = new List<string>();
        private ConcurrentQueue<PendingChannel> pendingChannels = new ConcurrentQueue<PendingChannel>();
        private System.Timers.Timer pendingTimer;
        private System.Timers.Timer PingTimer;
        private System.Timers.Timer closeCheckTimer = new System.Timers.Timer();

        private int retryNum = 0;
        public int retryLimit { get; set; } = 200000;

        public WebSocketor()
        {            
            ws = new ClientWebSocket();
            
            closeCheckTimer.Interval = 10000;
            closeCheckTimer.Elapsed += async (s, e) =>
            {
                await rebootAsync();
            };
            PingTimer = new System.Timers.Timer();
            PingTimer.Interval = 25000;
            PingTimer.Elapsed += PingTimer_Elapsed;
        }

        private void PingTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            CancellationToken ct = new CancellationToken();
            if (ws.State == WebSocketState.Open)
                ws.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes("ping")), WebSocketMessageType.Text, true, ct);
            else
                if (ws.State==WebSocketState.Aborted || ws.State==WebSocketState.Closed || ws.State== WebSocketState.None)
                rebootAsync().Wait();
        }

        public async Task ConnectAsync()
        {            
            try
            {       await ws.ConnectAsync(new Uri(url), cts.Token);
                    closeCheckTimer.Interval = 10000;
                    closeCheckTimer.Start();
                    receive();                
            }
            catch (Exception ex)
            {
                Console.WriteLine("error in connection: " + ex.Message);
            }
        }

        public async Task LoginAsync(string apiKey, string secret, string phrase)
        {
            this.apiKey = apiKey;
            this.secret = secret;
            this.phrase = phrase;
            isLogin = true;
            if (ws.State == WebSocketState.Open)
            {
                var sign = Encryptor.MakeSign(apiKey, secret, phrase);
                byte[] buff = Encoding.UTF8.GetBytes(sign);
                await ws.SendAsync(new ArraySegment<byte>(buff), WebSocketMessageType.Text, true, CancellationToken.None);
                closeCheckTimer.Interval = 31000;
            }
            else if (ws.State == WebSocketState.CloseReceived || ws.State == WebSocketState.Closed || ws.State == WebSocketState.Aborted ||ws.State==WebSocketState.None)
            {
                await rebootAsync();
            }
        }
        private static object Locker = new object();
        public async Task Subscribe(List<string> args)
        {
            if (ws.State == WebSocketState.Open)
            {
                channels.RemoveAll((x) => true);
                args.ForEach(channel =>
                {
                    channels.Add(channel);
                });
                var message = new
                {
                    op = "subscribe",
                    args
                };
                var messageStr = JsonConvert.SerializeObject(message);
                byte[] buffer = Encoding.UTF8.GetBytes(messageStr);
                lock (Locker)
                {
                    ws.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None).Wait();
                }
                closeCheckTimer.Interval = 31000;
            }
            else if (ws.State == WebSocketState.CloseReceived || ws.State == WebSocketState.Closed || ws.State == WebSocketState.Aborted)
            {
                args.ForEach(channel =>
                {
                    channels.Add(channel);
                });
                await rebootAsync();
            }
            else
            {
                pendingChannels.Enqueue(new PendingChannel { action = "subscribe", args = args });
                setPendingTimer();
            }
        }
        public async Task UnSubscribe(List<string> args)
        {
            foreach (var channel in args)
            {
                channels.Remove(channel);
            }
            if (ws.State == WebSocketState.Open)
            {
                var message = new
                {
                    op = "unsubscribe",
                    args
                };
                var messageStr = JsonConvert.SerializeObject(message);
                byte[] buffer = Encoding.UTF8.GetBytes(messageStr);
                await ws.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
                closeCheckTimer.Interval = 31000;
            }
            else if (ws.State == WebSocketState.CloseReceived || ws.State == WebSocketState.Closed || ws.State == WebSocketState.Aborted)
            {
                await rebootAsync();
            }
            else
            {
                pendingChannels.Enqueue(new PendingChannel { action = "unsubscribe", args = args });
                setPendingTimer();
            }
        }

        public void Dispose()
        {
            if (!cts.Token.CanBeCanceled)
            {
                cts.Cancel();
            }
            if (ws != null)
            {
                ws.Dispose();
                ws = null;
            }
            channels = null;
            pendingChannels = null;
            if (pendingTimer != null)
            {
                pendingTimer.Stop();
                pendingTimer.Dispose();
            }
            if (closeCheckTimer != null)
            {
                closeCheckTimer.Stop();
                closeCheckTimer.Dispose();
            }
        }

        private void receive()
        {
            Task.Factory.StartNew(
              async () =>
              {
                  while (ws.State == WebSocketState.Open)
                  {
                      byte[] buffer = new byte[1024];
                      WebSocketReceiveResult result= new WebSocketReceiveResult(0, WebSocketMessageType.Text, true);
                      try
                      {
                           result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                          PingTimer.Stop();
                          PingTimer.Start();
                      }
                      catch (Exception ex)
                      {
                          OnError?.Invoke(this, ex.Message);
                          continue;
                      }
                      if (result.MessageType == WebSocketMessageType.Binary)
                      {
                          closeCheckTimer.Interval = 31000;
                          string resultStr = string.Empty;
                          try
                          {
                              resultStr = Decompress(buffer);
                          }
                          catch (Exception ex)
                          {
                              Console.WriteLine("error in Decompress: " + ex.Message);
                              Console.WriteLine("error received message: " + resultStr);
                              continue;
                          }
                          if (resultStr is null || resultStr == string.Empty)
                              continue;
                          WebSocketPush?.Invoke(resultStr);
                          continue;
                      }                      

                      if (result.MessageType == WebSocketMessageType.Close)
                      {
                          try
                          {
                              await ws.CloseOutputAsync(WebSocketCloseStatus.Empty, null, cts.Token);
                          }
                          catch (Exception)
                          {
                              break;
                          }
                          break;
                      }
                  }
              }, cts.Token, TaskCreationOptions.LongRunning,
                 TaskScheduler.Default);
        }

        private string Decompress(byte[] baseBytes)
        {
            using (var decompressedStream = new MemoryStream())
            using (var compressedStream = new MemoryStream(baseBytes))
            using (var deflateStream = new DeflateStream(compressedStream, CompressionMode.Decompress))
            {
                try
                {
                    deflateStream.CopyTo(decompressedStream);
                    decompressedStream.Position = 0;
                    using (var streamReader = new StreamReader(decompressedStream))
                    {
                        return streamReader.ReadToEnd();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("error " + ex.Message);
                    return null;
                }
            }
        }
        private void setPendingTimer()
        {
            if (pendingTimer == null)
            {
                pendingTimer = new System.Timers.Timer(20000);
                pendingTimer.Elapsed += async (s, e) => { await retryPending(); };
                pendingTimer.Start();
            }
            else
            {
                pendingTimer.Start();
            }
        }
        private async Task retryPending()
        {
            while (pendingChannels.Count > 0)
            {
                retryNum++;
                if (retryNum > retryLimit)
                {
                    break;
                }
                PendingChannel channel;

                if (pendingChannels.TryDequeue(out channel))
                {
                    switch (channel.action)
                    {
                        case "subscribe":
                            await Subscribe(channel.args);
                            break;
                        case "unsubscribe":
                            await UnSubscribe(channel.args);
                            break;
                    }
                }
            }
            retryNum = 0;
            pendingTimer.Stop();
        }

        public async Task rebootAsync()
        {
            try
            {
                if (ws is null)
                {
                    ws = new ClientWebSocket();
                    if (pendingTimer != null)
                    {
                        pendingTimer.Stop();
                    }
                    await ConnectAsync();
                }
                if (! (ws.State == WebSocketState.Aborted || ws.State == WebSocketState.Closed))
                {
                    await ws.CloseOutputAsync(WebSocketCloseStatus.Empty, null, cts.Token);
                }
                if (cts.Token.CanBeCanceled)
                {
                    cts.Cancel();
                    cts = new CancellationTokenSource();
                }
                ws.Dispose();
                ws = null;
                ws = new ClientWebSocket();
                if (pendingTimer != null)
                {
                    pendingTimer.Stop();
                }
                //delaytime *= 1.05m;
                await Task.Delay((int)delaytime*1000);
                await ConnectAsync();
                if (isLogin)
                {
                    await LoginAsync(apiKey, secret, phrase);
                    await Task.Delay(500);//等待登录
                }
                if (channels.Count > 0)
                {
                    if (channels.Count > 129)
                    Console.WriteLine("channels count: " + channels.Count);    
                    await Subscribe(channels.ToList<string>());
                }
                setPendingTimer();
            }
            catch (Exception ex)
            { }
        }

        private class PendingChannel
        {
            public string action { get; set; }
            public List<string> args { get; set; }
        }
    }
}
