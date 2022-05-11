using System.Security;

namespace ExchangeSharp
{
    public interface IExchangeSecurity
    {
        /// <summary>
        /// Public API key - only needs to be set if you are using private authenticated end points. Please use CryptoUtility.SaveUnprotectedStringsToFile to store your API keys, never store them in plain text!
        /// </summary>
        SecureString PublicApiKey { get; }

        /// <summary>
        /// Private API key - only needs to be set if you are using private authenticated end points. Please use CryptoUtility.SaveUnprotectedStringsToFile to store your API keys, never store them in plain text!
        /// </summary>
        SecureString PrivateApiKey { get; }

        /// <summary>
        /// Pass phrase API key - only needs to be set if you are using private authenticated end points. Please use CryptoUtility.SaveUnprotectedStringsToFile to store your API keys, never store them in plain text!
        /// Most services do not require this, but GDAX is an example of one that does
        /// </summary>
        SecureString Passphrase { get; }
    }
}
