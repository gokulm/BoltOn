using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Data;
using BoltOn.Logging;
using BoltOn.Requestor.Pipeline;
using BoltOn.Samples.Application.Entities;

namespace BoltOn.Samples.Application.Handlers
{
    public class EnrollCourseRequest : IRequest
    {
        public Guid StudentId { get; set; }
        public Guid CourseId { get; set; }
    }

    public class EnrollCourseHandler : IHandler<EnrollCourseRequest>
    {
        private readonly IRepository<Student> _studentRepository;
        private readonly IAppLogger<EnrollCourseHandler> _logger;

        public EnrollCourseHandler(IRepository<Student> studentRepository,
            IAppLogger<EnrollCourseHandler> logger)
        {
            _studentRepository = studentRepository;
            _logger = logger;
        }

        public async Task HandleAsync(EnrollCourseRequest request, CancellationToken cancellationToken)
        {
            _logger.Debug("Enrolling course...");
            var student = (await _studentRepository.FindByAsync(f => f.StudentId == request.StudentId,
                                                        cancellationToken: cancellationToken,
                                                        i => i.Courses)).Single();
            student.EnrollCourse(request.CourseId);
            await _studentRepository.UpdateAsync(student, cancellationToken: cancellationToken);
        }
    }
}
