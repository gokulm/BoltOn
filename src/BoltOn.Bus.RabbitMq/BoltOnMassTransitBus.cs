using System.Threading;
using System.Threading.Tasks;
using BoltOn.Mediator.Pipeline;
using MassTransit;

namespace BoltOn.Bus.RabbitMq
{
	public class BoltOnMassTransitBus : IBus
	{
		private readonly IBusControl _busControl;

		public BoltOnMassTransitBus(IBusControl busControl)
		{
			_busControl = busControl;
		}

		public async Task PublishAsync<TRequest>(TRequest message, CancellationToken cancellationToken = default) 
			where TRequest : IRequest
		{
			await _busControl.Publish(message, cancellationToken).ConfigureAwait(false);
		}
	}
}
