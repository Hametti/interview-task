namespace CurrencyExchange.Server.API.Models.NbpApi
{
    public class Rate
    {
        public string Currency { get; set; }
        public string Code { get; set; }
        public decimal? Mid { get; set; }
        public decimal? Bid { get; set; }
        public decimal? Ask { get; set; }
    }
}
