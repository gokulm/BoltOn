using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Logging;
using BoltOn.Utilities;

namespace BoltOn.Mediator.Interceptors
{
	public interface IEnableStopwatchInterceptor
	{
	}

	public class StopwatchInterceptor : BaseRequestSpecificInterceptor<IEnableStopwatchInterceptor>
	{
		private readonly IBoltOnLogger<StopwatchInterceptor> _logger;
		private readonly IBoltOnClock _boltOnClock;

		public StopwatchInterceptor(IBoltOnLogger<StopwatchInterceptor> logger, IBoltOnClock boltOnClock)
		{
			_logger = logger;
			_boltOnClock = boltOnClock;
		}

		public override void Dispose()
		{
		}

		public override async Task<TResponse> ExecuteAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken,
			Func<TRequest, CancellationToken, Task<TResponse>> next)
		{
			var stopwatch = new Stopwatch();
			_logger.Debug($"StopwatchInterceptor started at {_boltOnClock.Now}");
			var response = await next(request, cancellationToken);
			_logger.Debug($"StopwatchInterceptor ended at {_boltOnClock.Now}. Time elapsed: {stopwatch.ElapsedMilliseconds}");
			return response;
		}
	}
}
