using System;
using System.Collections.Generic;
using Microsoft.Net.Http;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Formatting;

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

        public async Task<ValidationResponse> TransactionRequest(TransactionRequest transactionRequest)
        {
            var response = await Client.PostAsync(
                "/Terminal", 
                transactionRequest,
                new JsonMediaTypeFormatter());

            response.EnsureSuccessStatusCode();

            var result = await response.Content
                .ReadAsAsync<ValidationResponse>();

            return result;
        }
    }
}
