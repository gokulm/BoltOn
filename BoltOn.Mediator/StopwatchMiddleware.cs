using System;
using System.Diagnostics;
using BoltOn.Logging;
using BoltOn.Utilities;

namespace BoltOn.Mediator
{
	public interface IEnableStopwatchMiddleware
	{
	}

	public class StopwatchMiddleware : BaseRequestSpecificMiddleware<IEnableStopwatchMiddleware>
    {
        private readonly IBoltOnLogger<StopwatchMiddleware> _logger;
		private readonly ICurrentDateTimeRetriever _currentDateTimeRetriever;

		public StopwatchMiddleware(IBoltOnLogger<StopwatchMiddleware> logger, ICurrentDateTimeRetriever currentDateTimeRetriever)
        {
            _logger = logger;
			_currentDateTimeRetriever = currentDateTimeRetriever;
		}

		public override StandardDtoReponse<TResponse> Execute<TRequest, TResponse>(IRequest<TResponse> request, Func<IRequest<TResponse>, StandardDtoReponse<TResponse>> next)
		{
			var stopwatch = new Stopwatch();
			_logger.Debug($"StopwatchMiddleware started at {_currentDateTimeRetriever.Now}");
			var response = next.Invoke(request);
			_logger.Debug($"StopwatchMiddleware ended at {_currentDateTimeRetriever.Now}. Time elapsed: {stopwatch.ElapsedMilliseconds}");
			return response;
		}

		public override void Dispose()
		{
		}
	}
}
