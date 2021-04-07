using System;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Bootstrapping;
using BoltOn.Logging;
using BoltOn.Requestor.Interceptors;
using BoltOn.Requestor.Pipeline;

namespace BoltOn.Cqrs
{
	public class CqrsInterceptor : IInterceptor
	{
		private readonly EventBag _eventBag;
		private readonly IAppLogger<CqrsInterceptor> _logger;
		private readonly IEventDispatcher _eventDispatcher;
		private readonly CqrsOptions _cqrsOptions;

		public CqrsInterceptor(EventBag eventBag, IAppLogger<CqrsInterceptor> logger,
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

			return response;
		}

		private async Task DispatchEventsToBeProcessed(CancellationToken cancellationToken)
		{
			var eventId = Guid.Empty;
			try
			{
				_logger.Debug("About to dispatch EventsToBeProcessed...");
				foreach (var @event in _eventBag.EventsToBeProcessed.Keys)
				{
					eventId = @event.Id;
					_logger.Debug($"Publishing event. Id: {@event.Id} SourceType: {@event.EntityType}");
					await _eventDispatcher.DispatchAsync(@event, cancellationToken);

					if (_cqrsOptions.PurgeEventsToBeProcessed)
					{
						_logger.Debug($"Removing event. Id: {@event.Id}");
						var removeProcessedEventHandle = _eventBag.EventsToBeProcessed[@event];
						await removeProcessedEventHandle(@event);
						_logger.Debug("Removed event");
					}
					_eventBag.RemoveEventToBeProcessed(@event);
				}
			}
			catch (Exception ex)
			{
				_logger.Error($"Dispatching or purging failed. Event Id: {eventId}");
				_logger.Error(ex);
			}
		}

		public void Dispose()
		{
		}
	}
}
