namespace CurrencyExchange.Server.API.Exceptions
{
    public class UpdateFailureException : Exception
    {
        public UpdateFailureException() { }
        public UpdateFailureException(string? message) : base(message) { }
    }
}
