using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Logging;
using BoltOn.Mediator.Middlewares;
using BoltOn.Utilities;

namespace BoltOn.Mediator.Pipeline
{
	public interface IMediator
	{
		MediatorResponse<TResponse> Get<TResponse>(IRequest<TResponse> request);
		Task<MediatorResponse<TResponse>> GetAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default(CancellationToken));
	}

	public class Mediator : IMediator
	{
		private readonly IBoltOnLogger<Mediator> _logger;
		private readonly IServiceProvider _serviceProvider;
		private readonly IEnumerable<IMediatorMiddleware> _middlewares;

		public Mediator(IBoltOnLogger<Mediator> logger, IServiceProvider serviceProvider,
						IEnumerable<IMediatorMiddleware> middlewares)
		{
			_logger = logger;
			this._serviceProvider = serviceProvider;
			this._middlewares = middlewares;
		}

		public MediatorResponse<TResponse> Get<TResponse>(IRequest<TResponse> request)
		{
			return ExecuteMiddlewares(request, Handle);
		}

		public async Task<MediatorResponse<TResponse>> GetAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default(CancellationToken))
		{
			return await ExecuteMiddlewaresAsync(request, HandleAsync, cancellationToken);
		}

		private MediatorResponse<TResponse> ExecuteMiddlewares<TResponse>(IRequest<TResponse> request,
			Func<IRequest<TResponse>, MediatorResponse<TResponse>> handle)
		{
			_logger.Debug("Running middlewares...");
			var next = _middlewares.Reverse().Aggregate(handle,
				   (handleDelegate, middleware) => (req) => middleware.Run<IRequest<TResponse>, TResponse>(req, handleDelegate));
			try
			{
				return next.Invoke(request);
			}
			catch (Exception ex)
			{
				_logger.Error(ex);
				throw;
			}
			finally
			{
				_middlewares.ToList().ForEach(m => m.Dispose());
			}
		}

		private async Task<MediatorResponse<TResponse>> ExecuteMiddlewaresAsync<TResponse>(IRequest<TResponse> request,
			Func<IRequest<TResponse>, CancellationToken, Task<MediatorResponse<TResponse>>> handleAsync, CancellationToken cancellationToken)
		{
			_logger.Debug("Running middlewares...");
			var next = _middlewares.Reverse().Aggregate(handleAsync,
				   (handleDelegate, middleware) => (req, token) => middleware.RunAsync<IRequest<TResponse>, TResponse>(req, token, handleDelegate));
			try
			{
				return await next.Invoke(request, cancellationToken);
			}
			catch (Exception ex)
			{
				_logger.Error(ex);
				return await Task.FromResult(new MediatorResponse<TResponse>
				{
					IsSuccessful = false,
					Exception = ex
				});
			}
			finally
			{
				_middlewares.ToList().ForEach(m => m.Dispose());
			}
		}

		private MediatorResponse<TResponse> Handle<TResponse>(IRequest<TResponse> request)
		{
			var requestType = request.GetType();
			_logger.Debug($"Resolving handler for request: {requestType}");
			var genericRequestHandlerType = typeof(IRequestHandler<,>);
			var interfaceHandlerType = genericRequestHandlerType.MakeGenericType(request.GetType(), typeof(TResponse));
			var handler = _serviceProvider.GetService(interfaceHandlerType);
			Check.Requires(handler != null, string.Format(Constants.ExceptionMessages.HANDLER_NOT_FOUND, requestType));
			_logger.Debug($"Resolved handler: {handler.GetType()}");
			// this is to keep the request objects in the handlers strongly typed and to keep the handlers implement IRequestHandler
			// and not inherit baserequesthandler
			var decorator = (BaseRequestHandlerDecorator<TResponse>)Activator.CreateInstance(typeof(RequestHandlerDecorator<,>)
																					   .MakeGenericType(requestType, typeof(TResponse)), handler);
			var response = decorator.Handle(request);
			return new MediatorResponse<TResponse>
			{
				Data = response,
				IsSuccessful = true
			};
		}

		private async Task<MediatorResponse<TResponse>> HandleAsync<TResponse>(IRequest<TResponse> request,
			CancellationToken cancellationToken)
		{
			var requestType = request.GetType();
			_logger.Debug($"Resolving handler for request: {requestType}");
			var genericRequestHandlerType = typeof(IRequestAsyncHandler<,>);
			var interfaceHandlerType = genericRequestHandlerType.MakeGenericType(request.GetType(), typeof(TResponse));
			var handler = _serviceProvider.GetService(interfaceHandlerType);
			Check.Requires(handler != null, string.Format(Constants.ExceptionMessages.HANDLER_NOT_FOUND, requestType));
			_logger.Debug($"Resolved handler: {handler.GetType()}");
			// this is to keep the request objects in the handlers strongly typed and to keep the handlers implement IRequestHandler
			// and not inherit baserequesthandler
			var decorator = (BaseRequestAsyncHandlerDecorator<TResponse>)Activator.CreateInstance(typeof(RequestAsyncHandlerDecorator<,>)
																					   .MakeGenericType(requestType, typeof(TResponse)), handler);
			var response = await decorator.HandleAsync(request, cancellationToken);
			return await Task.FromResult(new MediatorResponse<TResponse>
			{
				Data = response,
				IsSuccessful = true
			});
		}
	}
}
