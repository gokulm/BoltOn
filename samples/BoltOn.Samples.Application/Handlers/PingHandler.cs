using System.Threading;
using System.Threading.Tasks;
using BoltOn.Logging;
using BoltOn.Requestor;

namespace BoltOn.Samples.Application.Handlers
{
	public class PingRequest : IRequest<PongResponse>
	{
	}

	public class PongResponse
	{
		public string Data { get; set; }
	}

	public class PingHandler : IHandler<PingRequest, PongResponse>
	{
		private readonly IAppLogger<PingHandler> _logger;

		public PingHandler(IAppLogger<PingHandler> logger)
		{
			_logger = logger;
		}

		public async Task<PongResponse> HandleAsync(PingRequest request, CancellationToken cancellationToken)
		{
			_logger.Info("About to ping...");
			return await Task.FromResult(new PongResponse { Data = "pong" });
		}
	}
}
