using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalisisVentas.Data.Entities.Csv
{
    public class ProductCsv
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string Category { get; set; } 
        public decimal Price { get; set; }
        public int Stock { get; set; }
    }
}
