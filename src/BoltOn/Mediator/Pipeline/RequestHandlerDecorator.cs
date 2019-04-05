using System.Threading;
using System.Threading.Tasks;

namespace BoltOn.Mediator.Pipeline
{
	internal class RequestHandlerDecorator<TRequest, TResponse>
		where TRequest : IRequest<TResponse>
	{
		private readonly IRequestHandler<TRequest, TResponse> _requestHandler;

		public RequestHandlerDecorator(IRequestHandler<TRequest, TResponse> requestHandler)
		{
			_requestHandler = requestHandler;
		}

		public TResponse Handle(IRequest<TResponse> request)
		{
			return _requestHandler.Handle((TRequest)request);
		}
	}

	internal class RequestHandlerDecorator<TRequest>
		where TRequest : IRequest
	{
		private readonly IRequestHandler<TRequest> _requestHandler;

		public RequestHandlerDecorator(IRequestHandler<TRequest> requestHandler)
		{
			_requestHandler = requestHandler;
		}

		public bool Handle(IRequest<DummyResponse> request)
		{
			_requestHandler.Handle((TRequest)request);
			return true;
		}
	}

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
