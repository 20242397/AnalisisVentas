
using AnalisisVentas.Data.Entities.Dwh.Dimensiones;
using AnalisisVentas.Data.Entities.Dwh.Dimensions;
using AnalisisVentas.Data.Entities.Dwh.Facts;

namespace AnalisisVentas.Data.Interfaces
{
    public class TransformedData
    {
        public List<DimCustomer> DimCustomers { get; set; } = new();
        public List<DimProduct> DimProducts { get; set; } = new();
        public List<DimDate> DimDates { get; set; } = new();

        public List<FactSales> FactSales { get; set; } = new();
    }
}