using System.Threading;
using System.Threading.Tasks;
using MassTransit;

namespace BoltOn.Bus.RabbitMq
{
	public class MassTransitBus : IBus
	{
		private readonly IBusControl _busControl;

		public MassTransitBus(IBusControl busControl)
		{
			_busControl = busControl;
		}


		public async Task Publish<TMessage>(TMessage message, CancellationToken cancellationToken = default)
		{
			await _busControl.Publish(message, cancellationToken).ConfigureAwait(false);
		}
	}
}
