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

        public CqrsInterceptor(EventBag eventBag, IBoltOnLogger<CqrsInterceptor> logger,
			IEventDispatcher eventDispatcher, CqrsOptions cqrsOptions)
		{
			_eventBag = eventBag;
			_logger = logger;
			_eventDispatcher = eventDispatcher;
            _cqrsOptions = cqrsOptions;
        }

		public async Task<TResponse> RunAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken,
			Func<TRequest, CancellationToken, Task<TResponse>> next) where TRequest : IRequest<TResponse>
		{
			var response = await next(request, cancellationToken);
			await DispatchEventsToBeProcessed(cancellationToken);
			if (_cqrsOptions.ClearEventsEnabled)
				await DispatchProcessedEvents(cancellationToken);

			return response;
		}

		private async Task DispatchEventsToBeProcessed(CancellationToken cancellationToken)
		{
			Guid eventId;
			try
			{
				foreach (var @event in _eventBag.EventsToBeProcessed.ToList())
				{
					eventId = @event.Id;
					_logger.Debug($"Publishing event. Id: {@event.Id} SourceType: {@event.SourceTypeName}");
					await _eventDispatcher.DispatchAsync(@event, cancellationToken);
					_eventBag.EventsToBeProcessed.Remove(@event);
				}
			}
			catch (Exception ex)
			{
				_logger.Error($"Dispatching failed. Id: {eventId}");
				_logger.Error(ex);
			}
		}

		private async Task DispatchProcessedEvents(CancellationToken cancellationToken)
		{
			Guid eventId;
			try
			{
				foreach (var @event in _eventBag.ProcessedEvents.ToList())
				{
					eventId = @event.Id;
					_logger.Debug($"Publishing processed event. Id: {@event.Id} " +
						$"SourceType: {@event.SourceTypeName}. DestinationType: {@event.DestinationTypeName}");
					var cqrsEventProcessedEvent = new CqrsEventProcessedEvent
					{
						Id = @event.Id,
						SourceId = @event.SourceId,
						DestinationId = @event.DestinationId,
						SourceTypeName = @event.SourceTypeName,
						DestinationTypeName = @event.DestinationTypeName,
						CreatedDate = @event.CreatedDate,
						ProcessedDate = @event.ProcessedDate
					};
					await _eventDispatcher.DispatchAsync(cqrsEventProcessedEvent, cancellationToken);
					_eventBag.ProcessedEvents.Remove(@event);
				}
			}
			catch (Exception ex)
			{
				_logger.Error($"Dispatching failed. Id: {eventId}");
				_logger.Error(ex);
			}
		}

		public void Dispose()
		{
		}
	}
}
