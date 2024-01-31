using CurrencyExchange.Server.API.Models;
using CurrencyExchange.Server.Database.Entities.Currency;

namespace CurrencyExchange.Server.API.Services.Currency
{
    public interface ICurrencyService
    {
        Task<IEnumerable<CurrencyModel>> GetAllCurrencies(DateTime effectiveDate);
        Task<CurrencyModel> GetCurrency(string code, DateTime effectiveDate);
        Task<CurrencyModel> AddCurrency(CurrencyModel currency);
        Task<IEnumerable<CurrencyModel>> AddCurrencies(IEnumerable<CurrencyModel> currencies);
        Task<CurrencyModel> UpdateCurrency(CurrencyModel currencyToUpdate);
        Task<IEnumerable<CurrencyModel>> UpdateCurrencies(IEnumerable<CurrencyModel> currencyToUpdate);
        Task DeleteCurrency(string code, DateTime effectiveDate);
        Task<decimal> ConvertCurrency(string sourceCurrencyCode, string targetCurrencyCode, decimal targetCurrencyAmount);
        Task<ChartData> ChartData(string currencyCode);
        Task<IEnumerable<string>> GetAvailableCurrencyCodes();
        Task<List<ExchangeRate>> GetExchangeRates();
        Task FetchDataFromNbp();
    }
}
