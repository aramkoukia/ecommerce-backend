﻿using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace EcommerceApi.Filters
{
    /// <summary>
    /// Use Idempotent operations on POST and PATCH HTTP methods
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class IdempotentAttribute : Attribute, IFilterFactory
    {
        public bool IsReusable => false;

        public bool Enabled { get; set; } = true;

        public int ExpireHours { get; set; } = 24;

        public string DistributedCacheKeysPrefix { get; set; } = "IdempAPI_";

        public string HeaderKeyName { get; set; } = "idempotency-key";

        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            var distributedCache = (IDistributedCache)serviceProvider.GetService(typeof(IDistributedCache));
            var loggerFactory = (ILoggerFactory)serviceProvider.GetService(typeof(ILoggerFactory));

            IdempotencyAttributeFilter idempotencyAttributeFilter = new IdempotencyAttributeFilter(distributedCache, loggerFactory, Enabled, ExpireHours, HeaderKeyName, DistributedCacheKeysPrefix);
            return idempotencyAttributeFilter;
        }
    }
}
