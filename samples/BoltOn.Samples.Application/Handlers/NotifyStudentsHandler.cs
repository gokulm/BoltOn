using System.Threading;
using System.Threading.Tasks;
using BoltOn.Logging;
using BoltOn.Requestor.Pipeline;

namespace BoltOn.Samples.Application.Handlers
{
	public class NotifyStudentsRequest : IRequest
    {
		public string JobType { get; set; }

		public override string ToString()
		{
			return "Student Notifier " + JobType;
		}
	}

    public class NotifyStudentsHandler : IHandler<NotifyStudentsRequest>
    {
		private readonly IAppLogger<NotifyStudentsHandler> _logger;

		public NotifyStudentsHandler(IAppLogger<NotifyStudentsHandler> logger)
		{
			_logger = logger;
		}

        public Task HandleAsync(NotifyStudentsRequest request, CancellationToken cancellationToken)
        {
            _logger.Debug($"Notifying students. JobType: {request.JobType}");
            return Task.CompletedTask;
        }
    }
}
