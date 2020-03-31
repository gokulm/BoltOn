using BoltOn.Requestor.Interceptors;

namespace BoltOn.Requestor.Pipeline
{
    public interface IQuery<out TResponse> : IRequest<TResponse>, IEnableInterceptor<UnitOfWorkInterceptor>
    {
    }
}
