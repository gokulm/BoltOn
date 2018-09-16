using System;
using BoltOn.Bootstrapping;
using Xunit;
using BoltOn.Mediator;
using BoltOn.IoC;
using BoltOn.IoC.SimpleInjector;
using BoltOn.Logging.NLog;

namespace BoltOn.Tests.Mediator
{
	public class MediatorTests : IDisposable
	{
		[Fact]
		public void Get_BootstrapWithDefaults_ResolvesHandlerAndExecutesIt()
		{
			// arrange
			// as there is conflict with Container_CallContainerAfterRun_ReturnsContainer test, added some delay
			System.Threading.Thread.Sleep(250);
			Bootstrapper
				.Instance
				// as mediator is register as scoped, and we cannot resolve scoped dependencies in simple
				// injector directly 
				.ExcludeAssemblies(typeof(SimpleInjectorContainerAdapter).Assembly, typeof(NLogLoggerAdapter<>).Assembly)
				.Run();

			// act
			var mediator = ServiceLocator.Current.GetInstance<IMediator>();
			var result = mediator.Get(new TestRequest());

			// assert 
			Assert.True(result.IsSuccessful);
			Assert.True(result.Data);
		}

		public void Dispose()
		{
			Bootstrapper
				.Instance
				.Dispose();
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
