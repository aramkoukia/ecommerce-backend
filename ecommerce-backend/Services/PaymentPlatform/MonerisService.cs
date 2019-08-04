using System;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Formatting;
using EcommerceApi.Models;
using System.Linq;

namespace EcommerceApi.Services.PaymentPlatform
{
    public class MonerisService : IMonerisService
    {
        public HttpClient Client { get; }
        private readonly IConfiguration _config;
        private readonly EcommerceContext _context;

        public MonerisService(HttpClient client,
                              IConfiguration config,
                              EcommerceContext context)
        {
            _config = config;
            _context = context;
            client.BaseAddress = new Uri(_config["Moneris:baseUrl"]);
            // client.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
            // client.DefaultRequestHeaders.Add("User-Agent", "HttpClientFactory-Sample");
            Client = client;
        }

        public async Task<ValidationResponse> TransactionRequestAsync(TransactionRequest transactionRequest)
        {
            var clientPosSettings = _context.ClientPosSettings.FirstOrDefault(c => c.ClientIp == transactionRequest.ClientIp);
            if (clientPosSettings == null)
            {
                return null; // log error, and don't return null dude!
            }

            var monerisRequest = new MonerisRequest
            {
                ApiToken = _config["Moneris:apiToken"],
                PostbackUrl = _config["Moneris:postbackUrl"],
                StoreId = clientPosSettings.StoreId,
                TerminalId = clientPosSettings.TerminalId,
                TxnType = transactionRequest.TransactionType,
                Request = new Request
                {
                    Amount = transactionRequest.Amount.ToString(),
                    OrderId = transactionRequest.OrderId.ToString()
                },
            };

            var response = await Client.PostAsync(
                "/Terminal", 
                monerisRequest,
                new JsonMediaTypeFormatter());

            // response.EnsureSuccessStatusCode();

            var result = await response.Content
                .ReadAsAsync<ValidationResponse>();

            return result;
        }
    }
}
