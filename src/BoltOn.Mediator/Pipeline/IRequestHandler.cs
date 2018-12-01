namespace BoltOn.Mediator.Pipeline
{
	public interface IRequestHandler<in TRequest, TResponse> 
		where TRequest : IRequest<TResponse> 
		where TResponse : class
    {
		TResponse Handle(TRequest request);
	}
}
