using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Logging;
using BoltOn.Requestor.Interceptors;
using System.Reflection;

namespace BoltOn.Requestor.Pipeline
{
	public interface IRequestor
	{
		Task<TResponse> ProcessAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
		Task ProcessAsync(IRequest request, CancellationToken cancellationToken = default);
	}

	public class Requestor : IRequestor
	{
		private readonly IBoltOnLogger<Requestor> _logger;
		private readonly IServiceProvider _serviceProvider;
		private readonly IEnumerable<IInterceptor> _interceptors;

		public Requestor(IBoltOnLogger<Requestor> logger, IServiceProvider serviceProvider,
						IEnumerable<IInterceptor> interceptors)
		{
			_logger = logger;
			_serviceProvider = serviceProvider;
			_interceptors = interceptors;
		}

		public async Task<TResponse> ProcessAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
		{
			return await ExecuteInterceptorsAsync(request, HandleAsync, cancellationToken);
		}

		public async Task ProcessAsync(IRequest request, CancellationToken cancellationToken = default)
		{
			await ExecuteInterceptorsAsync(request, HandleAsync, cancellationToken);
		}

		private async Task<TResponse> ExecuteInterceptorsAsync<TResponse>(IRequest<TResponse> request,
			Func<IRequest<TResponse>, CancellationToken, Task<TResponse>> handleAsync, CancellationToken cancellationToken)
		{
			_logger.Debug("Running Interceptors...");
			var next = _interceptors.Reverse().Aggregate(handleAsync,
				   (handleDelegate, interceptor) => (req, token) => interceptor.RunAsync(req, token, handleDelegate));
			try
			{
				return await next.Invoke(request, cancellationToken).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				_logger.Error(ex);
				throw;
			}
			finally
			{
				_interceptors.ToList().ForEach(m => m.Dispose());
			}
		}

		private async Task<TResponse> HandleAsync<TResponse>(IRequest<TResponse> request,
			CancellationToken cancellationToken)
		{
			// this is to keep the request objects in the handlers strongly typed and to keep the handlers implement IHandler
			// and not inherit basehandler. also the requestType can be inferred only if we use MakeGenericType
			if (request is IRequest)
			{
				var requestType = request.GetType();
				_logger.Debug($"Resolving handler for request: {requestType}");
				var genericHandlerType = typeof(IHandler<>);
				var interfaceHandlerType = genericHandlerType.MakeGenericType(request.GetType());
				dynamic handler = _serviceProvider.GetService(interfaceHandlerType);
				if (handler == null)
					throw new Exception($"Handler not found for request: {requestType}");
				_logger.Debug($"Resolved handler: {handler.GetType()}");
				var handleMethod = interfaceHandlerType.GetMethod("HandleAsync");
				await handleMethod.Invoke(handler, BindingFlags.DoNotWrapExceptions, null,
					new object[] { request, cancellationToken }, null).ConfigureAwait(false);
				dynamic response = await Task.FromResult(new DummyResponse());
				return response;
			}
			else
			{
				var requestType = request.GetType();
				_logger.Debug($"Resolving handler for request: {requestType}");
				var genericHandlerType = typeof(IHandler<,>);
				var interfaceHandlerType = genericHandlerType.MakeGenericType(request.GetType(), typeof(TResponse));
				dynamic handler = _serviceProvider.GetService(interfaceHandlerType);
				if (handler == null)
					throw new Exception($"Handler not found for request: {requestType}");
				_logger.Debug($"Resolved handler: {handler.GetType()}");
				var handleMethod = interfaceHandlerType.GetMethod("HandleAsync");
				var response = await handleMethod.Invoke(handler, BindingFlags.DoNotWrapExceptions, null,
					new object[] { request, cancellationToken }, null).ConfigureAwait(false);
				return response;
			}
		}
	}
}
