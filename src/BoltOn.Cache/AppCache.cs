using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Logging;
using Microsoft.Extensions.Caching.Distributed;

namespace BoltOn.Cache
{
	public class AppCache : IAppCache
	{
		private readonly IDistributedCache _distributedCache;
		private readonly IAppLogger<AppCache> _logger;
		private readonly IAppCacheSerializer _serializer;

		public AppCache(IDistributedCache distributedCache,
			IAppLogger<AppCache> logger,
			IAppCacheSerializer serializer)
		{
			_distributedCache = distributedCache;
			_logger = logger;
			_serializer = serializer;
		}

		public async Task<T> GetAsync<T>(string key, CancellationToken cancellationToken = default,
			Func<Task<T>> valueGetter = default, TimeSpan? slidingExpiration = default)
		{
			_logger.Debug($"Getting from cache... Key: {key}");
			var byteArray = await _distributedCache.GetAsync(key, cancellationToken);
			var cacheValue = _serializer.FromByteArray<T>(byteArray);

			if (EqualityComparer<T>.Default.Equals(cacheValue, default) && valueGetter != null)
			{
				_logger.Debug("Invoking valueGetter...");
				cacheValue = await valueGetter();
				await SetAsync(key, cacheValue, cancellationToken, slidingExpiration);
				return cacheValue;
			}

			if (slidingExpiration.HasValue)
			{
				_logger.Debug("Sliding cache expiration...");
				await SetAsync(key, cacheValue, cancellationToken, slidingExpiration);
			}

			return cacheValue;
		}

		public async Task SetAsync<T>(string key, T value, CancellationToken cancellationToken = default,
			TimeSpan? slidingExpiration = default)
		{
			_logger.Debug($"Setting value in cache... Key: {key}");
			if(value == null)
				return;
			var byteArray = _serializer.ToByteArray(value);
			var options = new DistributedCacheEntryOptions { SlidingExpiration = slidingExpiration };
			await _distributedCache.SetAsync(key, byteArray, options, cancellationToken);
		}

		public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
		{
			_logger.Debug($"Removing from cache... Key: {key}");
			await _distributedCache.RemoveAsync(key, cancellationToken);
		}
	}
}
