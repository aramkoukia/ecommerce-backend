using System;
using System.Collections.Generic;
using Microsoft.Net.Http;
using System.Threading.Tasks;
using System.Net.Http;

namespace EcommerceApi.PaymentPlatform
{
    public class MonerisService
    {
        public HttpClient Client { get; }

        public MonerisService(HttpClient client)
        {
            client.BaseAddress = new Uri("https://api.github.com/");
            // GitHub API versioning
            client.DefaultRequestHeaders.Add("Accept",
                "application/vnd.github.v3+json");
            // GitHub requires a user-agent
            client.DefaultRequestHeaders.Add("User-Agent",
                "HttpClientFactory-Sample");

            Client = client;
        }

        public async Task<IEnumerable<GitHubIssue>> GetAspNetDocsIssues()
        {
            var response = await Client.GetAsync(
                "/repos/aspnet/AspNetCore.Docs/issues?state=open&sort=created&direction=desc");

            response.EnsureSuccessStatusCode();

            var result = await response.Content
                .ReadAsAsync<IEnumerable<GitHubIssue>>();

            return result;
        }
    }
}
