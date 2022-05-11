using CryptoTrading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoTrading.BitMex
{
    public partial class  BitMexBase
    {
        async Task Receive(ArraySegment<byte> ar)
        {
            byte[] buffer = new byte[1024];
            ArraySegment<byte> arrbytes = new ArraySegment<byte>(buffer);
            while (clientWebSocket.State == WebSocketState.Open)
            {
                var result = await clientWebSocket.ReceiveAsync(arrbytes, CancellationToken.None);
                if (result.MessageType != WebSocketMessageType.Close)
                {
                    await clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                }
                else
                {
                    //LogStatus(true, result., result.Count);
                }
            }
        }
        async Task StartReceivingAsync()
        {
            string msg;
            byte[] buffer = new byte[2 * 1024];
            try
            {
                while (clientWebSocket.State == WebSocketState.Open)
                {
                    msg = "";
                    var result = await clientWebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        if (!result.EndOfMessage)
                        {
                            int totalLength = 0;
                            while (!result.EndOfMessage)
                            {
                                var st = Encoding.UTF8.GetString(buffer, 0, result.Count);
                                msg += st;
                                totalLength += result.Count;
                                result = await clientWebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                            }
                            msg += Encoding.UTF8.GetString(buffer, 0, result.Count);
                        }
                        else
                        {
                            msg = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        }

                        if (msg.Contains("pong"))
                        {
                            continue;
                        }
                        else if (msg.Contains("success"))
                        {
                            continue;
                        }
                        OnMessage?.Invoke(null, msg);
                    }
                }
                Thread.Sleep(1);
            }
            catch (Exception e)
            {
                CryptoTrading.Utility.WriteMemFile(e.Message);
            }
        }

    }
}
