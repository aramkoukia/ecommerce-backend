using System;
using System.Collections.Generic;
using Microsoft.Net.Http;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.Extensions.Configuration;

namespace EcommerceApi.PaymentPlatform
{
    public class MonerisService
    {
        public HttpClient Client { get; }
        private readonly IConfiguration _config;

        public MonerisService(HttpClient client, IConfiguration config)
        {
            _config = config;
            client.BaseAddress = new Uri(_config["Moneris:baseUrl"]);
            // GitHub API versioning
            // client.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
            // GitHub requires a user-agent
            // client.DefaultRequestHeaders.Add("User-Agent", "HttpClientFactory-Sample");
            Client = client;
        }

        public async Task<IEnumerable<GitHubIssue>> GetAspNetDocsIssues()
        {
            var response = await Client.GetAsync("/repos/aspnet/AspNetCore.Docs/issues?state=open&sort=created&direction=desc");

            response.EnsureSuccessStatusCode();

            var result = await response.Content
                .ReadAsAsync<IEnumerable<GitHubIssue>>();

            return result;
        }
    }
}
