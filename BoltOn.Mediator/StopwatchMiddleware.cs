using System;
using System.Diagnostics;
using BoltOn.Logging;

namespace BoltOn.Mediator
{
    public class StopwatchMiddleware : IMediatorMiddleware
    {
        private readonly IBoltOnLogger<StopwatchMiddleware> _logger;

        public StopwatchMiddleware(IBoltOnLogger<StopwatchMiddleware> logger)
        {
            _logger = logger;
        }

		public StandardDtoReponse<TResponse> Run<TRequest, TResponse>(IRequest<TResponse> request, 
		                                                              Func<IRequest<TResponse>, StandardDtoReponse<TResponse>> next) 
			where TRequest : IRequest<TResponse>
        {
			if (!(request is IEnableStopwatchMiddleware))
				return next.Invoke(request);

            var stopwatch = new Stopwatch();
            _logger.Debug($"StopwatchMiddleware started at {DateTime.Now}");
            var response = next.Invoke(request);
            _logger.Debug($"StopwatchMiddleware ended at {DateTime.Now}. Time elapsed: {stopwatch.ElapsedMilliseconds}");
            return response;
		}

		public void Dispose()
		{
		}
    }

	public interface IEnableStopwatchMiddleware
	{
	}
}
