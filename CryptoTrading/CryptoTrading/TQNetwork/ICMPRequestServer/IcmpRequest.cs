using System;
using System.Net;
using System.Threading;
using TickQuant.Network;
using System.Diagnostics;

namespace ICMPRequestServer
{
	/// <summary>
	/// Summary description for icmpRequest.
	/// </summary>
	public class IcmpRequest
	{
		public IcmpRequest(ToolsCommandRequest myRequest, Stopwatch stop)
		{
			//The request will be a ping
			if (myRequest.myCommandType == CommandType.ping)
			{
				Icmp myIcmp = new Icmp(Dns.GetHostEntry(myRequest.host).AddressList[0]);
				Console.WriteLine("Pinging " + myRequest.host + " [" + Dns.GetHostEntry(myRequest.host).AddressList[0].ToString()+ "] with " + myRequest.weightPacket.ToString() + " bytes of data:");
				Console.WriteLine();
				while(myRequest.nbrEcho > 0) 
				{
					try 
					{
						PingRequestInformation myResult = myIcmp.Ping(myRequest.timeout,myRequest.timeToLiveMax, myRequest.weightPacket, stop);
						if (myResult.duration.Equals(TimeSpan.MaxValue)) Console.WriteLine("Request timed out.");
						else
						{                            
							Console.WriteLine("Reply from " + Dns.GetHostEntry(myRequest.host).AddressList[0].ToString() + ": bytes=" + myRequest.weightPacket.ToString() 
                                + " time=" + (myResult.duration.TotalMilliseconds).ToString() + "ms " + "TTL " + myResult.TTL.ToString());
                            Console.WriteLine("Reply from " + Dns.GetHostEntry(myRequest.host).AddressList[0].ToString() + ": bytes=" + myRequest.weightPacket.ToString()
                                + " time=" + (myResult.duration1.TotalMilliseconds).ToString() + "ms " + "TTL " + myResult.TTL.ToString());

                        }
						if (1000 - myResult.duration.TotalMilliseconds > 0)
						{
							Thread.Sleep(myRequest.periodUpdate - (int)myResult.duration.TotalMilliseconds);
						}
					}
					catch 
					{
						Console.WriteLine("Network error.");
					}
					myRequest.nbrEcho--;
				}
			}
		}		
	}
	public class ToolsCommandRequest
	{
        //Default value
		public int periodUpdate = 1000;
		public CommandType myCommandType = CommandType.tracert;
		public string host = "127.0.0.1";
		public int weightPacket = 32;
		public string sessionID;
		public int nbrEcho = 10;
		public int timeout = 1000;
		public int timeToLiveMax = 128;

		public ToolsCommandRequest(string host,string sessionID,CommandType myCommandType, int nbrEcho, int weightPacket,int periodUpdate, int timeout, int timeToLiveMax)
		{
			if (weightPacket < 1)  weightPacket = 1;
			this.periodUpdate = periodUpdate;
			this.myCommandType = myCommandType;
			this.host = host;
			this.weightPacket = weightPacket;
			this.sessionID = sessionID;
			this.nbrEcho = nbrEcho;
			this.timeout = timeout;
			this.timeToLiveMax = timeToLiveMax;
		}
	}
	public enum CommandType
	{
		ping,
		tracert
	}
}
