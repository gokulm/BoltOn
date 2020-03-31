using BoltOn.Requestor.Interceptors;

namespace BoltOn.Requestor.Pipeline
{
    public interface ICommand : IRequest, IEnableInterceptor<UnitOfWorkInterceptor>
    {
	}

	public interface ICommand<out TResponse> : IRequest<TResponse>, 
        IEnableInterceptor<UnitOfWorkInterceptor>
	{
	}
}
