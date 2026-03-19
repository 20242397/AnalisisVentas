using AnalisisVentas.Data.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

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
            var folderPath = _config["Staging:Path"];

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            await SaveFile(folderPath, "customers.json", data.DbCustomers);
            await SaveFile(folderPath, "products.json", data.DbProducts);
            await SaveFile(folderPath, "orders.json", data.DbOrders);
            await SaveFile(folderPath, "orderdetails.json", data.DbOrderDetails);

            await SaveFile(folderPath, "customers_api.json", data.ApiCustomers);
            await SaveFile(folderPath, "products_api.json", data.ApiProducts);

            await SaveFile(folderPath, "products_csv.json", data.CsvProducts);
            await SaveFile(folderPath, "customers_csv.json", data.CsvCustomers);
            await SaveFile(folderPath, "orders_csv.json", data.CsvOrders);
            await SaveFile(folderPath, "orderdetails_csv.json", data.CsvOrderDetails);
        }

        private async Task SaveFile<T>(string folder, string fileName, List<T> data)
        {
            var path = Path.Combine(folder, fileName);

            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            await File.WriteAllTextAsync(path, json);
        }
    }
}