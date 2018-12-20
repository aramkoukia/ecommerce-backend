using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace EcommerceApi.Models
{
    public class EcommerceContextFactory : IDesignTimeDbContextFactory<EcommerceContext>
    {
        public EcommerceContext CreateDbContext(string[] args)
        {
            var config = new ConfigurationBuilder()
                  .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                  .AddJsonFile("appsettings.json")
                  .Build();

            var options = new DbContextOptionsBuilder<EcommerceContext>();
            options.UseSqlServer(config.GetConnectionString("defaultConnection"));

            return new EcommerceContext(options.Options);
        }
    }
}
