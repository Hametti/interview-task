namespace CurrencyExchange.Server.Database.Repositories
{
    public abstract class BaseRepository<Entity> : IBaseRepository<Entity> where Entity : class
    {
        protected readonly CurrencyExchangeDbContext _currencyExchangeDbContext;
        public BaseRepository(CurrencyExchangeDbContext currencyExchangeDbContext)
        {
            currencyExchangeDbContext.Database.EnsureCreated();
            _currencyExchangeDbContext = currencyExchangeDbContext;
        }

        public void SaveChanges()
        {
            _currencyExchangeDbContext.SaveChanges();
        }
    }
}
