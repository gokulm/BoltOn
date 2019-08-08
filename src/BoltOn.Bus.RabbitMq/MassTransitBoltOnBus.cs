using System.Threading;
using System.Threading.Tasks;
using MassTransit;

namespace BoltOn.Bus.RabbitMq
{
	public class MassTransitBoltOnBus : IBus
	{
		private readonly IBusControl _busControl;

		public MassTransitBoltOnBus(IBusControl busControl)
		{
			_busControl = busControl;
		}


		public async Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default) 
			where TMessage : IMessage
		{
			await _busControl.Publish(message, cancellationToken).ConfigureAwait(false);
		}
	}
}
