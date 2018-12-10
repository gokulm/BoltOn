using System;
using BoltOn.Mediator.Pipeline;

namespace BoltOn.Samples.Domain
{
    public class TestRequest : IQuery<TestResponse>
    {
    }

	public class TestResponse
	{
		public string Test { get; set; }
	}

	public class TestHandler : IRequestHandler<TestRequest, TestResponse>
	{
		public TestResponse Handle(TestRequest request)
		{
			return new TestResponse { Test = "abc" };
		}
	}
}
