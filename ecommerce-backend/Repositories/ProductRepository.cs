using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using EcommerceApi.Models;
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
	    ISNULL(Loc1OnHold.OnHold,0) As VancouverOnHold,
	    ISNULL(Loc2OnHold.OnHold,0) As AbbotsfordOnHold,
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
ON Loc2.ProductId = Product.ProductId
LEFT JOIN ( 
	SELECT SUM(OrderDetail.Amount) AS OnHold, ProductId
	FROM [Order]
	INNER JOIN OrderDetail
		ON OrderDetail.OrderId = [Order].OrderId
	WHERE [Order].LocationId = 1
            AND [Order].Status = 'OnHold'
	GROUP BY ProductId
) Loc1OnHold
ON Loc1OnHold.ProductId = Product.ProductId
LEFT JOIN (
	SELECT SUM(OrderDetail.Amount) AS OnHold, ProductId
	FROM [Order]
	INNER JOIN OrderDetail
		ON OrderDetail.OrderId = [Order].OrderId
	WHERE [Order].LocationId = 2
            AND [Order].Status = 'OnHold'
	GROUP BY ProductId
) Loc2OnHold
ON Loc2OnHold.ProductId = Product.ProductId
";
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
WHERE Disabled = 0 ; 

SELECT * FROM ProductPackage;
";

                // WHERE SalesPrice > 0";
                conn.Open();
                var result = await conn.QueryMultipleAsync(query);

                var products = result.Read<ProductViewModel>().ToList();
                var packages = result.Read<ProductPackage>().ToList(); //(Location will have that extra CourseId on it for the next part)
                var distinctProductIdsWithPackage = packages.Select(d => d.ProductId).Distinct();
                foreach (var productId in distinctProductIdsWithPackage)
                {
                    products.FirstOrDefault(p => p.ProductId == productId)
                        ?.ProductPackages.AddRange(packages.Where(p => p.ProductId == productId));
                }

                return products;
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
	    ISNULL(Loc1OnHold.OnHold,0) As VancouverOnHold,
	    ISNULL(Loc2OnHold.OnHold,0) As AbbotsfordOnHold,
        Product.Disabled
FROM Product
INNER JOIN ProductType
ON Product.ProductTypeId = ProductType.ProductTypeId
LEFT JOIN (
    SELECT * 
	FROM ProductInventory
    WHERE ProductId = @ProductId 
	      AND LocationId = 1
) Loc1
ON Loc1.ProductId = Product.ProductId
LEFT JOIN (
    SELECT * 
	FROM ProductInventory
    WHERE ProductId = @ProductId 
	      AND LocationId = 2
) Loc2 
ON Loc2.ProductId = Product.ProductId
LEFT JOIN ( 
	SELECT SUM(OrderDetail.Amount) AS OnHold, ProductId
	FROM [Order]
	INNER JOIN OrderDetail
		ON OrderDetail.OrderId = [Order].OrderId
	WHERE [Order].LocationId = 1
           AND ProductId = @ProductId
           AND [Order].Status = 'OnHold'
	GROUP BY ProductId
) Loc1OnHold
ON Loc1OnHold.ProductId = Product.ProductId
LEFT JOIN (
	SELECT SUM(OrderDetail.Amount) AS OnHold, ProductId
	FROM [Order]
	INNER JOIN OrderDetail
		ON OrderDetail.OrderId = [Order].OrderId
	WHERE [Order].LocationId = 2
            AND [Order].Status = 'OnHold'
			AND ProductId = @ProductId
	GROUP BY ProductId
) Loc2OnHold
ON Loc2OnHold.ProductId = Product.ProductId
WHERE Product.ProductId = @ProductId
";
                conn.Open();
                return await conn.QueryFirstAsync<ProductViewModel>(query, new { ProductId = productId });
            }
        }

        public async Task<IEnumerable<ProductTransactionViewModel>> GetProductTransactions(int productId, DateTime fromDate, DateTime toDate, string userId, int locationId)
        {
            using (IDbConnection conn = Connection)
            {
                string query = $@"
SELECT ModifiedDate AS Date, TransactionType, Balance AS Amount, Location.LocationName, Users.GivenName AS UserName, ProductInventoryHistory.Notes, ChangedBalance as Balance
FROM ProductInventoryHistory
INNER JOIN Location
	ON Location.LocationId = ProductInventoryHistory.LocationId
LEFT JOIN Users
    ON Users.Email = ProductInventoryHistory.CreatedByUserId
WHERE ProductId = @productId
      AND ModifiedDate BETWEEN @fromDate AND @toDate
      AND ProductInventoryHistory.LocationId IN @locIds
      AND Balance <> 0
Order By [Date] Desc
";
                conn.Open();
                var locationIds = new List<int>();
                if (locationId == 0)
                {
                    locationIds = (await GetUserLocations(userId)).ToList();
                }
                else
                {
                    locationIds.Add(locationId);
                }

                var locIds = locationIds.ToArray();
                return await conn.QueryAsync<ProductTransactionViewModel>(query, new { productId, fromDate, toDate, locIds });
            }
        }

        private async Task<IEnumerable<int>> GetUserLocations(string userId)
        {
            using (IDbConnection conn = Connection)
            {
                string query = $@"
SELECT LocationId FROM UserLocation
WHERE UserId = @userId";
                conn.Open();
                return await conn.QueryAsync<int>(query, new { userId });
            }
        }
    }
}
