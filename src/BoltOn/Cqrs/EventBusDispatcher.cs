using System.Threading;
using System.Threading.Tasks;
using BoltOn.Bus;
using BoltOn.Logging;

namespace BoltOn.Cqrs
{
    public class EventBusDispatcher : IEventDispatcher
    {
        private readonly IBoltOnLogger<EventBusDispatcher> _logger;
        private readonly IBus _bus;

        public EventBusDispatcher(IBoltOnLogger<EventBusDispatcher> logger,
            IBus bus)
        {
            _logger = logger;
            _bus = bus;
        }

        public async Task DispatchAsync(ICqrsEvent @event, CancellationToken cancellationToken = default)
        {
            _logger.Debug($"Publishing event to bus from EventBusDispatcher. Id: {@event.Id} SourceType: {@event.SourceTypeName}");
            await _bus.PublishAsync(@event, cancellationToken);
            _logger.Debug("Published event");
        }
    }
}