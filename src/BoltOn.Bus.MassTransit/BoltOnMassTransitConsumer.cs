using System.Threading.Tasks;
using BoltOn.Mediator.Pipeline;
using MassTransit;
using BoltOn.Logging;

namespace BoltOn.Bus.MassTransit
{
	public class BoltOnMassTransitConsumer<TRequest> : IConsumer<TRequest> where TRequest : class, IRequest
	{
		private readonly IMediator _mediator;
		private readonly IBoltOnLogger<BoltOnMassTransitConsumer<TRequest>> _logger;

		public BoltOnMassTransitConsumer(IMediator mediator,
			IBoltOnLogger<BoltOnMassTransitConsumer<TRequest>> logger)
		{
			_mediator = mediator;
			_logger = logger;
		}

		public async Task Consume(ConsumeContext<TRequest> context)
		{
			var request = context.Message;
			_logger.Debug($"Message of type {request.GetType().Name} consumer. " +
				"Sending to mediator...");
			await _mediator.ProcessAsync(request, context.CancellationToken);
			_logger.Debug("Message sent to Mediator");
		}
	}
}
