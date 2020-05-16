using System;
using BoltOn.Requestor.Interceptors;

namespace BoltOn.Cache
{
    public interface ICacheResponse : IEnableInterceptor<CacheResponseInterceptor>
    {
        string CacheKey { get; }
        TimeSpan? SlidingExpiration { get; }
    }
}
