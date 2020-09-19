using System;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace DataSyncFunctions
{
    public static class DataSyncFunctions
    {
        static HttpClient client = new HttpClient();

        //[FunctionName("SyncProducts")]
        //public static void RunSyncProducts([TimerTrigger("0 0 1,23,21,19,17 * * *")]TimerInfo myTimer, TraceWriter log)
        //{
        //    log.Info($"SyncProducts: {DateTime.Now}");
        //    client.GetAsync("https://lightsandpartsapi.azurewebsites.net/api/sync/products");
        //}

        [FunctionName("MakeOnHoldOrdersAsQuote")]
        public static void RunMakeOnHoldOrdersAsQuote([TimerTrigger("0 0 1 * * *")]TimerInfo myTimer, TraceWriter log)
        {
            log.Info($"MakeOnHoldOrdersAsQuote: {DateTime.Now}");
            client.GetAsync("https://lightsandpartsapi.azurewebsites.net/api/orders/cancelonholdorders");
        }

        //[FunctionName("SyncCustomers")]
        //public static void RunSyncCustomers([TimerTrigger("0 0 1 * * *")]TimerInfo myTimer, TraceWriter log)
        //{
        //    log.Info($"Sync Customers: {DateTime.Now}");
        //    client.GetAsync("https://lightsandpartsapi.azurewebsites.net/api/sync/customers");
        //}

        [FunctionName("SendCustomerInvoices")]
        // 8:00 a.m. every 1st of everymonth
        public static void RunSendCustomerInvoices([TimerTrigger("0 8 1 * * *")]TimerInfo myTimer, TraceWriter log)
        {
            log.Info($"Sending Customer Invoices: {DateTime.Now}");
            client.GetAsync("https://lightsandpartsapi.azurewebsites.net/api/orders/customerinvoices");
        }

        [FunctionName("SendInventoryValueReport")]
        // 7:00 a.m. every 1st of everymonth
        public static void RunSendInventoryValueReport([TimerTrigger("0 0 15 1 * *")]TimerInfo myTimer, TraceWriter log)
        {
            log.Info($"Sending Inventory Value Report: {DateTime.Now}");
            client.GetAsync("https://lightsandpartsapi.azurewebsites.net/api/reports/MonthlyInventoryValue");
        }
    }
}
