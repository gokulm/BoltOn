using BoltOn.Mediator.Interceptors;

namespace BoltOn.Mediator.Pipeline
{
    public interface IQuery<out TResponse> : IRequest<TResponse>, IEnableInterceptor<UnitOfWorkInterceptor>
    {
    }
}
