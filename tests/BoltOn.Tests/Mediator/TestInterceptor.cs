using System;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Logging;
using BoltOn.Mediator.Interceptors;
using BoltOn.Mediator.Pipeline;

namespace BoltOn.Tests.Mediator
{
	public class TestInterceptor : IInterceptor
	{
		private readonly IBoltOnLogger<TestInterceptor> _logger;

		public TestInterceptor(IBoltOnLogger<TestInterceptor> logger)
		{
			_logger = logger;
		}

		public MediatorResponse<TResponse> Run<TRequest, TResponse>(IRequest<TResponse> request,
																	 Func<IRequest<TResponse>, MediatorResponse<TResponse>> next)
		   where TRequest : IRequest<TResponse>
		{
			_logger.Debug("TestInterceptor Started");
			var response = next.Invoke(request);
			_logger.Debug("TestInterceptor Ended");
			return response;
		}

		public void Dispose()
		{
		}

		public async Task<MediatorResponse<TResponse>> RunAsync<TRequest, TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken, 
			Func<IRequest<TResponse>, CancellationToken, Task<MediatorResponse<TResponse>>> next) where TRequest : IRequest<TResponse>
		{
			_logger.Debug("TestInterceptor Started");
			var response = await next.Invoke(request, cancellationToken);
			_logger.Debug("TestInterceptor Ended");
			return response;
		}
	}

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

		public override MediatorResponse<TResponse> Execute<TRequest, TResponse>(IRequest<TResponse> request,
																				   Func<IRequest<TResponse>, MediatorResponse<TResponse>> next)
		{
			_logger.Debug($"TestRequestSpecificInterceptor Started");
			var response = next.Invoke(request);
			_logger.Debug($"TestRequestSpecificInterceptor Ended");
			return response;
		}

		public async override Task<MediatorResponse<TResponse>> ExecuteAsync<TRequest, TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken, 
			Func<IRequest<TResponse>, CancellationToken, Task<MediatorResponse<TResponse>>> next)
		{
			_logger.Debug($"TestRequestSpecificInterceptor Started");
			var response = await next.Invoke(request, cancellationToken);
			_logger.Debug($"TestRequestSpecificInterceptor Ended");
			return response;
		}
	}
}
