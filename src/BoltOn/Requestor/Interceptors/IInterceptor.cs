using System;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Requestor.Pipeline;

namespace BoltOn.Requestor.Interceptors
{
	public interface IInterceptor : IDisposable
	{
		Task<TResponse> RunAsync<TRequest, TResponse>(TRequest request, 
			CancellationToken cancellationToken,
		 	Func<TRequest, CancellationToken, Task<TResponse>> next)
			where TRequest : IRequest<TResponse>;
	}
}