using Microsoft.AspNetCore.Mvc;
using Dapper;
using SistemaVentas.ETL.App.Models.dboSchema;
using AnalisisVentas.Data.Persistence;

namespace AnalisisVentas.API.Controllers
{
    [ApiController]
    [Route("api/source")]
    public class SourceDataController : ControllerBase
    {
        private readonly DapperContext _context;

        public SourceDataController(DapperContext context)
        {
            _context = context;
        }

        // 🔹 GET: api/source/customers
        [HttpGet("customers")]
        public async Task<IActionResult> GetCustomers()
        {
            using var connection = _context.CreateSourceConnection();

            var customers = await connection.QueryAsync<Customer>("SELECT * FROM Customers");

            return Ok(customers);
        }

        // 🔹 GET: api/source/products
        [HttpGet("products")]
        public async Task<IActionResult> GetProducts()
        {
            using var connection = _context.CreateSourceConnection();

            var products = await connection.QueryAsync<Product>("SELECT * FROM Products");

            return Ok(products);
        }
    }
}