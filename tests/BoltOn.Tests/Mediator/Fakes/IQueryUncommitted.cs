using BoltOn.Requestor.Interceptors;
using BoltOn.Requestor.Pipeline;

namespace BoltOn.Tests.Mediator.Fakes
{
    public interface IQueryUncommitted<out TResponse> : IRequest<TResponse>, 
        IEnableInterceptor<UnitOfWorkInterceptor>
    {
    }
}
