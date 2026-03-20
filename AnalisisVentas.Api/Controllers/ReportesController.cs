
using Dapper;
using Microsoft.AspNetCore.Mvc;

namespace AnalisisVentas.Api.Controllers
{
    [ApiController]
    [Route("api/reportes")]
    public class ReportesController : ControllerBase
    {
        private readonly string _dwConnectionString;

        public ReportesController(IConfiguration configuration)
        {
            _dwConnectionString = configuration.GetConnectionString("DWH");
        }

        
        [HttpGet("ventas-por-producto")]
        public async Task<IActionResult> GetVentasPorProducto()
        {
            string sql = @"SELECT p.ProductName as Nombre, SUM(f.TotalPrice) as Total 
                       FROM FactSales f JOIN DimProduct p ON f.ProductID = p.ProductID 
                       GROUP BY p.ProductName";
            using var db = new Microsoft.Data.SqlClient.SqlConnection(_dwConnectionString);
            return Ok(await db.QueryAsync(sql));
        }

        
        [HttpGet("ventas-por-cliente")]
        public async Task<IActionResult> GetVentasPorCliente()
        {
            string sql = @"SELECT c.FirstName + ' ' + c.LastName as Nombre, SUM(f.TotalPrice) as Total 
                       FROM FactSales f JOIN DimCustomer c ON f.CustomerID = c.CustomerID 
                       GROUP BY c.FirstName, c.LastName";
            using var db = new Microsoft.Data.SqlClient.SqlConnection(_dwConnectionString);
            return Ok(await db.QueryAsync(sql));
        }

        
        [HttpGet("ventas-por-mes")]
        public async Task<IActionResult> GetVentasPorMes()
        {
            string sql = @"SELECT d.Year, d.Month, SUM(f.TotalPrice) as Total 
                       FROM FactSales f JOIN DimDate d ON f.DateID = d.DateID 
                       GROUP BY d.Year, d.Month ORDER BY d.Year, d.Month";
            using var db = new Microsoft.Data.SqlClient.SqlConnection(_dwConnectionString);
            return Ok(await db.QueryAsync(sql));
        }

        
        [HttpGet("top-productos")]
        public async Task<IActionResult> GetTopProductos()
        {
            string sql = @"SELECT TOP 5 p.ProductName as Nombre, SUM(f.Quantity) as TotalUnidades 
                       FROM FactSales f JOIN DimProduct p ON f.ProductID = p.ProductID 
                       GROUP BY p.ProductName ORDER BY TotalUnidades DESC";
            using var db = new Microsoft.Data.SqlClient.SqlConnection(_dwConnectionString);
            return Ok(await db.QueryAsync(sql));
        }

        
        [HttpGet("top-clientes")]
        public async Task<IActionResult> GetTopClientes()
        {
            string sql = @"SELECT TOP 5 c.FirstName + ' ' + c.LastName as Nombre, COUNT(DISTINCT f.OrderID) as TotalOrdenes 
                       FROM FactSales f JOIN DimCustomer c ON f.CustomerID = c.CustomerID 
                       GROUP BY c.FirstName, c.LastName ORDER BY TotalOrdenes DESC";
            using var db = new Microsoft.Data.SqlClient.SqlConnection(_dwConnectionString);
            return Ok(await db.QueryAsync(sql));
        }
    }
}
