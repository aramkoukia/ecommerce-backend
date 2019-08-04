using EcommerceApi;
using EcommerceApi.Services.PaymentPlatform;
using EcommerceApi.Untilities;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Xunit;

namespace IntegrationTests
{
    public class MonerisIntegrationTests
    {
        private DependencyResolver _serviceProvider;

        public MonerisIntegrationTests()
        {
            var webHost = WebHost.CreateDefaultBuilder()
                .UseStartup<Startup>()
                .ConfigureAppConfiguration((builderContext, config) =>
                {
                    IHostingEnvironment env = builderContext.HostingEnvironment;

                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .Build();
            _serviceProvider = new DependencyResolver(webHost);
        }

        [Fact]
        public async Task TestMonerisService()
        {
            var sut = _serviceProvider.GetService<MonerisService>();
            //var result = await sut.TransactionRequestAsync(
            //    new MonerisRequest
            //    {
            //        ApiToken = "7k7ZyQxg68sGUFCMDg4J",
            //        PostbackUrl = "https://lightsandpartsapi-staging.azurewebsites.net/api/moneris",
            //        Request = new Request {
            //           Amount = "1.0",
            //           OrderId = "1",
            //        },
            //        StoreId = "monca03695",
            //        TerminalId = "example_terminalId",
            //        TxnType = "purchase",
            //    }
            //);
            //Assert.NotNull(result);
        }
    }
}
