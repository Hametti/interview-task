using CurrencyExchange.Server.Database.Entities.Currency;

namespace CurrencyExchange.Server.API.Models.RequestResponseModels.Currency
{
    public class GetAllCurrenciesResponse : BasicResponse
    {
        public List<CurrencyModel> Currencies { get; set; }
    }
}
