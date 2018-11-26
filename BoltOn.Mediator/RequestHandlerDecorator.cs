namespace BoltOn.Mediator
{
	internal abstract class BaseRequestHandlerDecorator<TResponse>
	{
		public abstract TResponse Handle(IRequest<TResponse> request);
	}

	internal class RequestHandlerDecorator<TRequest, TResponse> : BaseRequestHandlerDecorator<TResponse> where TRequest : IRequest<TResponse>
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
}
