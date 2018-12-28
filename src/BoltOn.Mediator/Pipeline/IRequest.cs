using BoltOn.Mediator.Middlewares;

namespace BoltOn.Mediator.Pipeline
{
	public interface IRequest<out TResponse>
    {
    }

	public interface ICommand<out TResponse> : IRequest<TResponse>, IEnableUnitOfWorkMiddleware
	{	
	}

	public interface IQuery<out TResponse> : IRequest<TResponse>, IEnableUnitOfWorkMiddleware
	{
	}

	public enum RequestType
	{
		Query,
		Command
	}
}
