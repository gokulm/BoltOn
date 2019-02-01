using BoltOn.Mediator.Middlewares;
using BoltOn.Mediator.Pipeline;

namespace BoltOn.Tests.Mediator
{
	public class TestRequest : IRequest<bool>, IEnableStopwatchMiddleware
	{
	}

	public class TestCommand : ICommand<bool>
	{
	}

	public class TestQuery : IQuery<bool>
	{
	}

	public class TestHandler : IRequestHandler<TestRequest, bool>,
	    IRequestHandler<TestCommand, bool>,
	    IRequestHandler<TestQuery, bool>
    {
        public virtual bool Handle(TestRequest request)
        {
            return true;
        }

        public virtual bool Handle(TestCommand request)
        {
            return true;
        }

        public virtual bool Handle(TestQuery request)
        {
            return true;
        }
    }
}
