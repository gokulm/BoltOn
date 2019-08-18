using System.Threading;
using System.Threading.Tasks;
using BoltOn.Mediator.Pipeline;
using BoltOn.Samples.Application.Messages;

namespace BoltOn.Samples.Application.Handlers
{
    public class CreateStudentHandler : IRequestAsyncHandler<CreateStudent>
    {
        public async Task HandleAsync(CreateStudent request, CancellationToken cancellationToken)
        {
            System.Console.WriteLine("message from CreateStudentHandler: " + request.FirstName);
            await Task.FromResult("testing");
        }
    }
}
