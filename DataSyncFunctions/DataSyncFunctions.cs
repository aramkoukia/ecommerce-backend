using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace DataSyncFunctions
{
    public static class DataSyncFunctions
    {
        static HttpClient client = new HttpClient();

        [FunctionName("SyncProducts")]
        public static async Task Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, TraceWriter log)
        {
            log.Info($"SyncProducts: {DateTime.Now}");
            HttpResponseMessage response = await client.GetAsync("https://lightsandpartsapi.azurewebsites.net/api/sync/products");
        }

        [FunctionName("SyncCustomers")]
        public static async Task RunSyncProducts([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, TraceWriter log)
        {
            log.Info($"Sync Customers: {DateTime.Now}");
            HttpResponseMessage response = await client.GetAsync("https://lightsandpartsapi.azurewebsites.net/api/sync/customers");
        }
    }
}
