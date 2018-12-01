using BoltOn.Mediator.Middlewares;

namespace BoltOn.Mediator.Pipeline
{
	public interface IRequest<out TResponse> where TResponse : class
    {
    }

	public interface ICommand<out TResponse> : IRequest<TResponse>, IEnableUnitOfWorkMiddleware where TResponse : class
	{	
	}

	public interface IQuery<out TResponse> : IRequest<TResponse>, IEnableUnitOfWorkMiddleware where TResponse : class
	{
	}
}
