using AnalisisVentas.Data.Entities.Api;
using Dapper;
using Microsoft.AspNetCore.Mvc;

namespace AnalisisVentas.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SourceVentasController : ControllerBase
    {
        private readonly string _connectionString;

        public SourceVentasController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("SourceDb");
        }

        [HttpGet("extract")]
        public async Task<ActionResult<IEnumerable<VentaExtractDto>>> GetVentasParaEtl()
        {
            using var connection = new Microsoft.Data.SqlClient.SqlConnection(_connectionString);

           
            string sql = @"
            SELECT 
                o.OrderID, o.CustomerID, cu.FirstName, cu.LastName, ci.CityName, co.CountryName, o.OrderDate,
                p.ProductID, p.ProductName, cat.CategoryName,
                od.Quantity, p.Price, od.TotalPrice
            FROM Orders o
            INNER JOIN OrderDetails od ON o.OrderID = od.OrderID
            INNER JOIN Products p ON od.ProductID = p.ProductID
            INNER JOIN Categories cat ON p.CategoryID = cat.CategoryID
            INNER JOIN Customers cu ON o.CustomerID = cu.CustomerID
            INNER JOIN Cities ci ON cu.CityID = ci.CityID
            INNER JOIN Countries co ON ci.CountryID = co.CountryID";

            var ventas = await connection.QueryAsync<VentaExtractDto>(sql);
            return Ok(ventas);
        }
    }
}
