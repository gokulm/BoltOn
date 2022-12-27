using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using BoltOn.Cache;
using BoltOn.Data;
using BoltOn.Data.EF;
using BoltOn.Logging;
using BoltOn.Requestor;
using BoltOn.Samples.Application.DTOs;
using BoltOn.Samples.Application.Entities;

namespace BoltOn.Samples.Application.Handlers
{
	public class CreateStudentRequest : IRequest<StudentDto>
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
		private readonly IQueryRepository<StudentType> _studentTypeRepository;

		public CreateStudentHandler(IRepository<Student> studentRepository,
			IAppLogger<CreateStudentHandler> logger,
			IMapper mapper,
			IQueryRepository<StudentType> studentTypeRepository)
		{
			_studentRepository = studentRepository;
			_logger = logger;
			_mapper = mapper;
			_studentTypeRepository = studentTypeRepository;
		}

		public async Task<StudentDto> HandleAsync(CreateStudentRequest request, CancellationToken cancellationToken)
		{
			_logger.Debug("Creating student...");
			var studentType = await _studentTypeRepository.GetByIdAsync(request.StudentTypeId, cancellationToken);
			var student = await _studentRepository.AddAsync(new Student(request, studentType.Description), cancellationToken: cancellationToken);
			var studentDto = _mapper.Map<StudentDto>(student);
			return studentDto;
		}
	}
}
