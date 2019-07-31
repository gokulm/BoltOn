namespace BoltOn.Mediator.Pipeline
{
	public interface IRequestHandler<in TRequest, out TResponse> 
		where TRequest : IRequest<TResponse> 
    {
		TResponse Handle(TRequest request);
	}

	public interface IRequestHandler<in TRequest> 
		where TRequest : IRequest
	{
		void Handle(TRequest request);
	}
}
