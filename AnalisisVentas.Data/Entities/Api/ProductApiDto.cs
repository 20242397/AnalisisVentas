

namespace AnalisisVentas.Data.Entities.Api
{
    public class ProductApiDto
    {
        public int ProductID { get; set; }

        public string ProductName { get; set; }
        public string Category { get; set; }

        public decimal? Price { get; set; }
    }
}
