using System.Threading;
using System.Threading.Tasks;
using BoltOn.Logging;
using BoltOn.Data;
using BoltOn.Cqrs;
using BoltOn.Requestor.Pipeline;

namespace BoltOn.Tests.Cqrs.Fakes
{
	public class StudentCreatedEvent : BaseDomainEvent<Student>
	{
		public string Input { get; set; }
	}

	public class StudentCreatedEventHandler : IHandler<StudentCreatedEvent>
    {
        private readonly IAppLogger<StudentCreatedEventHandler> _logger;
        private readonly IRepository<StudentFlattened> _repository;

        public StudentCreatedEventHandler(IAppLogger<StudentCreatedEventHandler> logger,
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
