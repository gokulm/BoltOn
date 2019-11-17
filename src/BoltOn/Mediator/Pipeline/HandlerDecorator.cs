using System.Threading;
using System.Threading.Tasks;

namespace BoltOn.Mediator.Pipeline
{
	internal class HandlerDecorator<TRequest, TResponse>
		where TRequest : IRequest<TResponse>
	{
		private readonly IHandler<TRequest, TResponse> _handler;

		public HandlerDecorator(IHandler<TRequest, TResponse> handler)
		{
			_handler = handler;
		}

		public async Task<TResponse> HandleAsync(IRequest<TResponse> request, CancellationToken cancellationToken)
		{
			return await _handler.HandleAsync((TRequest)request, cancellationToken);
		}
	}

	internal class HandlerDecorator<TRequest>
		where TRequest : IRequest
	{
		private readonly IHandler<TRequest> _handler;

		public HandlerDecorator(IHandler<TRequest> handler)
		{
			_handler = handler;
		}

		public async Task<DummyResponse> HandleAsync(IRequest<DummyResponse> request, CancellationToken cancellationToken)
		{
			await _handler.HandleAsync((TRequest)request, cancellationToken);
			return await Task.FromResult(new DummyResponse());
		}
	}
}
