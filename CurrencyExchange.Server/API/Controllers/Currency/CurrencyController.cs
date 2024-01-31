using CurrencyExchange.Server.API.Exceptions;
using CurrencyExchange.Server.API.Models;
using CurrencyExchange.Server.API.Models.RequestResponseModels;
using CurrencyExchange.Server.API.Models.RequestResponseModels.Currency;
using CurrencyExchange.Server.API.Services.Currency;
using CurrencyExchange.Server.Database.Entities.Currency;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyExchange.Server.API.Controllers.Currency
{
    [Route("api/[controller]")]
    [ApiController]
    public class CurrencyController : ControllerBase
    {
        private const string UnknownExceptionMessage = "An unexpected error occurred. Please try again later.";
        private readonly ICurrencyService _currencyService;
        private readonly HttpClient _httpClient;
        public CurrencyController(ICurrencyService currencyService, HttpClient httpClient)
        {
            _currencyService = currencyService;
            _httpClient = httpClient;
        }

        [HttpGet("{code}")]
        public async Task<IActionResult> GetCurrency(string code)
        {
            try
            {
                CurrencyModel currency = await _currencyService.GetCurrency(code, DateTime.Now.Date);
                return Ok(new GetCurrencyResponse() { Currency = currency });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new BasicResponse() { Success = false, Message = ex.Message });
            }
            catch (InvalidDataException ex)
            {
                return BadRequest(new BasicResponse() { Success = false, Message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new BasicResponse() { Success = false, Message = UnknownExceptionMessage });
            }
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllCurrencies()
        {
            try
            {
                IEnumerable<CurrencyModel> currency = await _currencyService.GetAllCurrencies(DateTime.Now);

                var result = new GetAllCurrenciesResponse() { Currencies = currency.ToList() };
                return Ok(result);
            }
            catch (Exception)
            {
                var result = new BasicResponse() { Success = false, Message = UnknownExceptionMessage };
                return StatusCode(StatusCodes.Status500InternalServerError, result);
            }
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddCurrency([FromBody] CurrencyModel currencyToAdd)
        {
            try
            {
                var createdCurrency = await _currencyService.AddCurrency(currencyToAdd);
                return CreatedAtAction(nameof(GetCurrency), new { code = createdCurrency.Code }, createdCurrency);
            }
            catch (AlreadyExistsException ex)
            {
                return Conflict(new BasicResponse() { Success = false, Message = ex.Message});
            }
            catch (InvalidDataException ex)
            {
                return BadRequest(new BasicResponse() { Success = false, Message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new BasicResponse() { Success = false, Message = UnknownExceptionMessage });
            }
        }

        [HttpPost("add/multiple")]
        public async Task<IActionResult> AddCurrencies([FromBody] IEnumerable<CurrencyModel> currenciesToAdd)
        {
            try
            {
                var createdCurrencies = await _currencyService.AddCurrencies(currenciesToAdd);

                return CreatedAtAction(nameof(GetAllCurrencies), createdCurrencies);
            }
            catch (AlreadyExistsException ex)
            {
                return Conflict(new BasicResponse() { Success = false, Message = ex.Message });
            }
            catch (InvalidDataException ex)
            {
                return BadRequest(new BasicResponse() { Success = false, Message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new BasicResponse() { Success = false, Message = UnknownExceptionMessage });
            }
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateCurrency([FromBody] CurrencyModel currencyToUpdate)
        {
            try
            {
                var updatedCurrency = await _currencyService.UpdateCurrency(currencyToUpdate);

                return Ok(updatedCurrency);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new BasicResponse() { Success = false, Message = ex.Message });
            }
            catch (InvalidDataException ex)
            {
                return BadRequest(new BasicResponse() { Success = false, Message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new BasicResponse() { Success = false, Message = UnknownExceptionMessage });
            }
        }

        [HttpPut("update/multiple")]
        public async Task<IActionResult> UpdateCurrencies([FromBody] IEnumerable<CurrencyModel> currenciesToUpdate)
        {
            try
            {
                var updatedCurrencies = await _currencyService.UpdateCurrencies(currenciesToUpdate);
                return Ok(updatedCurrencies);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new BasicResponse() { Success = false, Message = ex.Message });
            }
            catch (InvalidDataException ex)
            {
                return BadRequest(new BasicResponse() { Success = false, Message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new BasicResponse() { Success = false, Message = UnknownExceptionMessage });
            }
        }

        [HttpDelete("delete/{currencyCode}")]
        public async Task<IActionResult> DeleteCurrency(string currencyCode)
        {
            try
            {
                await _currencyService.DeleteCurrency(currencyCode, DateTime.Now);
                return Ok();
            }
            catch (NotFoundException ex)
            {
                return NotFound(new BasicResponse() { Success = false, Message = ex.Message });
            }
            catch (InvalidDataException ex)
            {
                return BadRequest(new BasicResponse() { Success = false, Message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new BasicResponse() { Success = false, Message = UnknownExceptionMessage });
            }
        }

        [HttpGet("convert")]
        public async Task<IActionResult> ConvertCurrency([FromQuery] string sourceCurrencyCode, [FromQuery] string targetCurrencyCode, [FromQuery] decimal targetCurrencyAmount)
        {
            try
            {
                var convertedAmount = await _currencyService.ConvertCurrency(
                    sourceCurrencyCode,
                    targetCurrencyCode,
                    targetCurrencyAmount);

                var result = new ConvertCurrencyResponse() { ConvertedAmount = convertedAmount };
                return Ok(result);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new BasicResponse() { Success = false, Message = ex.Message });
            }
            catch (InvalidDataException ex)
            {
                return BadRequest(new BasicResponse() { Success = false, Message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new BasicResponse() { Success = false, Message = UnknownExceptionMessage });
            }
        }

        [HttpGet("chartData/{currencyCode}")]
        public async Task<IActionResult> ChartData(string currencyCode)
        {
            try
            {
                var chartData = await _currencyService.ChartData(currencyCode);
                return Ok(new ChartDataResponse() { ChartData = chartData });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new BasicResponse() { Success = false, Message = ex.Message });
            }
            catch (InvalidDataException ex)
            {
                return BadRequest(new BasicResponse() { Success = false, Message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new BasicResponse() { Success = false, Message = UnknownExceptionMessage });
            }
        }

        [HttpGet("getAvailableCurrencyCodes")]
        public async Task<IActionResult> GetAvailableCurrencyCodes()
        {
            try
            {
                IEnumerable<string> availableCurrencyCodes = await _currencyService.GetAvailableCurrencyCodes();
                return Ok(availableCurrencyCodes);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new BasicResponse() { Success = false, Message = ex.Message });
            }
            catch (InvalidDataException ex)
            {
                return BadRequest(new BasicResponse() { Success = false, Message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new BasicResponse() { Success = false, Message = UnknownExceptionMessage });
            }
        }

        [HttpGet("getExchangeRates")]
        public async Task<IActionResult> GetExchangeRates()
        {
            try
            {
                List<ExchangeRate> result = await _currencyService.GetExchangeRates();
                return Ok(new GetExchangeRatesResponse() { ExchangeRates = result });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new BasicResponse() { Success = false, Message = ex.Message });
            }
            catch (InvalidDataException ex)
            {
                return BadRequest(new BasicResponse() { Success = false, Message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new BasicResponse() { Success = false, Message = UnknownExceptionMessage });
            }
        }

        [HttpGet("fetchDataFromNbp")]
        public async Task<IActionResult> FetchDataFromNbp()
        {
            try
            {
                await _currencyService.FetchDataFromNbp();
                return Ok();
            }
            catch (NotFoundException ex)
            {
                return NotFound(new BasicResponse() { Success = false, Message = ex.Message });
            }
            catch (InvalidDataException ex)
            {
                return BadRequest(new BasicResponse() { Success = false, Message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new BasicResponse() { Success = false, Message = UnknownExceptionMessage });
            }
        }
    }
}
