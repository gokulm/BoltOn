using System.Threading.Tasks;
using BoltOn.Logging;
using BoltOn.Mediator.Pipeline;
using MassTransit;
using System.Threading;

namespace BoltOn.Bus.MassTransit
{
	public class BoltOnMassTransitBus : IBus
	{
		private readonly IBusControl _busControl;
		private readonly IBoltOnLogger<BoltOnMassTransitBus> _logger;

		public BoltOnMassTransitBus(IBusControl busControl, 
			IBoltOnLogger<BoltOnMassTransitBus> logger)
		{
			_busControl = busControl;
			_logger = logger;
		}

		public async Task PublishAsync<TRequest>(TRequest message, CancellationToken cancellationToken = default) 
			where TRequest : IRequest
		{
			_logger.Debug($"Publishing message of type - {message.GetType().Name} ...");
			await _busControl.Publish(message, cancellationToken).ConfigureAwait(false);
			_logger.Debug("Message published");
		}
	}
}
