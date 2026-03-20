using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AnalisisVentas.Api.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Data.SqlClient;
    using Dapper;
    using AnalisisVentas.Data.Entities.Api;

    [ApiController]
    [Route("api/source/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly string _connectionString;

        public CustomersController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("SourceDb");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustomerApiDto>>> GetCustomers()
        {
            using var connection = new SqlConnection(_connectionString);

            
            string sql = @"
            SELECT 
                c.CustomerID, 
                c.FirstName, 
                c.LastName, 
                c.Email, 
                c.Phone, 
                ci.CityName as City, 
                co.CountryName as Country
            FROM Customers c
            INNER JOIN Cities ci ON c.CityID = ci.CityID
            INNER JOIN Countries co ON ci.CountryID = co.CountryID";

            var customers = await connection.QueryAsync<CustomerApiDto>(sql);
            return Ok(customers);
        }
    }
}
