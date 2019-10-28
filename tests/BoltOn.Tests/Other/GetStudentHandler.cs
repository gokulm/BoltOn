using System.Linq;
using BoltOn.Data;
using BoltOn.Mediator.Pipeline;

namespace BoltOn.Tests.Other
{
	public class GetStudentRequest : IQuery<Student>
	{
		public int StudentId { get; set; }
	}

	public class GetStudentHandler : IRequestHandler<GetStudentRequest, Student>
    {
        readonly IRepository<Student> _studentRepository;

        public GetStudentHandler(IRepository<Student> studentRepository)
        {
            _studentRepository = studentRepository;
        }

        public virtual Student Handle(GetStudentRequest request)
        {
            var student = _studentRepository.FindBy(f => f.Id == request.StudentId).FirstOrDefault();
            return student;
        }
    }
}
