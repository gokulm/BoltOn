using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Logging;
using BoltOn.Requestor.Pipeline;
using BoltOn.Utilities;

namespace BoltOn.Requestor.Interceptors
{
	public class StopwatchInterceptor : IInterceptor
	{
		private readonly IAppLogger<StopwatchInterceptor> _logger;
		private readonly IAppClock _appClock;

		public StopwatchInterceptor(IAppLogger<StopwatchInterceptor> logger, IAppClock appClock)
		{
			_logger = logger;
			_appClock = appClock;
		}

		public async Task<TResponse> RunAsync<TRequest, TResponse>(TRequest request,
			CancellationToken cancellationToken,
			Func<TRequest, CancellationToken, Task<TResponse>> next) where TRequest : IRequest<TResponse>
		{
			if (!(request is IEnableInterceptor<StopwatchInterceptor>))
				return await next.Invoke(request, cancellationToken);

			var stopwatch = new Stopwatch();
			_logger.Debug($"StopwatchInterceptor started at {_appClock.Now}");
			var response = await next(request, cancellationToken);
			_logger.Debug($"StopwatchInterceptor ended at {_appClock.Now}. Time elapsed: {stopwatch.ElapsedMilliseconds}");
			return response;
		}

		public void Dispose()
		{
		}
	}
}
