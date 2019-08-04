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
            var transactionRequest = new TransactionRequest
            {
                OrderId = 1234,
                Amount = 10,
                ClientIp = "::1",
                TransactionType = TransactionType.purchase.ToString()
            };

            var result = await sut.TransactionRequestAsync(transactionRequest);
            Assert.NotNull(result);
        }
    }
}
