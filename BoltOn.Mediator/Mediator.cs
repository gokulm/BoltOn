using System;
using BoltOn.IoC;
using BoltOn.Logging;

namespace BoltOn.Mediator
{
	public interface IMediator
	{
		StandardDtoReponse<TResponse> Get<TResponse>(IRequest<TResponse> request);
	}

	public class Mediator : IMediator
	{
		private readonly IoC.IServiceProvider _serviceProvider;
		private readonly IBoltOnLogger<Mediator> _logger;

		//public Mediator(IServiceLocator serviceLocator, IBoltOnLogger<Mediator> logger)
		public Mediator(IoC.IServiceProvider serviceProvider, IBoltOnLogger<Mediator> logger)
		{
			_logger = logger;
			_serviceProvider = serviceProvider;
		}

		public StandardDtoReponse<TResponse> Get<TResponse>(IRequest<TResponse> request)
		{
			try
			{
				var genericRequestHandlerType = typeof(IRequestHandler<,>);
				var interfaceHandlerType =
					genericRequestHandlerType.MakeGenericType(request.GetType(), typeof(TResponse));
				dynamic handler = _serviceProvider.GetInstance(interfaceHandlerType);
				var response = handler.Handle(request);
				if (response is StandardDtoReponse<TResponse>)
					return response;
				return new StandardDtoReponse<TResponse> { Data = response };
			}
			catch (Exception ex)
			{
				//_logger.Error(ex);
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
