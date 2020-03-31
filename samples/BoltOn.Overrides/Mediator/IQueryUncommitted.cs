using BoltOn.Requestor.Interceptors;
using BoltOn.Requestor.Pipeline;

namespace BoltOn.Overrides.Mediator
{
    public interface IQueryUncommitted<out TResponse> : IRequest<TResponse>, IEnableInterceptor<UnitOfWorkInterceptor>
    {
    }
}
