using AnalisisVentas.Data.Interfaces;
using AnalisisVentas.WKService.Extract;
using AnalisisVentas.WKService.Staging; 
using System.Diagnostics;

namespace AnalisisVentas.WKService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public Worker(ILogger<Worker> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("🚀 Iniciando ciclo de proceso ETL: {time}", DateTimeOffset.Now);

                    using (var scope = _scopeFactory.CreateScope())
                    {
                      
                        var csvExtractor = scope.ServiceProvider.GetRequiredService<CsvExtractor>();
                        var apiExtractor = scope.ServiceProvider.GetRequiredService<ApiExtractor>();
                        var dbExtractor = scope.ServiceProvider.GetRequiredService<DatabaseExtractor>();
                        var transformer = scope.ServiceProvider.GetRequiredService<ITransformer>();
                        var loader = scope.ServiceProvider.GetRequiredService<ILoader>();
                        var stagingService = scope.ServiceProvider.GetRequiredService<JsonStagingService>();

                        var extractedData = new ExtractedData();

                       
                        var swExtract = Stopwatch.StartNew();
                        _logger.LogInformation("📥 Iniciando fase EXTRACT...");

                        csvExtractor.Extract(extractedData);

                       
                        await Task.WhenAll(
                            apiExtractor.ExtractAsync(extractedData),
                            dbExtractor.ExtractAsync(extractedData)
                        );

                        swExtract.Stop();
                        _logger.LogInformation(" Extract completado en {ms} ms", swExtract.ElapsedMilliseconds);

                       
                        _logger.LogInformation(" Guardando datos en STAGING...");
                        await stagingService.SaveAsync(extractedData);

                       
                        var swTransform = Stopwatch.StartNew();
                        _logger.LogInformation(" Iniciando fase TRANSFORM (Resolviendo JOINS)...");

                        var transformedData = transformer.Transform(extractedData);

                        swTransform.Stop();
                        _logger.LogInformation(" Transform completado en {ms} ms", swTransform.ElapsedMilliseconds);

                       
                        var swLoad = Stopwatch.StartNew();
                        _logger.LogInformation(" Iniciando fase LOAD hacia Data Warehouse...");

                        await loader.LoadAsync(transformedData);

                        swLoad.Stop();
                        _logger.LogInformation(" Load completado en {ms} ms", swLoad.ElapsedMilliseconds);

                        _logger.LogInformation(" ETL FINALIZADO CON ÉXITO");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, " Error fatal en el proceso ETL");
                }

                _logger.LogInformation(" Esperando 1 hora para el siguiente ciclo...");
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}