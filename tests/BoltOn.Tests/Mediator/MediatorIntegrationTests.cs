using System;
using System.Linq;
using System.Threading.Tasks;
using BoltOn.Bootstrapping;
using BoltOn.Data.EF;
using BoltOn.Mediator.Pipeline;
using BoltOn.Overrides.Mediator;
using BoltOn.Tests.Common;
using BoltOn.Tests.Mediator.Fakes;
using BoltOn.Tests.Other;
using BoltOn.Tests.UoW;
using BoltOn.UoW;
using BoltOn.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BoltOn.Tests.Mediator
{
	[TestCaseOrderer("BoltOn.Tests.Common.PriorityOrderer", "BoltOn.Tests")]
	[Collection("IntegrationTests")]
	public class MediatorIntegrationTests : IDisposable
	{
		public MediatorIntegrationTests()
		{
			MediatorTestHelper.IsRemoveStopwatchInterceptor = false;
			MediatorTestHelper.IsClearInterceptors = false;
			MediatorTestHelper.IsCustomizeIsolationLevel = false;
			MediatorTestHelper.LoggerStatements.Clear();
		}

		[Fact, TestPriority(5)]
		public async Task Process_BootstrapWithTestInterceptorsAndRemoveAll_InvokesOnlyTestInterceptorAndReturnsSuccessfulResult()
		{
			// arrange
			MediatorTestHelper.IsClearInterceptors = true;
			var serviceCollection = new ServiceCollection();
			serviceCollection.BoltOn();
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var boltOnClock = serviceProvider.GetService<IBoltOnClock>();
			var sut = serviceProvider.GetService<IMediator>();

			// act
			var result = await sut.ProcessAsync(new TestRequest());

			// assert 
			Assert.True(result);
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(f => f == "TestInterceptor Started"));
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(f => f == "TestInterceptor Ended"));
			Assert.Null(MediatorTestHelper.LoggerStatements.FirstOrDefault(d => d == $"StopwatchInterceptor started at {boltOnClock.Now}"));
			Assert.Null(MediatorTestHelper.LoggerStatements.FirstOrDefault(d => d == $"StopwatchInterceptor ended at {boltOnClock.Now}. " +
																				"Time elapsed: 0"));
		}

		[Fact, TestPriority(85)]
		public async Task Process_BootstrapWithRemoveInterceptor_DoesNotInvokeRemovedInterceptorAndReturnsSuccessfulResult()
		{
			// arrange
			MediatorTestHelper.IsRemoveStopwatchInterceptor = true;
			var serviceCollection = new ServiceCollection();
			serviceCollection.BoltOn();
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var boltOnClock = serviceProvider.GetService<IBoltOnClock>();
			var sut = serviceProvider.GetService<IMediator>();

			// act
			var result = await sut.ProcessAsync(new TestRequest());

			// assert 
			Assert.True(result);
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(f => f == "TestInterceptor Started"));
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(f => f == "TestInterceptor Ended"));
			Assert.Null(MediatorTestHelper.LoggerStatements.FirstOrDefault(d => d == $"StopwatchInterceptor started at {boltOnClock.Now}"));
			Assert.Null(MediatorTestHelper.LoggerStatements.FirstOrDefault(d => d == $"StopwatchInterceptor ended at {boltOnClock.Now}. " +
																				"Time elapsed: 0"));
		}

		[Fact, TestPriority(90)]
		public async Task Process_MediatorWithQueryRequest_StartsTransactionsWithDefaultQueryIsolationLevel()
		{
			// arrange
			MediatorTestHelper.IsCustomizeIsolationLevel = false;
			var serviceCollection = new ServiceCollection();
			serviceCollection
				.BoltOn();
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var sut = serviceProvider.GetService<IMediator>();

			// act
			var result = await sut.ProcessAsync(new TestQuery());

			// assert 
			Assert.True(result);
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(f => f == "Getting isolation level for Query"));
		}

		[Fact, TestPriority(100)]
		public async Task Process_MediatorWithQueryUncommittedRequest_ExecutesCustomChangeTrackerInterceptor()
		{
			// arrange
			MediatorTestHelper.IsCustomizeIsolationLevel = false;
			var serviceCollection = new ServiceCollection();
			serviceCollection.BoltOn(options => options.BoltOnEFModule());
			serviceCollection.AddSingleton<IUnitOfWorkOptionsBuilder, TestCustomUnitOfWorkOptionsBuilder>();
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var sut = serviceProvider.GetService<IMediator>();

			// act
			var result = await sut.ProcessAsync(new TestStaleQuery());

			// assert 
			Assert.True(result);
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(f => f == "Getting isolation level for QueryUncommitted"));
		}

		[Fact, TestPriority(11)]
		public async Task Process_MediatorWithQueryRequest_StartsTransactionsWithCustomizedQueryIsolationLevel()
		{
			// arrange
			MediatorTestHelper.IsCustomizeIsolationLevel = true;
			var serviceCollection = new ServiceCollection();
			serviceCollection.BoltOn(o => o.BoltOnEFModule());
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var sut = serviceProvider.GetService<IMediator>();

			// act
			var result = await sut.ProcessAsync(new TestQuery());

			// assert 
			Assert.True(result);
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(f => f == $"Entering {nameof(CustomChangeTrackerInterceptor)}..."));
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(f => f == "Getting isolation level for Command or Query"));
		}

		[Fact, TestPriority(12)]
		public async Task Process_MediatorWithQueryRequestAndAsyncHandler_StartsTransactionsWithCustomizedQueryIsolationLevel()
		{
			// arrange
			MediatorTestHelper.IsCustomizeIsolationLevel = true;
			var serviceCollection = new ServiceCollection();
			serviceCollection.BoltOn(o => o.BoltOnEFModule());
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var sut = serviceProvider.GetService<IMediator>();

			// act
			var result = await sut.ProcessAsync(new TestQuery());

			// assert 
			Assert.True(result); 
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(f => f == $"Entering {nameof(CustomChangeTrackerInterceptor)}..."));
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(f => f == "Getting isolation level for Command or Query"));
		}

		[Fact, TestPriority(13)]
		public async Task Process_MediatorWithOneWayCommandRequestAndAsyncHandler_StartsTransactionsWithCustomizedQueryIsolationLevel()
		{
			// arrange
			MediatorTestHelper.IsCustomizeIsolationLevel = true;
			var serviceCollection = new ServiceCollection();
			serviceCollection.BoltOn(o => o.BoltOnEFModule());
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var sut = serviceProvider.GetService<IMediator>();
			var command = new TestOneWayCommand();

			// act
			await sut.ProcessAsync(command);

			// assert 
			Assert.Equal(1, command.Value); 
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(f => f == $"Entering {nameof(CustomChangeTrackerInterceptor)}..."));
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(f => f == "Getting isolation level for Command or Query"));
		}


		[Fact, TestPriority(14)]
		public async Task Process_MediatorWithQueryRequest_ExecutesChangeTrackerContextInterceptorAndDisablesTracking()
		{
			// arrange
			IntegrationTestHelper.IsSeedData = true;
			var serviceCollection = new ServiceCollection();
			serviceCollection.BoltOn(options => options.BoltOnEFModule());
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var sut = serviceProvider.GetService<IMediator>();

			// act
			var result = await sut.ProcessAsync(new GetStudentRequest { StudentId = 2 });
			var dbContext = serviceProvider.GetService<IDbContextFactory>().Get<SchoolDbContext>();
			var isAutoDetectChangesEnabled = dbContext.ChangeTracker.AutoDetectChangesEnabled;
			var queryTrackingBehavior = dbContext.ChangeTracker.QueryTrackingBehavior;

			// assert 
			Assert.NotNull(result);
			Assert.Equal(Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking, queryTrackingBehavior);
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(f => f == $"Entering {nameof(ChangeTrackerInterceptor)}..."));
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(f => f == $"IsQueryRequest: {true}"));
			Assert.False(isAutoDetectChangesEnabled);
		}

		[Fact, TestPriority(120)]
		public async Task Process_MediatorWithCommandRequest_ExecutesChangeTrackerContextInterceptorAndEnablesTrackAll()
		{
			// arrange
			var serviceCollection = new ServiceCollection();
			serviceCollection.BoltOn(options => options.BoltOnEFModule());
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var sut = serviceProvider.GetService<IMediator>();

			// act
			var result = await sut.ProcessAsync(new TestCommand());
			var dbContext = serviceProvider.GetService<IDbContextFactory>().Get<SchoolDbContext>();
			var isAutoDetectChangesEnabled = dbContext.ChangeTracker.AutoDetectChangesEnabled;
			var queryTrackingBehavior = dbContext.ChangeTracker.QueryTrackingBehavior;

			// assert 
			Assert.True(result);
			Assert.Equal(Microsoft.EntityFrameworkCore.QueryTrackingBehavior.TrackAll, queryTrackingBehavior);
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(f => f == $"Entering {nameof(ChangeTrackerInterceptor)}..."));
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(f => f == $"IsQueryRequest: {false}"));
			Assert.True(isAutoDetectChangesEnabled);
		}

		public void Dispose()
		{
			Bootstrapper
				.Instance
				.Dispose();
		}
	}
}
