using System;
using System.Linq;
using BoltOn.Bootstrapping;
using BoltOn.Data.EF;
using BoltOn.Mediator.Data.EF;
using BoltOn.Mediator.Pipeline;
using BoltOn.Tests.Data.EF;
using BoltOn.Tests.Other;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BoltOn.Tests.Mediator.Data.EF
{
	[Collection("IntegrationTests")]
	public class MediatorDataEFIntegrationTests : IDisposable
	{
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
						.BoltOnEFModule()
						.BoltOnMediatorEFModule();
				})
				.AddLogging();
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var sut = serviceProvider.GetService<IMediator>();

			// act
			var result = sut.Process(new GetStudent { StudentId = 2 });
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
						.BoltOnEFModule()
						.BoltOnMediatorEFModule();
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
