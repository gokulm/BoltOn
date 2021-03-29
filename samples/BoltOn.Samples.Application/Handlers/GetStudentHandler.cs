using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using BoltOn.Data;
using BoltOn.Logging;
using BoltOn.Requestor.Pipeline;
using BoltOn.Samples.Application.DTOs;
using BoltOn.Samples.Application.Entities;

namespace BoltOn.Samples.Application.Handlers
{
    public class GetStudentRequest : IRequest<StudentDto>
    {
        public Guid StudentId { get; set; }
    }

    public class GetStudentHandler : IHandler<GetStudentRequest, StudentDto>
    {
        private readonly IRepository<Student> _studentRepository;
        private readonly IAppLogger<GetStudentHandler> _logger;
		private readonly IMapper _mapper;

		public GetStudentHandler(IRepository<Student> studentRepository,
            IAppLogger<GetStudentHandler> logger,
            IMapper mapper)
        {
            _studentRepository = studentRepository;
            _logger = logger;
			_mapper = mapper;
		}

        public async Task<StudentDto> HandleAsync(GetStudentRequest request, CancellationToken cancellationToken)
        {
            _logger.Debug("Getting student...");
            var student = (await _studentRepository.FindByAsync(f => f.StudentId == request.StudentId,
                cancellationToken: cancellationToken)).FirstOrDefault();
            var studentDto = _mapper.Map<StudentDto>(student);
            return studentDto;
        }
    }
}
