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

           
            var sqlCustomers = data.DbCustomers.Select(c => new DimCustomer
            {
                CustomerID = c.CustomerId,
                FirstName = c.FirstName,
                LastName = c.LastName,
                Email = c.Email,
                Phone = c.Phone,
                City = "Local Source",
                Country = "Dominican Republic"
            });

            var apiCustomers = data.ApiSales.Select(s => {
              
                var detail = data.ApiCustomers.FirstOrDefault(ac => ac.CustomerID == s.CustomerID);
                return new DimCustomer
                {
                    CustomerID = s.CustomerID,
                    FirstName = s.FirstName,
                    LastName = s.LastName,
                    Email = detail?.Email ?? "No Email",
                    Phone = detail?.Phone ?? "No Phone",
                    City = s.CityName,  
                    Country = s.CountryName 
                };
            });

            result.DimCustomers = sqlCustomers
                .Concat(apiCustomers)
                .GroupBy(c => c.CustomerID)
                .Select(g => g.First())
                .ToList();

            
            result.DimProducts = data.DbProducts
                .Select(p => new DimProduct { ProductID = p.ProductId, ProductName = p.ProductName, Category = "Local Stock", Price = p.Price ?? 0m })
                .Concat(data.CsvProducts.Select(c => new DimProduct { ProductID = c.ProductID, ProductName = c.ProductName, Category = "CSV Import", Price = c.Price }))
                .Concat(data.ApiSales.Select(a => new DimProduct
                {
                    ProductID = a.ProductID,
                    ProductName = a.ProductName,
                    Category = a.CategoryName,
                    Price = a.Price
                }))
                .GroupBy(p => p.ProductID)
                .Select(g => g.First())
                .ToList();

           
            var allDates = data.DbOrders.Select(o => o.OrderDate)
                .Concat(data.CsvOrders.Select(c => c.OrderDate))
                .Concat(data.ApiSales.Select(s => s.OrderDate))
                .Where(d => d != default)
                .Distinct();

            result.DimDates = allDates.Select(d => new DimDate
            {
                DateID = int.Parse(d.ToString("yyyyMMdd")),
                FullDate = d,
                Year = d.Year,
                Month = d.Month,
                Day = d.Day
            }).ToList();

           
            var sqlFacts = from o in data.DbOrders
                           join od in data.DbOrderDetails on o.OrderId equals od.OrderId
                           select new FactSales
                           {
                               OrderID = o.OrderId,
                               CustomerID = o.CustomerId ?? 0,
                               ProductID = od.ProductId,
                               Quantity = od.Quantity ?? 0,
                               TotalPrice = od.TotalPrice ?? 0m,
                               DateID = int.Parse(o.OrderDate.ToString("yyyyMMdd"))
                           };

            var csvFacts = from o in data.CsvOrders
                           join od in data.CsvOrderDetails on o.OrderID equals od.OrderID
                           select new FactSales
                           {
                               OrderID = o.OrderID,
                               CustomerID = o.CustomerID,
                               ProductID = od.ProductID,
                               Quantity = od.Quantity,
                               TotalPrice = od.TotalPrice,
                               DateID = int.Parse(o.OrderDate.ToString("yyyyMMdd"))
                           };

            var apiFacts = data.ApiSales.Select(s => new FactSales
            {
                OrderID = s.OrderID,
                CustomerID = s.CustomerID,
                ProductID = s.ProductID,
                Quantity = s.Quantity,
                TotalPrice = s.TotalPrice,
                DateID = int.Parse(s.OrderDate.ToString("yyyyMMdd"))
            });

            result.FactSales = sqlFacts.Concat(csvFacts).Concat(apiFacts).ToList();

            return result;
        }
    }
}