using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using BoltOn.Data.EF;
using BoltOn.Logging;
using BoltOn.Requestor;
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
		private readonly IQueryRepository<Course> _courseRepository;

		public GetStudentHandler(IRepository<Student> studentRepository,
            IAppLogger<GetStudentHandler> logger,
            IMapper mapper,
            IQueryRepository<Course> courseRepository)
        {
            _studentRepository = studentRepository;
            _logger = logger;
			_mapper = mapper;
			_courseRepository = courseRepository;
		}

        public async Task<StudentDto> HandleAsync(GetStudentRequest request, CancellationToken cancellationToken)
        {
            _logger.Debug("Getting student...");
            var student = (await _studentRepository.FindByAsync(f => f.StudentId == request.StudentId,
                cancellationToken: cancellationToken, i => i.Courses)).FirstOrDefault();
            var courses = await _courseRepository.GetAllAsync(cancellationToken);
            var studentCourseIds = student.Courses.Select(s => s.CourseId).ToList();
            var studentCourses = courses.Where(w => studentCourseIds.Contains(w.CourseId));
            var courseDtos = _mapper.Map<IList<CourseDto>>(studentCourses);

            var studentDto = _mapper.Map<StudentDto>(student);
            studentDto.Courses = courseDtos;

            return studentDto;
        }
    }
}
