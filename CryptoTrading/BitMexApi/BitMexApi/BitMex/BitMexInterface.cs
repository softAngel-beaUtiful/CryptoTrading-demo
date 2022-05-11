using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoTrading.BitMex
{
    public interface BitMexInterface
    {
        Task Ping();
        Task SubscribeQuote(string[] InstrumentIDs);

        Task UnsubscribeQuote(string[] InstrumentIDs);
    }
}
