namespace CurrencyExchange.Server.API.Models.NbpApi
{
    public class ExchangeRatesTable
    {
        public string Table { get; set; }
        public string No { get; set; }
        public DateTime EffectiveDate { get; set; }
        public List<Rate> Rates { get; set; }
    }
}
