namespace BoltOn.Mediator
{
    public delegate TResponse HandleDelegate<in TRequest, TResponse>(TRequest request);

    public interface IMiddleware<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        TResponse RunAsync(TRequest request, HandleDelegate<TRequest, TResponse> next);
    }
}