using System;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Mediator.Pipeline;

namespace BoltOn.Mediator.Interceptors
{
	public interface IInterceptor : IDisposable
	{
		TResponse Run<TRequest, TResponse>(IRequest<TResponse> request, 
		Func<IRequest<TResponse>, TResponse> next)
			where TRequest : IRequest<TResponse>;

		Task<TResponse> RunAsync<TRequest, TResponse>(IRequest<TResponse> request, 
			CancellationToken cancellationToken,
		 	Func<IRequest<TResponse>, CancellationToken, Task<TResponse>> next)
			where TRequest : IRequest<TResponse>;
	}
}