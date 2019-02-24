using System.Threading;
using System.Threading.Tasks;

namespace BoltOn.Mediator.Pipeline
{
	internal abstract class BaseRequestHandlerDecorator<TResponse>
	{
		public abstract TResponse Handle(IRequest<TResponse> request);
	}

	internal class RequestHandlerDecorator<TRequest, TResponse> : BaseRequestHandlerDecorator<TResponse>
		where TRequest : IRequest<TResponse>
	{
		private readonly IRequestHandler<TRequest, TResponse> _requestHandler;

		public RequestHandlerDecorator(IRequestHandler<TRequest, TResponse> requestHandler)
		{
			_requestHandler = requestHandler;
		}

		public override TResponse Handle(IRequest<TResponse> request)
		{
			return _requestHandler.Handle((TRequest)request);
		}
	}

	internal abstract class BaseRequestAsyncHandlerDecorator<TResponse>
	{
		public abstract Task<TResponse> HandleAsync(IRequest<TResponse> request, CancellationToken cancellationToken = default(CancellationToken));
	}

	internal class RequestAsyncHandlerDecorator<TRequest, TResponse> : BaseRequestAsyncHandlerDecorator<TResponse>
		where TRequest : IRequest<TResponse>
	{
		private readonly IRequestAsyncHandler<TRequest, TResponse> _requestHandler;

		public RequestAsyncHandlerDecorator(IRequestAsyncHandler<TRequest, TResponse> requestHandler)
		{
			_requestHandler = requestHandler;
		}

		public async override Task<TResponse> HandleAsync(IRequest<TResponse> request, CancellationToken cancellationToken = default(CancellationToken))
		{
			return await _requestHandler.HandleAsync((TRequest)request, cancellationToken);
		}
	}

}
