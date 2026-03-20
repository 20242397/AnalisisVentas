using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Dapper;
using AnalisisVentas.Data.Entities.Api;

namespace AnalisisVentas.Api.Controllers
{
    [ApiController]
    [Route("api/source/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly string _connectionString;

        public ProductsController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("SourceDb");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductApiDto>>> GetProducts()
        {
            using var connection = new SqlConnection(_connectionString);

            
            string sql = @"
            SELECT 
                p.ProductID, 
                p.ProductName, 
                cat.CategoryName as Category, 
                p.Price
            FROM Products p
            INNER JOIN Categories cat ON p.CategoryID = cat.CategoryID";

            var products = await connection.QueryAsync<ProductApiDto>(sql);
            return Ok(products);
        }
    }
}