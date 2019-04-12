using BoltOn.Mediator.Interceptors;
using BoltOn.Mediator.Pipeline;

namespace BoltOn.Ovverides.Mediator
{
    public interface IQueryUncommitted<out TResponse> : IRequest<TResponse>, IEnableUnitOfWorkInterceptor
    {
    }
}
