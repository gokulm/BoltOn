using System;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Logging;
using BoltOn.Mediator.Interceptors;

namespace BoltOn.Tests.Mediator.Fakes
{
	public interface IRequestSpecificInterceptor
	{
	}

	public class TestRequestSpecificInterceptor : BaseRequestSpecificInterceptor<IRequestSpecificInterceptor>
    {
        private readonly IBoltOnLogger<TestInterceptor> _logger;

        public TestRequestSpecificInterceptor(IBoltOnLogger<TestInterceptor> logger)
        {
            _logger = logger;
        }

        public override void Dispose()
        {
        }

        public override async Task<TResponse> ExecuteAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken,
            Func<TRequest, CancellationToken, Task<TResponse>> next)
        {
            _logger.Debug("TestRequestSpecificInterceptor Started");
            var response = await next.Invoke(request, cancellationToken);
            _logger.Debug("TestRequestSpecificInterceptor Ended");
            return response;
        }
    }
}
