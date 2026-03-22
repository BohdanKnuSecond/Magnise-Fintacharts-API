using System.Collections.Concurrent;
using MagniseTest.Application.Interfaces;

namespace MagniseTest.Infrastructure.Services
{
    public class PriceStorage : IPriceStorage
    {
        private readonly ConcurrentDictionary<string, (decimal Price, DateTime Timestamp)> _prices = new();

        public void UpdatePrice(string symbol, decimal price, DateTime timestamp)
        {
            _prices[symbol] = (price, timestamp);
        }

        public (decimal Price, DateTime Timestamp)? GetPrice(string symbol)
        {
            if (_prices.TryGetValue(symbol, out var data))
            {
                return data;
            }
            return null; 
        }
    }
}