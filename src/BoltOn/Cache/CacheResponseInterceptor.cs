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
        private readonly IAppLogger<CacheResponseInterceptor> _logger;
        private readonly IAppCache _appCache;

        public CacheResponseInterceptor(IAppLogger<CacheResponseInterceptor> logger,
            IAppCache appCache)
        {
            _logger = logger;
            _appCache = appCache;
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
                var responseFromCache = await _appCache.GetAsync<TResponse>(cacheRequest.CacheKey,
                    cancellationToken, absoluteExpiration: cacheRequest.AbsoluteExpiration);

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
                await _appCache.SetAsync(cacheRequest.CacheKey, response,
                    cancellationToken, cacheRequest.AbsoluteExpiration);
            }

            if (request is IClearCachedResponse clearCacheRequest)
            {
                _logger.Debug($"Removing response from cache. Key: {clearCacheRequest.CacheKey}");
                await _appCache.RemoveAsync(clearCacheRequest.CacheKey, cancellationToken);
            }

            _logger.Debug("CacheResponseInterceptor ended");
            return response;
        }

        public void Dispose()
        {
        }
    }
}
