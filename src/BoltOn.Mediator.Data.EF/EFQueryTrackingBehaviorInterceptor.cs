using System;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Logging;
using BoltOn.Mediator.Interceptors;
using BoltOn.Mediator.Pipeline;

namespace BoltOn.Mediator.Data.EF
{
	public class EFQueryTrackingBehaviorInterceptor : IInterceptor
	{
		private readonly IBoltOnLogger<EFQueryTrackingBehaviorInterceptor> _logger;
		private readonly IMediatorDataContext _mediatorDataContext;

		public EFQueryTrackingBehaviorInterceptor(IBoltOnLogger<EFQueryTrackingBehaviorInterceptor> logger,
			IMediatorDataContext mediatorDataContext)
		{
			_logger = logger;
			_mediatorDataContext = mediatorDataContext;
		}

		public TResponse Run<TRequest, TResponse>(IRequest<TResponse> request, 
			Func<IRequest<TResponse>, TResponse> next) where TRequest : IRequest<TResponse>
		{
			_logger.Debug($"Entering {nameof(EFQueryTrackingBehaviorInterceptor)}...");
			_mediatorDataContext.IsQueryRequest = request is IQuery<TResponse> || request is IStaleQuery<TResponse>;
			_logger.Debug($"IsQueryRequest: {_mediatorDataContext.IsQueryRequest}");
			var response = next(request);
			return response;
		}

		public void Dispose()
		{
		}

		public async Task<TResponse> RunAsync<TRequest, TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken, 
			Func<IRequest<TResponse>, CancellationToken, Task<TResponse>> next) where TRequest : IRequest<TResponse>
		{
			_logger.Debug($"Entering {nameof(EFQueryTrackingBehaviorInterceptor)}...");
			_mediatorDataContext.IsQueryRequest = request is IQuery<TResponse>;
			_logger.Debug($"IsQueryRequest: {_mediatorDataContext.IsQueryRequest}");
			var response = await next(request, cancellationToken);
			return response;
		}
	}
}
