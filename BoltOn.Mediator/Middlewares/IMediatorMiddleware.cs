using System;

namespace BoltOn.Mediator.Middlewares
{
	public interface IMediatorMiddleware : IDisposable
	{
		StandardDtoReponse<TResponse> Run<TRequest, TResponse>(IRequest<TResponse> request, Func<IRequest<TResponse>,
										  StandardDtoReponse<TResponse>> next)
			where TRequest : IRequest<TResponse>;
	}
}