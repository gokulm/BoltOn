using System;
using BoltOn.Mediator.Pipeline;

namespace BoltOn.Mediator.Middlewares
{
	public interface IMediatorMiddleware : IDisposable
	{
		MediatorResponse<TResponse> Run<TRequest, TResponse>(IRequest<TResponse> request, Func<IRequest<TResponse>,
										  MediatorResponse<TResponse>> next) 
			where TRequest : IRequest<TResponse>;
	}
}