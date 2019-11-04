using System.Linq;
using BoltOn.Mediator.Pipeline;
using BoltOn.Tests.Other;

namespace BoltOn.Tests.Mediator
{
	public class GetStudentRequest : IQuery<Student>
	{
		public int StudentId { get; set; }
	}

	public class GetStudentHandler : IRequestHandler<GetStudentRequest, Student>
    {
        readonly IStudentRepository _studentRepository;

        public GetStudentHandler(IStudentRepository studentRepository)
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
