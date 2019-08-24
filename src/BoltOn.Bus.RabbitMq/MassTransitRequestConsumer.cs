using System.Threading.Tasks;
using BoltOn.Mediator.Pipeline;
using MassTransit;

namespace BoltOn.Bus.RabbitMq
{
	public class MassTransitRequestConsumer<TRequest> : IConsumer<TRequest> where TRequest : class, IRequest
	{
		private readonly IMediator _mediator;

		public MassTransitRequestConsumer(IMediator mediator)
		{
			_mediator = mediator;
		}

		public async Task Consume(ConsumeContext<TRequest> context)
		{
			var request = context.Message;
			await _mediator.ProcessAsync(request);
		}
	}
}
