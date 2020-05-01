using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace BoltOn.Caching
{
	public class BoltOnCache : IBoltOnCache
	{
		private readonly IDistributedCache _distributedCache;

		public BoltOnCache(IDistributedCache distributedCache)
		{
			_distributedCache = distributedCache;
		}

		public async Task<T> GetAsync<T>(string cacheKey, CancellationToken cancellationToken,
			Func<Task<T>> valueGetter = null, TimeSpan? slidingExpiration = null) where T : class
		{
			var byteArray = await _distributedCache.GetAsync(cacheKey, cancellationToken);
			var cacheValue = FromByteArray<T>(byteArray);

			if (cacheValue != default(T))
			{
				if (valueGetter != null)
				{
					cacheValue = await valueGetter();
					await SetAsync(cacheKey, cacheValue, cancellationToken, slidingExpiration);
				}
				else if(slidingExpiration.HasValue)
				{
					await SetAsync(cacheKey, cacheValue, cancellationToken, slidingExpiration);
				}
			}
			return cacheValue;
		}

		public async Task SetAsync<T>(string cacheKey, T value, CancellationToken cancellationToken,
			TimeSpan? slidingExpiration = null) where T : class
		{
			var byteArray = ToByteArray(value);
			var options = new DistributedCacheEntryOptions { SlidingExpiration = slidingExpiration };
			await _distributedCache.SetAsync(cacheKey, byteArray, options, cancellationToken);
		}

		public async Task RemoveAsync(string cacheKey, CancellationToken cancellationToken)
		{
			await _distributedCache.RemoveAsync(cacheKey, cancellationToken);
		}

		private byte[] ToByteArray(object obj)
		{
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
			if (byteArray == null)
				return default;

			var binaryFormatter = new BinaryFormatter();
			using var memoryStream = new MemoryStream(byteArray);
			return binaryFormatter.Deserialize(memoryStream) as T;
		}
	}
}
