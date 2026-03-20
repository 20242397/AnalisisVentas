using AnalisisVentas.Data.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.IO;

namespace AnalisisVentas.WKService.Staging
{
    public class JsonStagingService
    {
        private readonly IConfiguration _config;

        public JsonStagingService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SaveAsync(ExtractedData data)
        {
           
            var folderPath = _config["Staging:Path"] ?? Path.Combine(AppContext.BaseDirectory, "StagingArea");

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

          
            await SaveFile(folderPath, "sql_customers.json", data.DbCustomers);
            await SaveFile(folderPath, "sql_products.json", data.DbProducts);
            await SaveFile(folderPath, "sql_orders.json", data.DbOrders);
            await SaveFile(folderPath, "sql_orderdetails.json", data.DbOrderDetails);

          
            await SaveFile(folderPath, "api_sales_consolidated.json", data.ApiSales);
            await SaveFile(folderPath, "api_customers_raw.json", data.ApiCustomers);
            await SaveFile(folderPath, "api_products_raw.json", data.ApiProducts);

          
            await SaveFile(folderPath, "csv_products.json", data.CsvProducts);
            await SaveFile(folderPath, "csv_customers.json", data.CsvCustomers);
            await SaveFile(folderPath, "csv_orders.json", data.CsvOrders);
            await SaveFile(folderPath, "csv_orderdetails.json", data.CsvOrderDetails);

            Console.WriteLine($"📂 Staging completado: Archivos guardados en {folderPath}");
        }

        private async Task SaveFile<T>(string folder, string fileName, List<T> data)
        {
            if (data == null || data.Count == 0) return;

            var path = Path.Combine(folder, fileName);

            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await File.WriteAllTextAsync(path, json);
        }
    }
}