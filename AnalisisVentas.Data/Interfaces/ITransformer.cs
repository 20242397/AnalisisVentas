using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalisisVentas.Data.Interfaces
{
        public interface ITransformer
        {
            TransformedData Transform(ExtractedData data);
        }
    
}
