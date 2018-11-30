using System;
using System.Diagnostics;
using BoltOn.Logging;
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
			var response = next.Invoke(request);
			_logger.Debug($"StopwatchMiddleware ended at {_boltOnClock.Now}. Time elapsed: {stopwatch.ElapsedMilliseconds}");
			return response;
		}

		public override void Dispose()
		{
		}
	}
}
