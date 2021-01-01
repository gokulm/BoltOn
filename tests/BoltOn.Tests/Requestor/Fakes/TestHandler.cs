using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using BoltOn.Requestor.Interceptors;
using BoltOn.Requestor.Pipeline;
using BoltOn.UoW;

namespace BoltOn.Tests.Requestor.Fakes
{
	public class TestRequest : IRequest<bool>, IEnableInterceptor<StopwatchInterceptor>
    {
	}

	public class TestCommand : IRequest<bool>, IEnableUnitOfWork
	{
		public IsolationLevel IsolationLevel => IsolationLevel.ReadCommitted;
	}

	public class TestOneWayRequest : IRequest, IEnableInterceptor<StopwatchInterceptor>
	{
		public int Value { get; set; }
	}

	public class TestOneWayCommand : IRequest, IEnableUnitOfWork
	{
		public int Value { get; set; }

		public IsolationLevel IsolationLevel => IsolationLevel.ReadCommitted;
	}

	public class TestQuery : IRequest<bool>, IEnableUnitOfWork
	{
		public IsolationLevel IsolationLevel => IsolationLevel.ReadCommitted;
	}

	public class TestStaleQuery : IRequest<bool>, IEnableUnitOfWork
	{
		public IsolationLevel IsolationLevel => IsolationLevel.ReadUncommitted;
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
