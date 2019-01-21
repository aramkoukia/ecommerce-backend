using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;

namespace WebJob
{
    public class Functions
    {
        private readonly ILogger<Functions> logger;
        private static readonly HttpClient client = new HttpClient();
        private static readonly string baseUrl = "https://lightsandpartsapi.azurewebsites.net/api/";
        public Functions(ILogger<Functions> logger)
        {
            this.logger = logger;
        }

        public static void TimerJob([TimerTrigger("24:00:00")] TimerInfo timer)
        {
            Console.WriteLine("Timer job fired!");

            client.DefaultRequestHeaders.Accept.Clear();
            var result1 = client.GetStringAsync($"{baseUrl}sync/Products");
            var result2 = client.GetStringAsync($"{baseUrl}sync/ProductsInventory");
            var result3 = client.GetStringAsync($"{baseUrl}sync/Customers");

            Console.Write("Ran all Apis");
        }
    }
}
