using System;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Data.EF;
using BoltOn.Logging;
using BoltOn.Requestor;
using BoltOn.Samples.Application.Entities;

namespace BoltOn.Samples.Application.Handlers
{
	public class UpdateStudentRequest : IRequest
	{
		public Guid StudentId { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Email { get; set; }
		public int StudentTypeId { get; set; }
		public string CacheKey => "Students";
	}

	public class UpdateStudentHandler : IHandler<UpdateStudentRequest>
	{
		private readonly IRepository<Student> _studentRepository;
		private readonly IAppLogger<UpdateStudentHandler> _logger;
		private readonly IQueryRepository<StudentType> _studentTypeRepository;

		public UpdateStudentHandler(IRepository<Student> studentRepository,
			IAppLogger<UpdateStudentHandler> logger,
			IQueryRepository<StudentType> studentTypeRepository)
		{
			_studentRepository = studentRepository;
			_logger = logger;
			_studentTypeRepository = studentTypeRepository;
		}

		public async Task HandleAsync(UpdateStudentRequest request, CancellationToken cancellationToken)
		{
			_logger.Debug("Updating student...");
			var studentType = await _studentTypeRepository.GetByIdAsync(request.StudentTypeId, cancellationToken: cancellationToken);
			var student = await _studentRepository.GetByIdAsync(request.StudentId, cancellationToken: cancellationToken);
			student.Update(request, studentType.Description);
			await _studentRepository.UpdateAsync(student, cancellationToken: cancellationToken);
		}
	}
}
