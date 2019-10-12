using System;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Cqrs;
using BoltOn.Data;
using BoltOn.Mediator.Pipeline;
using BoltOn.Samples.Application.Entities;

namespace BoltOn.Samples.Application.Handlers
{
	public class CreateStudentRequest : IRequest
	{
		public string FirstName { get; set; }

		public string LastName { get; set; }
	}

	public class CreateStudentHandler : IRequestAsyncHandler<CreateStudentRequest>
	{
		private readonly IRepository<Student> _studentRepository;

		public CreateStudentHandler(IRepository<Student> studentRepository)
		{
			_studentRepository = studentRepository;
		}

		public async Task HandleAsync(CreateStudentRequest request, CancellationToken cancellationToken)
		{
			await _studentRepository.AddAsync(
				new Student
				(
					Guid.NewGuid(),
					request.FirstName,
					request.LastName
				), cancellationToken);
			System.Console.WriteLine("message from CreateStudentHandler: " + request.FirstName);
		}
	}

	public class StudentCreatedEvent : EventToBeProcessed
	{
		public Student Student { get; set; }
	}

	public class StudentCreatedEventHandler : IRequestAsyncHandler<StudentCreatedEvent>
	{
		private readonly IRepository<StudentQueryEntity> _repository;

		public StudentCreatedEventHandler(IRepository<StudentQueryEntity> repository)
		{
			_repository = repository;
		}

		public async Task HandleAsync(StudentCreatedEvent request, CancellationToken cancellationToken)
		{
			await _repository.AddAsync(new StudentQueryEntity(request), cancellationToken);
		}
	}
}
