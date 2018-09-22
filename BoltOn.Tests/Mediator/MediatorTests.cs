﻿using System;
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
		public void Get_BootstrapWithDefaults_ReturnsSuccessfulResult()
		{
			// arrange
			// as there is conflict with Container_CallContainerAfterRun_ReturnsContainer test, added some delay
			//System.Threading.Thread.Sleep(250);
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
		public void Get_RegisteredHandlerThatReturnsBool_ReturnsSuccessulResult()
		{
			// arrange
			var autoMocker = new AutoMocker();
			var sut = autoMocker.CreateInstance<BoltOn.Mediator.Mediator>();
			var serviceFactory = autoMocker.GetMock<BoltOn.IoC.IServiceFactory>();
			var testHandler = new Mock<TestHandler>();
			serviceFactory.Setup(s => s.GetInstance(typeof(IRequestHandler<TestRequest, bool>)))
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
		public void Get_RegisteredHandlerThatThrowsException_ReturnsUnsuccessfulResult()
		{
			// arrange
			var autoMocker = new AutoMocker();
			var sut = autoMocker.CreateInstance<BoltOn.Mediator.Mediator>();
			var serviceFactory = autoMocker.GetMock<BoltOn.IoC.IServiceFactory>();
			var testHandler = new Mock<TestHandler>();
			serviceFactory.Setup(s => s.GetInstance(typeof(IRequestHandler<TestRequest, bool>)))
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

		[Fact]
		public void Get_UnregisteredHandler_ReturnsUnsuccessfulResult()
		{
			// arrange
			var autoMocker = new AutoMocker();
			var sut = autoMocker.CreateInstance<BoltOn.Mediator.Mediator>();
			var serviceFactory = autoMocker.GetMock<BoltOn.IoC.IServiceFactory>();
			var testHandler = new Mock<TestHandler>();
			serviceFactory.Setup(s => s.GetInstance(typeof(IRequestHandler<TestRequest, bool>)))
			               .Returns(null);
			var request = new TestRequest();

			// act
			var result = sut.Get(request);

			// assert 
			Assert.False(result.IsSuccessful);
			Assert.False(result.Data);
			Assert.NotNull(result.Exception);
			Assert.Equal(string.Format(Constants.ExceptionMessages.
			                           HANDLER_NOT_FOUND, request), result.Exception.Message);
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

	public class TestCommand : ICommand<bool>
	{
	}

	public class TestCommandHandler : IRequestHandler<TestRequest, bool>
	{
		public virtual bool Handle(IRequest<bool> request)
		{
			return true;
		}
	} 
}
