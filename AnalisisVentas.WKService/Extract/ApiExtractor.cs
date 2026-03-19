using AnalisisVentas.Data.Entities.Api;
using AnalisisVentas.Data.Interfaces;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

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
                _logger.LogInformation("🌐 Consumiento API...");

                var customers = await _httpClient.GetFromJsonAsync<List<CustomerApiDto>>($"{baseUrl}/source/customers");
                var products = await _httpClient.GetFromJsonAsync<List<ProductApiDto>>($"{baseUrl}/source/products");

                data.ApiCustomers = customers ?? new();
                data.ApiProducts = products ?? new();

                _logger.LogInformation("✅ API extraída correctamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error en API Extractor");

                data.ApiCustomers = new();
                data.ApiProducts = new();
            }
        }
    }
}