using System.Threading;
using System.Threading.Tasks;

namespace BoltOn.Mediator.Pipeline
{
	public interface IRequestHandler<in TRequest, out TResponse> 
		where TRequest : IRequest<TResponse> 
    {
		TResponse Handle(TRequest request);
	}

	public interface IRequestHandler<in TRequest> 
		where TRequest : IRequest
	{
		void Handle(TRequest request);
	}

	public interface IRequestAsyncHandler<in TRequest, TResponse>
		where TRequest : IRequest<TResponse>
	{
		Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken);
	}

	public interface IRequestAsyncHandler<in TRequest>
		where TRequest : IRequest
	{
		Task HandleAsync(TRequest request, CancellationToken cancellationToken);
	}
}
