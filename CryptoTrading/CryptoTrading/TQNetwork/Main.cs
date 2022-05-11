using System;
using System.Diagnostics;

namespace ICMPRequestServer
{
    
    class ICMPRequestServer
    {
        /// <summary>
        /// Demo of the icmpRequest utility.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            //test ping
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            Console.WriteLine("**********************TEST PING**********************\r\n");
            PingRequest myRequest = new PingRequest("www.baidu.com",stopWatch);
            			
            Console.ReadLine();
        }
    }
}
