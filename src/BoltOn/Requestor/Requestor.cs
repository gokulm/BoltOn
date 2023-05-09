using BoltOn.Logger;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace BoltOn.Requestor
{
    public interface IRequestor
	{
		Task<TResponse> ProcessAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
		Task ProcessAsync(IRequest request, CancellationToken cancellationToken = default);
	}

	public class Requestor : IRequestor
	{
		private readonly IAppLogger<Requestor> _logger;
		private readonly IServiceProvider _serviceProvider;

		public Requestor(IAppLogger<Requestor> logger, 
			IServiceProvider serviceProvider)
		{
			_logger = logger;
			_serviceProvider = serviceProvider;
		}

		public async Task<TResponse> ProcessAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
		{
			return await HandleAsync(request, cancellationToken);
		}

		public async Task ProcessAsync(IRequest request, CancellationToken cancellationToken = default)
		{
			await HandleAsync(request, cancellationToken);
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
