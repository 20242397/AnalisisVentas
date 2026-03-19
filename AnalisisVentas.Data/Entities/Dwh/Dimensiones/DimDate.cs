

namespace AnalisisVentas.Data.Entities.Dwh.Dimensiones
{
    public class DimDate
    {
        public int DateID { get; set; }

        public DateTime FullDate { get; set; }

        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
    }
}
