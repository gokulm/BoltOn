using BoltOn.Mediator.Pipeline;

namespace BoltOn.Samples.Application.Handlers
{
	public class PingRequest : IQuery<PongResponse>
	{
	}

	public class PongResponse
	{
		public string Data { get; set; }
	}

	public class PingHandler : IRequestHandler<PingRequest, PongResponse>
	{
		public PongResponse Handle(PingRequest request)
		{
			return new PongResponse { Data = "ping pong" };
		}
	}
}
