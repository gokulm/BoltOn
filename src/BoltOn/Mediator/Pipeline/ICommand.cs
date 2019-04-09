using BoltOn.Mediator.Interceptors;

namespace BoltOn.Mediator.Pipeline
{
    public interface ICommand : IRequest, IEnableUnitOfWorkInterceptor
    {
	}

	public interface ICommand<out TResponse> : IRequest<TResponse>, IEnableUnitOfWorkInterceptor
	{
	}
}
