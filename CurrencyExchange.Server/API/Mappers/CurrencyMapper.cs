using CurrencyExchange.Server.API.Models.NbpApi;
using CurrencyExchange.Server.Database.Entities.Currency;

namespace CurrencyExchange.Server.API.Mappers
{
    public static class CurrencyMapper
    {
        public static List<CurrencyModel> MapExchangeRateTablesToCurrencies(List<ExchangeRatesTable> exchangeRatesTable)
        {
            var currencies = new List<CurrencyModel>();

            foreach (var exchangeRate in exchangeRatesTable)
                foreach (var rate in exchangeRate.Rates)
                {
                    var currency = new CurrencyModel()
                    {
                        CurrencyName = rate.Currency,
                        Code = rate.Code,
                        Mid = rate.Mid,
                        Bid = rate.Bid,
                        Ask = rate.Ask,
                        EffectiveDate = exchangeRate.EffectiveDate,
                    };

                    currencies.Add(currency);
                }

            return currencies;
        }
    }
}
