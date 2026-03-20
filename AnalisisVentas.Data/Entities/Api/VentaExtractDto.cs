using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalisisVentas.Data.Entities.Api
{
    public class VentaExtractDto
    {
        
        public int OrderID { get; set; }
        public int CustomerID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CityName { get; set; }
        public string CountryName { get; set; }
        public DateTime OrderDate { get; set; }


        
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string CategoryName { get; set; }

        

        public int Quantity { get; set; }
        public decimal Price { get; set; } // Viene de Products
        public decimal TotalPrice { get; set; } // Viene de OrderDetails
    }
}
