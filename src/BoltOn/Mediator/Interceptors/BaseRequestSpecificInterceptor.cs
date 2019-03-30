using System;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Mediator.Pipeline;

namespace BoltOn.Mediator.Interceptors
{
	public abstract class BaseRequestSpecificInterceptor<T> : IInterceptor
	{
		public TResponse Run<TRequest, TResponse>(IRequest<TResponse> request,
			Func<IRequest<TResponse>, TResponse> next)
			where TRequest : IRequest<TResponse>
		{
			if (!(request is T))
				return next.Invoke(request);
			return Execute<IRequest<TResponse>, TResponse>(request, next);
		}

		public async Task<TResponse> RunAsync<TRequest, TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken,
			Func<IRequest<TResponse>, CancellationToken, Task<TResponse>> next) where TRequest : IRequest<TResponse>
		{
			if (!(request is T))
				return await next.Invoke(request, cancellationToken);
			return await ExecuteAsync<IRequest<TResponse>, TResponse>(request, cancellationToken, next);

		}

		public abstract void Dispose();

		public abstract TResponse Execute<TRequest, TResponse>(IRequest<TResponse> request,
																			   Func<IRequest<TResponse>, TResponse> next);

		public abstract Task<TResponse> ExecuteAsync<TRequest, TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken,
																			   Func<IRequest<TResponse>, CancellationToken, Task<TResponse>> next);

	}
}
