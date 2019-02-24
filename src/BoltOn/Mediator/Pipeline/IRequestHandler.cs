using System.Threading;
using System.Threading.Tasks;

namespace BoltOn.Mediator.Pipeline
{
	public interface IRequestHandler<in TRequest, TResponse> 
		where TRequest : IRequest<TResponse> 
    {
		TResponse Handle(TRequest request);
	}

	public interface IRequestAsyncHandler<in TRequest, TResponse>
		where TRequest : IRequest<TResponse>
	{
		Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken = default(CancellationToken));
	}
}
