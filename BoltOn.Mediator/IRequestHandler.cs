namespace BoltOn.Mediator
{
    public interface IRequestHandler<in TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        TResponse Handle(IRequest<TResponse> request);
    }
}
