using System;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Requestor.Interceptors;
using BoltOn.Requestor.Pipeline;

namespace BoltOn.Tests.Bootstrapping.Fakes
{
	public class TestBootstrappingInterceptor : IInterceptor
	{

		public void Dispose()
		{
		}

		public Task<TResponse> RunAsync<TRequest, TResponse>(TRequest request,
			CancellationToken cancellationToken, Func<TRequest, CancellationToken, Task<TResponse>> next)
			where TRequest : IRequest<TResponse>
		{
			return Task.FromResult(default(TResponse));
		}
	}
}
