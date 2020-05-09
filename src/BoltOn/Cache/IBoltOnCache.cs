using System;
using System.Threading;
using System.Threading.Tasks;

namespace BoltOn.Cache
{
	public interface IBoltOnCache
	{
		Task<T> GetAsync<T>(string key, CancellationToken cancellationToken = default,
			Func<Task<T>> valueGetter = default, TimeSpan? slidingExpiration = default) where T: class;
		Task SetAsync<T>(string key, T value, CancellationToken cancellationToken = default,
					TimeSpan? slidingExpiration = default) where T : class;
		Task RemoveAsync(string key, CancellationToken cancellationToken = default);
	}
}
