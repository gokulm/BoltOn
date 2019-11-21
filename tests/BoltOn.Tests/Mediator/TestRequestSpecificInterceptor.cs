using System;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Logging;
using BoltOn.Mediator.Interceptors;
using BoltOn.Mediator.Pipeline;

namespace BoltOn.Tests.Mediator
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

        public override async Task<TResponse> ExecuteAsync<TRequest, TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken,
            Func<IRequest<TResponse>, CancellationToken, Task<TResponse>> next)
        {
            _logger.Debug("TestRequestSpecificInterceptor Started");
            var response = await next.Invoke(request, cancellationToken);
            _logger.Debug("TestRequestSpecificInterceptor Ended");
            return response;
        }
    }
}
