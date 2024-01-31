namespace CurrencyExchange.Server.API.Models.RequestResponseModels.Currency
{
    public class ConvertCurrencyResponse : BasicResponse
    {
        public decimal ConvertedAmount { get; set; }
    }
}
