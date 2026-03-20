using AnalisisVentas.Data.Entities.Api;
using AnalisisVentas.Data.Entities.Csv;
using AnalisisVentas.Data.Interfaces.AnalisisVentas.Data.Interfaces;
using SistemaVentas.ETL.App.Models.dboSchema;

namespace AnalisisVentas.Data.Interfaces
{
    public class ExtractedData : IExtractor
    {
       
        public List<ProductCsv> CsvProducts { get; set; } = new();
        public List<CustomerCsv> CsvCustomers { get; set; } = new();
        public List<OrderCsv> CsvOrders { get; set; } = new();
        public List<OrderDetailCsv> CsvOrderDetails { get; set; } = new();

        public List<VentaExtractDto> ApiSales { get; set; } = new();

       
        public List<ProductApiDto> ApiProducts { get; set; } = new();
        public List<CustomerApiDto> ApiCustomers { get; set; } = new();

        
        public List<Customer> DbCustomers { get; set; } = new();
        public List<Product> DbProducts { get; set; } = new();
        public List<Order> DbOrders { get; set; } = new();
        public List<OrderDetail> DbOrderDetails { get; set; } = new();

       

        public async Task<ExtractedData> ExtractAsync()
        {
            return await Task.FromResult(this);
        }
    }
}