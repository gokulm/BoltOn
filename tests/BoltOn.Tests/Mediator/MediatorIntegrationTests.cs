using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Bootstrapping;
using BoltOn.Data.EF;
using BoltOn.Mediator;
using BoltOn.Mediator.Interceptors;
using BoltOn.Mediator.Pipeline;
using BoltOn.Tests.Other;
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
		public void Process_BootstrapWithDefaults_InvokesAllTheInterceptorsAndReturnsSuccessfulResult()
		{
			// arrange
			MediatorTestHelper.IsClearInterceptors = false;
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddLogging();
			serviceCollection.BoltOn();
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var boltOnClock = serviceProvider.GetService<IBoltOnClock>();
			var mediator = serviceProvider.GetService<IMediator>();

			// act
			var result = mediator.Process(new TestRequest());

			// assert 
			Assert.True(result);
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(d => d ==
																				   $"StopwatchInterceptor started at {boltOnClock.Now}"));
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(d => d ==
																				   $"StopwatchInterceptor ended at {boltOnClock.Now}. Time elapsed: 0"));
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(d => d == "TestInterceptor Started"));
		}


		[Fact]
		public void Process_BootstrapWithDefaults_InvokesAllTheInterceptorsAndReturnsSuccessfulResultForOneWayRequest()
		{
			// arrange
			MediatorTestHelper.IsClearInterceptors = false;
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddLogging();
			serviceCollection.BoltOn();
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var boltOnClock = serviceProvider.GetService<IBoltOnClock>();
			var mediator = serviceProvider.GetService<IMediator>();
			var request = new TestOneWayRequest();

			// act
			mediator.Process(request);

			// assert 
			Assert.Equal(1, request.Value);
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(d => d ==
																				   $"StopwatchInterceptor started at {boltOnClock.Now}"));
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(d => d ==
																				   $"StopwatchInterceptor ended at {boltOnClock.Now}. Time elapsed: 0"));
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(d => d == "TestInterceptor Started"));
		}

		[Fact]
		public void Process_BootstrapWithDefaults_InvokesAllTheInterceptorsAndReturnsSuccessfulResultForOneWayCommand()
		{
			// arrange
			MediatorTestHelper.IsClearInterceptors = false;
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddLogging();
			serviceCollection.BoltOn();
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var boltOnClock = serviceProvider.GetService<IBoltOnClock>();
			var mediator = serviceProvider.GetService<IMediator>();
			var request = new TestOneWayCommand();

			// act
			mediator.Process(request);

			// assert 
			Assert.Equal(1, request.Value);
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(d => d ==
																				   $"StopwatchInterceptor started at {boltOnClock.Now}"));
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(d => d ==
																				   $"StopwatchInterceptor ended at {boltOnClock.Now}. Time elapsed: 0"));
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(d => d == "TestInterceptor Started"));
		}

		[Fact]
		public async Task Process_BootstrapWithDefaults_InvokesAllTheInterceptorsAndReturnsSuccessfulResultForOneWayAsyncRequest()
		{
			// arrange
			MediatorTestHelper.IsClearInterceptors = false;
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddLogging();
			serviceCollection.BoltOn();
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var boltOnClock = serviceProvider.GetService<IBoltOnClock>();
			var mediator = serviceProvider.GetService<IMediator>();
			var request = new TestOneWayRequest();

			// act
			await mediator.ProcessAsync(request);

			// assert 
			Assert.Equal(1, request.Value);
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(d => d ==
																				   $"StopwatchInterceptor started at {boltOnClock.Now}"));
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(d => d ==
																				   $"StopwatchInterceptor ended at {boltOnClock.Now}. Time elapsed: 0"));
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(d => d == "TestInterceptor Started"));
		}

		[Fact]
		public async Task Process_BootstrapWithDefaultsAndAsyncHandler_InvokesAllTheInterceptorsAndReturnsSuccessfulResult()
		{
			// arrange
			MediatorTestHelper.IsClearInterceptors = false;
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddLogging();
			serviceCollection.BoltOn();
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var boltOnClock = serviceProvider.GetService<IBoltOnClock>();
			var mediator = serviceProvider.GetService<IMediator>();

			// act
			CancellationTokenSource cts = new CancellationTokenSource();
			cts.Cancel();
			CancellationToken token = cts.Token;
			var result = await mediator.ProcessAsync(new TestRequest(), token);

			// assert 
			Assert.True(result);
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(d => d ==
																				   $"StopwatchInterceptor started at {boltOnClock.Now}"));
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(d => d ==
																				   $"StopwatchInterceptor ended at {boltOnClock.Now}. Time elapsed: 0"));
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(d => d == "TestInterceptor Started"));
		}

		[Fact]
		public void Process_BootstrapWithCustomInterceptors_InvokesDefaultAndCustomInterceptorInOrderAndReturnsSuccessfulResult()
		{
			// arrange
			MediatorTestHelper.IsClearInterceptors = false;
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddLogging();
			serviceCollection.BoltOn();
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var boltOnClock = serviceProvider.GetService<IBoltOnClock>();
			var mediator = serviceProvider.GetService<IMediator>();

			// act
			var result = mediator.Process(new TestRequest());

			// assert 
			Assert.True(result);
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
		public async Task Process_BootstrapWithCustomInterceptorsAndAsyncHandler_InvokesDefaultAndCustomInterceptorInOrderAndReturnsSuccessfulResult()
		{
			// arrange
			MediatorTestHelper.IsClearInterceptors = false;
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddLogging();
			serviceCollection.BoltOn();
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var boltOnClock = serviceProvider.GetService<IBoltOnClock>();
			var mediator = serviceProvider.GetService<IMediator>();

			// act
			var result = await mediator.ProcessAsync(new TestRequest());

			// assert 
			Assert.True(result);
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
		public void Process_BootstrapWithCustomInterceptorsAndClear_InvokesOnlyCustomInterceptorAndReturnsSuccessfulResult()
		{
			// arrange
			MediatorTestHelper.IsClearInterceptors = true;
			var serviceCollection = new ServiceCollection();
			serviceCollection.BoltOn();
			serviceCollection.AddLogging();
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var boltOnClock = serviceProvider.GetService<IBoltOnClock>();
			var sut = serviceProvider.GetService<IMediator>();
			var testInterceptor = serviceProvider.GetService<TestInterceptor>();

			// act
			var result = sut.Process(new TestRequest());

			// assert 
			Assert.True(result);
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(f => f == "TestInterceptor Started"));
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(f => f == "TestInterceptor Ended"));
			Assert.Null(MediatorTestHelper.LoggerStatements.FirstOrDefault(d => d == $"StopwatchInterceptor started at {boltOnClock.Now}"));
			Assert.Null(MediatorTestHelper.LoggerStatements.FirstOrDefault(d => d == $"StopwatchInterceptor ended at {boltOnClock.Now}. " +
																				"Time elapsed: 0"));
		}

		[Fact]
		public void Process_BootstrapWithRemoveInterceptor_DoesNotInvokeRemovedInterceptorAndReturnsSuccessfulResult()
		{
			// arrange
			var serviceCollection = new ServiceCollection();
			serviceCollection.BoltOn();
			serviceCollection.AddLogging();
			serviceCollection.RemoveInterceptor<StopwatchInterceptor>();
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var boltOnClock = serviceProvider.GetService<IBoltOnClock>();
			var sut = serviceProvider.GetService<IMediator>();
			var testInterceptor = serviceProvider.GetService<TestInterceptor>();

			// act
			var result = sut.Process(new TestRequest());

			// assert 
			Assert.True(result);
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(f => f == "TestInterceptor Started"));
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(f => f == "TestInterceptor Ended"));
			Assert.Null(MediatorTestHelper.LoggerStatements.FirstOrDefault(d => d == $"StopwatchInterceptor started at {boltOnClock.Now}"));
			Assert.Null(MediatorTestHelper.LoggerStatements.FirstOrDefault(d => d == $"StopwatchInterceptor ended at {boltOnClock.Now}. " +
																				"Time elapsed: 0"));
		}

		[Fact]
		public void Process_MediatorWithQueryRequest_ExecutesUoWInterceptorAndStartsTransactionsWithDefaultQueryIsolationLevel()
		{
			// arrange
			MediatorTestHelper.IsCustomizeIsolationLevel = false;
			var serviceCollection = new ServiceCollection();
			serviceCollection
				.BoltOn()
				.AddLogging();
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var sut = serviceProvider.GetService<IMediator>();

			// act
			var result = sut.Process(new TestQuery());

			// assert 
			Assert.True(result);
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(f => f == "Getting isolation level for Query"));
		}

		[Fact]
		public void Process_MediatorWithStaleQueryRequest_ExecutesUoWInterceptorAndStartsTransactionsWithDefaultQueryIsolationLevel()
		{
			// arrange
			MediatorTestHelper.IsCustomizeIsolationLevel = false;
			var serviceCollection = new ServiceCollection();
			serviceCollection
				.BoltOn()
				.AddLogging();
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var sut = serviceProvider.GetService<IMediator>();

			// act
			var result = sut.Process(new TestStaleQuery());

			// assert 
			Assert.True(result);
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(f => f == "Getting isolation level for StaleQuery"));
		}

		[Fact]
		public void Process_MediatorWithQueryRequest_ExecutesUoWInterceptorAndStartsTransactionsWithCustomizedQueryIsolationLevel()
		{
			// arrange
			MediatorTestHelper.IsCustomizeIsolationLevel = true;
			var serviceCollection = new ServiceCollection();
			serviceCollection
				.BoltOn()
				.AddLogging();
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var sut = serviceProvider.GetService<IMediator>();

			// act
			var result = sut.Process(new TestQuery());

			// assert 
			Assert.True(result);
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(f => f == "Getting isolation level for Command or Query"));
		}

		[Fact]
		public async Task Process_MediatorWithQueryRequestAndAsyncHandler_ExecutesUoWInterceptorAndStartsTransactionsWithCustomizedQueryIsolationLevel()
		{
			// arrange
			MediatorTestHelper.IsCustomizeIsolationLevel = true;
			var serviceCollection = new ServiceCollection();
			serviceCollection
				.BoltOn()
				.AddLogging();
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var sut = serviceProvider.GetService<IMediator>();

			// act
			var result = await sut.ProcessAsync(new TestQuery());

			// assert 
			Assert.True(result);
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(f => f == "Getting isolation level for Command or Query"));
		}

		[Fact]
		public async Task Process_MediatorWithOneWayCommandRequestAndAsyncHandler_ExecutesUoWInterceptorAndStartsTransactionsWithCustomizedQueryIsolationLevel()
		{
			// arrange
			MediatorTestHelper.IsCustomizeIsolationLevel = true;
			var serviceCollection = new ServiceCollection();
			serviceCollection
				.BoltOn()
				.AddLogging();
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var sut = serviceProvider.GetService<IMediator>();
			var command = new TestOneWayCommand();

			// act
			await sut.ProcessAsync(command);

			// assert 
			Assert.Equal(1, command.Value);
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(f => f == "Getting isolation level for Command or Query"));
		}


		[Fact]
		public void Process_MediatorWithQueryRequest_ExecutesEFQueryTrackingBehaviorInterceptorAndDisablesTracking()
		{
			// arrange
			MediatorTestHelper.IsSeedData = true;
			var serviceCollection = new ServiceCollection();
			serviceCollection
				.BoltOn(options =>
				{
					options
						.BoltOnEFModule();
				})
				.AddLogging();
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var sut = serviceProvider.GetService<IMediator>();

			// act
			var result = sut.Process(new GetStudentRequest { StudentId = 2 });
			var dbContext = serviceProvider.GetService<IDbContextFactory>().Get<SchoolDbContext>();
			var student = dbContext.Set<Student>().Find(2);
			var isAutoDetectChangesEnabled = dbContext.ChangeTracker.AutoDetectChangesEnabled;
			var queryTrackingBehavior = dbContext.ChangeTracker.QueryTrackingBehavior;

			// assert 
			Assert.NotNull(result);
			Assert.Equal(Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking, queryTrackingBehavior);
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(f => f == $"Entering {nameof(EFQueryTrackingBehaviorInterceptor)}..."));
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(f => f == $"IsQueryRequest: {true}"));
			Assert.False(isAutoDetectChangesEnabled);
		}

		[Fact]
		public void Process_MediatorWithCommandRequest_ExecutesEFQueryTrackingBehaviorInterceptorAndEnablesTrackAll()
		{
			// arrange
			MediatorTestHelper.IsSeedData = false;
			var serviceCollection = new ServiceCollection();
			serviceCollection
				.BoltOn(options =>
				{
					options
						.BoltOnEFModule();
				})
				.AddLogging();
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var sut = serviceProvider.GetService<IMediator>();

			// act
			var result = sut.Process(new TestCommand());
			var dbContext = serviceProvider.GetService<IDbContextFactory>().Get<SchoolDbContext>();
			var isAutoDetectChangesEnabled = dbContext.ChangeTracker.AutoDetectChangesEnabled;
			var queryTrackingBehavior = dbContext.ChangeTracker.QueryTrackingBehavior;

			// assert 
			Assert.True(result);
			Assert.Equal(Microsoft.EntityFrameworkCore.QueryTrackingBehavior.TrackAll, queryTrackingBehavior);
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(f => f == $"Entering {nameof(EFQueryTrackingBehaviorInterceptor)}..."));
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(f => f == $"IsQueryRequest: {false}"));
			Assert.True(isAutoDetectChangesEnabled);
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
