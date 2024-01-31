namespace CurrencyExchange.Server.API.Models.RequestResponseModels.Currency
{
    public class GetExchangeRatesResponse : BasicResponse
    {
        public List<ExchangeRate> ExchangeRates { get; set; }
    }
}
