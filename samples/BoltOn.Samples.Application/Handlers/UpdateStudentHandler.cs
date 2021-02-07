using System;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Cache;
using BoltOn.Data;
using BoltOn.Logging;
using BoltOn.Requestor.Pipeline;
using BoltOn.Samples.Application.Entities;

namespace BoltOn.Samples.Application.Handlers
{
	public class UpdateStudentRequest : IRequest, IClearCachedResponse
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

		public UpdateStudentHandler(IRepository<Student> studentRepository,
			IAppLogger<UpdateStudentHandler> logger)
		{
			_studentRepository = studentRepository;
			_logger = logger;
		}

		public async Task HandleAsync(UpdateStudentRequest request, CancellationToken cancellationToken)
		{
			_logger.Debug("Updating student...");
			var student = await _studentRepository.GetByIdAsync(request.StudentId, cancellationToken: cancellationToken);
			student.Update(request);
			await _studentRepository.UpdateAsync(student, cancellationToken: cancellationToken);
		}
	}
}
