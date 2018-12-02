using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using BoltOn.Bootstrapping;
using BoltOn.Logging;
using BoltOn.Mediator;
using BoltOn.Mediator.Middlewares;
using BoltOn.Mediator.Pipeline;
using BoltOn.Mediator.UoW;
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
		public void Get_RegisteredHandlerThatReturnsBool_ReturnsSuccessfulResult()
		{
			// arrange
			var autoMocker = new AutoMocker();
			var serviceProvider = autoMocker.GetMock<IServiceProvider>();
			var testHandler = new Mock<TestHandler>();
			serviceProvider.Setup(s => s.GetService(typeof(IRequestHandler<TestRequest, StandardBooleanResponse>)))
						  .Returns(testHandler.Object);
			autoMocker.Use<IEnumerable<IMediatorMiddleware>>(new List<IMediatorMiddleware>());
			var sut = autoMocker.CreateInstance<BoltOn.Mediator.Pipeline.Mediator>();
			var request = new TestRequest();
			testHandler.Setup(s => s.Handle(request)).Returns(new StandardBooleanResponse { IsSuccessful = true });

			// act
			var result = sut.Get(request);

			// assert 
			Assert.True(result.IsSuccessful);
			Assert.True(result.Data.IsSuccessful);
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
			serviceProvider.Setup(s => s.GetService(typeof(IRequestHandler<TestRequest, StandardBooleanResponse>)))
						  .Returns(testHandler.Object);
			autoMocker.Use<IEnumerable<IMediatorMiddleware>>(new List<IMediatorMiddleware>() { new TestMiddleware(logger.Object) });
			var sut = autoMocker.CreateInstance<BoltOn.Mediator.Pipeline.Mediator>();
			var request = new TestRequest();
			testHandler.Setup(s => s.Handle(request)).Returns(new StandardBooleanResponse { IsSuccessful = true });

			// act
			var result = sut.Get(request);

			// assert 
			Assert.True(result.IsSuccessful);
			Assert.True(result.Data.IsSuccessful);
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
			var boltOnClock = new Mock<IBoltOnClock>();
			var currentDateTime = DateTime.Now;
			boltOnClock.Setup(s => s.Now).Returns(currentDateTime);
			serviceProvider.Setup(s => s.GetService(typeof(IRequestHandler<TestRequest, StandardBooleanResponse>)))
				.Returns(testHandler.Object);
			autoMocker.Use<IEnumerable<IMediatorMiddleware>>(new List<IMediatorMiddleware> { new TestRequestSpecificMiddleware(logger.Object),
				new StopwatchMiddleware(logger2.Object, boltOnClock.Object) });
			var sut = autoMocker.CreateInstance<BoltOn.Mediator.Pipeline.Mediator>();
			var request = new TestRequest();
			testHandler.Setup(s => s.Handle(request)).Returns(new StandardBooleanResponse { IsSuccessful = true });

			// act
			var result = sut.Get(request);

			// assert 
			Assert.True(result.IsSuccessful);
			Assert.True(result.Data.IsSuccessful);
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
			serviceProvider.Setup(s => s.GetService(typeof(IRequestHandler<TestCommand, StandardBooleanResponse>)))
				.Returns(testHandler.Object);
			var uowProvider = autoMocker.GetMock<IUnitOfWorkProvider>();
			var uow = new Mock<IUnitOfWork>();
			uowProvider.Setup(u => u.Get(It.IsAny<UnitOfWorkOptions>())).Returns(uow.Object);
			var uowOptions = autoMocker.GetMock<UnitOfWorkOptions>();
			uowOptions.Setup(u => u.IsolationLevel).Returns(IsolationLevel.ReadCommitted);
			var uowOptionsBuilder = autoMocker.GetMock<IUnitOfWorkOptionsBuilder>();
			var request = new TestCommand();
			uowOptionsBuilder.Setup(u => u.Build(request)).Returns(uowOptions.Object);
			autoMocker.Use<IEnumerable<IMediatorMiddleware>>(new List<IMediatorMiddleware>
			{
				new UnitOfWorkMiddleware(logger.Object, uowProvider.Object, uowOptionsBuilder.Object)
			});
			var sut = autoMocker.CreateInstance<BoltOn.Mediator.Pipeline.Mediator>();
			testHandler.Setup(s => s.Handle(request)).Returns(new StandardBooleanResponse { IsSuccessful = true });

			// act
			var result = sut.Get(request);

			// assert 
			Assert.True(result.IsSuccessful);
			Assert.True(result.Data.IsSuccessful);
			uowProvider.Verify(u => u.Get(uowOptions.Object));
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
			serviceProvider.Setup(s => s.GetService(typeof(IRequestHandler<TestRequest, StandardBooleanResponse>)))
						   .Returns(testHandler.Object);
			autoMocker.Use<IEnumerable<IMediatorMiddleware>>(new List<IMediatorMiddleware>());
			var sut = autoMocker.CreateInstance<BoltOn.Mediator.Pipeline.Mediator>();
			var request = new TestRequest();
			testHandler.Setup(s => s.Handle(request)).Throws(new Exception("handler failed"));

			// act
			var result = sut.Get(request);

			// assert 
			Assert.False(result.IsSuccessful);
			Assert.Null(result.Data);
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
			serviceProvider.Setup(s => s.GetService(typeof(IRequestHandler<TestRequest, StandardBooleanResponse>)))
						  .Returns(null);
			autoMocker.Use<IEnumerable<IMediatorMiddleware>>(new List<IMediatorMiddleware>());
			var sut = autoMocker.CreateInstance<BoltOn.Mediator.Pipeline.Mediator>();
			var request = new TestRequest();

			// act
			var result = sut.Get(request);

			// assert 
			Assert.False(result.IsSuccessful);
			Assert.Null(result.Data);
			Assert.NotNull(result.Exception);
			Assert.Equal(string.Format(Constants.ExceptionMessages.
									   HANDLER_NOT_FOUND, request), result.Exception.Message);
		}

		[Fact, Trait("Category", "Integration")]
		public void Get_BootstrapWithDefaults_InvokesAllTheMiddlewaresAndReturnsSuccessfulResult()
		{
			// arrange
			MediatorTestHelper.IsClearMiddlewares = false;
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddLogging();
			serviceCollection.BoltOn();
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.UseBoltOn();
			var boltOnClock = serviceProvider.GetService<IBoltOnClock>();
			var mediator = serviceProvider.GetService<IMediator>();

			// act
			var result = mediator.Get(new TestRequest());

			// assert 
			Assert.True(result.IsSuccessful);
			Assert.True(result.Data.IsSuccessful);
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(d => d ==
																				   $"StopwatchMiddleware started at {boltOnClock.Now}"));
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(d => d ==
																				   $"StopwatchMiddleware ended at {boltOnClock.Now}. Time elapsed: 0"));
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(d => d == "TestMiddleware Started"));
		}

		[Fact, Trait("Category", "Integration")]
		public void Get_BootstrapWithCustomMiddlewares_InvokesDefaultAndCustomMiddlewareInOrderAndReturnsSuccessfulResult()
		{
			// arrange
			MediatorTestHelper.IsClearMiddlewares = false;
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddLogging();
			serviceCollection.BoltOn();
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.UseBoltOn();
			var boltOnClock = serviceProvider.GetService<IBoltOnClock>();
			var mediator = serviceProvider.GetService<IMediator>();

			// act
			var result = mediator.Get(new TestRequest());

			// assert 
			Assert.True(result.IsSuccessful);
			Assert.True(result.Data.IsSuccessful);
			Assert.True(MediatorTestHelper.LoggerStatements.IndexOf("TestMiddleware Started") > 0);
			Assert.True(MediatorTestHelper.LoggerStatements.IndexOf("TestMiddleware Ended") > 0);
			Assert.True(MediatorTestHelper.LoggerStatements.IndexOf("TestRequestSpecificMiddleware Started") == -1);
			Assert.True(MediatorTestHelper.LoggerStatements.IndexOf($"StopwatchMiddleware started at {boltOnClock.Now}") <
						MediatorTestHelper.LoggerStatements.IndexOf("TestMiddleware Started"));
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(d => d == $"StopwatchMiddleware started at {boltOnClock.Now}"));
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(d => d == $"StopwatchMiddleware ended at {boltOnClock.Now}. " +
																				   "Time elapsed: 0"));
			Assert.True(MediatorTestHelper.LoggerStatements.IndexOf($"StopwatchMiddleware ended at {boltOnClock.Now}. Time elapsed: 0") >
						MediatorTestHelper.LoggerStatements.IndexOf("TestMiddleware Ended"));
		}

		[Fact, Trait("Category", "Integration")]
		public void Get_BootstrapWithCustomMiddlewaresAndClear_InvokesOnlyCustomMiddlewareAndReturnsSuccessfulResult()
		{
			// arrange
			MediatorTestHelper.IsClearMiddlewares = true;
			var serviceCollection = new ServiceCollection();
			serviceCollection.BoltOn();
			serviceCollection.AddLogging();
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.UseBoltOn();
			var boltOnClock = serviceProvider.GetService<IBoltOnClock>();
			var sut = serviceProvider.GetService<IMediator>();
			var testMiddleware = serviceProvider.GetService<TestMiddleware>();

			// act
			var result = sut.Get(new TestRequest());

			// assert 
			Assert.True(result.IsSuccessful);
			Assert.True(result.Data.IsSuccessful);
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(f => f == "TestMiddleware Started"));
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(f => f == "TestMiddleware Ended"));
			Assert.Null(MediatorTestHelper.LoggerStatements.FirstOrDefault(d => d == $"StopwatchMiddleware started at {boltOnClock.Now}"));
			Assert.Null(MediatorTestHelper.LoggerStatements.FirstOrDefault(d => d == $"StopwatchMiddleware ended at {boltOnClock.Now}. " +
																				"Time elapsed: 0"));
		}

		[Fact, Trait("Category", "Integration")]
		public void Get_MediatorWithQueryRequest_ExecutesUoWMiddlewareAndStartsTransactionsWithDefaultQueryIsolationLevel()
		{
			// arrange
			MediatorTestHelper.IsCustomizeIsolationLevel = false;
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
			Assert.True(result.Data.IsSuccessful);
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(f => f == "Getting isolation level for Query"));
		}

		[Fact, Trait("Category", "Integration")]
		public void Get_MediatorWithQueryRequest_ExecutesUoWMiddlewareAndStartsTransactionsWithCustomizedQueryIsolationLevel()
		{
			// arrange
			MediatorTestHelper.IsCustomizeIsolationLevel = true;
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
			Assert.True(result.Data.IsSuccessful);
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(f => f == "Getting isolation level for Command or Query"));
		}

		public void Dispose()
		{
			MediatorTestHelper.LoggerStatements.Clear();
			Bootstrapper
				.Instance
				.Dispose();
		}
	}

	public class Test2BootstrapperRegistrationTask : IBootstrapperRegistrationTask
	{
		public void Run(RegistrationTaskContext context)
		{
			var boltOnClock = new Mock<IBoltOnClock>();
			var currentDateTime = DateTime.Parse("10/27/2018 12:51:59 PM");
			boltOnClock.Setup(s => s.Now).Returns(currentDateTime);
			context.Container.AddTransient((s) => boltOnClock.Object);

			var testMiddlewareLogger = new Mock<IBoltOnLogger<TestMiddleware>>();
			testMiddlewareLogger.Setup(s => s.Debug(It.IsAny<string>()))
								.Callback<string>(st => MediatorTestHelper.LoggerStatements.Add(st));
			context.Container.AddTransient((s) => testMiddlewareLogger.Object);

			var stopWatchMiddlewareLogger = new Mock<IBoltOnLogger<StopwatchMiddleware>>();
			stopWatchMiddlewareLogger.Setup(s => s.Debug(It.IsAny<string>()))
									 .Callback<string>(st => MediatorTestHelper.LoggerStatements.Add(st));
			context.Container.AddTransient((s) => stopWatchMiddlewareLogger.Object);

			var customUoWOptionsBuilder = new Mock<IBoltOnLogger<CustomUnitOfWorkOptionsBuilder>>();
			customUoWOptionsBuilder.Setup(s => s.Debug(It.IsAny<string>()))
								.Callback<string>(st => MediatorTestHelper.LoggerStatements.Add(st));
			context.Container.AddTransient((s) => customUoWOptionsBuilder.Object);

			var uowOptionsBuilderLogger = new Mock<IBoltOnLogger<UnitOfWorkOptionsBuilder>>();
			uowOptionsBuilderLogger.Setup(s => s.Debug(It.IsAny<string>()))
								.Callback<string>(st => MediatorTestHelper.LoggerStatements.Add(st));
			context.Container.AddTransient((s) => uowOptionsBuilderLogger.Object);

			if (MediatorTestHelper.IsClearMiddlewares)
				context.Container.RemoveAllMiddlewares();

			if (MediatorTestHelper.IsCustomizeIsolationLevel)
				context.Container.AddSingleton<IUnitOfWorkOptionsBuilder, CustomUnitOfWorkOptionsBuilder>();

			context.Container.AddMiddleware<TestMiddleware>();
		}
	}

	public class TestPreRegistrationTask : IBootstrapperPreRegistrationTask
	{
		public void Run(PreRegistrationTaskContext context)
		{
		}
	}

	public class TestRequest : IRequest<StandardBooleanResponse>, IEnableStopwatchMiddleware
	{
	}

	public class TestHandler : IRequestHandler<TestRequest, StandardBooleanResponse>,
	IRequestHandler<TestCommand, StandardBooleanResponse>,
	IRequestHandler<TestQuery, StandardBooleanResponse>
	{
		public virtual StandardBooleanResponse Handle(TestRequest request)
		{
			return new StandardBooleanResponse { IsSuccessful = true };
		}

		public virtual StandardBooleanResponse Handle(TestCommand request)
		{
			return new StandardBooleanResponse { IsSuccessful = true };
		}

		public virtual StandardBooleanResponse Handle(TestQuery request)
		{
			return new StandardBooleanResponse { IsSuccessful = true };
		}
	}

	public class TestCommand : ICommand<StandardBooleanResponse>
	{
	}

	public class TestQuery : IQuery<StandardBooleanResponse>
	{
	}

	public class TestMiddleware : IMediatorMiddleware
	{
		private readonly IBoltOnLogger<TestMiddleware> _logger;

		public TestMiddleware(IBoltOnLogger<TestMiddleware> logger)
		{
			_logger = logger;
		}

		public MediatorResponse<TResponse> Run<TRequest, TResponse>(IRequest<TResponse> request,
																	 Func<IRequest<TResponse>, MediatorResponse<TResponse>> next)
		   where TRequest : IRequest<TResponse>
		 	where TResponse : class
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

		public override MediatorResponse<TResponse> Execute<TRequest, TResponse>(IRequest<TResponse> request,
																				   Func<IRequest<TResponse>, MediatorResponse<TResponse>> next)
		{
			_logger.Debug($"TestRequestSpecificMiddleware Started");
			var response = next.Invoke(request);
			_logger.Debug($"TestRequestSpecificMiddleware Ended");
			return response;
		}
	}

	public class CustomUnitOfWorkOptionsBuilder : IUnitOfWorkOptionsBuilder
	{
		private readonly IBoltOnLogger<CustomUnitOfWorkOptionsBuilder> _logger;

		public CustomUnitOfWorkOptionsBuilder(IBoltOnLogger<CustomUnitOfWorkOptionsBuilder> logger)
		{
			_logger = logger;
		}

		public UnitOfWorkOptions Build<TResponse>(IRequest<TResponse> request) where TResponse : class
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

	public class MediatorTestHelper
	{
		public static List<string> LoggerStatements { get; set; } = new List<string>();
		public static bool IsClearMiddlewares { get; set; }
		public static bool IsCustomizeIsolationLevel { get; set; }
	}
}
