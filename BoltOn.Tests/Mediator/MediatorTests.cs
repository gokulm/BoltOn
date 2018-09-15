using System;
using BoltOn.Bootstrapping;
using Xunit;
using BoltOn.Mediator;
using BoltOn.IoC;
using BoltOn.IoC.SimpleInjector;

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
				.ExcludeAssemblies(typeof(SimpleInjectorContainerAdapter).Assembly)
				.Run();

			// act
			var mediator = ServiceLocator.Current.GetInstance<BoltOn.Mediator.IMediator>();
			mediator.Get(new TestRequest());

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
