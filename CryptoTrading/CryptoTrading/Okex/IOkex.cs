using CryptoTrading.Model;
using System.Threading.Tasks;

namespace CryptoTrading
{
    public interface IOkex
    {
        #region public methods
        Task StartAsync();
       
        //合约交易类        
        bool ReqLogin();       
        bool SendOrderRest(OrderData order);
        bool CancelOrder(string InstrumentID, string orderid);        
      
        //暂时先不做现货交易部分
        //bool ReqSubSpotTicker(SpotSymbol s);
        //bool ReqSubSpotDepth();            
        #endregion                     
    }
}
