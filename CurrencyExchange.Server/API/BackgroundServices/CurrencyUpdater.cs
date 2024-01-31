using CurrencyExchange.Server.API.Services.Currency;

namespace CurrencyExchange.Server.API.BackgroundServices
{
    public class CurrencyUpdater : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly TimeSpan _timeInterval = TimeSpan.FromSeconds(10);

        public CurrencyUpdater(IServiceProvider services)
        {
            _services = services;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
           while(!stoppingToken.IsCancellationRequested)
            {
                await UpdateCurrencies();
                await Task.Delay(_timeInterval, stoppingToken);
            }
        }

        public async Task UpdateCurrencies()
        {
            var currencyService = _services.CreateScope().ServiceProvider.GetRequiredService<ICurrencyService>();
            await currencyService.FetchDataFromNbp();
        }
    }
}
