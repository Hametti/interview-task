using CurrencyExchange.Server.API.Controllers.Currency;
using CurrencyExchange.Server.API.Exceptions;
using CurrencyExchange.Server.API.Models.RequestResponseModels.Currency;
using CurrencyExchange.Server.API.Services.Currency;
using CurrencyExchange.Server.Database.Entities.Currency;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace CurrencyExchange.Test.API.Controllers
{
    [TestFixture]
    public class CurrencyControllerTests
    {
        private ICurrencyService _mockCurrencyService;
        private CurrencyController _controller;
        private HttpClient _httpClient;

        [SetUp]
        public void Setup()
        {
            _httpClient = Substitute.For<HttpClient>();
            _mockCurrencyService = Substitute.For<ICurrencyService>();
            _controller = new CurrencyController(_mockCurrencyService, _httpClient);
        }

        [Test]
        public async Task GetCurrency_WhenCurrencyExists_ReturnsOkObjectResult()
        {
            var currencyCode = "USD";
            var mockCurrency = new CurrencyModel { Code = "USD", CurrencyName = "US Dollar" };
            _mockCurrencyService.GetCurrency(currencyCode, Arg.Any<DateTime>()).Returns(Task.FromResult(mockCurrency));
            var result = await _controller.GetCurrency(currencyCode);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(mockCurrency, Is.EqualTo((okResult.Value as GetCurrencyResponse).Currency));
        }

        [Test]
        public async Task GetCurrency_WhenCurrencyDoesNotExist_ReturnsNotFoundResult()
        {
            var currencyCode = "XYZ";
            _mockCurrencyService.GetCurrency(currencyCode, Arg.Any<DateTime>()).Throws(new NotFoundException("Currency not found"));
            var result = await _controller.GetCurrency(currencyCode);

            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        [TestCase("ASD1")]
        [TestCase("1234")]
        [TestCase("111")]
        public async Task GetCurrency_WhenCurrencyCodeIsInvalidFormat_ReturnsBadRequestResult(string invalidCurrencyCode)
        {
            _mockCurrencyService.GetCurrency(invalidCurrencyCode, Arg.Any<DateTime>()).Throws(new InvalidDataException("Invalid currency code format"));
            var result = await _controller.GetCurrency(invalidCurrencyCode);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task GetCurrency_WhenUnexpectedExceptionOccurs_ReturnsInternalServerErrorResult()
        {
            var currencyCode = "EUR";
            _mockCurrencyService.GetCurrency(currencyCode, Arg.Any<DateTime>()).Throws(new Exception("Unexpected error"));
            var result = await _controller.GetCurrency(currencyCode);

            Assert.That(result, Is.InstanceOf<ObjectResult>());
            var objectResult = result as ObjectResult;
            Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
        }

        [Test]
        public async Task GetAllCurrencies_WhenCurrenciesExist_ReturnsOkObjectResultWithCurrencies()
        {
            var mockCurrencies = new List<CurrencyModel>
            {
                new CurrencyModel { Code = "USD", CurrencyName = "US Dollar" },
                new CurrencyModel { Code = "EUR", CurrencyName = "Euro" }
            };
            _mockCurrencyService.GetAllCurrencies(Arg.Any<DateTime>()).Returns(Task.FromResult<IEnumerable<CurrencyModel>>(mockCurrencies));
            var result = await _controller.GetAllCurrencies();

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(mockCurrencies, Is.EqualTo((okResult.Value as GetAllCurrenciesResponse).Currencies));
        }

        [Test]
        public async Task GetAllCurrencies_WhenAnExceptionOccurs_ReturnsInternalServerError()
        {
            _mockCurrencyService.GetAllCurrencies(Arg.Any<DateTime>()).Throws(new Exception());
            var result = await _controller.GetAllCurrencies();

            Assert.That(result, Is.InstanceOf<ObjectResult>());
            var objectResult = result as ObjectResult;
            Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
        }

        [Test]
        public async Task AddCurrency_WhenCurrencyIsSuccessfullyAdded_ReturnsCreatedAtActionResult()
        {
            var newCurrency = new CurrencyModel { Code = "CAD", CurrencyName = "Canadian Dollar" };
            _mockCurrencyService.AddCurrency(newCurrency).Returns(Task.FromResult(newCurrency));
            var result = await _controller.AddCurrency(newCurrency);

            Assert.That(result, Is.InstanceOf<CreatedAtActionResult>());
            var createdAtActionResult = result as CreatedAtActionResult;
            Assert.That(newCurrency, Is.EqualTo(createdAtActionResult.Value as CurrencyModel));
        }

        [Test]
        public async Task AddCurrency_WhenCurrencyAlreadyExists_ReturnsConflictObjectResult()
        {
            var existingCurrency = new CurrencyModel { Code = "USD", CurrencyName = "US Dollar" };
            _mockCurrencyService.AddCurrency(existingCurrency).Throws(new AlreadyExistsException("Currency already exists"));
            var result = await _controller.AddCurrency(existingCurrency);

            Assert.That(result, Is.InstanceOf<ConflictObjectResult>());
        }

        [Test]
        public async Task AddCurrency_WhenInvalidDataIsProvided_ReturnsBadRequestObjectResult()
        {
            var invalidCurrency = new CurrencyModel { Code = "123", CurrencyName = "Invalid Currency" };
            _mockCurrencyService.AddCurrency(invalidCurrency).Throws(new InvalidDataException("Invalid currency data"));
            var result = await _controller.AddCurrency(invalidCurrency);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task AddCurrency_WhenExceptionOccurs_ReturnsInternalServerError()
        {
            var currency = new CurrencyModel { Code = "NZD", CurrencyName = "New Zealand Dollar" };
            _mockCurrencyService.AddCurrency(currency).Throws(new Exception());
            var result = await _controller.AddCurrency(currency);

            Assert.That(result, Is.InstanceOf<ObjectResult>());
            var objectResult = result as ObjectResult;
            Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
        }

        [Test]
        public async Task AddCurrencies_WhenCurrenciesAreSuccessfullyAdded_ReturnsCreatedAtActionResult()
        {
            var currenciesToAdd = new List<CurrencyModel>
            {
                new CurrencyModel { Code = "EUR", CurrencyName = "Euro" },
                new CurrencyModel { Code = "GBP", CurrencyName = "British Pound" }
            };
            _mockCurrencyService.AddCurrencies(currenciesToAdd).Returns(Task.FromResult(currenciesToAdd.AsEnumerable()));
            var result = await _controller.AddCurrencies(currenciesToAdd);

            Assert.That(result, Is.InstanceOf<CreatedAtActionResult>());
            var createdAtActionResult = result as CreatedAtActionResult;
            Assert.That(currenciesToAdd, Is.EqualTo(createdAtActionResult.Value as IEnumerable<CurrencyModel>));
        }

        [Test]
        public async Task AddCurrencies_WhenOneOrMoreCurrenciesAlreadyExist_ReturnsConflictObjectResult()
        {
            var existingCurrencies = new List<CurrencyModel>
            {
                new CurrencyModel { Code = "USD", CurrencyName = "US Dollar" },
                new CurrencyModel { Code = "JPY", CurrencyName = "Japanese Yen" }
            };
            _mockCurrencyService.AddCurrencies(existingCurrencies).Throws(new AlreadyExistsException("One or more currencies already exist"));
            var result = await _controller.AddCurrencies(existingCurrencies);

            Assert.That(result, Is.InstanceOf<ConflictObjectResult>());
        }

        [Test]
        public async Task AddCurrencies_WhenInvalidDataIsProvided_ReturnsBadRequestObjectResult()
        {
            var invalidCurrencies = new List<CurrencyModel>
            {
                new CurrencyModel { Code = "123", CurrencyName = "Invalid Currency" },
                new CurrencyModel { Code = "???", CurrencyName = "Unknown Currency" }
            };
            _mockCurrencyService.AddCurrencies(invalidCurrencies).Throws(new InvalidDataException("Invalid currency data"));
            var result = await _controller.AddCurrencies(invalidCurrencies);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task AddCurrencies_WhenExceptionOccurs_ReturnsInternalServerError()
        {
            var currencies = new List<CurrencyModel>
            {
                new CurrencyModel { Code = "AUD", CurrencyName = "Australian Dollar" },
                new CurrencyModel { Code = "CAD", CurrencyName = "Canadian Dollar" }
            };
            _mockCurrencyService.AddCurrencies(currencies).Throws(new Exception());
            var result = await _controller.AddCurrencies(currencies);

            Assert.That(result, Is.InstanceOf<ObjectResult>());
            var objectResult = result as ObjectResult;
            Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
        }

        [Test]
        public async Task UpdateCurrency_WhenCurrencyIsSuccessfullyUpdated_ReturnsOkObjectResult()
        {
            var currencyToUpdate = new CurrencyModel { Code = "EUR", CurrencyName = "Euro", Mid = 1.1M };
            _mockCurrencyService.UpdateCurrency(currencyToUpdate).Returns(Task.FromResult(currencyToUpdate));
            var result = await _controller.UpdateCurrency(currencyToUpdate);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(currencyToUpdate, Is.EqualTo(okResult.Value as CurrencyModel));
        }

        [Test]
        public async Task UpdateCurrency_WhenCurrencyDoesNotExist_ReturnsNotFoundResult()
        {
            var nonExistingCurrency = new CurrencyModel { Code = "XYZ", CurrencyName = "Non Existing" };
            _mockCurrencyService.UpdateCurrency(nonExistingCurrency).Throws(new NotFoundException("Currency not found"));
            var result = await _controller.UpdateCurrency(nonExistingCurrency);

            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task UpdateCurrency_WhenInvalidDataIsProvided_ReturnsBadRequestResult()
        {
            var invalidCurrency = new CurrencyModel { Code = "123", CurrencyName = "Invalid Currency" };
            _mockCurrencyService.UpdateCurrency(invalidCurrency).Throws(new InvalidDataException("Invalid currency data"));
            var result = await _controller.UpdateCurrency(invalidCurrency);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task UpdateCurrency_WhenExceptionOccurs_ReturnsInternalServerError()
        {
            var currency = new CurrencyModel { Code = "AUD", CurrencyName = "Australian Dollar" };
            _mockCurrencyService.UpdateCurrency(currency).Throws(new Exception());
            var result = await _controller.UpdateCurrency(currency);

            Assert.That(result, Is.InstanceOf<ObjectResult>());
            var objectResult = result as ObjectResult;
            Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
        }

        [Test]
        public async Task UpdateCurrencies_WhenCurrenciesAreSuccessfullyUpdated_ReturnsOkObjectResult()
        {
            var currenciesToUpdate = new List<CurrencyModel>
            {
                new CurrencyModel { Code = "EUR", CurrencyName = "Euro", Mid = 1.1M },
                new CurrencyModel { Code = "GBP", CurrencyName = "British Pound", Mid = 1.2M }
            };
            _mockCurrencyService.UpdateCurrencies(currenciesToUpdate).Returns(Task.FromResult(currenciesToUpdate.AsEnumerable()));
            var result = await _controller.UpdateCurrencies(currenciesToUpdate);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(currenciesToUpdate, Is.EqualTo(okResult.Value as IEnumerable<CurrencyModel>));
        }

        [Test]
        public async Task UpdateCurrencies_WhenCurrencyDoesNotExist_ReturnsNotFoundResult()
        {
            var currenciesToUpdate = new List<CurrencyModel> { new CurrencyModel { Code = "XYZ", CurrencyName = "Non Existing" } };
            _mockCurrencyService.UpdateCurrencies(currenciesToUpdate).Throws(new NotFoundException("One or more currencies not found"));
            var result = await _controller.UpdateCurrencies(currenciesToUpdate);

            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task UpdateCurrencies_WhenInvalidDataIsProvided_ReturnsBadRequestResult()
        {
            var invalidCurrencies = new List<CurrencyModel> { new CurrencyModel { Code = "123", CurrencyName = "Invalid Currency" } };
            _mockCurrencyService.UpdateCurrencies(invalidCurrencies).Throws(new InvalidDataException("Invalid currency data"));
            var result = await _controller.UpdateCurrencies(invalidCurrencies);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task UpdateCurrencies_WhenExceptionOccurs_ReturnsInternalServerError()
        {
            var currencies = new List<CurrencyModel> { new CurrencyModel { Code = "AUD", CurrencyName = "Australian Dollar" } };
            _mockCurrencyService.UpdateCurrencies(currencies).Throws(new Exception());
            var result = await _controller.UpdateCurrencies(currencies);

            Assert.That(result, Is.InstanceOf<ObjectResult>());
            var objectResult = result as ObjectResult;
            Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
        }
        [Test]
        public async Task DeleteCurrency_WhenCurrencyExists_DeletesCurrencyAndReturnsOkResult()
        {
            var currencyCode = "USD";
            _mockCurrencyService.When(x => x.DeleteCurrency(currencyCode, DateTime.Now)).Do(x => { });
            var result = await _controller.DeleteCurrency(currencyCode);

            Assert.That(result, Is.InstanceOf<OkResult>());
        }

        [Test]
        public async Task DeleteCurrency_WhenCurrencyDoesNotExist_ReturnsNotFoundResult()
        {
            var currencyCode = "XYZ";
            _mockCurrencyService.When(x => x.DeleteCurrency(currencyCode, Arg.Any<DateTime>())).Throw(new NotFoundException("Currency not found"));
            var result = await _controller.DeleteCurrency(currencyCode);

            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task DeleteCurrency_WhenInvalidDataIsProvided_ReturnsBadRequestResult()
        {
            var invalidCurrencyCode = "123";
            _mockCurrencyService.When(x => x.DeleteCurrency(invalidCurrencyCode, Arg.Any<DateTime>())).Throw(new InvalidDataException("Invalid currency code"));
            var result = await _controller.DeleteCurrency(invalidCurrencyCode);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task DeleteCurrency_WhenExceptionOccurs_ReturnsInternalServerError()
        {
            var currencyCode = "AUD";
            _mockCurrencyService.When(x => x.DeleteCurrency(currencyCode, Arg.Any<DateTime>())).Throw(new Exception());
            var result = await _controller.DeleteCurrency(currencyCode);

            Assert.That(result, Is.InstanceOf<ObjectResult>());
            var objectResult = result as ObjectResult;
            Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
        }

        [Test]
        public async Task ConvertCurrency_WhenConversionIsValid_ReturnsOkResultWithConvertedAmount()
        {
            var sourceCurrencyCode = "USD";
            var targetCurrencyCode = "EUR";
            var amount = 100m;
            var convertedAmount = 85m;
            _mockCurrencyService.ConvertCurrency(sourceCurrencyCode, targetCurrencyCode, amount)
                                .Returns(Task.FromResult(convertedAmount));
            var result = await _controller.ConvertCurrency(sourceCurrencyCode, targetCurrencyCode, amount);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            var response = okResult.Value as ConvertCurrencyResponse;
            Assert.That(response.ConvertedAmount, Is.EqualTo(convertedAmount));
        }

        [Test]
        public async Task ConvertCurrency_WhenCurrencyNotFound_ReturnsNotFoundResult()
        {
            var sourceCurrencyCode = "XYZ";
            var targetCurrencyCode = "EUR";
            var amount = 100m;
            _mockCurrencyService.ConvertCurrency(sourceCurrencyCode, targetCurrencyCode, amount)
                                .Throws(new NotFoundException("Currency not found"));
            var result = await _controller.ConvertCurrency(sourceCurrencyCode, targetCurrencyCode, amount);

            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task ConvertCurrency_WhenInvalidDataProvided_ReturnsBadRequestResult()
        {
            var sourceCurrencyCode = "USD";
            var targetCurrencyCode = "123";
            var amount = -100m;
            _mockCurrencyService.ConvertCurrency(sourceCurrencyCode, targetCurrencyCode, amount)
                                .Throws(new InvalidDataException("Invalid data"));
            var result = await _controller.ConvertCurrency(sourceCurrencyCode, targetCurrencyCode, amount);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task ConvertCurrency_WhenExceptionOccurs_ReturnsInternalServerError()
        {
            var sourceCurrencyCode = "USD";
            var targetCurrencyCode = "EUR";
            var amount = 100m;
            _mockCurrencyService.ConvertCurrency(sourceCurrencyCode, targetCurrencyCode, amount)
                                .Throws(new Exception());
            var result = await _controller.ConvertCurrency(sourceCurrencyCode, targetCurrencyCode, amount);

            Assert.That(result, Is.InstanceOf<ObjectResult>());
            var objectResult = result as ObjectResult;
            Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
        }
    }
}