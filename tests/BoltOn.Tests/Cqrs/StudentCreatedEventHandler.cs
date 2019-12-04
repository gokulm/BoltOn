using System.Threading;
using System.Threading.Tasks;
using BoltOn.Logging;
using BoltOn.Mediator.Pipeline;
using BoltOn.Data;
using BoltOn.Cqrs;

namespace BoltOn.Tests.Cqrs
{
	public class StudentCreatedEvent : CqrsEvent
	{
		public string Input { get; set; }
	}

	public class StudentCreatedEventHandler : IHandler<StudentCreatedEvent>
    {
        private readonly IBoltOnLogger<StudentCreatedEventHandler> _logger;
        private readonly IRepository<StudentFlattened> _repository;

        public StudentCreatedEventHandler(IBoltOnLogger<StudentCreatedEventHandler> logger,
            IRepository<StudentFlattened> repository)
        {
            _logger = logger;
            _repository = repository;
        }

        public async Task HandleAsync(StudentCreatedEvent request, CancellationToken cancellationToken)
        {
            _logger.Debug($"{nameof(StudentCreatedEventHandler)} invoked");
            var testCqrsReadEntity = new StudentFlattened(request);
			await _repository.AddAsync(testCqrsReadEntity, cancellationToken);
        }
    }
}
