using System;
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
                                           ISNULL(Loc2.BinCode,'') AS AbbotsfordBinCode,
                                           Product.Disabled
                                    FROM Product
                                    LEFT JOIN ProductType
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

        public async Task<IEnumerable<ProductViewModel>> GetAvailableProducts()
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
        ISNULL(Loc2.BinCode,'') AS AbbotsfordBinCode,
		ISNULL(OnHoldItems.OnHoldAmount, 0) AS OnHoldAmount
FROM Product
LEFT JOIN ProductType
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
LEFT JOIN (
  SELECT ProductId, SUM(Amount) As OnHoldAmount
  FROM [Order]
  INNER JOIN OrderDetail
	ON [Order].OrderId = OrderDetail.OrderId
  WHERE [Order].Status = 'OnHold'
  GROUP BY ProductId
) AS OnHoldItems
ON OnHoldItems.ProductId = Product.ProductId
WHERE Disabled = 0
";
// WHERE SalesPrice > 0";
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
                                           ISNULL(Loc2.BinCode,'') AS AbbotsfordBinCode,
                                           Product.Disabled
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
                                    WHERE Product.ProductId = @ProductId";
                conn.Open();
                return await conn.QueryFirstAsync<ProductViewModel>(query, new { ProductId = productId });
            }
        }

        public async Task<IEnumerable<ProductTransactionViewModel>> GetProductTransactions(int productId, DateTime fromDate, DateTime toDate)
        {
            using (IDbConnection conn = Connection)
            {
                string query = $@"
declare @productid int = 7549
declare @fromDate datetime = dateadd(day, -7, getdate())
declare @toDate datetime = getdate()

SELECT * FROM (
SELECT OrderDate AS Date, 'Order' AS TransactionType, (-1 * OrderDetail.Amount) AS Amount, Location.LocationName, Users.GivenName As UserName, 'Id: ' + CAST([Order].OrderId AS NVARCHAR(100)) AS Notes
FROM [Order]
INNER JOIN OrderDetail
	On [Order].OrderId = OrderDetail.OrderId
INNER JOIN Location
	ON Location.LocationId = [Order].LocationId
LEFT JOIN Users
    ON Users.Id = [Order].CreatedByUserId
WHERE ProductId = @ProductId 
      AND OrderDate BETWEEN @fromDate AND @toDate
	  AND Status IN ('Paid', 'Account')

UNION ALL 

SELECT OrderDate, 'Returned Order' AS TransactionType, OrderDetail.Amount, Location.LocationName, Users.GivenName, 'Id: ' + CAST([Order].OrderId AS NVARCHAR(100)) AS Notes
FROM [Order]
INNER JOIN OrderDetail
	On [Order].OrderId = OrderDetail.OrderId
INNER JOIN Location
	ON Location.LocationId = [Order].LocationId
LEFT JOIN Users
    ON Users.Id = [Order].CreatedByUserId
WHERE ProductId = @ProductId 
      AND OrderDate BETWEEN @fromDate AND @toDate
      AND Status IN ('Return') 

UNION ALL 

SELECT PurchaseDate, 'Purchase' AS TransactionType, PurchaseDetail.Amount, '', Users.GivenName, 'Id: ' + CAST([Purchase].PurchaseId AS NVARCHAR(100)) AS Notes
FROM [Purchase]
INNER JOIN PurchaseDetail
	On Purchase.PurchaseId = PurchaseDetail.PurchaseId
LEFT JOIN Users
    ON Users.Id = Purchase.CreatedByUserId
WHERE ProductId = @ProductId
      AND PurchaseDate BETWEEN @fromDate AND @toDate

UNION ALL 

SELECT ModifiedDate, TransactionType, Balance, Location.LocationName, Users.GivenName, ProductInventoryHistory.Notes
FROM ProductInventoryHistory
INNER JOIN Location
	ON Location.LocationId = ProductInventoryHistory.LocationId
LEFT JOIN Users
    ON Users.Email = ProductInventoryHistory.CreatedByUserId
WHERE ProductId = @productId
      AND ModifiedDate BETWEEN @fromDate AND @toDate
) Transactions 
Order By [Date] Desc
";
                conn.Open();
                return await conn.QueryAsync<ProductTransactionViewModel>(query, new { productId, fromDate, toDate });
            }
        }
    }
}
