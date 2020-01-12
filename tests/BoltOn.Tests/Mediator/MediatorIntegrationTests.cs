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
		public async Task Process_BootstrapAndRemoveAllInterceptorsAndAddTestInterceptorAfterRemove_InvokesOnlyTestInterceptorAndReturnsSuccessfulResult()
		{
			// arrange
			if (!MediatorTestHelper.IsIntegrationTestsEnabled)
				return;
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

		[Fact, TestPriority(10)]
		public async Task Process_BootstrapWithRemoveInterceptor_DoesNotInvokeRemovedInterceptorAndReturnsSuccessfulResult()
		{
			// arrange
			if (!MediatorTestHelper.IsIntegrationTestsEnabled)
				return;
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

		[Fact, TestPriority(15)]
		public async Task Process_MediatorWithQueryUncommittedRequest_ExecutesCustomChangeTrackerInterceptor()
		{
			// arrange
			if (!MediatorTestHelper.IsIntegrationTestsEnabled)
				return;
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

		[Fact, TestPriority(20)]
		public async Task Process_MediatorWithQueryRequestAndAsyncHandler_StartsTransactionsWithCustomizedQueryIsolationLevel()
		{
			// arrange
			if (!MediatorTestHelper.IsIntegrationTestsEnabled)
				return;
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

		[Fact, TestPriority(25)]
		public async Task Process_MediatorWithCommandRequest_ExecutesChangeTrackerContextInterceptorAndEnablesTrackAll()
		{
			// arrange
			if (!MediatorTestHelper.IsIntegrationTestsEnabled)
				return;
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
