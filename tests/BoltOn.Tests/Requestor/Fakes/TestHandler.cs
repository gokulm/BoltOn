using System.Threading;
using System.Threading.Tasks;
using BoltOn.Requestor.Interceptors;
using BoltOn.Requestor.Pipeline;

namespace BoltOn.Tests.Requestor.Fakes
{
	public class TestRequest : IRequest<bool>, IEnableInterceptor<StopwatchInterceptor>
    {
	}

	public class TestCommand : ICommand<bool>
	{
	}

	public class TestOneWayRequest : IRequest, IEnableInterceptor<StopwatchInterceptor>
	{
		public int Value { get; set; }
	}

	public class TestOneWayCommand : ICommand, IEnableInterceptor<StopwatchInterceptor>
	{
		public int Value { get; set; }
	}

	public class TestQuery : IQuery<bool>
	{
	}

	public class TestStaleQuery : IQueryUncommitted<bool>
	{
	}

	public class TestHandler : 
		IHandler<TestQuery, bool>,
		IHandler<TestRequest, bool>,
		IHandler<TestCommand, bool>,
		IHandler<TestOneWayRequest>,
		IHandler<TestOneWayCommand>,
		IHandler<TestStaleQuery, bool>
	{
		public virtual async Task<bool> HandleAsync(TestQuery request, CancellationToken cancellationToken)
		{
			return await Task.FromResult(true);
		}

		public virtual async Task<bool> HandleAsync(TestRequest request, CancellationToken cancellationToken)
		{
			return await Task.FromResult(true);
		}

		public virtual async Task<bool> HandleAsync(TestCommand request, CancellationToken cancellationToken)
		{
			return await Task.FromResult(true);
		}

		public Task HandleAsync(TestOneWayRequest request, CancellationToken cancellationToken)
		{
			request.Value = 1;
			return Task.CompletedTask;
		}

		public Task HandleAsync(TestOneWayCommand request, CancellationToken cancellationToken)
		{
			request.Value = 1;
			return Task.CompletedTask;
		}

		public virtual async Task<bool> HandleAsync(TestStaleQuery request, CancellationToken cancellationToken)
		{
			return await Task.FromResult(true);
		}
	}
}
