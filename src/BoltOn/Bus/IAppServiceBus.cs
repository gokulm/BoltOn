using System.Threading;
using System.Threading.Tasks;
using BoltOn.Requestor;

namespace BoltOn.Bus
{
	public interface IAppServiceBus
	{
		Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default) 
			where TMessage : IRequest;
	}
}
