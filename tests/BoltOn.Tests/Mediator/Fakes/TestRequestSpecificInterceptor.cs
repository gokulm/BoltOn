using System;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Logging;
using BoltOn.Requestor.Interceptors;
using BoltOn.Requestor.Pipeline;

namespace BoltOn.Tests.Mediator.Fakes
{
	public class TestRequestSpecificInterceptor : IInterceptor
    {
        private readonly IBoltOnLogger<TestRequestSpecificInterceptor> _logger;

        public TestRequestSpecificInterceptor(IBoltOnLogger<TestRequestSpecificInterceptor> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> RunAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken, 
            Func<TRequest, CancellationToken, Task<TResponse>> next) where TRequest : IRequest<TResponse>
        {
            if (!(request is IEnableInterceptor<TestRequestSpecificInterceptor>))
                return await next.Invoke(request, cancellationToken);

            _logger.Debug("TestRequestSpecificInterceptor Started");
            var response = await next.Invoke(request, cancellationToken);
            _logger.Debug("TestRequestSpecificInterceptor Ended");
            return response;
        }

        public void Dispose()
        {
        }
    }
}
