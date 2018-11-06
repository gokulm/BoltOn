using System;
using System.Collections.Generic;
using System.Linq;
using BoltOn.Logging;
using BoltOn.Utilities;

namespace BoltOn.Mediator
{
	public interface IMediator
	{
		StandardDtoReponse<TResponse> Get<TResponse>(IRequest<TResponse> request);
	}

	public class Mediator : IMediator
	{
		private readonly IoC.IServiceFactory _serviceFactory;
		private readonly IBoltOnLogger<Mediator> _logger;
		private readonly IServiceProvider _serviceProvider;
		private readonly IEnumerable<IMediatorMiddleware> _middlewares;

		public Mediator(IoC.IServiceFactory serviceFactory, IBoltOnLogger<Mediator> logger, IServiceProvider serviceProvider,
		                IEnumerable<IMediatorMiddleware> middlewares)
		{
			_logger = logger;
			this._serviceProvider = serviceProvider;
			this._middlewares = middlewares;
			_serviceFactory = serviceFactory;
		}

		public StandardDtoReponse<TResponse> Get<TResponse>(IRequest<TResponse> request)
		{
			return RunMiddleware(request, Handle);
		}

		private StandardDtoReponse<TResponse> RunMiddleware<TResponse>(IRequest<TResponse> request,
			Func<IRequest<TResponse>, StandardDtoReponse<TResponse>> handle)
		{
			_logger.Debug("Running middlewares...");
			var next = _middlewares.Reverse().Aggregate(handle,
				   (requestDelegate, middleware) => ((req) => middleware.Run<IRequest<TResponse>, TResponse>(req, requestDelegate)));
			try
			{
				return next.Invoke(request);
			}
			catch (Exception ex)
			{
				_logger.Error(ex);
				return new StandardDtoReponse<TResponse>
				{
					IsSuccessful = false,
					Exception = ex
				};
			}
			finally
			{
				_middlewares.ToList().ForEach(m => m.Dispose());
			}
		}

		private StandardDtoReponse<TResponse> Handle<TResponse>(IRequest<TResponse> request)
		{
			var requestType = request.GetType();
			_logger.Debug($"Resolving handler for request: {requestType}");
			var genericRequestHandlerType = typeof(IRequestHandler<,>);
			var interfaceHandlerType =
				genericRequestHandlerType.MakeGenericType(request.GetType(), typeof(TResponse));
			//dynamic handler = _serviceFactory.GetInstance(interfaceHandlerType);
			// todo (med): try to get rid of dynamic
			dynamic handler = _serviceProvider.GetService(interfaceHandlerType);
			//var test = handler as IRequestHandler<
			Check.Requires(handler != null, string.Format(Constants.ExceptionMessages.HANDLER_NOT_FOUND, requestType));
			_logger.Debug($"Resolved handler: {handler.GetType()}");
			var response = handler.Handle(request);
			return new StandardDtoReponse<TResponse>
			{
				Data = response,
				IsSuccessful = true
			};
		}
	}
}
