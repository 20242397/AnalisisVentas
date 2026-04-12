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

           
            var cityLookup = data.DbCities.ToDictionary(c => c.CityId, c => c.CityName);
            var countryLookup = data.DbCountries.ToDictionary(c => c.CountryId, c => c.CountryName);

           
            var apiCustomerLookup = data.ApiCustomers
                .GroupBy(ac => ac.CustomerID)
                .ToDictionary(g => g.Key, g => g.First());

            var sqlCustomerLookup = data.DbCustomers
                .GroupBy(c => c.CustomerId)
                .ToDictionary(g => g.Key, g => g.First());

            
            result.DimCustomers = data.ApiSales
                .GroupBy(s => s.CustomerID)
                .Where(g => g.Key > 0) 
                .Select(g =>
                {
                    var sale = g.First();
                    var apiDetail = apiCustomerLookup.GetValueOrDefault(sale.CustomerID);
                    var sqlDetail = sqlCustomerLookup.GetValueOrDefault(sale.CustomerID);

                    string city = sale.CityName;
                    string country = sale.CountryName;

                    if (string.IsNullOrEmpty(city) && sqlDetail?.CityId != null)
                    {
                        city = cityLookup.GetValueOrDefault(sqlDetail.CityId.Value, string.Empty);

                        var dbCity = data.DbCities.FirstOrDefault(c => c.CityId == sqlDetail.CityId);
                        if (dbCity?.CountryId != null)
                            country = countryLookup.GetValueOrDefault(dbCity.CountryId.Value, string.Empty);
                    }

                    return new DimCustomer
                    {
                        CustomerID = sale.CustomerID,
                        FirstName = sale.FirstName ?? string.Empty,
                        LastName = sale.LastName ?? string.Empty,
                        Email = apiDetail?.Email ?? sqlDetail?.Email ?? string.Empty,
                        Phone = apiDetail?.Phone ?? sqlDetail?.Phone ?? string.Empty,
                        City = city ?? string.Empty,
                        Country = country ?? string.Empty
                    };
                })
                .ToList();

           
            var apiCustomerIds = result.DimCustomers.Select(c => c.CustomerID).ToHashSet();
            var sqlOnlyCustomers = data.DbCustomers
                .Where(c => c.CustomerId > 0) 
                .Where(c => !apiCustomerIds.Contains(c.CustomerId))
                .Select(c =>
                {
                    string city = string.Empty;
                    string country = string.Empty;

                    if (c.CityId != null)
                    {
                        city = cityLookup.GetValueOrDefault(c.CityId.Value, string.Empty);

                        var dbCity = data.DbCities.FirstOrDefault(ci => ci.CityId == c.CityId);
                        if (dbCity?.CountryId != null)
                            country = countryLookup.GetValueOrDefault(dbCity.CountryId.Value, string.Empty);
                    }

                    return new DimCustomer
                    {
                        CustomerID = c.CustomerId,
                        FirstName = c.FirstName ?? string.Empty,
                        LastName = c.LastName ?? string.Empty,
                        Email = c.Email ?? string.Empty,
                        Phone = c.Phone ?? string.Empty,
                        City = city,
                        Country = country
                    };
                });

            result.DimCustomers = result.DimCustomers
                .Concat(sqlOnlyCustomers)
                .ToList();

           
            var apiProducts = data.ApiSales
                .Where(a => a.ProductID > 0 && !string.IsNullOrEmpty(a.ProductName))
                .Select(a => new DimProduct
                {
                    ProductID = a.ProductID,
                    ProductName = a.ProductName ?? string.Empty,
                    Category = a.CategoryName ?? string.Empty,
                    Price = a.Price
                });

            var csvProducts = data.CsvProducts
                .Where(c => c.ProductID > 0 && !string.IsNullOrEmpty(c.ProductName))
                .Select(c => new DimProduct
                {
                    ProductID = c.ProductID,
                    ProductName = c.ProductName ?? string.Empty,
                    Category = c.Category ?? string.Empty,
                    Price = c.Price
                });

            var sqlProducts = data.DbProducts
                .Where(p => p.ProductId > 0 && !string.IsNullOrEmpty(p.ProductName))
                .Select(p => new DimProduct
                {
                    ProductID = p.ProductId,
                    ProductName = p.ProductName ?? string.Empty,
                    Category = string.Empty,
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
                           where o.CustomerId > 0 && od.ProductId > 0 
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
                           where o.CustomerID > 0 && od.ProductID > 0 
                           select new FactSales
                           {
                               OrderID = o.OrderID,
                               CustomerID = o.CustomerID,
                               ProductID = od.ProductID,
                               Quantity = od.Quantity,
                               TotalPrice = od.TotalPrice,
                               DateID = int.Parse(o.OrderDate.ToString("yyyyMMdd"))
                           };

            var apiFacts = data.ApiSales
                .Where(s => s.CustomerID > 0 && s.ProductID > 0) 
                .Select(s => new FactSales
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
                .GroupBy(f => new { f.OrderID, f.ProductID })
                .Select(g => g.First())
                .ToList();

            return result;
        }
    }
}