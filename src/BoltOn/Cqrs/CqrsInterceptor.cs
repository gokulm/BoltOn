using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Logging;
using BoltOn.Mediator.Interceptors;
using BoltOn.Mediator.Pipeline;

namespace BoltOn.Cqrs
{
	public class CqrsInterceptor : IInterceptor
	{
		private readonly EventBag _eventBag;
		private readonly IBoltOnLogger<CqrsInterceptor> _logger;
		private readonly IEventDispatcher _eventDispatcher;

		public CqrsInterceptor(EventBag eventBag, IBoltOnLogger<CqrsInterceptor> logger,
			IEventDispatcher eventDispatcher)
		{
			_eventBag = eventBag;
			_logger = logger;
			_eventDispatcher = eventDispatcher;
		}

		public TResponse Run<TRequest, TResponse>(IRequest<TResponse> request,
			Func<IRequest<TResponse>, TResponse> next) where TRequest : IRequest<TResponse>
		{
			var response = next(request);
			if (_eventBag.EventsToBeProcessed.Any())
			{
				throw new NotSupportedException("CQRS not supported for non-async calls");
			}

			return response;
		}

		public async Task<TResponse> RunAsync<TRequest, TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken,
			Func<IRequest<TResponse>, CancellationToken, Task<TResponse>> next) where TRequest : IRequest<TResponse>
		{
			var response = await next(request, cancellationToken);
			foreach (var @event in _eventBag.EventsToBeProcessed.ToList())
			{
				try
				{
					_logger.Debug($"Publishing event. Id: {@event.Id} SourceType: {@event.SourceTypeName}");
					await _eventDispatcher.DispatchAsync(@event, cancellationToken);
				}
				catch(Exception ex)
				{
					_logger.Error($"Dispatching failed. Id: {@event.Id}");
					_logger.Error(ex);
				}
				finally
				{
					_eventBag.EventsToBeProcessed.Remove(@event);
				}
			}

			return response;
		}

		public void Dispose()
		{
		}
	}
}
