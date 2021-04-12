using System;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Bus;
using BoltOn.Logging;

namespace BoltOn.Cqrs
{
	// todo: this interface can be removed
	[Obsolete]
	public interface IEventDispatcher
	{
		Task DispatchAsync(IDomainEvent @event, CancellationToken cancellationToken = default);
    }

	// todo: this class can be removed
	[Obsolete]
	public class DefaultEventDispatcher : IEventDispatcher
	{
		public Task DispatchAsync(IDomainEvent @event, CancellationToken cancellationToken = default)
		{
			return Task.CompletedTask;
		}
	}

	// todo: this class can be removed
	[Obsolete]
	public class EventDispatcher : IEventDispatcher
	{
		private readonly IAppLogger<EventDispatcher> _logger;
		private readonly IAppServiceBus _bus;

		public EventDispatcher(IAppLogger<EventDispatcher> logger,
			IAppServiceBus bus)
		{
			_logger = logger;
			_bus = bus;
		}

		public async Task DispatchAsync(IDomainEvent @event, CancellationToken cancellationToken = default)
		{
			_logger.Debug($"Publishing event to bus from EventDispatcher. Id: {@event.Id} SourceType: {@event.EntityType}");
			await _bus.PublishAsync(@event, cancellationToken);
			_logger.Debug("Published event");
		}
	}
}
