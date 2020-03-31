using System;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Logging;
using BoltOn.Requestor.Interceptors;
using BoltOn.Requestor.Pipeline;

namespace BoltOn.Tests.Mediator.Fakes
{
	public class TestInterceptor : IInterceptor
	{
		private readonly IBoltOnLogger<TestInterceptor> _logger;

		public TestInterceptor(IBoltOnLogger<TestInterceptor> logger)
		{
			_logger = logger;
		}

		public void Dispose()
		{
		}

		public async Task<TResponse> RunAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken, 
			Func<TRequest, CancellationToken, Task<TResponse>> next) where TRequest : IRequest<TResponse>
		{
			_logger.Debug("TestInterceptor Started");
			var response = await next.Invoke(request, cancellationToken);
			_logger.Debug("TestInterceptor Ended");
			return response;
		}
	}
}
