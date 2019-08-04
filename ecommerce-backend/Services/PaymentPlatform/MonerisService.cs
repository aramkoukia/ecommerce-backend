using System;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using EcommerceApi.Models;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace EcommerceApi.Services.PaymentPlatform
{
    public class MonerisService : IMonerisService
    {
        public HttpClient Client { get; }
        private readonly IConfiguration _config;
        private readonly EcommerceContext _context;
        private readonly IHttpClientFactory _clientFactory;

        public MonerisService(IHttpClientFactory clientFactory,
                              IConfiguration config,
                              EcommerceContext context)
        {
            _config = config;
            _context = context;
            _clientFactory = clientFactory;
        }

        public async Task<ValidationResponse> TransactionRequestAsync(TransactionRequest transactionRequest)
        {
            try
            {
                var client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var clientPosSettings = _context.ClientPosSettings.FirstOrDefault(c => c.ClientIp == transactionRequest.ClientIp);
                if (clientPosSettings == null)
                {
                    return null; // log error, and don't return null dude!
                }

                var monerisRequest = new MonerisRequest
                {
                    apiToken = _config["Moneris:apiToken"],
                    postbackUrl = _config["Moneris:postbackUrl"],
                    storeId = clientPosSettings.StoreId,
                    terminalId = clientPosSettings.TerminalId,
                    txnType = transactionRequest.TransactionType,
                    request = new Request
                    {
                        amount = transactionRequest.Amount.ToString(),
                        orderId = transactionRequest.OrderId.ToString()
                    },
                };

                var response = await client.PostAsync(
                    _config["Moneris:baseUrl"], 
                    new StringContent(
                        JsonConvert.SerializeObject(monerisRequest), 
                        Encoding.UTF8, 
                        "application/json"));

                var result = await response.Content
                    .ReadAsAsync<ValidationResponse>();

                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
