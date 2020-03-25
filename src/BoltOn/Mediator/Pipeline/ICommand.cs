using BoltOn.Mediator.Interceptors;

namespace BoltOn.Mediator.Pipeline
{
    public interface ICommand : IRequest, IEnableInterceptor<UnitOfWorkInterceptor>
    {
	}

	public interface ICommand<out TResponse> : IRequest<TResponse>, 
        IEnableInterceptor<UnitOfWorkInterceptor>
	{
	}
}
