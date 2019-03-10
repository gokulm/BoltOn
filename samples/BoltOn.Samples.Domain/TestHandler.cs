using BoltOn.Mediator.Pipeline;
using BoltOn.Samples.DTOs;

namespace BoltOn.Samples.Domain
{
	public class TestHandler : IRequestHandler<TestRequest, TestResponse>
	{
		public TestResponse Handle(TestRequest request)
		{
			return new TestResponse { Test = "abc" };
		}
	}
}
