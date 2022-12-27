using System.Threading;
using System.Threading.Tasks;

namespace BoltOn.Requestor
{
	public interface IHandler<in TRequest, TResponse>
		where TRequest : IRequest<TResponse>
	{
		Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken);
	}

	public interface IHandler<in TRequest>
		where TRequest : IRequest
	{
		Task HandleAsync(TRequest request, CancellationToken cancellationToken);
	}
}
