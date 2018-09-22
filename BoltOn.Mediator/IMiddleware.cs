using System;
using BoltOn.Logging;

namespace BoltOn.Mediator
{
    // public delegate TResponse Handle<in TRequest, TResponse>(TRequest request);

    public interface IMiddleware<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        StandardDtoReponse<TResponse> Run(IRequest<TResponse> request, Func<IRequest<TResponse>, StandardDtoReponse<TResponse>> next);
    }

    public class PerformanceMiddleware<TRequest, TResponse> : IMiddleware<TRequest, TResponse> 
    where TRequest : IRequest<TResponse>
    {
        private readonly IBoltOnLogger<PerformanceMiddleware<TRequest, TResponse>> _logger;

        public PerformanceMiddleware(IBoltOnLogger<PerformanceMiddleware<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public StandardDtoReponse<TResponse> Run(IRequest<TResponse> request, Func<IRequest<TResponse>, StandardDtoReponse<TResponse>> next)
        {
            _logger.Debug("started");
            var response = next.Invoke(request);
            _logger.Debug("completed");
            return response;
        }
    }
}