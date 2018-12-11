namespace BoltOn.Mediator.Pipeline
{
	public interface IRequestHandler<in TRequest, TResponse> 
		where TRequest : IRequest<TResponse> 
    {
		TResponse Handle(TRequest request);
	}
}
