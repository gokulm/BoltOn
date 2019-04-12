using System;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Logging;
using BoltOn.Mediator.Interceptors;
using BoltOn.Mediator.Pipeline;

namespace BoltOn.Data.EF
{
	public class ChangeTrackerInterceptor : IInterceptor
	{
		private readonly IBoltOnLogger<ChangeTrackerInterceptor> _logger;
		private readonly ChangeTrackerContext _changeTrackerContext;

		public ChangeTrackerInterceptor(IBoltOnLogger<ChangeTrackerInterceptor> logger,
			ChangeTrackerContext changeTrackerContext)
		{
			_logger = logger;
			_changeTrackerContext = changeTrackerContext;
		}

		public TResponse Run<TRequest, TResponse>(IRequest<TResponse> request,
			Func<IRequest<TResponse>, TResponse> next) where TRequest : IRequest<TResponse>
		{
			_logger.Debug($"Entering {nameof(ChangeTrackerInterceptor)}...");
			_changeTrackerContext.IsQueryRequest = request is IQuery<TResponse>;
			_logger.Debug($"IsQueryRequest: {_changeTrackerContext.IsQueryRequest}");
			var response = next(request);
			return response;
		}

		public async Task<TResponse> RunAsync<TRequest, TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken,
			Func<IRequest<TResponse>, CancellationToken, Task<TResponse>> next) where TRequest : IRequest<TResponse>
		{
			_logger.Debug($"Entering {nameof(ChangeTrackerInterceptor)}...");
			_changeTrackerContext.IsQueryRequest = request is IQuery<TResponse>;
			_logger.Debug($"IsQueryRequest: {_changeTrackerContext.IsQueryRequest}");
			var response = await next(request, cancellationToken);
			return response;
		}

		public void Dispose()
		{
		}
	}
}
