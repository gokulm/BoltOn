using System.Threading;
using System.Threading.Tasks;
using BoltOn.Logging;
using BoltOn.Requestor.Pipeline;

namespace BoltOn.Tests.Bus.Fakes
{
	public class CreateTestStudent : IRequest
	{
		public string FirstName { get; set; }
	}

	public class CreateTestStudentHandler : IHandler<CreateTestStudent>
    {
        private readonly IAppLogger<CreateTestStudentHandler> _logger;

        public CreateTestStudentHandler(IAppLogger<CreateTestStudentHandler> logger)
        {
            _logger = logger;
        }

        public async Task HandleAsync(CreateTestStudent request, CancellationToken cancellationToken)
        {
            _logger.Debug($"{nameof(CreateTestStudentHandler)} invoked");
            await Task.FromResult("testing");
        }
    }
}
