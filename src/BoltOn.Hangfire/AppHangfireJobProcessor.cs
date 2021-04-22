using System.Threading;
using System.Threading.Tasks;
using BoltOn.Logging;
using BoltOn.Requestor.Pipeline;
using Hangfire;

namespace BoltOn.Hangfire
{
	public class AppHangfireJobProcessor
	{
		private readonly IRequestor _requestor;
		private readonly IAppLogger<AppHangfireJobProcessor> _logger;

		public AppHangfireJobProcessor(IRequestor requestor,
			IAppLogger<AppHangfireJobProcessor> logger)
		{
			_requestor = requestor;
			_logger = logger;
		}

		[JobDisplayName("{0}")]
		public async Task ProcessAsync<TRequest>(TRequest request, CancellationToken cancellationToken = default)
			where TRequest : IRequest
		{
			_logger.Debug($"Sending request of type {request.GetType().Name} to requestor...");
			await _requestor.ProcessAsync(request, cancellationToken);
			_logger.Debug("Request sent to Requestor");
		}
	}
}
