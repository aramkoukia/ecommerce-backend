using EcommerceApi;
using EcommerceApi.PaymentPlatform;
using EcommerceApi.Untilities;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
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
            var result = await sut.TransactionRequest(
                new TransactionRequest
                {
                    ApiToken = "example_apiToken",
                    PostbackUrl = "https://example.client.url",
                    Request = new Request {
                       Amount = "1",
                       OrderId = "1",
                    },
                    StoreId = "example_storeId",
                    TerminalId = "example_terminalId",
                    TxnType = "example_txnType",
                }
            );
            Assert.NotNull(result);
            // Moner
        }
    }
}
