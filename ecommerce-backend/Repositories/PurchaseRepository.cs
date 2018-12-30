using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using EcommerceApi.ViewModel;
using Microsoft.Extensions.Configuration;

namespace EcommerceApi.Repositories
{
    public class PurchaseRepository : IPurchaseRepository
    {
        private readonly IConfiguration _config;

        public IDbConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("defaultConnection"));
            }
        }

        public PurchaseRepository(IConfiguration config)
        {
            _config = config;
        }

        public async Task<IEnumerable<PurchaseViewModel>> GetPurchases()
        {
            using (IDbConnection conn = Connection)
            {
                string query = $@"
                                    SELECT [Purchase].[PurchaseId]
                                          ,[CustomerId]
                                          ,[Purchase].[LocationId]
                                          ,[PurchaseDate]
                                          ,[Total]
                                          ,[SubTotal]
                                          ,[Notes]
                                          ,[DeliveryDate]
                                          ,[CreatedByUserId]
	                                      Location.LocationName
                                    FROM [Purchase]
                                 ";
                conn.Open();
                return await conn.QueryAsync<PurchaseViewModel>(query);
            }
        }
    }
}
