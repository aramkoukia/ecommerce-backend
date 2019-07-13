using EcommerceApi;
using EcommerceApi.PaymentPlatform;
using EcommerceApi.Untilities;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
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
                       // .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);
                })
                .Build();
            _serviceProvider = new DependencyResolver(webHost);
        }

        [Fact]
        public void TestMonerisService()
        {
            var sut = _serviceProvider.GetService<MonerisService>();

            Assert.NotNull(sut);
            // Moner
        }
    }
}
