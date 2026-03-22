namespace MagniseTest.Application.Interfaces
{
    public interface IPriceStorage
    {

        void UpdatePrice(string symbol, decimal price, DateTime timestamp);

        (decimal Price, DateTime Timestamp)? GetPrice(string symbol);
    }
}