using System.Threading;
using System.Threading.Tasks;
using BoltOn.Logging;
using BoltOn.Mediator.Pipeline;
using BoltOn.Data;

namespace BoltOn.Tests.Cqrs
{
	public class AddStudentRequest : IRequest
	{
		public string Name { get; set; }
	}

	public class AddStudentHandler : IHandler<AddStudentRequest>
    {
        private readonly IBoltOnLogger<AddStudentHandler> _logger;
        private readonly IRepository<Student> _repository;

        public AddStudentHandler(IBoltOnLogger<AddStudentHandler> logger,
            IRepository<Student> repository)
        {
            _logger = logger;
            _repository = repository;
        }

        public async Task HandleAsync(AddStudentRequest request, CancellationToken cancellationToken)
        {
            _logger.Debug($"{nameof(AddStudentHandler)} invoked");
            var testCqrsWriteEntity = new Student(request.Name);
            await _repository.AddAsync(testCqrsWriteEntity, cancellationToken);
        }
    }
}
