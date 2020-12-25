using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
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
SELECT ProductType.*, ProductCount, 0 AS Rank  
FROM ProductType
INNER JOIN (
  SELECT ProductTypeId, COUNT(*) AS ProductCount
  FROM Product
  WHERE Product.Disabled = 0
  GROUP BY ProductTypeId
) Product
ON Product.ProductTypeId = ProductType.ProductTypeId
WHERE ProductType.Disabled = 0
";
                conn.Open();
                return await conn.QueryAsync<WebsiteProductTypeViewModel>(query);
            }
        }

        public async Task<WebsiteProductTypeViewModel> GetWebsiteProductType(string slugsUrl)
        {
            using (IDbConnection conn = Connection)
            {
                string query = $@"SELECT ProductType.* FROM ProductType WHERE ProductType.SlugsUrl = @slugsUrl";
                conn.Open();
                return await conn.QueryFirstAsync<WebsiteProductTypeViewModel>(query, new { slugsUrl });
            }
        }

        public async Task<IEnumerable<ProductTypeViewModel>> GetProductTypes()
        {
            using (IDbConnection conn = Connection)
            {
                string query = $@"
SELECT ProductType.*, ProductCount, 0 AS Rank  
FROM ProductType
LEFT JOIN (
  SELECT ProductTypeId, COUNT(*) AS ProductCount
  FROM Product
  WHERE Product.Disabled = 0
  GROUP BY ProductTypeId
) Product
ON Product.ProductTypeId = ProductType.ProductTypeId
WHERE ShowOnWebsite = 1
      AND Disabled = 0
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
SELECT Product.ProductId, ProductCode, ProductName, ProductTypeName, Product.ProductDescription, 0 AS Rank, ProductWebsite.SlugsUrl, 'In Stock' AS Balance
FROM Product
INNER JOIN ProductType
  ON Product.ProductTypeId = ProductType.ProductTypeId
LEFT JOIN ProductWebsite
  ON ProductWebsite.ProductId = Product.ProductId
WHERE Product.Disabled = 0
      AND (ProductType.SlugsUrl = @slugsUrl or @slugsUrl = 'all-categories')

SELECT DISTINCT ProductWebsite.ProductId, ImagePath
FROM ProductWebsiteImage
INNER JOIN ProductWebsite
	ON ProductWebsiteImage.ProductId = ProductWebsite.ProductId
AND ProductWebsite.SlugsUrl = @slugsUrl
";
                conn.Open();

                var result = await conn.QueryMultipleAsync(query, new { slugsUrl });

                var products = result.Read<WebsiteProductsInCategoryViewModel>().ToList();
                var images = result.Read<ProductImage>().ToList();

                foreach (var product in products)
                {
                    product.Images.AddRange(images.Where(p => p.ProductId == product.ProductId));
                }
                return products;
            }
        }

        public async Task<IEnumerable<WebsiteProductsInCategoryViewModel>> GetWebsiteProducts()
        {
            using (IDbConnection conn = Connection)
            {
                string query = $@"
SELECT ProductCode, 
	   ProductName, 
	   ProductTypeName, 
	   ProductWebsite.SlugsUrl, 
       'In Stock' AS Balance
FROM Product
INNER JOIN ProductType
ON Product.ProductTypeId = ProductType.ProductTypeId
LEFT JOIN ProductWebsite
ON ProductWebsite.ProductId = Product.ProductId
WHERE Product.Disabled = 0
";
                conn.Open();
                return await conn.QueryAsync<WebsiteProductsInCategoryViewModel>(query);
            }
        }

        public async Task<IEnumerable<WebsiteProductsInCategoryViewModel>> GetWebsiteProductDetails()
        {
            using (IDbConnection conn = Connection)
            {
                string query = $@"
SELECT Product.ProductId, 
       ProductCode, 
	   ProductName, 
	   ProductTypeName, 
	   ProductWebsite.SlugsUrl, 
	   ProductWebsite.Description, 
	   ProductWebsite.WarrantyInformation, 
	   ProductWebsite.UserManualPath, 
	   ProductWebsite.HeaderImagePath,
       'In Stock' AS Balance
FROM Product
INNER JOIN ProductType
ON Product.ProductTypeId = ProductType.ProductTypeId
LEFT JOIN ProductWebsite
ON ProductWebsite.ProductId = Product.ProductId
WHERE Product.Disabled = 0

SELECT ProductWebsite.ProductId, ImagePath
FROM ProductWebsiteImage
INNER JOIN ProductWebsite
	ON ProductWebsiteImage.ProductId = ProductWebsite.ProductId
ORDER BY ProductWebsite.ProductId DESC
";
                conn.Open();
                var result = await conn.QueryMultipleAsync(query);
                var products = result.Read<WebsiteProductsInCategoryViewModel>().ToList();
                var images = result.Read<ProductImage>().ToList();

                foreach (var product in products)
                {
                    product.Images.AddRange(images.Where(p => p.ProductId == product.ProductId));
                }
                return products;
            }
        }

        public async Task<IEnumerable<WebsiteProductsInCategoryViewModel>> GetPopularWebsiteProducts()
        {
            using (IDbConnection conn = Connection)
            {
                string query = $@"
SELECT top 20 Product.ProductId, 
       ProductCode, ProductName, ProductTypeName, Product.ProductDescription, RANK() OVER (ORDER BY Sales.Total DESC) AS Rank, ProductWebsite.SlugsUrl,
       'In Stock' AS Balance
FROM Product
INNER JOIN ProductType
ON Product.ProductTypeId = ProductType.ProductTypeId
LEFT JOIN ProductWebsite
ON ProductWebsite.ProductId = Product.ProductId
LEFT JOIN (
	SELECT top 20 Product.ProductId, SUM(OrderDetail.Total) AS Total
	FROM OrderDetail
	INNER JOIN [Order]
	  ON [Order].OrderId = OrderDetail.OrderId
	INNER JOIN Product
	  ON OrderDetail.ProductId = Product.ProductId
	WHERE [Order].OrderDate >= DATEADD(MONTH, -6, GETDATE())
	GROUP BY Product.ProductId
	ORDER BY SUM(OrderDetail.Total) DESC
) Sales
ON Product.ProductId = Sales.ProductId
WHERE Product.Disabled = 0
      AND Product.ProductId in ( SELECT ProductId FROM ProductWebsiteImage )

SELECT ProductWebsite.ProductId, ImagePath
FROM ProductWebsiteImage
INNER JOIN ProductWebsite
	ON ProductWebsiteImage.ProductId = ProductWebsite.ProductId
";
                conn.Open();
                var result = await conn.QueryMultipleAsync(query);
                var products = result.Read<WebsiteProductsInCategoryViewModel>().ToList();
                var images = result.Read<ProductImage>().ToList();

                foreach (var product in products)
                {
                    product.Images.AddRange(images.Where(p => p.ProductId == product.ProductId));
                }
                return products;
            }
        }

        public async Task<IEnumerable<WebsiteProductsInCategoryViewModel>> GetNewWebsiteProducts()
        {
            using (IDbConnection conn = Connection)
            {
                string query = $@"
SELECT TOP 20 Product.ProductId, ProductCode, ProductName, ProductTypeName, Product.ProductDescription, 0 AS Rank, ProductWebsite.SlugsUrl,
       'In Stock'AS Balance
FROM Product
INNER JOIN ProductType
ON Product.ProductTypeId = ProductType.ProductTypeId
LEFT JOIN ProductWebsite
ON ProductWebsite.ProductId = Product.ProductId
WHERE Product.Disabled = 0
      AND Product.ProductId in ( SELECT ProductId FROM ProductWebsiteImage )
ORDER BY ProductId DESC

SELECT DISTINCT TOP 100 ProductWebsite.ProductId, ImagePath
FROM ProductWebsiteImage
INNER JOIN ProductWebsite
	ON ProductWebsiteImage.ProductId = ProductWebsite.ProductId
ORDER BY ProductWebsite.ProductId DESC
";
                conn.Open();
                var result = await conn.QueryMultipleAsync(query);
                var products = result.Read<WebsiteProductsInCategoryViewModel>().ToList();
                var images = result.Read<ProductImage>().ToList();

                foreach (var product in products)
                {
                    product.Images.AddRange(images.Where(p => p.ProductId == product.ProductId));
                }
                return products;
            }
        }

        public async Task<IEnumerable<string>> GetWebsiteProductSlugs()
        {
            using (IDbConnection conn = Connection)
            {
                string query = $@"
SELECT distinct ProductWebsite.SlugsUrl
FROM Product
LEFT JOIN ProductWebsite
ON ProductWebsite.ProductId = Product.ProductId
WHERE Product.Disabled = 0
";
                conn.Open();
                return await conn.QueryAsync<string>(query);
            }
        }

        public async Task<WebsiteProductViewModel> GetWebsiteProduct(string slugsUrl)
        {
            using (IDbConnection conn = Connection)
            {
                string query = $@"
SELECT TOP 1 Product.ProductId, 
       ProductCode, 
       ProductName, 
	   ProductTypeName, Product.ProductDescription, 
	   0 AS Rank, 
	   ProductWebsite.SlugsUrl,
	   ProductWebsite.Description,
	   ProductWebsite.Detail,
	   ProductWebsite.SlugsUrl,
	   ProductWebsite.UserManualPath,
	   ProductWebsite.WarrantyInformation,
	   ProductWebsite.AdditionalInformation,
       ProductWebsite.HeaderImagePath,
       'In Stock'AS Balance
FROM Product
INNER JOIN ProductType
ON Product.ProductTypeId = ProductType.ProductTypeId
LEFT JOIN ProductWebsite
ON ProductWebsite.ProductId = Product.ProductId
WHERE Product.Disabled = 0
AND ProductWebsite.SlugsUrl = @slugsUrl;

SELECT DISTINCT ImagePath
FROM ProductWebsiteImage
INNER JOIN ProductWebsite
	ON ProductWebsiteImage.ProductId = ProductWebsite.ProductId
AND ProductWebsite.SlugsUrl = @slugsUrl
";
                conn.Open();
                var result = await conn.QueryMultipleAsync(query, new { slugsUrl });
                var product = result.Read<WebsiteProductViewModel>().ToList().FirstOrDefault();
                var images = result.Read<string>().ToList();
                product.ImagePaths = images.ToArray();
                return product;
            }
        }
    }
}
