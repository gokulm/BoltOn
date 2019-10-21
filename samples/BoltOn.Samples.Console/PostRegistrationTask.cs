using BoltOn.Mediator.Pipeline;
using BoltOn.Samples.Application.Handlers;
using BoltOn.Bootstrapping;

namespace BoltOn.Samples.Console
{
    public class PostRegistrationTask : IPostRegistrationTask
    {
        private readonly IMediator _mediator;

        public PostRegistrationTask(IMediator mediator)
        {
            _mediator = mediator;
        }

        public void Run(PostRegistrationTaskContext context)
        {
            var response = _mediator.Process(new PingRequest());
            System.Console.WriteLine(response.Data);
        }
    }
}
