using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Logging;
using BoltOn.Mediator.Pipeline;
using BoltOn.Utilities;

namespace BoltOn.Mediator.Middlewares
{
	public interface IEnableStopwatchMiddleware
	{
	}

	public class StopwatchMiddleware : BaseRequestSpecificMiddleware<IEnableStopwatchMiddleware>
	{
		private readonly IBoltOnLogger<StopwatchMiddleware> _logger;
		private readonly IBoltOnClock _boltOnClock;

		public StopwatchMiddleware(IBoltOnLogger<StopwatchMiddleware> logger, IBoltOnClock boltOnClock)
		{
			_logger = logger;
			_boltOnClock = boltOnClock;
		}

		public override MediatorResponse<TResponse> Execute<TRequest, TResponse>(IRequest<TResponse> request,
																				   Func<IRequest<TResponse>, MediatorResponse<TResponse>> next)
		{
			var stopwatch = new Stopwatch();
			_logger.Debug($"StopwatchMiddleware started at {_boltOnClock.Now}");
			var response = next(request);
			_logger.Debug($"StopwatchMiddleware ended at {_boltOnClock.Now}. Time elapsed: {stopwatch.ElapsedMilliseconds}");
			return response;
		}

		public override void Dispose()
		{
		}

		public override async Task<MediatorResponse<TResponse>> ExecuteAsync<TRequest, TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken,
			Func<IRequest<TResponse>, CancellationToken, Task<MediatorResponse<TResponse>>> next)
		{
			var stopwatch = new Stopwatch();
			_logger.Debug($"StopwatchMiddleware started at {_boltOnClock.Now}");
			var response = await next(request, cancellationToken);
			_logger.Debug($"StopwatchMiddleware ended at {_boltOnClock.Now}. Time elapsed: {stopwatch.ElapsedMilliseconds}");
			return response;
		}
	}
}
