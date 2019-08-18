using System;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Mediator.Pipeline;

namespace BoltOn.Bus
{
	public interface IBus
	{
		Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default) 
			where TMessage : IMessage;
	}

	public interface IMessage : IRequest
	{
		Guid CorrelationId { get; set; }
	}

	public abstract class BaseMessage : IMessage
	{
		public Guid CorrelationId { get; set; }
	}
}
