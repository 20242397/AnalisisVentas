
using Microsoft.Extensions.Configuration;
using System.Data;

namespace AnalisisVentas.Data.Persistence
{
    public class DapperContext
    {
        private readonly IConfiguration _configuration;

        public DapperContext(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IDbConnection CreateSourceConnection()
        {
            var connectionString = _configuration.GetConnectionString("SourceDb");
            return new DbConnectionFactory(connectionString).CreateConnection();
        }

        public IDbConnection CreateDwhConnection()
        {
            var connectionString = _configuration.GetConnectionString("DWH");
            return new DbConnectionFactory(connectionString).CreateConnection();
        }
    }
}