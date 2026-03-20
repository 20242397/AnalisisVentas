
using AnalisisVentas.Data.Interfaces;
using AnalisisVentas.Data.Persistence;
using AnalisisVentas.WKService.Extract;
using AnalisisVentas.WKService.Load;
using AnalisisVentas.WKService.Staging;
using AnalisisVentas.WKService.Transform;

namespace AnalisisVentas.WKService
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            builder.Services.AddSingleton<DapperContext>();
            builder.Services.AddHttpClient(); 

           
            builder.Services.AddTransient<CsvExtractor>();
            builder.Services.AddTransient<ApiExtractor>();
            builder.Services.AddTransient<DatabaseExtractor>();

            builder.Services.AddTransient<JsonStagingService>();
            builder.Services.AddTransient<ITransformer, DataTransformer>();
            builder.Services.AddTransient<ILoader, DataLoader>();

          
            builder.Services.AddHostedService<Worker>();

           
            var host = builder.Build();
            await host.RunAsync();
        }
    }
}