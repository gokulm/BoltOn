using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using BoltOn.Bootstrapping;
using BoltOn.Logging;
using BoltOn.Mediator;
using BoltOn.Mediator.Middlewares;
using BoltOn.Tests.Common;
using BoltOn.UoW;
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
		public void Get_MediatorWithCommandRequest_ExecutesUoWMiddlewareAndStartsTransactionsWithDefaultCommandIsolationLevel()
		{
			// arrange
			var autoMocker = new AutoMocker();
			var serviceProvider = autoMocker.GetMock<IServiceProvider>();
			var testHandler = new Mock<TestHandler>();
			var middleware = new Mock<IMediatorMiddleware>();
			var logger = new Mock<IBoltOnLogger<UnitOfWorkMiddleware>>();
			serviceProvider.Setup(s => s.GetService(typeof(IRequestHandler<TestCommand, bool>)))
				.Returns(testHandler.Object);
			var uowProvider = autoMocker.GetMock<IUnitOfWorkProvider>();
			var uow = new Mock<IUnitOfWork>();
			uowProvider.Setup(u => u.Get(It.IsAny<IsolationLevel>(), It.IsAny<TimeSpan>())).Returns(uow.Object);
			var uowOptions = autoMocker.GetMock<UnitOfWorkOptions>();
			uowOptions.Setup(u => u.IsolationLevel).Returns(IsolationLevel.ReadCommitted);
			var uowOptionsRetriever = autoMocker.GetMock<IUnitOfWorkOptionsRetriever>();
			var request = new TestCommand();
			uowOptionsRetriever.Setup(u => u.Get(request)).Returns(uowOptions.Object);
			autoMocker.Use<IEnumerable<IMediatorMiddleware>>(new List<IMediatorMiddleware>
			{
				new UnitOfWorkMiddleware(logger.Object, uowProvider.Object, uowOptionsRetriever.Object)
			});
			var sut = autoMocker.CreateInstance<BoltOn.Mediator.Mediator>();
			testHandler.Setup(s => s.Handle(request)).Returns(true);

			// act
			var result = sut.Get(request);

			// assert 
			Assert.True(result.IsSuccessful);
			Assert.True(result.Data);
			uowProvider.Verify(u => u.Get(IsolationLevel.ReadCommitted, TransactionManager.DefaultTimeout));
			uow.Verify(u => u.Begin());
			uow.Verify(u => u.Commit());
			logger.Verify(l => l.Debug($"About to begin UoW with IsolationLevel: {IsolationLevel.ReadCommitted.ToString()}"));
			logger.Verify(l => l.Debug("Committed UoW"));
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

		[Fact, Trait("Category", "Integration")]
		public void Get_BootstrapWithDefaults_InvokesAllTheMiddlewaresAndReturnsSuccessfulResult()
		{
			// arrange
			TestHelper.IsClearMiddlewares = false;
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
			Assert.NotNull(TestHelper.LoggerStatements.FirstOrDefault(d => d ==
																				   $"StopwatchMiddleware started at {currentDateTimeRetriever.Now}"));
			Assert.NotNull(TestHelper.LoggerStatements.FirstOrDefault(d => d ==
																				   $"StopwatchMiddleware ended at {currentDateTimeRetriever.Now}. Time elapsed: 0"));
			Assert.NotNull(TestHelper.LoggerStatements.FirstOrDefault(d => d == "TestMiddleware Started"));
		}

		[Fact, Trait("Category", "Integration")]
		public void Get_BootstrapWithCustomMiddlewares_InvokesDefaultAndCustomMiddlewareInOrderAndReturnsSuccessfulResult()
		{
			// arrange
			TestHelper.IsClearMiddlewares = false;
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
			Assert.True(TestHelper.LoggerStatements.IndexOf("TestMiddleware Started") > 0);
			Assert.True(TestHelper.LoggerStatements.IndexOf("TestMiddleware Ended") > 0);
			Assert.True(TestHelper.LoggerStatements.IndexOf("TestRequestSpecificMiddleware Started") == -1);
			Assert.True(TestHelper.LoggerStatements.IndexOf($"StopwatchMiddleware started at {currentDateTimeRetriever.Now}") <
						TestHelper.LoggerStatements.IndexOf("TestMiddleware Started"));
			Assert.NotNull(TestHelper.LoggerStatements.FirstOrDefault(d => d == $"StopwatchMiddleware started at {currentDateTimeRetriever.Now}"));
			Assert.NotNull(TestHelper.LoggerStatements.FirstOrDefault(d => d == $"StopwatchMiddleware ended at {currentDateTimeRetriever.Now}. " +
																				   "Time elapsed: 0"));
			Assert.True(TestHelper.LoggerStatements.IndexOf($"StopwatchMiddleware ended at {currentDateTimeRetriever.Now}. Time elapsed: 0") >
						TestHelper.LoggerStatements.IndexOf("TestMiddleware Ended"));
		}

		[Fact, Trait("Category", "Integration")]
		public void Get_BootstrapWithCustomMiddlewaresAndClear_InvokesOnlyCustomMiddlewareAndReturnsSuccessfulResult()
		{
			// arrange
			TestHelper.IsClearMiddlewares = true;
			var serviceCollection = new ServiceCollection();
			serviceCollection.BoltOn();
			serviceCollection.AddLogging();
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.UseBoltOn();
			var currentDateTimeRetriever = serviceProvider.GetService<ICurrentDateTimeRetriever>();
			var sut = serviceProvider.GetService<IMediator>();
			var testMiddleware = serviceProvider.GetService<TestMiddleware>(); 

			// act
			var result = sut.Get(new TestRequest());

			// assert 
			Assert.True(result.IsSuccessful);
			Assert.True(result.Data);
			Assert.NotNull(TestHelper.LoggerStatements.FirstOrDefault(f => f == "TestMiddleware Started"));
			Assert.NotNull(TestHelper.LoggerStatements.FirstOrDefault(f => f == "TestMiddleware Ended"));
			Assert.Null(TestHelper.LoggerStatements.FirstOrDefault(d => d == $"StopwatchMiddleware started at {currentDateTimeRetriever.Now}"));
			Assert.Null(TestHelper.LoggerStatements.FirstOrDefault(d => d == $"StopwatchMiddleware ended at {currentDateTimeRetriever.Now}. " +
																				"Time elapsed: 0"));
		}

		[Fact, Trait("Category", "Integration")]
		public void Get_MediatorWithQueryRequest_ExecutesUoWMiddlewareAndStartsTransactionsWithDefaultQueryIsolationLevel()
		{
			// arrange
			TestHelper.IsCustomizeIsolationLevel = false;
			var serviceCollection = new ServiceCollection();
			serviceCollection
				.BoltOn()
				.AddLogging();
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.UseBoltOn();
			var sut = serviceProvider.GetService<IMediator>();

			// act
			var result = sut.Get(new TestQuery());

			// assert 
			Assert.True(result.IsSuccessful);
			Assert.True(result.Data);
			Assert.NotNull(TestHelper.LoggerStatements.FirstOrDefault(f => f == "Getting isolation level for Query"));
		}

		[Fact, Trait("Category", "Integration")]
		public void Get_MediatorWithQueryRequest_ExecutesUoWMiddlewareAndStartsTransactionsWithCustomizedQueryIsolationLevel()
		{
			// arrange
			TestHelper.IsCustomizeIsolationLevel = true;
			var serviceCollection = new ServiceCollection();
			serviceCollection
				.BoltOn()
				.AddLogging();
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.UseBoltOn();
			var sut = serviceProvider.GetService<IMediator>();

			// act
			var result = sut.Get(new TestQuery());

			// assert 
			Assert.True(result.IsSuccessful);
			Assert.True(result.Data);
			Assert.NotNull(TestHelper.LoggerStatements.FirstOrDefault(f => f == "Getting isolation level for Command or Query"));
		}

		public void Dispose()
		{
			TestHelper.LoggerStatements.Clear();
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
								.Callback<string>(st => TestHelper.LoggerStatements.Add(st));
			context.Container.AddTransient((s) => testMiddlewareLogger.Object);

			var stopWatchMiddlewareLogger = new Mock<IBoltOnLogger<StopwatchMiddleware>>();
			stopWatchMiddlewareLogger.Setup(s => s.Debug(It.IsAny<string>()))
									 .Callback<string>(st => TestHelper.LoggerStatements.Add(st));
			context.Container.AddTransient((s) => stopWatchMiddlewareLogger.Object);

			var customUoWOptionRetrieverLogger = new Mock<IBoltOnLogger<CustomUnitOfWorkOptionsRetriever>>();
			customUoWOptionRetrieverLogger.Setup(s => s.Debug(It.IsAny<string>()))
								.Callback<string>(st => TestHelper.LoggerStatements.Add(st));
			context.Container.AddTransient((s) => customUoWOptionRetrieverLogger.Object);

			var uowOptionsRetrieverLogger = new Mock<IBoltOnLogger<UnitOfWorkOptionsRetriever>>();
			uowOptionsRetrieverLogger.Setup(s => s.Debug(It.IsAny<string>()))
								.Callback<string>(st => TestHelper.LoggerStatements.Add(st));
			context.Container.AddTransient((s) => uowOptionsRetrieverLogger.Object);

			if (TestHelper.IsClearMiddlewares)
				context.Container.RemoveAllMiddlewares();

			if (TestHelper.IsCustomizeIsolationLevel)
				context.Container.AddSingleton<IUnitOfWorkOptionsRetriever, CustomUnitOfWorkOptionsRetriever>();

			context.Container.AddMiddleware<TestMiddleware>();
		}
	}

	public class TestPreRegistrationTask : IBootstrapperPreRegistrationTask
	{
		public void Run(PreRegistrationTaskContext context)
		{
		}
	}

	public class TestRequest : IRequest<bool>, IEnableStopwatchMiddleware
	{
	}

	public class TestHandler : IRequestHandler<TestRequest, bool>, IRequestHandler<TestCommand, bool>,
											IRequestHandler<TestQuery, bool>
	{
		public virtual bool Handle(TestRequest request)
		{
			return true;
		}

		public virtual bool Handle(TestCommand request)
		{
			return true;
		}

		public virtual bool Handle(TestQuery request)
		{
			return true;
		}
	}

	public class TestCommand : ICommand<bool>
	{
	}

	public class TestQuery : IQuery<bool>
	{
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

	public class CustomUnitOfWorkOptionsRetriever : IUnitOfWorkOptionsRetriever
	{
		private readonly IBoltOnLogger<CustomUnitOfWorkOptionsRetriever> _logger;

		public CustomUnitOfWorkOptionsRetriever(IBoltOnLogger<CustomUnitOfWorkOptionsRetriever> logger)
		{
			_logger = logger;
		}

		public UnitOfWorkOptions Get<TResponse>(IRequest<TResponse> request)
		{
			IsolationLevel isolationLevel;
			switch (request)
			{
				case ICommand<TResponse> c:
				case IQuery<TResponse> q:
					_logger.Debug("Getting isolation level for Command or Query");
					isolationLevel = IsolationLevel.ReadCommitted;
					break;
				default:
					throw new Exception("Request should implement ICommand<> or IQuery<> to enable Unit of Work.");
			}
			return new UnitOfWorkOptions { IsolationLevel = isolationLevel };
		}
	}

	public class TestHelper
	{
		public static List<string> LoggerStatements { get; set; } = new List<string>();
		public static bool IsClearMiddlewares { get; set; }
		public static bool IsCustomizeIsolationLevel { get; set; }
	}
}
