using System;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Logging;
using Microsoft.Extensions.Caching.Distributed;

namespace BoltOn.Caching
{
	public class BoltOnCache : IBoltOnCache
	{
		private readonly IDistributedCache _distributedCache;
		private readonly IBoltOnLogger<BoltOnCache> _logger;
		private readonly IBoltOnCacheSerializer _serializer;

		public BoltOnCache(IDistributedCache distributedCache,
			IBoltOnLogger<BoltOnCache> logger,
			IBoltOnCacheSerializer serializer)
		{
			_distributedCache = distributedCache;
			_logger = logger;
			_serializer = serializer;
		}

		public async Task<T> GetAsync<T>(string key, CancellationToken cancellationToken = default,
			Func<Task<T>> valueGetter = default, TimeSpan? slidingExpiration = default) where T : class
		{
			_logger.Debug($"Getting from cache... Key: {key}");
			var byteArray = await _distributedCache.GetAsync(key, cancellationToken);
			var cacheValue = _serializer.FromByteArray<T>(byteArray);

			if (cacheValue == default(T) && valueGetter != null)
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
			TimeSpan? slidingExpiration = default) where T : class
		{
			_logger.Debug($"Setting value in cache... Key: {key}");
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
