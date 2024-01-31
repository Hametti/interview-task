using CurrencyExchange.Server.API.Exceptions;
using CurrencyExchange.Server.API.Extensions;
using CurrencyExchange.Server.API.Mappers;
using CurrencyExchange.Server.API.Models;
using CurrencyExchange.Server.API.Models.NbpApi;
using CurrencyExchange.Server.Database.Entities.Currency;
using CurrencyExchange.Server.Database.Repositories.CurrencyRepository;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace CurrencyExchange.Server.API.Services.Currency
{
    public class CurrencyService : ICurrencyService
    {
        private readonly ICurrencyRepository _currencyRepository;
        private readonly HttpClient _httpClient;

        public CurrencyService(ICurrencyRepository currencyRepository, HttpClient httpClient)
        {
            _currencyRepository = currencyRepository;
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<CurrencyModel>> GetAllCurrencies(DateTime effectiveDate)
        {
            var result = await _currencyRepository.GetAllCurrencies(effectiveDate);
            return result;
        }

        public async Task<CurrencyModel> GetCurrency(string code, DateTime effectiveDate)
        {
            ValidateCurrencyCode(code);

            CurrencyModel result = await _currencyRepository.GetCurrency(code.ToUpper(), effectiveDate);
            
            if (result == null)
                throw new NotFoundException($"Currency with provided code '{code}' doesnt exist in database");

            return result;
        }

        public async Task<CurrencyModel> AddCurrency(CurrencyModel currencyToAdd)
        {
            ValidateCurrencyCode(currencyToAdd.Code);

            if (await CurrencyAlreadyExists(currencyToAdd.Code, currencyToAdd.EffectiveDate))
                throw new AlreadyExistsException($"Currency with provided code '{currencyToAdd.Code}' already exist in database");

            ValidateCurrencyMidValue(currencyToAdd.Code, currencyToAdd.Mid);
            ValidateCurrencyAskBidValues(currencyToAdd.Code, currencyToAdd.Ask, currencyToAdd.Bid);

            await _currencyRepository.AddCurrency(currencyToAdd);

            return currencyToAdd;
        }

        public async Task<IEnumerable<CurrencyModel>> AddCurrencies(IEnumerable<CurrencyModel> currenciesToAdd)
        {
            if (currenciesToAdd == null || !currenciesToAdd.Any())
                throw new InvalidDataException("The list of currencies to add cannot be null or empty.");

            var currenciesToBeAdded = new List<CurrencyModel>();

            foreach (var currency in currenciesToAdd)
            {
                if (await CurrencyAlreadyExists(currency.Code, currency.EffectiveDate))
                    throw new AlreadyExistsException($"Currency with provided code '{currency.Code}' already exists in the database");

                ValidateCurrencyCode(currency.Code);
                ValidateCurrencyMidValue(currency.Code, currency.Mid);
                ValidateCurrencyAskBidValues(currency.Code, currency.Ask, currency.Bid);

                currenciesToBeAdded.Add(currency);
            }

            await _currencyRepository.AddCurrencies(currenciesToBeAdded);

            return currenciesToBeAdded;
        }

        public async Task<CurrencyModel> UpdateCurrency(CurrencyModel currencyToUpdate)
        {
            ValidateCurrencyCode(currencyToUpdate.Code);

            if (!await CurrencyAlreadyExists(currencyToUpdate.Code, currencyToUpdate.EffectiveDate))
                throw new NotFoundException($"Currency with provided code '{currencyToUpdate.Code}' doesnt exist in database");

            ValidateCurrencyCode(currencyToUpdate.Code);
            ValidateCurrencyMidValue(currencyToUpdate.Code, currencyToUpdate.Mid);
            ValidateCurrencyAskBidValues(currencyToUpdate.Code, currencyToUpdate.Ask, currencyToUpdate.Bid);

            await _currencyRepository.UpdateCurrency(currencyToUpdate);

            return currencyToUpdate;
        }

        public async Task<IEnumerable<CurrencyModel>> UpdateCurrencies(IEnumerable<CurrencyModel> currenciesToUpdate)
        {
            if (currenciesToUpdate == null || !currenciesToUpdate.Any())
                throw new InvalidDataException("The list of currencies to update cannot be null or empty.");

            var updatedCurrencies = new List<CurrencyModel>();
            var currenciesToAdd = new List<CurrencyModel>();

            foreach (var currency in currenciesToUpdate)
            {
                if (!await CurrencyAlreadyExists(currency.Code, currency.EffectiveDate))
                    currenciesToAdd.Add(currency);

                ValidateCurrencyCode(currency.Code);
                ValidateCurrencyMidValue(currency.Code, currency.Mid);
                ValidateCurrencyAskBidValues(currency.Code, currency.Ask, currency.Bid);

                updatedCurrencies.Add(currency);
            }

            await _currencyRepository.AddCurrencies(currenciesToAdd);
            await _currencyRepository.UpdateCurrencies(updatedCurrencies);

            return updatedCurrencies;
        }

        public async Task DeleteCurrency(string code, DateTime effectiveDate)
        {
            ValidateCurrencyCode(code);

            if (!await CurrencyAlreadyExists(code, effectiveDate))
                throw new NotFoundException($"Currency with provided code '{code}' with date '{effectiveDate} doesnt exist in database");

            await _currencyRepository.DeleteCurrency(code, effectiveDate);
        }

        public async Task<decimal> ConvertCurrency(string sourceCurrencyCode, string targetCurrencyCode, decimal targetCurrencyAmount)
        {
            ValidateCurrencyCode(sourceCurrencyCode);
            ValidateCurrencyCode(targetCurrencyCode);
            ValidateCurrencyAmount(targetCurrencyAmount);

            var sourceCurrency = await GetCurrency(sourceCurrencyCode, DateTime.Now);
            var targetCurrency = await GetCurrency(targetCurrencyCode, DateTime.Now);

            ValidateCurrencyMidValue(sourceCurrency.Code, sourceCurrency.Mid);
            ValidateCurrencyMidValue(targetCurrency.Code, targetCurrency.Mid);

            var conversionRate = targetCurrency.Mid / sourceCurrency.Mid;
            decimal result = targetCurrencyAmount * conversionRate.Value;

            return result;
        }

        public async Task<ChartData> ChartData(string currencyCode)
        {
            ValidateCurrencyCode(currencyCode);

            var result = await _currencyRepository.GetChartData(currencyCode);
            return result;
        }

        public async Task<IEnumerable<string>> GetAvailableCurrencyCodes()
        {
            IEnumerable<string> result = await _currencyRepository.GetAvailableCurrencyCodes();
            return result;
        }

        public async Task<List<ExchangeRate>> GetExchangeRates()
        {
            var result = await _currencyRepository.GetExchangeRates();
            return result;
        }

        public async Task FetchDataFromNbp()
        {
            const string GetCurrenciesByTableAndDateEndpoint = "http://api.nbp.pl/api/exchangerates/tables/{0}/{1}/{2}/?format=json";

            var midRateCurrencyTable = "A";
            var exchangeRateCurrencyTable = "C";

            var startDate = DateTime.Now.AddDays(-14);
            var endDate = DateTime.Now;

            var midRatesEndpoint = string.Format(GetCurrenciesByTableAndDateEndpoint, midRateCurrencyTable, startDate.Date.ToString("yyyy-MM-dd"), endDate.Date.ToString("yyyy-MM-dd"));
            var exchangeRatesEndpoint = string.Format(GetCurrenciesByTableAndDateEndpoint, exchangeRateCurrencyTable, startDate.Date.ToString("yyyy-MM-dd"), endDate.Date.ToString("yyyy-MM-dd"));

            HttpResponseMessage midRateCurrenciesResponse = await _httpClient.GetAsync(midRatesEndpoint);
            HttpResponseMessage exchangeRateCurrenciesResponse = await _httpClient.GetAsync(exchangeRatesEndpoint);

            var midRates = JsonConvert.DeserializeObject<List<ExchangeRatesTable>>(await midRateCurrenciesResponse.Content.ReadAsStringAsync());
            var exchangeRates = JsonConvert.DeserializeObject<List<ExchangeRatesTable>>(await exchangeRateCurrenciesResponse.Content.ReadAsStringAsync());

            var mappedMidRates = CurrencyMapper.MapExchangeRateTablesToCurrencies(midRates);
            var mappedExchangeRates = CurrencyMapper.MapExchangeRateTablesToCurrencies(exchangeRates);

            List<CurrencyModel> finalCurrencyList = new List<CurrencyModel>();

            foreach (var midRate in mappedMidRates)
            {
                var rateToAdd = mappedExchangeRates.FirstOrDefault(exchangeRate => exchangeRate.Code == midRate.Code && exchangeRate.EffectiveDate == midRate.EffectiveDate);
                if (rateToAdd != null)
                {
                    rateToAdd.Mid = midRate.Mid;
                    finalCurrencyList.Add(rateToAdd);
                }
            }

            await UpdateCurrencies(finalCurrencyList);
        }

        private async Task<bool> CurrencyAlreadyExists(string code, DateTime effectiveDate) => await _currencyRepository.GetCurrency(code, effectiveDate) != null;

        private void ValidateCurrencyAmount(decimal targetCurrencyAmount)
        {
            if (targetCurrencyAmount <= 0)
                throw new InvalidDataException($"Provided amount '{targetCurrencyAmount}' is incorrect");
        }

        private void ValidateCurrencyCode(string code)
        {
            if (code == null || code.Length != 3 || Regex.IsMatch(code, @"\d"))
                throw new InvalidDataException($"Provided currency code '{code}' is invalid");
        }

        private void ValidateCurrencyMidValue(string code, decimal? mid)
        {
            if (mid == null || mid == 0)
                throw new InvalidDataException($"Currency '{code}' has null or zero 'Mid' value.");
        }

        private void ValidateCurrencyAskBidValues(string code, decimal? ask, decimal? bid)
        {
            if (ask.IsNullOrZero() || bid.IsNullOrZero())
                throw new InvalidDataException($"Currency '{code}' has incorrect Ask/Bid value");
        }
    }
}
