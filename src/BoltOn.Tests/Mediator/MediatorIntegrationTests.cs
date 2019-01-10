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
using Xunit;

namespace BoltOn.Tests.Mediator
{
	[Collection("IntegrationTests")]
	public class MediatorIntegrationTests : IDisposable
	{
		public MediatorIntegrationTests()
		{
			Bootstrapper
				.Instance
				.Dispose();
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
			Assert.True(result.Data);
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
			Assert.True(result.Data);
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
			Assert.True(result.Data);
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
			Assert.True(result.Data);
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
			Assert.True(result.Data);
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

	public class TestMediatorRegistrationTask : IBootstrapperRegistrationTask
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

	public class TestRequest : IRequest<bool>, IEnableStopwatchMiddleware
	{
	}

	public class TestHandler : IRequestHandler<TestRequest, bool>,
	IRequestHandler<TestCommand, bool>,
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

		public MediatorResponse<TResponse> Run<TRequest, TResponse>(IRequest<TResponse> request,
																	 Func<IRequest<TResponse>, MediatorResponse<TResponse>> next)
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

		public RequestType RequestType { get; private set; }

		public UnitOfWorkOptions Build<TResponse>(IRequest<TResponse> request)
		{
			IsolationLevel isolationLevel;
			switch (request)
			{
				case ICommand<TResponse> c:
				case IQuery<TResponse> q:
					_logger.Debug("Getting isolation level for Command or Query");
					isolationLevel = IsolationLevel.ReadCommitted;
					RequestType = RequestType.Command;
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
