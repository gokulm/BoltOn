using System;
using BoltOn.Bootstrapping;
using Xunit;
using BoltOn.Mediator;
using BoltOn.IoC;
using Moq.AutoMock;
using Moq;
using System.Collections.Generic;
using BoltOn.Logging;
using BoltOn.IoC.SimpleInjector;

namespace BoltOn.Tests.Mediator
{
	public class MediatorTests : IDisposable
	{
		[Fact]
		public void Get_RegisteredHandlerThatReturnsBool_ReturnsSuccessulResult()
		{
			// arrange
			var autoMocker = new AutoMocker();
			var sut = autoMocker.CreateInstance<BoltOn.Mediator.Mediator>();
			var serviceFactory = autoMocker.GetMock<IServiceFactory>();
			var testHandler = new Mock<TestHandler>();
			serviceFactory.Setup(s => s.GetInstance(typeof(IRequestHandler<TestRequest, bool>)))
						  .Returns(testHandler.Object);
			serviceFactory.Setup(s => s.GetInstance(typeof(IEnumerable<IMediatorMiddleware>)))
						  .Returns(new List<IMediatorMiddleware>());
			var request = new TestRequest();
			testHandler.Setup(s => s.Handle(request)).Returns(true);

			// act
			var result = sut.Get(request);

			// assert 
			Assert.True(result.IsSuccessful);
			Assert.True(result.Data);
		}

		[Fact]
		public void Get_MediatorWithMiddleware_ExecutesMiddleware()
		{
			// arrange
			var autoMocker = new AutoMocker();
			var sut = autoMocker.CreateInstance<BoltOn.Mediator.Mediator>();
			var serviceFactory = autoMocker.GetMock<IServiceFactory>();
			var testHandler = new Mock<TestHandler>();
			var middleware = new Mock<IMediatorMiddleware>();
			var logger = new Mock<IBoltOnLogger<TestMiddleware>>();
			serviceFactory.Setup(s => s.GetInstance(typeof(IRequestHandler<TestRequest, bool>)))
						  .Returns(testHandler.Object);
			serviceFactory.Setup(s => s.GetInstance(typeof(IEnumerable<IMediatorMiddleware>)))
						  .Returns(new List<IMediatorMiddleware> { new TestMiddleware(logger.Object) });
			var request = new TestRequest();
			testHandler.Setup(s => s.Handle(request)).Returns(true);

			// act
			var result = sut.Get(request);

			// assert 
			Assert.True(result.IsSuccessful);
			Assert.True(result.Data);
			logger.Verify(l => l.Debug("TestMiddleware Started"));
			logger.Verify(l => l.Debug("TestMiddleware Ended"));
		}

		[Fact]
		public void Get_RegisteredHandlerThatThrowsException_ReturnsUnsuccessfulResult()
		{
			// arrange
			var autoMocker = new AutoMocker();
			var sut = autoMocker.CreateInstance<BoltOn.Mediator.Mediator>();
			var serviceFactory = autoMocker.GetMock<IServiceFactory>();
			var testHandler = new Mock<TestHandler>();
			serviceFactory.Setup(s => s.GetInstance(typeof(IRequestHandler<TestRequest, bool>)))
						   .Returns(testHandler.Object);
			serviceFactory.Setup(s => s.GetInstance(typeof(IEnumerable<IMediatorMiddleware>)))
						  .Returns(new List<IMediatorMiddleware>());
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
			var serviceFactory = autoMocker.GetMock<IServiceFactory>();
			var testHandler = new Mock<TestHandler>();
			serviceFactory.Setup(s => s.GetInstance(typeof(IRequestHandler<TestRequest, bool>)))
						  .Returns(null);
			serviceFactory.Setup(s => s.GetInstance(typeof(IEnumerable<IMediatorMiddleware>)))
						  .Returns(new List<IMediatorMiddleware>());
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

		[Fact, Trait("Category", "Integration")]
		public void Get_BootstrapWithDefaults_ReturnsSuccessfulResult()
		{
			// arrange
			Bootstrapper
				.Instance.ConfigureIoC(b =>
				{
					b.AssemblyOptions = new BoltOnIoCAssemblyOptions
					{
						AssembliesToBeExcluded = new List<System.Reflection.Assembly>
							{
								typeof(SimpleInjectorContainerAdapter).Assembly
							}
					};
				})
				//.ConfigureMediator(m =>
				//{
				//	m.RegisterMiddleware<TestMiddleware>();
				//})
				.BoltOn();

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

	public class TestRequest : IRequest<bool>, IEnableUnitOfWorkMiddleware
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

	public class TestMiddleware : IMediatorMiddleware
	{
		private readonly IBoltOnLogger<TestMiddleware> _logger;

		public TestMiddleware(IBoltOnLogger<TestMiddleware> logger)
		{
			_logger = logger;
		}

		public StandardDtoReponse<TResponse> Run<TRequest, TResponse>(IRequest<TResponse> request,
																	 Func<IRequest<TResponse>, StandardDtoReponse<TResponse>> next)
		   where TRequest : IRequest<TResponse>
		{
			_logger.Debug($"TestMiddleware Started");
			var response = next.Invoke(request);
			_logger.Debug($"TestMiddleware Ended");
			return response;
		}

		public void Dispose()
		{
		}
	}
}
