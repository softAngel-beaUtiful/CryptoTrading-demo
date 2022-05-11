using System;
using System.Net;
using System.Threading;
using TickQuant.Network;
using System.Diagnostics;
using TickQuant;

namespace ICMPRequestServer
{
    /// <summary>
    /// Summary description for icmpRequest.
    /// </summary>
    public class PingRequest
    {
        public int periodUpdate = 1000;
        public string host = "127.0.0.1";
        public int weightPacket = 32;
        public string sessionID;
        public int nbrEcho = 8;
        public int timeout = 1000;
        public int timeToLiveMax = 128;        
        public PingRequest(string HOST, Stopwatch stop)
        {
            host = HOST;
            Icmp myIcmp = new Icmp(Dns.GetHostEntry(host).AddressList[0]);
            Console.WriteLine("Pinging " + host + " [" + Dns.GetHostEntry(host).AddressList[0].ToString() + "] with " + weightPacket.ToString() + " bytes of data:");
            Console.WriteLine();
            while (nbrEcho > 0)
            {
                try
                {
                    PingRequestInformation myResult = myIcmp.Ping(timeout, timeToLiveMax, weightPacket, stop);
                    if (myResult.duration.Equals(TimeSpan.MaxValue)) Console.WriteLine("Request timed out.");
                    else
                    {
                        Console.WriteLine("Reply from " + Dns.GetHostEntry(host).AddressList[0].ToString() + ": bytes=" + weightPacket.ToString()
                            + " time=" + (myResult.duration.TotalMilliseconds).ToString() + "ms " + "TTL " + myResult.TTL.ToString());
                        Console.WriteLine("Reply from " + Dns.GetHostEntry(host).AddressList[0].ToString() + ": bytes=" + weightPacket.ToString()
                            + " time=" + (myResult.duration1.TotalMilliseconds).ToString() + "ms " + "TTL " + myResult.TTL.ToString());

                    }
                    if (80 > myResult.duration.TotalMilliseconds)
                    {
                        Thread.Sleep(periodUpdate - (int)myResult.duration.TotalMilliseconds);
                    }
                }
                catch
                {
                    Console.WriteLine("Network error.");
                }
                nbrEcho--;
            }
        }
    }	
}
