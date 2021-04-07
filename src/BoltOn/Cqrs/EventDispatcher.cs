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

	public class DefaultEventDispatcher : IEventDispatcher
	{
		public Task DispatchAsync(ICqrsEvent @event, CancellationToken cancellationToken = default)
		{
			return Task.CompletedTask;
		}
	}

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

		public async Task DispatchAsync(ICqrsEvent @event, CancellationToken cancellationToken = default)
		{
			_logger.Debug($"Publishing event to bus from EventDispatcher. Id: {@event.Id} SourceType: {@event.EntityType}");
			await _bus.PublishAsync(@event, cancellationToken);
			_logger.Debug("Published event");
		}
	}
}
