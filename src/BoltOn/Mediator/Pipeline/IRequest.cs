using BoltOn.Mediator.Interceptors;

namespace BoltOn.Mediator.Pipeline
{
	public interface IRequest<out TResponse>
    {
    }

	public interface IRequest : IRequest<bool>
	{
	}

	public interface ICommand<out TResponse> : IRequest<TResponse>, IEnableUnitOfWorkInterceptor
	{	
	}

	public interface IQuery<out TResponse> : IRequest<TResponse>, IEnableUnitOfWorkInterceptor
	{
	}

	public interface IStaleQuery<out TResponse> : IRequest<TResponse>, IEnableUnitOfWorkInterceptor
	{
	}
}
