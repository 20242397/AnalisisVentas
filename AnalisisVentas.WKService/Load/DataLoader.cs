using AnalisisVentas.Data.Persistence;
using AnalisisVentas.Data.Interfaces;
using Dapper;
using System.Data;

namespace AnalisisInventario.WorkerService.Load
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
            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                // 🔹 LIMPIAR TABLAS (orden correcto por FK)
                await connection.ExecuteAsync("DELETE FROM FactSales", transaction: transaction);
                await connection.ExecuteAsync("DELETE FROM DimDate", transaction: transaction);
                await connection.ExecuteAsync("DELETE FROM DimProduct", transaction: transaction);
                await connection.ExecuteAsync("DELETE FROM DimCustomer", transaction: transaction);

                // 🔹 INSERT MASIVO (Dapper soporta listas)
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
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Console.WriteLine($"❌ Error en Load: {ex.Message}");
                throw;
            }
        }
    }
}