using System;
using System.Diagnostics;

namespace TickQuant.Common
{
    public class Profiler : IDisposable
    {
        private readonly Stopwatch ProfilerStopwatch;

        public Profiler()
        {
            this.ProfilerStopwatch = new Stopwatch();
            this.ProfilerStopwatch.Start();
        }

        public void Dispose()
        {
        }

        public string GetResult(string title)
        {
            this.ProfilerStopwatch.Stop();           
            return $"{title}耗时:  {this.ProfilerStopwatch.ElapsedMilliseconds}ms(毫秒) 高精度:{this.ProfilerStopwatch.ElapsedTicks * 1000000F / Stopwatch.Frequency:n3}μs(微秒)";
        }
    }
}
