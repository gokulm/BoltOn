using System;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Cqrs;
using BoltOn.Data;
using BoltOn.Requestor.Pipeline;
using BoltOn.Samples.Application.Entities;

namespace BoltOn.Samples.Application.Handlers
{
	public class StudentCreatedEvent : BaseDomainEvent<Student>, IDomainEvent
	{
		public Guid StudentId { get; set; }
		public string FirstName { get; private set; }
		public string LastName { get; private set; }
		public string Email { get; set; }
		public string StudentType { get; set; }
		public int StudentTypeId { get; set; }

		public StudentCreatedEvent(Guid studentId, string firstName,
			string lastName, string email, int studentTypeId, string studentType) 
		{
			StudentId = studentId;
			FirstName = firstName;
			LastName = lastName;
			Email = email;
			StudentType = studentType;
			StudentTypeId = studentTypeId;
		}
	}

	public class StudentCreatedEventHandler : IHandler<StudentCreatedEvent>
    {
		private readonly IRepository<StudentFlattened> _studentRepository;

		public StudentCreatedEventHandler(IRepository<StudentFlattened> studentRepository)
		{
			_studentRepository = studentRepository;
		}

        public async Task HandleAsync(StudentCreatedEvent request, CancellationToken cancellationToken)
        {
			var studentFlattened = new StudentFlattened(request);
			await _studentRepository.AddAsync(studentFlattened, cancellationToken);
        }
    }
}
