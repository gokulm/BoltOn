using BoltOn.Mediator.Interceptors;
using BoltOn.Mediator.Pipeline;

namespace BoltOn.Overrides.Mediator
{
    public interface IQueryUncommitted<out TResponse> : IRequest<TResponse>, IEnableInterceptor<UnitOfWorkInterceptor>
    {
    }
}
