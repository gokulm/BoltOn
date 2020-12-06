using System.Threading;
using System.Threading.Tasks;
using BoltOn.Requestor.Pipeline;

namespace BoltOn.Samples.Application.Handlers
{
	public class PingRequest : IRequest<string>
	{
	}

	public class PingHandler : IHandler<PingRequest, string>
	{
		public async Task<string> HandleAsync(PingRequest request, CancellationToken cancellationToken)
		{
			return await Task.FromResult("pong");
		}
	}
}
