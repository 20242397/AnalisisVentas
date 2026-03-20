using AnalisisVentas.Data.Entities.Csv;
using AnalisisVentas.Data.Interfaces;
using CsvHelper;
using System.Globalization;

namespace AnalisisVentas.WKService.Extract
{
    public class CsvExtractor
    {
        private readonly IConfiguration _config;
        private readonly ILogger<CsvExtractor> _logger;

        public CsvExtractor(IConfiguration config, ILogger<CsvExtractor> logger)
        {
            _config = config;
            _logger = logger;
        }

        public void Extract(ExtractedData data)
        {
            try
            {
                _logger.LogInformation(" Iniciando extracción de archivos CSV desde ruta externa...");

                
                string basePath = _config["CsvSettings:BasePath"] ?? "";

                data.CsvProducts = ReadCsv<ProductCsv>(basePath, "CsvSettings:ProductsPath");
                data.CsvCustomers = ReadCsv<CustomerCsv>(basePath, "CsvSettings:CustomersPath");
                data.CsvOrders = ReadCsv<OrderCsv>(basePath, "CsvSettings:OrdersPath");
                data.CsvOrderDetails = ReadCsv<OrderDetailCsv>(basePath, "CsvSettings:OrderDetailsPath");

                _logger.LogInformation(" Fase de extracción CSV finalizada.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, " Error crítico en CsvExtractor");
            }
        }

        private List<T> ReadCsv<T>(string basePath, string configKey)
        {
            
            var fileName = _config[configKey];

            
            var fullPath = Path.Combine(basePath, fileName ?? "");

            if (string.IsNullOrEmpty(fileName) || !File.Exists(fullPath))
            {
                _logger.LogWarning($" Archivo no encontrado o ruta inválida: {fullPath}");
                return new List<T>();
            }

            try
            {
                using var reader = new StreamReader(fullPath);
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
                return csv.GetRecords<T>().ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($" Error al leer el archivo {fileName}: {ex.Message}");
                return new List<T>();
            }
        }
    }
}