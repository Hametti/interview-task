namespace CurrencyExchange.Server.API.Models
{
    public class ExchangeRate
    {
        public string CurrencyName { get; set; }
        public string CurrencyCode { get; set; }
        public decimal CurrencyMidValue { get; set; }
        public decimal CurrencyBidValue { get; set; }
        public decimal CurrencyAskValue { get; set; }
    }
}
