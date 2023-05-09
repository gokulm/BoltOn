using System.Threading.Tasks;
using MassTransit;
using BoltOn.Logger;
using BoltOn.Requestor;

namespace BoltOn.Bus.MassTransit
{
	public class AppMessageConsumer<TRequest> : IConsumer<TRequest> where TRequest : class, IRequest
	{
		private readonly IRequestor _requestor;
		private readonly IAppLogger<AppMessageConsumer<TRequest>> _logger;

		public AppMessageConsumer(IRequestor requestor,
			IAppLogger<AppMessageConsumer<TRequest>> logger)
		{
			_requestor = requestor;
			_logger = logger;
		}

		public async Task Consume(ConsumeContext<TRequest> context)
		{
			var request = context.Message;
			_logger.Debug($"Message of type {request.GetType().Name} consumer. " +
				"Sending to requestor...");
			await _requestor.ProcessAsync(request, context.CancellationToken);
			_logger.Debug("Message sent to Requestor");
		}
	}
}
