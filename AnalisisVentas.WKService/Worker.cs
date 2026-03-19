using AnalisisVentas.Data.Interfaces;
using AnalisisVentas.WKService.Extract;
using AnalisisVentas.WKService.Transform;
using AnalisisInventario.WorkerService.Load;
using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection; // Necesario para IServiceScopeFactory

namespace AnalisisVentas.WKService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        // Inyectamos la fábrica en lugar de los servicios directamente
        private readonly IServiceScopeFactory _scopeFactory;

        public Worker(ILogger<Worker> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Los Worker Services suelen correr en bucle, si solo quieres que corra una vez, 
            // puedes quitar el while, pero para producción suele usarse así:
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("🚀 Iniciando ciclo de proceso ETL...");

                    // CREAMOS UN SCOPE MANUAL
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        // Resolvemos los servicios DESDE el scope creado
                        var csvExtractor = scope.ServiceProvider.GetRequiredService<CsvExtractor>();
                        var apiExtractor = scope.ServiceProvider.GetRequiredService<ApiExtractor>();
                        var dbExtractor = scope.ServiceProvider.GetRequiredService<DatabaseExtractor>();
                        var transformer = scope.ServiceProvider.GetRequiredService<ITransformer>();
                        var loader = scope.ServiceProvider.GetRequiredService<ILoader>();

                        var extractedData = new ExtractedData();

                        // =========================
                        // 🔹 EXTRACT
                        // =========================
                        var swExtract = Stopwatch.StartNew();
                        _logger.LogInformation("📥 Iniciando fase EXTRACT...");

                        // Asumiendo que Extract es sincrónico según tu código
                        csvExtractor.Extract(extractedData);

                        await Task.WhenAll(
                            apiExtractor.ExtractAsync(extractedData),
                            dbExtractor.ExtractAsync(extractedData)
                        );

                        swExtract.Stop();
                        _logger.LogInformation($"⏱️ Extract completado en {swExtract.ElapsedMilliseconds} ms");

                        // =========================
                        // 🔹 STAGING (JSON)
                        // =========================
                        _logger.LogInformation("💾 Guardando datos en STAGING (JSON)...");
                        var stagingPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "staging");
                        Directory.CreateDirectory(stagingPath);

                        await File.WriteAllTextAsync(
                            Path.Combine(stagingPath, "extracted.json"),
                            JsonSerializer.Serialize(extractedData, new JsonSerializerOptions { WriteIndented = true })
                        );

                        // =========================
                        // 🔹 TRANSFORM
                        // =========================
                        var swTransform = Stopwatch.StartNew();
                        _logger.LogInformation("🔄 Iniciando fase TRANSFORM...");

                        var transformedData = transformer.Transform(extractedData);

                        swTransform.Stop();
                        _logger.LogInformation($"⏱️ Transform completado en {swTransform.ElapsedMilliseconds} ms");

                        // =========================
                        // 🔹 LOAD
                        // =========================
                        var swLoad = Stopwatch.StartNew();
                        _logger.LogInformation("📤 Iniciando fase LOAD...");

                        await loader.LoadAsync(transformedData);

                        swLoad.Stop();
                        _logger.LogInformation($"⏱️ Load completado en {swLoad.ElapsedMilliseconds} ms");

                        _logger.LogInformation("🎯 ETL FINALIZADO CORRECTAMENTE");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error crítico en el proceso ETL");
                }

                // Espera antes de la siguiente ejecución (ej. 1 hora)
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}