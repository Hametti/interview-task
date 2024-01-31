namespace CurrencyExchange.Server.API.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException() { }
        public NotFoundException(string? message) : base(message) { }
    }
}
