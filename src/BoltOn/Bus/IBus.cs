using System;
using System.Threading;
using System.Threading.Tasks;

namespace BoltOn.Bus
{
	public interface IBus
	{
		Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default) 
			where TMessage : IMessage;
	}
}
