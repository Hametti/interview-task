using CurrencyExchange.Server.API.Exceptions;
using CurrencyExchange.Server.API.Services.Currency;
using CurrencyExchange.Server.Database.Entities.Currency;
using CurrencyExchange.Server.Database.Repositories.CurrencyRepository;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace CurrencyExchange.Test.API.Services
{
    internal class CurrencyServiceTests
    {
        private ICurrencyRepository _mockCurrencyRepository;
        private CurrencyService _currencyService;
        private HttpClient _httpClient;

        [SetUp]
        public void Setup()
        {
            _httpClient = Substitute.For<HttpClient>();
            _mockCurrencyRepository = Substitute.For<ICurrencyRepository>();
            _currencyService = new CurrencyService(_mockCurrencyRepository, _httpClient);
        }

        [Test]
        public async Task GetCurrency_WhenCurrencyExists_ReturnsCurrencyModel()
        {
            var currencyCode = "USD";
            var expectedCurrency = new CurrencyModel { Code = "USD", CurrencyName = "US Dollar" };
            _mockCurrencyRepository.GetCurrency(currencyCode, Arg.Any<DateTime>()).Returns(Task.FromResult(expectedCurrency));
            var result = await _currencyService.GetCurrency(currencyCode, DateTime.Now.Date);

            Assert.IsNotNull(result);
            Assert.That(result, Is.EqualTo(expectedCurrency));
        }

        [Test]
        public void GetCurrency_WhenCurrencyDoesNotExist_ThrowsNotFoundException()
        {
            var currencyCode = "XYZ";
            _mockCurrencyRepository.GetCurrency(currencyCode, DateTime.Now.Date).Returns(Task.FromResult<CurrencyModel>(null));

            Assert.ThrowsAsync<NotFoundException>(async () => await _currencyService.GetCurrency(currencyCode, DateTime.Now.Date));
        }

        [Test]
        public void GetCurrency_WhenExceptionOccurs_ThrowsException()
        {
            var currencyCode = "USD";
            _mockCurrencyRepository.GetCurrency(currencyCode, DateTime.Now.Date).Throws(new NotFoundException($"Currency with provided code '{currencyCode}' doesnt exist in database"));

            Assert.ThrowsAsync<NotFoundException>(async () => await _currencyService.GetCurrency(currencyCode, DateTime.Now.Date));
        }

        [Test]
        public async Task GetAllCurrencies_WhenCurrenciesExist_ReturnsListOfCurrencies()
        {
            var mockCurrencies = new List<CurrencyModel>
            {
                new CurrencyModel { Code = "USD", CurrencyName = "US Dollar" },
                new CurrencyModel { Code = "EUR", CurrencyName = "Euro" }
            };
            _mockCurrencyRepository.GetAllCurrencies(DateTime.Now.Date).Returns(Task.FromResult(mockCurrencies.AsEnumerable()));
            var result = await _currencyService.GetAllCurrencies(DateTime.Now.Date);

            Assert.IsNotNull(result);
            Assert.That(result.Count(), Is.EqualTo(2));
            Assert.That(result, Is.EquivalentTo(mockCurrencies));
        }

        [Test]
        public async Task GetAllCurrencies_WhenNoCurrenciesExist_ReturnsEmptyList()
        {
            _mockCurrencyRepository.GetAllCurrencies(DateTime.Now.Date).Returns(Task.FromResult(Enumerable.Empty<CurrencyModel>()));
            var result = await _currencyService.GetAllCurrencies(DateTime.Now.Date);

            Assert.IsNotNull(result);
            Assert.That(result.Count(), Is.EqualTo(0));
        }

        [Test]
        public void GetAllCurrencies_WhenExceptionOccurs_ThrowsException()
        {
            _mockCurrencyRepository.GetAllCurrencies(DateTime.Now.Date).Throws(new Exception("Database error"));

            Assert.ThrowsAsync<Exception>(async () => await _currencyService.GetAllCurrencies(DateTime.Now.Date));
        }

        [Test]
        public async Task AddCurrency_WhenNewCurrency_AddsCurrencySuccessfully()
        {
            var newCurrency = new CurrencyModel { Code = "JPY", CurrencyName = "Japanese Yen", Mid = 1.0m, Ask = 1.1m, Bid = 0.9m };
            _mockCurrencyRepository.AddCurrency(Arg.Is<CurrencyModel>(c => c.Code == newCurrency.Code && c.CurrencyName == newCurrency.CurrencyName)).Returns(Task.FromResult(newCurrency));
            var result = await _currencyService.AddCurrency(newCurrency);

            Assert.IsNotNull(result);
            Assert.That(result.Code, Is.EqualTo(newCurrency.Code));
            Assert.That(result.CurrencyName, Is.EqualTo(newCurrency.CurrencyName));
        }

        [Test]
        public void AddCurrency_WhenCurrencyAlreadyExists_ThrowsAlreadyExistsException()
        {
            var existingCurrency = new CurrencyModel { Code = "USD", CurrencyName = "US Dollar", Mid = 1.0m, Ask = 1.1m, Bid = 0.9m };
            _mockCurrencyRepository.When(r => r.AddCurrency(Arg.Any<CurrencyModel>())).Do(x => { throw new AlreadyExistsException("Currency already exists"); });

            var ex = Assert.ThrowsAsync<AlreadyExistsException>(async () => await _currencyService.AddCurrency(existingCurrency));
            Assert.That(ex.Message, Is.EqualTo("Currency already exists"));
        }

        [Test]
        public void AddCurrency_WhenExceptionOccurs_ThrowsException()
        {
            var newCurrency = new CurrencyModel { Code = "CAD", CurrencyName = "Canadian Dollar", Mid = 1.0m, Ask = 1.1m, Bid = 0.9m };
            _mockCurrencyRepository.When(r => r.AddCurrency(Arg.Any<CurrencyModel>())).Do(x => { throw new Exception("Database error"); });

            Assert.ThrowsAsync<Exception>(async () => await _currencyService.AddCurrency(newCurrency));
        }

        [Test]
        public async Task AddCurrencies_WhenNewCurrencies_AddsCurrenciesSuccessfully()
        {
            var currenciesToAdd = new List<CurrencyModel>
            {
                new CurrencyModel { Code = "JPY", CurrencyName = "Japanese Yen", Mid = 1.0m, Ask = 1.1m, Bid = 0.9m },
                new CurrencyModel { Code = "GBP", CurrencyName = "British Pound", Mid = 1.0m, Ask = 1.1m, Bid = 0.9m }
            };
            _mockCurrencyRepository.AddCurrencies(currenciesToAdd).Returns(Task.FromResult(currenciesToAdd));
            var result = await _currencyService.AddCurrencies(currenciesToAdd);

            Assert.IsNotNull(result);
            Assert.That(result.Count(), Is.EqualTo(2));
            CollectionAssert.AreEquivalent(currenciesToAdd.Select(c => c.Code), result.Select(r => r.Code));
        }

        [Test]
        public void AddCurrencies_WhenOneOrMoreCurrenciesExist_ThrowsAlreadyExistsException()
        {
            var currenciesToAdd = new List<CurrencyModel>
            {
                new CurrencyModel { Code = "EUR", CurrencyName = "Euro", Mid = 1.0m, Ask = 1.1m, Bid = 0.9m },
                new CurrencyModel { Code = "USD", CurrencyName = "US Dollar", Mid = 1.0m, Ask = 1.1m, Bid = 0.9m }
            };
            _mockCurrencyRepository.When(r => r.AddCurrencies(Arg.Any<IEnumerable<CurrencyModel>>()))
                .Do(x => { throw new AlreadyExistsException("One or more currencies already exist"); });

            var ex = Assert.ThrowsAsync<AlreadyExistsException>(async () => await _currencyService.AddCurrencies(currenciesToAdd));
            Assert.That(ex.Message, Is.EqualTo("One or more currencies already exist"));
        }

        [Test]
        public void AddCurrencies_WhenExceptionOccurs_ThrowsException()
        {
            var currenciesToAdd = new List<CurrencyModel>
            {
                new CurrencyModel { Code = "AUD", CurrencyName = "Australian Dollar", Mid = 1.0m, Ask = 1.1m, Bid = 0.9m },
                new CurrencyModel { Code = "NZD", CurrencyName = "New Zealand Dollar", Mid = 1.0m, Ask = 1.1m, Bid = 0.9m }
            };
            _mockCurrencyRepository.When(r => r.AddCurrencies(Arg.Any<IEnumerable<CurrencyModel>>()))
                .Do(x => { throw new Exception("Database error"); });

            Assert.ThrowsAsync<Exception>(async () => await _currencyService.AddCurrencies(currenciesToAdd));
        }

        [Test]
        public async Task UpdateCurrency_WhenCurrencyExists_UpdatesCurrencySuccessfully()
        {
            var currencyToUpdate = new CurrencyModel { Code = "EUR", CurrencyName = "Euro", Mid = 1.1M, Ask = 1.2M, Bid = 1.0M };
            _mockCurrencyRepository.GetCurrency(Arg.Any<string>(), Arg.Any<DateTime>()).Returns(Task.FromResult(currencyToUpdate));
            _mockCurrencyRepository.UpdateCurrency(currencyToUpdate).Returns(Task.FromResult(currencyToUpdate));
            var result = await _currencyService.UpdateCurrency(currencyToUpdate);

            Assert.IsNotNull(result);
            Assert.That(result.Code, Is.EqualTo("EUR"));
            Assert.That(result.Mid, Is.EqualTo(1.1M));
            Assert.That(result.Ask, Is.EqualTo(1.2M));
            Assert.That(result.Bid, Is.EqualTo(1.0M));
        }

        [Test]
        public void UpdateCurrency_WhenCurrencyDoesNotExist_ThrowsNotFoundException()
        {
            var currencyToUpdate = new CurrencyModel { Code = "XYZ", CurrencyName = "This currency doesnt exist", Mid = 2.1M, Ask = 2.2M, Bid = 2.0M };
            _mockCurrencyRepository.When(r => r.UpdateCurrency(Arg.Any<CurrencyModel>()))
                .Do(x => { throw new NotFoundException("Currency with provided code 'XYZ' doesnt exist in database"); });

            var ex = Assert.ThrowsAsync<NotFoundException>(async () => await _currencyService.UpdateCurrency(currencyToUpdate));
            Assert.That(ex.Message, Is.EqualTo("Currency with provided code 'XYZ' doesnt exist in database"));
        }

        [Test]
        public void UpdateCurrency_WhenExceptionOccurs_ThrowsException()
        {
            var currencyToUpdate = new CurrencyModel { Code = "CAD", CurrencyName = "Canadian Dollar", Mid = 1.5M, Ask = 1.6M, Bid = 1.4M };
            _mockCurrencyRepository.GetCurrency(Arg.Any<string>(), Arg.Any<DateTime>()).Returns(Task.FromResult(currencyToUpdate));
            _mockCurrencyRepository.When(r => r.UpdateCurrency(Arg.Any<CurrencyModel>()))
                .Do(x => { throw new Exception("Database error"); });

            Assert.ThrowsAsync<Exception>(async () => await _currencyService.UpdateCurrency(currencyToUpdate));
        }

        [Test]
        public async Task UpdateCurrencies_WhenAllCurrenciesExist_UpdatesCurrenciesSuccessfully()
        {
            var currenciesToUpdate = new List<CurrencyModel>
            {
                new CurrencyModel { Code = "EUR", CurrencyName = "Euro", Mid = 1.1M, Ask = 1.2M, Bid = 1.0M },
                new CurrencyModel { Code = "USD", CurrencyName = "US Dollar", Mid = 1.3M, Ask = 1.4M, Bid = 1.2M }
            };
            _mockCurrencyRepository.GetCurrency(currenciesToUpdate.First().Code, Arg.Any<DateTime>()).Returns(Task.FromResult(currenciesToUpdate.First()));
            _mockCurrencyRepository.GetCurrency(currenciesToUpdate.Last().Code, Arg.Any<DateTime>()).Returns(Task.FromResult(currenciesToUpdate.Last()));
            _mockCurrencyRepository.UpdateCurrencies(currenciesToUpdate).Returns(Task.FromResult(currenciesToUpdate));

            var result = await _currencyService.UpdateCurrencies(currenciesToUpdate);

            Assert.IsNotNull(result);
            Assert.That(result.Count(), Is.EqualTo(2));
            Assert.IsTrue(result.Any(c => c.Code == "EUR" && c.Mid == 1.1M));
            Assert.IsTrue(result.Any(c => c.Code == "USD" && c.Mid == 1.3M));
        }

        [Test]
        public void UpdateCurrencies_WhenSomeCurrenciesDoNotExist_ThrowsNotFoundException()
        {
            var currenciesToUpdate = new List<CurrencyModel>
            {
                new CurrencyModel { Code = "EUR", CurrencyName = "Euro", Mid = 1.1M, Ask = 1.2M, Bid = 1.0M },
                new CurrencyModel { Code = "XYZ", CurrencyName = "This currency doesnt exist", Mid = 2.1M, Ask = 2.2M, Bid = 2.0M }
            };
            _mockCurrencyRepository.GetCurrency(currenciesToUpdate.First().Code, Arg.Any<DateTime>()).Returns(Task.FromResult(currenciesToUpdate.First()));
            _mockCurrencyRepository.GetCurrency(currenciesToUpdate.Last().Code, Arg.Any<DateTime>()).Returns(Task.FromResult(currenciesToUpdate.Last()));
            _mockCurrencyRepository.UpdateCurrencies(currenciesToUpdate).Returns(Task.FromResult(currenciesToUpdate));
            _mockCurrencyRepository.When(r => r.UpdateCurrencies(Arg.Any<List<CurrencyModel>>()))
                .Do(x => { throw new NotFoundException("One or more currencies not found"); });

            var ex = Assert.ThrowsAsync<NotFoundException>(async () => await _currencyService.UpdateCurrencies(currenciesToUpdate));
            Assert.That(ex.Message, Is.EqualTo("One or more currencies not found"));
        }

        [Test]
        public void UpdateCurrencies_WhenExceptionOccurs_ThrowsException()
        {
            var currenciesToUpdate = new List<CurrencyModel>
            {
                new CurrencyModel { Code = "CAD", CurrencyName = "Canadian Dollar", Mid = 1.5M, Ask = 1.6M, Bid = 1.4M },
                new CurrencyModel { Code = "AUD", CurrencyName = "Australian Dollar", Mid = 1.7M, Ask = 1.8M, Bid = 1.6M }
            };
            _mockCurrencyRepository.GetCurrency(currenciesToUpdate.First().Code, Arg.Any<DateTime>()).Returns(Task.FromResult(currenciesToUpdate.First()));
            _mockCurrencyRepository.GetCurrency(currenciesToUpdate.Last().Code, Arg.Any<DateTime>()).Returns(Task.FromResult(currenciesToUpdate.Last()));
            _mockCurrencyRepository.UpdateCurrencies(currenciesToUpdate).Returns(Task.FromResult(currenciesToUpdate));
            _mockCurrencyRepository.When(r => r.UpdateCurrencies(Arg.Any<List<CurrencyModel>>()))
                .Do(x => { throw new Exception("Database error"); });

            Assert.ThrowsAsync<Exception>(async () => await _currencyService.UpdateCurrencies(currenciesToUpdate));
        }

        [Test]
        public async Task DeleteCurrency_WhenCurrencyExists_DeletesCurrency()
        {
            var currencyCode = "USD";
            _mockCurrencyRepository.GetCurrency(currencyCode, DateTime.Now.Date).Returns(Task.FromResult(new CurrencyModel { Code = currencyCode }));
            await _currencyService.DeleteCurrency(currencyCode, DateTime.Now.Date);

            await _mockCurrencyRepository.Received(1).DeleteCurrency(currencyCode, DateTime.Now.Date);
        }

        [Test]
        public void DeleteCurrency_WhenCurrencyDoesNotExist_ThrowsNotFoundException()
        {
            var currencyCode = "XYZ";
            _mockCurrencyRepository.GetCurrency(currencyCode, DateTime.Now.Date).Returns(Task.FromResult<CurrencyModel>(null));

            Assert.ThrowsAsync<NotFoundException>(async () => await _currencyService.DeleteCurrency(currencyCode, DateTime.Now.Date));
        }

        [Test]
        public void DeleteCurrency_WhenInvalidCurrencyCode_ThrowsInvalidDataException()
        {
            var invalidCurrencyCode = "123";

            Assert.ThrowsAsync<InvalidDataException>(async () => await _currencyService.DeleteCurrency(invalidCurrencyCode, DateTime.Now.Date));
        }

        [Test]
        public async Task ConvertCurrency_WhenValidCurrenciesAndAmount_ReturnsConvertedAmount()
        {
            var sourceCurrencyCode = "USD";
            var targetCurrencyCode = "EUR";
            var amount = 100m;
            var sourceCurrency = new CurrencyModel { Code = sourceCurrencyCode, Mid = 1 };
            var targetCurrency = new CurrencyModel { Code = targetCurrencyCode, Mid = 0.85m };
            _mockCurrencyRepository.GetCurrency(sourceCurrencyCode, Arg.Any<DateTime>()).Returns(Task.FromResult(sourceCurrency));
            _mockCurrencyRepository.GetCurrency(targetCurrencyCode, Arg.Any<DateTime>()).Returns(Task.FromResult(targetCurrency));
            var result = await _currencyService.ConvertCurrency(sourceCurrencyCode, targetCurrencyCode, amount);

            var expectedConversion = (targetCurrency.Mid / sourceCurrency.Mid) * amount;
            Assert.That(result, Is.EqualTo(expectedConversion));
        }

        [Test]
        public void ConvertCurrency_WhenInvalidSourceCurrencyCode_ThrowsInvalidDataException()
        {
            var invalidCurrencyCode = "123";
            var targetCurrencyCode = "EUR";
            var amount = 100m;

            Assert.ThrowsAsync<InvalidDataException>(async () => await _currencyService.ConvertCurrency(invalidCurrencyCode, targetCurrencyCode, amount));
        }

        [Test]
        public void ConvertCurrency_WhenInvalidTargetCurrencyCode_ThrowsInvalidDataException()
        {
            var sourceCurrencyCode = "USD";
            var invalidCurrencyCode = "123";
            var amount = 100m;
            _mockCurrencyRepository.GetCurrency(sourceCurrencyCode, DateTime.Now.Date).Returns(Task.FromResult(new CurrencyModel() { Code = sourceCurrencyCode, Mid = 1.0m, Ask = 1.1m, Bid = 0.9m}));

            Assert.ThrowsAsync<InvalidDataException>(async () => await _currencyService.ConvertCurrency(sourceCurrencyCode, invalidCurrencyCode, amount));
        }

        [Test]
        public void ConvertCurrency_WhenInvalidAmount_ThrowsInvalidDataException()
        {
            var sourceCurrencyCode = "USD";
            var targetCurrencyCode = "EUR";
            var invalidAmount = -100m;

            Assert.ThrowsAsync<InvalidDataException>(async () => await _currencyService.ConvertCurrency(sourceCurrencyCode, targetCurrencyCode, invalidAmount));
        }

        [Test]
        public void ConvertCurrency_WhenSourceCurrencyNotFound_ThrowsNotFoundException()
        {
            var nonExistentCurrencyCode = "ABC";
            var targetCurrencyCode = "EUR";
            var amount = 100m;
            _mockCurrencyRepository.GetCurrency(nonExistentCurrencyCode, DateTime.Now.Date).Returns(Task.FromResult<CurrencyModel>(null));

            Assert.ThrowsAsync<NotFoundException>(async () => await _currencyService.ConvertCurrency(nonExistentCurrencyCode, targetCurrencyCode, amount));
        }

        [Test]
        public void ConvertCurrency_WhenTargetCurrencyNotFound_ThrowsNotFoundException()
        {
            var sourceCurrencyCode = "USD";
            var nonExistentCurrencyCode = "XYZ";
            var amount = 100m;
            _mockCurrencyRepository.GetCurrency(nonExistentCurrencyCode, DateTime.Now.Date).Returns(Task.FromResult<CurrencyModel>(null));

            Assert.ThrowsAsync<NotFoundException>(async () => await _currencyService.ConvertCurrency(sourceCurrencyCode, nonExistentCurrencyCode, amount));
        }
    }
}
