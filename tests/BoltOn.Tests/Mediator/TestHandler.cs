using System.Threading;
using System.Threading.Tasks;
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
	    IRequestHandler<TestQuery, bool>,
		IRequestAsyncHandler<TestQuery, bool>,
		IRequestAsyncHandler<TestRequest, bool>,
		IRequestAsyncHandler<TestCommand, bool>
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

		public virtual async Task<bool> HandleAsync(TestQuery request, CancellationToken cancellationToken)
		{
			return await Task.FromResult(true);
		}

		public virtual async Task<bool> HandleAsync(TestRequest request, CancellationToken cancellationToken)
		{
			return await Task.FromResult(true);
		}

		public virtual async Task<bool> HandleAsync(TestCommand request, CancellationToken cancellationToken = default(CancellationToken))
		{
			return await Task.FromResult(true);
		}
	}
}
