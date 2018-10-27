using System;

namespace BoltOn.Mediator
{
    public abstract class BaseRequestSpecificMiddleware<T> : IMediatorMiddleware
    {
        public StandardDtoReponse<TResponse> Run<TRequest, TResponse>(IRequest<TResponse> request,
                                                                      Func<IRequest<TResponse>, StandardDtoReponse<TResponse>> next)
            where TRequest : IRequest<TResponse>
        {
            if (!(request is T))
                return next.Invoke(request);

            return Execute<IRequest<TResponse>, TResponse>(request, next);
        }

        public abstract void Dispose();

        public abstract StandardDtoReponse<TResponse> Execute<TRequest, TResponse>(IRequest<TResponse> request,
                                                                               Func<IRequest<TResponse>, StandardDtoReponse<TResponse>> next);
    }
}
