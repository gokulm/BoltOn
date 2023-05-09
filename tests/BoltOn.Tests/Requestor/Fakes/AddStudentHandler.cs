using System.Threading;
using System.Threading.Tasks;
using BoltOn.Data.EF;
using BoltOn.Requestor;
using BoltOn.Tests.Other;
using BoltOn.DataAbstractions.EF;

namespace BoltOn.Tests.Requestor.Fakes
{
	public class AddStudentRequest : IRequest<Student>
	{
		public int Id { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
	}

	public class AddStudentHandler : IHandler<AddStudentRequest, Student>
    {
        private readonly IRepository<Student> _studentRepository;

        public AddStudentHandler(IRepository<Student> studentRepository)
        {
            _studentRepository = studentRepository;
        }

        public async Task<Student> HandleAsync(AddStudentRequest request, CancellationToken cancellationToken)
        {
            var result = await _studentRepository.AddAsync(new Student
            {
                Id = request.Id,
                FirstName = request.FirstName,
                LastName = request.LastName
            }, cancellationToken);
            return result;
        }
    }
}
