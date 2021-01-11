using System.Threading;
using System.Threading.Tasks;
using BoltOn.Cache;
using BoltOn.Data;
using BoltOn.Logging;
using BoltOn.Requestor.Pipeline;
using BoltOn.Samples.Application.Entities;

namespace BoltOn.Samples.Application.Handlers
{
	public class CreateStudentRequest : IRequest<Student>, IClearCachedResponse
	{
		public string FirstName { get; set; }

		public string LastName { get; set; }

		public int StudentTypeId { get; set; }

		public string CacheKey => "Students";
	}

	public class CreateStudentHandler : IHandler<CreateStudentRequest, Student>
	{
		private readonly IRepository<Student> _studentRepository;
		private readonly IAppLogger<CreateStudentHandler> _logger;

		public CreateStudentHandler(IRepository<Student> studentRepository,
			IAppLogger<CreateStudentHandler> logger)
		{
			_studentRepository = studentRepository;
			_logger = logger;
		}

		public async Task<Student> HandleAsync(CreateStudentRequest request, CancellationToken cancellationToken)
		{
			_logger.Debug("Creating student...");
			var student = await _studentRepository.AddAsync(new Student(request), cancellationToken: cancellationToken);
			return student;
		}
	}
}
