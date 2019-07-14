using System.Threading;
using System.Threading.Tasks;

namespace BoltOn.Bus
{
	public interface IBus
	{
		Task Publish<TMessage>(TMessage message, CancellationToken cancellationToken = default);
	}
}
