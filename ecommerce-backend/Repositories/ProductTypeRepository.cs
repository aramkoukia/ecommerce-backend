using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using EcommerceApi.Models;
using EcommerceApi.ViewModel;
using EcommerceApi.ViewModel.Website;
using Microsoft.Extensions.Configuration;

namespace EcommerceApi.Repositories
{
    public class ProductTypeRepository : IProductTypeRepository
    {
        private readonly IConfiguration _config;

        public IDbConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("defaultConnection"));
            }
        }

        public ProductTypeRepository(IConfiguration config)
        {
            _config = config;
        }

        public async Task<IEnumerable<WebsiteProductTypeViewModel>> GetWebsiteProductTypes()
        {
            using (IDbConnection conn = Connection)
            {
                string query = $@"
SELECT ProductType.*, ProductCount, RANK() OVER (ORDER BY Sales.Total DESC) AS Rank  
FROM ProductType
INNER JOIN (
  SELECT ProductTypeId, COUNT(*) AS ProductCount
  FROM Product
  WHERE Product.Disabled = 0
  GROUP BY ProductTypeId
) Product
ON Product.ProductTypeId = ProductType.ProductTypeId
LEFT JOIN (
	SELECT ProductTypeId, SUM(OrderDetail.Total) AS Total
	FROM OrderDetail
	INNER JOIN [Order]
	  ON [Order].OrderId = OrderDetail.OrderId
	INNER JOIN Product
	  ON OrderDetail.ProductId = Product.ProductId
	WHERE [Order].OrderDate >= DATEADD(MONTH, -6, GETDATE())
	GROUP BY ProductTypeId
) Sales
ON Product.ProductTypeId = Sales.ProductTypeId
WHERE ProductType.Disabled = 0
";
                conn.Open();
                return await conn.QueryAsync<WebsiteProductTypeViewModel>(query);
            }
        }

        public async Task<IEnumerable<ProductTypeViewModel>> GetProductTypes()
        {
            using (IDbConnection conn = Connection)
            {
                string query = $@"
SELECT ProductType.*, ProductCount, RANK() OVER (ORDER BY Sales.Total DESC) AS Rank  
FROM ProductType
LEFT JOIN (
  SELECT ProductTypeId, COUNT(*) AS ProductCount
  FROM Product
  WHERE Product.Disabled = 0
  GROUP BY ProductTypeId
) Product
ON Product.ProductTypeId = ProductType.ProductTypeId
LEFT JOIN (
	SELECT ProductTypeId, SUM(OrderDetail.Total) AS Total
	FROM OrderDetail
	INNER JOIN [Order]
	  ON [Order].OrderId = OrderDetail.OrderId
	INNER JOIN Product
	  ON OrderDetail.ProductId = Product.ProductId
	WHERE [Order].OrderDate >= DATEADD(MONTH, -6, GETDATE())
	GROUP BY ProductTypeId
) Sales
ON Product.ProductTypeId = Sales.ProductTypeId
";
                conn.Open();
                return await conn.QueryAsync<ProductTypeViewModel>(query);
            }
        }

        public async Task<IEnumerable<WebsiteProductsInCategoryViewModel>> GetWebsiteProductsByProductType(string slugsUrl)
        {
            using (IDbConnection conn = Connection)
            {
                string query = $@"
SELECT Product.ProductId, ProductCode, ProductName, ProductTypeName, Product.ProductDescription, RANK() OVER (ORDER BY Sales.Total DESC) AS Rank, Images.ImagePath, ProductWebsite.SlugsUrl,
       CASE WHEN ProductInventory.Balance > 0 THEN 'In Scock' ELSE 'Out Of Stock' END AS Balance
FROM Product
INNER JOIN ProductType
ON Product.ProductTypeId = ProductType.ProductTypeId
LEFT JOIN (
	SELECT Product.ProductId, SUM(OrderDetail.Total) AS Total
	FROM OrderDetail
	INNER JOIN [Order]
	  ON [Order].OrderId = OrderDetail.OrderId
	INNER JOIN Product
	  ON OrderDetail.ProductId = Product.ProductId
	WHERE [Order].OrderDate >= DATEADD(MONTH, -6, GETDATE())
	GROUP BY Product.ProductId
) Sales
ON Product.ProductId = Sales.ProductId
LEFT JOIN (
	SELECT  ProductId, ImagePath
	FROM (
		  SELECT  ProductId, ImagePath, ROW_NUMBER() OVER (PARTITION BY ProductId ORDER BY ProductId) AS rownumber
		  FROM    ProductWebsiteImage
		  ) Images
	WHERE rownumber = 1
) Images
ON Images.ProductId = Product.ProductId
LEFT JOIN ProductWebsite
ON ProductWebsite.ProductId = Product.ProductId
LEFT JOIN (
	SELECT ProductId, SUM(Balance) AS Balance from ProductInventory
	GROUP BY ProductId
) AS ProductInventory
ON ProductInventory.ProductId = Product.ProductId
WHERE Product.Disabled = 0
      AND ProductType.SlugsUrl = @slugsUrl
";
                conn.Open();
                return await conn.QueryAsync<WebsiteProductsInCategoryViewModel>(query, new { slugsUrl });
            }
        }
    }
}
