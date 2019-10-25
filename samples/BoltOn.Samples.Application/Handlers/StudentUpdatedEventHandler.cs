using System;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Cqrs;
using BoltOn.Mediator.Pipeline;
using BoltOn.Samples.Application.Abstractions.Data;

namespace BoltOn.Samples.Application.Handlers
{
	public class StudentUpdatedEvent : CqrsEvent
	{
		public Guid StudentId { get; set; }

		public string FirstName { get; set; }

		public string LastName { get; set; }

		public string StudentType { get; set; }

		public int StudentTypeId { get; set; }
	}

	public class StudentUpdatedEventHandler : IRequestAsyncHandler<StudentUpdatedEvent>
    {
        private readonly IStudentFlattenedRepository _repository;

        public StudentUpdatedEventHandler(IStudentFlattenedRepository repository)
        {
            _repository = repository;
        }

        public async Task HandleAsync(StudentUpdatedEvent request, CancellationToken cancellationToken)
        {
            var student = await _repository.GetAsync(request.StudentId, request.StudentTypeId, cancellationToken);
			student.Update(request);
            await _repository.UpdateAsync(student, cancellationToken);
        }
    }
}
