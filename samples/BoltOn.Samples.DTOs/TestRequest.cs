using BoltOn.Mediator.Pipeline;

namespace BoltOn.Samples.DTOs
{
	public class TestRequest : IQuery<TestResponse>
	{
	}

	public class TestResponse
	{
		public string Test { get; set; }
	}
}
