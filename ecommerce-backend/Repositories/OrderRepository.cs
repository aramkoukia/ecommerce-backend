using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace EcommerceApi.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly IConfiguration _config;

        public IDbConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("defaultConnection"));
            }
        }

        public OrderRepository(IConfiguration config)
        {
            _config = config;
        }
    }
}
