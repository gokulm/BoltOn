using System;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Data;
using BoltOn.Logging;
using BoltOn.Mediator.Pipeline;
using BoltOn.Samples.Application.Entities;

namespace BoltOn.Samples.Application.Handlers
{
	public class UpdateStudentRequest : IRequest
	{
		public Guid StudentId { get; set; }

		public string FirstName { get; set; }

		public string LastName { get; set; }

		public int StudentTypeId { get; set; }
	}

	public class UpdateStudentHandler : IHandler<UpdateStudentRequest>
	{
		private readonly IRepository<Student> _studentRepository;
		private readonly IBoltOnLogger<UpdateStudentHandler> _logger;
		private readonly IRepository<StudentType> _studentTypeRepository;

		public UpdateStudentHandler(IRepository<Student> studentRepository,
			IBoltOnLogger<UpdateStudentHandler> logger,
			IRepository<StudentType> studentTypeRepository)
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
