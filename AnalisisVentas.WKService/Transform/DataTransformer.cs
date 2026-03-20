using AnalisisVentas.Data.Entities.Dwh.Dimensiones;
using AnalisisVentas.Data.Entities.Dwh.Dimensions;
using AnalisisVentas.Data.Entities.Dwh.Facts;
using AnalisisVentas.Data.Interfaces;


namespace AnalisisVentas.WKService.Transform
{
    public class DataTransformer : ITransformer
    {
        public TransformedData Transform(ExtractedData data)
        {
            var result = new TransformedData();

          
            var apiCustomerLookup = data.ApiCustomers
                .GroupBy(ac => ac.CustomerID)
                .ToDictionary(g => g.Key, g => g.First());

            var sqlCustomerLookup = data.DbCustomers
                .GroupBy(c => c.CustomerId)
                .ToDictionary(g => g.Key, g => g.First());

            
            result.DimCustomers = data.ApiSales
                .GroupBy(s => s.CustomerID)
                .Select(g =>
                {
                    var sale = g.First();
                    var apiDetail = apiCustomerLookup.GetValueOrDefault(sale.CustomerID);
                    var sqlDetail = sqlCustomerLookup.GetValueOrDefault(sale.CustomerID);

                    return new DimCustomer
                    {
                        CustomerID = sale.CustomerID,
                        FirstName = sale.FirstName,
                        LastName = sale.LastName,
                       

                        Email = apiDetail?.Email ?? sqlDetail?.Email ?? "No Email",
                        Phone = apiDetail?.Phone ?? sqlDetail?.Phone ?? "No Phone",

                       
                        City = sale.CityName,
                        Country = sale.CountryName
                    };
                })
                .ToList();

            
            var apiCustomerIds = result.DimCustomers
                .Select(c => c.CustomerID)
                .ToHashSet();

            var sqlOnlyCustomers = data.DbCustomers
                .Where(c => !apiCustomerIds.Contains(c.CustomerId))
                .Select(c => new DimCustomer
                {
                    CustomerID = c.CustomerId,
                    FirstName = c.FirstName,
                    LastName = c.LastName,
                    Email = c.Email,
                    Phone = c.Phone,
                    City = "Desconocido",
                    Country = "República Dominicana"
                });

            result.DimCustomers = result.DimCustomers
                .Concat(sqlOnlyCustomers)
                .ToList();


         
            var apiProducts = data.ApiSales.Select(a => new DimProduct
            {
                ProductID = a.ProductID,
                ProductName = a.ProductName,
                Category = a.CategoryName,
                Price = a.Price
            });

            var csvProducts = data.CsvProducts.Select(c => new DimProduct
            {
                ProductID = c.ProductID,
                ProductName = c.ProductName,
                Category = "CSV Import",
                Price = c.Price
            });

            var sqlProducts = data.DbProducts.Select(p => new DimProduct
            {
                ProductID = p.ProductId,
                ProductName = p.ProductName,
                Category = "Stock Local",
                Price = p.Price ?? 0m
            });

            result.DimProducts = apiProducts
                .Concat(csvProducts)
                .Concat(sqlProducts)
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

            result.FactSales = sqlFacts
                .Concat(csvFacts)
                .Concat(apiFacts)
                .ToList();

            return result;
        }
    }
}