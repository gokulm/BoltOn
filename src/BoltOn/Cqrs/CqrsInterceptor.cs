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
		private readonly IProcessedEventPurger _processedEventPurger;

		public CqrsInterceptor(EventBag eventBag, IBoltOnLogger<CqrsInterceptor> logger,
			IEventDispatcher eventDispatcher,
			IProcessedEventPurger processedEventPurger)
		{
			_eventBag = eventBag;
			_logger = logger;
			_eventDispatcher = eventDispatcher;
			_processedEventPurger = processedEventPurger;
		}

		public TResponse Run<TRequest, TResponse>(IRequest<TResponse> request,
			Func<IRequest<TResponse>, TResponse> next) where TRequest : IRequest<TResponse>
		{
			var response = next(request);
			foreach (var @event in _eventBag.Events)
			{
				_logger.Debug($"Publishing event: {@event.Id} {@event.SourceTypeName}");
			}

			return response;
		}

		public async Task<TResponse> RunAsync<TRequest, TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken,
			Func<IRequest<TResponse>, CancellationToken, Task<TResponse>> next) where TRequest : IRequest<TResponse>
		{
			var response = await next(request, cancellationToken);
			// creating a new list by calling .ToList() as the events in the original list need to be removed
			foreach (var @event in _eventBag.Events.ToList())
			{
				_logger.Debug($"Publishing event. Id: {@event.Id} SourceType: {@event.SourceTypeName}");
				await _eventDispatcher.DispatchAsync(@event, cancellationToken);
				_eventBag.Events.Remove(@event);
			}

			if (request is CqrsEvent cqrsEvent)
			{
				await _processedEventPurger.PurgeAsync(cqrsEvent);
			}

			return response;
		}

		public void Dispose()
		{
		}
	}
}
