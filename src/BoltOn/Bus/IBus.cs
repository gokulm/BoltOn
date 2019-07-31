using System;
using System.Threading;
using System.Threading.Tasks;

namespace BoltOn.Bus
{
	public interface IBus
	{
		Task Publish<TMessage>(TMessage message, CancellationToken cancellationToken = default) where TMessage : IMessage;
	}

	public interface IMessage
	{
		Guid CorrelationId { get; set; }
	}

	public abstract class BaseMessage : IMessage
	{
		public Guid CorrelationId { get; set; }
	}

	public interface IMessageAsyncHandler<in TMessage>
		where TMessage : IMessage
	{
		Task HandleAsync(TMessage message, CancellationToken cancellationToken);
	}
}
