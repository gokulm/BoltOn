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

		public bool Handle(IRequest<bool> request)
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

		public async Task<TResponse> HandleAsync(IRequest<TResponse> request, CancellationToken cancellationToken = default(CancellationToken))
		{
			return await _requestAsyncHandler.HandleAsync((TRequest)request, cancellationToken);
		}
	}

}
