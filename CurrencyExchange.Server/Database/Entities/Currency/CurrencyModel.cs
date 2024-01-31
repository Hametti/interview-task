namespace CurrencyExchange.Server.Database.Entities.Currency
{
    public class CurrencyModel
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string CurrencyName { get; set; }
        public decimal? Mid { get; set; }
        public decimal? Bid { get; set; }
        public decimal? Ask { get; set; }
        public DateTime EffectiveDate { get; set; }
    }
}
