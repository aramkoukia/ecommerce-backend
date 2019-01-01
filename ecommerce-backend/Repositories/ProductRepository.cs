using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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
	                                       ISNULL(Loc2.Balance,0) As AbbotsfordBalance,
                                           ISNULL(Loc1.BinCode,'') AS VancouverBinCode,
                                           ISNULL(Loc2.BinCode,'') AS AbbotsfordBinCode
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

        public async Task<ProductViewModel> GetProduct(int productId)
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
	                                       ISNULL(Loc2.Balance,0) As AbbotsfordBalance,
                                           ISNULL(Loc1.BinCode,'') AS VancouverBinCode,
                                           ISNULL(Loc2.BinCode,'') AS AbbotsfordBinCode
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
                                    ON Loc2.ProductId = Product.ProductId
                                    AND Product.ProductId = @ProductId";
                conn.Open();
                return await conn.QueryFirstAsync<ProductViewModel>(query, new { ProductId = productId });
            }
        }

        public async Task<IEnumerable<ProductTransactionViewModel>> GetProductTransactions(int productId)
        {
            using (IDbConnection conn = Connection)
            {
                string query = $@"
SELECT * FROM (
SELECT OrderDate AS Date, 'Order' AS TransactionType, (-1 * OrderDetail.Amount) AS Amount, Location.LocationName, CreatedByUserId As UserName FROM [Order]
INNER JOIN OrderDetail
	On [Order].OrderId = OrderDetail.OrderId
INNER JOIN Location
	ON Location.LocationId = [Order].LocationId
WHERE ProductId = @ProductId 
      AND Status IN ('Paid', 'Account')

UNION ALL 

SELECT OrderDate, 'Returned Order' AS TransactionType, OrderDetail.Amount, Location.LocationName, CreatedByUserId As [User] FROM [Order]
INNER JOIN OrderDetail
	On [Order].OrderId = OrderDetail.OrderId
INNER JOIN Location
	ON Location.LocationId = [Order].LocationId
WHERE ProductId = @ProductId 
      AND Status IN ('Return') 

UNION ALL 

SELECT PurchaseDate, 'Purchase' AS TransactionType, PurchaseDetail.Amount, '', CreatedByUserId As [User] FROM [Purchase]
INNER JOIN PurchaseDetail
	On Purchase.PurchaseId = PurchaseDetail.PurchaseId
WHERE ProductId = @ProductId

UNION ALL 

SELECT ModifiedDate, 'Inventory Adjustment', Balance, Location.LocationName, CreatedByUserId
FROM ProductInventoryHistory
INNER JOIN Location
	ON Location.LocationId = ProductInventoryHistory.LocationId
WHERE ProductId = @ProductId
) Transactions 
Order By Date Desc
";
                conn.Open();
                return await conn.QueryAsync<ProductTransactionViewModel>(query, new { ProductId = productId });
            }
        }
    }
}
