using BoltOn.Requestor.Pipeline;
using BoltOn.Samples.Application.Handlers;
using BoltOn.Bootstrapping;

namespace BoltOn.Samples.Console
{
	public class PostRegistrationTask : IPostRegistrationTask
    {
        private readonly IRequestor _requestor;

        public PostRegistrationTask(IRequestor requestor)
        {
            _requestor = requestor;
        }

        public void Run()
        {
			var response = _requestor.ProcessAsync(new PingRequest()).GetAwaiter().GetResult();
			System.Console.WriteLine(response.Data);
		}
    }
}
