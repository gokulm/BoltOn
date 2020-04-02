using System.Threading;
using System.Threading.Tasks;
using BoltOn.Data;
using BoltOn.Logging;
using BoltOn.Requestor.Pipeline;
using BoltOn.Samples.Application.Entities;

namespace BoltOn.Samples.Application.Handlers
{
	public class CreateStudentRequest : IRequest<Student>
	{
		public string FirstName { get; set; }

		public string LastName { get; set; }

		public int StudentTypeId { get; set; }
	}

	public class CreateStudentHandler : IHandler<CreateStudentRequest, Student>
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

		public async Task<Student> HandleAsync(CreateStudentRequest request, CancellationToken cancellationToken)
		{
			_logger.Debug("Creating student...");
			var studentType = await _studentTypeRepository.GetByIdAsync(request.StudentTypeId, cancellationToken: cancellationToken);
			var student = await _studentRepository.AddAsync(new Student(request, studentType.Description), cancellationToken: cancellationToken);
			return student;
		}
	}
}
