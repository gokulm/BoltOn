using System.Threading;
using System.Threading.Tasks;
using BoltOn.Cqrs;
using BoltOn.Data;
using BoltOn.Mediator.Pipeline;
using BoltOn.Samples.Application.Entities;

namespace BoltOn.Samples.Application.Handlers
{
	public class StudentCreatedEvent : EventToBeProcessed
	{
	}

	public class StudentCreatedEventHandler : IRequestAsyncHandler<StudentCreatedEvent>
    {
        private readonly IRepository<StudentFlattened> _repository;

        public StudentCreatedEventHandler(IRepository<StudentFlattened> repository)
        {
            _repository = repository;
        }

        public async Task HandleAsync(StudentCreatedEvent request, CancellationToken cancellationToken)
        {
            await _repository.AddAsync(new StudentFlattened(request), cancellationToken);
        }
    }
}
