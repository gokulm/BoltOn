using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Logging;
using BoltOn.Mediator.Pipeline;
using BoltOn.Utilities;

namespace BoltOn.Mediator.Interceptors
{
	public class StopwatchInterceptor : IInterceptor
	{
		private readonly IBoltOnLogger<StopwatchInterceptor> _logger;
		private readonly IBoltOnClock _boltOnClock;

		public StopwatchInterceptor(IBoltOnLogger<StopwatchInterceptor> logger, IBoltOnClock boltOnClock)
		{
			_logger = logger;
			_boltOnClock = boltOnClock;
		}

        public async Task<TResponse> RunAsync<TRequest, TResponse>(TRequest request, 
            CancellationToken cancellationToken,
            Func<TRequest, CancellationToken, Task<TResponse>> next) where TRequest : IRequest<TResponse>
		{
			if (!(request is IEnableInterceptor<StopwatchInterceptor>))
                return await next.Invoke(request, cancellationToken);

            var stopwatch = new Stopwatch();
            _logger.Debug($"StopwatchInterceptor started at {_boltOnClock.Now}");
            var response = await next(request, cancellationToken);
            _logger.Debug($"StopwatchInterceptor ended at {_boltOnClock.Now}. Time elapsed: {stopwatch.ElapsedMilliseconds}");
            return response;
		}

        public void Dispose()
        {
        }
	}
}
