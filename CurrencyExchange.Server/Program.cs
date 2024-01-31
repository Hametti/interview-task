
using CurrencyExchange.Server.API.BackgroundServices;
using CurrencyExchange.Server.API.Services.Currency;
using CurrencyExchange.Server.Database;
using CurrencyExchange.Server.Database.Repositories.CurrencyRepository;

namespace CurrencyExchange.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<CurrencyExchangeDbContext>();
            builder.Services.AddHttpClient();
            builder.Services.AddDbContext<CurrencyExchangeDbContext>();
            builder.Services.AddScoped<ICurrencyRepository, CurrencyRepository>();
            builder.Services.AddScoped<ICurrencyService, CurrencyService>();
            builder.Services.AddScoped<ICurrencyService, CurrencyService>();
            builder.Services.AddHostedService<CurrencyUpdater>();

            var app = builder.Build();

            app.UseCors(x => x
                .AllowAnyMethod()
                .AllowAnyHeader()
                .SetIsOriginAllowed(origin => true)
                .AllowCredentials());

            app.UseDefaultFiles();
            app.UseStaticFiles();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();


            app.MapControllers();

            app.MapFallbackToFile("/index.html");

            app.Run();
        }
    }
}
