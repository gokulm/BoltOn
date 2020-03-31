using BoltOn.Requestor.Interceptors;
using BoltOn.Requestor.Pipeline;

namespace BoltOn.Overrides.Requestor
{
    public interface IQueryUncommitted<out TResponse> : IRequest<TResponse>,
		IEnableInterceptor<UnitOfWorkInterceptor>
    {
    }
}
