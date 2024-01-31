namespace CurrencyExchange.Server.API.Extensions
{
    public static class DecimalExtensions
    {
        public static bool IsNullOrZero(this decimal? value) => (value ?? 0) == 0;
    }
}
