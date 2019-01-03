using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;

namespace WebJob
{
    public class Functions
    {
        private readonly ILogger<Functions> logger;

        public Functions(ILogger<Functions> logger)
        {
            this.logger = logger;
        }

        public static void TimerJob([TimerTrigger("00:10:00")] TimerInfo timer)
        {
            Console.WriteLine("Timer job fired!");
        }
    }
}
