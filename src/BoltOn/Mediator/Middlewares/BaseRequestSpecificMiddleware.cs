using System;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Mediator.Pipeline;

namespace BoltOn.Mediator.Middlewares
{
	public abstract class BaseRequestSpecificMiddleware<T> : IMediatorMiddleware
	{
		public MediatorResponse<TResponse> Run<TRequest, TResponse>(IRequest<TResponse> request,
			Func<IRequest<TResponse>, MediatorResponse<TResponse>> next)
			where TRequest : IRequest<TResponse>
		{
			if (!(request is T))
				return next.Invoke(request);
			return Execute<IRequest<TResponse>, TResponse>(request, next);
		}

		public async Task<MediatorResponse<TResponse>> RunAsync<TRequest, TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken,
			Func<IRequest<TResponse>, CancellationToken, Task<MediatorResponse<TResponse>>> next) where TRequest : IRequest<TResponse>
		{
			if (!(request is T))
				return await next.Invoke(request, cancellationToken);
			return await ExecuteAsync<IRequest<TResponse>, TResponse>(request, cancellationToken, next);

		}

		public abstract void Dispose();

		public abstract MediatorResponse<TResponse> Execute<TRequest, TResponse>(IRequest<TResponse> request,
																			   Func<IRequest<TResponse>, MediatorResponse<TResponse>> next);

		public abstract Task<MediatorResponse<TResponse>> ExecuteAsync<TRequest, TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken,
																			   Func<IRequest<TResponse>, CancellationToken, Task<MediatorResponse<TResponse>>> next);

	}
}
