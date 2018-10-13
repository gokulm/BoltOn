﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using BoltOn.Logging;

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

        public Mediator(IoC.IServiceFactory serviceFactory, IBoltOnLogger<Mediator> logger)
        {
            _logger = logger;
            _serviceFactory = serviceFactory;
        }

        public StandardDtoReponse<TResponse> Get<TResponse>(IRequest<TResponse> request)
        {
			return RunMiddleware<TResponse>(request, Handle);
        }

		private StandardDtoReponse<TResponse> RunMiddleware<TResponse>(IRequest<TResponse> request, 
			Func<IRequest<TResponse>, StandardDtoReponse<TResponse>> handle) 
        {
			var middlewares = (IEnumerable<IMiddleware>)
                _serviceFactory.GetInstance(typeof(IEnumerable<IMiddleware>));
            var next = middlewares.Reverse().Aggregate(handle, (requestDelegate, middleware) =>
			                                           ((req) => middleware.Run<IRequest<TResponse>, TResponse>(req, requestDelegate)));
            return next.Invoke(request);
        }

        private StandardDtoReponse<TResponse> Handle<TResponse>(IRequest<TResponse> request)
        {
            try
            {
                var requestType = request.GetType();
                _logger.Debug($"Resolving handler for request: {requestType}");
                var genericRequestHandlerType = typeof(IRequestHandler<,>);
                var interfaceHandlerType =
                    genericRequestHandlerType.MakeGenericType(request.GetType(), typeof(TResponse));
                dynamic handler = _serviceFactory.GetInstance(interfaceHandlerType);
				Contract.Requires<Exception>(handler != null, string.Format(Constants.ExceptionMessages.HANDLER_NOT_FOUND, requestType));
                _logger.Debug($"Resolved handler: {handler.GetType()}");
                var response = handler.Handle(request);
                return new StandardDtoReponse<TResponse>
                {
                    Data = response,
                    IsSuccessful = true
                };
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
            }
        }
    }
}
