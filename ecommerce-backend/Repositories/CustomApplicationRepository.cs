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
    public class CustomApplicationRepository : ICustomApplicationRepository
    {
        private readonly IConfiguration _config;

        public IDbConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("defaultConnection"));
            }
        }

        public CustomApplicationRepository(IConfiguration config)
        {
            _config = config;
        }

        public async Task<IEnumerable<CustomApplicationViewModel>> GetCustomApplicationSteps()
        {
            using (IDbConnection conn = Connection)
            {
                string query = $@"
SELECT * 
FROM ApplicationStep;

SELECT * 
FROM ApplicationStepDetail;

SELECT ApplicationStepDetailTag.*, Tag.TagName
FROM ApplicationStepDetailTag
INNER JOIN Tag
	ON Tag.TagId = ApplicationStepDetailTag.TagId;
";
                conn.Open();
                var result = await conn.QueryMultipleAsync(query);
                var steps = result.Read<CustomApplicationViewModel>().ToList();
                var stepDetails = result.Read<CustomApplicationDetail>().ToList();
                var stepDetailTags = result.Read<CustomApplicationStepDetailTag>().ToList();
                foreach (var step in steps)
                {
                    step.StepDetails = stepDetails.Where(s => s.ApplicationStepId == step.ApplicationStepId).ToList();
                }
                foreach (var stepDetail in stepDetails)
                {
                    stepDetail.Tags = stepDetailTags.Where(s => s.ApplicationStepDetailId == stepDetail.ApplicationStepDetailId).ToList();
                }

                return steps;
            }
        }

        public async Task<IEnumerable<WebsiteProductsInCategoryViewModel>> GetCustomApplicationResult(string[] selectedOptions)
        {
            // selectedOptions
            // todo: query step ids with the names passed in.
            var ids = new int[1, 2, 5, 8];
            using (IDbConnection conn = Connection)
            {
                string query = $@"
SELECT DISTINCT ProductCode, ProductName, SlugsUrl, Detail, Description, WarrantyInformation, AdditionalInformation
FROM Product
INNER JOIN ProductTag
	ON Product.ProductId = ProductTag.ProductId
LEFT JOIN ProductWebsite
	ON ProductWebsite.ProductId = Product.ProductId
WHERE TagId IN (
	SELECT DISTINCT TagId 
	FROM ApplicationStepDetailTag
	INNER JOIN ApplicationStepDetail
	ON ApplicationStepDetailTag.ApplicationStepDetailId = ApplicationStepDetail.ApplicationStepDetailId
	WHERE StepDetailTitle IN @selectedOptions
)
";
                conn.Open();
                return await conn.QueryAsync<WebsiteProductsInCategoryViewModel>(query, new { selectedOptions });
            }
        }
    }
}
