using System;
using BoltOn.Bootstrapping;
using Xunit;
using BoltOn.Mediator;
using BoltOn.IoC;
using BoltOn.IoC.SimpleInjector;
using BoltOn.Logging.NLog;
using Moq.AutoMock;
using Moq;

namespace BoltOn.Tests.Mediator
{
	public class MediatorTests : IDisposable
	{
		[Fact, Trait("Category", "Integration")]
		public void Get_BootstrapWithDefaults_HandlerIsSuccessful()
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

		[Fact]
		public void Get_MediatorWithRegisteredHandler_ReturnsSuccessulResult()
		{
			// arrange
			var autoMocker = new AutoMocker();
			var sut = autoMocker.CreateInstance<BoltOn.Mediator.Mediator>();
			var serviceProvider = autoMocker.GetMock<BoltOn.IoC.IServiceProvider>();
			var testHandler = new Mock<TestHandler>();
			serviceProvider.Setup(s => s.GetInstance(typeof(IRequestHandler<TestRequest, bool>)))
			               .Returns(testHandler.Object);
			var request = new TestRequest();
			testHandler.Setup(s => s.Handle(request)).Returns(true);

			// act
			var result = sut.Get(request);

			// assert 
			Assert.True(result.IsSuccessful);
			Assert.True(result.Data);
		}

		[Fact]
		public void Get_MediatorWithRegisteredHandler_ReturnsUnsuccessfulResult()
		{
			// arrange
			var autoMocker = new AutoMocker();
			var sut = autoMocker.CreateInstance<BoltOn.Mediator.Mediator>();
			var serviceProvider = autoMocker.GetMock<BoltOn.IoC.IServiceProvider>();
			var testHandler = new Mock<TestHandler>();
			serviceProvider.Setup(s => s.GetInstance(typeof(IRequestHandler<TestRequest, bool>)))
						   .Returns(testHandler.Object);
			var request = new TestRequest();
			testHandler.Setup(s => s.Handle(request)).Throws(new Exception("handler failed"));

			// act
			var result = sut.Get(request);

			// assert 
			Assert.False(result.IsSuccessful);
			Assert.False(result.Data);
			Assert.NotNull(result.Exception);
			Assert.Equal("handler failed", result.Exception.Message);
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
		public virtual bool Handle(IRequest<bool> request)
		{
			return true;
		}
	} 
}
