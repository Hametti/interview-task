namespace CurrencyExchange.Server.API.Models.RequestResponseModels
{
    public class BasicResponse
    {
        public BasicResponse()
        {
            Success = true;
        }

        public bool Success { get; set; }
        public string Message { get; set; }
    }
}
