namespace CurrencyExchange.Server.Database.Repositories
{
    public interface IBaseRepository<Entity>
    {
        void SaveChanges();
    }
}
