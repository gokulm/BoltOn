using System;
using System.Collections.Generic;
using System.Linq;
using BoltOn.Bootstrapping;
using BoltOn.Logging;
using BoltOn.Mediator;
using BoltOn.Tests.Common;
using BoltOn.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.AutoMock;
using Xunit;

namespace BoltOn.Tests.Mediator
{
	public class MediatorTests : IDisposable
	{
		[Fact]
		public void Get_RegisteredHandlerThatReturnsBool_ReturnsSuccessulResult()
		{
			// arrange
			var autoMocker = new AutoMocker();
			var serviceProvider = autoMocker.GetMock<IServiceProvider>();
			var testHandler = new Mock<TestHandler>();
			serviceProvider.Setup(s => s.GetService(typeof(IRequestHandler<TestRequest, bool>)))
						  .Returns(testHandler.Object);
			autoMocker.Use<IEnumerable<IMediatorMiddleware>>(new List<IMediatorMiddleware>());
			var sut = autoMocker.CreateInstance<BoltOn.Mediator.Mediator>();
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
			var serviceProvider = autoMocker.GetMock<IServiceProvider>();
			var testHandler = new Mock<TestHandler>();
			var middleware = new Mock<IMediatorMiddleware>();
			var logger = new Mock<IBoltOnLogger<TestMiddleware>>();
			serviceProvider.Setup(s => s.GetService(typeof(IRequestHandler<TestRequest, bool>)))
						  .Returns(testHandler.Object);
			autoMocker.Use<IEnumerable<IMediatorMiddleware>>(new List<IMediatorMiddleware>() { new TestMiddleware(logger.Object) });
			var sut = autoMocker.CreateInstance<BoltOn.Mediator.Mediator>();
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
			var serviceProvider = autoMocker.GetMock<IServiceProvider>();
			var testHandler = new Mock<TestHandler>();
			var middleware = new Mock<IMediatorMiddleware>();
			var logger = new Mock<IBoltOnLogger<TestMiddleware>>();
			var logger2 = new Mock<IBoltOnLogger<StopwatchMiddleware>>();
			var currentDateTimeRetriever = new Mock<ICurrentDateTimeRetriever>();
			var currentDateTime = DateTime.Now;
			currentDateTimeRetriever.Setup(s => s.Now).Returns(currentDateTime);
			serviceProvider.Setup(s => s.GetService(typeof(IRequestHandler<TestRequest, bool>)))
				.Returns(testHandler.Object);
			autoMocker.Use<IEnumerable<IMediatorMiddleware>>(new List<IMediatorMiddleware> { new TestRequestSpecificMiddleware(logger.Object),
				new StopwatchMiddleware(logger2.Object, currentDateTimeRetriever.Object) });
			var sut = autoMocker.CreateInstance<BoltOn.Mediator.Mediator>();
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
			var serviceProvider = autoMocker.GetMock<IServiceProvider>();
			var testHandler = new Mock<TestHandler>();
			serviceProvider.Setup(s => s.GetService(typeof(IRequestHandler<TestRequest, bool>)))
						   .Returns(testHandler.Object);
			autoMocker.Use<IEnumerable<IMediatorMiddleware>>(new List<IMediatorMiddleware>());
			var sut = autoMocker.CreateInstance<BoltOn.Mediator.Mediator>();
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
			var serviceProvider = autoMocker.GetMock<IServiceProvider>();
			var testHandler = new Mock<TestHandler>();
			serviceProvider.Setup(s => s.GetService(typeof(IRequestHandler<TestRequest, bool>)))
						  .Returns(null);
			autoMocker.Use<IEnumerable<IMediatorMiddleware>>(new List<IMediatorMiddleware>());
			var sut = autoMocker.CreateInstance<BoltOn.Mediator.Mediator>();
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
			LoggerDebugStatementContainer.IsClearMiddlewares = false;
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddLogging();
			serviceCollection.BoltOn();
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.UseBoltOn();
			var currentDateTimeRetriever = serviceProvider.GetService<ICurrentDateTimeRetriever>();
			var mediator = serviceProvider.GetService<IMediator>();

			// act
			var result = mediator.Get(new TestRequest());

			// assert 
			Assert.True(result.IsSuccessful);
			Assert.True(result.Data);
			Assert.NotNull(LoggerDebugStatementContainer.Statements.FirstOrDefault(d => d ==
																				   $"StopwatchMiddleware started at {currentDateTimeRetriever.Now}"));
			Assert.NotNull(LoggerDebugStatementContainer.Statements.FirstOrDefault(d => d ==
																				   $"StopwatchMiddleware ended at {currentDateTimeRetriever.Now}. Time elapsed: 0"));
			Assert.NotNull(LoggerDebugStatementContainer.Statements.FirstOrDefault(d => d == "TestMiddleware Started"));
		}

		[Fact, Trait("Category", "Integration"), TestPriority(22)]
		public void Get_BootstrapWithCustomMiddlewares_InvokesDefaultAndCustomMiddlewareInOrderAndReturnsSuccessfulResult()
		{
			// arrange
			LoggerDebugStatementContainer.IsClearMiddlewares = false;
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddLogging();
			serviceCollection.BoltOn();
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.UseBoltOn();
			var currentDateTimeRetriever = serviceProvider.GetService<ICurrentDateTimeRetriever>();
			var mediator = serviceProvider.GetService<IMediator>();

			// act
			var result = mediator.Get(new TestRequest());

			// assert 
			Assert.True(result.IsSuccessful);
			Assert.True(result.Data);
			Assert.True(LoggerDebugStatementContainer.Statements.IndexOf("TestMiddleware Started") > 0);
			Assert.True(LoggerDebugStatementContainer.Statements.IndexOf("TestMiddleware Ended") > 0);
			Assert.True(LoggerDebugStatementContainer.Statements.IndexOf("TestRequestSpecificMiddleware Started") == -1);
			Assert.True(LoggerDebugStatementContainer.Statements.IndexOf($"StopwatchMiddleware started at {currentDateTimeRetriever.Now}") <
						LoggerDebugStatementContainer.Statements.IndexOf("TestMiddleware Started"));
			Assert.NotNull(LoggerDebugStatementContainer.Statements.FirstOrDefault(d => d == $"StopwatchMiddleware started at {currentDateTimeRetriever.Now}"));
			Assert.NotNull(LoggerDebugStatementContainer.Statements.FirstOrDefault(d => d == $"StopwatchMiddleware ended at {currentDateTimeRetriever.Now}. Time elapsed: 0"));
			Assert.True(LoggerDebugStatementContainer.Statements.IndexOf($"StopwatchMiddleware ended at {currentDateTimeRetriever.Now}. Time elapsed: 0") >
						LoggerDebugStatementContainer.Statements.IndexOf("TestMiddleware Ended"));
		}

		[Fact, Trait("Category", "Integration"), TestPriority(23)]
		public void Get_BootstrapWithCustomMiddlewaresAndClear_InvokesOnlyCustomMiddlewareAndReturnsSuccessfulResult()
		{
			// arrange
			LoggerDebugStatementContainer.IsClearMiddlewares = true;
			var serviceCollection = new ServiceCollection();
			serviceCollection.BoltOn();
			serviceCollection.AddLogging();
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.UseBoltOn();
			var currentDateTimeRetriever = serviceProvider.GetService<ICurrentDateTimeRetriever>();
			var mediator = serviceProvider.GetService<IMediator>();

			// act
			var result = mediator.Get(new TestRequest());

			// assert 
			Assert.True(result.IsSuccessful);
			Assert.True(result.Data);
			Assert.NotNull(LoggerDebugStatementContainer.Statements.FirstOrDefault(f => f == "TestMiddleware Started"));
			Assert.NotNull(LoggerDebugStatementContainer.Statements.FirstOrDefault(f => f == "TestMiddleware Ended"));
			Assert.Null(LoggerDebugStatementContainer.Statements.FirstOrDefault(d => d == $"StopwatchMiddleware started at {currentDateTimeRetriever.Now}"));
			Assert.Null(LoggerDebugStatementContainer.Statements.FirstOrDefault(d => d == $"StopwatchMiddleware ended at {currentDateTimeRetriever.Now}. Time elapsed: 0"));
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
			currentDateTimeRetriever.Setup(s => s.Now).Returns(currentDateTime);
			context.Container.AddTransient((s) => currentDateTimeRetriever.Object);

			var testMiddlewareLogger = new Mock<IBoltOnLogger<TestMiddleware>>();
			testMiddlewareLogger.Setup(s => s.Debug(It.IsAny<string>()))
								.Callback<string>(st => LoggerDebugStatementContainer.Statements.Add(st));
			context.Container.AddTransient((s) => testMiddlewareLogger.Object);
			var stopWatchMiddlewareLogger = new Mock<IBoltOnLogger<StopwatchMiddleware>>();
			stopWatchMiddlewareLogger.Setup(s => s.Debug(It.IsAny<string>()))
									 .Callback<string>(st => LoggerDebugStatementContainer.Statements.Add(st));
			context.Container.AddTransient((s) => stopWatchMiddlewareLogger.Object);


			if (LoggerDebugStatementContainer.IsClearMiddlewares)
				context.Container.RemoveAllMiddlewares();
			context.Container.AddMiddleware<TestMiddleware>();
		}
	}

	public class TestPreRegistrationTask : IBootstrapperPreRegistrationTask
	{
		public void Run(PreRegistrationTaskContext context)
		{
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
		public static bool IsClearMiddlewares { get; set; }
	}
}
