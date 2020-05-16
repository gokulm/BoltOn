using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Logging;
using BoltOn.Requestor.Interceptors;
using BoltOn.Requestor.Pipeline;

namespace BoltOn.Cache
{
    public class CacheResponseInterceptor : IInterceptor
    {
        private readonly IBoltOnLogger<CacheResponseInterceptor> _logger;
        private readonly IBoltOnCache _boltOnCache;

        public CacheResponseInterceptor(IBoltOnLogger<CacheResponseInterceptor> logger,
            IBoltOnCache boltOnCache)
        {
            _logger = logger;
            _boltOnCache = boltOnCache;
        }

        public async Task<TResponse> RunAsync<TRequest, TResponse>(TRequest request,
            CancellationToken cancellationToken,
            Func<TRequest, CancellationToken, Task<TResponse>> next) where TRequest : IRequest<TResponse>
        {
            if (!(request is IEnableInterceptor<CacheResponseInterceptor>))
                return await next.Invoke(request, cancellationToken);

            _logger.Debug("CacheResponseInterceptor started");
            var cacheRequest = request as ICacheResponse;
            if (cacheRequest != null)
            {
                _logger.Debug($"Retrieving response from cache. Key: {cacheRequest.CacheKey}");
                var responseFromCache = await _boltOnCache.GetAsync<TResponse>(cacheRequest.CacheKey,
                    cancellationToken, null, cacheRequest.SlidingExpiration);

                if (!EqualityComparer<TResponse>.Default.Equals(responseFromCache, default))
                {
                    _logger.Debug("Returning response from cache");
                    _logger.Debug("CacheResponseInterceptor ended");
                    return responseFromCache;
                }
            }

            var response = await next(request, cancellationToken);

            if (cacheRequest != null)
            {
                _logger.Debug($"Saving response in cache. Key: {cacheRequest.CacheKey}");
                await _boltOnCache.SetAsync(cacheRequest.CacheKey, response,
                    cancellationToken, cacheRequest.SlidingExpiration);
            }

            if (request is IClearCachedResponse clearCacheRequest)
            {
                _logger.Debug($"Removing response from cache. Key: {clearCacheRequest.CacheKey}");
                await _boltOnCache.RemoveAsync(cacheRequest.CacheKey, cancellationToken);
            }

            _logger.Debug("CacheResponseInterceptor ended");
            return response;
        }

        public void Dispose()
        {
        }
    }
}
