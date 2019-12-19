using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Bootstrapping;
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
		private readonly CqrsOptions _cqrsOptions;
		private readonly IEventPurger _eventPurger;

		public CqrsInterceptor(EventBag eventBag, IBoltOnLogger<CqrsInterceptor> logger,
			IEventDispatcher eventDispatcher, CqrsOptions cqrsOptions,
			IEventPurger eventPurger)
		{
			_eventBag = eventBag;
			_logger = logger;
			_eventDispatcher = eventDispatcher;
			_cqrsOptions = cqrsOptions;
			_eventPurger = eventPurger;
		}

		public async Task<TResponse> RunAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken,
			Func<TRequest, CancellationToken, Task<TResponse>> next) where TRequest : IRequest<TResponse>
		{
			var response = await next(request, cancellationToken);
			await DispatchEventsToBeProcessed(cancellationToken);

			return response;
		}

		private async Task DispatchEventsToBeProcessed(CancellationToken cancellationToken)
		{
			Guid eventId;
			try
			{
				_logger.Debug("About to dispatch EventsToBeProcessed...");
				foreach (var @event in _eventBag.EventsToBeProcessed.ToList())
				{
					eventId = @event.Id;
					_logger.Debug($"Publishing event. Id: {@event.Id} SourceType: {@event.SourceTypeName}");
					await _eventDispatcher.DispatchAsync(@event, cancellationToken);
					_eventBag.EventsToBeProcessed.Remove(@event);

					if (_cqrsOptions.PurgeEventsToBeProcessed)
						await PurgeEvent(@event, cancellationToken);
				}
			}
			catch (Exception ex)
			{
				_logger.Error($"Dispatching failed. Id: {eventId}");
				_logger.Error(ex);
			}
		}

		private async Task PurgeEvent(ICqrsEvent cqrsEvent, CancellationToken cancellationToken)
		{
			try
			{
				await _eventPurger.PurgeAsync(cqrsEvent, cancellationToken);
			}
			catch (Exception ex)
			{
				_logger.Error(ex);
			}
		}

		public void Dispose()
		{
		}
	}
}
