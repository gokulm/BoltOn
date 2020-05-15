using BoltOn.Requestor.Interceptors;

namespace BoltOn.Cache
{
    public interface IClearCachedResponse : IEnableInterceptor<CacheResponseInterceptor>
    {
        string CacheKey { get; set; }
    }
}
