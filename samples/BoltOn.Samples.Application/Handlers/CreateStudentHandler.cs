using System;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Data;
using BoltOn.Logging;
using BoltOn.Mediator.Pipeline;
using BoltOn.Samples.Application.Entities;

namespace BoltOn.Samples.Application.Handlers
{
	public class CreateStudentRequest : IRequest<Guid>
	{
		public string FirstName { get; set; }

		public string LastName { get; set; }

		public int StudentTypeId { get; set; }
	}

	public class CreateStudentHandler : IRequestAsyncHandler<CreateStudentRequest, Guid>
	{
		private readonly IRepository<Student> _studentRepository;
		private readonly IBoltOnLogger<CreateStudentHandler> _logger;
		private readonly IRepository<StudentType> _studentTypeRepository;

		public CreateStudentHandler(IRepository<Student> studentRepository,
			IBoltOnLogger<CreateStudentHandler> logger,
			IRepository<StudentType> studentTypeRepository)
		{
			_studentRepository = studentRepository;
			_logger = logger;
			_studentTypeRepository = studentTypeRepository;
		}

		public async Task<Guid> HandleAsync(CreateStudentRequest request, CancellationToken cancellationToken)
		{
			_logger.Debug("Creating student...");
			var studentType = await _studentTypeRepository.GetByIdAsync(request.StudentTypeId);
			var student = await _studentRepository.AddAsync(new Student(request, studentType.Description), cancellationToken);
			return student.Id;
		}
	}
}
