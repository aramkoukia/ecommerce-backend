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
        public static void RunSyncProducts([TimerTrigger("0 0 1 * * *")]TimerInfo myTimer, TraceWriter log)
        {
            log.Info($"SyncProducts: {DateTime.Now}");
            client.GetAsync("https://lightsandpartsapi.azurewebsites.net/api/sync/products");
        }

        [FunctionName("SyncCustomers")]
        public static void RunSyncCustomers([TimerTrigger("0 0 1 * * *")]TimerInfo myTimer, TraceWriter log)
        {
            log.Info($"Sync Customers: {DateTime.Now}");
            client.GetAsync("https://lightsandpartsapi.azurewebsites.net/api/sync/customers");
        }
    }
}
