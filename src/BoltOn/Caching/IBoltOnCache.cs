using System;
using System.Threading;
using System.Threading.Tasks;

namespace BoltOn.Caching
{
	public interface IBoltOnCache
	{
		Task<T> GetAsync<T>(string cacheKey, CancellationToken cancellationToken,
			Func<Task<T>> valueGetter = null, TimeSpan? slidingExpiration = null) where T: class;
		Task SetAsync<T>(string cacheKey, T value, CancellationToken cancellationToken,
					TimeSpan? slidingExpiration = null) where T : class;
		Task RemoveAsync(string cacheKey, CancellationToken cancellationToken);
	}
}
