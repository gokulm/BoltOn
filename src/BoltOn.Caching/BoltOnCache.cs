using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
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

		public BoltOnCache(IDistributedCache distributedCache,
			IBoltOnLogger<BoltOnCache> logger)
		{
			_distributedCache = distributedCache;
			_logger = logger;
		}

		public async Task<T> GetAsync<T>(string key, CancellationToken cancellationToken = default,
			Func<Task<T>> valueGetter = default, TimeSpan? slidingExpiration = default) where T : class
		{
			_logger.Debug($"Getting from cache... Key: {key}");
			var byteArray = await _distributedCache.GetAsync(key, cancellationToken);
			var cacheValue = FromByteArray<T>(byteArray);

			if (cacheValue != default(T))
			{
				if (valueGetter != null)
				{
					_logger.Debug("Invoking valueGetter...");
					cacheValue = await valueGetter();
					await SetAsync(key, cacheValue, cancellationToken, slidingExpiration);
				}
				else if(slidingExpiration.HasValue)
				{
					_logger.Debug("Sliding cache expiration...");
					await SetAsync(key, cacheValue, cancellationToken, slidingExpiration);
				}
			}
			return cacheValue;
		}

		public async Task SetAsync<T>(string key, T value, CancellationToken cancellationToken = default,
			TimeSpan? slidingExpiration = default) where T : class
		{
			_logger.Debug($"Setting value in cache... Key: {key}");
			var byteArray = ToByteArray(value);
			var options = new DistributedCacheEntryOptions { SlidingExpiration = slidingExpiration };
			await _distributedCache.SetAsync(key, byteArray, options, cancellationToken);
		}

		public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
		{
			_logger.Debug($"Removing from cache... Key: {key}");
			await _distributedCache.RemoveAsync(key, cancellationToken);
		}

		private byte[] ToByteArray(object obj)
		{
			_logger.Debug("Converting object to byteArray...");
			if (obj == null)
			{
				return null;
			}

			var binaryFormatter = new BinaryFormatter();
			using var memoryStream = new MemoryStream();
			binaryFormatter.Serialize(memoryStream, obj);
			return memoryStream.ToArray();
		}

		private T FromByteArray<T>(byte[] byteArray) where T : class
		{
			_logger.Debug("Getting object from byteArray...");
			if (byteArray == null)
				return default;

			var binaryFormatter = new BinaryFormatter();
			using var memoryStream = new MemoryStream(byteArray);
			return binaryFormatter.Deserialize(memoryStream) as T;
		}
	}
}
