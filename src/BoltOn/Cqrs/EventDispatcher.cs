using System;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Bus;
using BoltOn.Logging;

namespace BoltOn.Cqrs
{
	public interface IEventDispatcher
	{
		Task DispatchAsync(ICqrsEvent @event, CancellationToken cancellationToken = default);
	}

	public class EventDispatcher : IEventDispatcher
	{
		private readonly IBoltOnLogger<EventDispatcher> _logger;
		private readonly IBus _bus;
		private readonly IProcessedEventPurger _processedEventPurger;

		public EventDispatcher(IBoltOnLogger<EventDispatcher> logger,
			IBus bus,
			IProcessedEventPurger processedEventPurger)
		{
			_logger = logger;
			_bus = bus;
			_processedEventPurger = processedEventPurger;
		}

		public async Task DispatchAsync(ICqrsEvent @event, CancellationToken cancellationToken = default)
		{
			_logger.Debug($"Publishing event to bus from EventDispatcher. Id: {@event.Id} SourceType: {@event.SourceTypeName}");
			await _bus.PublishAsync(@event, cancellationToken);
			_logger.Debug("Published event");
			await _processedEventPurger.PurgeAsync(@event, cancellationToken);
		}
	}

	public class DefaultEventDispatcher : IEventDispatcher
	{
		public Task DispatchAsync(ICqrsEvent @event, CancellationToken cancellationToken = default)
		{
			return Task.CompletedTask;
		}
	}
}
