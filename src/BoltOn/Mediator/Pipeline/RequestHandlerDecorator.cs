using System.Threading;
using System.Threading.Tasks;

namespace BoltOn.Mediator.Pipeline
{
	internal class RequestAsyncHandlerDecorator<TRequest, TResponse>
		where TRequest : IRequest<TResponse>
	{
		private readonly IRequestAsyncHandler<TRequest, TResponse> _requestAsyncHandler;

		public RequestAsyncHandlerDecorator(IRequestAsyncHandler<TRequest, TResponse> requestAsyncHandler)
		{
			_requestAsyncHandler = requestAsyncHandler;
		}

		public async Task<TResponse> HandleAsync(IRequest<TResponse> request, CancellationToken cancellationToken)
		{
			return await _requestAsyncHandler.HandleAsync((TRequest)request, cancellationToken);
		}
	}

	internal class RequestAsyncHandlerDecorator<TRequest>
		where TRequest : IRequest
	{
		private readonly IRequestAsyncHandler<TRequest> _requestAsyncHandler;

		public RequestAsyncHandlerDecorator(IRequestAsyncHandler<TRequest> requestAsyncHandler)
		{
			_requestAsyncHandler = requestAsyncHandler;
		}

		public async Task<DummyResponse> HandleAsync(IRequest<DummyResponse> request, CancellationToken cancellationToken)
		{
			await _requestAsyncHandler.HandleAsync((TRequest)request, cancellationToken);
			return await Task.FromResult(new DummyResponse());
		}
	}
}
