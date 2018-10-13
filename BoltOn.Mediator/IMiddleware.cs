using System;

namespace BoltOn.Mediator
{
	public interface IMiddleware
	{
		StandardDtoReponse<TResponse> Run<TRequest, TResponse>(IRequest<TResponse> request, Func<IRequest<TResponse>,
										  StandardDtoReponse<TResponse>> next)
			where TRequest : IRequest<TResponse>;
	}
}