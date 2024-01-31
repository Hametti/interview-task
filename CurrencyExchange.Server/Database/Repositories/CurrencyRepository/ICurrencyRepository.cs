using CurrencyExchange.Server.API.Models;
using CurrencyExchange.Server.Database.Entities.Currency;

namespace CurrencyExchange.Server.Database.Repositories.CurrencyRepository
{
    public interface ICurrencyRepository : IBaseRepository<CurrencyModel>
    {
        Task<IEnumerable<CurrencyModel>> GetAllCurrencies(DateTime effectiveDate);
        Task<CurrencyModel> GetCurrency(string code, DateTime effectiveDate);
        Task AddCurrency(CurrencyModel currencyToAdd);
        Task AddCurrencies(IEnumerable<CurrencyModel> currenciesToAdd);
        Task UpdateCurrency(CurrencyModel currencyToUpdate);
        Task UpdateCurrencies(IEnumerable<CurrencyModel> currenciesToUpdate);
        Task DeleteCurrency(string code, DateTime effectiveDate);
        Task<ChartData> GetChartData(string currencyCode);
        Task<IEnumerable<string>> GetAvailableCurrencyCodes();
        Task<List<ExchangeRate>> GetExchangeRates();
    }
}
