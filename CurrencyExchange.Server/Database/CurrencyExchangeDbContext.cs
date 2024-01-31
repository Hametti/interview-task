using CurrencyExchange.Server.Database.Entities.Currency;
using Microsoft.EntityFrameworkCore;

namespace CurrencyExchange.Server.Database
{
    public class CurrencyExchangeDbContext : DbContext
    {
        public DbSet<CurrencyModel> Currencies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CurrencyModel>()
                .HasKey(c => c.Id); 
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=CurrencyExchange.db");
        }
    }
}
