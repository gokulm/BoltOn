using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Data.EF;
using BoltOn.Requestor.Pipeline;
using BoltOn.Tests.Data.EF;
using BoltOn.Tests.Other;
using BoltOn.Tests.Requestor.Fakes;
using BoltOn.Tests.UoW;
using BoltOn.UoW;
using BoltOn.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BoltOn.Tests.Requestor
{
	[Collection("IntegrationTests")]
	public class RequestorIntegrationTests : IDisposable
	{
		[Fact]
		public async Task Process_BootstrapWithDefaults_InvokesAllTheInterceptorsAndReturnsSuccessfulResult()
		{
			// arrange
			RequestorTestHelper.IsRemoveStopwatchInterceptor = false;
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddLogging();
			serviceCollection.BoltOn(b => b.RegisterRequestorFakes());
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var boltOnClock = serviceProvider.GetService<IBoltOnClock>();
			var requestor = serviceProvider.GetService<IRequestor>();

			// act
			var result = await requestor.ProcessAsync(new TestRequest());

			// assert 
			Assert.True(result);
			Assert.NotNull(RequestorTestHelper.LoggerStatements.FirstOrDefault(d => d ==
																				   $"StopwatchInterceptor started at {boltOnClock.Now}"));
			Assert.NotNull(RequestorTestHelper.LoggerStatements.FirstOrDefault(d => d ==
																				   $"StopwatchInterceptor ended at {boltOnClock.Now}. Time elapsed: 0"));
			Assert.NotNull(RequestorTestHelper.LoggerStatements.FirstOrDefault(d => d == "TestInterceptor Started"));
		}


		[Fact]
		public async Task Process_BootstrapWithDefaults_InvokesAllTheInterceptorsAndReturnsSuccessfulResultForOneWayRequest()
		{
			// arrange
			RequestorTestHelper.IsRemoveStopwatchInterceptor = false;
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddLogging();
			serviceCollection.BoltOn(b => b.RegisterRequestorFakes());
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var boltOnClock = serviceProvider.GetService<IBoltOnClock>();
			var requestor = serviceProvider.GetService<IRequestor>();
			var request = new TestOneWayRequest();

			// act
			await requestor.ProcessAsync(request);

			// assert 
			Assert.Equal(1, request.Value);
			Assert.NotNull(RequestorTestHelper.LoggerStatements.FirstOrDefault(d => d ==
																				   $"StopwatchInterceptor started at {boltOnClock.Now}"));
			Assert.NotNull(RequestorTestHelper.LoggerStatements.FirstOrDefault(d => d ==
																				   $"StopwatchInterceptor ended at {boltOnClock.Now}. Time elapsed: 0"));
			Assert.NotNull(RequestorTestHelper.LoggerStatements.FirstOrDefault(d => d == "TestInterceptor Started"));
		}

		[Fact]
		public async Task Process_BootstrapWithDefaults_InvokesAllTheInterceptorsAndReturnsSuccessfulResultForOneWayCommand()
		{
			// arrange
			RequestorTestHelper.IsRemoveStopwatchInterceptor = false;
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddLogging();
            serviceCollection.BoltOn(b => b.RegisterRequestorFakes());
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var boltOnClock = serviceProvider.GetService<IBoltOnClock>();
			var requestor = serviceProvider.GetService<IRequestor>();
			var request = new TestOneWayCommand();

			// act
			await requestor.ProcessAsync(request);

			// assert 
			Assert.Equal(1, request.Value);
			Assert.NotNull(RequestorTestHelper.LoggerStatements.FirstOrDefault(d => d ==
																				   $"StopwatchInterceptor started at {boltOnClock.Now}"));
			Assert.NotNull(RequestorTestHelper.LoggerStatements.FirstOrDefault(d => d ==
																				   $"StopwatchInterceptor ended at {boltOnClock.Now}. Time elapsed: 0"));
			Assert.NotNull(RequestorTestHelper.LoggerStatements.FirstOrDefault(d => d == "TestInterceptor Started"));
		}

		[Fact]
		public async Task Process_BootstrapWithDefaults_InvokesAllTheInterceptorsAndReturnsSuccessfulResultForOneWayAsyncRequest()
		{
			// arrange
			RequestorTestHelper.IsRemoveStopwatchInterceptor = false;
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddLogging();
            serviceCollection.BoltOn(b => b.RegisterRequestorFakes());
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var boltOnClock = serviceProvider.GetService<IBoltOnClock>();
			var requestor = serviceProvider.GetService<IRequestor>();
			var request = new TestOneWayRequest();

			// act
			await requestor.ProcessAsync(request);

			// assert 
			Assert.Equal(1, request.Value);
			Assert.NotNull(RequestorTestHelper.LoggerStatements.FirstOrDefault(d => d ==
																				   $"StopwatchInterceptor started at {boltOnClock.Now}"));
			Assert.NotNull(RequestorTestHelper.LoggerStatements.FirstOrDefault(d => d ==
																				   $"StopwatchInterceptor ended at {boltOnClock.Now}. Time elapsed: 0"));
			Assert.NotNull(RequestorTestHelper.LoggerStatements.FirstOrDefault(d => d == "TestInterceptor Started"));
		}

		[Fact]
		public async Task Process_BootstrapWithDefaultsAndAsyncHandler_InvokesAllTheInterceptorsAndReturnsSuccessfulResult()
		{
			// arrange
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddLogging();
            serviceCollection.BoltOn(b => b.RegisterRequestorFakes());
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var boltOnClock = serviceProvider.GetService<IBoltOnClock>();
			var requestor = serviceProvider.GetService<IRequestor>();

			// act
			CancellationTokenSource cts = new CancellationTokenSource();
			cts.Cancel();
			CancellationToken token = cts.Token;
			var result = await requestor.ProcessAsync(new TestRequest(), token);

			// assert 
			Assert.True(result);
			Assert.NotNull(RequestorTestHelper.LoggerStatements.FirstOrDefault(d => d ==
																				   $"StopwatchInterceptor started at {boltOnClock.Now}"));
			Assert.NotNull(RequestorTestHelper.LoggerStatements.FirstOrDefault(d => d ==
																				   $"StopwatchInterceptor ended at {boltOnClock.Now}. Time elapsed: 0"));
			Assert.NotNull(RequestorTestHelper.LoggerStatements.FirstOrDefault(d => d == "TestInterceptor Started"));
		}

		[Fact]
		public async Task Process_BootstrapWithTestInterceptors_InvokesDefaultAndTestInterceptorInOrderAndReturnsSuccessfulResult()
		{
			// arrange
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddLogging();
            serviceCollection.BoltOn(b => b.RegisterRequestorFakes());
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var boltOnClock = serviceProvider.GetService<IBoltOnClock>();
			var requestor = serviceProvider.GetService<IRequestor>();

			// act
			var result = await requestor.ProcessAsync(new TestRequest());

			// assert 
			Assert.True(result);
			Assert.True(RequestorTestHelper.LoggerStatements.IndexOf("TestInterceptor Started") > 0);
			Assert.True(RequestorTestHelper.LoggerStatements.IndexOf("TestInterceptor Ended") > 0);
			Assert.True(RequestorTestHelper.LoggerStatements.IndexOf("TestRequestSpecificInterceptor Started") == -1);
			Assert.True(RequestorTestHelper.LoggerStatements.IndexOf($"StopwatchInterceptor started at {boltOnClock.Now}") <
						RequestorTestHelper.LoggerStatements.IndexOf("TestInterceptor Started"));
			Assert.NotNull(RequestorTestHelper.LoggerStatements.FirstOrDefault(d => d == $"StopwatchInterceptor started at {boltOnClock.Now}"));
			Assert.NotNull(RequestorTestHelper.LoggerStatements.FirstOrDefault(d => d == $"StopwatchInterceptor ended at {boltOnClock.Now}. " +
																				   "Time elapsed: 0"));
			Assert.True(RequestorTestHelper.LoggerStatements.IndexOf($"StopwatchInterceptor ended at {boltOnClock.Now}. Time elapsed: 0") >
						RequestorTestHelper.LoggerStatements.IndexOf("TestInterceptor Ended"));
		}

		[Fact]
		public async Task Process_BootstrapWithTestInterceptorsAndAsyncHandler_InvokesDefaultAndTestInterceptorInOrderAndReturnsSuccessfulResult()
		{
			// arrange
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddLogging();
            serviceCollection.BoltOn(b => b.RegisterRequestorFakes());
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var boltOnClock = serviceProvider.GetService<IBoltOnClock>();
			var requestor = serviceProvider.GetService<IRequestor>();

			// act
			var result = await requestor.ProcessAsync(new TestRequest());

			// assert 
			Assert.True(result);
			Assert.True(RequestorTestHelper.LoggerStatements.IndexOf("TestInterceptor Started") > 0);
			Assert.True(RequestorTestHelper.LoggerStatements.IndexOf("TestInterceptor Ended") > 0);
			Assert.True(RequestorTestHelper.LoggerStatements.IndexOf("TestRequestSpecificInterceptor Started") == -1);
			Assert.True(RequestorTestHelper.LoggerStatements.IndexOf($"StopwatchInterceptor started at {boltOnClock.Now}") <
						RequestorTestHelper.LoggerStatements.IndexOf("TestInterceptor Started"));
			Assert.NotNull(RequestorTestHelper.LoggerStatements.FirstOrDefault(d => d == $"StopwatchInterceptor started at {boltOnClock.Now}"));
			Assert.NotNull(RequestorTestHelper.LoggerStatements.FirstOrDefault(d => d == $"StopwatchInterceptor ended at {boltOnClock.Now}. " +
																				   "Time elapsed: 0"));
			Assert.True(RequestorTestHelper.LoggerStatements.IndexOf($"StopwatchInterceptor ended at {boltOnClock.Now}. Time elapsed: 0") >
						RequestorTestHelper.LoggerStatements.IndexOf("TestInterceptor Ended"));
		}

		[Fact]
		public async Task Process_BootstrapWithTestInterceptorsAndRemoveAll_InvokesOnlyTestInterceptorAndReturnsSuccessfulResult()
		{
			// arrange
			RequestorTestHelper.IsClearInterceptors = true;
			var serviceCollection = new ServiceCollection();
            serviceCollection.BoltOn(b => b.RegisterRequestorFakes());
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var boltOnClock = serviceProvider.GetService<IBoltOnClock>();
			var sut = serviceProvider.GetService<IRequestor>();

			// act
			var result = await sut.ProcessAsync(new TestRequest());

			// assert 
			Assert.True(result);
			Assert.NotNull(RequestorTestHelper.LoggerStatements.FirstOrDefault(f => f == "TestInterceptor Started"));
			Assert.NotNull(RequestorTestHelper.LoggerStatements.FirstOrDefault(f => f == "TestInterceptor Ended"));
			Assert.Null(RequestorTestHelper.LoggerStatements.FirstOrDefault(d => d == $"StopwatchInterceptor started at {boltOnClock.Now}"));
			Assert.Null(RequestorTestHelper.LoggerStatements.FirstOrDefault(d => d == $"StopwatchInterceptor ended at {boltOnClock.Now}. " +
																				"Time elapsed: 0"));
		}

		[Fact]
		public async Task Process_BootstrapWithRemoveInterceptor_DoesNotInvokeRemovedInterceptorAndReturnsSuccessfulResult()
		{
			// arrange
			RequestorTestHelper.IsRemoveStopwatchInterceptor = true;
			var serviceCollection = new ServiceCollection();
            serviceCollection.BoltOn(b => b.RegisterRequestorFakes());
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var boltOnClock = serviceProvider.GetService<IBoltOnClock>();
			var sut = serviceProvider.GetService<IRequestor>();

			// act
			var result = await sut.ProcessAsync(new TestRequest());

			// assert 
			Assert.True(result);
			Assert.NotNull(RequestorTestHelper.LoggerStatements.FirstOrDefault(f => f == "TestInterceptor Started"));
			Assert.NotNull(RequestorTestHelper.LoggerStatements.FirstOrDefault(f => f == "TestInterceptor Ended"));
			Assert.Null(RequestorTestHelper.LoggerStatements.FirstOrDefault(d => d == $"StopwatchInterceptor started at {boltOnClock.Now}"));
			Assert.Null(RequestorTestHelper.LoggerStatements.FirstOrDefault(d => d == $"StopwatchInterceptor ended at {boltOnClock.Now}. " +
																				"Time elapsed: 0"));
		}

		[Fact]
		public async Task Process_RequestorWithQueryRequest_StartsTransactionsWithDefaultQueryIsolationLevel()
		{
			// arrange
			RequestorTestHelper.IsCustomizeIsolationLevel = false;
			var serviceCollection = new ServiceCollection();
            serviceCollection.BoltOn(b => b.RegisterRequestorFakes());
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var sut = serviceProvider.GetService<IRequestor>();

			// act
			var result = await sut.ProcessAsync(new TestQuery());

			// assert 
			Assert.True(result);
			Assert.NotNull(RequestorTestHelper.LoggerStatements.FirstOrDefault(f => f == "Getting isolation level for Query"));
		}

		[Fact]
		public async Task Process_RequestorWithQueryUncommittedRequest_ExecutesCustomChangeTrackerInterceptor()
		{
			// arrange
			RequestorTestHelper.IsCustomizeIsolationLevel = false;
			var serviceCollection = new ServiceCollection();
			serviceCollection.BoltOn(options =>
            {
				options.RegisterRequestorFakes();
                options.BoltOnEFModule();
            });
			serviceCollection.AddSingleton<IUnitOfWorkOptionsBuilder, TestCustomUnitOfWorkOptionsBuilder>();
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var sut = serviceProvider.GetService<IRequestor>();

			// act
			var result = await sut.ProcessAsync(new TestStaleQuery());

			// assert 
			Assert.True(result);
			Assert.NotNull(RequestorTestHelper.LoggerStatements.FirstOrDefault(f => f == "Getting isolation level for QueryUncommitted"));
		}

		[Fact]
		public async Task Process_RequestorWithQueryRequest_StartsTransactionsWithCustomizedQueryIsolationLevel()
		{
			// arrange
			RequestorTestHelper.IsCustomizeIsolationLevel = true;
			var serviceCollection = new ServiceCollection();
            serviceCollection.BoltOn(options =>
            {
                options.RegisterRequestorFakes();
                options.BoltOnEFModule();
            });
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var sut = serviceProvider.GetService<IRequestor>();

			// act
			var result = await sut.ProcessAsync(new TestQuery());

			// assert 
			Assert.True(result);
			Assert.NotNull(RequestorTestHelper.LoggerStatements.FirstOrDefault(f => f == $"Entering {nameof(CustomChangeTrackerInterceptor)}..."));
			Assert.NotNull(RequestorTestHelper.LoggerStatements.FirstOrDefault(f => f == "Getting isolation level for Command or Query"));
		}

		[Fact]
		public async Task Process_RequestorWithQueryRequestAndAsyncHandler_StartsTransactionsWithCustomizedQueryIsolationLevel()
		{
			// arrange
			RequestorTestHelper.IsCustomizeIsolationLevel = true;
			RequestorTestHelper.IsRemoveStopwatchInterceptor = false;
			var serviceCollection = new ServiceCollection();
            serviceCollection.BoltOn(options =>
            {
                options.RegisterRequestorFakes();
                options.BoltOnEFModule();
            });
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var sut = serviceProvider.GetService<IRequestor>();

			// act
			var result = await sut.ProcessAsync(new TestQuery());

			// assert 
			Assert.True(result); 
			Assert.NotNull(RequestorTestHelper.LoggerStatements.FirstOrDefault(f => f == $"Entering {nameof(CustomChangeTrackerInterceptor)}..."));
			Assert.NotNull(RequestorTestHelper.LoggerStatements.FirstOrDefault(f => f == "Getting isolation level for Command or Query"));
		}

		[Fact]
		public async Task Process_RequestorWithOneWayCommandRequestAndAsyncHandler_StartsTransactionsWithCustomizedQueryIsolationLevel()
		{
			// arrange
			RequestorTestHelper.IsCustomizeIsolationLevel = true;
			RequestorTestHelper.IsRemoveStopwatchInterceptor = false;
			var serviceCollection = new ServiceCollection();
            serviceCollection.BoltOn(options =>
			{
				options.BoltOnEFModule();
				options.RegisterRequestorFakes();
            });
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var sut = serviceProvider.GetService<IRequestor>();
			var command = new TestOneWayCommand();

			// act
			await sut.ProcessAsync(command);

			// assert 
			Assert.Equal(1, command.Value); 
			Assert.NotNull(RequestorTestHelper.LoggerStatements.FirstOrDefault(f => f == $"Entering {nameof(CustomChangeTrackerInterceptor)}..."));
			Assert.NotNull(RequestorTestHelper.LoggerStatements.FirstOrDefault(f => f == "Getting isolation level for Command or Query"));
		}


		[Fact]
		public async Task Process_RequestorWithQueryRequest_ExecutesChangeTrackerContextInterceptorAndDisablesTracking()
		{
			// arrange
			IntegrationTestHelper.IsSeedData = true;
			RequestorTestHelper.IsCustomizeIsolationLevel = true;
			var serviceCollection = new ServiceCollection();
            serviceCollection.BoltOn(options =>
            {
                options.RegisterRequestorFakes();
				options.RegisterDataFakes();
                options.BoltOnEFModule();
            });
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var sut = serviceProvider.GetService<IRequestor>();

			// act
			var result = await sut.ProcessAsync(new GetStudentRequest { StudentId = 2 });
			var dbContext = serviceProvider.GetService<IDbContextFactory>().Get<SchoolDbContext>();
			var isAutoDetectChangesEnabled = dbContext.ChangeTracker.AutoDetectChangesEnabled;
			var queryTrackingBehavior = dbContext.ChangeTracker.QueryTrackingBehavior;

			// assert 
			Assert.NotNull(result);
			Assert.Equal(Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking, queryTrackingBehavior);
			Assert.NotNull(RequestorTestHelper.LoggerStatements.FirstOrDefault(f => f == $"Entering {nameof(ChangeTrackerInterceptor)}..."));
			Assert.NotNull(RequestorTestHelper.LoggerStatements.FirstOrDefault(f => f == $"IsQueryRequest: {true}"));
			Assert.False(isAutoDetectChangesEnabled);
		}

		[Fact]
		public async Task Process_RequestorWithCommandRequest_ExecutesChangeTrackerContextInterceptorAndEnablesTrackAll()
		{
			// arrange
			IntegrationTestHelper.IsSeedData = false;
			var serviceCollection = new ServiceCollection();
            serviceCollection.BoltOn(options =>
            {
                options.RegisterRequestorFakes();
				options.RegisterDataFakes();
                options.BoltOnEFModule();
            });
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var sut = serviceProvider.GetService<IRequestor>();

			// act
			var result = await sut.ProcessAsync(new TestCommand());
			var dbContext = serviceProvider.GetService<IDbContextFactory>().Get<SchoolDbContext>();
			var isAutoDetectChangesEnabled = dbContext.ChangeTracker.AutoDetectChangesEnabled;
			var queryTrackingBehavior = dbContext.ChangeTracker.QueryTrackingBehavior;

			// assert 
			Assert.True(result);
			Assert.Equal(Microsoft.EntityFrameworkCore.QueryTrackingBehavior.TrackAll, queryTrackingBehavior);
			Assert.NotNull(RequestorTestHelper.LoggerStatements.FirstOrDefault(f => f == $"Entering {nameof(ChangeTrackerInterceptor)}..."));
			Assert.NotNull(RequestorTestHelper.LoggerStatements.FirstOrDefault(f => f == $"IsQueryRequest: {false}"));
			Assert.True(isAutoDetectChangesEnabled);
		}

		[Fact]
		public async Task Process_HandlerRegistrationsDisabled_ThrowsException()
		{
			// arrange
			IntegrationTestHelper.IsSeedData = false;
			var serviceCollection = new ServiceCollection();
			serviceCollection.BoltOn(options =>
			{
				options.DisableRequestorHandlersRegistration = true;
			});
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var sut = serviceProvider.GetService<IRequestor>();

			// act
			var result = await Record.ExceptionAsync(() => sut.ProcessAsync(new TestCommand()));

			// assert 
			Assert.NotNull(result);
			Assert.Equal("Handler not found for request: BoltOn.Tests.Requestor.Fakes.TestCommand", result.Message);
		}

		[Fact]
		public async Task Process_HandlerRegistrationsDisabledAndExplicitRegistration_ReturnsExpectedResult()
		{
			// arrange
			IntegrationTestHelper.IsSeedData = false;
			var serviceCollection = new ServiceCollection();
			serviceCollection.BoltOn(options =>
			{
				options.DisableRequestorHandlersRegistration = true;
			});
			serviceCollection.AddTransient<IHandler<TestCommand, bool>, TestHandler>();
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var sut = serviceProvider.GetService<IRequestor>();

			// act
			var result = await sut.ProcessAsync(new TestCommand());

			// assert 
			Assert.True(result);
		}

		public void Dispose()
		{
			RequestorTestHelper.IsRemoveStopwatchInterceptor = false;
			RequestorTestHelper.IsClearInterceptors = false;
			RequestorTestHelper.IsCustomizeIsolationLevel = false;
			RequestorTestHelper.LoggerStatements.Clear();
			IntegrationTestHelper.IsSeedData = false;
		}
	}
}
