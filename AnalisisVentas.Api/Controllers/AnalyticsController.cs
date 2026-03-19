using Microsoft.AspNetCore.Mvc;
using Dapper;
using AnalisisVentas.Data.Persistence;

namespace AnalisisVentas.API.Controllers
{
    [ApiController]
    [Route("api/analytics")]
    public class AnalyticsController : ControllerBase
    {
        private readonly DapperContext _context;

        public AnalyticsController(DapperContext context)
        {
            _context = context;
        }

        // 🔥 1. TOP PRODUCTOS
        [HttpGet("top-products")]
        public async Task<IActionResult> GetTopProducts()
        {
            using var connection = _context.CreateDwhConnection();

            var result = await connection.QueryAsync(@"
                SELECT TOP 10 
                    p.ProductName,
                    SUM(f.Quantity) AS TotalSold,
                    SUM(f.TotalPrice) AS Revenue
                FROM FactSales f
                INNER JOIN DimProduct p ON f.ProductID = p.ProductID
                GROUP BY p.ProductName
                ORDER BY Revenue DESC
            ");

            return Ok(result);
        }

        // 🔥 2. VENTAS POR FECHA
        [HttpGet("sales-by-date")]
        public async Task<IActionResult> GetSalesByDate()
        {
            using var connection = _context.CreateDwhConnection();

            var result = await connection.QueryAsync(@"
                SELECT 
                    d.Year,
                    d.Month,
                    SUM(f.TotalPrice) AS TotalSales
                FROM FactSales f
                INNER JOIN DimDate d ON f.DateID = d.DateID
                GROUP BY d.Year, d.Month
                ORDER BY d.Year, d.Month
            ");

            return Ok(result);
        }

        // 🔥 3. RENDIMIENTO DE CLIENTES
        [HttpGet("customer-performance")]
        public async Task<IActionResult> GetCustomerPerformance()
        {
            using var connection = _context.CreateDwhConnection();

            var result = await connection.QueryAsync(@"
                SELECT 
                    c.FirstName,
                    c.LastName,
                    SUM(f.TotalPrice) AS TotalSpent
                FROM FactSales f
                INNER JOIN DimCustomer c ON f.CustomerID = c.CustomerID
                GROUP BY c.FirstName, c.LastName
                ORDER BY TotalSpent DESC
            ");

            return Ok(result);
        }
    }
}