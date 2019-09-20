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

        public EventDispatcher(IBoltOnLogger<EventDispatcher> logger,
            IBus bus)
        {
            _logger = logger;
            _bus = bus;
        }

        public async Task DispatchAsync(ICqrsEvent @event, CancellationToken cancellationToken = default)
        {
            _logger.Debug($"Publishing event: {@event.Id} {@event.SourceTypeName}");
            await _bus.PublishAsync(@event, cancellationToken);
        }
    }
}
