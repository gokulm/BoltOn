using System.Threading;
using System.Threading.Tasks;
using BoltOn.Mediator.Interceptors;
using BoltOn.Mediator.Pipeline;

namespace BoltOn.Tests.Other
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

	public class TestStaleQuery : IStaleQuery<bool>
	{
	}

	public class TestHandler : IRequestHandler<TestRequest, bool>,
		IRequestHandler<TestCommand, bool>,
		IRequestHandler<TestQuery, bool>,
		IRequestAsyncHandler<TestQuery, bool>,
		IRequestHandler<TestStaleQuery, bool>,
		IRequestAsyncHandler<TestRequest, bool>,
		IRequestAsyncHandler<TestCommand, bool>,
		IRequestHandler<TestOneWayRequest>,
		IRequestAsyncHandler<TestOneWayRequest>,
		IRequestHandler<TestOneWayCommand>,
		IRequestAsyncHandler<TestOneWayCommand>
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

		public virtual bool Handle(TestStaleQuery request)
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

		public virtual async Task<bool> HandleAsync(TestCommand request, CancellationToken cancellationToken)
		{
			return await Task.FromResult(true);
		}

		public void Handle(TestOneWayRequest request)
		{
			request.Value = 1;
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

		public void Handle(TestOneWayCommand request)
		{
			request.Value = 1;
		}
	}
}
