using System.Threading;
using System.Threading.Tasks;
using BoltOn.Mediator.Interceptors;
using BoltOn.Mediator.Pipeline;
using BoltOn.Overrides.Mediator;

namespace BoltOn.Tests.Mediator
{
	public class TestRequest : IRequest<bool>, IEnableStopwatchInterceptor
	{
	}

	public class TestCommand : ICommand<bool>
	{
	}

	public class TestOneWayRequest : IRequest, IEnableStopwatchInterceptor
	{
		public int Value { get; set; }
	}

	public class TestOneWayCommand : ICommand, IEnableStopwatchInterceptor
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
		IRequestAsyncHandler<TestQuery, bool>,
		IRequestAsyncHandler<TestRequest, bool>,
		IRequestAsyncHandler<TestCommand, bool>,
		IRequestAsyncHandler<TestOneWayRequest>,
		IRequestAsyncHandler<TestOneWayCommand>,
		IRequestAsyncHandler<TestStaleQuery, bool>
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
