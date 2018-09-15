using System;
using BoltOn.IoC;
using BoltOn.Logging;

namespace BoltOn.MediatorModule
{
	public interface IRequest<out TResponse>
	{
	}

	public class StandardDtoReponse<TResponse>
	{
		public TResponse Data { get; set; }
		public bool IsSuccessful { get; set; }
		public Exception Exception
		{
			get;
			set;
		}
	}

	public interface IRequestHandler<in TRequest, TResponse> where TRequest : IRequest<TResponse>
	{
		TResponse Handle(IRequest<TResponse> request);
	}

	public interface IMediator
	{
		StandardDtoReponse<TResponse> Get<TResponse>(IRequest<TResponse> request);
	}

	public class Mediator
	{
		private readonly IServiceLocator _serviceLocator;
		private readonly IBoltOnLogger<Mediator> _logger;

		public Mediator(IServiceLocator serviceLocator, IBoltOnLogger<Mediator> logger)
		{
			_logger = logger;
			_serviceLocator = serviceLocator;
		}

		public StandardDtoReponse<TResponse> Get<TResponse>(IRequest<TResponse> request)
		{
			try
			{
				var genericRequestHandlerType = typeof(IRequestHandler<,>);
				var interfaceHandlerType =
					genericRequestHandlerType.MakeGenericType(request.GetType(), typeof(TResponse));
				dynamic handler = _serviceLocator.GetInstance(interfaceHandlerType);
				var response = handler.Handle(request);
				if (response is StandardDtoReponse<TResponse>)
					return response;
				return new StandardDtoReponse<TResponse> { Data = response };
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
