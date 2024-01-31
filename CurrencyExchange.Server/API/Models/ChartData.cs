namespace CurrencyExchange.Server.API.Models
{
    public class ChartData
    {
        public string CurrencyCode { get; set; }
        public List<KeyValuePair<string, decimal>> DateToPrice { get; set; }
    }
}
