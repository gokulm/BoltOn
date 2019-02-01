using System;
using System.Linq;
using BoltOn.Bootstrapping;
using BoltOn.Mediator.Pipeline;
using BoltOn.Utilities;
using Microsoft.Extensions.DependencyInjection;
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

		[Fact]
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

		[Fact]
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

		[Fact]
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

		[Fact]
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

		[Fact]
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
}
