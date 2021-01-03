using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using BoltOn.Requestor.Pipeline;
using BoltOn.Tests.Other;
using BoltOn.Transaction;

namespace BoltOn.Tests.Requestor.Fakes
{
	public class GetStudentRequest : IRequest<Student>, IEnableTransaction
	{
		public int StudentId { get; set; }

		public IsolationLevel IsolationLevel => IsolationLevel.ReadCommitted;
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
			var student = (await _studentRepository
                .FindByAsync(f => f.Id == request.StudentId, cancellationToken: cancellationToken)).FirstOrDefault();
			return student;
		}
	}
}
