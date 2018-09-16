using System;
using BoltOn.Bootstrapping;
using Xunit;
using BoltOn.Mediator;
using BoltOn.IoC;
using BoltOn.IoC.SimpleInjector;
using BoltOn.Logging.NLog;

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
				// as mediator is register as scoped, and we cannot resolve scoped dependencies in simple
				// injector directly 
				.ExcludeAssemblies(typeof(SimpleInjectorContainerAdapter).Assembly, typeof(NLogLoggerAdapter<>).Assembly)
				.Run();

			// act
			var mediator = ServiceLocator.Current.GetInstance<IMediator>();
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
