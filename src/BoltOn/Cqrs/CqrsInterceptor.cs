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

		public async Task<TResponse> RunAsync<TRequest, TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken,
			Func<IRequest<TResponse>, CancellationToken, Task<TResponse>> next) where TRequest : IRequest<TResponse>
		{
			var response = await next(request, cancellationToken);
			await DispatchEventsToBeProcessed(cancellationToken);
			if (Bootstrapping.Bootstrapper.Instance.Options.CqrsOptions.ClearEventsEnabled)
				await DispatchProcessedEvents(cancellationToken);

			return response;
		}

		private async Task DispatchEventsToBeProcessed(CancellationToken cancellationToken)
		{
			foreach (var @event in _eventBag.EventsToBeProcessed.ToList())
			{
				try
				{
					_logger.Debug($"Publishing event. Id: {@event.Id} SourceType: {@event.SourceTypeName}");
					await _eventDispatcher.DispatchAsync(@event, cancellationToken);
				}
				catch (Exception ex)
				{
					_logger.Error($"Dispatching failed. Id: {@event.Id}");
					_logger.Error(ex);
				}
				finally
				{
					_eventBag.EventsToBeProcessed.Remove(@event);
				}
			}
		}

		private async Task DispatchProcessedEvents(CancellationToken cancellationToken)
		{
			foreach (var @event in _eventBag.ProcessedEvents.ToList())
			{
				try
				{
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
				}
				catch (Exception ex)
				{
					_logger.Error($"Dispatching failed. Id: {@event.Id}");
					_logger.Error(ex);
				}
				finally
				{
					_eventBag.ProcessedEvents.Remove(@event);
				}
			}
		}

		public void Dispose()
		{
		}
	}
}
