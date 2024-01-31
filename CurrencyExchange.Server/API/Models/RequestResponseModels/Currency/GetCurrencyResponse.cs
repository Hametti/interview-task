using CurrencyExchange.Server.Database.Entities.Currency;

namespace CurrencyExchange.Server.API.Models.RequestResponseModels.Currency
{
    public class GetCurrencyResponse : BasicResponse
    {
        public CurrencyModel Currency { get; set; }
    }
}
