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
";
                conn.Open();
                var result = await conn.QueryMultipleAsync(query);
                var steps = result.Read<CustomApplicationViewModel>().ToList();
                var stepDetails = result.Read<CustomApplicationDetail>().ToList();
                foreach (var step in steps)
                {
                    step.StepDetails = stepDetails.Where(s => s.ApplicationStepId == step.ApplicationStepId).ToList();
                }
                return steps;
            }
        }
    }
}
