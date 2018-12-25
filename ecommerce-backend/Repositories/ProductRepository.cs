using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using EcommerceApi.ViewModel;
using Microsoft.Extensions.Configuration;

namespace EcommerceApi.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly IConfiguration _config;

        public IDbConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("defaultConnection"));
            }
        }

        public ProductRepository(IConfiguration config)
        {
            _config = config;
        }

        public async Task<IEnumerable<ProductViewModel>> GetProducts()
        {
            using (IDbConnection conn = Connection)
            {
                string query = $@"
                                    SELECT Product.ProductId, 
                                           ProductCode, 
	                                       ProductName, 
	                                       ChargeTaxes, 
	                                       AllowOutOfStockPurchase, 
	                                       SalesPrice, 
	                                       PurchasePrice, 
	                                       Product.ModifiedDate, 
	                                       Product.ProductTypeId, 
	                                       ProductType.ProductTypeName,
	                                       ISNULL(Loc1.Balance,0) As VancouverBalance,
	                                       ISNULL(Loc2.Balance,0) As AbbotsfordBalance
                                    FROM Product
                                    INNER JOIN ProductType
                                    ON Product.ProductTypeId = ProductType.ProductTypeId
                                    LEFT JOIN (
                                      SELECT * FROM ProductInventory
                                      WHERE LocationId = 1
                                    ) Loc1
                                    ON Loc1.ProductId = Product.ProductId
                                    LEFT JOIN (
                                      SELECT * FROM ProductInventory
                                      WHERE LocationId = 2
                                    ) Loc2 
                                    ON Loc2.ProductId = Product.ProductId";
                conn.Open();
                return await conn.QueryAsync<ProductViewModel>(query);
            }
        }
    }
}
