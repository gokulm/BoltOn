﻿using System.Threading.Tasks;
using MassTransit;
using BoltOn.Logging;
using BoltOn.Requestor.Pipeline;

namespace BoltOn.Bus.MassTransit
{
	public class BoltOnMassTransitConsumer<TRequest> : IConsumer<TRequest> where TRequest : class, IRequest
	{
		private readonly IRequestor _requestor;
		private readonly IAppLogger<BoltOnMassTransitConsumer<TRequest>> _logger;

		public BoltOnMassTransitConsumer(IRequestor requestor,
			IAppLogger<BoltOnMassTransitConsumer<TRequest>> logger)
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
