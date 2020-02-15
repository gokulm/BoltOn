using System.Threading;
using System.Threading.Tasks;

namespace BoltOn.Cqrs
{
	public interface IEventDispatcher
	{
		Task DispatchAsync(ICqrsEvent @event, CancellationToken cancellationToken = default);
    }

    public class EventDispatcher : IEventDispatcher
    {
        public Task DispatchAsync(ICqrsEvent @event, CancellationToken cancellationToken = default)
        {
			return Task.CompletedTask;
        }
    }
}
