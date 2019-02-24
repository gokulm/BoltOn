using System;
using System.Threading;
using System.Threading.Tasks;
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

		public async Task<MediatorResponse<TResponse>> RunAsync<TRequest, TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken, 
			Func<IRequest<TResponse>, CancellationToken, Task<MediatorResponse<TResponse>>> next) where TRequest : IRequest<TResponse>
		{
			_logger.Debug("TestMiddleware Started");
			var response = await next.Invoke(request, cancellationToken);
			_logger.Debug("TestMiddleware Ended");
			return response;
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

		public async override Task<MediatorResponse<TResponse>> ExecuteAsync<TRequest, TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken, 
			Func<IRequest<TResponse>, CancellationToken, Task<MediatorResponse<TResponse>>> next)
		{
			_logger.Debug($"TestRequestSpecificMiddleware Started");
			var response = await next.Invoke(request, cancellationToken);
			_logger.Debug($"TestRequestSpecificMiddleware Ended");
			return response;
		}
	}
}
