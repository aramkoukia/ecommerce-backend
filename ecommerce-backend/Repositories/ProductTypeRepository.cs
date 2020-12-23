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
SELECT Product.ProductId, ProductCode, ProductName, ProductTypeName, Product.ProductDescription, 0 AS Rank, Images.ImagePath, ProductWebsite.SlugsUrl,
       'In Stock' AS Balance
FROM Product
INNER JOIN ProductType
ON Product.ProductTypeId = ProductType.ProductTypeId
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
WHERE Product.Disabled = 0
      AND ProductType.SlugsUrl = @slugsUrl
";
                conn.Open();
                return await conn.QueryAsync<WebsiteProductsInCategoryViewModel>(query, new { slugsUrl });
            }
        }

        public async Task<IEnumerable<WebsiteProductsInCategoryViewModel>> GetWebsiteProducts()
        {
            using (IDbConnection conn = Connection)
            {
                string query = $@"
SELECT TOP 10 Product.ProductId, ProductCode, ProductName, ProductTypeName, Product.ProductDescription, 0 AS Rank, Images.ImagePath, ProductWebsite.SlugsUrl,
       'In Stock'AS Balance
FROM Product
INNER JOIN ProductType
ON Product.ProductTypeId = ProductType.ProductTypeId
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
WHERE Product.Disabled = 0
      AND ImagePath IS NOT NULL
";
                conn.Open();
                return await conn.QueryAsync<WebsiteProductsInCategoryViewModel>(query);
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
	   Images.ImagePath, 
	   ProductWebsite.SlugsUrl,
	   ProductWebsite.Description,
	   ProductWebsite.Detail,
	   ProductWebsite.SlugsUrl,
	   ProductWebsite.UserManualPath,
	   ProductWebsite.WarrantyInformation,
	   ProductWebsite.AdditionalInformation,
       'In Stock'AS Balance
FROM Product
INNER JOIN ProductType
ON Product.ProductTypeId = ProductType.ProductTypeId
LEFT JOIN (
    SELECT TOP 1 ProductWebsiteImage.ProductId, ImagePath
    FROM ProductWebsiteImage
    INNER JOIN ProductWebsite
	    ON ProductWebsiteImage.ProductId = ProductWebsite.ProductId
    AND ProductWebsite.SlugsUrl = @slugsUrl
) Images
ON Images.ProductId = Product.ProductId
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
