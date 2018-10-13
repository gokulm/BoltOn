using System;
using BoltOn.Bootstrapping;
using Xunit;
using BoltOn.Mediator;
using BoltOn.IoC;
using BoltOn.IoC.SimpleInjector;
using BoltOn.Logging.NLog;
using Moq.AutoMock;
using Moq;
using System.Collections.Generic;
using BoltOn.Logging;
using System.Reflection;
using BoltOn.Logging.NetStandard;

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
				//.Configure(a => a.Middlewares = )
				.BoltOn();
				// as mediator is register as scoped, and we cannot resolve scoped dependencies in simple
				// injector directly 
				//.ExcludeAssemblies(typeof(SimpleInjectorContainerAdapter).Assembly, typeof(NLogLoggerAdapter<>).Assembly)
				//.BoltOnSimpleInjector(b =>
				//{
				//	b.AssemblyOptions = new BoltOnIoCAssemblyOptions
				//	{
				//		AssembliesToBeExcluded = new List<Assembly> { typeof(NLogLoggerAdapter<>).Assembly },
				//	};
				//})
				//.BoltOnNetStandardLogger()
				//.BoltOnMediator();

			// act
			var mediator = ServiceLocator.Current.GetInstance<IMediator>();
			var result = mediator.Get(new TestRequest());

			// assert 
			Assert.True(result.IsSuccessful);
			Assert.True(result.Data);
		}

		[Fact, Trait("Category", "Integration")]
		public void Get_MediatorWithMiddleware_ExecutesMiddleware()
		{
			// arrange
			Bootstrapper
				.Instance
				.BoltOn();
				// as mediator is register as scoped, and we cannot resolve scoped dependencies in simple
				// injector directly 
				//.ExcludeAssemblies(typeof(SimpleInjectorContainerAdapter).Assembly, typeof(NLogLoggerAdapter<>).Assembly)
				//.BoltOnSimpleInjector(b =>
				//{
				//	b.AssembliesToBeExcluded.Add(typeof(NLogLoggerAdapter<>).Assembly);
				//	b.AssembliesToBeExcluded.Add(typeof(SimpleInjectorContainerAdapter).Assembly);
				//})
				//.BoltOnSimpleInjector();

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
			var serviceFactory = autoMocker.GetMock<IServiceFactory>();
			var testHandler = new Mock<TestHandler>();
			serviceFactory.Setup(s => s.GetInstance(typeof(IRequestHandler<TestRequest, bool>)))
						  .Returns(testHandler.Object);
			serviceFactory.Setup(s => s.GetInstance(typeof(IEnumerable<IMiddleware>)))
						  .Returns(new List<IMiddleware>());
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
			var serviceFactory = autoMocker.GetMock<IServiceFactory>();
			var testHandler = new Mock<TestHandler>();
			serviceFactory.Setup(s => s.GetInstance(typeof(IRequestHandler<TestRequest, bool>)))
						   .Returns(testHandler.Object);
			serviceFactory.Setup(s => s.GetInstance(typeof(IEnumerable<IMiddleware>)))
						  .Returns(new List<IMiddleware>());
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
			serviceFactory.Setup(s => s.GetInstance(typeof(IEnumerable<IMiddleware>)))
						  .Returns(new List<IMiddleware>());
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

	public class TestMiddleware : IMiddleware
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
			_logger.Debug($"StopwatchMiddleware Ended");
			return response;
		}
	}

	public class MediatorTestsRegistrationTask : IBootstrapperRegistrationTask
	{
		public void Run(RegistrationTaskContext context)
		{
			context.Container.RegisterTransientCollection(typeof(IMiddleware), new[] { typeof(TestMiddleware) });
		}
	}
}
