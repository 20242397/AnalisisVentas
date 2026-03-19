using AnalisisVentas.Data.Entities.Dwh.Dimensiones;
using AnalisisVentas.Data.Entities.Dwh.Dimensions;
using AnalisisVentas.Data.Entities.Dwh.Facts;
using AnalisisVentas.Data.Interfaces;
using SistemaVentas.ETL.App.Models.dboSchema;

namespace AnalisisVentas.WKService.Transform
{
    public class DataTransformer : ITransformer
    {
        public TransformedData Transform(ExtractedData data)
        {
            var result = new TransformedData();

            // =========================
            // 🔹 1. CUSTOMERS (DB + API)
            // =========================
            var customers = data.DbCustomers
                .Concat(data.ApiCustomers.Select(a => new Customer
                {
                    CustomerId = a.CustomerID,
                    FirstName = a.FirstName,
                    LastName = a.LastName,
                    Email = a.Email,
                    Phone = "",
                    CityId = 0
                }))
                .Where(c => !string.IsNullOrEmpty(c.Email))
                .GroupBy(c => c.CustomerId)
                .Select(g => g.First())
                .ToList();

            result.DimCustomers = customers.Select(c => new DimCustomer
            {
                CustomerID = c.CustomerId,
                FirstName = c.FirstName,
                LastName = c.LastName,
                Email = c.Email,
                Phone = c.Phone,
                City = "N/A",     // porque no hicimos JOIN aún
                Country = "N/A"
            }).ToList();

            // =========================
            // 🔹 2. PRODUCTS (DB + CSV + API)
            // =========================
            var products = data.DbProducts
                .Concat(data.CsvProducts.Select(c => new Product
                {
                    ProductId = c.ProductID,
                    ProductName = c.ProductName,
                    CategoryId = 0,
                    Price = c.Price,
                    Stock = c.Stock
                }))
                .Concat(data.ApiProducts.Select(a => new Product
                {
                    ProductId = a.ProductID,
                    ProductName = a.ProductName,
                    CategoryId = 0,
                    Price = a.Price,
                    Stock = 0
                }))
                .GroupBy(p => p.ProductId)
                .Select(g => g.First())
                .ToList();

            result.DimProducts = products.Select(p => new DimProduct
            {
                ProductID = p.ProductId,
                ProductName = p.ProductName,
                Category = "General", // porque no hicimos JOIN a Categories
                Price = (decimal)p.Price,
            }).ToList();

            // =========================
            // 🔹 3. ORDERS (DB + CSV)
            // =========================
            // Fix for CS0029: Convert DateTime to DateOnly when mapping OrderCsv to Order
            var orders = data.DbOrders
                .Concat(data.CsvOrders.Select(c => new Order
                {
                    OrderId = c.OrderID,
                    CustomerId = c.CustomerID,
                    OrderDate = c.OrderDate,
                    StatusId = 0
                }))
                .GroupBy(o => o.OrderId)
                .Select(g => g.First())
                .ToList();

            var orderDetails = data.DbOrderDetails
                .Concat(data.CsvOrderDetails.Select(c => new OrderDetail
                {
                    OrderId = c.OrderID,
                    ProductId = c.ProductID,
                    Quantity = c.Quantity,
                    TotalPrice = c.TotalPrice
                }))
                .ToList();

            // =========================
            // 🔹 4. DIM DATE
            // =========================
            result.DimDates = orders
                .Where(o => o.OrderDate != default(DateTime))
                .Select(o => new DimDate
                {
                    DateID = int.Parse(o.OrderDate.ToString("yyyyMMdd")),
                    FullDate = o.OrderDate,
                    Year = o.OrderDate.Year,
                    Month = o.OrderDate.Month,
                    Day = o.OrderDate.Day
                })
                .DistinctBy(d => d.DateID)
                .ToList();

            // =========================
            // 🔹 5. FACT SALES
            // =========================
            result.FactSales = (from o in orders
                                join od in orderDetails on o.OrderId equals od.OrderId
                                where o.CustomerId.HasValue && od.Quantity.HasValue
                                select new FactSales
                                {
                                    OrderID = o.OrderId,
                                    CustomerID = o.CustomerId.Value, // Fix: use .Value to get int from int?
                                    ProductID = od.ProductId,
                                    DateID = int.Parse(o.OrderDate.ToString("yyyyMMdd")), // Fix: use .Value and correct ToString overload
                                    Quantity = od.Quantity.Value, // Fix: use .Value to get int from int?
                                    TotalPrice = od.TotalPrice ?? 0m // Fix: handle nullable decimal
                                }).ToList();

            return result;
        }
    }
}