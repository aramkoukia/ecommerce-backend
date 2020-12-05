using EcommerceApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace EcommerceApi.Middleware
{
    public class AdminSafeListMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AdminSafeListMiddleware> _logger;
        // private readonly string _adminSafeList;

        public AdminSafeListMiddleware(
            RequestDelegate next, 
            ILogger<AdminSafeListMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context, EcommerceContext dbContext)
        {
            var adminSafeList = dbContext.Settings.AsNoTracking().FirstOrDefault().AllowedIPAddresses;
            var remoteIp = context.Connection.RemoteIpAddress;
            // _logger.LogDebug($"Request from Remote IP address: {remoteIp}");

            var ipList = adminSafeList?.Split(',');

            var bytes = remoteIp.GetAddressBytes();
            var badIp = true;
            if (ipList == null || !ipList.Any())
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await context.Response.WriteAsync(new ErrorDetails()
                {
                    StatusCode = 401,
                    Message = $"Unautorized Client IP"
                }.ToString());
                return;
            }

            foreach (var address in ipList)
            {
                var testIp = IPAddress.Parse(address);
                if (testIp.GetAddressBytes().SequenceEqual(bytes))
                {
                    badIp = false;
                    break;
                }
            }

            if (badIp)
            {

                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                // _logger.LogInformation($"Forbidden Request from Remote IP address: {remoteIp}");
                await context.Response.WriteAsync(new ErrorDetails()
                {
                    StatusCode = 401,
                    Message = $"Unautorized Client IP"
                }.ToString());
                return;
            }

            await _next.Invoke(context);
        }
    }
}
