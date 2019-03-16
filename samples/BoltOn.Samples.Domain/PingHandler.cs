using BoltOn.Mediator.Pipeline;
using BoltOn.Samples.DTOs;

namespace BoltOn.Samples.Domain
{
	public class PingHandler : IRequestHandler<PingRequest, PongResponse>
	{
		public PongResponse Handle(PingRequest request)
		{
			return new PongResponse { Data = "ping pong" };
		}
	}
}
