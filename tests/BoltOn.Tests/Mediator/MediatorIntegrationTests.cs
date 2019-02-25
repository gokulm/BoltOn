using System;
using System.Linq;
using System.Threading.Tasks;
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
		public void Get_BootstrapWithDefaults_InvokesAllTheInterceptorsAndReturnsSuccessfulResult()
		{
			// arrange
			MediatorTestHelper.IsClearInterceptors = false;
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
																				   $"StopwatchInterceptor started at {boltOnClock.Now}"));
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(d => d ==
																				   $"StopwatchInterceptor ended at {boltOnClock.Now}. Time elapsed: 0"));
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(d => d == "TestInterceptor Started"));
		}

		[Fact]
		public async Task Get_BootstrapWithDefaultsAndAsyncHandler_InvokesAllTheInterceptorsAndReturnsSuccessfulResult()
		{
			// arrange
			MediatorTestHelper.IsClearInterceptors = false;
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddLogging();
			serviceCollection.BoltOn();
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.UseBoltOn();
			var boltOnClock = serviceProvider.GetService<IBoltOnClock>();
			var mediator = serviceProvider.GetService<IMediator>();

			// act
			var result = await mediator.GetAsync(new TestRequest());

			// assert 
			Assert.True(result.IsSuccessful);
			Assert.True(result.Data);
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(d => d ==
																				   $"StopwatchInterceptor started at {boltOnClock.Now}"));
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(d => d ==
																				   $"StopwatchInterceptor ended at {boltOnClock.Now}. Time elapsed: 0"));
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(d => d == "TestInterceptor Started"));
		}

		[Fact]
		public void Get_BootstrapWithCustomInterceptors_InvokesDefaultAndCustomInterceptorInOrderAndReturnsSuccessfulResult()
		{
			// arrange
			MediatorTestHelper.IsClearInterceptors = false;
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
			Assert.True(MediatorTestHelper.LoggerStatements.IndexOf("TestInterceptor Started") > 0);
			Assert.True(MediatorTestHelper.LoggerStatements.IndexOf("TestInterceptor Ended") > 0);
			Assert.True(MediatorTestHelper.LoggerStatements.IndexOf("TestRequestSpecificInterceptor Started") == -1);
			Assert.True(MediatorTestHelper.LoggerStatements.IndexOf($"StopwatchInterceptor started at {boltOnClock.Now}") <
						MediatorTestHelper.LoggerStatements.IndexOf("TestInterceptor Started"));
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(d => d == $"StopwatchInterceptor started at {boltOnClock.Now}"));
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(d => d == $"StopwatchInterceptor ended at {boltOnClock.Now}. " +
																				   "Time elapsed: 0"));
			Assert.True(MediatorTestHelper.LoggerStatements.IndexOf($"StopwatchInterceptor ended at {boltOnClock.Now}. Time elapsed: 0") >
						MediatorTestHelper.LoggerStatements.IndexOf("TestInterceptor Ended"));
		}

		[Fact]
		public void Get_BootstrapWithCustomInterceptorsAndClear_InvokesOnlyCustomInterceptorAndReturnsSuccessfulResult()
		{
			// arrange
			MediatorTestHelper.IsClearInterceptors = true;
			var serviceCollection = new ServiceCollection();
			serviceCollection.BoltOn();
			serviceCollection.AddLogging();
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.UseBoltOn();
			var boltOnClock = serviceProvider.GetService<IBoltOnClock>();
			var sut = serviceProvider.GetService<IMediator>();
			var testInterceptor = serviceProvider.GetService<TestInterceptor>();

			// act
			var result = sut.Get(new TestRequest());

			// assert 
			Assert.True(result.IsSuccessful);
			Assert.True(result.Data);
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(f => f == "TestInterceptor Started"));
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(f => f == "TestInterceptor Ended"));
			Assert.Null(MediatorTestHelper.LoggerStatements.FirstOrDefault(d => d == $"StopwatchInterceptor started at {boltOnClock.Now}"));
			Assert.Null(MediatorTestHelper.LoggerStatements.FirstOrDefault(d => d == $"StopwatchInterceptor ended at {boltOnClock.Now}. " +
																				"Time elapsed: 0"));
		}

		[Fact]
		public void Get_MediatorWithQueryRequest_ExecutesUoWInterceptorAndStartsTransactionsWithDefaultQueryIsolationLevel()
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
		public void Get_MediatorWithQueryRequest_ExecutesUoWInterceptorAndStartsTransactionsWithCustomizedQueryIsolationLevel()
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

		[Fact]
		public async Task Get_MediatorWithQueryRequestAndAsyncHandler_ExecutesUoWInterceptorAndStartsTransactionsWithCustomizedQueryIsolationLevel()
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
			var result = await sut.GetAsync(new TestQuery());

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
