using System;
using BoltOn.Bootstrapping;
using Xunit;
using BoltOn.Mediator;

namespace BoltOn.Tests.Mediator
{
    public class MediatorTests
    {
		[Fact]
		public void GetInstance_RegisterTransient_ResolvesDependencies()
		{

			// arrange
			Bootstrapper
				.Instance
				.Run();

			// act

			// assert
		}
    }

	public class TestRequest : IRequest<bool>
	{
	}

	public class TestHandler : IRequestHandler<TestRequest, bool>
	{
		public bool Handle(IRequest<bool> request)
		{
			return true;
		}
	}
}
