using System;
using BoltOn.Logging;
using BoltOn.Mediator.Middlewares;
using BoltOn.Mediator.Pipeline;

namespace BoltOn.Tests.Mediator
{
    public class TestMiddleware : IMediatorMiddleware
    {
        private readonly IBoltOnLogger<TestMiddleware> _logger;

        public TestMiddleware(IBoltOnLogger<TestMiddleware> logger)
        {
            _logger = logger;
        }

        public MediatorResponse<TResponse> Run<TRequest, TResponse>(IRequest<TResponse> request,
                                                                     Func<IRequest<TResponse>, MediatorResponse<TResponse>> next)
           where TRequest : IRequest<TResponse>
        {
            _logger.Debug("TestMiddleware Started");
            var response = next.Invoke(request);
            _logger.Debug("TestMiddleware Ended");
            return response;
        }

        public void Dispose()
        {
        }
	}

	public interface IRequestSpecificMiddleware
	{
	}

	public class TestRequestSpecificMiddleware : BaseRequestSpecificMiddleware<IRequestSpecificMiddleware>
	{
		private readonly IBoltOnLogger<TestMiddleware> _logger;

		public TestRequestSpecificMiddleware(IBoltOnLogger<TestMiddleware> logger)
		{
			_logger = logger;
		}

		public override void Dispose()
		{
		}

		public override MediatorResponse<TResponse> Execute<TRequest, TResponse>(IRequest<TResponse> request,
																				   Func<IRequest<TResponse>, MediatorResponse<TResponse>> next)
		{
			_logger.Debug($"TestRequestSpecificMiddleware Started");
			var response = next.Invoke(request);
			_logger.Debug($"TestRequestSpecificMiddleware Ended");
			return response;
		}
	}
}
