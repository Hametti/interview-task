using CurrencyExchange.Server.API.Models;
using CurrencyExchange.Server.Database.Entities.Currency;
using Microsoft.EntityFrameworkCore;

namespace CurrencyExchange.Server.Database.Repositories.CurrencyRepository
{
    public class CurrencyRepository : BaseRepository<CurrencyModel>, ICurrencyRepository
    {
        public CurrencyRepository(CurrencyExchangeDbContext currencyExchangeDbContext) : base(currencyExchangeDbContext) { }

        public async Task<IEnumerable<CurrencyModel>> GetAllCurrencies(DateTime effectiveDate)
        {
            var result = await _currencyExchangeDbContext
                .Currencies
                .Where(currency => currency.EffectiveDate.Date == effectiveDate.Date)
                .ToListAsync();

            return result;
        }

        public async Task<CurrencyModel> GetCurrency(string code, DateTime effectiveDate)
        {
            var result = await _currencyExchangeDbContext.Currencies
                .FirstOrDefaultAsync(currency => currency.Code == code && currency.EffectiveDate.Date == effectiveDate.Date);
            
            return result;
        }

        public async Task AddCurrency(CurrencyModel currency)
        {
            currency.Code = currency.Code.ToUpper();
            _currencyExchangeDbContext.Currencies.Add(currency);
            await _currencyExchangeDbContext.SaveChangesAsync();
        }

        public async Task AddCurrencies(IEnumerable<CurrencyModel> currenciesToAdd)
        {
            foreach (var currency in currenciesToAdd)
                currency.Code = currency.Code.ToUpper();
        
            _currencyExchangeDbContext.AddRange(currenciesToAdd);
            await _currencyExchangeDbContext.SaveChangesAsync();
        }

        public async Task UpdateCurrency(CurrencyModel currencyToUpdate)
        {
            currencyToUpdate.Code = currencyToUpdate.Code.ToUpper();
            var currencyFromDb = await _currencyExchangeDbContext.Currencies
                .FirstOrDefaultAsync(currencyFromDb => currencyFromDb.Code == currencyToUpdate.Code && currencyFromDb.EffectiveDate.Date == currencyToUpdate.EffectiveDate.Date);

            currencyFromDb.CurrencyName = currencyToUpdate.CurrencyName;
            currencyFromDb.Mid = currencyToUpdate.Mid;
            currencyFromDb.Ask = currencyToUpdate.Ask;
            currencyFromDb.Bid = currencyToUpdate.Bid;

            await _currencyExchangeDbContext.SaveChangesAsync();
        }

        public async Task UpdateCurrencies(IEnumerable<CurrencyModel> currenciesToUpdate)
        {
            foreach (var currencyToUpdate in currenciesToUpdate)
            {
                currencyToUpdate.Code.ToUpper();
                var currencyFromDb = _currencyExchangeDbContext.Currencies
                    .FirstOrDefault(currencyFromDb => currencyFromDb.Code == currencyToUpdate.Code && currencyFromDb.EffectiveDate.Date == currencyToUpdate.EffectiveDate.Date);
                
                currencyFromDb.CurrencyName = currencyToUpdate.CurrencyName;
                currencyFromDb.Mid = currencyToUpdate.Mid;
                currencyFromDb.Ask = currencyToUpdate.Ask;
                currencyFromDb.Bid = currencyToUpdate.Bid;
            }

            await _currencyExchangeDbContext.SaveChangesAsync();
        }

        public async Task DeleteCurrency(string code, DateTime effectiveDate)
        {
            var currencyToDelete = _currencyExchangeDbContext.Currencies
                .FirstOrDefault(currencyToDelete => currencyToDelete.Code == code && currencyToDelete.EffectiveDate.Date == effectiveDate.Date);
            
            _currencyExchangeDbContext.Currencies.Remove(currencyToDelete);
            await _currencyExchangeDbContext.SaveChangesAsync();
        }

        public async Task<ChartData> GetChartData(string currencyCode)
        {
            currencyCode = currencyCode.ToUpper();
            var startDate = DateTime.Now.AddDays(-14);
            var endDate = DateTime.Now;

            var currenciesFromLast14Days = await _currencyExchangeDbContext
                .Currencies
                .Where(currency => currency.Code == currencyCode)
                .Where(currency => currency.EffectiveDate.Date >= startDate.Date)
                .Where(currency => currency.EffectiveDate.Date <= endDate.Date)
                .OrderBy(currency => currency.EffectiveDate)
                .ToListAsync();

            var dateToPrice = new List<KeyValuePair<string, decimal>>();
            foreach(var currency in currenciesFromLast14Days)
            {
                string dateKey = currency.EffectiveDate.ToString("d MMM");
                dateToPrice.Add(new KeyValuePair<string, decimal>(dateKey, currency.Mid.Value));
            }

            ChartData result = new ChartData
            {
                CurrencyCode = currencyCode,
                DateToPrice = dateToPrice
            };

            return result;
        }

        public async Task<IEnumerable<string>> GetAvailableCurrencyCodes()
        {
            var result = await _currencyExchangeDbContext.Currencies
                .Select(currency => currency.Code)
                .Distinct()
                .ToListAsync();

            return result;
        }

        public async Task<List<ExchangeRate>> GetExchangeRates()
        {
            var currentDate = DateTime.Now.Date;
            List<ExchangeRate> currencies = await _currencyExchangeDbContext.Currencies
                .Where(currency => currency.EffectiveDate == currentDate)
                .Select(currency => new ExchangeRate
                {
                    CurrencyName = currency.CurrencyName,
                    CurrencyCode = currency.Code,
                    CurrencyMidValue = currency.Mid.Value,
                    CurrencyBidValue = currency.Bid.Value,
                    CurrencyAskValue = currency.Ask.Value,
                })
                .ToListAsync();

            return currencies;
        }
    }
}
