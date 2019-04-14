using System;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Data.EF;
using BoltOn.Logging;
using BoltOn.Mediator.Interceptors;
using BoltOn.Mediator.Pipeline;
using BoltOn.Ovverides.Mediator;

namespace BoltOn.Overrides.Mediator
{
	public class CustomChangeTrackerInterceptor : IInterceptor
	{
		private readonly IBoltOnLogger<CustomChangeTrackerInterceptor> _logger;
		private readonly ChangeTrackerContext _changeTrackerContext;

		public CustomChangeTrackerInterceptor(IBoltOnLogger<CustomChangeTrackerInterceptor> logger,
			ChangeTrackerContext changeTrackerContext)
		{
			_logger = logger;
			_changeTrackerContext = changeTrackerContext;
		}

		public TResponse Run<TRequest, TResponse>(IRequest<TResponse> request, 
			Func<IRequest<TResponse>, TResponse> next) where TRequest : IRequest<TResponse>
		{
			_logger.Debug($"Entering {nameof(CustomChangeTrackerInterceptor)}...");
			_changeTrackerContext.IsQueryRequest = request is IQuery<TResponse> || request is IQueryUncommitted<TResponse>;
			_logger.Debug($"IsQueryRequest or IQueryUncommitted: {_changeTrackerContext.IsQueryRequest}");
			var response = next(request);
			return response;
		}

		public async Task<TResponse> RunAsync<TRequest, TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken, 
			Func<IRequest<TResponse>, CancellationToken, Task<TResponse>> next) where TRequest : IRequest<TResponse>
		{
			_logger.Debug($"Entering {nameof(CustomChangeTrackerInterceptor)}...");
			_changeTrackerContext.IsQueryRequest = request is IQuery<TResponse> || request is IQueryUncommitted<TResponse>;
			_logger.Debug($"IsQueryRequest or IQueryUncommitted: {_changeTrackerContext.IsQueryRequest}");
			var response = await next(request, cancellationToken);
			return response;
		}

		public void Dispose()
		{
		}
	}
}
