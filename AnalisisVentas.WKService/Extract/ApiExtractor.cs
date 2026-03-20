using AnalisisVentas.Data.Entities.Api;
using AnalisisVentas.Data.Interfaces;
using System.Net.Http.Json;

namespace AnalisisVentas.WKService.Extract
{
    public class ApiExtractor
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly ILogger<ApiExtractor> _logger;

        public ApiExtractor(HttpClient httpClient, IConfiguration config, ILogger<ApiExtractor> logger)
        {
            _httpClient = httpClient;
            _config = config;
            _logger = logger;
        }

        public async Task ExtractAsync(ExtractedData data)
        {
            
            var baseUrl = _config["Api:BaseUrl"];

            try
            {
                _logger.LogInformation("==> Iniciando extracción desde API REST...");

                
                var response = await _httpClient.GetFromJsonAsync<List<VentaExtractDto>>($"{baseUrl}/SourceVentas/extract");

                if (response != null && response.Count > 0)
                {
                    data.ApiSales = response;
                    _logger.LogInformation($"==> Éxito: {data.ApiSales.Count} registros de ventas extraídos de la API.");
                }
                else
                {
                    _logger.LogWarning("==> La API respondió correctamente pero no se encontraron registros.");
                }
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError($"!!! Error de conexión: ¿Está la Web API corriendo en {baseUrl}? Detalle: {httpEx.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "!!! Error inesperado en ApiExtractor");
            }
        }
    }
}