using System;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Logging;
using BoltOn.Mediator.Pipeline;

namespace BoltOn.Mediator.Interceptors
{
	public class MediatorContextInterceptor : IInterceptor
	{
		private readonly IBoltOnLogger<MediatorContextInterceptor> _logger;
		private readonly MediatorContext _mediatorContext;

		public MediatorContextInterceptor(IBoltOnLogger<MediatorContextInterceptor> logger,
			MediatorContext mediatorContext)
		{
			_logger = logger;
			_mediatorContext = mediatorContext;
		}

		public TResponse Run<TRequest, TResponse>(IRequest<TResponse> request,
			Func<IRequest<TResponse>, TResponse> next) where TRequest : IRequest<TResponse>
		{
			_logger.Debug($"Entering {nameof(MediatorContextInterceptor)}...");
			_mediatorContext.IsQueryRequest = request is IQuery<TResponse>;
			_logger.Debug($"IsQueryRequest: {_mediatorContext.IsQueryRequest}");
			var response = next(request);
			return response;
		}

		public async Task<TResponse> RunAsync<TRequest, TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken,
			Func<IRequest<TResponse>, CancellationToken, Task<TResponse>> next) where TRequest : IRequest<TResponse>
		{
			_logger.Debug($"Entering {nameof(MediatorContextInterceptor)}...");
			_mediatorContext.IsQueryRequest = request is IQuery<TResponse>;
			_logger.Debug($"IsQueryRequest: {_mediatorContext.IsQueryRequest}");
			var response = await next(request, cancellationToken);
			return response;
		}

		public void Dispose()
		{
		}
	}
}
