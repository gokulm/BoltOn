using System.Threading;
using System.Threading.Tasks;

namespace BoltOn.Cqrs
{
    public class DefaultEventDispatcher : IEventDispatcher
    {
        public Task DispatchAsync(ICqrsEvent @event, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
