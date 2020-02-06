using BoltOn.Mediator.Interceptors;
using BoltOn.Mediator.Pipeline;

namespace BoltOn.Tests.Mediator.Fakes
{
    public interface IQueryUncommitted<out TResponse> : IRequest<TResponse>, IEnableUnitOfWorkInterceptor
    {
    }
}
