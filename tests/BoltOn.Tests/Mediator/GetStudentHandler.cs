using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Mediator.Pipeline;
using BoltOn.Tests.Other;

namespace BoltOn.Tests.Mediator
{
	public class GetStudentRequest : IQuery<Student>
	{
		public int StudentId { get; set; }
	}

	public class GetStudentHandler : IHandler<GetStudentRequest, Student>
    {
        readonly IStudentRepository _studentRepository;

        public GetStudentHandler(IStudentRepository studentRepository)
        {
            _studentRepository = studentRepository;
        }

		public async Task<Student> HandleAsync(GetStudentRequest request, CancellationToken cancellationToken)
		{
			var student = (await _studentRepository.FindByAsync(f => f.Id == request.StudentId)).FirstOrDefault();
			return student;
		}
	}
}
