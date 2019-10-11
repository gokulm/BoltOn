using System.Threading;
using System.Threading.Tasks;
using BoltOn.Mediator.Pipeline;

namespace BoltOn.Samples.Application.Handlers
{
	public class CreateStudentRequest : IRequest
	{
		public string FirstName { get; set; }

		public string LastName { get; set; }
	}

	public class CreateStudentHandler : IRequestAsyncHandler<CreateStudentRequest>
    {
        public async Task HandleAsync(CreateStudentRequest request, CancellationToken cancellationToken)
        {
            System.Console.WriteLine("message from CreateStudentHandler: " + request.FirstName);
            await Task.FromResult("testing");
        }
    }
}
