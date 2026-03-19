

namespace AnalisisVentas.Data.Entities.Dwh.Facts
{
    public class FactSales
    {
        public int SaleID { get; set; } // Identity en SQL

        public int OrderID { get; set; }

        public int CustomerID { get; set; }
        public int ProductID { get; set; }
        public int DateID { get; set; }

        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
