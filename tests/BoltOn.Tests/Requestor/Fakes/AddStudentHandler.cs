using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using BoltOn.Data;
using BoltOn.Requestor.Pipeline;
using BoltOn.Tests.Other;
using BoltOn.UoW;

namespace BoltOn.Tests.Requestor.Fakes
{
	public class AddStudentRequest : IRequest<Student>, IEnableUnitOfWork
	{
		public int Id { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }

        public IsolationLevel IsolationLevel => IsolationLevel.ReadCommitted;
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
