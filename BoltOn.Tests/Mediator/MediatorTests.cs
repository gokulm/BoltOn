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
using BoltOn.Utilities;
using System.Linq;
using BoltOn.Tests.Common;

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
		public void Get_MediatorWithRequestSpecificMiddleware_ExecutesMiddleware()
		{
			// arrange
			var autoMocker = new AutoMocker();
			var sut = autoMocker.CreateInstance<BoltOn.Mediator.Mediator>();
			var serviceFactory = autoMocker.GetMock<IServiceFactory>();
			var testHandler = new Mock<TestHandler>();
			var middleware = new Mock<IMediatorMiddleware>();
			var logger = new Mock<IBoltOnLogger<TestMiddleware>>();
			var logger2 = new Mock<IBoltOnLogger<StopwatchMiddleware>>();
			var currentDateTimeRetriever = new Mock<ICurrentDateTimeRetriever>();
			var currentDateTime = DateTime.Now;
			currentDateTimeRetriever.Setup(s => s.Get()).Returns(currentDateTime)
									; serviceFactory.Setup(s => s.GetInstance(typeof(IRequestHandler<TestRequest, bool>)))
				.Returns(testHandler.Object);
			serviceFactory.Setup(s => s.GetInstance(typeof(IEnumerable<IMediatorMiddleware>)))
						  .Returns(new List<IMediatorMiddleware> { new TestRequestSpecificMiddleware(logger.Object),
				new StopwatchMiddleware(logger2.Object, currentDateTimeRetriever.Object) });
			var request = new TestRequest();
			testHandler.Setup(s => s.Handle(request)).Returns(true);

			// act
			var result = sut.Get(request);

			// assert 
			Assert.True(result.IsSuccessful);
			Assert.True(result.Data);
			logger.Verify(l => l.Debug("TestRequestSpecificMiddleware Started"), Times.Never);
			logger.Verify(l => l.Debug("TestRequestSpecificMiddleware Ended"), Times.Never);
			logger2.Verify(l => l.Debug($"StopwatchMiddleware started at {currentDateTime}"), Times.Once);
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

		[Fact, Trait("Category", "Integration"), TestPriority(21)]
		public void Get_BootstrapWithDefaults_InvokesAllTheMiddlewaresAndReturnsSuccessfulResult()
		{
			// arrange
			Bootstrapper
				.Instance
				.ConfigureIoC(b =>
				{
					b.AssemblyOptions = new BoltOnIoCAssemblyOptions
					{
						AssembliesToBeExcluded = new List<System.Reflection.Assembly>
							{
								typeof(SimpleInjectorContainerAdapter).Assembly
							}
					};
				})
				.BoltOn();
			var currentDateTimeRetriever = ServiceLocator.Current.GetInstance<ICurrentDateTimeRetriever>();

			// act
			var mediator = ServiceLocator.Current.GetInstance<IMediator>();
			var result = mediator.Get(new TestRequest());

			// assert 
			Assert.True(result.IsSuccessful);
			Assert.True(result.Data);
			Assert.NotNull(LoggerDebugStatementContainer.Statements.FirstOrDefault(d => d == $"StopwatchMiddleware started at {currentDateTimeRetriever.Get()}"));
			Assert.NotNull(LoggerDebugStatementContainer.Statements.FirstOrDefault(d => d == $"StopwatchMiddleware ended at {currentDateTimeRetriever.Get()}. Time elapsed: 0"));
			Assert.NotNull(LoggerDebugStatementContainer.Statements.FirstOrDefault(d => d == "TestMiddleware Started"));
		}

		[Fact, Trait("Category", "Integration"), TestPriority(20)]
		public void Get_BootstrapWithCustomMiddlewares_InvokesDefaultAndCustomMiddlewareInOrderAndReturnsSuccessfulResult()
		{
			// arrange
			System.Threading.Thread.Sleep(1000);
			Bootstrapper
				.Instance
				.ConfigureIoC(b =>
				{
					b.AssemblyOptions = new BoltOnIoCAssemblyOptions
					{
						AssembliesToBeExcluded = new List<System.Reflection.Assembly>
							{
								typeof(SimpleInjectorContainerAdapter).Assembly
							}
					};
				})
				.ConfigureMediator(m =>
				{
					m.RegisterMiddleware<TestMiddleware>();
				})
				.BoltOn();
			var currentDateTimeRetriever = ServiceLocator.Current.GetInstance<ICurrentDateTimeRetriever>();

			// act
			var mediator = ServiceLocator.Current.GetInstance<IMediator>();
			var result = mediator.Get(new TestRequest());

			// assert 
			Assert.True(result.IsSuccessful);
			Assert.True(result.Data);
			Assert.True(LoggerDebugStatementContainer.Statements.IndexOf("TestMiddleware Started") > 0);
			Assert.True(LoggerDebugStatementContainer.Statements.IndexOf("TestMiddleware Ended") > 0);
			Assert.True(LoggerDebugStatementContainer.Statements.IndexOf("TestRequestSpecificMiddleware Started") == -1);
			Assert.True(LoggerDebugStatementContainer.Statements.IndexOf($"StopwatchMiddleware started at {currentDateTimeRetriever.Get()}") <
						LoggerDebugStatementContainer.Statements.IndexOf("TestMiddleware Started"));
			Assert.NotNull(LoggerDebugStatementContainer.Statements.FirstOrDefault(d => d == $"StopwatchMiddleware started at {currentDateTimeRetriever.Get()}"));
			Assert.NotNull(LoggerDebugStatementContainer.Statements.FirstOrDefault(d => d == $"StopwatchMiddleware ended at {currentDateTimeRetriever.Get()}. Time elapsed: 0"));
			Assert.True(LoggerDebugStatementContainer.Statements.IndexOf($"StopwatchMiddleware ended at {currentDateTimeRetriever.Get()}. Time elapsed: 0") >
						LoggerDebugStatementContainer.Statements.IndexOf("TestMiddleware Ended"));
		}

		[Fact, Trait("Category", "Integration"), TestPriority(20)]
		public void Get_BootstrapWithCustomMiddlewaresAndClear_InvokesOnlyCustomMiddlewareAndReturnsSuccessfulResult()
		{
			// arrange
			Bootstrapper
				.Instance
				.ConfigureIoC(b =>
				{
					b.AssemblyOptions = new BoltOnIoCAssemblyOptions
					{
						AssembliesToBeExcluded = new List<System.Reflection.Assembly>
							{
								typeof(SimpleInjectorContainerAdapter).Assembly
							}
					};
				})
				.ConfigureMediator(m =>
				{
					m.ClearMiddlewares();
					m.RegisterMiddleware<TestMiddleware>();
				})
				.BoltOn();
			var currentDateTimeRetriever = ServiceLocator.Current.GetInstance<ICurrentDateTimeRetriever>();

			// act
			var mediator = ServiceLocator.Current.GetInstance<IMediator>();
			var result = mediator.Get(new TestRequest());

			// assert 
			Assert.True(result.IsSuccessful);
			Assert.True(result.Data);
			Assert.NotNull(LoggerDebugStatementContainer.Statements.FirstOrDefault(f => f == "TestMiddleware Started"));
			Assert.NotNull(LoggerDebugStatementContainer.Statements.FirstOrDefault(f => f == "TestMiddleware Ended"));
			Assert.Null(LoggerDebugStatementContainer.Statements.FirstOrDefault(d => d == $"StopwatchMiddleware started at {currentDateTimeRetriever.Get()}"));
			Assert.Null(LoggerDebugStatementContainer.Statements.FirstOrDefault(d => d == $"StopwatchMiddleware ended at {currentDateTimeRetriever.Get()}. Time elapsed: 0"));
		}

		public void Dispose()
		{
			LoggerDebugStatementContainer.Statements.Clear();
			Bootstrapper
				.Instance
				.Dispose();
		}
	}

	public class Test2BootstrapperRegistrationTask : IBootstrapperRegistrationTask
	{
		public void Run(RegistrationTaskContext context)
		{
			var currentDateTimeRetriever = new Mock<ICurrentDateTimeRetriever>();
			var currentDateTime = DateTime.Parse("10/27/2018 12:51:59 PM");
			currentDateTimeRetriever.Setup(s => s.Get()).Returns(currentDateTime);
			context.Container.RegisterTransient(() => currentDateTimeRetriever.Object);

			var testMiddlewareLogger = new Mock<IBoltOnLogger<TestMiddleware>>();
			testMiddlewareLogger.Setup(s => s.Debug(It.IsAny<string>()))
								.Callback<string>(st => LoggerDebugStatementContainer.Statements.Add(st));
			context.Container.RegisterTransient(() => testMiddlewareLogger.Object);
			var stopWatchMiddlewareLogger = new Mock<IBoltOnLogger<StopwatchMiddleware>>();
			stopWatchMiddlewareLogger.Setup(s => s.Debug(It.IsAny<string>()))
									 .Callback<string>(st => LoggerDebugStatementContainer.Statements.Add(st));
			context.Container.RegisterTransient(() => stopWatchMiddlewareLogger.Object);
		}
	}

	public class TestRequest : IRequest<bool>, IEnableUnitOfWorkMiddleware, IEnableStopwatchMiddleware
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
			_logger.Debug("TestMiddleware Started");
			var response = next.Invoke(request);
			_logger.Debug("TestMiddleware Ended");
			return response;
		}

		public void Dispose()
		{
		}
	}

	public interface IRequestSpecificMiddleware
	{
	}

	public class TestRequestSpecificMiddleware : BaseRequestSpecificMiddleware<IRequestSpecificMiddleware>
	{
		private readonly IBoltOnLogger<TestMiddleware> _logger;

		public TestRequestSpecificMiddleware(IBoltOnLogger<TestMiddleware> logger)
		{
			_logger = logger;
		}

		public override void Dispose()
		{
		}

		public override StandardDtoReponse<TResponse> Execute<TRequest, TResponse>(IRequest<TResponse> request,
																				   Func<IRequest<TResponse>, StandardDtoReponse<TResponse>> next)
		{
			_logger.Debug($"TestRequestSpecificMiddleware Started");
			var response = next.Invoke(request);
			_logger.Debug($"TestRequestSpecificMiddleware Ended");
			return response;
		}
	}

	public class LoggerDebugStatementContainer
	{
		public static List<string> Statements { get; set; } = new List<string>();
	}
}
