using System;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Mediator.Pipeline;

namespace BoltOn.Mediator.Middlewares
{
	public interface IMediatorMiddleware : IDisposable
	{
		MediatorResponse<TResponse> Run<TRequest, TResponse>(IRequest<TResponse> request, 
		Func<IRequest<TResponse>, MediatorResponse<TResponse>> next)
			where TRequest : IRequest<TResponse>;

		Task<MediatorResponse<TResponse>> RunAsync<TRequest, TResponse>(IRequest<TResponse> request, 
			CancellationToken cancellationToken,
		 	Func<IRequest<TResponse>, CancellationToken, Task<MediatorResponse<TResponse>>> next)
			where TRequest : IRequest<TResponse>;
	}
}