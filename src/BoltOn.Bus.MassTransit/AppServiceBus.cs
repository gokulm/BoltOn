using System.Threading.Tasks;
using BoltOn.Logging;
using MassTransit;
using System.Threading;
using BoltOn.Requestor.Pipeline;

namespace BoltOn.Bus.MassTransit
{
	public class AppServiceBus : IAppServiceBus
	{
		private readonly IBusControl _busControl;
		private readonly IAppLogger<AppServiceBus> _logger;

		public AppServiceBus(IBusControl busControl,
			IAppLogger<AppServiceBus> logger)
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
