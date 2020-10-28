using System.Threading;
using System.Threading.Tasks;
using BoltOn.Requestor.Pipeline;

namespace BoltOn.Samples.Application.Handlers
{
	public class PingRequest : IRequest<PongResponse>
	{
	}

	public class PongResponse
	{
		public string Data { get; set; }
	}

	public class PingHandler : IHandler<PingRequest, PongResponse>
	{
		public async Task<PongResponse> HandleAsync(PingRequest request, CancellationToken cancellationToken)
		{
			return await Task.FromResult(new PongResponse { Data = "pong" });
		}
	}

	public class TestRequest : IRequest
	{
		public override string ToString()
		{
			return "Test Request Job";
		}
	}

	public class TestHandler : IHandler<TestRequest>
	{
		public async Task HandleAsync(TestRequest request, CancellationToken cancellationToken)
		{
			System.Console.WriteLine($"test: {System.DateTime.Now}");
			 await Task.FromResult("test");
		}
	}

}
