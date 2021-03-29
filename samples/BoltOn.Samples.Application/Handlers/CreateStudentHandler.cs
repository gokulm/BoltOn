using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using BoltOn.Cache;
using BoltOn.Data;
using BoltOn.Logging;
using BoltOn.Requestor.Pipeline;
using BoltOn.Samples.Application.DTOs;
using BoltOn.Samples.Application.Entities;

namespace BoltOn.Samples.Application.Handlers
{
	public class CreateStudentRequest : IRequest<StudentDto>, IClearCachedResponse
	{
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public int StudentTypeId { get; set; }
		public string Email { get; set; }
		public string CacheKey => "Students";
	}

	public class CreateStudentHandler : IHandler<CreateStudentRequest, StudentDto>
	{
		private readonly IRepository<Student> _studentRepository;
		private readonly IAppLogger<CreateStudentHandler> _logger;
		private readonly IMapper _mapper;

		public CreateStudentHandler(IRepository<Student> studentRepository,
			IAppLogger<CreateStudentHandler> logger,
			IMapper mapper)
		{
			_studentRepository = studentRepository;
			_logger = logger;
			_mapper = mapper;
		}

		public async Task<StudentDto> HandleAsync(CreateStudentRequest request, CancellationToken cancellationToken)
		{
			_logger.Debug("Creating student...");
			var student = await _studentRepository.AddAsync(new Student(request), cancellationToken: cancellationToken);
			var studentDto = _mapper.Map<StudentDto>(student);
			return studentDto;
		}
	}
}
