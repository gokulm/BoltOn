using System;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Logging;
using BoltOn.Requestor.Interceptors;
using BoltOn.Requestor.Pipeline;

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

		public async Task<TResponse> RunAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken,
			Func<TRequest, CancellationToken, Task<TResponse>> next) where TRequest : IRequest<TResponse>
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
