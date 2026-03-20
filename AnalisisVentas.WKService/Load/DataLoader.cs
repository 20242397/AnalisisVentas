using AnalisisVentas.Data.Persistence;
using AnalisisVentas.Data.Interfaces;
using Dapper;
using System.Data;

namespace AnalisisVentas.WKService.Load // Corregido para ser consistente con tus otros namespaces
{
    public class DataLoader : ILoader
    {
        private readonly DapperContext _context;

        public DataLoader(DapperContext context)
        {
            _context = context;
        }

        public async Task LoadAsync(TransformedData data)
        {
            using var connection = _context.CreateDwhConnection();
            if (connection.State != ConnectionState.Open) connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                await connection.ExecuteAsync("DELETE FROM FactSales", transaction: transaction);
                await connection.ExecuteAsync("DELETE FROM DimDate", transaction: transaction);
                await connection.ExecuteAsync("DELETE FROM DimProduct", transaction: transaction);
                await connection.ExecuteAsync("DELETE FROM DimCustomer", transaction: transaction);

                
                await connection.ExecuteAsync(@"
                    INSERT INTO DimCustomer (CustomerID, FirstName, LastName, Email, Phone, City, Country)
                    VALUES (@CustomerID, @FirstName, @LastName, @Email, @Phone, @City, @Country)
                ", data.DimCustomers, transaction);

                await connection.ExecuteAsync(@"
                    INSERT INTO DimProduct (ProductID, ProductName, Category, Price)
                    VALUES (@ProductID, @ProductName, @Category, @Price)
                ", data.DimProducts, transaction);

                await connection.ExecuteAsync(@"
                    INSERT INTO DimDate (DateID, FullDate, Year, Month, Day)
                    VALUES (@DateID, @FullDate, @Year, @Month, @Day)
                ", data.DimDates, transaction);

                
                await connection.ExecuteAsync(@"
                    INSERT INTO FactSales (OrderID, CustomerID, ProductID, DateID, Quantity, TotalPrice)
                    VALUES (@OrderID, @CustomerID, @ProductID, @DateID, @Quantity, @TotalPrice)
                ", data.FactSales, transaction);

                transaction.Commit();
                Console.WriteLine("✅ Carga en Data Warehouse completada con éxito.");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
               
                Console.WriteLine($"❌ Error crítico en el proceso de Carga (Load): {ex.Message}");
                throw;
            }
        }
    }
}