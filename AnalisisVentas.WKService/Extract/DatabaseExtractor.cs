using AnalisisVentas.Data.Persistence;
using AnalisisVentas.Data.Interfaces;
using Dapper;
using SistemaVentas.ETL.App.Models.dboSchema;
using Microsoft.Extensions.Logging;

namespace AnalisisVentas.WKService.Extract
{
    public class DatabaseExtractor
    {
        private readonly DapperContext _context;
        private readonly ILogger<DatabaseExtractor> _logger;

        public DatabaseExtractor(DapperContext context, ILogger<DatabaseExtractor> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task ExtractAsync(ExtractedData data)
        {
            using var connection = _context.CreateSourceConnection();

            try
            {
                _logger.LogInformation("🗄️ Extrayendo datos de la base de datos...");

                data.DbCustomers = (await connection.QueryAsync<Customer>("SELECT * FROM Customers")).ToList();
                data.DbProducts = (await connection.QueryAsync<Product>("SELECT * FROM Products")).ToList();
                data.DbOrders = (await connection.QueryAsync<Order>("SELECT * FROM Orders")).ToList();
                data.DbOrderDetails = (await connection.QueryAsync<OrderDetail>("SELECT * FROM OrderDetails")).ToList();

                _logger.LogInformation("✅ Datos de BD extraídos correctamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error en DatabaseExtractor");

                data.DbCustomers = new();
                data.DbProducts = new();
                data.DbOrders = new();
                data.DbOrderDetails = new();
            }
        }
    }
}