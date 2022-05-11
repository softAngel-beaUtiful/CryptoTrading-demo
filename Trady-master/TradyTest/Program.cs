using System;
using System.Collections.Generic;
using Trady.Core;
using Trady.Analysis;
namespace TradyTest
{
    class Program
    {
        static void Main(string[] args)
        {
            int index = 2;
            var closes = new List<decimal> { 11, 214, 433, 3333, 333, 233 };
            /*var p =new Trady.Analysis.Indicator.BollingerBands(closes, 12,2);
            var smaDiff = closes.Sma(30).Diff(index);   // i-th term - (i-1)-th term
            var smaSma = closes.Sma(30).Sma(10, index); // average(n items)
            var smaRDiff = closes.Sma(30).RDiff(index); // (i-th term - (i-1)-th term) / (i-1)-th term * 100
            var smaSd = closes.Sma(30).Sd(10, 2, index);*/
        }
    }
}
